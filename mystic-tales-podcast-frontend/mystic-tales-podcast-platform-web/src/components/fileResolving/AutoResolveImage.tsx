// import { useState, useEffect } from "react";
// import { Skeleton } from "../ui/skeleton";
// import { getPublisSourceFileUrl } from "@/core/services/file/file2.service";
// import UserFallBackImage from "/images/unknown/user.png";
// import ContentFallBackImage from "/images/unknown/content.png";

// interface AutoResolveImageProps {
//   FileKey: string;
//   Name?: string;
//   type:
//     | "AccountPublicSource"
//     | "BookingPublicSource"
//     | "PodcastPublicSource"
//     | "CategoryPublicSource";
//   className?: string; // wrapper classes
//   imgClassName?: string; // img/fallback classes
// }

// const AutoResolveImage = (props: AutoResolveImageProps) => {
//   const { FileKey, Name, type, className, imgClassName } = props;
//   const [isLoading, setIsLoading] = useState(true);
//   const [url, setUrl] = useState<string | null>(null);
//   const [fallBackUrl, setFallBackUrl] = useState<string>("");

//   // Reset error state khi FileKey thay đổi để không dùng trạng thái cũ
//   useEffect(() => {
//     let mounted = true;
//     setIsLoading(true);

//     const resolveImage = async () => {
//       if (!FileKey || FileKey.trim() === "") {
//         setUrl(null);
//         setIsLoading(false);
//         return;
//       }
//       console.log("Resolving image for FileKey:", FileKey, "of type:", type);

//       if (type === "AccountPublicSource") {
//         setFallBackUrl(UserFallBackImage);
//       } else if (type === "PodcastPublicSource") {
//         setFallBackUrl(ContentFallBackImage);
//       } else {
//         setFallBackUrl(ContentFallBackImage);
//       }

//       const responseUrl = await getPublisSourceFileUrl({
//         fileKey: FileKey,
//         type: type,
//       });

//       if (mounted && responseUrl && responseUrl.trim() !== "") {
//         console.log("Resolved URL:", responseUrl);
//         setUrl(responseUrl);
//         setIsLoading(false);
//       } else if (mounted) {
//         console.log("Failed to resolve URL for FileKey:", FileKey);
//         setUrl(null);
//         setIsLoading(false);
//       }
//     };

//     resolveImage();

//     return () => {
//       mounted = false;
//     };
//   }, [FileKey, type]);

//   if (isLoading && FileKey) {
//     return (
//       <Skeleton className={imgClassName || className || "w-full h-full"} />
//     );
//   }

//   return (
//     <img
//       src={url || fallBackUrl}
//       alt={Name || "Image"}
//       className={`${imgClassName || className} object-cover`}
//       onError={() => setUrl(fallBackUrl)}
//     />
//   );
// };

// export default AutoResolveImage;

import { useEffect, useMemo, useState } from "react";
import { Skeleton } from "../ui/skeleton";
import { getPublisSourceFileUrl } from "@/core/services/file/file2.service";
import UserFallBackImage from "/images/unknown/user.png";
import ContentFallBackImage from "/images/unknown/content.png";

interface AutoResolveImageProps {
  FileKey: string;
  Name?: string;
  type:
    | "AccountPublicSource"
    | "BookingPublicSource"
    | "PodcastPublicSource"
    | "CategoryPublicSource";
  className?: string;
  imgClassName?: string;
}

const AutoResolveImage = ({
  FileKey,
  Name,
  type,
  className,
  imgClassName,
}: AutoResolveImageProps) => {
  const [isLoading, setIsLoading] = useState(false);
  const [url, setUrl] = useState<string | null>(null);

  // ✅ Fallback xác định ngay từ type → không cần state
  const fallBackUrl = useMemo(() => {
    return type === "AccountPublicSource"
      ? UserFallBackImage
      : ContentFallBackImage;
  }, [type]);

  useEffect(() => {
    let mounted = true;

    // reset khi FileKey đổi
    setUrl(null);

    if (!FileKey?.trim()) {
      return;
    }

    setIsLoading(true);

    const resolveImage = async () => {
      try {
        const responseUrl = await getPublisSourceFileUrl({
          fileKey: FileKey,
          type,
        });

        if (mounted && responseUrl?.trim()) {
          setUrl(responseUrl);
        }
      } finally {
        if (mounted) {
          setIsLoading(false);
        }
      }
    };

    resolveImage();

    return () => {
      mounted = false;
    };
  }, [FileKey, type]);

  // ✅ Loading
  if (isLoading) {
    return (
      <Skeleton className={imgClassName || className || "w-full h-full"} />
    );
  }

  const finalSrc = url || fallBackUrl;

  // ✅ Không render img nếu không có src hợp lệ (tránh src="")
  if (!finalSrc) return null;

  return (
    <img
      src={finalSrc}
      alt={Name || "Image"}
      className={`${imgClassName || className} object-cover`}
      onError={() => setUrl(fallBackUrl)}
    />
  );
};

export default AutoResolveImage;
