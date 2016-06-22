using HAMS.Domain;
using HAMS.Repository.Abstract;
using HAMS.WebUI.Controllers;
using HAMS.WebUI.Models;
using HAMS.WebUI.Services.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rhino.Mocks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace HAMS.UnitTests
{
    [TestClass]
    public class TaskControllerTest
    {
        private const string _testUserId = "test user id";
        private List<TaskViewModel> _teacherTasks;
        private List<TaskViewModel> _studentTasks;
        private List<TaskViewModel> _adminTasks;
        private List<Comment> _comments;
        private List<TaskDetail> _taskDetail;
        private List<Subject> _subjects;

        private Mock<IAccountService> _accountMock;
        private Mock<ITaskService> _taskMock;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [TestInitialize]
        public void Initialize()
        {
            InitializeMockObj();

            _accountMock = new Mock<IAccountService>();
            _taskMock = new Mock<ITaskService>();
            _unitOfWorkMock = new Mock<IUnitOfWork>();

            _accountMock.Setup(m => m.GetCurrentUserId()).Returns(_testUserId);
        }
        

        [TestMethod]
        public void CanGetTasksForTeacher()
        {
            // Arrange   
            _taskMock.Setup(m => m.GetTeacherTasks(_testUserId)).Returns(_teacherTasks);
            _accountMock.Setup(m => m.CurrentUserIsInRole("Teacher")).Returns(true);
            TaskController controller = new TaskController(null, _accountMock.Object, _taskMock.Object);

            // Act           
            var actual = controller.GetTasks() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 10);

            Assert.IsTrue(result.All(r => r["Group"] != null));            
            Assert.IsTrue(result.All(r => r["TeacherFullName"] == null));
            Assert.IsTrue(result.All(r => (bool)r["IsDone"] == default(bool)));
            
        }

        [TestMethod]
        public void CanGetTasksForStudent()
        {
            // Arrange   
            _taskMock.Setup(m => m.GetStudentTasks(_testUserId)).Returns(_studentTasks);
            _accountMock.Setup(m => m.CurrentUserIsInRole("Student")).Returns(true);
            TaskController controller = new TaskController(null, _accountMock.Object, _taskMock.Object);

            // Act           
            var actual = controller.GetTasks() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 10);

            Assert.IsTrue(result.All(r => r["Group"] == null));
            Assert.IsTrue(result.Any(r => (bool)r["IsDone"] == true));
        }

        [TestMethod]
        public void CanGetTasksForAdmin()
        {
            // Arrange   
            _taskMock.Setup(m => m.GetAdminTasks()).Returns(_adminTasks);
            _accountMock.Setup(m => m.CurrentUserIsInRole("Admin")).Returns(true);
            TaskController controller = new TaskController(null, _accountMock.Object, _taskMock.Object);

            // Act           
            var actual = controller.GetTasks() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 10);

            Assert.IsTrue(result.All(r => r["TeacherFullName"] != null));
            Assert.IsTrue(result.All(r => r["Group"] != null));
            Assert.IsTrue(result.All(r => (bool)r["IsDone"] == false));
        }

        [TestMethod]
        public void CanGetComments()
        {
            // Arrange   
            _unitOfWorkMock.Setup(m => m.CommentRepository.Get(null, null, "")).Returns(_comments);
            TaskController controller = new TaskController(_unitOfWorkMock.Object, null, null);

            // Act           
            var actual = controller.GetComments(1) as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 5);
            Assert.AreEqual("CommentContent5", result[0]["Content"]);            
        }

        [TestMethod]
        public void CanGetTaskDetailByTaskId()
        {
            // Arrange  
            var taskId = 1; 
            _unitOfWorkMock.Setup(m => m.TaskDetailRepository.GetByID(taskId)).Returns(_taskDetail.Single(t => t.ID == taskId));
            _unitOfWorkMock.Setup(m => m.CommentRepository.Get(null, null, "")).Returns(_comments);

            TaskController controller = new TaskController(_unitOfWorkMock.Object, null, null);

            // Act           
            var actual = controller.GetTaskDetail(taskId) as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<IDictionary<string, object>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("TaskTitle1", result["TaskTitle"]);
            Assert.AreEqual("PMI - 32", result["Group"]);
            Assert.IsNotNull(result["Comments"]);
        }

        [TestMethod]
        public void CanGetSubjectsByTeacher()
        {
            // Arrange  
            _unitOfWorkMock.Setup(m => m.SubjectRepository.Get(null, null, "")).Returns(_subjects);
            _accountMock.Setup(m => m.GetCurrentUserId()).Returns("teacherId #1");

            TaskController controller = new TaskController(_unitOfWorkMock.Object, _accountMock.Object, null);

            // Act           
            var actual = controller.GetSubjectsByTeacher() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [TestMethod]
        public void CanPostComment()
        {
            // Arrange  
            CommentViewModel cvm = new CommentViewModel()
            {
                Content = "Content",
                TaskId = 1
            };

            Comment comResult = new Comment();

            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });
            _unitOfWorkMock.Setup(m => m.CommentRepository.Insert(It.IsAny<Comment>())).Callback(() => { comResult = It.IsAny<Comment>(); });

            TaskController controller = new TaskController(_unitOfWorkMock.Object, _accountMock.Object, null);

            // Act           
            var actual = controller.Comment(cvm) as ActionResult;
            
            // Assert
            Assert.IsNull(comResult);
        }



        private void InitializeMockObj()
        {
            _teacherTasks = new List<TaskViewModel>();
            for (int i = 1; i <= 10; i++)
            {
                _teacherTasks.Add(new TaskViewModel()
                {
                    TaskId = i,
                    Title = $"TitleTeacher{i}",
                    Description = $"Description{i}",
                    DeadLine = $"DeadLine{i}",
                    Group = $"Group{i}",
                    Subject = $"Subject{i}"
                });
            }



            _studentTasks = new List<TaskViewModel>();
            for (int i = 1; i <= 10; i++)
            {
                _studentTasks.Add(new TaskViewModel()
                {
                    TaskId = i,
                    Title = $"TitleStudent{i}",
                    Description = $"Description{i}",
                    DeadLine = $"DeadLine{i}",
                    Subject = $"Subject{i}",
                    TeacherFullName = $"TeacherFullName{i}",
                    IsDone = i % 2 == 0 ? false : true
                });
            }

            _adminTasks = new List<TaskViewModel>();
            for (int i = 1; i <= 10; i++)
            {
                _adminTasks.Add(new TaskViewModel()
                {
                    TaskId = i,
                    Title = $"TitleAdmin{i}",
                    Description = $"Description{i}",
                    DeadLine = $"DeadLine{i}",
                    Group = $"Group{i}",
                    Subject = $"Subject{i}",
                    TeacherFullName = $"TeacherFullName{i}"
                });
            }

            _comments = new List<Comment>();
            for (int i = 0; i < 10; i++)
            {
                _comments.Add(new Comment()
                {
                    ID = i,
                    Task_ID = i < 5 ? 0 : 1,
                    Content = $"CommentContent{i}",
                    Date = DateTime.Now,
                    AspNetUsers = new AspNetUsers()
                    {
                        Id = $"userId#{i}",
                        FirstName = $"FirstName{i}",
                        LastName = $"LastName{i}"
                    }
                });
            }

            _taskDetail = new List<TaskDetail>();
            for (int i = 0; i < 2; i++)
            {
                _taskDetail.Add(new TaskDetail()
                {
                    ID = i,
                    Task = new Domain.Task()
                    {
                        Title = $"TaskTitle{i}",
                        Description = $"TaskDescription{i}"
                    },
                    Group = new Group()
                    {
                        Specialization = "PMI",
                        DepartmentNumber = 2,
                        EnterYear = 2013
                    },
                    Subject = new Subject()
                    {
                        Name = $"SubjectName{i}",
                        AspNetUsers = new AspNetUsers()
                        {
                            FirstName = $"FirstName{i}",
                            LastName = $"LastName{i}"
                        }
                    },
                    DeadLine = DateTime.Now
                });
            }

            _subjects = new List<Subject>();
            for (int i = 0; i < 10; i++)
            {
                _subjects.Add(new Subject()
                {
                    ID = i,
                    Name = $"Name{i}",
                    Teacher_ID = $"teacherId #{i % 3}"
                });
            }

        }
    }
}
