import React, { useState } from "react";
import { cn } from "@/lib/utils";
import { useGetPodcastPublicSourceQuery } from "@/core/services/file/file.service";

type AutoResolveImageProps = {
  name: string;
  fileKey?: string | null;
  className?: string; // wrapper classes
  imgClassName?: string; // img/fallback classes
  alt?: string;
};

const AutoResolveImage: React.FC<AutoResolveImageProps> = ({
  name,
  fileKey,
  className,
  imgClassName,
  alt,
}) => {
  const [imgErrored, setImgErrored] = useState(false);
  const { data, isLoading, isError } = useGetPodcastPublicSourceQuery(
    { FileKey: fileKey as string },
    { skip: !fileKey }
  );

  const resolvedUrl = data?.FileUrl ?? "";
  const showFallback = !fileKey || isError || imgErrored || !resolvedUrl;

  if (isLoading && fileKey) {
    return (
      <div
        className={cn(
          "w-full h-full rounded-md bg-white/10 animate-pulse",
          className
        )}
      />
    );
  }

  return (
    <div className={cn("w-full h-full overflow-hidden", className)}>
      {!showFallback ? (
        <img
          src={resolvedUrl}
          alt={alt ?? name}
          className={cn("w-full h-full object-cover", imgClassName)}
          onError={() => setImgErrored(true)}
        />
      ) : (
        <div
          className={cn(
            "w-full h-full bg-gradient-to-br from-gray-700 to-gray-800 flex items-center justify-center",
            imgClassName
          )}
        >
          <span className="text-white/70 text-lg font-semibold">
            {name?.trim()?.charAt(0)?.toUpperCase() || "?"}
          </span>
        </div>
      )}
    </div>
  );
};

export default AutoResolveImage;
