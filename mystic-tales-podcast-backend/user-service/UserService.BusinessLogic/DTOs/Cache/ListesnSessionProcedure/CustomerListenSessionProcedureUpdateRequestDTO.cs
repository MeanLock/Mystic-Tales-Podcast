using System.ComponentModel.DataAnnotations;
using UserService.BusinessLogic.Enums.ListenSessionProcedure;

namespace UserService.BusinessLogic.DTOs.Cache.ListesnSessionProcedure
{

    public class CustomerListenSessionProcedureUpdateRequestDTO
    {
        public required CustomerListenSessionProcedureUpdateInfoDTO CustomerListenSessionProcedureUpdateInfo { get; set; }
    }

    public class CustomerListenSessionProcedureUpdateInfoDTO
    {
        [Required]
        [EnumDataType(typeof(CustomerListenSessionProcedurePlayOrderModeEnum))]
        public required CustomerListenSessionProcedurePlayOrderModeEnum PlayOrderMode { get; set; }
        
        public required bool IsAutoPlay { get; set; }
    }
}