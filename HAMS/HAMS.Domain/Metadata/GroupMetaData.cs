using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAMS.Domain
{
    public class GroupMetaData
    {
        [Required(ErrorMessage = "Choose Specialization.")]
        public string Specialization { get; set; }

        [Required(ErrorMessage = "Choose Department.")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Choose Enter year.")]
        [MyYear("Choose valid Enter year.")]
        public int EnterYear { get; set; }
    }

    public class MyYearAttribute : ValidationAttribute
    {
        public MyYearAttribute()
        {

        }

        public MyYearAttribute(string errorMessage)
        {
            ErrorMessage = errorMessage;
        }
        public override bool IsValid(object value)
        {
            int year = Convert.ToInt32(value);
            return year >= DateTime.Now.Year;
        }
    }
}
