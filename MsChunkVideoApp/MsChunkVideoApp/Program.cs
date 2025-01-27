using Application.UseCases;
using Domain.Repositories;
using Infra;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services
builder.Services.AddTransient<IProcessVideoUseCase, ProcessVideoUseCase>();

// Register infrastructure services
builder.Services.AddSingleton<IStoragePort, StorageAdapter>();
builder.Services.AddSingleton<IVideoPort, VideoAdapter>();
builder.Services.AddSingleton<IQueuePort, QueueAdapter>();

// Add RabbitMQ configuration (replace with your actual configuration)
//builder.Services.Configure<RabbitMQOptions>(builder.Configuration.GetSection("RabbitMQ"));

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
