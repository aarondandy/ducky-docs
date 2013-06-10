using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using ServiceStack.Razor;
using ServiceStack.WebHost.Endpoints;

namespace DandyDoc.Web.ServiceStack
{
    public class Global : HttpApplication
    {

        public class AppHost : AppHostBase
        {
            public AppHost() : base("dandy-doc", typeof(AppHost).Assembly) { }

            public override void Configure(Funq.Container container) {
                Plugins.Add(new RazorFormat());
            }
        }

        protected void Application_Start(object sender, EventArgs e) {
            new AppHost().Init();
        }

    }
}