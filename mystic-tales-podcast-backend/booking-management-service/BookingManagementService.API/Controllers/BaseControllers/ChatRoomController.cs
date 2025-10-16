using Microsoft.AspNetCore.Mvc;
using BookingManagementService.API.Filters.ExceptionFilters;
using BookingManagementService.BusinessLogic.Models.CrossService;
using BookingManagementService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using Microsoft.AspNetCore.Authorization;
using BookingManagementService.BusinessLogic.Services.DbServices.BookingServices;

namespace BookingManagementService.API.Controllers.BaseControllers
{
    [Route("api/chat-rooms")]
    [ApiController]
    [TypeFilter(typeof(HttpExceptionFilter))]
    [Authorize(Policy = "OptionalAccess")]
    public class ChatRoomController : ControllerBase
    {
        private readonly GenericQueryService _genericQueryService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly ChatRoomService _chatRoomService;

        public ChatRoomController(GenericQueryService genericQueryService, HttpServiceQueryClient httpServiceQueryClient, ChatRoomService chatRoomService)
        {
            _genericQueryService = genericQueryService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _chatRoomService = chatRoomService;
        }

        [HttpGet("bookings/{BookingId}")]
        [Authorize(Policy = "Customer.BasicAccess")]
        public async Task<IActionResult> GetChatMessageByBookingId([FromRoute] int BookingId)
        {
            var chatMessage = await _chatRoomService.GetAllChatRoomByBookingIdAsync(BookingId);
            if (chatMessage == null || chatMessage.Count == 0)
            {
                return NotFound($"Chat messages for Booking ID {BookingId} not found.");
            }
            return Ok(chatMessage);
        }
    }
}
