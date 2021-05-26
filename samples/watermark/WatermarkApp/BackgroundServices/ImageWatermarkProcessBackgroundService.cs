using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using WatermarkApp.Services;

namespace WatermarkApp.BackgroundServices
{
    public class ImageWatermarkProcessBackgroundService : BackgroundService
    {
        private readonly RabbitMQClientService _rabbitMQClientService;
        private readonly ILogger<ImageWatermarkProcessBackgroundService> _logger;
        private IModel _channel;
        
        public ImageWatermarkProcessBackgroundService(RabbitMQClientService rabbitMQClientService, ILogger<ImageWatermarkProcessBackgroundService> logger)
        {
            _rabbitMQClientService = rabbitMQClientService;
            _logger = logger;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            _channel = _rabbitMQClientService.Connect(); //kanala bağlanıyor.
            _channel.BasicQos(0, 1, false); //kanal genişliği
            return base.StartAsync(cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            AsyncEventingBasicConsumer consumer = new(_channel);
            _channel.BasicConsume(RabbitMQClientService.QueueName, false, consumer); 
            consumer.Received += Consumer_Received;
            return Task.CompletedTask;
        }

        //watermak eklemek.
        private Task Consumer_Received(object sender, BasicDeliverEventArgs @event)
        {
            try
            {
                productImageCreatedEvent productImageCreatedEvent = JsonSerializer.Deserialize<productImageCreatedEvent>(Encoding.UTF8.GetString(@event.Body.ToArray()));
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", productImageCreatedEvent.ImageName);
                string siteName = "www.mustafadikyar.com";
                
                using Image img = Image.FromFile(path);
                using Graphics graphic = Graphics.FromImage(img);

                Font font = new(family: FontFamily.GenericMonospace, emSize: 40, style: FontStyle.Bold, unit: GraphicsUnit.Pixel);
                SizeF textSize = graphic.MeasureString(siteName, font);
                Color color = Color.FromArgb(128, 255, 255, 255);
                SolidBrush brush = new(color);
                Point position = new(x: img.Width - ((int)textSize.Width + 30), y: img.Height - ((int)textSize.Height + 30));
                graphic.DrawString(s: siteName, font: font, brush: brush, point: position);
                img.Save("wwwroot/Images/watermarks/" + productImageCreatedEvent.ImageName);

                img.Dispose();
                graphic.Dispose();

                _channel.BasicAck(@event.DeliveryTag, false); //işlemin tamamlandığını bildiriyor.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
            return Task.CompletedTask;
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            return base.StopAsync(cancellationToken);
        }
    }
}