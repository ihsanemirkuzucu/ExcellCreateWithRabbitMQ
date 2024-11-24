using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using ExcellCreateWithRabbitMQ.Shared;

namespace ExcellCreateWithRabbitMQ.Web.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMqClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMqClientService)
        {
            _rabbitMqClientService = rabbitMqClientService;
        }

        public void Publish(CreateExcellMessage createdMessage)
        {
            var channel = _rabbitMqClientService.Connect();
            var bodyString = JsonSerializer.Serialize(createdMessage);
            var bodyByte = Encoding.UTF8.GetBytes(bodyString);
            var property = channel.CreateBasicProperties();
            property.Persistent = true;
            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingExcell, basicProperties: property, body: bodyByte);
        }
    }
}
