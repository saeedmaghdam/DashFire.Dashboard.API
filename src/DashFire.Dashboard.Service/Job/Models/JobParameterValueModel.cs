using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job.Models
{
    public class JobParameterValueModel : IJobParameterValue
    {
        public string ParameterName
        {
            get;
            set;
        }

        public string Value
        {
            get;
            set;
        }
    }
}
