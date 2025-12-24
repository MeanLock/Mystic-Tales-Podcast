import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";
import { useNavigate } from "react-router-dom";

interface PodcasterCardProps {
  podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
}

const PodcasterCard = ({ podcaster }: PodcasterCardProps) => {
  const navigate = useNavigate();
  return (
    <div
      onClick={() => navigate(`/media-player/podcasters/${podcaster.Id}`)}
      className="w-full relative flex flex-col items-center p-2 transition-all duration-500 cursor-pointer ease-out hover:scale-105 hover:-translate-y-1"
    >
      <div className="w-full mb-3 flex items-center justify-center">
        <AutoResolveImage
          FileKey={podcaster.MainImageFileKey}
          type="AccountPublicSource"
          className="w-full aspect-square object-cover rounded-full"
          imgClassName="w-full aspect-square rounded-full shadow-2xl"
        />
      </div>
      <div className="w-full flex flex-col items-center gap-1">
        <p className="font-semibold text-white line-clamp-1">
          {podcaster.FullName}
        </p>
      </div>
    </div>
  );
};

export default PodcasterCard;
