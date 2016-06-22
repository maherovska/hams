using HAMS.Domain;
using HAMS.Repository.Abstract;
using HAMS.WebUI.Controllers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HAMS.UnitTests
{
    [TestClass]
    public class AccountControllerTest
    {
        private List<Group> _groups;
        private Mock<IUnitOfWork> _unitOfWorkMock;

        [TestInitialize]
        public void Initialize()
        {
            _unitOfWorkMock = new Mock<IUnitOfWork>();
            InitializeMockObj();
        }

        [TestMethod]
        public void CanRegister()
        {
            // Arrange  
            _unitOfWorkMock.Setup(m => m.GroupRepository.Get(null, null, "")).Returns(_groups);
            AccountController controller = new AccountController(_unitOfWorkMock.Object);

            // Act           
            var result = controller.Register() as ViewResult;

            // Assert
            Assert.IsNotNull(result.ViewBag.SpecializationList);
            Assert.IsNotNull(result.ViewBag.DepartmentList);
            Assert.IsNotNull(result.ViewBag.EnterYearList);

            Assert.AreEqual(5, ((List<string>)result.ViewBag.SpecializationList).Count());
            Assert.AreEqual(5, ((List<string>)result.ViewBag.DepartmentList).Count());
            Assert.AreEqual(10, ((List<int>)result.ViewBag.EnterYearList).Count());
        }

        private void InitializeMockObj()
        {
            _groups = new List<Group>();
            for (int i = 1; i <= 10; i++)
            {
                _groups.Add(new Group()
                {
                    Specialization = $"Specialization{i % 5}",
                    Department = $"Department{i % 5}",
                    EnterYear = 2010 + i
                });
            }
        }
    }
}
