// @ts-nocheck
import { useState, useEffect } from "react";
import { store } from "@/redux/store";
import fileApi from "@/core/services/file/file.service";

const ResolvedImage = ({
  MainImageFileKey,
  Name,
}: {
  MainImageFileKey: string;
  Name: string;
}) => {
  const [ImageUrl, setImageUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const resolveImage = async () => {
      if (!MainImageFileKey) {
        setImageUrl("/images/unknown/content.png");
        setIsLoading(false);
        return;
      }

      setIsLoading(true);
      try {
        const res: any = await store.dispatch(
          fileApi.endpoints.getPodcastPublicSource.initiate({
            FileKey: MainImageFileKey,
          }) as any
        );

        if ("data" in res && res.data && res.data.FileUrl) {
          setImageUrl(res.data.FileUrl);
        } else {
          setImageUrl("/images/unknown/content.png");
        }
      } catch (error) {
        console.error("Failed to resolve image:", error);
        setImageUrl("/images/unknown/content.png");
      } finally {
        setIsLoading(false);
      }
    };

    resolveImage();
  }, [MainImageFileKey]);

  return (
    <img
      src={ImageUrl || "/images/unknown/content.png"}
      alt={Name}
      className="w-12 h-12 aspect-square rounded-md shadow-md"
    />
  );
};

export default ResolvedImage;
