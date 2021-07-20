﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Options;
using DashFire.Dashboard.Framework.Services.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashFire.Dashboard.API.Workers.Subscribers
{
    public class JobStatusSubscriber : IHostedService
    {
        private readonly IOptions<ApplicationOptions> _options;
        private readonly IServiceProvider _serviceProvider;

        private const string _serviceSideQueueName = "DashFire.Dashboard";

        private const string _dashboardSideExchangeName = "DashFire.Service";

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public JobStatusSubscriber(IOptions<ApplicationOptions> options, IServiceProvider serviceProvider)
        {
            _options = options;
            _serviceProvider = serviceProvider;

            var factory = new ConnectionFactory() { Uri = new Uri(_options.Value.RabbitMqOptions.ConnectionString) };
            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            QueueManager.Initialize(_channel);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += ConsumerReceived;

            _channel.BasicConsume($"{_serviceSideQueueName}_{MessageTypes.JobStatus}", true, consumer);

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

            var jobStatusModel = JsonSerializer.Deserialize<Models.JobStatusModel>(message);

            ProcessMessage(jobStatusModel);
        }

        private void ProcessMessage(Models.JobStatusModel model)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using (var scope = _serviceProvider.CreateScope())
            {
                var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                jobService.PatchJobStatus(model.Key, model.InstanceId, model.JobStatus, cancellationTokenSource.Token).GetAwaiter().GetResult();
            }

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
            headers.Add("message_type", MessageTypes.JobStatus.ToString().ToLower());
            properties.Headers = headers;

            var responseModel = new Models.JobStatusModel();
            var messageBodyBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(responseModel));
            _channel.BasicPublish(_dashboardSideExchangeName, "", properties, messageBodyBytes);
        }
    }
}
