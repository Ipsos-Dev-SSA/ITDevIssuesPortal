using System;

namespace CallCentreFollowUps.Controllers
{
    internal class IssueHistory
    {
        public int IssueId { get; set; }
        public object StatusId { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangeDate { get; set; }
        public string Comments { get; set; }
    }
}