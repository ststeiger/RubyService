
namespace RedmineInterface
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(System.Web.Mvc.GlobalFilterCollection filters)
        {
            filters.Add(new System.Web.Mvc.HandleErrorAttribute());
        }
    }
}
