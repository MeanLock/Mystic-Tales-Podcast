using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PodcastService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain.LoginAccountGoogle
{
    public class LoginAccountGoogleParameterDTO
    {
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
    }
}
