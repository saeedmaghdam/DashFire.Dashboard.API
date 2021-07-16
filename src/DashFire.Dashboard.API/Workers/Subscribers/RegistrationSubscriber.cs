using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Options;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashFire.Dashboard.API.Workers.Subscribers
{
    public class RegistrationSubscriber : IHostedService
    {
        private readonly IOptions<ApplicationOptions> _options;

        private const string _serviceSideQueueName = "DashFire.Dashboard";

        private const string _dashboardSideExchangeName = "DashFire.Service";

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public RegistrationSubscriber(IOptions<ApplicationOptions> options)
        {
            _options = options;

            var factory = new ConnectionFactory() { Uri = new Uri(_options.Value.RabbitMqOptions.ConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            QueueManager.Initialize(_channel);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ConsumerReceived;

            _channel.BasicConsume($"{_serviceSideQueueName}_Registration", true, consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void ConsumerReceived(object sender, BasicDeliverEventArgs e)
        {
            var body = e.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            
            var registrationModel = JsonSerializer.Deserialize<Models.RegistrationModel>(message);

            ProcessMessage(registrationModel);
        }

        private void ProcessMessage(Models.RegistrationModel model)
        {
            var headers = new Dictionary<string, object>()
            {
                {
                    "job_key", model.Key
                },
                {
                    "job_instance_id", model.InstanceId
                }
            };

            QueueManager.DeclareExchangeAndQueue(_channel, _dashboardSideExchangeName, model.Key, model.InstanceId, headers);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = false;
            headers.Add("message_type", MessageTypes.Registration.ToString().ToLower());
            properties.Headers = headers;

            var responseModel = new Models.RegistrationResponseModel();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseModel));
            _channel.BasicPublish(_dashboardSideExchangeName, "", properties, messageBodyBytes);
        }
    }
}
