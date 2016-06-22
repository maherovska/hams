using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class GroupViewModel
    {
        public int GroupId { get; set; }
        public string GroupName { get; set; }

        [Required(ErrorMessage = "Choose Specialization.")]
        public string Specialization { get; set; }
        [Required(ErrorMessage = "Choose Department.")]
        public string Department { get; set; }
        [Required(ErrorMessage = "Choose Enter year.")]
        public int EnterYear { get; set; }
    }
}