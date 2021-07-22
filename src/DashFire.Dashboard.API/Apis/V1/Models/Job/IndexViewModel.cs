using System;
using System.Collections.Generic;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.API.Apis.V1.Models.Job
{
    public class IndexViewModel
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

        public IEnumerable<JobParameterViewModel> Parameters
        {
            get;
            set;
        }

        public DateTime? HeartBitDateTime
        {
            get;
            set;
        }
    }
}
