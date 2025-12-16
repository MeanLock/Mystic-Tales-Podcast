import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { useFocusEffect, useLocalSearchParams, useRouter } from "expo-router";
import { useCallback, useEffect, useState } from "react";
import {
  ActivityIndicator,
  Alert,
  Modal,
  Pressable,
  ScrollView,
  StyleSheet,
  TextInput,
} from "react-native";
import EpisodeInformations from "./components/EpisodeInformations";
import EpisodeDescription from "./components/EpisodeDescription";
import {
  useGetEpisodeDetailsQuery,
  useSaveEpisodeMutation,
} from "@/src/core/services/episode/episode.service";
import EpisodeMoreInformations from "./components/EpisodeMoreInformations";
import {
  useLazyGetEpisodeReportTypesQuery,
  useReportEpisodeMutation,
} from "@/src/core/services/report/report.service";
import { useDispatch, useSelector } from "react-redux";
import { setDataAndShowAlert } from "@/src/features/alert/alertSlice";
import { RootState } from "@/src/store/store";
import { ReportType } from "@/src/core/types/report.type";
import { registerAlertAction } from "@/src/components/alert/GlobalAlert";

export default function EpisdeDetailsScreen() {
  // STATES
  const [isSaved, setIsSaved] = useState<boolean>(false);
  const [isReportModalVisible, setIsReportModalVisible] =
    useState<boolean>(false);
  const [avaiableReportTypes, setAvaiableReportTypes] = useState<ReportType[]>(
    []
  );
  const [selectedReportReason, setSelectedReportReason] =
    useState<ReportType | null>(null);
  const [customReportReason, setCustomReportReason] = useState<string>("");
  // REDUX
  const user = useSelector((state: RootState) => state.auth.user);
  // HOOKS
  const { id } = useLocalSearchParams<{ id: string }>();
  const router = useRouter();
  const dispatch = useDispatch();

  const [getReportReasons] = useLazyGetEpisodeReportTypesQuery();
  const [reportEpisode, { isLoading: isReporting }] =
    useReportEpisodeMutation();

  const {
    data: episodeData,
    isLoading: isEpisodeLoading,
    refetch: refetchEpisode,
  } = useGetEpisodeDetailsQuery({ PodcastEpisodeId: id! }, { skip: !id });

  const [saveEpisode, { isLoading: isSaveEpisodeLoading }] =
    useSaveEpisodeMutation();

  useEffect(() => {
    if (!episodeData || !episodeData.Episode) return;
    setIsSaved(episodeData.Episode.IsSavedByCurrentUser);
  }, [episodeData, isEpisodeLoading]);

  const handleSaveEpisode = async () => {
    if (!episodeData || !episodeData.Episode) return;
    const snapShotValue = episodeData.Episode.IsSavedByCurrentUser;
    setIsSaved(!episodeData.Episode.IsSavedByCurrentUser);
    try {
      await saveEpisode({
        PodcastEpisodeId: episodeData.Episode.Id,
        IsSave: !episodeData.Episode.IsSavedByCurrentUser,
      }).unwrap();

      refetchEpisode();
    } catch (error) {
      Alert.alert("Failed to update saved status. Please try again later.");
      setIsSaved(snapShotValue);
    }
  };

  const handleShowReportModal = async () => {
    // GỌI API ĐỂ LẤY DANH SÁCH REPORT REASON
    if (!user) {
      dispatch(
        setDataAndShowAlert({
          type: "info",
          title: "You need to log in to report",
          description: "Please log in to your account to report this episode.",
          isCloseable: true,
          isFunctional: false,
        })
      );
      return;
    }
    try {
      const response = await getReportReasons({
        PodcastEpisodeId: id!,
      }).unwrap();
      if (response.EpisodeReportTypeList.length === 0) {
        dispatch(
          setDataAndShowAlert({
            type: "info",
            title: "You have already reported this episode",
            description:
              "You have already reported this episode. Our team will review it shortly.",
            isCloseable: true,
            isFunctional: false,
          })
        );
        return;
      } else {
        setAvaiableReportTypes(response.EpisodeReportTypeList);
        setSelectedReportReason(response.EpisodeReportTypeList[0]);
        setIsReportModalVisible(true);
      }
    } catch (error) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          title: "Failed to fetch report reasons",
          description: "Please try again later.",
          isCloseable: true,
          isFunctional: false,
        })
      );
    }
  };

  const handleSubmitReport = async () => {
    if (!selectedReportReason) return;

    // Nếu chọn "Other" (Id = 8) mà chưa nhập custom reason
    if (selectedReportReason.Id === 8 && !customReportReason.trim()) {
      dispatch(
        setDataAndShowAlert({
          type: "warning",
          title: "Please provide a reason",
          description: "Please enter your reason for reporting this episode.",
          isCloseable: true,
          isFunctional: false,
        })
      );
      return;
    }

    try {
      await reportEpisode({
        PodcastEpisodeId: id!,
        Content:
          selectedReportReason.Id === 8
            ? customReportReason.trim()
            : selectedReportReason.Name,
        ReportTypeId: selectedReportReason.Id,
      }).unwrap();
      setIsReportModalVisible(false);
      setCustomReportReason(""); // Reset custom reason
      registerAlertAction("refetch-episode-after-report", async () => {
        await refetchEpisode();
      });
      dispatch(
        setDataAndShowAlert({
          type: "success",
          title: "Report submitted successfully",
          description:
            "Thank you for helping us keep the community safe. Our team will review the episode shortly.",
          isCloseable: true,
          isFunctional: true,
          actionId: "refetch-episode-after-report",
          functionalButtonText: "OK",
        })
      );
    } catch (error) {
      dispatch(
        setDataAndShowAlert({
          type: "error",
          title: "Failed to submit report",
          description: "Please try again later.",
          isCloseable: true,
          isFunctional: false,
        })
      );
    }
  };

  if (isEpisodeLoading) {
    return (
      <View className="w-full h-screen items-center justify-center gap-5">
        <ActivityIndicator size="large" color="#AEE339" />
        <Text>Loading ...</Text>
      </View>
    );
  }

  if (!episodeData) {
    return (
      <View className="w-full h-screen items-center justify-center gap-3">
        <Text className="text-gray-500 font-bold">
          Can't Find This Episode Due To Some Reasons :(
        </Text>
        <Pressable onPress={() => router.back()}>
          <Text className="text-[#AEE339] underline">Go back</Text>
        </Pressable>
      </View>
    );
  }
  return (
    <ScrollView showsVerticalScrollIndicator={false}>
      <EpisodeInformations
        onSaveToggle={handleSaveEpisode}
        isSaved={isSaved}
        episode={episodeData.Episode}
        onReport={handleShowReportModal}
      />
      <EpisodeDescription description={episodeData.Episode.Description} />
      <EpisodeMoreInformations episode={episodeData.Episode} />
      <View className="h-52" />

      <Modal
        transparent={true}
        visible={isReportModalVisible}
        animationType="fade"
        onRequestClose={() => setIsReportModalVisible(false)}
      >
        {/* Overlay tối */}
        <Pressable
          style={styles.overlay}
          onPress={() => setIsReportModalVisible(false)} // bấm ngoài để đóng
        >
          {/* Chặn sự kiện bấm lan xuống overlay */}
          <Pressable style={styles.dialog} onPress={() => {}}>
            <Text style={styles.reportTitle} numberOfLines={1}>
              Report Episode
            </Text>
            <Text style={styles.message}>
              Thank you for helping us keep the community safe. Please select
              the reason for reporting this episode.
            </Text>

            {/* Report Reasons List */}
            <ScrollView
              style={styles.reportReasonsContainer}
              showsVerticalScrollIndicator={false}
            >
              {avaiableReportTypes.map((reportType) => (
                <Pressable
                  key={reportType.Id}
                  style={[
                    styles.reportReasonItem,
                    selectedReportReason?.Id === reportType.Id &&
                      styles.selectedReportReasonItem,
                  ]}
                  onPress={() => setSelectedReportReason(reportType)}
                >
                  <View style={styles.radioOuter}>
                    {selectedReportReason?.Id === reportType.Id && (
                      <View style={styles.radioInner} />
                    )}
                  </View>
                  <Text style={styles.reportReasonText}>{reportType.Name}</Text>
                </Pressable>
              ))}
            </ScrollView>

            {/* Custom Reason Input - Show only when "Other" (Id = 8) is selected */}
            {selectedReportReason?.Id === 8 && (
              <TextInput
                style={styles.customReasonInput}
                placeholder="Please enter your reason..."
                placeholderTextColor="#666"
                value={customReportReason}
                onChangeText={setCustomReportReason}
                multiline
                numberOfLines={3}
                maxLength={500}
              />
            )}

            {/* Action Buttons */}
            <View style={styles.buttonsRow}>
              <Pressable
                style={[styles.button, styles.cancelButton]}
                onPress={() => setIsReportModalVisible(false)}
              >
                <Text style={styles.cancelText}>Cancel</Text>
              </Pressable>
              <Pressable
                style={[styles.button, styles.confirmButton]}
                onPress={handleSubmitReport}
                disabled={!selectedReportReason}
              >
                <Text style={styles.confirmText}>
                  {isReporting ? "Reporting..." : "Submit Report"}
                </Text>
              </Pressable>
            </View>
          </Pressable>
        </Pressable>
      </Modal>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.45)",
    justifyContent: "center",
    alignItems: "center",
  },
  dialog: {
    width: "80%",
    padding: 16,
    borderRadius: 20,
    backgroundColor: "#000",
  },
  reportTitle: {
    fontSize: 18,
    fontWeight: "700",
    color: "#aee339",
    marginBottom: 6,
  },
  subscriptionContentContainer: {
    width: "100%",
    marginLeft: 2,
    marginRight: 2,
    backgroundColor: "#1f2937",
  },
  message: {
    fontSize: 14,
    color: "#e5e7eb",
    textAlign: "left",
    marginBottom: 16,
  },
  reportReasonsContainer: {
    maxHeight: 200,
    marginBottom: 16,
  },
  reportReasonItem: {
    flexDirection: "row",
    alignItems: "center",
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderRadius: 10,
    backgroundColor: "#000",
    marginBottom: 8,
    gap: 12,
  },
  selectedReportReasonItem: {
    backgroundColor: "#000",
  },
  radioOuter: {
    width: 20,
    height: 20,
    borderRadius: 10,
    borderWidth: 2,
    borderColor: "#fff",
    alignItems: "center",
    justifyContent: "center",
  },
  radioInner: {
    width: 10,
    height: 10,
    borderRadius: 5,
    backgroundColor: "#aee339",
  },
  reportReasonText: {
    fontSize: 14,
    color: "#fff",
    flex: 1,
  },
  customReasonInput: {
    backgroundColor: "#000",
    borderWidth: 1,
    borderColor: "#fff",
    borderRadius: 10,
    padding: 12,
    color: "#fff",
    fontSize: 14,
    minHeight: 80,
    textAlignVertical: "top",
    marginBottom: 16,
  },
  buttonsRow: {
    flexDirection: "row",
    justifyContent: "flex-end",
    gap: 10,
    marginTop: 10,
  },
  button: {
    flex: 1,
    paddingVertical: 10,
    paddingHorizontal: 14,
    borderRadius: 10,
    alignItems: "center",
  },
  normalCycleButton: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#aee339",
  },
  normalCycleText: {
    color: "#aee339",
  },
  selectedCyclePriceText: {
    fontSize: 40,
    lineHeight: 40, // quan trọng: cho lineHeight = fontSize
    fontWeight: "600",
    color: "#aee339",
  },
  activeCycleButton: {
    paddingHorizontal: 12,
    paddingVertical: 6,
    borderRadius: 10,
    borderWidth: 1,
    borderColor: "#aee339",
    backgroundColor: "#aee339",
  },
  activeCycleText: {
    color: "black",
  },
  cancelButton: {
    backgroundColor: "#374151",
  },
  confirmButton: {
    backgroundColor: "#aee339",
  },
  cancelText: {
    color: "#e5e7eb",
    fontSize: 14,
    fontWeight: "600",
  },
  confirmText: {
    color: "#000",
    fontWeight: "600",
    fontSize: 14,
  },
  subscriptionButton: {
    backgroundColor: "#aee339",
  },
  subscriptionText: {
    color: "black",
    fontWeight: "600",
    fontSize: 14,
  },
});
