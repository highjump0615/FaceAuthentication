using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FaceRecog
{
    public class ResponseMessage
    {
        public bool success { get; set; }
        public string message { get; set; }

        public string redirectUrl { get; set; }

        public ResponseMessage(bool su, string msg)
        {
            success = su;
            message = msg;
        }
    }
}