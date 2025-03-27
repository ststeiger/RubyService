<%@ WebHandler Language="C#" Class="MyApplicatoin.Json" %>

namespace MyApplicatoin
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für Json
    /// </summary>
    public class Json 
            : System.Web.IHttpHandler
    {

        public void ProcessRequest(System.Web.HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write("{ \"foo\":\"bar\"}");
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