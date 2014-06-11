using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Oclumen.Crawler.DataContexts;

namespace Oclumen.Web.Controllers
{
    public class BaseController : Controller
    {
        public BaseController()
        {
            OclumenContext = new OclumenContext();
        }

        protected OclumenContext OclumenContext { get; set; }

        protected IDictionary<String, String> Dictionary
        {
            get { return (IDictionary<String, String>) System.Web.HttpContext.Current.Application["dictionary"]; }
        }
    }
}