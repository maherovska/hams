using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class TaskViewModel
    {
        public int TaskId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DeadLine { get; set; }

        public string Group { get; set; }
        public string Subject { get; set; }
        public string TeacherFullName { get; set; }

        public bool IsDone { get; set; }
    }
}