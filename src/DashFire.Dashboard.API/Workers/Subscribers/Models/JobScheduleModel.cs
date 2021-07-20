using System;

namespace DashFire.Dashboard.API.Workers.Subscribers.Models
{
    internal class JobScheduleModel
    {
        public string Key
        {
            get;
            set;
        }

        public string InstanceId
        {
            get;
            set;
        }

        public DateTime NextExecutionDateTime
        {
            get;
            set;
        }
    }
}
