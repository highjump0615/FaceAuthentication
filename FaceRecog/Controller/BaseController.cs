using Newtonsoft.Json;
using ShopifyMultipassTokenGenerator;
using ShopifyMultipassTokenGenerator.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace FaceRecog.Controller
{
    public class BaseController : ApiController
    {
        /// <summary>
        /// Make Redirect response for failure
        /// </summary>
        /// <param name="response"></param>
        /// <param name="returnUri"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        protected ResponseMessage makeRedirectResponse(ResponseMessage response, string returnUri, string from)
        {
            response.redirectUrl = "default.aspx";

            if ("myshopify".Equals(from))
            {
                response.redirectUrl = "https://" + returnUri;
            }

            return response;
        }

        /// <summary>
        /// Make Shopify Login Url 
        /// </summary>
        /// <param name="email"></param>
        /// <param name="returnUri"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        protected ResponseMessage makeShopifyLoginResponse(string email, string returnUri, string from)
        {
            // Succeeded
            var response = new ResponseMessage(true, "OK");
            response.redirectUrl = "MainSite.aspx";

            if ("myshopify".Equals(from) && !String.IsNullOrEmpty(email))
            {
                var input = new Customer()
                {
                    Email = email
                };

                var secret = ConfigurationManager.AppSettings["shopifyMultipassSecret"];
                var customerJSONString = JsonConvert.SerializeObject(input);

                ShopifyMultipass shopifyMultipass = new ShopifyMultipass(secret, returnUri);

                // Generate url for shopify login 
                response.redirectUrl = shopifyMultipass.Process(customerJSONString);
            }

            return response;
        }
    }
}
