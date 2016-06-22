using HAMS.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class TaskDetailViewModel
    {
        public int TaskDetailId { get; set; }
        public int TaskId { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public string Group { get; set; }
        public int GroupId { get; set; }
        public string Subject { get; set; }
        public int SubjectId { get; set; }
        public string TeacherFullName { get; set; }
        public string DeadLine { get; set; }

        public IEnumerable<CommentViewModel> Comments { get; set; }
    }
}