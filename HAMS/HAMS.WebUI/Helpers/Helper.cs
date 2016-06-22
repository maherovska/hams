using HAMS.Repository.Concrete;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace HAMS.WebUI.Helpers
{
    public static class Helper
    {
        public static string GetGroupAbbr(string specialisation, int departmentNumber, int enterYear)
        {
            int year = DateTime.Now.Month >= 9 ? DateTime.Now.Year + 1 : DateTime.Now.Year;
            year -= enterYear;

            string abbr = string.Format("{0} - {1}{2}", specialisation, year, departmentNumber);
            return abbr;
        }

        public static string GetUserRole(string userId)
        {
            using (UnitOfWork unitOfWork = new UnitOfWork())
            {
                string roleId = unitOfWork.AspNetUserRepository.Get()
                                                    .Where(u => u.Id == userId)
                                                    .Select(u => u.RoleId)
                                                    .SingleOrDefault();
                
                string roleName = unitOfWork.AspNetRoleRepository.Get()
                                                              .Where(r => r.Id == roleId)
                                                              .Select(r => r.Name)
                                                              .SingleOrDefault();
                return roleName;
            }


                //string userId = HttpContext.Current.User.Identity.GetUserId();

                

            
        }        
    }
}