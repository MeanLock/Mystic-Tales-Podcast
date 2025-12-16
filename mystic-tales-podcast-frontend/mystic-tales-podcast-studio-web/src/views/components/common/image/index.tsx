import React, { useEffect, useState } from "react";
import NotFound from '../../../../assets/notfound.png'
import { getPublicSource } from "@/core/services/file/file.service";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";

interface ImageProps {
  mainImageFileKey?: string | null;
  mainImageUrl?: string | null;
  className?: string;
  alt?: string;
  size?: number;
  fallbackSrc?: string;
}


const Image: React.FC<ImageProps> = ({
  mainImageFileKey,
  mainImageUrl,
  className,
  alt = "Avatar",
  size,
  fallbackSrc = NotFound,
}) => {
  const [url, setUrl] = useState<string | null>(null);

  useEffect(() => {
    let mounted = true;
    if (!mainImageFileKey && !mainImageUrl) {
      setUrl(null);
      return;
    }
    if (mainImageUrl) {
      setUrl(mainImageUrl);
      return;
    }
    getPublicSource(loginRequiredAxiosInstance, mainImageFileKey).then((res) => {
      if (mounted) setUrl(res.success ? res.data.FileUrl : fallbackSrc);
      
    });
    return () => {
      mounted = false;
    };
  }, [mainImageFileKey, mainImageUrl]);

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