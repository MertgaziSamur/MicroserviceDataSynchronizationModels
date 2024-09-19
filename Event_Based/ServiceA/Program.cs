﻿using MassTransit;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using ServiceA.Models.Entities;
using ServiceA.Services;
using Shared.Events;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configurator) =>
    {
        _configurator.Host(builder.Configuration["RabbitMQ"]);
    });
});


#region MongoDB Seed Data
using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
MongoDbService mongoDBService = scope.ServiceProvider.GetService<MongoDbService>();
var collection = mongoDBService.GetCollection<Person>();
if (!collection.FindSync(s => true).Any())
{
    await collection.InsertOneAsync(new() { Name = "Mert" });
    await collection.InsertOneAsync(new() { Name = "Hilmi" });
    await collection.InsertOneAsync(new() { Name = "Şuayip" });
    await collection.InsertOneAsync(new() { Name = "Rıfkı" });
    await collection.InsertOneAsync(new() { Name = "Rukiye" });
    await collection.InsertOneAsync(new() { Name = "Muiddin" });
}
#endregion


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("updateName/{id}/{newName}", async (
    [FromRoute] string id,
    [FromRoute] string newName,
    MongoDbService mongoDBService,
   IPublishEndpoint publishEndpoint) =>
{
    var persons = mongoDBService.GetCollection<Person>();

    Person person = await (await persons.FindAsync(p => p.Id == ObjectId.Parse(id))).FirstOrDefaultAsync();
    person.Name = newName;
    await persons.FindOneAndReplaceAsync(p => p.Id == ObjectId.Parse(id), person);

    UpdatedPersonNameEvent updatedPersonNameEvent = new()
    {
        PersonId = id,
        NewName = newName
    };

    await publishEndpoint.Publish(updatedPersonNameEvent);
});

app.Run();
