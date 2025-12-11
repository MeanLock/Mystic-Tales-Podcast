import { useEffect, useRef, useState } from "react";
import {
  View,
  Text,
  ScrollView,
  Image,
  TouchableOpacity,
  TextInput,
  ActivityIndicator,
  Alert,
  Modal,
  StyleSheet,
  Platform,
} from "react-native";
import { useDispatch } from "react-redux";
import { Ionicons } from "@expo/vector-icons";
import { router, useRouter } from "expo-router";
import * as ImagePicker from "expo-image-picker";
import { User } from "@/src/types/user";
import { logoutLocal } from "@/src/features/auth/authSlice";
import {
  useUpdateAccountInformationsMutation,
  useUpdateAccountMeQuery,
} from "@/src/core/services/account/account.service";
import MixxingText from "@/src/components/ui/MixxingText";
import DateTimePicker from "@react-native-community/datetimepicker";
import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";

type UpdateInformationsForm = {
  AccountUpdateInfo: {
    FullName: string;
    Dob: string; // ISO String
    Gender: string;
    Address: string;
    Phone: string;
  };
  MainImageFile: { uri: string; type: string; name: string } | null;
};

type AccountUpdateInfoErrors = Partial<
  Record<keyof UpdateInformationsForm["AccountUpdateInfo"], string>
>;

const ProfilePage = () => {
  const [accountInformation, setAccountInformation] = useState<any | null>(
    null
  );
  const [tempUploadedImageUri, setTempUploadedImageUri] = useState<
    string | null
  >(null);
  const [showDatePicker, setShowDatePicker] = useState(false);
  const [isResetPasswordOpen, setIsResetPasswordOpen] =
    useState<boolean>(false);
  const [oldPassword, setOldPassword] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");
  const [resetPasswordError, setResetPasswordError] = useState<string>("");
  const [resetPasswordSuccess, setResetPasswordSuccess] = useState<string>("");

  const {
    data: dataAccount,
    isLoading: isAccountMeLoading,
    isError,
    refetch,
  } = useUpdateAccountMeQuery();

  const [isLoading, setIsLoading] = useState<boolean>(false);
  const dispatch = useDispatch();
  // const [resetPassword, { isLoading: isResettingPassword }] =
  //   useUpdatePasswordMutation();

  const [
    updateAccountInformations,
    {
      isLoading: isUpdating,
      isError: isUpdateError,
      error: updateError,
      isSuccess: isUpdateSuccess,
    },
  ] = useUpdateAccountInformationsMutation();

  const [formState, setFormState] = useState<UpdateInformationsForm | null>(
    null
  );
  const [isEditing, setIsEditing] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<AccountUpdateInfoErrors>({});
  const [updateErrorMessage, setUpdateErrorMessage] = useState<string | null>(
    null
  );
  const [updateSuccessMessage, setUpdateSuccessMessage] = useState<
    string | null
  >(null);

  const fileInputRef = useRef<HTMLInputElement | null>(null);

  // Lấy data account
  useEffect(() => {
    if (dataAccount) {
      const parsedData = dataAccount.Account;
      setAccountInformation(parsedData);

      // init form từ data api nếu chưa có
      setFormState({
        AccountUpdateInfo: {
          FullName: parsedData.FullName ?? "",
          Dob: parsedData.Dob ?? "",
          Gender: parsedData.Gender ?? "Other",
          Address: parsedData.Address ?? "",
          Phone: parsedData.Phone ?? "",
        },
        MainImageFile: null,
      });
    }
  }, [dataAccount]);

  // FUNCTIONS
  const handleLogout = () => {
    dispatch(logoutLocal());
    router.replace("/(auth)/login");
  };

  const resetFormFromAccountInfo = () => {
    if (!accountInformation) return;
    setFormState({
      AccountUpdateInfo: {
        FullName: accountInformation.FullName ?? "",
        Dob: accountInformation.Dob ?? "",
        Gender: accountInformation.Gender ?? "Other",
        Address: accountInformation.Address ?? "",
        Phone: accountInformation.Phone ?? "",
      },
      MainImageFile: null,
    });
    setFieldErrors({});
  };

  const handleToggleEdit = () => {
    if (!isEditing) {
      // bật edit thì sync lại form
      resetFormFromAccountInfo();
    }
    setIsEditing((prev) => !prev);
    setUpdateErrorMessage(null);
    setUpdateSuccessMessage(null);
  };
  const handleDateChange = (event: any, selectedDate?: Date) => {
    if (event.type === "set" && selectedDate) {
      const isoString = selectedDate.toISOString();
      handleInputChange("Dob", isoString);
    }
  };
  const getDateValue = () => {
    const dobIso = formState?.AccountUpdateInfo.Dob;
    if (!dobIso) return new Date();
    return new Date(dobIso);
  };
  const handleUploadAvatarClick = async () => {
    const permissionResult =
      await ImagePicker.requestMediaLibraryPermissionsAsync();

    if (!permissionResult.granted) {
      Alert.alert(
        "Permission Required",
        "Please grant camera roll permissions to upload an avatar."
      );
      return;
    }

    const result = await ImagePicker.launchImageLibraryAsync({
      mediaTypes: ImagePicker.MediaTypeOptions.Images,
      allowsEditing: true,
      aspect: [1, 1],
      quality: 0.8,
    });

    if (!result.canceled && result.assets[0]) {
      const asset = result.assets[0];

      // Set temp image immediately for UI feedback
      setTempUploadedImageUri(asset.uri);

      const file = {
        uri: asset.uri,
        type: "image/jpeg",
        name: `avatar_${Date.now()}.jpg`,
        size: asset.fileSize,
      };
      await handleSaveEdit({ onlyAvatar: true, file });
    }
  };

  const handleApplyToBePodcaster = () => {
    // TODO: implement apply podcaster
  };

  const handleInputChange = (
    field: keyof UpdateInformationsForm["AccountUpdateInfo"],
    value: string
  ) => {
    if (!formState) return;
    setFormState((prev) =>
      prev
        ? {
            ...prev,
            AccountUpdateInfo: {
              ...prev.AccountUpdateInfo,
              [field]: value,
            },
          }
        : prev
    );
    setFieldErrors((prev) => ({ ...prev, [field]: undefined }));
  };

  const validateForm = (
    values: UpdateInformationsForm["AccountUpdateInfo"]
  ): AccountUpdateInfoErrors => {
    const errors: AccountUpdateInfoErrors = {};

    if (!values.FullName.trim()) {
      errors.FullName = "Full name is required.";
    } else if (values.FullName.trim().length < 2) {
      errors.FullName = "Full name must be at least 2 characters.";
    }

    if (!values.Phone.trim()) {
      errors.Phone = "Phone is required.";
    } else if (!/^[0-9+\-\s]{8,20}$/.test(values.Phone.trim())) {
      errors.Phone = "Phone format is not valid.";
    }

    if (!values.Gender) {
      errors.Gender = "Gender is required.";
    } else if (!["Male", "Female", "Other"].includes(values.Gender)) {
      errors.Gender = "Gender is not valid.";
    }

    if (values.Dob) {
      const dobDate = new Date(values.Dob);
      if (Number.isNaN(dobDate.getTime())) {
        errors.Dob = "Date of birth is not valid.";
      } else {
        const now = new Date();
        if (dobDate > now) {
          errors.Dob = "Date of birth cannot be in the future.";
        }
      }
    }

    return errors;
  };

  /**
   * options.onlyAvatar = true → chỉ update avatar, AccountUpdateInfo lấy giá trị cũ
   * options.file = file avatar mới
   */
  const handleSaveEdit = async (options?: {
    onlyAvatar?: boolean;
    file?: File | { uri: string; type: string; name: string; size?: number };
  }) => {
    if (!accountInformation) return;

    setUpdateErrorMessage(null);
    setUpdateSuccessMessage(null);

    let accountUpdateInfo: UpdateInformationsForm["AccountUpdateInfo"];

    if (options?.onlyAvatar || !formState) {
      // dùng giá trị cũ
      accountUpdateInfo = {
        FullName: accountInformation.FullName ?? "",
        Dob: accountInformation.Dob ?? "",
        Gender: accountInformation.Gender ?? "Other",
        Address: accountInformation.Address ?? "",
        Phone: accountInformation.Phone ?? "",
      };
    } else {
      // merge form với giá trị cũ
      const merged: UpdateInformationsForm["AccountUpdateInfo"] = {
        FullName:
          formState.AccountUpdateInfo.FullName ||
          accountInformation.FullName ||
          "",
        Dob: formState.AccountUpdateInfo.Dob || accountInformation.Dob || "",
        Gender:
          formState.AccountUpdateInfo.Gender ||
          accountInformation.Gender ||
          "Other",
        Address:
          formState.AccountUpdateInfo.Address ||
          accountInformation.Address ||
          "",
        Phone:
          formState.AccountUpdateInfo.Phone || accountInformation.Phone || "",
      };

      // validate
      const errors = validateForm(merged);
      if (Object.keys(errors).length > 0) {
        setFieldErrors(errors);
        return;
      }

      accountUpdateInfo = merged;
    }

    const formData = new FormData();
    formData.append("AccountUpdateInfo", JSON.stringify(accountUpdateInfo));

    // MainImageFile: ưu tiên file trong options (upload avatar),
    // nếu không có thì lấy trong formState (trường hợp về sau cho phép edit avatar trong form)
    const mainImageFile = options?.file || formState?.MainImageFile || null;

    if (mainImageFile) {
      setIsLoading(true);
      formData.append("MainImageFile", mainImageFile as any);
    }

    try {
      const response = await updateAccountInformations({
        uploadAccountInformationsFormData: formData,
        accountId: accountInformation.Id,
      }).unwrap();

      // refetch lại thông tin TRƯỚC KHI hiện alert
      await refetch();

      // Clear temp image after successful update
      setTempUploadedImageUri(null);

      Alert.alert("Success", response?.Message || "Update successfully.");
      setFieldErrors({});
      setUpdateErrorMessage(null);

      // tắt mode edit nếu đang edit
      if (!options?.onlyAvatar) {
        setIsEditing(false);
      }

      // clear file trong form
      setFormState((prev) =>
        prev
          ? {
              ...prev,
              MainImageFile: null,
            }
          : prev
      );
    } catch (error: any) {
      console.error("Failed to update account information:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Failed to update account information.";
      Alert.alert("Error", msg);
      setUpdateErrorMessage(msg);

      // Clear temp image on error
      setTempUploadedImageUri(null);
    } finally {
      setIsLoading(false);
    }
  };

  const getDobDisplayValue = () => {
    const dobIso = formState?.AccountUpdateInfo.Dob || accountInformation?.Dob;
    if (!dobIso) return "Not Updated";
    return new Date(dobIso).toLocaleDateString("en-GB");
  };

  const handleResetPassword = () => {
    setIsResetPasswordOpen(true);
    setOldPassword("");
    setNewPassword("");
    setConfirmPassword("");
    setResetPasswordError("");
    setResetPasswordSuccess("");
  };

  // const handleConfirmResetPassword = async () => {
  //   setResetPasswordError("");
  //   setResetPasswordSuccess("");

  //   // Validation
  //   if (!oldPassword.trim()) {
  //     setResetPasswordError("Old password is required.");
  //     return;
  //   }

  //   if (!newPassword.trim()) {
  //     setResetPasswordError("New password is required.");
  //     return;
  //   }

  //   if (newPassword.length < 8) {
  //     setResetPasswordError("New password must be at least 8 characters.");
  //     return;
  //   }

  //   if (newPassword !== confirmPassword) {
  //     setResetPasswordError("New password and confirm password do not match.");
  //     return;
  //   }

  //   if (oldPassword === newPassword) {
  //     setResetPasswordError(
  //       "New password must be different from old password."
  //     );
  //     return;
  //   }

  //   try {
  //     const result = await resetPassword({
  //       CurrentPassword: oldPassword,
  //       NewPassword: newPassword,
  //     }).unwrap();

  //     Alert.alert(
  //       "Success",
  //       result?.Message || "Password reset successfully!",
  //       [
  //         {
  //           text: "OK",
  //           onPress: () => {
  //             setIsResetPasswordOpen(false);
  //             setOldPassword("");
  //             setNewPassword("");
  //             setConfirmPassword("");
  //             setResetPasswordError("");
  //             setResetPasswordSuccess("");

  //             // Clear auth and navigate to login
  //             dispatch(clearAuth());
  //             router.replace("/(auth)/login");
  //           },
  //         },
  //       ]
  //     );
  //   } catch (error: any) {
  //     console.error("Failed to reset password:", error);
  //     const msg =
  //       error?.data?.Message ||
  //       error?.message ||
  //       "Failed to reset password. Please try again.";
  //     Alert.alert("Error", msg);
  //     setResetPasswordError(msg);
  //   }
  // };

  if (isAccountMeLoading) {
    return (
      <View style={styles.centerContainer}>
        <ActivityIndicator size="large" color="#aee339" />
        <Text style={styles.loadingText}>Loading...</Text>
      </View>
    );
  }

  if (isError) {
    return (
      <View style={styles.centerContainer}>
        <Text style={styles.errorText}>
          Cannot load your profile. Please try again later.
        </Text>
        <TouchableOpacity style={styles.logoutButton} onPress={handleLogout}>
          <Ionicons name="log-out-outline" size={20} color="white" />
          <Text style={styles.buttonText}>Logout</Text>
        </TouchableOpacity>
      </View>
    );
  }

  return (
    <ScrollView
      style={styles.container}
      contentContainerStyle={styles.contentContainer}
    >
      <View style={{ height: 50 }} />
      <MixxingText
        style={styles.title}
        originalText="Your Profile"
        coloredText="Profile"
      />
      <View style={styles.section}>
        <View style={styles.avatarCard}>
          {tempUploadedImageUri ? (
            // Show temp uploaded image with loading overlay
            <View style={styles.avatarPlaceholderContainer}>
              <Image
                source={{ uri: tempUploadedImageUri }}
                style={styles.avatar}
              />
              <View
                style={[
                  styles.avatarOverlay,
                  { backgroundColor: "rgba(0, 0, 0, 0.4)" },
                ]}
              >
                <ActivityIndicator size="small" color="#aee339" />
                <Text
                  style={[
                    styles.avatarOverlayText,
                    { fontSize: 12, marginTop: 8 },
                  ]}
                >
                  Uploading...
                </Text>
              </View>
            </View>
          ) : accountInformation?.MainImageFileKey ? (
            <AutoResolvingImage
              key={`avatar-${accountInformation.MainImageFileKey}-${accountInformation.Id}`}
              FileKey={accountInformation.MainImageFileKey}
              type="AccountPublicSource"
              style={styles.avatar}
            />
          ) : (
            <View style={styles.avatarPlaceholderContainer}>
              <Image
                source={require("@/assets/images/user/unknown.jpg")}
                style={styles.avatar}
              />
            </View>
          )}

          <View style={styles.avatarInfo}>
            <TouchableOpacity
              style={styles.uploadButton}
              onPress={handleUploadAvatarClick}
              disabled={isUpdating || isLoading}
            >
              <Ionicons name="cloud-upload-outline" size={20} color="#aee339" />
              <Text style={styles.buttonTextGreen}>
                {isLoading ? "Uploading..." : "Upload New Avatar"}
              </Text>
            </TouchableOpacity>
          </View>
        </View>

        {/* Personal Info */}
        <View style={styles.infoCard}>
          <View style={styles.infoHeader}>
            <Text style={styles.sectionTitle}>Personal Info</Text>

            {!isEditing ? (
              <TouchableOpacity
                style={styles.editButton}
                onPress={handleToggleEdit}
              >
                <Ionicons name="create-outline" size={20} color="black" />
                <Text style={styles.buttonText}>Edit Profile</Text>
              </TouchableOpacity>
            ) : (
              <View style={styles.editActions}>
                <TouchableOpacity
                  onPress={handleToggleEdit}
                  disabled={isUpdating}
                  style={[
                    styles.cancelButton,
                    isUpdating && styles.disabledButton,
                  ]}
                >
                  <Text style={styles.cancelButtonText}>Cancel</Text>
                </TouchableOpacity>
                <TouchableOpacity
                  onPress={() => handleSaveEdit()}
                  disabled={isUpdating}
                  style={[
                    styles.saveButton,
                    isUpdating && styles.disabledButton,
                  ]}
                >
                  <Text style={styles.saveButtonText}>
                    {isUpdating ? "Saving..." : "Save Changes"}
                  </Text>
                </TouchableOpacity>
              </View>
            )}
          </View>

          {updateErrorMessage && (
            <Text style={styles.errorMessage}>{updateErrorMessage}</Text>
          )}

          <View style={styles.fieldsContainer}>
            {/* Full Name */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Full Name</Text>
              {isEditing ? (
                <>
                  <TextInput
                    style={styles.input}
                    value={formState?.AccountUpdateInfo.FullName ?? ""}
                    onChangeText={(text) => handleInputChange("FullName", text)}
                    placeholderTextColor="#666"
                  />
                  {fieldErrors.FullName && (
                    <Text style={styles.fieldError}>
                      {fieldErrors.FullName}
                    </Text>
                  )}
                </>
              ) : (
                <Text style={styles.fieldValue}>
                  {accountInformation?.FullName}
                </Text>
              )}
            </View>

            {/* Email - chỉ view */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Email</Text>
              <Text style={styles.fieldValue}>{accountInformation?.Email}</Text>
            </View>

            {/* Phone */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Phone</Text>
              {isEditing ? (
                <>
                  <TextInput
                    style={styles.input}
                    value={formState?.AccountUpdateInfo.Phone ?? ""}
                    onChangeText={(text) => handleInputChange("Phone", text)}
                    keyboardType="phone-pad"
                    placeholderTextColor="#666"
                  />
                  {fieldErrors.Phone && (
                    <Text style={styles.fieldError}>{fieldErrors.Phone}</Text>
                  )}
                </>
              ) : (
                <Text style={styles.fieldValue}>
                  {accountInformation?.Phone}
                </Text>
              )}
            </View>

            {/* Gender */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Gender</Text>
              {isEditing ? (
                <>
                  <View style={styles.pickerContainer}>
                    <TouchableOpacity
                      style={styles.pickerButton}
                      onPress={() => {
                        Alert.alert("Select Gender", "", [
                          {
                            text: "Male",
                            onPress: () => handleInputChange("Gender", "Male"),
                          },
                          {
                            text: "Female",
                            onPress: () =>
                              handleInputChange("Gender", "Female"),
                          },
                          {
                            text: "Other",
                            onPress: () => handleInputChange("Gender", "Other"),
                          },
                          { text: "Cancel", style: "cancel" },
                        ]);
                      }}
                    >
                      <Text style={styles.pickerText}>
                        {formState?.AccountUpdateInfo.Gender ?? "Other"}
                      </Text>
                      <Ionicons name="chevron-down" size={20} color="#fff" />
                    </TouchableOpacity>
                  </View>
                  {fieldErrors.Gender && (
                    <Text style={styles.fieldError}>{fieldErrors.Gender}</Text>
                  )}
                </>
              ) : (
                <Text style={styles.fieldValue}>
                  {accountInformation?.Gender}
                </Text>
              )}
            </View>

            {/* DOB */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Date Of Birth</Text>
              {isEditing ? (
                <>
                  <TouchableOpacity
                    style={styles.input}
                    onPress={() => setShowDatePicker(!showDatePicker)}
                  >
                    <View
                      style={{
                        flexDirection: "row",
                        justifyContent: "space-between",
                        alignItems: "center",
                      }}
                    >
                      <Text style={styles.fieldValue}>
                        {getDobDisplayValue()}
                      </Text>
                      <Ionicons
                        name="calendar-outline"
                        size={20}
                        color="#aee339"
                      />
                    </View>
                  </TouchableOpacity>

                  {showDatePicker && (
                    <DateTimePicker
                      value={getDateValue()}
                      mode="date"
                      display={Platform.OS === "ios" ? "spinner" : "default"}
                      onChange={handleDateChange}
                      maximumDate={new Date()}
                      minimumDate={new Date(1900, 0, 1)}
                      textColor="#fff"
                      themeVariant="dark"
                    />
                  )}

                  {fieldErrors.Dob && (
                    <Text style={styles.fieldError}>{fieldErrors.Dob}</Text>
                  )}
                </>
              ) : (
                <Text style={styles.fieldValue}>{getDobDisplayValue()}</Text>
              )}
            </View>

            {/* Address */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Address</Text>
              {isEditing ? (
                <>
                  <TextInput
                    style={[styles.input, styles.textArea]}
                    value={formState?.AccountUpdateInfo.Address ?? ""}
                    onChangeText={(text) => handleInputChange("Address", text)}
                    multiline
                    numberOfLines={3}
                    placeholderTextColor="#666"
                  />
                  {fieldErrors.Address && (
                    <Text style={styles.fieldError}>{fieldErrors.Address}</Text>
                  )}
                </>
              ) : (
                <Text style={styles.fieldValue}>
                  {accountInformation?.Address || "Not updated"}
                </Text>
              )}
            </View>

            {/* Balance */}
            <View style={styles.fieldWrapper}>
              <Text style={styles.fieldLabel}>Account Balance</Text>
              <Text style={styles.fieldValue}>
                {accountInformation?.Balance?.toLocaleString()} Coins
              </Text>
            </View>
          </View>
        </View>

        {/* bottom actions */}
        <View style={styles.actionsContainer}>
          {!accountInformation?.IsPodcaster && (
            <TouchableOpacity
              style={[styles.actionButton, { backgroundColor: "#4F8BFF" }]}
              onPress={handleApplyToBePodcaster}
            >
              <Ionicons name="mic-outline" size={20} color="white" />
              <Text style={[styles.buttonText, { color: "#fff" }]}>
                Apply to be Podcaster
              </Text>
            </TouchableOpacity>
          )}

          <TouchableOpacity
            style={[styles.actionButton, { backgroundColor: "#e67615ff" }]}
            onPress={handleResetPassword}
          >
            <Ionicons name="lock-closed-outline" size={20} color="white" />
            <Text style={[styles.buttonText, { color: "#fff" }]}>
              Reset Password
            </Text>
          </TouchableOpacity>

          <TouchableOpacity
            style={[styles.actionButton, { backgroundColor: "#dc2636ff" }]}
            onPress={handleLogout}
          >
            <Ionicons name="log-out-outline" size={20} color="white" />
            <Text style={[styles.buttonText, { color: "#fff" }]}>Logout</Text>
          </TouchableOpacity>
        </View>
        <View style={{ height: 30 }} />
      </View>

      {/* Reset Password Modal */}
      <Modal
        visible={isResetPasswordOpen}
        transparent
        animationType="fade"
        onRequestClose={() => {
          setIsResetPasswordOpen(false);
          setOldPassword("");
          setNewPassword("");
          setConfirmPassword("");
          setResetPasswordError("");
          setResetPasswordSuccess("");
        }}
      >
        <View style={styles.modalOverlay}>
          <View style={styles.modalContent}>
            <View className="flex flex-row justify-between">
              <Text style={styles.modalTitle}>Reset Password</Text>
              <Ionicons
                name="close"
                size={18}
                color="#aee339"
                onPress={() => {
                  setIsResetPasswordOpen(false);
                }}
              />
            </View>
            <Text style={styles.modalDescription}>
              Enter your old password and choose a new password.
            </Text>

            <View style={styles.modalForm}>
              {/* Old Password */}
              <View style={styles.modalField}>
                <Text style={styles.modalLabel}>Old Password</Text>
                <TextInput
                  style={styles.modalInput}
                  placeholder="Enter old password"
                  placeholderTextColor="#666"
                  value={oldPassword}
                  onChangeText={setOldPassword}
                  secureTextEntry
                />
              </View>

              {/* New Password */}
              <View style={styles.modalField}>
                <Text style={styles.modalLabel}>New Password</Text>
                <TextInput
                  style={styles.modalInput}
                  placeholder="Enter new password (min 8 characters)"
                  placeholderTextColor="#666"
                  value={newPassword}
                  onChangeText={setNewPassword}
                  secureTextEntry
                />
              </View>

              {/* Confirm Password */}
              <View style={styles.modalField}>
                <Text style={styles.modalLabel}>Confirm New Password</Text>
                <TextInput
                  style={styles.modalInput}
                  placeholder="Re-enter new password"
                  placeholderTextColor="#666"
                  value={confirmPassword}
                  onChangeText={setConfirmPassword}
                  secureTextEntry
                />
              </View>

              {/* Error/Success Messages */}
              {resetPasswordError && (
                <Text style={styles.errorMessage}>{resetPasswordError}</Text>
              )}
              {resetPasswordSuccess && (
                <Text style={styles.successMessage}>
                  {resetPasswordSuccess}
                </Text>
              )}
            </View>

            <View style={styles.modalFooter}>
              {/* <TouchableOpacity
                style={styles.modalCancelButton}
                onPress={() => setIsResetPasswordOpen(false)}
                disabled={isResettingPassword}
              >
                <Text style={styles.modalCancelText}>Cancel</Text>
              </TouchableOpacity> */}
              {/* <TouchableOpacity
                style={[styles.modalConfirmButton, isResettingPassword && styles.disabledButton]}
                onPress={handleConfirmResetPassword}
                disabled={isResettingPassword}
              >
                <Text style={styles.modalConfirmText}>
                  {isResettingPassword ? "Resetting..." : "Reset Password"}
                </Text>
              </TouchableOpacity> */}
              {/* Thịnh để tạm sau này có thì mở cái trên */}
              <TouchableOpacity style={[styles.modalConfirmButton]}>
                <Text style={styles.modalConfirmText}>Reset Password</Text>
              </TouchableOpacity>
            </View>
          </View>
        </View>
      </Modal>
      <View style={{ height: 50 }} />
    </ScrollView>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: "#000",
  },
  contentContainer: {
    padding: 20,
  },
  centerContainer: {
    flex: 1,
    justifyContent: "center",
    alignItems: "center",
    backgroundColor: "#000",
    gap: 20,
  },
  title: {
    fontSize: 32,
    fontWeight: "bold",
    color: "#fff",
    marginBottom: 20,
  },
  loadingText: {
    fontSize: 18,
    fontWeight: "bold",
    color: "#d9d9d9",
  },
  errorText: {
    fontSize: 16,
    color: "#f93a3aff",
    textAlign: "center",
    marginBottom: 20,
  },
  section: {
    gap: 15,
  },
  avatarCard: {
    padding: 20,
    flexDirection: "row",
    alignItems: "center",
    gap: 20,
    backgroundColor: "rgba(255, 255, 255, 0.1)",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    borderRadius: 8,
  },
  avatar: {
    width: 120,
    height: 120,
    borderRadius: 60,
  },
  avatarPlaceholderContainer: {
    position: "relative",
  },
  avatarOverlay: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: "rgba(107, 114, 128, 0.5)",
    borderRadius: 60,
    justifyContent: "center",
    alignItems: "center",
  },
  avatarOverlayText: {
    color: "#fff",
    fontWeight: "bold",
    textAlign: "center",
    paddingHorizontal: 10,
  },
  avatarInfo: {
    flex: 1,
    gap: 8,
  },
  uploadButton: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    borderWidth: 2,
    borderColor: "#aee339",
    paddingVertical: 10,
    paddingHorizontal: 16,
    borderRadius: 20,
    alignSelf: "flex-start",
  },
  buttonTextGreen: {
    color: "#aee339",
    fontSize: 14,
    fontWeight: "600",
  },
  buttonText: {
    color: "#000",
    fontSize: 14,
    fontWeight: "600",
  },
  infoText: {
    color: "#d9d9d9",
    fontSize: 12,
  },
  highlightText: {
    color: "#00D9A5",
  },
  infoCard: {
    padding: 20,
    backgroundColor: "rgba(255, 255, 255, 0.1)",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    borderRadius: 8,
    gap: 15,
  },
  infoHeader: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    marginBottom: 10,
  },
  sectionTitle: {
    fontSize: 20,
    fontWeight: "bold",
    color: "#fff",
  },
  editButton: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    backgroundColor: "#aee339",
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 6,
  },
  editActions: {
    flexDirection: "row",
    gap: 12,
  },
  cancelButton: {
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.3)",
  },
  cancelButtonText: {
    color: "rgba(255, 255, 255, 0.8)",
    fontSize: 14,
  },
  saveButton: {
    paddingVertical: 8,
    paddingHorizontal: 16,
    borderRadius: 6,
    backgroundColor: "#aee339",
  },
  saveButtonText: {
    color: "#000",
    fontSize: 14,
    fontWeight: "600",
  },
  disabledButton: {
    opacity: 0.5,
  },
  errorMessage: {
    color: "#f87171",
    fontSize: 12,
    marginTop: 5,
  },
  successMessage: {
    color: "#10b981",
    fontSize: 12,
    marginTop: 5,
  },
  fieldsContainer: {
    gap: 15,
  },
  fieldWrapper: {
    gap: 8,
  },
  fieldLabel: {
    color: "#a1a1a1",
    fontSize: 14,
    fontWeight: "bold",
  },
  fieldValue: {
    color: "#fff",
    fontSize: 14,
  },
  input: {
    backgroundColor: "rgba(255, 255, 255, 0.05)",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    borderRadius: 6,
    paddingHorizontal: 12,
    paddingVertical: 10,
    color: "#fff",
    fontSize: 14,
  },
  textArea: {
    minHeight: 80,
    textAlignVertical: "top",
  },
  fieldError: {
    color: "#f87171",
    fontSize: 12,
  },
  pickerContainer: {
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    borderRadius: 6,
    backgroundColor: "rgba(255, 255, 255, 0.05)",
  },
  pickerButton: {
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    paddingHorizontal: 12,
    paddingVertical: 10,
  },
  pickerText: {
    color: "#fff",
    fontSize: 14,
  },
  actionsContainer: {
    marginTop: 10,
    gap: 10,
  },
  actionButton: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "center",
    gap: 8,
    paddingVertical: 12,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  logoutButton: {
    flexDirection: "row",
    alignItems: "center",
    gap: 8,
    backgroundColor: "#E06C75",
    paddingVertical: 12,
    paddingHorizontal: 20,
    borderRadius: 8,
  },
  modalOverlay: {
    flex: 1,
    backgroundColor: "rgba(0, 0, 0, 0.8)",
    justifyContent: "center",
    alignItems: "center",
    padding: 20,
  },
  modalContent: {
    width: "100%",
    maxWidth: 480,
    backgroundColor: "#1a1a1a",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.1)",
    borderRadius: 12,
    padding: 24,
  },
  modalTitle: {
    fontSize: 24,
    fontWeight: "bold",
    color: "#aee339",
    marginBottom: 8,
  },
  modalDescription: {
    fontSize: 14,
    color: "#d9d9d9",
    marginBottom: 20,
  },
  modalForm: {
    gap: 16,
  },
  modalField: {
    gap: 8,
  },
  modalLabel: {
    fontSize: 14,
    fontWeight: "500",
    color: "rgba(255, 255, 255, 0.8)",
  },
  modalInput: {
    backgroundColor: "rgba(255, 255, 255, 0.05)",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    borderRadius: 6,
    paddingHorizontal: 12,
    paddingVertical: 10,
    color: "#fff",
    fontSize: 14,
  },
  modalFooter: {
    flexDirection: "row",
    gap: 12,
    marginTop: 24,
  },
  modalCancelButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 6,
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.2)",
    backgroundColor: "#fff",
    alignItems: "center",
  },
  modalCancelText: {
    color: "#000",
    fontSize: 14,
    fontWeight: "600",
  },
  modalConfirmButton: {
    flex: 1,
    paddingVertical: 12,
    borderRadius: 6,
    backgroundColor: "#aee339",
    alignItems: "center",
  },
  modalConfirmText: {
    color: "#000",
    fontSize: 14,
    fontWeight: "600",
  },
});

export default ProfilePage;
