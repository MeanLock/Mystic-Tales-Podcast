using BookingManagementService.BusinessLogic.Enums.ListenSessionProcedure;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.DTOs.Booking
{
    public class BookingTrackListenRequestDTO
    {
        [Required]
        [EnumDataType(typeof(CustomerListenSessionProcedureSourceDetailTypeEnum))]
        public required CustomerListenSessionProcedureSourceDetailTypeEnum SourceType { get; set; }
    }
}
