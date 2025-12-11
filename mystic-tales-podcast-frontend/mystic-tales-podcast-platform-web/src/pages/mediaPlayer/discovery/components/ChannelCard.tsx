import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { Card, CardContent } from "@/components/ui/card";
import type { ChannelFromAPI } from "@/core/types/channel";
import { useNavigate } from "react-router-dom";

const ChannelCard = ({ card }: { card: ChannelFromAPI }) => {
  const navigate = useNavigate();

  return (
    <Card
      onClick={() => navigate(`/media-player/channels/${card.Id}`)}
      className="bg-transparent border-none shadow-sm p-1 transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <CardContent className="flex flex-col aspect-square items-start justify-between text-card-foreground bg-transparent p-2 rounded-lg">
        <AutoResolveImage
          FileKey={card.MainImageFileKey}
          type="PodcastPublicSource"
          imgClassName="w-full h-full aspect-square object-cover rounded-lg mb-2"
        />

        <div className="w-full overflow-ellipsis">
          <p className=" text-white text-sm font-medium mb-2 truncate">
            {card.Name}
          </p>
          <p className=" text-gray-300 text-xs font-medium">
            {card.Podcaster.FullName}
          </p>
        </div>
      </CardContent>
    </Card>
  );
};

export default ChannelCard;
