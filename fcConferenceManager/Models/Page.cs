using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace fcConferenceManager.Models
{
    public class page
    {
        

        [Display(Name = "Page #")]
        public int Id { get; set; }

        [Display(Name = "Page Title")]
        public string Title { get; set; }

        [Display(Name = "New Page Title")]
        public string newTitle { get; set; }

        [Display(Name = "Link Name")]
        public string LinkName { get; set; }

        [Display(Name = "Section")]
        public string Section { get; set; }

        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; }
        [Display(Name = "Event")]
        public string EventName { get; set; }
        [Display(Name = "URL")]
        public string URL { get; set; }

        [Display(Name = "New URL")]
        public string newURL { get; set; }

        [Display(Name = "Access From")]
        public string AccessFrom { get; set; }
        [Display(Name = "Access To")]
        public string AccessTo { get; set; }
        [Display(Name = "Notes")]
        public string Notes { get; set; }

        [Display(Name = "Link to Documentation")]
        public string LinkDocumentation { get; set; }
    }
}