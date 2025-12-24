import { MdOutlineHeadphones } from "react-icons/md";

interface TopShowCardProps {
  show: {
    Id: string;
    Name: string;
    Podcaster: {
      Id: string;
      FullName: string;
    };
    TotalListenCount: number;
    ImageUrl: string;
  };
}

const LiquidGlassListenCount = ({ count }: { count: number }) => {
  return (
    <div
      className="
        gap-2
        rounded-lg
        px-3
        py-1
        bg-black/20 
        backdrop-blur-sm 
        border 
        border-white/50 
        shadow-[inset_0_1px_0px_rgba(255,255,255,0.75),0_0_9px_rgba(0,0,0,0.2),0_3px_8px_rgba(0,0,0,0.15)] 
        relative 
        before:absolute 
        before:inset-0 
        before:rounded-[50px]
        before:bg-gradient-to-br 
        before:from-white/60 
        before:via-transparent 
        before:to-transparent 
        before:opacity-70 
        before:pointer-events-none 
        after:absolute 
        after:inset-0 
        after:rounded-[50px]
        after:bg-gradient-to-tl 
        after:from-white/30 
        after:via-transparent 
        after:to-transparent 
        after:opacity-50 
        after:pointer-events-none
        flex items-center justify-center
        "
    >
      <p className="text-white text-sm">{count.toLocaleString()}</p>
      <MdOutlineHeadphones className="text-white w-4 h-4" />
    </div>
  );
};
const TopShowCard = ({ show }: TopShowCardProps) => {
  return (
    <div className="gap-3">
      <div className="flex items-center justify-center relative">
        <img
          src={show.ImageUrl}
          alt={show.Name}
          className="w-[219px] h-[219px] rounded-lg object-cover shadow-xl"
        />

        <div className="absolute bottom-2 right-2 z-30">
          <LiquidGlassListenCount count={show.TotalListenCount} />
        </div>
      </div>
      <div className="gap-2">
        <p className="text-[20px] text-white">{show.Name}</p>
        <p className="text-[#D9D9D9] text-sm">{show.Podcaster.FullName}</p>
      </div>
    </div>
  );
};

export default TopShowCard;
