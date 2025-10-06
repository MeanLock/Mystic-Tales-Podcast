using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaOrchestratorService.BusinessLogic.DTOs.MessageQueue.UserManagementDomain
{
    public class LoginAccountGoogleParameterDTO
    {
        public string AuthorizationCode { get; set; }
        public string RedirectUri { get; set; }
    }
}
