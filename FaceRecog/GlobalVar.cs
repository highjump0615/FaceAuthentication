using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace FaceRecog
{
    public static class GlobalVar
    {
        //public static String sqlDatabaseAccessString =  "Server=.;Database=FRSignIn;User Id=sa;Password=snow829hms;Integrated Security=SSPI;";
        public static String sqlDatabaseAccessString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        public static bool bInitialized = false;
    }
}