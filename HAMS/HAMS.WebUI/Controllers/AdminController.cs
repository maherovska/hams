using HAMS.Domain;
using HAMS.Repository.Concrete;
using HAMS.WebUI.Helpers;
using HAMS.WebUI.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using System.Data.Entity;
using PagedList;
using HAMS.Repository.Abstract;

namespace HAMS.WebUI.Controllers
{
    //[RequireHttps]
    [Authorize(Roles = "Admin")]
    public class AdminController : ApplicationBaseController
    {
        HAMS_DB_AspNetEntities _db = new HAMS_DB_AspNetEntities();
        ApplicationDbContext context = new ApplicationDbContext();
        private IUnitOfWork _unitOfWork = new UnitOfWork();
       // private AccountController account;

        public AdminController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public ApplicationUser ChangeRoleForUser(string userId, string oldRole, string newRole)
        {
            //var account = new AccountController();
            ApplicationUser user = context.Users
                                          .Where(u => u.Id == userId)
                                          .FirstOrDefault();

            var store = new UserStore<ApplicationUser>(context);
            var manager = new ApplicationUserManager(store);

            if (manager.IsInRole(user.Id, oldRole))
            {
                string newRoleId;
                if ((newRoleId = context.Roles.Where(r => r.Name == newRole).Select(r => r.Id).FirstOrDefault()) != null)
                {
                    manager.RemoveFromRole(user.Id, oldRole);
                    manager.AddToRole(user.Id, newRole);
                    user.RoleId = newRoleId;
                    context.SaveChanges();
                }
            }

            return user;
        }


        // GET: Admin
        //public ActionResult Index()
        //{     
        //    return View();
        //}

        public ActionResult ApproveRegistration()
        {
            IEnumerable<StudentViewModel> students = _db.StudentsGroup
                                                        .Where(sg => sg.AspNetUsers.RoleId == sg.AspNetUsers.AspNetRoles.Where(r => r.Name == "Candidate").FirstOrDefault().Id)
                                                        .Select(user => new StudentViewModel
                                                        {
                                                            Id = user.AspNetUsers.Id,
                                                            Role = "Candidate",
                                                            FirstName = user.AspNetUsers.FirstName,
                                                            LastName = user.AspNetUsers.LastName,
                                                            Email = user.AspNetUsers.Email,
                                                            Specialization = user.Group.Specialization,
                                                            DepartmentNumber = user.Group.DepartmentNumber,
                                                            EnterYear = user.Group.EnterYear
                                                        })
                                                        .ToList();            
            return View(students);
        }

        [HttpPost]
        public async Task<ActionResult> ApproveRegistration(string studentId, string studentRole)
        {
            ApplicationUser user = ChangeRoleForUser(studentId, studentRole, "Student");

            await SendEmail(fullName: string.Format("{0} {1}", user.FirstName, user.LastName),
                          email: user.Email,
                          template: "ApprovedEmailTemplate",
                          emailTitle: "Student registration");

            return RedirectToAction("ApproveRegistration");
        }

        [HttpPost]
        public async Task<ActionResult> DisapproveRegistration(string studentId)
        {
            // send reason in email 
            // remove that student from table StudentsGroup and maybe from AspNetUsers
            // rewrite view ApproveRegistration.cshtml
            var user = _unitOfWork.AspNetUserRepository.GetByID(studentId);

            int recordId = _unitOfWork.StudentsGroupRepository.Get()
                                                              .Where(g => g.AspNetUsers.Id == studentId)
                                                              .Select(g => g.ID)
                                                              .FirstOrDefault();

            _unitOfWork.StudentsGroupRepository.Delete(recordId);
            _unitOfWork.AspNetUserRepository.Delete(studentId);
            _unitOfWork.Save();

            await SendEmail(fullName: string.Format("{0} {1}", user.FirstName, user.LastName),
                          email: user.Email,
                          template: "DisapprovedEmailTemplate",
                          emailTitle: "Student registration");

            return RedirectToAction("ApproveRegistration");
        }
  
        public ActionResult GetAllUsers()
        {            
            return View();            
        }  
        
        public async Task<PartialViewResult> GetUsersByRole(string role = "All", string status = "All", int pageNumber = 1, int pageSize = 10)
        {
            using (HAMS_DB_AspNetEntities db = new HAMS_DB_AspNetEntities())
            {
                //var store = new UserStore<ApplicationUser>(context);
                //var manager = new ApplicationUserManager(store);


                var testUsers = await (db.AspNetUsers
                                    
                                    .Select(u => new StudentViewModel
                                    {
                                        Id = u.Id,
                                        FirstName = u.FirstName,
                                        LastName = u.LastName,
                                        Email = u.Email,
                                        Role = u.AspNetRoles.Where(r => r.Id == u.RoleId).FirstOrDefault().Name,
                                        IsActive = u.IsActive
                                    })
                                    
                                    .OrderBy(u => u.Role)
                                    .ThenBy(u => u.LastName)
                                    .ThenBy(u => u.FirstName)
                                    
                                    .ToListAsync());

                if (status == "Active")
                {
                    testUsers = testUsers.Where(u => u.IsActive == true).ToList();
                }
                else if (status == "Inactive")
                {
                    testUsers = testUsers.Where(u => u.IsActive == false).ToList();
                }

                if (role == "All")
                {
                    testUsers = testUsers.Where(u => u.Role == "Teacher" || u.Role == "Student")
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }
                else
                {
                    testUsers = testUsers.Where(u => u.Role == role)
                        .Skip((pageNumber - 1) * pageSize)
                        .Take(pageSize)
                        .ToList();
                }

                return PartialView(testUsers);
            }
        }

        public ActionResult UserInfo(string userId)
        {
            using (HAMS_DB_AspNetEntities db = new HAMS_DB_AspNetEntities())
            {
                var user = db.AspNetUsers.Where(u => u.Id == userId)
                        .Select(u => new StudentViewModel
                        {
                            Id = u.Id,
                            Role = u.AspNetRoles.Where(r => r.Id == u.RoleId).FirstOrDefault().Name,
                            FirstName = u.FirstName,
                            LastName = u.LastName,
                            Email = u.Email,
                            IsActive = u.IsActive
                        })
                        .FirstOrDefault();

                if (user.Role == "Student")
                {
                    Group studentGroup = db.StudentsGroup.Where(g => g.Student_ID == user.Id)
                                    .FirstOrDefault()
                                    .Group;
                    user.Specialization = studentGroup.Specialization;

                    var groups = db.Group
                                .ToList()
                                .OrderByDescending(g => g.EnterYear);

                    
                    ViewBag.Department = new SelectList(groups.Where(g => g.Specialization == studentGroup.Specialization).Select(g => g.Department).Distinct(), studentGroup.Department);
                    ViewBag.EnterYear = new SelectList(groups.Select(g => g.EnterYear).Distinct(), studentGroup.EnterYear);

                }
                

                return View(user);
            }           
        }

        [HttpPost]
        public ActionResult UserInfo(StudentViewModel userModel)
        {
            using (HAMS_DB_AspNetEntities db = new HAMS_DB_AspNetEntities())
            {
                var user = db.AspNetUsers.Where(u => u.Id == userModel.Id).FirstOrDefault();
                user.IsActive = userModel.IsActive;

                if (userModel.Role == "Student")
                {
                    var groupId = db.Group.Where(g => g.Specialization == userModel.Specialization &&
                                                    g.Department == userModel.Department &&
                                                    g.EnterYear == userModel.EnterYear)
                                         .FirstOrDefault().ID;
                    user.StudentsGroup.Where(g => g.Student_ID == user.Id).FirstOrDefault().Group_ID = groupId;
                }

                db.SaveChanges();

                return RedirectToAction("UserInfo", new { userId = userModel.Id });
            }
            
        }        

        public ActionResult RegisterTeacher()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RegisterTeacher(RegisterTeacherViewModel model)
        {
            if (ModelState.IsValid)
            { 
                string roleId = _unitOfWork.AspNetRoleRepository.Get()
                                                                .Where(r => r.Name == "Teacher")//"Student")
                                                                .Select(r => r.Id)
                                                                .FirstOrDefault();                
                string defaultPassword = "Qwerty1@";

                var store = new UserStore<ApplicationUser>(context);
                var manager = new ApplicationUserManager(store);

                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    IsActive = true,
                    RoleId = roleId
                };                

                await manager.CreateAsync(user, defaultPassword);
                await manager.AddToRoleAsync(user.Id, "Teacher");                

                await SendEmail(fullName: string.Format("{0} {1}", user.FirstName, user.LastName),
                          email: user.Email,
                          template: "RegisterTeacherEmailTemplate",
                          emailTitle: "Teacher registration");

                return RedirectToAction("ApproveRegistration", "Admin");

            }            
            return View(model);
        }

        public ActionResult AddNewGroup()
        {
            using (var uow = new UnitOfWork())
            {
                var groups = uow.GroupRepository.Get();
                ViewBag.Specialization = groups.Select(g => g.Specialization).Distinct();
                ViewBag.Department = null;
            }   

            return View();
        }

        [HttpPost]
        public ActionResult AddNewGroup(Group group)
        {
            if (ModelState.IsValid) 
            {
                using (UnitOfWork uow = new UnitOfWork())
                {
                    var departmentNumber = uow.GroupRepository.Get(filter: g => g.Department == group.Department).Select(g => g.DepartmentNumber).Distinct().FirstOrDefault();

                    var newGroup = new Group
                    {
                        Specialization = group.Specialization,
                        Department = group.Department,
                        DepartmentNumber = departmentNumber,
                        EnterYear = group.EnterYear
                    };

                    Group groupToCheck = uow.GroupRepository.Get(filter: g => g.Specialization == newGroup.Specialization &&
                                                             g.Department == newGroup.Department &&
                                                             g.DepartmentNumber == newGroup.DepartmentNumber &&
                                                             g.EnterYear == newGroup.EnterYear)
                                                             .FirstOrDefault();

                    if (groupToCheck == null)
                    {
                        uow.GroupRepository.Insert(newGroup);
                        uow.Save();

                        return RedirectToAction("AddNewGroup");
                    }
                    else
                    {                      

                        var groups = uow.GroupRepository.Get();
                        ViewBag.Specialization = groups.Select(g => g.Specialization).Distinct();

                        if (group.Specialization != null)
                        {
                            ViewBag.Department = uow.GroupRepository.Get()
                                                       .Where(g => g.Specialization == group.Specialization)
                                                       .Select(g => g.Department)
                                                       .Distinct();

                            ViewBag.SelectedDepartment = group.Department;
                            ViewBag.ErrorMessage = "This group already exists";
                        }

                        return View(group);
                    }


                }
            }

            using (var uow = new UnitOfWork())
            {
                var groups = uow.GroupRepository.Get();
                ViewBag.Specialization = groups.Select(g => g.Specialization).Distinct();

                if (group.Specialization != null)
                {
                    ViewBag.Department = uow.GroupRepository.Get()
                                               .Where(g => g.Specialization == group.Specialization)
                                               .Select(g => g.Department)
                                               .Distinct();

                    ViewBag.SelectedDepartment = group.Department;
                    ViewBag.ErrorMessage = null;
                }
            }            

            return View(group);
        }

        [AllowAnonymous]
        public ActionResult GetDepartmentsBySpecialization(string specialization)
        {
            using (var uow = new UnitOfWork())
            {
                var departments =  uow.GroupRepository.Get()
                                               .Where(g => g.Specialization == specialization)                                               
                                               .Select(g => g.Department)
                                               .Distinct();
                return Json(departments, JsonRequestBehavior.AllowGet);
            }           
        }


        public async System.Threading.Tasks.Task SendEmail(string fullName, string email, string template, string emailTitle)       
        {
            var message = await EmailTemplate(template);
            message = message.Replace("UserFullName", CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fullName));
            message = message.Replace("UserEmail", email);
            await MessageServices.SendEmailAsync(email, emailTitle, message);
        }

        public static async Task<string> EmailTemplate(string template)
        {
            var templateFilePath = HostingEnvironment.MapPath("~/Content/Templates/") + template + ".cshtml";
            StreamReader objStreamReaderFile = new StreamReader(templateFilePath);
            var body = await objStreamReaderFile.ReadToEndAsync();
            objStreamReaderFile.Close();

            return body;
        }        
    }
}