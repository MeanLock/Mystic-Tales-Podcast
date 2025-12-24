using BookingManagementService.DataAccess.Entities.SqlServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingManagementService.DataAccess.Repositories.interfaces
{
    public interface IPodcastBuddyBookingToneRepository
    {
        Task<bool> DeletePodcastBuddyBookingTone(List<PodcastBuddyBookingTone> existingBookingPodcastTones);
    }
}
