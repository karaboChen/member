using member.Extensions;
using member.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();


builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder
             .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();

    });
});

builder.Services.AddDbContextPool<MyDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TW"));
});
builder.Services.AddProblemDetails();

builder.Services.AddBusinessServices();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapOpenApi();
app.MapScalarApiReference();


app.UseExceptionHandler();
app.UseStatusCodePages(); // 處理 404 等非例外錯誤
app.UseCors();
app.UseAuthorization();

app.MapControllers();

app.Run();
