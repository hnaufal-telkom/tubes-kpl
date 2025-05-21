using CoreLibrary;
using Microsoft.AspNetCore.Mvc;
using CoreLibrary.InterfaceLib;
using CoreLibrary.Repository;
using CoreLibrary.Service;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<ILeaveRequestRepository, InMemoryLeaveRequestRepository>();

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<LeaveService>();

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<Serilog.ILogger>(Log.Logger);
builder.Services.AddSingleton<UserService>();
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<IBusinessTripRepository, InMemoryBusinessTripRepository>();
builder.Services.AddSingleton<BusinessTripService>();
builder.Services.AddSingleton<ILeaveRequestRepository, InMemoryLeaveRequestRepository>();
builder.Services.AddSingleton<IPayrollRepository, InMemoryPayrollRepository>();
builder.Services.AddSingleton<PayrollService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
