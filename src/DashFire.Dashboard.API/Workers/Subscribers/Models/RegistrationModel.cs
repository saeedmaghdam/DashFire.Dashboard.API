using System.Collections.Generic;

namespace DashFire.Dashboard.API.Workers.Subscribers.Models
{
    internal class RegistrationModel
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

        public List<JobParameterModel> Parameters
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
    }
}
