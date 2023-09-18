using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

using var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints = { "redis_image:6379" }, //localhost:6379
    AbortOnConnectFail = false
});
var db = redis.GetDatabase();

var app = builder.Build();

app.MapGet("/", () => "Welcome to my docker compose AspNetCore_Redis example");

app.MapGet("/fetch/{key}", (string key) =>
{
    if (string.IsNullOrEmpty(key))
        return Results.BadRequest("Invalid Key");

    var keyValue = db.StringGet(key);
    if (keyValue.HasValue)
    {
        return Results.Ok($"{key} = {keyValue.ToString()}");
    }
    return Results.Ok($"{key} Not Found");
});

app.MapPost("/submit", ([FromBody] RedisDTO data) => 
{
    if (data == null)
        return Results.BadRequest("Invalid data");
    if (string.IsNullOrEmpty(data.key) || string.IsNullOrEmpty(data.value))
        return Results.BadRequest("Key or Value missing");
    db.StringSet(data.key, data.value);
    return Results.Ok("Redis updated");
});

app.Run();

internal class RedisDTO {
    public string key { get; set; }
    public string value { get; set; }
}
