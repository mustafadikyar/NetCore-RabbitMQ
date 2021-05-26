using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System;

namespace WatermarkApp.Services
{
    public class RabbitMQClientService : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection;
        private IModel _channel;
        public static string ExchangeName = "ImageDirectExchange";
        public static string RoutingWatermark = "watermark-route-image";
        public static string QueueName = "queue-watermark-image";

        private readonly ILogger<RabbitMQClientService> _logger;

        public RabbitMQClientService(ConnectionFactory connectionFactory, ILogger<RabbitMQClientService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;

        }

        public IModel Connect()
        {
            _connection = _connectionFactory.CreateConnection(); //bağlantı oluşturuldu.

            if (_channel is { IsOpen: true }) //kanal açık mı?
                return _channel;

            _channel = _connection.CreateModel(); //kanal oluşturuldu.
            _channel.ExchangeDeclare(exchange: ExchangeName, type: "direct", durable: true, autoDelete: false); //exchange bildirildi.
            _channel.QueueDeclare(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null); //kuyruk bildirildi. 
            _channel.QueueBind(exchange: ExchangeName, queue: QueueName, routingKey: RoutingWatermark); //exchange ve kuyruğu bağlandı..
            _logger.LogInformation(message: "RabbitMQ ile bağlantı kuruldu."); //log
            return _channel;
        }

        public void Dispose()
        {
            _channel?.Close();
            _channel?.Dispose();

            _connection?.Close();
            _connection?.Dispose();

            _logger.LogInformation(message: "RabbitMQ ile bağlantı kapatıldı.");

        }
    }
}