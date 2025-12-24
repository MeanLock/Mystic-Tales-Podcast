// @ts-nocheck
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { Card, CardContent } from "@/components/ui/card";
import type { ShowFromAPI, ShowUI } from "@/core/types/show";
import { useNavigate } from "react-router-dom";

const YouMightLikeItCard = ({ card }: { card: ShowFromAPI }) => {
  const navigate = useNavigate();
  return (
    <Card
      onClick={() => navigate(`/media-player/shows/${card.Id}`)}
      className="bg-transparent border-none shadow-sm p-1 transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer"
    >
      <CardContent className="flex aspect-square items-center text-card-foreground bg-transparent justify-center p-2 rounded-lg">
        <AutoResolveImage
          FileKey={card.MainImageFileKey}
          type="PodcastPublicSource"
          Name={card.Name || "show-image"}
          imgClassName="w-full h-full aspect-square object-cover rounded-lg"
        />
      </CardContent>
    </Card>
  );
};

export default YouMightLikeItCard;
