import { GoCheckCircleFill } from "react-icons/go";
import { FaMapMarkerAlt } from "react-icons/fa";
import { PiDotsThreeCircleFill } from "react-icons/pi";
import { MdCancel } from "react-icons/md";

interface BookingStatusType {
  Id: number;
  Name: string;
}

interface BookingStatusTrackingBarProps {
  currentStatus: {
    Id: number;
    Name: string;
  };
  statusTracking: {
    id: string;
    bookingId: number;
    bookingStatusId: number;
    createdAt: string;
  }[];
}

interface StatusComponentProps {
  status: BookingStatusType;
  isCompleted: boolean;
  isEndStatus: boolean;
  completeAt?: string;
  isCurrent: boolean;
  isLast: boolean;
}

const statusDatas: BookingStatusType[] = [
  { Id: 1, Name: "Quotation Request" },
  { Id: 2, Name: "Quotation Dealing" },
  { Id: 3, Name: "Quotation Rejected" },
  { Id: 4, Name: "Quotation Cancelled" },
  { Id: 5, Name: "Producing" },
  { Id: 6, Name: "Track Previewing" },
  { Id: 7, Name: "Producing Requested" },
  { Id: 8, Name: "Completed" },
  { Id: 9, Name: "Reviewing Cancel Request (By You)..." },
  { Id: 10, Name: "Reviewing Cancel Request (By Buddy)..." },
  { Id: 11, Name: "Cancelled Automatically" },
  { Id: 12, Name: "Cancelled Manually" },
];

const endFlowStatuses = [3, 4, 9, 10, 11, 12];

const StatusComponent: React.FC<StatusComponentProps> = ({
  status,
  isCompleted,
  isEndStatus,
  completeAt,
  isCurrent,
}) => {
  return (
    <div className="flex flex-col items-center gap-1 min-w-[120px] px-2">
      {isCurrent && !isCompleted && !isEndStatus && (
        <FaMapMarkerAlt color="#aee339" size={40} />
      )}
      {isCurrent && isEndStatus && <MdCancel color="#9B1817" size={40} />}
      {isCompleted && <GoCheckCircleFill color="#aee339" size={40} />}
      {!isCompleted && !isCurrent && (
        <PiDotsThreeCircleFill color="#d9d9d9" size={40} />
      )}
      <p
        className={`font-poppins text-xs font-bold text-center ${
          isEndStatus
            ? "text-[#9B1817]"
            : isCurrent
            ? "text-mystic-green"
            : "text-white"
        }`}
      >
        {status.Name}
      </p>
      {completeAt && (
        <p className="font-poppins text-[10px] italic text-[#d9d9d9] text-center">
          Change at: {new Date(completeAt).toLocaleDateString()}
        </p>
      )}
    </div>
  );
};

const BookingStatusTrackingBar: React.FC<BookingStatusTrackingBarProps> = ({
  currentStatus,
  statusTracking,
}) => {
  // Determine the flow based on currentStatus.Id
  const getStatusFlow = (currentStatusId: number): number[] => {
    switch (currentStatusId) {
      case 3:
        return [1, 2, 3];
      case 4:
        return [1, 2, 4];
      case 9:
        return [1, 2, 5, 9];
      case 10:
        return [1, 2, 5, 10];
      case 11:
        return [1, 2, 5, 11];
      case 12:
        return [1, 2, 5, 12];
      case 6:
        return [1, 2, 5, 6, 8];
      case 7:
        return [1, 2, 5, 6, 7, 8];
      default:
        return [1, 2, 5, 8];
    }
  };

  const flowIds = getStatusFlow(currentStatus.Id);

  // Get the latest tracking entry for replicable statuses
  const getLatestTrackingForStatus = (statusId: number): string | undefined => {
    const trackings = statusTracking
      .filter((t) => t.bookingStatusId === statusId)
      .sort(
        (a, b) =>
          new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
      );

    return trackings[0]?.createdAt;
  };

  // Build the display statuses
  const displayStatuses = flowIds.map((statusId, index) => {
    const status = statusDatas.find((s) => s.Id === statusId)!;
    const isEndStatus = endFlowStatuses.includes(statusId);
    const isCurrent = currentStatus.Id === statusId;

    // Check if status is completed (exists in tracking and is before current status)
    const statusIndex = flowIds.indexOf(statusId);
    const currentIndex = flowIds.indexOf(currentStatus.Id);
    const isCompleted = statusIndex < currentIndex;

    // Get the latest createdAt for this status
    let completeAt: string | undefined;
    if (isCompleted || isCurrent) {
      completeAt = getLatestTrackingForStatus(statusId);
    }

    return {
      status,
      isCompleted,
      isEndStatus,
      completeAt,
      isCurrent,
      isLast: index === flowIds.length - 1,
    };
  });

  // Build grid template: alternate 'auto' (status) and '1fr' (connector)
  const templateCols = displayStatuses
    .flatMap((_, i) =>
      i === displayStatuses.length - 1 ? ["auto"] : ["auto", "1fr"]
    )
    .join(" ");

  // Render status cells and connector cells in sequence so connectors grow/shrink
  const gridChildren: React.ReactNode[] = [];
  displayStatuses.forEach((statusProps, i) => {
    gridChildren.push(
      <div
        key={`status-${statusProps.status.Id}`}
        className="flex justify-center"
      >
        <StatusComponent {...statusProps} />
      </div>
    );

    if (i !== displayStatuses.length - 1) {
      gridChildren.push(
        <div
          key={`conn-${i}`}
          className={`w-full h-[2px] self-center ${
            displayStatuses[i].isCompleted ? "bg-mystic-green" : "bg-white"
          }`}
        />
      );
    }
  });

  return (
    <div
      className="w-full grid items-center gap-x-4 px-4 py-6"
      style={{ gridTemplateColumns: templateCols }}
    >
      {gridChildren}
    </div>
  );
};

export default BookingStatusTrackingBar;
