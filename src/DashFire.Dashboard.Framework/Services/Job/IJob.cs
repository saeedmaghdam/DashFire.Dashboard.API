﻿using System;

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

        DateTime LastExecutionDateTime
        {
            get;
            set;
        }

        DateTime NextExecutionDateTime
        {
            get;
            set;
        }

        string LastStatusMessage
        {
            get;
            set;
        }
    }
}
