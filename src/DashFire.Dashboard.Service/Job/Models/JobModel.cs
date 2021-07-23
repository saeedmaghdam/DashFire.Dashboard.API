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

        public DateTime? LastExecutionDateTime
        {
            get;
            set;
        }

        public DateTime? NextExecutionDateTime
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

        public string SystemName
        {
            get;
            set;
        }

        public string DisplayName
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public bool RegistrationRequired
        {
            get;
            set;
        }

        public DateTime? HeartBitDateTime
        {
            get;
            set;
        }

        public JobExecutionMode JobExecutionMode
        {
            get;
            set;
        }

        public IJob OriginalJob
        {
            get;
            set;
        }
    }
}
