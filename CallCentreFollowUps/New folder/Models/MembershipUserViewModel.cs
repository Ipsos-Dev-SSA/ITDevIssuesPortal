using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCentreFollowUps.Models
{
    public class MembershipUserViewModel
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public DateTime LastLoginDate { get; set; }
        public bool IsApproved { get; set; }
        public bool IsLockedOut { get; set; }
    }
}