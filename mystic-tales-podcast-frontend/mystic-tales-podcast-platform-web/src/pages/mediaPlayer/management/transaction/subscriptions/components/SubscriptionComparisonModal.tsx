// @ts-nocheck
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  CheckCircle2,
  XCircle,
  DollarSign,
  Calendar,
  Shield,
} from "lucide-react";
import type { RegistrationUI } from "..";
import type {
  PodcastSubscriptionRegistrationDetails,
  SubscriptionDetails,
} from "@/core/types/subscription";
import { FaLongArrowAltRight } from "react-icons/fa";

type SubscriptionComparisonModalProps = {
  isOpen: boolean;
  onClose: () => void;
  registration: RegistrationUI;
  currentSubscriptionDetails: PodcastSubscriptionRegistrationDetails | null;
  newSubscriptionDetails: SubscriptionDetails | null;
  isLoading: boolean;
  onAccept: () => void;
  onReject: () => void;
  isProcessing: boolean;
};

const SubscriptionComparisonModal = ({
  isOpen,
  onClose,
  registration,
  currentSubscriptionDetails,
  newSubscriptionDetails,
  isLoading,
  onAccept,
  onReject,
  isProcessing,
}: SubscriptionComparisonModalProps) => {
  // Get current price from registration (this is the price at time of registration)
  const currentPrice = registration.Price;

  // Get new cycle price from subscription details
  const newCycle =
    newSubscriptionDetails?.PodcastSubscriptionCycleTypePriceList?.find(
      (cycle) =>
        cycle.SubscriptionCycleType.Id === registration.SubscriptionCycleType.Id
    );
  const newPrice = newCycle?.Price || 0;

  // Compare benefits
  const currentBenefits =
    currentSubscriptionDetails?.PodcastSubscriptionBenefitList || [];
  const newBenefits =
    newSubscriptionDetails?.PodcastSubscriptionBenefitMappingList || [];

  // Find added and removed benefits
  const addedBenefits = newBenefits.filter(
    (newBenefit) =>
      !currentBenefits.some(
        (current) => current.Id === newBenefit.PodcastSubscriptionBenefit.Id
      )
  );
  const removedBenefits = currentBenefits.filter(
    (currentBenefit) =>
      !newBenefits.some(
        (newB) => newB.PodcastSubscriptionBenefit.Id === currentBenefit.Id
      )
  );
  const unchangedBenefits = currentBenefits.filter((currentBenefit) =>
    newBenefits.some(
      (newB) => newB.PodcastSubscriptionBenefit.Id === currentBenefit.Id
    )
  );

  const priceChange = newPrice - currentPrice;
  const priceChangePercent =
    currentPrice > 0 ? (priceChange / currentPrice) * 100 : 0;

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent
        className="scrollbar-hide max-w-[70vw] sm:max-w-[70vw]
              z-[9999] 
              w-full  max-h-[90vh] overflow-y-auto backdrop-blur-md bg-black/20 border-[1px] border-mystic-green rounded-md text-white"
      >
        <DialogHeader>
          <DialogTitle className="text-3xl font-bold text-white mb-2">
            Subscription Update Review
          </DialogTitle>
          <p className="text-white/70">
            Compare the changes between your current subscription and the new
            version
          </p>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-mystic-green"></div>
          </div>
        ) : (
          <div className="space-y-6 mt-4">
            {/* Subscription Info */}
            <div className="p-4 rounded-lg bg-white/5 backdrop-blur-sm border border-white/10 text-center">
              <h3 className="text-2xl font-bold text-white mb-2">
                {registration.SourceInformation.Name}
              </h3>
              <Badge
                variant="outline"
                className="bg-mystic-green/20 px-3 py-1 text-mystic-green border-mystic-green/30"
              >
                {registration.SubscriptionCycleType.Name}
              </Badge>
            </div>

            {/* Two Column Comparison */}
            <div className="grid grid-cols-12 gap-6">
              {/* LEFT COLUMN - CURRENT VERSION */}
              <div className="space-y-4 col-span-5">
                <div className="flex items-center gap-2 justify-center pb-2 border-b border-white/10">
                  <h3 className="text-lg font-bold text-gray-400">
                    Current Version
                  </h3>
                </div>

                {/* Current Price */}
                <div className="p-6 rounded-lg bg-white/5 backdrop-blur-sm border border-white/10">
                  <div className="flex items-center gap-2 mb-3">
                    <DollarSign className="w-5 h-5 text-gray-400" />
                    <h4 className="text-base font-semibold text-white">
                      Price
                    </h4>
                  </div>
                  <p className="text-3xl font-bold text-white">
                    {currentPrice.toLocaleString()}
                  </p>
                  <p className="text-xs text-white/60 mt-1">
                    per {registration.SubscriptionCycleType.Name.toLowerCase()}
                  </p>
                </div>

                {/* Current Benefits */}
                <div className="p-4 rounded-lg bg-white/5 backdrop-blur-sm border border-white/10">
                  <div className="flex items-center gap-2 mb-3">
                    <Shield className="w-5 h-5 text-gray-400" />
                    <h4 className="text-base font-semibold text-white">
                      Benefits
                    </h4>
                  </div>
                  <ul className="space-y-2">
                    {currentBenefits.map((benefit) => {
                      const isRemoved = removedBenefits.some(
                        (b) => b.Id === benefit.Id
                      );
                      return (
                        <li
                          key={benefit.Id}
                          className={`flex items-start gap-2 ${
                            isRemoved
                              ? "text-white/40 line-through"
                              : "text-white/80"
                          }`}
                        >
                          {isRemoved ? (
                            <XCircle className="w-4 h-4 text-red-400 mt-0.5 flex-shrink-0" />
                          ) : (
                            <CheckCircle2 className="w-4 h-4 text-blue-400 mt-0.5 flex-shrink-0" />
                          )}
                          <span className="text-sm">{benefit.Name}</span>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              </div>
              <div className="col-span-2 h-full w-full flex items-center justify-center">
                <FaLongArrowAltRight size={40} />
              </div>
              {/* RIGHT COLUMN - NEW VERSION */}
              <div className="space-y-4 col-span-5">
                <div className="flex items-center gap-2 justify-center pb-2 border-b border-mystic-green/30">
                  <h3 className="text-lg font-bold text-mystic-green">
                    Next Version
                  </h3>
                </div>

                {/* New Price */}
                <div className="p-6 rounded-lg bg-gradient-to-br from-mystic-green/20 to-mystic-green/10 backdrop-blur-sm border border-mystic-green/30">
                  <div className="flex items-center gap-2 mb-3">
                    <DollarSign className="w-5 h-5 text-mystic-green" />
                    <h4 className="text-base font-semibold text-white">
                      Price
                    </h4>
                  </div>
                  <p className="text-3xl font-bold text-mystic-green">
                    {newPrice.toLocaleString()}
                  </p>
                  <p className="text-xs text-white/80 mt-1">
                    per {registration.SubscriptionCycleType.Name.toLowerCase()}
                  </p>
                  {priceChange !== 0 && (
                    <Badge
                      variant="outline"
                      className={`mt-3 ${
                        priceChange < 0
                          ? "bg-red-500/20 text-red-300 border-red-400/30"
                          : "bg-green-500/20 text-green-300 border-green-400/30"
                      }`}
                    >
                      {priceChange > 0 ? "+" : "-"}
                      {priceChange.toLocaleString()}
                    </Badge>
                  )}
                </div>

                {/* New Benefits */}
                <div className="p-4 rounded-lg bg-gradient-to-br from-mystic-green/20 to-mystic-green/10 backdrop-blur-sm border border-mystic-green/30">
                  <div className="flex items-center gap-2 mb-3">
                    <Shield className="w-5 h-5 text-mystic-green" />
                    <h4 className="text-base font-semibold text-white">
                      Benefits
                    </h4>
                  </div>
                  <ul className="space-y-2">
                    {newBenefits.map((benefit) => {
                      const isNew = addedBenefits.some(
                        (b) =>
                          b.PodcastSubscriptionBenefit.Id ===
                          benefit.PodcastSubscriptionBenefit.Id
                      );
                      return (
                        <li
                          key={benefit.PodcastSubscriptionBenefit.Id}
                          className="flex items-start gap-2 text-white"
                        >
                          <CheckCircle2
                            className={`w-4 h-4 mt-0.5 flex-shrink-0 ${
                              isNew ? "text-green-400" : "text-mystic-green"
                            }`}
                          />
                          <span className="text-sm flex items-center gap-2">
                            {benefit.PodcastSubscriptionBenefit.Name}
                            {isNew && (
                              <Badge className="bg-green-500/20 text-green-300 border-green-400/30 text-[10px] px-1.5 py-0">
                                NEW
                              </Badge>
                            )}
                          </span>
                        </li>
                      );
                    })}
                  </ul>
                </div>
              </div>
            </div>

            {/* Next Payment Info */}
            <div className="p-4 rounded-lg bg-amber-500/10 border border-amber-400/20 backdrop-blur-sm">
              <div className="flex items-start gap-3">
                <Calendar className="w-5 h-5 text-amber-400 mt-0.5" />
                <div>
                  <h5 className="font-semibold text-amber-200 mb-1">
                    Important Note
                  </h5>
                  <p className="text-sm text-amber-100/80">
                    If you accept this update, the new pricing and benefits will
                    take effect on your next payment date:{" "}
                    <span className="font-semibold text-amber-200">
                      {new Date(registration.NextPaidAt).toLocaleDateString()}
                    </span>
                  </p>
                </div>
              </div>
            </div>

            {/* Action Buttons */}
            <div className="flex gap-4 pt-4">
              {registration.IsAcceptNewestVersionSwitch !== false && (
                <Button
                  onClick={onReject}
                  disabled={isProcessing}
                  className="flex-1 bg-red-600 hover:bg-red-700 text-white font-semibold shadow-md"
                >
                  {isProcessing ? "Processing..." : "Reject Update"}
                </Button>
              )}

              <Button
                onClick={onAccept}
                disabled={isProcessing}
                className="flex-1 bg-mystic-green hover:bg-mystic-green/90 text-black font-semibold shadow-md"
              >
                {isProcessing ? "Processing..." : "Accept Update"}
              </Button>
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default SubscriptionComparisonModal;
