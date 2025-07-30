using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CallCentreFollowUps.Models
{
    public class RespondentViewModel
    {
        public int RespondentID { get; set; }
        public string ResponseId { get; set; }
        public string InstanceID { get; set; }
        public string RESPID { get; set; }
        public string Q1 { get; set; }
        public string Q2 { get; set; }
        public string Q3 { get; set; }
        public string Q202_EDD { get; set; }
        public string Resp_Surname { get; set; }
        public string Resp_FirstName { get; set; }
        public string Resp_Oth { get; set; }
        public string PhoneNumber { get; set; }
        public string Q607_C_1 { get; set; }
        public string Q608a_q608_C_1 { get; set; }
        public string Q609a_q609_C_1 { get; set; }
        public string Q610a_q610_C_1 { get; set; }
        public string Q611a_q611_C_1 { get; set; }
        public string Q612_Phone_C_1 { get; set; }
        public string TimeZone { get; set; }
        public string Q610_ { get; set; }
        public string CheckIn1 { get; set; }
        public DateTime? CheckIn1Date { get; set; }
        public string CheckIn2 { get; set; }
        public DateTime? CheckIn2Date { get; set; }
        public string CheckIn3 { get; set; }
        public DateTime? CheckIn3Date { get; set; }
        public string CheckIn4 { get; set; }
        public DateTime? CheckIn4Date { get; set; }
        public string EDD_Latest { get; set; }
        public string CheckInLevelID { get; set; }
        public string CheckInLevel { get; set; }
        public DateTime? CheckInDate { get; set; }
        public string StatusID { get; set; }
        public DateTime? DateAdded { get; set; }
        public string AddedBy { get; set; }
        public string CheckIn1StatusId { get; set; }
        public string CheckIn2StatusId { get; set; }
        public string CheckIn3StatusId { get; set; }
        public string CheckIn4StatusId { get; set; }
        public string CheckIn5StatusId { get; set; }
        public string PhysicalAttempt1 { get; set; }
        public string PhysicalAttempt2 { get; set; }
        public string PhysicalAttempt3 { get; set; }
        public string CheckIn5 { get; set; }
        public DateTime? CheckIn5Date { get; set; }
        public DateTime? PhysicalAttempt1Date { get; set; }
        public DateTime? PhysicalAttempt2Date { get; set; }
        public DateTime? PhysicalAttempt3Date { get; set; }
        public string VisitLevel { get; set; }
        public string ScreeningID { get; set; }
        public string InterviewID { get; set; }
        public DateTime? ScreeningDate { get; set; }
        public DateTime? InterviewDate { get; set; }
        public string PhysicalAttemptStatusId1 { get; set; }
        public string PhysicalAttemptStatusId2 { get; set; }
        public string PhysicalAttemptStatusId3 { get; set; }
        public string ScreeningLevel { get; set; }
        public string InterviewerLevel { get; set; }
        public string PhaseStatus { get; set; }
        public string PhaseOutcome { get; set; }
        public string ScreeningCheckIn { get; set; }
        public string InterviewCheckIn { get; set; }
        public string interviewStatus { get; set; }
        public string InterviewOutcome { get; set; }
        public string CountryID { get; set; }
    }
}