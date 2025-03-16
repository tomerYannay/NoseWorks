using Amazon.S3;
using Amazon.S3.Transfer;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MyFirstMvcApp.Data;

public class SqsBackgroundService : BackgroundService
{
    private readonly IAmazonSQS _sqsClient;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<SqsBackgroundService> _logger;
    private readonly string _queueUrl;
    private readonly IServiceProvider _serviceProvider;

    public SqsBackgroundService(IAmazonSQS sqsClient, IAmazonS3 s3Client, string queueUrl, IServiceProvider serviceProvider, ILogger<SqsBackgroundService> logger)
    {
        _sqsClient = sqsClient;
        _s3Client = s3Client;
        _queueUrl = queueUrl;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var receiveMessageRequest = new ReceiveMessageRequest
            {
                QueueUrl = _queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 20
            };

            var response = await _sqsClient.ReceiveMessageAsync(receiveMessageRequest, stoppingToken);

            foreach (var message in response.Messages)
            {
                try
                {
                    var messageBody = JsonConvert.DeserializeObject<dynamic>(message.Body);
                    var trialId = (int)messageBody.TrialId;
                    var bucketName = (string)messageBody.BucketName;
                    var keyName = (string)messageBody.KeyName;
                    var fileName = (string)messageBody.FileName;
                    var contentType = (string)messageBody.ContentType;
                    var fileContentBase64 = (string)messageBody.FileContent;

                    // Check if fileContentBase64 is null
                    if (string.IsNullOrEmpty(fileContentBase64))
                    {
                        _logger.LogError("File content is missing in the message.");
                        continue;
                    }

                    // Decode the file content from Base64
                    var fileContent = Convert.FromBase64String(fileContentBase64);

                    // Process the video upload
                    await ProcessVideoUpload(trialId, bucketName, keyName, fileContent, contentType);

                    // Delete the message from the queue
                    var deleteMessageRequest = new DeleteMessageRequest
                    {
                        QueueUrl = _queueUrl,
                        ReceiptHandle = message.ReceiptHandle
                    };
                    await _sqsClient.DeleteMessageAsync(deleteMessageRequest, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message from SQS queue.");
                }
            }
        }
    }

    private async Task ProcessVideoUpload(int trialId, string bucketName, string keyName, byte[] fileContent, string contentType)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Retrieve the trial from the database
            var trial = await context.Trials.FindAsync(trialId);
            if (trial == null)
            {
                _logger.LogError($"Trial with ID {trialId} not found.");
                return;
            }

            // Upload the video to S3
            using (var memoryStream = new MemoryStream(fileContent))
            {
                var fileTransferUtility = new TransferUtility(_s3Client);
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = memoryStream,
                    Key = keyName,
                    BucketName = bucketName,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.PublicRead
                };
                uploadRequest.PartSize = 10 * 1024 * 1024; 
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // Update the trial with the video URL
            trial.VideoUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";
            context.Trials.Update(trial);
            await context.SaveChangesAsync();
        }
    }
}