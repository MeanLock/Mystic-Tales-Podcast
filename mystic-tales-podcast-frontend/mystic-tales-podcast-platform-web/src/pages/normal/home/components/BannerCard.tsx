import { useNavigate } from "react-router-dom";
import "./bannerCardStyle.css";

interface BannerCardProps {
  title: string;
  description: string;
  categoryId: string;
  imageUrl: string;
}

const BannerCard = ({
  title,
  description,
  categoryId,
  imageUrl,
}: BannerCardProps) => {
  const navigate = useNavigate();

  const handleNavigate = () => {
    navigate(`/media-player/categories/${categoryId}`);
  };

  return (
    <div
      onClick={() => handleNavigate()}
      className="w-[270px] h-[370px] rounded-[16px] p-2 backdrop-blur-xl bg-white/10 border border-white/30 shadow-2xl flex items-center justify-center transition-all duration-300 ease-out hover:-translate-y-2 hover:shadow-[0_20px_50px_rgba(174,227,57,0.15)] hover:bg-white/20 cursor-pointer"
    >
      {/* Div này dùng imageUrl làm nền */}
      <div
        style={{
          backgroundImage: `url(${imageUrl})`,
          backgroundSize: "cover",
          borderRadius: "16px",
        }}
        className="w-full h-full relative overflow-hidden"
      >
        {/* Overlay nền tối, làm rõ nội dung */}
        <div className="absolute inset-0 bg-gradient-to-t from-black/70 via-black/50 to-transparent rounded-2xl"></div>

        {/* Phần content */}
        <div className="relative z-10 p-4 flex flex-col h-full justify-start">
          <h2 className="text-white text-2xl font-bold mb-2 drop-shadow-lg truncate">
            {title}
          </h2>
          <p className="text-white/90 text-sm mb-4 drop-shadow-md">
            {description}
          </p>
        </div>
      </div>
    </div>
  );
};

export default BannerCard;
