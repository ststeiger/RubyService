<%@ WebHandler Language="C#" Class="MyApplicatoin.GetSettings" %>

namespace MyApplicatoin
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für GetSettings
    /// </summary>
    public class GetSettings 
        : System.Web.IHttpHandler
    {

        public void ProcessRequest(System.Web.HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            context.Response.Write("Hello World");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
