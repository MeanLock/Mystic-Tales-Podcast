import React, { useEffect, useState } from "react";
import { adminAxiosInstance, loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import NotFound from '../../../../assets/images/notfound.png'
import { getPublicSource } from "@/core/services/file/file.service";
import { getReceiptUrl } from "@/core/services/transaction/transaction.service";

interface ImageProps {
  mainImageFileKey?: string | null;
  className?: string;
  alt?: string;
  size?: number;
  type?: string;
  fallbackSrc?: string;
}


const Image: React.FC<ImageProps> = ({
  mainImageFileKey,
  className,
  alt = "Avatar",
  size,
  fallbackSrc = NotFound,
  type = "public",
}) => {
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    if (!mainImageFileKey) {
      setUrl(null);
      return;
    }
    if (type === "public") {
      getPublicSource(loginRequiredAxiosInstance, mainImageFileKey).then((res) => {
        if (mounted) setUrl(res.success ? res.data.FileUrl : fallbackSrc);

      });
    }
    if (type === "receipt") {
      getReceiptUrl(adminAxiosInstance, mainImageFileKey).then((res) => {
        if (mounted) setUrl(res.success ? res.data.FileUrl : fallbackSrc);

      });
    }

    return () => {
      mounted = false;
    };
  }, [mainImageFileKey]);

  return (
    <img
      src={url || fallbackSrc}
      alt={alt}
      className={className}
      style={size ? { width: size, height: size, objectFit: "cover" } : undefined}
      onError={() => setUrl(fallbackSrc)}
    />
  );
};

export default Image;