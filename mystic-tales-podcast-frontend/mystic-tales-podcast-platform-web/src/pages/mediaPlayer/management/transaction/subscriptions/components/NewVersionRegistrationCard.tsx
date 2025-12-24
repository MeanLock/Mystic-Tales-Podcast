// @ts-nocheck
import { Avatar, AvatarImage, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
// import type { RegistrationUI } from "..";
import { Calendar, DollarSign, Eye } from "lucide-react";

type NewVersionRegistrationCardProps = {
  registration: RegistrationUI;
  onViewChanges: (registrationId: string) => void;
};

const NewVersionRegistrationCard = ({
  registration,
  onViewChanges,
}: NewVersionRegistrationCardProps) => {
  return (
    <div className="w-full h-full backdrop-blur-md bg-gradient-to-br from-white/10 to-white/5 border border-white/20 rounded-xl p-6 shadow-xl hover:shadow-2xl transition-all duration-300 hover:scale-[1.02] hover:bg-white/15">
      <div className="flex flex-col gap-4">
        {/* Header with Image and Title */}
        <div className="flex items-start gap-4">
          <Avatar className="w-20 h-20 ring-2 ring-white/30 ring-offset-2 ring-offset-transparent">
            <AvatarImage
              src={registration.SourceInformation.ImageUrl}
              alt={registration.SourceInformation.Name}
              className="object-cover"
            />
            <AvatarFallback className="bg-mystic-green text-black font-bold text-xl">
              {registration.SourceInformation.Name.charAt(0)}
            </AvatarFallback>
          </Avatar>
          <div className="flex-1">
            <h3 className="text-xl font-bold text-white mb-1 line-clamp-2">
              {registration.SourceInformation.Name}
            </h3>
            <Badge
              variant="outline"
              className="backdrop-blur-sm bg-amber-500/20 border-amber-400/30 text-amber-300 font-semibold"
            >
              New Version Available
            </Badge>
          </div>
        </div>

        {/* Current Info */}
        <div className="flex flex-col gap-2 p-4 rounded-lg bg-white/5 backdrop-blur-sm border border-white/10">
          <div className="flex items-center gap-2 text-white/80">
            <DollarSign className="w-4 h-4 text-mystic-green" />
            <span className="text-sm">Current Price:</span>
            <span className="font-bold text-white">
              ${registration.Price.toFixed(2)}
            </span>
            <span className="text-sm text-white/60">
              / {registration.SubscriptionCycleType.Name}
            </span>
          </div>
          <div className="flex items-center gap-2 text-white/80">
            <Calendar className="w-4 h-4 text-mystic-green" />
            <span className="text-sm">Next Payment:</span>
            <span className="font-semibold text-white">
              {new Date(registration.NextPaidAt).toLocaleDateString()}
            </span>
          </div>
        </div>

        {/* Warning Message */}
        <div className="p-3 rounded-lg bg-amber-500/10 border border-amber-400/20 backdrop-blur-sm">
          <p className="text-sm text-amber-200">
            A new version of this subscription is available with updated pricing
            and benefits. Please review the changes before your next payment.
          </p>
        </div>

        {/* Action Button */}
        <Button
          onClick={() => onViewChanges(registration.Id)}
          className="w-full bg-mystic-green hover:bg-mystic-green/90 text-black font-semibold shadow-md flex items-center gap-2 group"
        >
          <Eye className="w-4 h-4 group-hover:scale-110 transition-transform" />
          View Changes & Decide
        </Button>
      </div>
    </div>
  );
};

export default NewVersionRegistrationCard;
