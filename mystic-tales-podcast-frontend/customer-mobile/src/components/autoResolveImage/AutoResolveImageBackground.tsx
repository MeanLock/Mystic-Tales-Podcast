import {
  ActivityIndicator,
  ImageBackground,
  StyleSheet,
  View,
  type ImageBackgroundProps,
  type StyleProp,
  type ImageStyle,
  type ViewStyle,
} from "react-native";

import { getPublisSourceFileUrl } from "@/src/core/services/file/file-v2.service";
import { useState, useEffect } from "react";

type SourceType =
  | "AccountPublicSource"
  | "BookingPublicSource"
  | "PodcastPublicSource"
  | "CategoryPublicSource"
  | "TemplateCommitment"
  | "CommitmentDocument";

interface AutoResolvingImageBackgroundProps
  extends Omit<ImageBackgroundProps, "source"> {
  FileKey?: string | null;
  type: SourceType;
  containerStyle?: StyleProp<ViewStyle>;
  fileEnum?: string; // Required when type is "TemplateCommitment"
}

const AutoResolvingImageBackground = ({
  FileKey,
  type,
  containerStyle,
  fileEnum,
  style,
  ...backgroundProps // blurRadius, children, etc...
}: AutoResolvingImageBackgroundProps) => {
  const [loadError, setLoadError] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);
  const fallbackSource = require("@/assets/images/user/unknown.jpg");
  const hasFileKey = Boolean(FileKey);
  const validFileKey = (FileKey ?? "") as string;

  // Use file-v2 service for all types
  useEffect(() => {
    const loadFileUrl = async () => {
      // Skip TemplateCommitment and CommitmentDocument (not implemented in v2)
      if (type === "TemplateCommitment" || type === "CommitmentDocument") {
        setFileUrl(null);
        return;
      }

      if (!hasFileKey) {
        setFileUrl(null);
        return;
      }

      setIsLoading(true);
      try {
        const url = await getPublisSourceFileUrl({
          fileKey: validFileKey,
          type,
        });
        setFileUrl(url);
      } catch (error) {
        console.error("Error loading file URL:", error);
        setFileUrl(null);
      } finally {
        setIsLoading(false);
      }
    };

    loadFileUrl();
  }, [FileKey, type, hasFileKey, validFileKey]);

  if (isLoading) {
    return (
      <View style={[styles.loadingContainer, containerStyle]}>
        <ActivityIndicator size="small" color="#aee339" />
      </View>
    );
  }

  if (!fileUrl || loadError) {
    return (
      <ImageBackground
        source={fallbackSource}
        style={style}
        resizeMode="cover"
        {...backgroundProps}
      />
    );
  }

  return (
    <ImageBackground
      source={{ uri: fileUrl }}
      onError={() => setLoadError(true)}
      style={style}
      resizeMode="cover"
      defaultSource={fallbackSource}
      {...backgroundProps}
    />
  );
};

const styles = StyleSheet.create({
  loadingContainer: {
    backgroundColor: "#1a1d24",
    justifyContent: "center",
    alignItems: "center",
  },
});

export default AutoResolvingImageBackground;
