using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace FaceRecog
{
    public partial class signup : System.Web.UI.Page
    {
        public int majorVersion = 0;
        public string browserType;
        protected void Page_Load(object sender, EventArgs e)
        {
            HttpBrowserCapabilities browser = Request.Browser;
            browserType = browser.Type;
            majorVersion = browser.MajorVersion;
        }
    }
}