using DAL;
using Model;
using Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Backend.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
          .WithOrigins("http://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials();
    });
});

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();
builder.Services.AddDbContext<QuestionContext>();

builder.Services.AddScoped<QuestionService>();
builder.Services.AddScoped<AccessService>();
builder.Services.AddSingleton<EvaluationService>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseHttpsRedirection();
app.UseCors();

app.UseAuthorization();

app.MapControllers();
app.MapHub<ExamHub>("/hubs/exam");

app.Run();
