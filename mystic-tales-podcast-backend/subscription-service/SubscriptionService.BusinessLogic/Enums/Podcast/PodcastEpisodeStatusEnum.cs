using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Enums.Podcast
{
    public enum PodcastEpisodeStatusEnum
    {
        Draft = 1,
        PendingReview = 2,
        PendingEditRequired = 3,
        ReadyToRelease = 4,
        Published = 5,
        TakenDown = 6,
        Removed = 7
    }
}
