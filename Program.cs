using Microsoft.EntityFrameworkCore;
using PathwiseAPI.Data;
using PathwiseAPI.Services; // ⬅️ Add this if you place EmailService in Services folder

var builder = WebApplication.CreateBuilder(args);

// Register configuration section for EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register custom EmailService
builder.Services.AddScoped<EmailService>(); // ⬅️ Add this line

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:3001", "http://localhost:3002", "http://localhost:3003")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=pathwise.db"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthorization();
app.MapControllers();
app.Run();
