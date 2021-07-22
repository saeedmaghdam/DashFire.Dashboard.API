using System;

namespace DashFire.Dashboard.API.Apis.V1.Models.Job
{
    public class LogViewModel
    {
        public DateTime RecordInsertDateTime
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }
    }
}
