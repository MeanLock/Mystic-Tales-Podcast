import { Skeleton } from "@/components/ui/skeleton";
import { useGetPodcastPublicSourceQuery } from "@/core/services/file/file.service";
import { useEffect, useState } from "react";

const FALL_BACK_URL =
  "https://i.pinimg.com/1200x/1a/54/aa/1a54aab46709edc36c6e0ae2dd12237f.jpg";
const BOOKING_DEFAULT_URL =
  "https://i.pinimg.com/1200x/1a/54/aa/1a54aab46709edc36c6e0ae2dd12237f.jpg";

type AutoResolveImageProps = {
  FileKey?: string | null;
  Name?: string;
  Type?: "booking" | "episode" | "channel" | "other";
};

const AutoResolveImage = ({
  FileKey,
  Name,
  Type = "episode",
}: AutoResolveImageProps) => {
  const [isResolveError, setIsResolveError] = useState(false);

  // 🔁 Khi FileKey đổi thì reset error để thử load lại
  useEffect(() => {
    setIsResolveError(false);
  }, [FileKey]);

  // 🔹 Case 1: Booking => dùng default image riêng
  if (Type === "booking") {
    return (
      <div className="w-12 h-12 flex items-center justify-center">
        <img
          src={BOOKING_DEFAULT_URL}
          className="w-full h-full rounded-full shadow-sm object-cover"
          alt={Name || "Booking"}
          loading="lazy"
        />
      </div>
    );
  }

  // Gọi hook TRƯỚC khi return JSX có điều kiện
  const {
    data: resolveUrl,
    isLoading: isResolveLoading,
    isError: isResolveErrorApi,
  } = useGetPodcastPublicSourceQuery(
    { FileKey: FileKey as string },
    {
      skip: !FileKey || isResolveError, // skip khi không có fileKey hoặc đã lỗi cứng
    }
  );

  // 🔹 Loading state
  if (isResolveLoading && FileKey && !isResolveError) {
    return (
      <div className="w-12 h-12 flex items-center justify-center ">
        <Skeleton className="w-full h-full rounded-full shadow-sm" />
      </div>
    );
  }

  // 🔹 Nếu không có FileKey, hoặc lỗi API, hoặc lỗi onError => fallback
  const shouldShowFallback =
    !FileKey || isResolveError || isResolveErrorApi || !resolveUrl?.FileUrl;

  if (shouldShowFallback) {
    return (
      <div className="w-12 h-12 flex items-center justify-center">
        <img
          src={FALL_BACK_URL}
          className="w-full h-full rounded-full shadow-sm object-cover"
          alt={Name || "Podcast"}
          loading="lazy"
        />
      </div>
    );
  }

  // 🔹 Case 2: resolve được URL thành công
  return (
    <div className="w-12 h-12 flex items-center justify-center">
      <img
        src={resolveUrl.FileUrl}
        className="w-full h-full rounded-full shadow-sm object-cover"
        alt={Name || "Podcast"}
        loading="lazy"
        onError={() => setIsResolveError(true)}
      />
    </div>
  );
};

export default AutoResolveImage;
