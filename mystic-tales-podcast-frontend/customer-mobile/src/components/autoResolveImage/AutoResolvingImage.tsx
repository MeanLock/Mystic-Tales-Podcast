import { getPublisSourceFileUrl } from "@/src/core/services/file/file-v2.service";
import { Image, View, ActivityIndicator, StyleSheet } from "react-native";
import type { ImageStyle, ViewStyle } from "react-native";
import { useState, useEffect } from "react";

interface AutoResolvingImageProps {
  FileKey?: string | null;
  type:
    | "AccountPublicSource"
    | "BookingPublicSource"
    | "PodcastPublicSource"
    | "CategoryPublicSource"
    | "TemplateCommitment"
    | "CommitmentDocument";
  style?: ImageStyle;
  containerStyle?: ViewStyle;
  fileEnum?: string; // Required when type is "TemplateCommitment"
}

const AutoResolvingImage = ({
  FileKey,
  type,
  style,
  containerStyle,
  fileEnum,
}: AutoResolvingImageProps) => {
  const [loadError, setLoadError] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(false);

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
      <Image
        source={require("@/assets/images/user/unknown.jpg")}
        style={style}
        resizeMode="cover"
        onError={() => setLoadError(true)}
      />
    );
  }

  return (
    <Image
      source={{ uri: fileUrl }}
      style={style}
      resizeMode="cover"
      defaultSource={require("@/assets/images/user/unknown.jpg")}
      onError={() => setLoadError(true)}
    />
  );
};

const styles = StyleSheet.create({
  loadingContainer: {
    backgroundColor: "#1a1d24",
    justifyContent: "center",
    alignItems: "center",
  },
  placeholderContainer: {
    backgroundColor: "#2a2d34",
  },
});

export default AutoResolvingImage;
