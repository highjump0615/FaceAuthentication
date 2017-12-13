using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FaceRecog
{
    public static class GlobalVar
    {
        public static String sqlDatabaseAccessString = "Server=DESKTOP-N56981U;Database=FRSignIn;User Id=sa;Password=snow829hms;Integrated Security=SSPI;";
        public static bool bInitialized = false;
    }
}