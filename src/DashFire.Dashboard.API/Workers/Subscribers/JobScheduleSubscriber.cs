using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DashFire.Dashboard.Framework;
using DashFire.Dashboard.Framework.Cache;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Options;
using DashFire.Dashboard.Framework.Services.Job;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace DashFire.Dashboard.API.Workers.Subscribers
{
    public class JobScheduleSubscriber : IHostedService
    {
        private readonly IOptions<ApplicationOptions> _options;
        private readonly ILogger<JobScheduleSubscriber> _logger;
        private readonly IServiceProvider _serviceProvider;

        private const string _serviceSideQueueName = "DashFire.Dashboard";

        private const string _dashboardSideExchangeName = "DashFire.Service";

        private readonly IConnection _connection;
        private readonly IModel _channel;

        public JobScheduleSubscriber(IOptions<ApplicationOptions> options, ILogger<JobScheduleSubscriber> logger, IServiceProvider serviceProvider, DashFireCacheManager cacheManager)
        {
            cacheManager.InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();

            _options = options;
            _logger = logger;
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

            _channel.BasicConsume($"{_serviceSideQueueName}_{MessageTypes.JobSchedule}", true, consumer);

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

            var jobScheduleModel = JsonSerializer.Deserialize<Models.JobScheduleModel>(message);

            int remainingAttempts = 3;
            do
            {
                try
                {
                    ProcessMessage(jobScheduleModel);
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                    remainingAttempts--;
                }
            } while (remainingAttempts != 0);
        }

        private void ProcessMessage(Models.JobScheduleModel model)
        {
            var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using (var scope = _serviceProvider.CreateScope())
            {
                var jobService = scope.ServiceProvider.GetRequiredService<IJobService>();
                jobService.PatchJobNextExecutionDateTimeAsync(model.Key, model.InstanceId, model.NextExecutionDateTime, cancellationTokenSource.Token).GetAwaiter().GetResult();
            }
        }
    }
}
