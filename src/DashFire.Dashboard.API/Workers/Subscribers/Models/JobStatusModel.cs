using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.API.Workers.Subscribers.Models
{
    internal class JobStatusModel
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

        public JobStatus JobStatus
        {
            get;
            set;
        }
    }
}
