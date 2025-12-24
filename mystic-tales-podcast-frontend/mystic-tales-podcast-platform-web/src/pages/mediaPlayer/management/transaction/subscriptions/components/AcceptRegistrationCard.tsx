import { useCallback } from "react";
import { GrPowerCycle } from "react-icons/gr";
import { RiMoneyDollarCircleLine } from "react-icons/ri";
import { MdArrowForward } from "react-icons/md";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import type { Registration } from "..";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

const AcceptRegistrationCard = ({
  registration,
  onViewDetails,
}: {
  registration: Registration;
  onViewDetails?: (id: string) => void;
}) => {
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
      {/* New Update Badge */}
      <div className="absolute top-3 right-3 z-10">
        <Badge className="bg-mystic-green/20 text-mystic-green border border-mystic-green/40 backdrop-blur-sm font-semibold px-3 py-1 shadow-lg">
          Update Available
        </Badge>
      </div>

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
            border border-mystic-green/10
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
            border border-mystic-green/10
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

        {/* Warning Message */}
        <div className="p-3 rounded-lg bg-amber-500/10 border border-amber-400/30 backdrop-blur-sm">
          <p className="text-xs text-amber-200 font-medium">
            ⚠️ New subscription version available. Review changes before next
            payment.
          </p>
        </div>

        <div className="flex gap-2 pt-2">
          <Button
            size="sm"
            onClick={handleViewDetails}
            className="
                flex-1 gap-2
                shadow-[0_5px_5px_rgba(0,0,0,0.3)]
                bg-mystic-green
                text-black
                border-none
                hover:bg-mystic-green/90
                hover:shadow-[0_0_20px_rgba(174,227,57,0.4)]
                transition-all duration-300
                font-semibold
            "
          >
            <MdArrowForward className="h-4 w-4" />
            View Update Details
          </Button>
        </div>
      </div>
    </div>
  );
};

export default AcceptRegistrationCard;
