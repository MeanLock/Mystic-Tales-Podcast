import { IoChatboxEllipsesOutline } from "react-icons/io5";
import { MdOutlineHeadphones } from "react-icons/md";

interface PodcasterCardProps {
  // You can define props here if needed in the future
  podcaster: {
    Id: string;
    FullName: string;
    ImageUrl: string;
    Description: string;
  };
}

const PodcasterCard = ({ podcaster }: PodcasterCardProps) => {
  return (
    <div
      className="
        rounded-[50px]
        p-3 
        w-[350px] 
        h-[301px]
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
    "
    >
      {/* Content của PodcasterCard sẽ được thêm vào đây */}
      <div className="w-full h-full bg-transparent relative flex flex-col items-center justify-end">
        <div
          className="
            w-full
            h-5/6
            rounded-[50px]
          bg-black/20 
            relative
            flex flex-col items-center 
        "
        >
          <img
            src={podcaster.ImageUrl}
            className="w-24 h-24 rounded-full absolute bg-cover object-cover -top-[48px] left-[20px] z-30"
          />

          <div className="w-full p-2 flex items-center">
            <div className="ml-[120px] w-[200px]">
              <h3 className="text-white text-lg font-semibold mb-1 truncate">
                {podcaster.FullName}
              </h3>
            </div>
          </div>

          <div className="mt-[20px] w-full h-[82px]">
            <p className="text-white/90 text-sm px-4 text-start line-clamp-3">
              {podcaster.Description}
            </p>
          </div>

          <div className="w-full flex items-center gap-2 px-4">
            {/* Follow Button */}
            <div
              className="
                w-2/3
                rounded-[50px]
                p-4 
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
                cursor-pointer
            "
            >
              <p className="font-semibold text-white">+ Follow</p>
            </div>
            {/* Quick Icons */}

            <div className="flex flex-1 items-center justify-end gap-2">
              <div
                className="
                rounded-[50px]
                p-4 
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
                cursor-pointer
            "
              >
                <IoChatboxEllipsesOutline color="#fff" size={20} />
              </div>
              <div
                className="
                rounded-[50px]
                p-4 
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
                cursor-pointer
            "
              >
                <MdOutlineHeadphones color="#fff" size={20} />
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PodcasterCard;
