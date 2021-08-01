using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MinimalAPI.DataAccess;
using MinimalAPI.DataAccess.Models;
using MinimalAPI.WebApi.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .SetBasePath(Environment.CurrentDirectory)
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.Configure<DatabaseConnectionManager>(builder.Configuration.GetSection("DatabaseConnectionManager"));
builder.Services.AddDbContext<Context>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

await using var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<Context>();
    db.Database.Migrate();
}

app.UseSwagger();
app.MapGet("/api/customers", async (HttpContext httpContext, Context db) =>
{
    var customers = await db.Customers.ToListAsync();
    httpContext.Response.StatusCode = 200;
    await httpContext.Response.WriteAsJsonAsync(customers);
});

app.MapGet("/api/customer/{customerId}", async (HttpContext httpContext, Context db, Guid customerId) =>
{
    var customer = await db.Customers.FirstOrDefaultAsync(x => x.Id == customerId);

    httpContext.Response.StatusCode = 200;
    await httpContext.Response.WriteAsJsonAsync(customer);
});

app.MapPost("/api/customer", async (HttpContext httpContext, Context db, ApiCustomer apiCustomer) =>
{
    await db.Customers.AddAsync(new Customer
        {Id = Guid.NewGuid(), Firstname = apiCustomer.Firstname, Lastname = apiCustomer.Lastname});

    await db.SaveChangesAsync();
    
    httpContext.Response.StatusCode = 201;
});

app.MapPut("/api/customer/{customerId}", async (HttpContext httpContext, Context db, Guid customerId, ApiCustomer apiCustomer) =>
{
    var customer = await db.Customers.AsTracking().FirstOrDefaultAsync(x => x.Id == customerId);
    if (customer != null)
    {
        customer.Firstname = apiCustomer.Firstname;
        customer.Lastname = apiCustomer.Lastname;

        db.Customers.Update(customer);
        await db.SaveChangesAsync();
    }

    httpContext.Response.StatusCode = 200;
});

app.MapDelete("/api/customer/{customerId}", async (HttpContext httpContext, Context db, Guid customerId) =>
{
    var customer = await db.Customers.AsTracking().FirstOrDefaultAsync(x => x.Id == customerId);
    if (customer != null)
    {
        db.Customers.Remove(customer);
        await db.SaveChangesAsync();
    }

    httpContext.Response.StatusCode = 200;
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MinimalAPI V1");
});

await app.RunAsync();
