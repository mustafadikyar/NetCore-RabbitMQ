using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace HelloWorld.Publisher
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

            //kuyruk oluşturuyoruz.
            //queue : kuyruğun adı
            //durable : kuyruğun nerede tutulacakğı (false : memory | true : disk)
            //exclusive : bağlantı türü
            //autodelete : en az bir tüketiciye sahip olan kuyruk, son tüketici abonelikten çıktığında silinir.
            //arguments : (isteğe bağlı) eklentiler ve mesaj TTL'si, kuyruk uzunluğu sınırı vb. Gibi aracıya özgü özellikler tarafından kullanılır
            channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);
            //Kuyruk var ise kullanır yok ise sıfırdan oluşturur.

            Enumerable.Range(1, 50).ToList().ForEach(item => //kuyruğa toplu mesaj gönderdik.
            {
                string message = $"Hello World! Step {item}"; //gönderilecek mesaj
                byte[] body = Encoding.UTF8.GetBytes(message); //mesajlar byte array tipinde taşınır.

                //exchange : belirtmezsek doğrudan kuyruğa gönderilir.
                //routingkey : yönlendirilecek kuyruk adı
                //basicproperties : 
                //body : veri
                channel.BasicPublish(exchange: string.Empty, routingKey: "hello-queue", basicProperties: null, body: body);
                Console.WriteLine($"Gönderilen mesaj : {item}");
            });
            Console.ReadLine();

            //bir bağlantı üzerinden birden fazla kanal oluşturabiliriz.
        }
    }
}
