using System.Collections.Generic;

namespace DashFire.Dashboard.API.Apis.V1.Models.Job
{
    public class ExecuteInputModel
    {
        public IEnumerable<JobParameterInputModel> Parameters
        {
            get;
            set;
        }
    }
}
