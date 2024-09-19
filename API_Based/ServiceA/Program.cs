using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using ServiceA.Model.Entities;
using ServiceA.Services;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddHttpClient("ServiceB", httpclient =>
{
    httpclient.BaseAddress = new Uri("https://localhost:7294/");
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

app.MapGet("/{id}/{newName}", async (
    [FromRoute] string id,
    [FromRoute] string newName,
    MongoDbService mongoDBService,
    IHttpClientFactory httpClientFactory) =>
{
    var httpClient = httpClientFactory.CreateClient("ServiceB");

    var persons = mongoDBService.GetCollection<Person>();

    Person person = await (await persons.FindAsync(p => p.Id == ObjectId.Parse(id))).FirstOrDefaultAsync();
    person.Name = newName;
    await persons.FindOneAndReplaceAsync(p => p.Id == ObjectId.Parse(id), person);

    var httpResponseMessage = await httpClient.GetAsync($"update/{person.Id}/{person.Name}");
    if (httpResponseMessage.IsSuccessStatusCode)
    {
        var content = await httpResponseMessage.Content.ReadAsStringAsync();
        await Console.Out.WriteLineAsync(content);
    }
});




app.Run();
