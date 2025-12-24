import { useCallback } from "react";
import { GrPowerCycle } from "react-icons/gr";
import { RiMoneyDollarCircleLine } from "react-icons/ri";
import { MdArrowForward, MdClose } from "react-icons/md";
import { Button } from "@/components/ui/button";
import type { Registration } from "..";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const NormalRegistrationCard = ({
  registration,
  onCancel,
  onViewDetails,
}: {
  registration: Registration;
  onCancel?: (id: string) => void;
  onViewDetails?: (id: string) => void;
}) => {
  const handleCancel = useCallback(() => {
    onCancel?.(registration.Id);
  }, [registration.Id, onCancel]);

  const handleViewDetails = useCallback(() => {
    onViewDetails?.(registration.Id);
  }, [registration.Id, onViewDetails]);

  const lastPaidDate = new Date(registration.LastPaidAt).toLocaleDateString(
    "en-US",
    {
      month: "short",
      day: "numeric",
      year: "numeric",
    }
  );

  const nextPaidDate = new Date(registration.NextPaidAt).toLocaleDateString(
    "en-US",
    {
      month: "short",
      day: "numeric",
      year: "numeric",
    }
  );

  return (
    <div
      className="
        group relative w-full overflow-hidden
        rounded-2xl
        border border-white/15
        bg-black/10
        shadow-[5px_5px_10px_rgba(0,0,0,0.4)]
        hover:shadow-[5px_5px_15px_rgba(0,0,0,0.4)]
        transition-all duration-500
        hover:scale-[1.02] 
      "
    >
      <div className="space-y-4 px-4 py-4">
        <div className="flex items-center gap-2">
          <AutoResolveImage
            FileKey={registration.SourceInformation.MainImageFileKey}
            type="PodcastPublicSource"
            className="w-10 h-10 object-cover rounded-full shadow-md"
          />
          <h3 className="text-xl font-bold text-white line-clamp-1 dark:text-white">
            {registration.SourceInformation.Type === "Channel"
              ? "Channel: "
              : "Show: "}
            {registration.SourceInformation.Name}
          </h3>
        </div>

        <div
          className="
            grid grid-cols-2 gap-3
            rounded-xl
            bg-black/30
            p-3
          "
        >
          <div className="flex items-center gap-2">
            <div className="rounded-lg bg-white/15 p-2 text-mystic-green shadow-md">
              <GrPowerCycle className="h-5 w-5" />
            </div>
            <div className="min-w-0">
              <p className="text-xs font-medium text-white/70">Type</p>
              <p className="font-semibold text-white">
                {registration.SubscriptionCycleType.Name}
              </p>
            </div>
          </div>

          <div className="flex items-center gap-2">
            <div className="rounded-lg bg-white/15 p-2 text-mystic-green shadow-md">
              <RiMoneyDollarCircleLine className="h-5 w-5" />
            </div>
            <div className="min-w-0">
              <p className="text-xs font-medium text-white/70">Price</p>
              <p className="font-semibold text-white">
                {registration.Price.toLocaleString()}
              </p>
            </div>
          </div>
        </div>

        <div
          className="
            space-y-2
            rounded-xl
            bg-black/30
            p-3
          "
        >
          <div className="flex items-center justify-between text-xs">
            <span className="font-medium text-white/70">Last Paid</span>
            <span className="font-semibold text-white">{lastPaidDate}</span>
          </div>
          <div className="h-1.5 w-full rounded-full bg-white/15 shadow-inner">
            <div
              className="h-full rounded-full bg-linear-to-r from-[#aee339]/40 via-[#aee339]/60 to-[#aee339] shadow-lg transition-all"
              style={{
                width: `${Math.max(
                  10,
                  Math.min(
                    100,
                    ((registration.DayLeft || 0) /
                      (registration.SubscriptionCycleType.Id === 1
                        ? 30
                        : 365)) *
                      100
                  )
                )}%`,
              }}
            />
          </div>
          <div className="flex items-center py-2 justify-between text-xs">
            <span className="font-medium text-white/70">
              Next Payment: {nextPaidDate}
            </span>
            <span className="inline-flex items-center rounded-full bg-[#aee339]/15 px-2.5 py-1 text-[10px] md:text-xs font-semibold text-[#aee339] border border-[#aee339]/30">
              {registration.DayLeft} days left
            </span>
          </div>
        </div>

        <div className="flex gap-2 pt-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleViewDetails}
            className="
                flex-1 gap-2
                 shadow-[0_5px_5px_rgba(0,0,0,0.3)]
                text-mystic-green
                bg-white/5
                border-none
                hover:bg-mystic-green
                hover:text-black
                hover:border-white
              
                transition-all duration-300
            "
          >
            <MdArrowForward className="h-4 w-4" />
            View Details
          </Button>
          <Button
            variant="ghost"
            size="sm"
            onClick={handleCancel}
            className="
                gap-2
                bg-white/5
                text-[#f8770e]
                hover:bg-[#f8770e]
                hover:text-white
                border border-transparent
                hover:border-[#f8770e]/40
                shadow-[0_5px_5px_rgba(0,0,0,0.3)]
                transition-all duration-300
            "
          >
            <MdClose className="h-4 w-4" />
            Cancel
          </Button>
        </div>
      </div>
    </div>
  );
};

export default NormalRegistrationCard;
