using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Mvc;
using CallCentreFollowUps.Models;

namespace CallCentreFollowUps.Controllers
{
    public class ValuesController : Controller
    {
        // GET api/values
        public IEnumerable<object> Get()
        {

            CallCentreTrackerEntities1 db = new CallCentreTrackerEntities1();
            var RespondentDetails = db.tblRespondents.Select(x => new { x.RespondentID, x.Resp_Surname });
            return RespondentDetails;
            //return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
