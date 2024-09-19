using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using ServiceB.Consumers;
using ServiceB.Models.Entities;
using ServiceB.Services;
using Shared;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddMassTransit(configurator =>
{
    configurator.AddConsumer<UpdatePersonNameEventConsumer>();

    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);

        _configurator.ReceiveEndpoint(RabbitMQSettings.ServiceB_UpdatePersonNameEventQueue, e => e.ConfigureConsumer<UpdatePersonNameEventConsumer>(context));
    });
});

#region MongoDB Seed Data
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

app.Run();
