using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using ShopDunk.Helpers;

namespace ShopDunk
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            try
            {
                Logger.Log("Unhandled exception: " + ex.ToString());
            }
            catch
            {
                // nếu logging lỗi thì bỏ qua để tránh vòng lặp
            }
        }
    }
}