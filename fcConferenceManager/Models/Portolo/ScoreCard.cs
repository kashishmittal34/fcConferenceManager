using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using Elimar;

namespace Elimar.Models
{
    public class UsersquestionResponse
    {
        public int pkey { get; set; }

        public string questions { get; set; }

        public int? ratingscore { get; set; }
    }
   

    public class UserRatingResponse
    {
        public int questions { get; set; }

        public int ratingscore { get; set; }
    }
}