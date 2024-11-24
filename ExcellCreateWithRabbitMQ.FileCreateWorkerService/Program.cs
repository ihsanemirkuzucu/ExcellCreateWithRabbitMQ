using ExcellCreateWithRabbitMQ.FileCreateWorkerService;
using ExcellCreateWithRabbitMQ.FileCreateWorkerService.Models;
using ExcellCreateWithRabbitMQ.FileCreateWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context,services) =>
    {
        IConfiguration config = context.Configuration;
        services.AddDbContext<AdventureWorks2019Context>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("SqlServer"));
        });
        services.AddSingleton<RabbitMQClientService>();
        services.AddSingleton(sp => new ConnectionFactory()
        {
            Uri = new Uri(context.Configuration.GetConnectionString("RabbitMQ")),
            DispatchConsumersAsync = true
        });
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();
