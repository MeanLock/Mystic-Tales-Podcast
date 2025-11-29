import {
  useGetAccountPublicSourceQuery,
  useGetBookingPublicSourceQuery,
  useGetPodcastPublicSourceQuery,
  useGetCategoryPublicSourceQuery,
  useGetTemplatePodcastBuddyCommitmentFileQuery,
  useGetPodcastBuddyCommitmentFileQuery,
} from "@/src/core/services/file/file.service";
import { Image, View, ActivityIndicator, StyleSheet } from "react-native";
import type { ImageStyle, ViewStyle } from "react-native";
import { useState } from "react";

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
  const hasFileKey = Boolean(FileKey);
  const validFileKey = (FileKey ?? "") as string;

  // Conditionally call the appropriate query based on type
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

  // Determine which data and loading state to use
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
