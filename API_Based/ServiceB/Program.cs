using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServiceB.Models.Entities;
using ServiceB.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();

#region MongoDb Seed Data
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDBService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDBService.GetCollection<Employee>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d04", Name = "Mert", Department = "Yazılım" });
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d05", Name = "Hilmi", Department = "Ağır Vasıta" });
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d06", Name = "Şuayip", Department = "Oluk&Çatı" });
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d07", Name = "Rıfkı", Department = "Muhabbet Sohbet" });
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d08", Name = "Rukiye", Department = "Şoför" });
    await collection.InsertOneAsync(new() { PersonId = "66ec1c1bf80628d2a7459d09", Name = "Muiddin", Department = "Muhasebe" });
}
#endregion

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("update/{personId}/{newName}", async (
    [FromRoute] string personId,
    [FromRoute] string newName,
    MongoDbService mongoDBService) =>
{
    var employees = mongoDBService.GetCollection<Employee>();
    Employee employee = await (await employees.FindAsync(e => e.PersonId == personId)).FirstOrDefaultAsync();
    employee.Name = newName;
    await employees.FindOneAndReplaceAsync(p => p.Id == employee.Id, employee);
    return true;
});

app.Run();
