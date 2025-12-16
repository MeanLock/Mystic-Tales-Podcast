import { useState, useEffect } from "react";
import { Skeleton } from "../ui/skeleton";
import { getPublisSourceFileUrl } from "@/core/services/file/file2.service";
import UserFallBackImage from "/images/unknown/user.png";
import ContentFallBackImage  from "/images/unknown/content.png";

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
  const [isLoading, setIsLoading] = useState(true);
  const [url, setUrl] = useState<string | null>(null);
  const [fallBackUrl, setFallBackUrl] = useState<string>("");

  // Reset error state khi FileKey thay đổi để không dùng trạng thái cũ
  useEffect(() => {
    let mounted = true;
    setIsLoading(true);

    const resolveImage = async () => {
      if (!FileKey) {
        setUrl(null);
        setIsLoading(false);
        return;
      }

      if (type === "AccountPublicSource") {
        setFallBackUrl(UserFallBackImage);
      } else if (type === "PodcastPublicSource") {
        setFallBackUrl(ContentFallBackImage);
      } else {
        setFallBackUrl(ContentFallBackImage);
      }

      const responseUrl = await getPublisSourceFileUrl({
        fileKey: FileKey,
        type: type,
      });

      if (mounted && responseUrl) {
        setUrl(responseUrl);
        setIsLoading(false);
      } else if (mounted) {
        setUrl(null);
        setIsLoading(false);
      }
    };

    resolveImage();

    return () => {
      mounted = false;
    };
  }, [FileKey, type]);

  if (isLoading && FileKey) {
    return (
      <Skeleton className={imgClassName || className || "w-full h-full"} />
    );
  }

  return (
    <img
      src={url || fallBackUrl}
      alt={Name || "Image"}
      className={`${imgClassName || className} object-cover`}
      onError={() => setUrl(fallBackUrl)}
    />
  );
};

export default AutoResolveImage;
