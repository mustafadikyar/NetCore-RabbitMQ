using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;

namespace FanoutExchange.Publisher
{
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
            //type : tipi
            //durable : kuyruğun nerede tutulacakğı (false : memory | true : disk)
            //autodelete : en az bir tüketiciye sahip olan kuyruk, son tüketici abonelikten çıktığında silinir.
            //arguments : (isteğe bağlı) eklentiler ve mesaj TTL'si, kuyruk uzunluğu sınırı vb. Gibi aracıya özgü özellikler tarafından kullanılır.
            channel.ExchangeDeclare(exchange: "logs-fanout", type: ExchangeType.Fanout, durable: false, autoDelete: false, arguments: null);


            Enumerable.Range(1, 50).ToList().ForEach(item => //kuyruğa toplu mesaj gönderdik.
            {
                string message = $"Log, Step {item}"; //gönderilecek mesaj
                byte[] body = Encoding.UTF8.GetBytes(message); //mesajlar byte array tipinde taşınır.

                //exchange : belirtmezsek doğrudan kuyruğa gönderilir.
                //routingkey : yönlendirilecek kuyruk adı
                //basicproperties : 
                //body : veri
                channel.BasicPublish(exchange: "logs-fanout", routingKey: string.Empty, basicProperties: null, body: body);
                Console.WriteLine($"Gönderilen mesaj : {item}");
            });
            Console.ReadLine();

            //bir bağlantı üzerinden birden fazla kanal oluşturabiliriz.
        }
    }
}
