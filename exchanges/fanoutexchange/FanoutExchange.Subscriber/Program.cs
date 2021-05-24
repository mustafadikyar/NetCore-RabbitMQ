using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace FanoutExchange.Subscriber
{
    class Program
    {
        static void Main(string[] args)
        {
            ConnectionFactory factory = new(); //rabbitmq için bir bağlantı örneği oluşturduk.
            factory.Uri = new Uri("amqps://gpgmtthl:dtDiiqd4qyog_b3f7WjHYjVWlYvai-ff@baboon.rmq.cloudamqp.com/gpgmtthl"); //bağlantı adresi

            using IConnection connection = factory.CreateConnection(); //bağlantı açıyoruz.
            using IModel channel = connection.CreateModel(); //kanal oluşturuyoruz.

            //kuyruğu kalıcı hale getirmek için
            //var queueName = "save-queue";
            //channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false);


            //rasgele kuyruk ismi oluşturur. isim çakışmalarını önlemek için.
            var randomQueueName = channel.QueueDeclare().QueueName;

            //uygulama her ayağa kalktığında bir kuyruk oluşacak.
            channel.QueueBind(queue: randomQueueName, exchange: "logs-fanout", routingKey: string.Empty, arguments: null);

            //mesajlara erişim kıstasları
            //prefetchSize : mesaj boyutu (0 : herhangi bir boyuttaki mesaj)
            //prefetchCount : mesajlar tüketicilere kaçar kaçar gönderilecek.
            //global :  toplamda tüm kullanıcılara kaçar kaçar gönderilecek. 3 consumer ve 6 mesaj için her consumer'a ikişer tane mesaj gönderir.
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);


            //tüketici örneği oluşturuyoruz. 
            EventingBasicConsumer consumer = new(channel);

            //queue : izlenecek kuyruk
            //autoAck : (true : mesaj işlendiği zaman (başarılı başarısız farketmez) kuyruktan silinir. | false : işlem başarılıysa kuyruktan silmesini biz bildiriyoruz.)
            //consumer : tüketici
            channel.BasicConsume(queue: randomQueueName, autoAck: false, consumer: consumer);
            Console.WriteLine("Mesaj bekleniyor...");

            //mesajı dinleyen event.
            consumer.Received += (object sender, BasicDeliverEventArgs e) =>
            {
                var message = Encoding.UTF8.GetString(e.Body.ToArray()); //yakalanan mesaj
                Console.WriteLine($"Gelen mesaj : {message}");

                //işlemin kuyruktan silinmesi için haber yolluyoruz.
                //deliveryTag : gelen mesajın adresi
                //multiple : eğer kuyrukta işlem tamamlanmış fakat silinmemiş işlemler var ise onları siler.
                channel.BasicAck(deliveryTag: e.DeliveryTag, multiple: false);
            };
            Console.ReadLine();
        }
    }
}
