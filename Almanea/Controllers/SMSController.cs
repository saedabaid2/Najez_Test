using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Almanea.Models;

namespace Almanea.Controllers
{
    public class SMSController : ApiController
    {
        [Route("api/CompleteOrder")]
        [HttpGet]
        public Boolean CompleteOrder()
        {

            return true;
        }
        
    }
}

