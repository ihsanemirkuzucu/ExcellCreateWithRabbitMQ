using ClosedXML.Excel;
using ExcellCreateWithRabbitMQ.FileCreateWorkerService.Models;
using ExcellCreateWithRabbitMQ.FileCreateWorkerService.Services;
using ExcellCreateWithRabbitMQ.Shared;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data;
using System.Text;
using System.Text.Json;

namespace ExcellCreateWithRabbitMQ.FileCreateWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly RabbitMQClientService _rabbitMqClientService;
        private IModel _channel;

        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, RabbitMQClientService rabbitMqClientService)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _rabbitMqClientService = rabbitMqClientService;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMqClientService.Connect();
            _channel.BasicQos(0, 1, false);

            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExecuteAsync çalýþýyor");
            var consumer = new AsyncEventingBasicConsumer(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueName, false, consumer);
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            _logger.LogInformation("Consumer_Received çalýþýyor");
            await Task.Delay(5000);

            var createExcellMessage = JsonSerializer.Deserialize<CreateExcellMessage>(Encoding.UTF8.GetString(@event.Body.ToArray()));

            using var memoryStream = new MemoryStream();
            var workBook = new XLWorkbook();
            var dataSet = new DataSet();

            dataSet.Tables.Add(GetTable("products"));
            workBook.Worksheets.Add(dataSet);
            workBook.SaveAs(memoryStream);

            MultipartFormDataContent multipartFormDataContent = new();
            multipartFormDataContent.Add(new ByteArrayContent(memoryStream.ToArray()), "file",
                Guid.NewGuid() + ".xlsx");
            var baseUrl = "https://localhost:7043/api/File";
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.PostAsync($"{baseUrl}?fileId={createExcellMessage.FileId}",
                    multipartFormDataContent);
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"File(Id : {createExcellMessage.FileId}) was created successfully.");
                    _channel.BasicAck(@event.DeliveryTag, false);
                }
                else
                {
                    _logger.LogInformation("Kral burda hata var amk.");
                }
            }
        }

        private DataTable GetTable(string tableName)
        {
            List<Product> products;
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AdventureWorks2019Context>();
                products = context.Products.ToList();
            }

            DataTable table = new DataTable { TableName = tableName };
            table.Columns.Add("ProductId", typeof(int));
            table.Columns.Add("Name", typeof(String));
            table.Columns.Add("ProductNumber", typeof(string));
            table.Columns.Add("Color", typeof(string));

            products.ForEach(x =>
            {
                table.Rows.Add(x.ProductId, x.Name, x.ProductNumber, x.Color);

            });

            return table;

        }
    }
}
