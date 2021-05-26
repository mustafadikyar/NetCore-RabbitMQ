docker run -d -p 5672:5672 -p 15672:15672 --name rabbitmqcontainer rabbitmq:3.8.16-management

- Mesajlarý kalýcý hale getirmek için;
properties.Persistent = true;

- ComplexType'ler ile çalýþmak;
content byte array olduðu için nesnelerimizi önce string'e serialize edip sonra byte array'a cast edip buradan gönderebiliriz.