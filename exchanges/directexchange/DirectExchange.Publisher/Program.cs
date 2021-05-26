using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace DirectExchange.Publisher
{
    public enum LogType
    {
        Critical = 1,
        Error,
        Warning,
        Info
    }

    /// <summary>
    /// Mesaj doğrudan routingKey üzerinden eşleştirilen kuyruk üzerinde işlemleri gerçekleştirir.
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
            channel.ExchangeDeclare(exchange: "logs-direct", type: ExchangeType.Direct, durable: false, autoDelete: false, arguments: null);

            Enum.GetNames(typeof(LogType)).ToList().ForEach(log =>
            {
                var routeKey = $"route-{log}";
                var queueName = $"direct-queue-{log}";

                //kuyruk oluşturuyoruz.
                //queue : kuyruğun adı
                //durable : kuyruğun nerede tutulacakğı (false : memory | true : disk)
                //exclusive : bağlantı türü
                //autodelete : en az bir tüketiciye sahip olan kuyruk, son tüketici abonelikten çıktığında silinir.
                //arguments : (isteğe bağlı) eklentiler ve mesaj TTL'si, kuyruk uzunluğu sınırı vb. Gibi aracıya özgü özellikler tarafından kullanılır.
                channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);

                //routeKey : yönlendirilecek kuyruk.
                channel.QueueBind(queue: queueName, exchange: "logs-direct", routingKey: routeKey, arguments: null);

            });

            Enumerable.Range(1, 50).ToList().ForEach(item => //kuyruğa toplu mesaj gönderdik.
            {
                LogType log = (LogType)new Random().Next(1, 5);

                string message = $"Log type : {log}"; //gönderilecek mesaj
                byte[] body = Encoding.UTF8.GetBytes(message); //mesajlar byte array tipinde taşınır.

                var routeKey = $"route-{log}";

                //exchange : belirtmezsek doğrudan kuyruğa gönderilir.
                //routingkey : yönlendirilecek kuyruk adı
                //basicproperties : 
                //body : veri
                channel.BasicPublish(exchange: "logs-direct", routingKey: routeKey, basicProperties: null, body: body);
                Console.WriteLine($"Log : {log}");
            });
            Console.ReadLine();
            //bir bağlantı üzerinden birden fazla kanal oluşturabiliriz.
        }
    }
}