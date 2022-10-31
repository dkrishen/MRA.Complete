﻿using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using Microsoft.Extensions.Hosting;
using MRA.Bookings.Repositories;
using Newtonsoft.Json;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using MRA.Bookings.Logic.SignalR;
using MRA.Bookings.Models;

namespace MRA.Bookings.Logic.RabbitMQ.Consumers
{
    public class RabbitMqDeleteListener : BackgroundService
    {
        private string hostName;
        private string exchangeName;
        private string exchangeType;
        private string queueName;
        private string routingKey;
        private IConnection _connection;
        private IModel _channel;
        private IServiceProvider _serviceProvider;
        private readonly ISignalRClient signalRClient;

        public RabbitMqDeleteListener(IServiceProvider provider, IConfiguration configuration, ISignalRClient signalRClient)
        {
            this.hostName = configuration.GetSection("RabbitMQ").GetValue<string>("HostName");
            this.exchangeName = configuration.GetSection("RabbitMQ").GetValue<string>("ExchangeName");
            this.exchangeType = configuration.GetSection("RabbitMQ").GetValue<string>("ExchangeType");
            this.queueName = configuration.GetSection("RabbitMQ").GetValue<string>("DeleteQueueName");
            this.routingKey = configuration.GetSection("RabbitMQ").GetValue<string>("DeleteRoutingKey");

            _serviceProvider = provider;
            this.signalRClient = signalRClient;
            var factory = new ConnectionFactory { HostName = hostName };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: exchangeName, type: exchangeType);

            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested)
            {
                _channel.Dispose();
                _connection.Dispose();
                return Task.CompletedTask;
            }

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                var jsonData = Encoding.UTF8.GetString(ea.Body.ToArray());
                var idTokenDto = JsonConvert.DeserializeObject<GuidTokenDto>(jsonData);

                using (var scope = _serviceProvider.CreateScope())
                {
                    var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                    var booking = await bookingRepository.GetBookingByIdAsync(idTokenDto.Id);
                    await bookingRepository.DeleteBookingAsync(idTokenDto.Id);
                    await signalRClient.SendNotificationAsync($"Your booking at {booking.StartTime.ToString()} removed succesfully!", idTokenDto.Token);
                }

                _channel.BasicAck(ea.DeliveryTag, false);
            };

            _channel.BasicConsume(queueName, false, consumer);
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
