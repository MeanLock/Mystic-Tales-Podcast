import { useState, useEffect } from "react";
import { Skeleton } from "../ui/skeleton";
import { getPublisSourceFileUrl } from "@/core/services/file/file2.service";

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
  const [isLoading, setIsLoading] = useState(true);
  const [url, setUrl] = useState<string | null>(null);

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
    <div className={"rounded-md flex items-center justify-center"}>
      <img
        src={url || FALL_BACK_URL}
        alt={Name || "Image"}
        className={`object-cover w-full h-full` + (className || "")}
        onError={() => setUrl(FALL_BACK_URL)}
      />
    </div>
  );
};

export default AutoResolveImage;
