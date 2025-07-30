using CallCentreFollowUps.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace CallCentreFollowUps.Models
{
    public class RespDataBase
    {
        public Boolean Selected { get; set; }
        public string ResonseId { get; set; }
        public string AddedBy { get; set; }

        public string DateAdded { get; set; }
        public string InstanceID { get; set; }
        public string PhoneNumber { get; set; }

        public string Q1 { get; set; }
        public string Q2 { get; set; }
        public string Q202_EDD { get; set; }
        public string Q3 { get; set; }
        public string Q607_C_1 { get; set; }
        public string Q608a_q608_C_1 { get; set; }
        public string Q609a_q609_C_1 { get; set; }
        public string Q610_ { get; set; }
        public string Q610a_q610_C_1 { get; set; }
        public string Q611a_q611_C_1 { get; set; }
        public string Q612_Phone_C_1 { get; set; }
        public string CheckInLevel { get; set; }
        public string VisitLevel { get; set; }
        public string Resp_FirstName { get; set; }
        public string Resp_Oth { get; set; }
        public string Resp_Surname { get; set; }
        public string RESPID { get; set; }
        public int RespondentID { get; set; }
        public string StatusID { get; set; }

        public string CheckIn1 { get; set; }
        public string CheckIn1Date { get; set; }
        public string CheckIn2 { get; set; }
        public string CheckIn2Date { get; set; }
        public string CheckIn3 { get; set; }
        public string CheckIn3Date { get; set; }
        public string CheckInDate { get; set; }
        public string CheckIn4 { get; set; }
        public string CheckIn4Date { get; set; }
        public string CheckIn5Date { get; set; }
        public string CheckIn1StatusId { get; set; } 
        public string CheckIn2StatusId { get; set; }
        public string CheckIn3StatusId { get; set; } 
        public string CheckIn4StatusId { get; set; }
        public string CheckIn5StatusId { get; set; }
        public string PhysicalCheckInLevel { get; set; }

        public string PhysicalAttempt1 { get; set; }
        public string PhysicalAttempt2 { get; set; }
        public string PhysicalAttempt3 { get; set; }

        public string  ScreeningLevel { get; set; }
        public string InterviewerLevel { get; set; }

        public int ScreeningID { get; set; }
        public DateTime? ScreeningDate { get; set; }
        public int InterviewID { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string Name { get; set; }
        public int? PhaseStatus { get; set; }
        public int? PhaseOutcome { get; set; }

        public int? interviewStatus { get; set; }
        public int? InterviewOutcome { get; set; }

        public Nullable<System.DateTime> EDD_Latest { get; set; }
        public string Status { get; set; }
        public string TimeZone { get; set; }

        public virtual ICollection<tblAgent> tblAgents { get; set; }

        public virtual tblAgent tblAgent { get; set; }
       
        public virtual ICollection<tblAgentActivity> tblAgentActivities { get; set; }

    }
}