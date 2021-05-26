using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace WatermarkApp.Services
{
    public class RabbitMQPublisher
    {
        private readonly RabbitMQClientService _rabbitMQClientService;

        public RabbitMQPublisher(RabbitMQClientService rabbitMQClientService)
        {
            _rabbitMQClientService = rabbitMQClientService;
        }

        public void Publish(productImageCreatedEvent productImageCreatedEvent)
        {
            IModel channel = _rabbitMQClientService.Connect();  //kanala bağlantı.
            string body = JsonSerializer.Serialize(value: productImageCreatedEvent); //mesaj
            byte[] bodyByte = Encoding.UTF8.GetBytes(s: body); 
            IBasicProperties properties = channel.CreateBasicProperties(); 
            properties.Persistent = true; //kalıcı olarak işaretlendi.

            channel.BasicPublish(exchange: RabbitMQClientService.ExchangeName, routingKey: RabbitMQClientService.RoutingWatermark, basicProperties: properties, body: bodyByte);
        }
    }

    public class productImageCreatedEvent
    {
        public string ImageName { get; set; }
    }
}