import { useState } from "react";
import {
  useGetAccountPublicSourceQuery,
  useGetBookingPublicSourceQuery,
  useGetCategoryPublicSourceQuery,
  useGetPodcastPublicSourceQuery,
} from "@/core/services/file/file.service";
import { Skeleton } from "../ui/skeleton";

const FALL_BACK_URL =
  "https://i.pinimg.com/1200x/b6/2b/e8/b62be83c782a82871177abe584c9e78e.jpg";

interface AutoResolveImageProps {
  FileKey: string;
  Name?: string;
  type:
    | "AccountPublicSource"
    | "BookingPublicSource"
    | "PodcastPublicSource"
    | "CategoryPublicSource";
  className?: string; // wrapper classes
  imgClassName?: string; // img/fallback classes
}

const AutoResolveImage = (props: AutoResolveImageProps) => {
  const { FileKey, Name, type, className, imgClassName } = props;
  const [imgErrored, setImgErrored] = useState(false);

  // Hooks are declared consistently with skip flags so only one executes
  const isAccount = type === "AccountPublicSource";
  const isBooking = type === "BookingPublicSource";
  const isPodcast = type === "PodcastPublicSource";
  const isCategory = type === "CategoryPublicSource";

  const accountQ = useGetAccountPublicSourceQuery(
    { FileKey },
    { skip: !isAccount || !FileKey }
  );
  const bookingQ = useGetBookingPublicSourceQuery(
    { FileKey },
    { skip: !isBooking || !FileKey }
  );
  const podcastQ = useGetPodcastPublicSourceQuery(
    { FileKey },
    { skip: !isPodcast || !FileKey }
  );
  const categoryQ = useGetCategoryPublicSourceQuery(
    { FileKey },
    { skip: !isCategory || !FileKey }
  );

  const resolvedUrl =
    accountQ.data?.FileUrl ||
    bookingQ.data?.FileUrl ||
    podcastQ.data?.FileUrl ||
    categoryQ.data?.FileUrl ||
    "";

  const isLoading =
    accountQ.isLoading ||
    bookingQ.isLoading ||
    podcastQ.isLoading ||
    categoryQ.isLoading;

  const isError =
    accountQ.isError ||
    bookingQ.isError ||
    podcastQ.isError ||
    categoryQ.isError;

  const showFallback = !FileKey || isError || imgErrored || !resolvedUrl;

  if (isLoading && FileKey) {
    return (
      <div
        className={
          "rounded-md flex items-center justify-center" + (className || "")
        }
      >
        <Skeleton className={"w-full h-full " + (imgClassName || "")} />
      </div>
    );
  }

  return (
    <div className={"overflow-hidden " + (className || "")}>
      {!showFallback ? (
        <img
          src={resolvedUrl}
          alt={Name || "image"}
          className={"w-full h-full object-cover " + (imgClassName || "")}
          onError={() => setImgErrored(true)}
        />
      ) : (
        <div
          className={
            "w-full h-full bg-gradient-to-br from-gray-700 to-gray-800 flex items-center justify-center " +
            (imgClassName || "")
          }
        >
          <img
            src={FALL_BACK_URL}
            alt={Name || "image"}
            className={"w-full h-full object-cover " + (imgClassName || "")}
            onError={() => setImgErrored(true)}
          />
        </div>
      )}
    </div>
  );
};

export default AutoResolveImage;
