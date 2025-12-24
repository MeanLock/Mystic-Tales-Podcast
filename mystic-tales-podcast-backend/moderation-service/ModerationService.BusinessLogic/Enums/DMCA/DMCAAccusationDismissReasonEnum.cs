using ModerationService.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ModerationService.BusinessLogic.Enums.DMCA
{
    public enum DMCAAccusationDismissReasonEnum
    {
        PodcasterDeactivated = 1,
        ChannelDeleted = 2,
        ShowDeleted = 3,
        EpisodeDeleted = 4,
        ChannelUnpublished = 5,
        ShowUnpublished = 6,
        EpisodeUnpublished = 7,
        ShowRemovedInDMCAAccusation = 8,
        EpisodeRemovedInDMCAAccusation = 9
    }

    public static class DMCAAccusationDismissReasonExtensions
    {
        public static string GetDescription(this DMCAAccusationDismissReasonEnum reason)
        {
            return reason switch
            {
                DMCAAccusationDismissReasonEnum.PodcasterDeactivated => "Content owner account has been deactivated",
                DMCAAccusationDismissReasonEnum.ChannelDeleted => "Podcast channel has been deleted",
                DMCAAccusationDismissReasonEnum.ShowDeleted => "Podcast show has been deleted",
                DMCAAccusationDismissReasonEnum.EpisodeDeleted => "Podcast episode has been deleted",
                DMCAAccusationDismissReasonEnum.ChannelUnpublished => "Podcast channel has been unpublished",
                DMCAAccusationDismissReasonEnum.ShowUnpublished => "Podcast show has been unpublished",
                DMCAAccusationDismissReasonEnum.EpisodeUnpublished => "Podcast episode has been unpublished",
                DMCAAccusationDismissReasonEnum.ShowRemovedInDMCAAccusation => "Podcast show has been removed in a DMCA accusation",
                DMCAAccusationDismissReasonEnum.EpisodeRemovedInDMCAAccusation => "Podcast episode has been removed in a DMCA accusation",
                _ => "Unknown reason"
            };
        }
    }
}
