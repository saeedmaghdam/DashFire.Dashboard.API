using DashFire.Dashboard.Framework.Services.Job;

namespace DashFire.Dashboard.Service.Job.Models
{
    public class JobParameterModel : IJobParameter
    {
        public string ParameterName
        {
            get;
            set;
        }

        public string TypeFullName
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
    }
}
