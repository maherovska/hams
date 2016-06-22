using HAMS.WebUI.Services.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HAMS.WebUI.Models;
using HAMS.Domain;
using HAMS.WebUI.Helpers;

namespace HAMS.WebUI.Services.Concrete
{
    public class TaskService : ITaskService
    {
        private HAMS_DB_AspNetEntities _context;

        public TaskService()
        {           
            _context = new HAMS_DB_AspNetEntities();
        }

        public IEnumerable<TaskViewModel> GetAdminTasks()
        {
            IEnumerable<TaskViewModel> tasks = _context.TaskDetail
                                     .ToList()
                                     .Where(t => t.Subject.AspNetUsers.IsActive)
                                     .Select(task => new TaskViewModel
                                     {
                                         TaskId = task.ID,
                                         Title = task.Task.Title,
                                         Description = task.Task.Description,
                                         DeadLine = DateTimeToUnixTimestamp(task.DeadLine),
                                         Group = Helper.GetGroupAbbr(task.Group.Specialization, task.Group.DepartmentNumber, task.Group.EnterYear),
                                         Subject = task.Subject.Name,
                                         TeacherFullName = string.Format("{0} {1}", task.Subject.AspNetUsers.FirstName, task.Subject.AspNetUsers.LastName)
                                     });
            return tasks;
        }

        public IEnumerable<TaskViewModel> GetStudentTasks(string userId)
        {
            IEnumerable<TaskViewModel> tasks = _context.TaskDetail
                            .Where(t => t.Group_ID == t.Group.StudentsGroup.Where(g => g.Student_ID == userId).Select(g => g.Group_ID).FirstOrDefault()
                                    && t.Subject.AspNetUsers.IsActive)
                            .ToList()
                            .Select(task => new TaskViewModel
                            {
                                TaskId = task.Task_ID,
                                Title = task.Task.Title,
                                Description = task.Task.Description,
                                DeadLine = DateTimeToUnixTimestamp(task.DeadLine),
                                Subject = task.Subject.Name,
                                TeacherFullName = string.Format("{0} {1}", task.Subject.AspNetUsers.FirstName, task.Subject.AspNetUsers.LastName),
                                IsDone = task.Task.StudentsTask.Where(t => t.Student_ID == userId).Select(t => t.IsDone).FirstOrDefault() ?? false
                            });
            return tasks;
        }

        public IEnumerable<TaskViewModel> GetTeacherTasks(string userId)
        {
            IEnumerable<TaskViewModel> tasks = _context.TaskDetail
                                        .Where(t => t.Subject.Teacher_ID == userId)
                                        .ToList()
                                        .Select(task => new TaskViewModel
                                        {
                                            TaskId = task.Task_ID,
                                            Title = task.Task.Title,
                                            Description = task.Task.Description,
                                            DeadLine = DateTimeToUnixTimestamp(task.DeadLine),
                                            Group = Helper.GetGroupAbbr(task.Group.Specialization, task.Group.DepartmentNumber, task.Group.EnterYear),
                                            Subject = task.Subject.Name
                                        });
            return tasks;
        }


        private static string DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds.ToString();
        }
    }
}