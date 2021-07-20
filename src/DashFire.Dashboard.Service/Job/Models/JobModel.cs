using System;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job.Models
{
    public class JobModel : EntityRecord, IJob
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

        public string LastStatusMessage
        {
            get;
            set;
        }

        public JobStatus Status
        {
            get;
            set;
        }
    }
}
