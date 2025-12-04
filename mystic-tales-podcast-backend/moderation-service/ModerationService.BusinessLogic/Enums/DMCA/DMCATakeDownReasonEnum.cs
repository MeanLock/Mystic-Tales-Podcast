using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCATakeDownReasonEnum
    {
        DuplicateContent = 1,
        RestrictedTermsViolation = 2,
        ExplicitOrAdultContent = 3,
        HateSpeech = 4,
        HarassmentAbuse = 5,
        PrivacyViolation = 6,
        Impersonation = 7,
        MisinformationFalseClaims = 8,
        PromotingIllegalActivity = 9,
        LawsuitDMCA = 10
    }
    public static class DMCATakeDownReasonEnumExtensions
    {
        public static string GetDescription(this DMCATakeDownReasonEnum reason)
        {
            return reason switch
            {
                DMCATakeDownReasonEnum.DuplicateContent => "Nội dung trùng lặp",
                DMCATakeDownReasonEnum.RestrictedTermsViolation => "Vi phạm điều khoản hạn chế",
                DMCATakeDownReasonEnum.ExplicitOrAdultContent => "Nội dung khiêu dâm hoặc người lớn",
                DMCATakeDownReasonEnum.HateSpeech => "Ngôn ngữ thù địch",
                DMCATakeDownReasonEnum.HarassmentAbuse => "Quấy rối hoặc lạm dụng",
                DMCATakeDownReasonEnum.PrivacyViolation => "Vi phạm quyền riêng tư",
                DMCATakeDownReasonEnum.Impersonation => "Mạo danh",
                DMCATakeDownReasonEnum.MisinformationFalseClaims => "Thông tin sai lệch hoặc tuyên bố sai",
                DMCATakeDownReasonEnum.PromotingIllegalActivity => "Khuyến khích hoạt động bất hợp pháp",
                DMCATakeDownReasonEnum.LawsuitDMCA => "Vụ kiện DMCA",
                _ => "Lý do không xác định",
            };
        }
    }
}
