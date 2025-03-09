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

    public SqsBackgroundService(IAmazonSQS sqsClient, IAmazonS3 s3Client, ILogger<SqsBackgroundService> logger, string queueUrl, IServiceProvider serviceProvider)
    {
        _sqsClient = sqsClient;
        _s3Client = s3Client;
        _logger = logger;
        _queueUrl = queueUrl;
        _serviceProvider = serviceProvider;
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

                    // Process the video upload
                    await ProcessVideoUpload(trialId, bucketName, keyName, fileName);

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

    private async Task ProcessVideoUpload(int trialId, string bucketName, string keyName, string fileName)
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

            // Define the actual path where the uploaded files are stored
            var uploadDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Uploads");
            var filePath = Path.Combine(uploadDirectory, fileName);

            // Ensure the upload directory exists
            if (!Directory.Exists(uploadDirectory))
            {
                Directory.CreateDirectory(uploadDirectory);
            }

            // Upload the video to S3
            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var uploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = fileStream,
                    Key = keyName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.PublicRead
                };

                var fileTransferUtility = new TransferUtility(_s3Client);
                await fileTransferUtility.UploadAsync(uploadRequest);
            }

            // Update the trial with the video URL
            trial.VideoUrl = $"https://{bucketName}.s3.amazonaws.com/{keyName}";
            context.Trials.Update(trial);
            await context.SaveChangesAsync();
        }
    }
}