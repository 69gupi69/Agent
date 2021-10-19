using System.Web;
using System.Web.Mvc;

namespace Diascan.Agent.WebProject
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
