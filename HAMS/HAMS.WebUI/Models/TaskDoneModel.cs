using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class TaskDoneModel
    {
        public int TaskId { get; set; }
        public bool? IsDone { get; set; }
    }
}