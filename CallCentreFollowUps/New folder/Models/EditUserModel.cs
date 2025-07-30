using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCentreFollowUps.Models
{
    public class EditUserModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public int CountryID { get; set; }
        public string RoleName { get; set; }
    }

}