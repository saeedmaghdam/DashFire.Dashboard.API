using System;
using System.ComponentModel.DataAnnotations.Schema;
using DashFire.Dashboard.Framework.Constants;
using DashFire.Dashboard.Framework.Domains;

namespace DashFire.Dashboard.Domain
{
    [Table("job", Schema = "job")]
    public class Job : Entity
    {
        [ForeignKey("OriginalJobId")]
        public Job OriginalJob
        {
            get;
            set;
        }

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
        public DateTime? LastExecutionDateTime
        {
            get;
            set;
        }

        [Column("next_execution_date_time")]
        public DateTime? NextExecutionDateTime
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

        [Column("system_name")]
        public string SystemName
        {
            get;
            set;
        }

        [Column("display_name")]
        public string DisplayName
        {
            get;
            set;
        }

        [Column("description")]
        public string Description
        {
            get;
            set;
        }

        [Column("registration_required")]
        public bool RegistrationRequired
        {
            get;
            set;
        }

        [Column("heartbit_date_time")]
        public DateTime? HeartBitDateTime
        {
            get;
            set;
        }

        [Column("job_execution_mode")]
        public JobExecutionMode JobExecutionMode
        {
            get;
            set;
        }

        [Column("original_job_id")]
        public long? OriginalJobId
        {
            get;
            set;
        }
    }
}
