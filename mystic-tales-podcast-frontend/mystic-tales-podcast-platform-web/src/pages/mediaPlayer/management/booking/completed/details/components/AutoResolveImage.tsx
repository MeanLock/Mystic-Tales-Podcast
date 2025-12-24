import { useGetAccountPublicSourceQuery } from "@/core/services/file/file.service";
import { useState } from "react";

const AutoResolveImage = ({ FileKey }: { FileKey: string | null }) => {
  const [isImageError, setIsImageError] = useState(false);

  if (!FileKey) {
    return (
      <img
        src="https://i.pinimg.com/1200x/a7/4d/41/a74d41df10e0700557259776ef26beb1.jpg"
        className="w-8 h-8 object-cover rounded-full shadow-sm"
        alt="Booking Image"
      />
    );
  }

  const { data: userImageUrl, isLoading } = useGetAccountPublicSourceQuery(
    {
      FileKey,
    },
    {
      refetchOnMountOrArgChange: true,
      refetchOnFocus: true,
      refetchOnReconnect: true,
    }
  );

  if (isLoading) {
    return (
      <img
        src="https://i.pinimg.com/1200x/a7/4d/41/a74d41df10e0700557259776ef26beb1.jpg"
        className="w-8 h-8 object-cover rounded-full shadow-sm"
        alt="Booking Image"
      />
    );
  }

  if (isImageError || !userImageUrl?.FileUrl) {
    return (
      <img
        src="https://i.pinimg.com/1200x/a7/4d/41/a74d41df10e0700557259776ef26beb1.jpg"
        className="w-8 h-8 object-cover rounded-full shadow-sm"
        alt="Booking Image"
      />
    );
  }

  return (
    <img
      src={userImageUrl?.FileUrl}
      className="w-8 h-8 object-cover rounded-full shadow-sm"
      alt="Booking Image"
      onError={() => setIsImageError(true)}
    />
  );
};

export default AutoResolveImage;
