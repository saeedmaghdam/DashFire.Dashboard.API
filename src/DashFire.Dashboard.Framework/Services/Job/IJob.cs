using System;
using DashFire.Dashboard.Framework.Constants;

namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJob : IEntityRecord
    {
        string Key
        {
            get;
            set;
        }

        string InstanceId
        {
            get;
            set;
        }

        string Parameters
        {
            get;
            set;
        }

        bool IsOnline
        {
            get;
            set;
        }

        DateTime? LastExecutionDateTime
        {
            get;
            set;
        }

        DateTime? NextExecutionDateTime
        {
            get;
            set;
        }

        string LastStatusMessage
        {
            get;
            set;
        }

        JobStatus Status
        {
            get;
            set;
        }

        string SystemName
        {
            get;
            set;
        }

        string DisplayName
        {
            get;
            set;
        }

        string Description
        {
            get;
            set;
        }

        bool RegistrationRequired
        {
            get;
            set;
        }

        DateTime? HeartBitDateTime
        {
            get;
            set;
        }

        JobExecutionMode JobExecutionMode
        {
            get;
            set;
        }

        IJob OriginalJob
        {
            get;
            set;
        }
    }
}
