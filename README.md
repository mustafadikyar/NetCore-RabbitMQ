# NetCore-RabbitMQ

## RABBITMQ

### HELLO-WORLD

RabbitMQ ile tanıştığımız bu ilk adımda evrensel yazılım geleniğimizi sürdürelim ve dünyayı selamlayalım.

En temel senaryomuz olarak publisher üzerinden mesajımızı göndereceğiz. Subscriber(Consumer) üzerinden bu mesajı yakalayacağız.

- Publisher

```csharp
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

string message = "Hello World!"; //gönderilecek mesaj
byte[] body = Encoding.UTF8.GetBytes(message); //mesajlar byte array tipinde taşınır.

//exchange : belirtmezsek doğrudan kuyruğa gönderilir.
//routingkey : yönlendirilecek kuyruk adı
//basicproperties : 
//body : veri
channel.BasicPublish(exchange: string.Empty, routingKey: "hello-queue", basicProperties: null, body: body);
Console.WriteLine("Mesaj gönderildi.");
Console.ReadLine();

//bir bağlantı üzerinden birden fazla kanal oluşturabiliriz.
```

- Consumer

```csharp
ConnectionFactory factory = new(); //rabbitmq için bir bağlantı örneği oluşturduk.
factory.Uri = new Uri("amqps://gpgmtthl:dtDiiqd4qyog_b3f7WjHYjVWlYvai-ff@baboon.rmq.cloudamqp.com/gpgmtthl"); //bağlantı adresi

using IConnection connection = factory.CreateConnection(); //bağlantı açıyoruz.
using IModel channel = connection.CreateModel(); //kanal oluşturuyoruz.

//kuyruk oluşturuyoruz.
//queue : kuyruğun adı
//durable : kuyruğun nerede tutulacakğı (false : memory | true : disk)
//exclusive : bağlantı türü
//autodelete : en az bir tüketiciye sahip olan kuyruk, son tüketici abonelikten çıktığında silinir.
//arguments : (isteğe bağlı) eklentiler ve mesaj TTL'si, kuyruk uzunluğu sınırı vb. Gibi aracıya özgü özellikler tarafından kullanılır

//subscriber kuyruğu oluşturduğundan emin değilsek burada kuyruğu oluşturuyoruz.
//parametrelerin tutarlılığı önemli.
//channel.QueueDeclare(queue: "hello-queue", durable: true, exclusive: false, autoDelete: false, arguments: null);

//tüketici örneği oluşturuyoruz. 
EventingBasicConsumer consumer = new(channel);

//queue : izlenecek kuyruk
//autoAck : (true : mesaj işlendiği zaman (başarılı başarısız farketmez) kuyruktan silinir. | false : işlem başarılıysa kuyruktan silmesini biz bildiriyoruz.)
//consumer : tüketici
channel.BasicConsume(queue: "hello-queue", autoAck: true, consumer: consumer);

//mesajı dinleyen event.
consumer.Received += (object sender ,BasicDeliverEventArgs e) =>
{
    var message = Encoding.UTF8.GetString(e.Body.ToArray()); //yakalanan mesaj
    Console.WriteLine($"Gelen mesaj : {message}");
};
Console.ReadLine();
```
