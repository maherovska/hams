using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class SubjectViewModel
    {
        public int SubjectId { get; set; }

        [Required(ErrorMessage = "Subject name is required", AllowEmptyStrings = false)]
        [Display(Name = "Subject")]
        public string SubjectName { get; set; }

        public string TeacherId { get; set; }
    }
}