using HAMS.Domain;
using HAMS.Repository.Concrete;
using HAMS.WebUI.Helpers;
using HAMS.WebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using HAMS.WebUI.Services.Concrete;
using HAMS.WebUI.Services.Abstract;
using HAMS.Repository.Abstract;

namespace HAMS.WebUI.Controllers
{
   // [RequireHttps]
    [Authorize]
    public class TaskController : ApplicationBaseController
    {
        private IUnitOfWork _unitOfWork;
        private IAccountService _accountService;
        private ITaskService _taskService;

        public TaskController(IUnitOfWork unitOfWork, IAccountService accountService, ITaskService taskService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _taskService = taskService;
        }

        // GET: Task
        public ActionResult Index()
        {
           return View();
        }

        public ActionResult TaskDetail()
        {
            return View();
        }

        //tasks - get (вернути всі таски для юзера, який зараз залогінений)
        public JsonResult GetTasks()
        {
            IEnumerable<TaskViewModel> tasks;
            string userId = _accountService.GetCurrentUserId();
            
            if (_accountService.CurrentUserIsInRole("Admin"))
            {                
                tasks = _taskService.GetAdminTasks();             

            }
            else if (_accountService.CurrentUserIsInRole("Teacher"))
            {                
                tasks = _taskService.GetTeacherTasks(userId);
            }
            else //if (accountService.CurrentUserIsInRole("Student"))
            {                
                tasks = _taskService.GetStudentTasks(userId);
            }

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        //- get(інфа про такс + коментарі) -> передаю тобі task_id, ти мені інфу про таск + коментарі цього таску.
        public JsonResult GetTaskDetail(int id)
        {
            var task = _unitOfWork.TaskDetailRepository.GetByID(id);
            IEnumerable<CommentViewModel> comments = GetCommentsByTaskId(id);                       

            TaskDetailViewModel taskDetail = new TaskDetailViewModel
            {
                TaskDetailId = id,
                TaskTitle = task.Task.Title,
                TaskDescription = task.Task.Description,
                Group = Helper.GetGroupAbbr(task.Group.Specialization, task.Group.DepartmentNumber, task.Group.EnterYear),
                Subject = task.Subject.Name,
                TeacherFullName = string.Format("{0} {1}", task.Subject.AspNetUsers.FirstName, task.Subject.AspNetUsers.LastName),
                DeadLine = DateTimeToUnixTimestamp(task.DeadLine),
                Comments = comments
            };

            return Json(taskDetail, JsonRequestBehavior.AllowGet);
        }


        //detail task:
        //- get (вернути всі коментарі до таску) -> передаю task_id тобі, ти мені всі його коменти.
        public JsonResult GetComments(int id)//(int taskId)
        {
            return Json(GetCommentsByTaskId(id), JsonRequestBehavior.AllowGet);
        }

        private IEnumerable<CommentViewModel> GetCommentsByTaskId(int id)
        {
           var comments = _unitOfWork.CommentRepository.Get()
                                                        .ToList()
                                                        .Where(c => c.Task_ID == id)
                                                        .Select(c => new CommentViewModel
                                                        {
                                                            CommentId = c.ID,
                                                            UserId = c.AspNetUsers.Id,
                                                            TaskId = id,
                                                            Content = c.Content,
                                                            Date = DateTimeToUnixTimestamp(c.Date),
                                                            UserFullName = string.Format("{0} {1}", c.AspNetUsers.FirstName, c.AspNetUsers.LastName)
                                                        });
            return comments;
        }

        [Authorize(Roles = "Teacher")]
        public JsonResult GetSubjectsByTeacher()
        {
            string teacherId = _accountService.GetCurrentUserId();

            var subjects = _unitOfWork.SubjectRepository.Get()
                                                        .Where(s => s.Teacher_ID == teacherId)
                                                        .Select(s => new
                                                        {
                                                            s.ID,
                                                            s.Name                                                            
                                                        })
                                                        .Distinct();
            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Teacher")]
        public JsonResult GetGroupsForTeacher()
        {
            var teacherId = _accountService.GetCurrentUserId();

            var groups = _unitOfWork.GroupRepository.Get()
                                                    .Where(g => g.ID == g.TeachersGroup.Where(tg => tg.Teacher_ID == teacherId).Select(tg => tg.Group_ID).FirstOrDefault())
                                                    .ToList()
                                                    .Select(group => new
                                                    {
                                                        GroupId = group.ID,
                                                        GroupName = Helper.GetGroupAbbr(group.Specialization, group.DepartmentNumber, group.EnterYear)
                                                    });
            return Json(groups, JsonRequestBehavior.AllowGet);
        }



        //detail task:
        //- post(запостити новий коментар) -> передаю інфу про коментар, ти його в базу записуєш.
        [HttpPost]
        public ActionResult Comment(CommentViewModel commentModel)
        {
            Comment comment = new Comment
            {
                Content = commentModel.Content,
                Date = DateTime.Now,
                User_ID = _accountService.GetCurrentUserId(),
                Task_ID = commentModel.TaskId
            };

            _unitOfWork.CommentRepository.Insert(comment);
            _unitOfWork.Save();

            return View("TaskDetail");
        }
       
        public ActionResult IsTeacher()
        {
            return Json(_accountService.CurrentUserIsInRole("Teacher"), JsonRequestBehavior.AllowGet);
        }

        public ActionResult IsStudent()
        {
            return Json(_accountService.CurrentUserIsInRole("Student"), JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult AddTask(TaskDetailViewModel taskModel)
        {            
            try
            {
                
                if (taskModel.TaskId > 0) //edit
                {
                    _unitOfWork.TaskRepository.Update(new Task
                    {
                        ID = taskModel.TaskId,
                        Title = taskModel.TaskTitle,
                        Description = taskModel.TaskDescription
                    });
                    var taskDetail = _unitOfWork.TaskDetailRepository.Get()
                                                        .Where(t => t.Task_ID == taskModel.TaskId)
                                                        .FirstOrDefault();
                    taskDetail.DeadLine = ConvertUnixTimeStamp(taskModel.DeadLine).AddHours(3);
                    _unitOfWork.Save();
                }
                else //add
                {
                    Task task = new Task
                    {
                        Title = taskModel.TaskTitle,
                        Description = taskModel.TaskDescription
                    };
                    _unitOfWork.TaskRepository.Insert(task);
                    _unitOfWork.Save();

                    TaskDetail taskDetail = new TaskDetail
                    {
                        Task_ID = task.ID,
                        Group_ID = taskModel.GroupId,
                        Subject_ID = taskModel.SubjectId,
                        DeadLine = ConvertUnixTimeStamp(taskModel.DeadLine).AddHours(3)
                    };
                    
                    _unitOfWork.TaskDetailRepository.Insert(taskDetail);
                    _unitOfWork.Save();                    
                }




                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }  
        }        

        [Authorize(Roles = "Teacher")]
        [HttpPost]
        public ActionResult DeleteTask(int taskId)
        {
            try
            {
                int taskDetailId = _unitOfWork.TaskDetailRepository.Get().Where(t => t.Task_ID == taskId).Select(t => t.ID).FirstOrDefault();
                int[] commentId = _unitOfWork.CommentRepository.Get().Where(c => c.Task_ID == taskId).Select(c => c.ID).ToArray();

                _unitOfWork.TaskDetailRepository.Delete(taskDetailId);
                foreach (var c in commentId)
                {
                    _unitOfWork.CommentRepository.Delete(c);
                }
                _unitOfWork.TaskRepository.Delete(taskId);                 
                _unitOfWork.Save();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
        }

        public ActionResult TaskDone(TaskDoneModel taskModel)
        {
            try
            {                
                var task = _unitOfWork.StudentsTaskRepository.Get()
                                                             .Where(t => t.Task_ID == taskModel.TaskId)
                                                             .FirstOrDefault();
                var studentId = _accountService.GetCurrentUserId();

                if (task == null) //add
                {
                    _unitOfWork.StudentsTaskRepository.Insert(new StudentsTask
                    {
                        Task_ID = taskModel.TaskId,
                        Student_ID = studentId,
                        IsDone = taskModel.IsDone
                    });
                }
                else //edit
                {
                    task.IsDone = taskModel.IsDone;
                }

                _unitOfWork.Save();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
        }





        private static string DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
                   new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalMilliseconds.ToString();
        }

        private static DateTime ConvertUnixTimeStamp(string unixTimeStamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0).AddMilliseconds(Convert.ToDouble(unixTimeStamp));
        }


    }
}