using MainLibrary;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IUserRepository, InMemoryUserRepository>();
builder.Services.AddSingleton<ILeaveRequestRepository, InMemoryLeaveRequestRepository>();
builder.Services.AddSingleton<IBusinessTripRepository, InMemoryBusinessTripRepository>();
builder.Services.AddSingleton<IPayrollRepository, InMemoryPayrollRepository>();

builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddSingleton<LeaveService>();
builder.Services.AddSingleton<BusinessTripService>();
builder.Services.AddSingleton<PayrollService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
