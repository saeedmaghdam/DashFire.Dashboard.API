using System.Collections.Generic;
using DashFire.Dashboard.Framework.Constants;
using RabbitMQ.Client;

namespace DashFire.Dashboard.Framework
{
    public class QueueManager
    {
        private const string _serviceSideExchangeName = "DashFire.Dashboard";
        private const string _serviceSideQueueName = "DashFire.Dashboard";

        public static void Initialize(IModel channel)
        {
            // Declare dashboard exchanges and queue
            channel.ExchangeDeclare(_serviceSideExchangeName, "headers", true);

            var serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.Registration}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.Registration.ToString().ToLower()
                }
            });

            serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.HeartBit}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.HeartBit.ToString().ToLower()
                }
            });

            serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.JobStatus}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.JobStatus.ToString().ToLower()
                }
            });

            serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.LogJobStatus}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.LogJobStatus.ToString().ToLower()
                }
            });

            serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.JobSchedule}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.JobSchedule.ToString().ToLower()
                }
            });

            serviceSideQueueName = $"{_serviceSideQueueName}_{MessageTypes.Shutdown}";
            channel.QueueDeclare(queue: serviceSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(serviceSideQueueName, _serviceSideExchangeName, string.Empty, new Dictionary<string, object>()
            {
                {
                    "message_type", MessageTypes.Shutdown.ToString().ToLower()
                }
            });
        }

        public static void DeclareExchangeAndQueue(IModel channel, string exchangeName, string jobKey, string jobInstanceId, IDictionary<string, object> headers)
        {
            var dashboardSideQueueName = $"{exchangeName}_{jobKey}_{jobInstanceId}";

            channel.ExchangeDeclare(exchangeName, "headers", true);
            channel.QueueDeclare(queue: dashboardSideQueueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);
            channel.QueueBind(dashboardSideQueueName, exchangeName, string.Empty, headers);
        }
    }
}
