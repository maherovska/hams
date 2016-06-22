using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HAMS.WebUI.Services.Abstract
{
    public interface IAccountService
    {
        string GetCurrentUserId();
        bool CurrentUserIsInRole(string role);
    }
}
