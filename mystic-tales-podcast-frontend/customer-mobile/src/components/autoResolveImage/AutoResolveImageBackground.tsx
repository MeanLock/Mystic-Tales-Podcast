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

import {
  useGetAccountPublicSourceQuery,
  useGetBookingPublicSourceQuery,
  useGetPodcastPublicSourceQuery,
  useGetCategoryPublicSourceQuery,
  useGetTemplatePodcastBuddyCommitmentFileQuery,
  useGetPodcastBuddyCommitmentFileQuery,
} from "@/src/core/services/file/file.service";
import { useState } from "react";

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
  const fallbackSource = require("@/assets/images/user/unknown.jpg");
  const hasFileKey = Boolean(FileKey);
  const validFileKey = (FileKey ?? "") as string;

  // Queries theo type
  const { data: accountData, isLoading: isAccountLoading } =
    useGetAccountPublicSourceQuery(
      { FileKey: validFileKey },
      {
        skip: type !== "AccountPublicSource" || !hasFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const { data: bookingData, isLoading: isBookingLoading } =
    useGetBookingPublicSourceQuery(
      { FileKey: validFileKey },
      {
        skip: type !== "BookingPublicSource" || !hasFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const { data: podcastData, isLoading: isPodcastLoading } =
    useGetPodcastPublicSourceQuery(
      { FileKey: validFileKey },
      {
        skip: type !== "PodcastPublicSource" || !hasFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const { data: categoryData, isLoading: isCategoryLoading } =
    useGetCategoryPublicSourceQuery(
      { FileKey: validFileKey },
      {
        skip: type !== "CategoryPublicSource" || !hasFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const { data: templateData, isLoading: isTemplateLoading } =
    useGetTemplatePodcastBuddyCommitmentFileQuery(
      { fileEnum: fileEnum! },
      {
        skip: type !== "TemplateCommitment" || !fileEnum,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const { data: commitmentData, isLoading: isCommitmentLoading } =
    useGetPodcastBuddyCommitmentFileQuery(
      { FileKey: validFileKey },
      {
        skip: type !== "CommitmentDocument" || !hasFileKey,
        refetchOnMountOrArgChange: true,
        refetchOnFocus: true,
        refetchOnReconnect: true,
      }
    );

  const isLoading =
    isAccountLoading ||
    isBookingLoading ||
    isPodcastLoading ||
    isCategoryLoading ||
    isTemplateLoading ||
    isCommitmentLoading;

  const fileUrl =
    accountData?.FileUrl ||
    bookingData?.FileUrl ||
    podcastData?.FileUrl ||
    categoryData?.FileUrl ||
    templateData?.FileUrl ||
    commitmentData?.FileUrl;

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
