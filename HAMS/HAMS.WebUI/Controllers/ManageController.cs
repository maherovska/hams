using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using HAMS.WebUI.Models;
using HAMS.Repository.Concrete;
using HAMS.Domain;
using System.Net;
using HAMS.WebUI.Helpers;
using System.Data.SqlClient;
using System.Collections.Generic;
using HAMS.WebUI.Services.Abstract;
using HAMS.WebUI.Services.Concrete;
using HAMS.Repository.Abstract;

namespace HAMS.WebUI.Controllers
{
   // [RequireHttps]
    [Authorize]
    public class ManageController : ApplicationBaseController
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private IUnitOfWork _unitOfWork;
        private IAccountService _accountService;

        public ManageController(IUnitOfWork unitOfWork, IAccountService accountService)
        {
            _unitOfWork = unitOfWork;
            _accountService = accountService;
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult UploadPhoto(HttpPostedFileBase file)
        {
            var db = HttpContext.GetOwinContext().Get<ApplicationDbContext>();

            // find the user. I am skipping validations and other checks.
            var userid = User.Identity.GetUserId();
            var user = db.Users.Where(x => x.Id == userid).FirstOrDefault();

            // convert image stream to byte array
            byte[] image = new byte[file.ContentLength];
            file.InputStream.Read(image, 0, Convert.ToInt32(file.ContentLength));

            user.ProfilePicture = image;

            // save changes to database
            db.SaveChanges();

            return RedirectToAction("Index", "Manage");
        }

        [Authorize(Roles = "Teacher")]
        public ActionResult SubjectManagement()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        public JsonResult GetSubjects()
        {
            string userId = _accountService.GetCurrentUserId();

            var subjects = _unitOfWork.SubjectRepository.Get()
                                                        .Where(s => s.Teacher_ID == userId)
                                                        .Select(s => new SubjectViewModel
                                                        {
                                                            SubjectId = s.ID,
                                                            SubjectName = s.Name,
                                                            TeacherId = s.Teacher_ID
                                                        });

            return Json(subjects, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Teacher")]
        public ActionResult GetSubject(int id)
        {            
            var s = _unitOfWork.SubjectRepository.GetByID(id);

            SubjectViewModel subject = new SubjectViewModel
            {
                SubjectId = id,
                SubjectName = s.Name,
                TeacherId = s.Teacher_ID
            };

            return Json(subject, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public ActionResult SaveSubject(SubjectViewModel subject)
        {            
            try
            {                
                if (subject.SubjectId > 0)
                {
                    var s = _unitOfWork.SubjectRepository.GetByID(subject.SubjectId);
                    s.Name = subject.SubjectName;
                }
                else
                {
                    string userId = _accountService.GetCurrentUserId();

                    _unitOfWork.SubjectRepository.Insert(new Subject
                    {
                        ID = -1,
                        Name = subject.SubjectName,
                        Teacher_ID = userId
                    });
                }
                _unitOfWork.Save();
                
                return new HttpStatusCodeResult(HttpStatusCode.OK);                
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }            
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public ActionResult DeleteSubject(SubjectViewModel subject)
        {
            try
            {   
                _unitOfWork.SubjectRepository.Delete(subject.SubjectId);
                _unitOfWork.Save();
                
                return new HttpStatusCodeResult(HttpStatusCode.OK);                
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
        }      


        [Authorize(Roles = "Teacher")]
        public ActionResult GroupManagement()
        {
            return View();
        }

        [Authorize(Roles = "Teacher")]
        public JsonResult GetGroups()
        {
            int toYear = DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year - 1;
            int fromYear = toYear - 4;
            string userId = _accountService.GetCurrentUserId();

            var groups = _unitOfWork.TeachersGroupRepository
                                         .Get()
                                         .Where(g => g.Teacher_ID == userId &&
                                                g.Group.EnterYear >= fromYear &&
                                                g.Group.EnterYear <= toYear)
                                         .ToList()
                                         .Select(g => new GroupViewModel
                                         {
                                             GroupId = g.Group.ID,
                                             GroupName = Helper.GetGroupAbbr(g.Group.Specialization, g.Group.DepartmentNumber, g.Group.EnterYear)
                                         })
                                         .OrderBy(g => g.GroupName);

            return Json(groups, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Teacher")]
        public JsonResult GetAvailableGroups()
        {
            int toYear = DateTime.Now.Month >= 9 ? DateTime.Now.Year : DateTime.Now.Year - 1;
            int fromYear = toYear - 4;
            string userId = _accountService.GetCurrentUserId();

            var groups = _unitOfWork.TeachersGroupRepository
                                         .Get()
                                         .Where(g => g.Teacher_ID == userId &&
                                                g.Group.EnterYear >= fromYear &&
                                                g.Group.EnterYear <= toYear)
                                         .ToList()
                                         .Select(g => new GroupViewModel
                                         {
                                             GroupId = g.Group.ID,
                                             GroupName = Helper.GetGroupAbbr(g.Group.Specialization, g.Group.DepartmentNumber, g.Group.EnterYear)
                                         })
                                         .OrderBy(g => g.GroupName);

            var availableGroups = _unitOfWork.GroupRepository
                                    .Get()
                                    .Where(g => g.EnterYear >= fromYear &&
                                                g.EnterYear <= toYear)
                                    .ToList()   
                                    .Select(gt => new GroupViewModel
                                    {
                                        GroupId = gt.ID,
                                        GroupName = Helper.GetGroupAbbr(gt.Specialization, gt.DepartmentNumber, gt.EnterYear)
                                    })
                                    .Except(groups, new GroupComparer())
                                    .OrderBy(g => g.GroupName);            

            return Json(availableGroups, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        public ActionResult SaveGroup(GroupViewModel group)
        {
            try
            {
                var userId = _accountService.GetCurrentUserId();
                var id = _unitOfWork.TeachersGroupRepository.Get()
                                    .Where(g => g.Group_ID == group.GroupId)
                                    .Where(g => g.Teacher_ID == userId)
                                    .Select(g => g.ID)
                                    .FirstOrDefault();
                //delete
                if (id > 0)
                {
                    _unitOfWork.TeachersGroupRepository.Delete(id);                    
                }
                else //add new
                {
                    _unitOfWork.TeachersGroupRepository.Insert(new TeachersGroup
                    {
                        ID = -1,
                        Group_ID = group.GroupId,
                        Teacher_ID = userId
                    });
                }
                _unitOfWork.Save();

                return new HttpStatusCodeResult(HttpStatusCode.OK);
            }
            catch (SqlException)
            {
                return new HttpStatusCodeResult(HttpStatusCode.NotFound);
            }
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess; 
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error"); 
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        #region Helpers
        class GroupComparer : IEqualityComparer<GroupViewModel>
        {
            public bool Equals(GroupViewModel x, GroupViewModel y)
            {
                if (Object.ReferenceEquals(x, y)) return true;

                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                    return false;

                return x.GroupId == y.GroupId && x.GroupName == y.GroupName;
            }

            public int GetHashCode(GroupViewModel product)
            {
                if (Object.ReferenceEquals(product, null)) return 0;

                int hashName = product.GroupName == null ? 0 : product.GroupName.GetHashCode();
                int hashID = product.GroupId.GetHashCode();

                return hashName ^ hashID;
            }
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}