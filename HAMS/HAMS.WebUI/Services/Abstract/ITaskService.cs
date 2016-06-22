using HAMS.WebUI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAMS.WebUI.Services.Abstract
{
    public interface ITaskService
    {
        IEnumerable<TaskViewModel> GetAdminTasks();
        IEnumerable<TaskViewModel> GetTeacherTasks(string userId);
        IEnumerable<TaskViewModel> GetStudentTasks(string userId);
    }
}
