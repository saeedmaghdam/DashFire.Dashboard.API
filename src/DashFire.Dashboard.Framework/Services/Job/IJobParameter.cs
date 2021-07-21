namespace DashFire.Dashboard.Framework.Services.Job
{
    public interface IJobParameter
    {
        string ParameterName
        {
            get;
            set;
        }

        string TypeFullName
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
    }
}
