using System.Text;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using PostService.Data;
using PostService.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<PostServiceContext>(options =>
        options.UseSqlite(@"Data Source=post.db"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    using(var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
        dbContext.Database.EnsureCreated();
    }
    app.UseSwagger();
    app.UseSwaggerUI();
}

// RabbitMQ Setup
// 100% should not be done this way
var factory = new ConnectionFactory();
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (model, ea) =>
{
    using(var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
            
    
        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}", message);

        var data = JObject.Parse(message);
        var type = ea.RoutingKey;
        if (type == "user.add")
        {
            dbContext.User.Add(new User()
            {
                ID = data["id"].Value<int>(),
                Name = data["name"].Value<string>()
            });
            dbContext.SaveChangesAsync();
        }
        else if (type == "user.update")
        {
            var user = dbContext.User.First(a => a.ID == data["id"].Value<int>());
            user.Name = data["newname"].Value<string>();
            dbContext.SaveChangesAsync();
        }
    };
};

channel.BasicConsume(queue: "user.postservice",
                            autoAck: true,
                            consumer: consumer);

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();