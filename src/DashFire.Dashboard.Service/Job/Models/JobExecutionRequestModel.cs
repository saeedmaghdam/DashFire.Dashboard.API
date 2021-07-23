using System.Collections.Generic;
using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job.Models
{
    internal class JobExecutionRequestModel
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

        public string NewInstanceId
        {
            get;
            set;
        }

        public IEnumerable<IJobParameterValue> Parameters
        {
            get;
            set;
        }
    }
}
