using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace fcConferenceManager.Models
{
    public class EventPlanData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Brand { get; set; }
        public string TargetAudience { get; set; }
        public string Unit { get; set; }
        public string Producer { get; set; }
        public string Location { get; set; }
        public string Venue { get; set; }
        public string Format { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Plan_Participants { get; set; }
        public string Actual_Participants { get; set; }
        public string Pricing { get; set; }
        public string Plan_Revenue { get; set; }
        public string Actual_Revenue { get; set; }
        public string Plan_Cost { get; set; }
        public string Actual_Cost { get; set; }
        public string Plan_Cost_PerPoint { get; set; }
        public string Actual_Cost_PerPoint { get; set; }
        public string Plan_Profit { get; set; }
        public string Actual_Profit { get; set; }
        public string Plan_Total { get; set; }
        public string Actual_Total { get; set; }
        public string Plan_Follow_Up { get; set; }
        public string Actual_Follow_Up { get; set; }
        public string Plan_Engagement { get; set; }
        public string Actual_Engagement { get; set; }
        public string Plan_Affinity { get; set; }
        public string Actual_Affinity { get; set; }
        public string Plan_Visibility { get; set; }
        public string Actual_Visibility { get; set; }
        public string Comments { get; set; }
        public string Priority { get; set; }
        public string PointsPlanned { get; set; }
        public string PointsEarned { get; set; }
    }
}