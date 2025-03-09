using Microsoft.OpenApi.Models;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using MyFirstMvcApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MyFirstMvcApp.Profiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Amazon.Extensions.NETCore.Setup;
using Amazon.S3;
using Amazon.SQS;
using DotNetEnv;
using Microsoft.Extensions.Hosting;

// Load environment variables from .env file
Env.Load();

// Explicitly retrieve environment variables and build the connection string
string postgresUser = "tomer_yannay";
string postgresPassword = "Apple2020";
string postgresHost = "localhost"; // Adjust if needed
string postgresDatabase = "NoseWorks"; // Adjust to match your DB name

// Build connection string manually
string connectionString = $"Host={postgresHost};Database={postgresDatabase};Username={postgresUser};Password={postgresPassword}";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NoseWorks", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddAutoMapper(typeof(UserProfile).Assembly);

builder.Services.AddLogging(options =>
{
    options.AddConsole();
    options.AddDebug();
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add AWS S3 and SQS services using environment variables
builder.Services.AddDefaultAWSOptions(new AWSOptions
{
    Credentials = new Amazon.Runtime.BasicAWSCredentials(
        Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
        Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")),
    Region = Amazon.RegionEndpoint.GetBySystemName(Environment.GetEnvironmentVariable("AWS_REGION"))
});
builder.Services.AddAWSService<IAmazonS3>();
builder.Services.AddAWSService<IAmazonSQS>();

// Register the background service
builder.Services.AddHostedService<SqsBackgroundService>(provider =>
{
    var sqsClient = provider.GetRequiredService<IAmazonSQS>();
    var s3Client = provider.GetRequiredService<IAmazonS3>();
    var logger = provider.GetRequiredService<ILogger<SqsBackgroundService>>();
    var serviceProvider = provider.GetRequiredService<IServiceProvider>();
    var queueUrl = "https://sqs.eu-central-1.amazonaws.com/931894660086/noseWorks"; // Keep the queue URL
    return new SqsBackgroundService(sqsClient, s3Client, logger, queueUrl, serviceProvider);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "NoseWorks v1");
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure this line is present to serve static files
app.UseAuthentication();
app.UseAuthorization();
app.UseCors(); // Enable CORS
app.MapControllers();

app.Run();