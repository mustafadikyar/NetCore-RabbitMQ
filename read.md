docker run -d -p 5672:5672 -p 15672:15672 --name rabbitmqcontainer rabbitmq:3.8.16-management

- Mesajlar� kal�c� hale getirmek i�in;
properties.Persistent = true;