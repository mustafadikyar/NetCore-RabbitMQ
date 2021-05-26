using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace TopicExchange.Publisher
{
    public enum LogType
    {
        Critical = 1,
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// Detaylı routing yapısına ihtiyacımız olduğu zamanlarda kullanırız.
    /// Direct Exchange'den farkı routing yapısıdır.
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
            channel.ExchangeDeclare(exchange: "logs-topic", type: ExchangeType.Topic, durable: false, autoDelete: false, arguments: null);

            Random random = new Random();
            Enumerable.Range(1, 50).ToList().ForEach(item => //kuyruğa toplu mesaj gönderdik.
            {
                LogType log1 = (LogType)random.Next(1, 5);
                LogType log2 = (LogType)random.Next(1, 5);
                LogType log3 = (LogType)random.Next(1, 5);

                //Örnek root
                var routeKey = $"{log1}.{log2}.{log3}";
                
                string message = $"Log type : {log1}-{log2}-{log3}"; //gönderilecek mesaj
                byte[] body = Encoding.UTF8.GetBytes(message); //mesajlar byte array tipinde taşınır.

                //exchange : belirtmezsek doğrudan kuyruğa gönderilir.
                //routingkey : yönlendirilecek kuyruk adı
                //basicproperties : 
                //body : veri
                channel.BasicPublish(exchange: "logs-topic", routingKey: routeKey, basicProperties: null, body: body);
                Console.WriteLine($"{message}");
            });
            Console.ReadLine();
            //bir bağlantı üzerinden birden fazla kanal oluşturabiliriz.
        }
    }
}