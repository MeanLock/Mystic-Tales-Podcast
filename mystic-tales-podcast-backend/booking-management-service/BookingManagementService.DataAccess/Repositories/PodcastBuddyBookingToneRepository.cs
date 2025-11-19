using BookingManagementService.DataAccess.Data;
using BookingManagementService.DataAccess.Entities.SqlServer;
using BookingManagementService.DataAccess.Repositories.interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BookingManagementService.DataAccess.Repositories
{
    public class PodcastBuddyBookingToneRepository : IPodcastBuddyBookingToneRepository
    {
        private readonly AppDbContext _context;
        public PodcastBuddyBookingToneRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> DeletePodcastBuddyBookingTone(List<PodcastBuddyBookingTone> existingBookingPodcastTones)
        {
            try
            {
                _context.PodcastBuddyBookingTones.RemoveRange(existingBookingPodcastTones);
                var rowsAffected = await _context.SaveChangesAsync();
                return rowsAffected > 0;    
            }
            catch (Exception ex)
            {
                throw new HttpRequestException("Delete PodcastBuddyBookingTones failed, error: " + ex.Message);
            }
        }
    }
}
