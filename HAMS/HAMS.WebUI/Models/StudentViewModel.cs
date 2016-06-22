using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HAMS.WebUI.Models
{
    public class StudentViewModel
    {
        public string Id { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Specialization { get; set; }
        public int DepartmentNumber { get; set; }
        public string Department { get; set; }
        public int EnterYear { get; set; }
        public bool IsActive { get; set; }
        //public byte[] Avatar { get; set; }
    }
}