// @ts-nocheck
import { Skeleton } from "@/components/ui/skeleton";
import { useGetAccountPublicSourceQuery } from "@/core/services/file/file.service";
import { useState } from "react";

const AutoResolveImageBackground = ({
  FileKey,
  FullName,
}: {
  FileKey: string;
  key: string;
  FullName: string;
}) => {
  // STATES
  const [isResolveError, setIsResolveError] = useState(false);
  const [fallBackImageUrl, setFallbackImageUrl] = useState(
    "/images/unknown/user.png"
  );

  // HOOKS
  const { data: resolvedFileData, isLoading: isFileResolving } =
    useGetAccountPublicSourceQuery(
      { FileKey },
      { skip: !FileKey || isResolveError }
    );

  // RENDER
  if (isFileResolving) {
    return (
      <Skeleton className="w-[134px] aspect-square shadow-2xl object-cover rounded-sm" />
    );
  }
  if (isResolveError || !resolvedFileData || !resolvedFileData.FileUrl) {
    return (
      <img
        src={fallBackImageUrl}
        alt={FullName}
        className="w-[134px] aspect-square shadow-2xl object-cover rounded-sm"
        style={{ transformOrigin: "center" }}
      />
    );
  }
  return (
    <img
      src={resolvedFileData.FileUrl}
      alt={FullName}
      onError={() => setIsResolveError(true)}
      className="w-[134px] aspect-square shadow-2xl object-cover rounded-sm"
      style={{ transformOrigin: "center" }}
    />
  );
};
export default AutoResolveImageBackground;
