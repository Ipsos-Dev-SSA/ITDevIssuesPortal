using CallCentreFollowUps.Models;
using CallCentreFollowUps.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CallCentreFollowUps.ViewModels
{
    public class ScreeningHistoryModel
    {

        public int ScreeningID { get; set; }
        public DateTime? ScreeningDate { get; set; }
    }
}
