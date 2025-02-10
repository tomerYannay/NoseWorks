using Microsoft.OpenApi.Models;
using MyFirstMvcApp.Data;
using MyFirstMvcApp.Models;
using MyFirstMvcApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NoseWorks", Version = "v1" });
});

// Explicitly retrieve environment variables and build the connection string
string postgresUser = Environment.GetEnvironmentVariable("POSTGRES_USER");
string postgresPassword = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");
string postgresHost = "localhost"; // Adjust if needed
string postgresDatabase = "NoseWorks"; // Adjust to match your DB name

// Build connection string manually
string connectionString = $"Host={postgresHost};Database={postgresDatabase};Username={postgresUser};Password={postgresPassword}";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
        .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddLogging(options =>
{
    options.AddConsole();
    options.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NoseWorks v1"));
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Ensure this line is present to serve static files

app.UseAuthorization();

app.MapControllers();

app.Run();