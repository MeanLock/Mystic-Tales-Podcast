import Loading from "@/components/loading";
import {
  useCancelSubscriptionRegistrationMutation,
  useGetCustomerRegistrationsQuery,
  useGetRegistrationDetailsQuery,
  useMakeDecisionOnAcceptingNewestVersionMutation,
} from "@/core/services/subscription/subscription.service";
import { useEffect, useState } from "react";
import NormalRegistrationCard from "./components/NormalRegistrationCard";
import SubscriptionComparisonModal from "./components/SubscriptionComparisonModal";

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { setError } from "@/redux/slices/errorSlice/errorSlice";
import { useDispatch } from "react-redux";
import { useGetActiveChannelSubscriptionQuery } from "@/core/services/channel/channel.service";
import { useGetActiveShowSubscriptionQuery } from "@/core/services/show/show.service";
import AcceptRegistrationCard from "./components/AcceptRegistrationCard";

export type Registration = {
  // Id của registration
  Id: string;
  // Id của PodcastSubscription
  PodcastSubscriptionId: number;
  // Loại chu kỳ đăng ký
  SubscriptionCycleType: {
    Id: number; // 1 là Monthly, 2 là Annually
    Name: string; // Monthly hoặc Annually
  };
  Price: number; // Giá tại thời điểm đăng ký
  SourceInformation: {
    Type: "Show" | "Channel"; // Loại nguồn thông tin
    Id: string;
    Name: string;
    MainImageFileKey: string;
  };
  IsAcceptNewestVersionSwitch: boolean | null; // Có chấp nhận tự động nâng cấp phiên bản mới không, null là chưa có sự thay đổi, false là đang đợi xác nhận update, true là đã chấp nhận
  LastPaidAt: string; // Ngày thanh toán cuối cùng
  NextPaidAt: string; // Lấy LastPaidAt + Cycle Duration (30 hoặc 365 ngày)
  DayLeft: number; // Số ngày còn lại đến kỳ thanh toán tiếp theo
};

const ManagementSubscriptionsPage = () => {
  // STATES
  const [customerRegistrations, setCustomerRegistrations] = useState<
    Registration[]
  >([]);

  // IsAcceptNewestVersionSwitch === false
  const [newVersionRegistrations, setNewVersionRegistrations] = useState<
    Registration[]
  >([]);
  const [viewMode, setViewMode] = useState<"all" | "new-version-only">("all");
  const [isResolving, setIsResolving] = useState(false);
  const [selectedRegistration, setSelectedRegistration] =
    useState<Registration | null>(null);
  const [isConfirmingCancelAlertOpen, setIsConfirmingCancelAlertOpen] =
    useState(false);
  const [isComparisonModalOpen, setIsComparisonModalOpen] = useState(false);

  // HOOKS
  const dispatch = useDispatch();

  const {
    data: customerRegistrationsData,
    isLoading: isLoadingCustomerRegistrations,
    refetch: refetchCustomerRegistrations,
  } = useGetCustomerRegistrationsQuery(undefined, {
    refetchOnMountOrArgChange: true,
    refetchOnFocus: true,
    refetchOnReconnect: true,
  });

  const [cancelSubscription, { isLoading: isCancellingSubscription }] =
    useCancelSubscriptionRegistrationMutation();

  const [makeDecision, { isLoading: isMakingDecision }] =
    useMakeDecisionOnAcceptingNewestVersionMutation();

  const {
    data: registrationDetailsData,
    isLoading: isLoadingRegistrationDetails,
  } = useGetRegistrationDetailsQuery(
    { PodcastSubscriptionRegistrationId: selectedRegistration?.Id! },
    { skip: !selectedRegistration?.Id }
  );

  const {
    data: channelNewSubscriptionInformations,
    isLoading: isLoadingChannelSubscriptionInformations,
  } = useGetActiveChannelSubscriptionQuery(
    { ChannelId: selectedRegistration?.SourceInformation.Id! },
    {
      skip:
        !selectedRegistration?.SourceInformation.Id ||
        selectedRegistration?.SourceInformation.Type !== "Channel",
    }
  );
  const {
    data: showNewSubscriptionInformations,
    isLoading: isLoadingShowSubscriptionInformations,
  } = useGetActiveShowSubscriptionQuery(
    { ShowId: selectedRegistration?.SourceInformation.Id! },
    {
      skip:
        !selectedRegistration?.SourceInformation.Id ||
        selectedRegistration?.SourceInformation.Type !== "Show",
    }
  );

  useEffect(() => {
    const resolveFileData = async () => {
      if (!customerRegistrationsData || isLoadingCustomerRegistrations) return;

      setIsResolving(true);

      // Resolve files for both Channel and Show images

      // Transform to RegistrationUI format
      const transformedRegistrations: Registration[] =
        customerRegistrationsData.PodcastSubscriptionRegistrationList.map(
          (reg) => {
            // Calculate next paid date based on cycle type
            const lastPaidDate = new Date(reg.LastPaidAt);
            const cycleDays = reg.SubscriptionCycleType.Id === 1 ? 30 : 365; // Monthly or Annually
            const nextPaidDate = new Date(lastPaidDate);
            nextPaidDate.setDate(nextPaidDate.getDate() + cycleDays);

            // Calculate days left
            const today = new Date();
            const daysLeft = Math.ceil(
              (nextPaidDate.getTime() - today.getTime()) / (1000 * 60 * 60 * 24)
            );

            // Determine source information
            const sourceInfo = reg.PodcastChannel
              ? {
                  Type: "Channel" as const,
                  Id: reg.PodcastChannel.Id,
                  Name: reg.PodcastChannel.Name,
                  MainImageFileKey: reg.PodcastChannel.MainImageFileKey,
                }
              : reg.PodcastShow
              ? {
                  Type: "Show" as const,
                  Id: reg.PodcastShow!.Id,
                  Name: reg.PodcastShow!.Name,
                  MainImageFileKey: reg.PodcastShow.MainImageFileKey,
                }
              : {
                  Type: "Show" as const,
                  Id: "",
                  Name: "Unknown Show",
                  MainImageFileKey: "",
                };

            return {
              Id: reg.Id,
              PodcastSubscriptionId: reg.PodcastSubscriptionId,
              SubscriptionCycleType: reg.SubscriptionCycleType,
              Price: reg.Price,
              SourceInformation: sourceInfo,
              IsAcceptNewestVersionSwitch: reg.IsAcceptNewestVersionSwitch,
              LastPaidAt: reg.LastPaidAt,
              NextPaidAt: nextPaidDate.toISOString(),
              DayLeft: daysLeft,
            };
          }
        );
      console.log("Transformed Registrations: ", transformedRegistrations);
      setCustomerRegistrations(transformedRegistrations);

      // Filter registrations with IsAcceptNewestVersionSwitch === false
      const newVersionOnly = transformedRegistrations.filter(
        (reg) => reg.IsAcceptNewestVersionSwitch === false
      );
      setNewVersionRegistrations(newVersionOnly);

      setIsResolving(false);
    };
    resolveFileData();
  }, [customerRegistrationsData, isLoadingCustomerRegistrations]);

  // FUNCTIONS
  const handleCancelRegistration = (registrationId: string) => {
    const registration = customerRegistrations.find(
      (r) => r.Id === registrationId
    );
    if (registration) {
      setSelectedRegistration(registration);
      setIsConfirmingCancelAlertOpen(true);
    }
  };

  const handleViewChanges = (registrationId: string) => {
    const registration = customerRegistrations.find(
      (r) => r.Id === registrationId
    );
    if (registration) {
      console.log("Registration ID Current: ", registration.Id);
      setSelectedRegistration(registration);
      setIsComparisonModalOpen(true);
    }
  };

  const handleAcceptUpdate = async () => {
    if (!selectedRegistration) return;

    try {
      await makeDecision({
        PodcastSubscriptionRegistrationId: selectedRegistration.Id,
        IsAccepted: true,
      }).unwrap();

      await refetchCustomerRegistrations();
      setIsComparisonModalOpen(false);
      setSelectedRegistration(null);
    } catch (error) {
      console.error("Failed to accept update:", error);
      dispatch(
        setError({
          message: `Failed to accept update: ${error}`,
          autoClose: 10,
        })
      );
    }
  };

  const handleRejectUpdate = async () => {
    if (!selectedRegistration) return;

    try {
      await makeDecision({
        PodcastSubscriptionRegistrationId: selectedRegistration.Id,
        IsAccepted: false,
      }).unwrap();

      await refetchCustomerRegistrations();
      setIsComparisonModalOpen(false);
      setSelectedRegistration(null);
    } catch (error) {
      console.error("Failed to reject update:", error);
      dispatch(
        setError({
          message: `Failed to reject update: ${error}`,
          autoClose: 10,
        })
      );
    }
  };

  const handleConfirmCancel = async () => {
    if (!selectedRegistration) return;

    try {
      await cancelSubscription({
        PodcastSubscriptionRegistrationId: selectedRegistration.Id,
      }).unwrap();

      // Refetch data after successful cancellation
      await refetchCustomerRegistrations();

      setIsConfirmingCancelAlertOpen(false);
      setSelectedRegistration(null);
    } catch (error) {
      console.error("Failed to cancel subscription:", error);
      dispatch(
        setError({
          message: `Failed to cancel subscription: ${error}`,
          autoClose: 10,
        })
      );
    }
  };

  // RENDER
  if (isLoadingCustomerRegistrations || isResolving) {
    return (
      <div className="w-full h-full flex flex-col items-center justify-center">
        <Loading />
        <p className="font-poppins font-bold text-[#D9D9D9]">
          Loading Subscriptions...
        </p>
      </div>
    );
  }

  return (
    <div className="w-full h-full flex flex-col items-start gap-5">
      <p className="text-5xl m-8 font-poppins  font-bold text-white">
        <span className="text-mystic-green">Subscriptions</span> Management
      </p>

      {/* View Mode Toggle */}
      <div className="w-full px-8 flex items-center gap-5">
        <div
          onClick={() => setViewMode("all")}
          className={`
          w-38.5 py-2 cursor-pointer shadow-md rounded-md font-poppins font-semibold
          flex items-center justify-center
          transition-all duration-500 ease-out
          ${
            viewMode === "all"
              ? "bg-mystic-green text-black"
              : "text-white border-white border hover:bg-mystic-green"
          }
          `}
        >
          <p>All</p>
        </div>
        <div
          onClick={() => setViewMode("new-version-only")}
          className={`
          w-38.5 py-2 cursor-pointer shadow-md rounded-md font-poppins font-semibold
          flex items-center justify-center
          transition-all duration-500 ease-out
          ${
            viewMode === "new-version-only"
              ? "bg-mystic-green text-black"
              : "text-white border-white border hover:bg-mystic-green"
          }
          `}
        >
          <p>New Versions</p>
        </div>
      </div>

      {/* Content */}
      {viewMode === "all" ? (
        <div className="w-full">
          <div className="w-full p-8 grid grid-cols-3">
            {customerRegistrations.map((registration) => (
              <div key={registration.Id} className="p-2 w-full col-span-1">
                <NormalRegistrationCard
                  registration={registration}
                  onCancel={handleCancelRegistration}
                />
              </div>
            ))}
          </div>
        </div>
      ) : (
        <div className="w-full">
          <div className="w-full p-8 grid grid-cols-3">
            {newVersionRegistrations.map((registration) => (
              <div key={registration.Id} className="p-2 w-full col-span-1">
                {/* <NewVersionRegistrationCard
                  registration={registration}
                  onViewChanges={handleViewChanges}
                /> */}
                <AcceptRegistrationCard
                  registration={registration}
                  onViewDetails={handleViewChanges}
                />
              </div>
            ))}
          </div>
          {newVersionRegistrations.length === 0 && (
            <div className="w-full p-8 flex flex-col items-center justify-center gap-4">
              <p className="text-white/70 text-lg font-poppins">
                No pending subscription updates
              </p>
            </div>
          )}
        </div>
      )}

      {/* Comparison Modal */}
      {selectedRegistration && (
        <SubscriptionComparisonModal
          isOpen={isComparisonModalOpen}
          onClose={() => {
            setIsComparisonModalOpen(false);
            setSelectedRegistration(null);
          }}
          registration={selectedRegistration}
          currentSubscriptionDetails={
            registrationDetailsData?.PodcastSubscriptionRegistration || null
          }
          newSubscriptionDetails={
            selectedRegistration.SourceInformation.Type === "Channel"
              ? channelNewSubscriptionInformations?.PodcastSubscription || null
              : showNewSubscriptionInformations?.PodcastSubscription || null
          }
          isLoading={
            isLoadingRegistrationDetails ||
            isLoadingChannelSubscriptionInformations ||
            isLoadingShowSubscriptionInformations
          }
          onAccept={handleAcceptUpdate}
          onReject={handleRejectUpdate}
          isProcessing={isMakingDecision}
        />
      )}

      {/* Cancel Confirmation Dialog */}
      <AlertDialog
        open={isConfirmingCancelAlertOpen}
        onOpenChange={setIsConfirmingCancelAlertOpen}
      >
        <AlertDialogContent className="backdrop-blur-md bg-white/10 border border-white/20 text-white">
          <AlertDialogHeader>
            <AlertDialogTitle className="text-2xl font-bold text-white">
              Cancel Subscription?
            </AlertDialogTitle>
            <AlertDialogDescription className="text-white/70">
              {selectedRegistration && (
                <>
                  Are you sure you want to cancel your subscription to{" "}
                  <span className="font-semibold text-mystic-green">
                    {selectedRegistration.SourceInformation.Name}
                  </span>
                  ? This action cannot be undone and you will lose access to all
                  premium content.
                </>
              )}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel className="bg-white/10 backdrop-blur-sm text-white border-white/20 hover:bg-white/20">
              Keep Subscription
            </AlertDialogCancel>
            <AlertDialogAction
              onClick={handleConfirmCancel}
              disabled={isCancellingSubscription}
              className="bg-red-600 hover:bg-red-700 text-white"
            >
              {isCancellingSubscription ? "Cancelling..." : "Yes, Cancel"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
};

export default ManagementSubscriptionsPage;
