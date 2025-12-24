using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.BusinessLogic.Services.DbServices.BookingServices
{
    public class ChatRoomService
    {
        private readonly IGenericRepository<BookingChatRoom> _bookingChatRoomGenericRepository;
        private readonly IGenericRepository<BookingChatMessage> _bookingChatMessageGenericRepository;
        private readonly IGenericRepository<BookingChatMember> _bookingChatMemberGenericRepository;
        private readonly IGenericRepository<Booking> _bookingGenericRepository;
        public ChatRoomService(
            IGenericRepository<BookingChatRoom> bookingChatRoomGenericRepository,
            IGenericRepository<BookingChatMessage> bookingChatMessageGenericRepository,
            IGenericRepository<BookingChatMember> bookingChatMemberGenericRepository,
            IGenericRepository<Booking> bookingGenericRepository)
        {
            _bookingChatRoomGenericRepository = bookingChatRoomGenericRepository;
            _bookingChatMessageGenericRepository = bookingChatMessageGenericRepository;
            _bookingChatMemberGenericRepository = bookingChatMemberGenericRepository;
            _bookingGenericRepository = bookingGenericRepository;
        }
        public async Task<BookingChatRoom?> GetChatRoomByBookingIdAsync(int bookingId)
        {
            var chatRoom = _bookingChatRoomGenericRepository.FindAll().Where(br => br.BookingId == bookingId).FirstOrDefault();
            return chatRoom;
        }
        public async Task<List<BookingChatMessage>> GetAllChatRoomByBookingIdAsync(int bookingId)
        {
            return _bookingChatMessageGenericRepository.FindAll()
                .Include(cm => cm.ChatRoom)
                .Where(cm => cm.ChatRoom.BookingId == bookingId).ToList();
        }

    }
}
