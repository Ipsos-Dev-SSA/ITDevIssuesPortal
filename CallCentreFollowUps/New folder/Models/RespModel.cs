using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCentreFollowUps.Models
{
    public class RespModel
    {
        public List<RespDataBase> RespondentList { get; set; }
        public int agentId { get; set; }
        public string Name { get; set; }

        public int RespondentID { get; set; }
        public string PhaseStatus { get; set; }
        public string PhaseOutcome { get; set; }
        public string InterviewStatus { get; set; }
        public string InterviewOutcome { get; set; }
    }
}