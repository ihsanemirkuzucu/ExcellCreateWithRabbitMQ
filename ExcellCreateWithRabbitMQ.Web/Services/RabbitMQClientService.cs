using RabbitMQ.Client;

namespace ExcellCreateWithRabbitMQ.Web.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQClientService> _logger;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ExcellDirectExchange";
        public static string RoutingExcell = "excell-route-file";
        public static string QueName = "queue-excell-file";


        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
            Connect();
        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection();
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(ExchangeName, type: "direct", true, false);
            _channel.QueueDeclare(QueName, true, false, false, null);
            _channel.QueueBind(exchange: ExchangeName, queue: QueName, routingKey: RoutingExcell);
            _logger.LogInformation("RabbitMQ ile bağlantı kuruldu...");
            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation("RabbitMQ ile Bağlantı Koptu...");
        }
    }
}
