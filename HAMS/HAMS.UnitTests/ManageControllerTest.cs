using HAMS.Domain;
using HAMS.Repository.Abstract;
using HAMS.WebUI.Controllers;
using HAMS.WebUI.Models;
using HAMS.WebUI.Services.Abstract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HAMS.UnitTests
{
    [TestClass]
    public class ManageControllerTest
    {
        private const string UserFakeId = "userFakeId";

        private Mock<IUnitOfWork> _unitOfWorkMock;
        private Mock<IAccountService> _accountMock;

        private List<Subject> _subjects;
        private List<TeachersGroup> _teacherGroups;
        private List<Group> _groups;
        private SubjectViewModel _subject;

        [TestInitialize]
        public void Initialize()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            _accountMock = new Mock<IAccountService>();

            _accountMock.Setup(m => m.GetCurrentUserId()).Returns(UserFakeId);
            InitializeMockObj();
        }

        [TestMethod]
        public void CanGetSubjects()
        {
            // Arrange  
            _unitOfWorkMock.Setup(m => m.SubjectRepository.Get(null, null, "")).Returns(_subjects);
            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.GetSubjects() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0 && result.Count < 10);
        }

        [TestMethod]
        public void CanGetSubjectById()
        {
            // Arrange 
            var subjId = 3;
            _unitOfWorkMock.Setup(m => m.SubjectRepository.GetByID(subjId)).Returns(_subjects.Single(s => s.ID == subjId));
            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.GetSubject(subjId) as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<IDictionary<string, object>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("Name3", result["SubjectName"]);
        }

        [TestMethod]
        public void CanAddNewSubjectForTeacher()
        {
            // Arrange 
            var canAdd = false;
            _subject.SubjectId = -1;

            _unitOfWorkMock.Setup(m => m.SubjectRepository.Insert(It.IsAny<Subject>())).Callback(() => canAdd = true);
            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.SaveSubject(_subject) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(200, actual.StatusCode);
            Assert.IsTrue(canAdd);            
        }

        [TestMethod]
        public void CanEditSubjectForTeacher()
        {
            // Arrange 
            var canEdit = false;
            _subject.SubjectId = 1;

            _unitOfWorkMock.Setup(m => m.SubjectRepository.GetByID(It.IsAny<int>())).Returns(new Subject() { Name = "Name" }).Callback(() => { canEdit = true; });
            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.SaveSubject(_subject) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(200, actual.StatusCode);
            Assert.IsTrue(canEdit);
        }

        [TestMethod]
        public void CanDeleteSubjectForTeacher()
        {
            // Arrange 
            var canDelete = false;

            _unitOfWorkMock.Setup(m => m.SubjectRepository.Delete(It.IsAny<int>())).Callback(() => canDelete = true);
            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.DeleteSubject(_subject) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(200, actual.StatusCode);
            Assert.IsTrue(canDelete);
        }

        [TestMethod]
        public void CanGetAvailableGroups()
        {
            // Arrange  
            _unitOfWorkMock.Setup(m => m.TeachersGroupRepository.Get(null, null, "")).Returns(_teacherGroups);
            _unitOfWorkMock.Setup(m => m.GroupRepository.Get(null, null, "")).Returns(_groups);

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.GetAvailableGroups() as JsonResult;
            var result = JsonObjectRepresentor.GetJsonObjectRepresentation<List<IDictionary<string, object>>>(actual);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 6);
        }

        [TestMethod]
        public void CanAddNewGroupForTeacher()
        {
            // Arrange 
            var canAdd = false;
            var group = new GroupViewModel() { GroupId = -1 };

            _unitOfWorkMock.Setup(m => m.TeachersGroupRepository.Get(null, null, "")).Returns(_teacherGroups);
            _unitOfWorkMock.Setup(m => m.TeachersGroupRepository.Insert(It.IsAny<TeachersGroup>())).Callback(() => canAdd = true);
            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.SaveGroup(group) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(200, actual.StatusCode);
            Assert.IsTrue(canAdd);
        }

        [TestMethod]
        public void CanDeleteGroupForTeacher()
        {
            // Arrange 
            var canDelete = false;
            var group = new GroupViewModel() { GroupId = 0 };

            _unitOfWorkMock.Setup(m => m.TeachersGroupRepository.Get(null, null, "")).Returns(_teacherGroups);
            _unitOfWorkMock.Setup(m => m.SubjectRepository.Delete(It.IsAny<int>())).Callback(() => canDelete = true);
            _unitOfWorkMock.Setup(m => m.Save()).Callback(() => { });

            ManageController controller = new ManageController(_unitOfWorkMock.Object, _accountMock.Object);

            // Act           
            var actual = controller.SaveGroup(group) as HttpStatusCodeResult;

            // Assert
            Assert.AreEqual(200, actual.StatusCode);
            Assert.IsFalse(canDelete);
        }

        private void InitializeMockObj()
        {
            _subjects = new List<Subject>();
            for (int i = 0; i < 10; i++)
            {
                _subjects.Add(new Subject()
                {
                    ID = i,
                    Name = $"Name{i}",
                    Teacher_ID = i % 3 == 0 ? UserFakeId : $"userFakeId#{i}"
                });
            }

            _subject = new SubjectViewModel { SubjectName = "SubjectName" };

            _teacherGroups = new List<TeachersGroup>();
            for (int i = 0; i < 10; i++)
            {
                _teacherGroups.Add(new TeachersGroup()
                {
                    Teacher_ID = i % 3 == 0 ? UserFakeId : $"teacher id #{i}",
                    Group = new Group()
                    {
                        ID = i,
                        Specialization = "PMI",
                        DepartmentNumber = i % 3 + 1,
                        EnterYear = DateTime.Now.Year - 3 + i % 3
                    }
                });
            }

            _groups = new List<Group>();
            for (int i = 0; i < 10; i++)
            {
                _groups.Add(new Group()
                {
                    ID = i,
                    Specialization = "PMI",
                    DepartmentNumber = i % 3 + 1,
                    EnterYear = DateTime.Now.Year - 3 + i % 3
                });
            }                        
        }
    }
}
