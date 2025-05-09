using Hangfire.Annotations;
using Hangfire.Dashboard;

namespace T1_Wormhole_2._0._1.Filter
{
    public class ClaimBaseHangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _roleName;

        public ClaimBaseHangfireDashboardAuthorizationFilter(string roleName)
        {
            _roleName = roleName;
        }
        public bool Authorize([NotNull] DashboardContext context)
        {
            return context.GetHttpContext().User.IsInRole(_roleName);
        }
    }
}
