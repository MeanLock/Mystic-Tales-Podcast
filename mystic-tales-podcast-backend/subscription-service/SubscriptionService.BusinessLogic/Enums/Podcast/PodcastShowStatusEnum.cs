using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubscriptionService.BusinessLogic.Enums.Podcast
{
    public enum PodcastShowStatusEnum
    {
        Draft = 1,
        ReadyToRelease = 2,
        Published = 3,
        TakenDown = 4,
        Removed = 5
    }
}
