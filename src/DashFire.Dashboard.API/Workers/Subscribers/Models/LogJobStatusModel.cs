using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.API.Workers.Subscribers.Models
{
    internal class LogJobStatusModel
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

        public string Message
        {
            get;
            set;
        }
    }
}
