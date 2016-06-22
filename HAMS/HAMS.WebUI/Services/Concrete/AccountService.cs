using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using System.Web.Security;
using HAMS.WebUI.Services.Abstract;

namespace HAMS.WebUI.Services.Concrete
{
    public class AccountService : IAccountService
    {
        // gets id of current user
        public string GetCurrentUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        // checks wheather user is in specified role 
        public bool CurrentUserIsInRole(string role)
        {
            return HttpContext.Current.User.IsInRole(role);
        }

    }
}