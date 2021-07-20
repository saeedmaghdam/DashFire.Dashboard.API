using System;
using System.ComponentModel.DataAnnotations.Schema;
using DashFire.Dashboard.Framework.Domains;

namespace DashFire.Dashboard.Domain
{
    [Table("job", Schema = "job")]
    public class Job : Entity
    {
        [Column("key")]
        public string Key
        {
            get;
            set;
        }

        [Column("instance_id")]
        public string InstanceId
        {
            get;
            set;
        }

        [Column("parameters")]
        public string Parameters
        {
            get;
            set;
        }

        [Column("is_online")]
        public bool IsOnline
        {
            get;
            set;
        }

        [Column("last_execution_date_time")]
        public DateTime LastExecutionDateTime
        {
            get;
            set;
        }

        [Column("next_execution_date_time")]
        public DateTime NextExecutionDateTime
        {
            get;
            set;
        }

        [Column("last_status_message")]
        public string LastStatusMessage
        {
            get;
            set;
        }

        [Column("status")]
        public short Status
        {
            get;
            set;
        }
    }
}
