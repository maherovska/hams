using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public string UserId { get; set; }
        public int TaskId { get; set; }
        public string Content { get; set; }
        public string Date { get; set; }
        public string UserFullName { get; set; }
    }
}