using System;

namespace DashFire.Dashboard.Service.Job.Models
{
    public class JobModel : EntityRecord
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

        public string Parameters
        {
            get;
            set;
        }

        public bool IsOnline
        {
            get;
            set;
        }

        public DateTime LastExecutionDateTime
        {
            get;
            set;
        }

        public DateTime NextExecutionDateTime
        {
            get;
            set;
        }

        public string LastStatus
        {
            get;
            set;
        }
    }
}
