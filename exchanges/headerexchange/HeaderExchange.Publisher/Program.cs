using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HeaderExchange.Publisher
{
    /// <summary>
    /// Mesaj bilgilerini header üzerinden gönderdiğimiz exchange tipi.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new(); //rabbitmq için bir bağlantı örneği oluşturduk.

            //bağlantı adresi için rabbitmq-cloud kullanıyoruz.
            factory.Uri = new Uri("amqps://gpgmtthl:dtDiiqd4qyog_b3f7WjHYjVWlYvai-ff@baboon.rmq.cloudamqp.com/gpgmtthl"); //bağlantı adresi

            using IConnection connection = factory.CreateConnection(); //bağlantı açıyoruz.
            using IModel channel = connection.CreateModel(); //kanal oluşturuyoruz.           

            //exchange : adı
            //type : exchange tipi
            //durable : kuyruğun nerede tutulacakğı (false : memory | true : disk)
            //autodelete : en az bir tüketiciye sahip olan kuyruk, son tüketici abonelikten çıktığında silinir.
            //arguments : (isteğe bağlı) eklentiler ve mesaj TTL'si, kuyruk uzunluğu sınırı vb. Gibi aracıya özgü özellikler tarafından kullanılır.
            channel.ExchangeDeclare(exchange: "header-exchange", type: ExchangeType.Headers, durable: false, autoDelete: false, arguments: null);

            Dictionary<string, object> headers = new();
            headers.Add("format", "pdf");
            headers.Add("shape", "a4");

            IBasicProperties properties = channel.CreateBasicProperties();
            properties.Headers = headers;
            channel.BasicPublish(exchange: "header-exchange", routingKey: string.Empty, basicProperties: properties, body: Encoding.UTF8.GetBytes("message for header"));

            Console.WriteLine("Mesaj gönderildi.");
            Console.ReadLine();
        }
    }
}