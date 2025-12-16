import {
  useGetAccountInformationsQuery,
  useUpdateAccountInformationsMutation,
} from "@/core/services/account/account.service";
import { useRef, useState } from "react";
import "./style.css";
import { useDispatch } from "react-redux";
import { clearAuth, setUser } from "@/redux/slices/authSlice/authSlice";
import { MdEdit, MdLockReset, MdOutlineLogout, MdUpload } from "react-icons/md";
import ShowOnHoverButton from "@/components/button/ShowOnHoverButton";
import { TimeUtil } from "@/core/utils/time";
import { FaMicrophoneAlt } from "react-icons/fa";
import Loading from "@/components/loading";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { useUpdatePasswordMutation } from "@/core/services/auth/auth.service";
import { useNavigate } from "react-router-dom";
import MTPCoinOutline from "@/components/coinIcons/CoinIconOutline";
import AutoResolveImage from "@/components/fileResolving/AutoResolveImage";

type UpdateInformationsForm = {
  AccountUpdateInfo: {
    FullName: string;
    Dob: string; // ISO String
    Gender: string;
    Address: string;
    Phone: string;
  };
  MainImageFile: File | null;
};

type AccountUpdateInfoErrors = Partial<
  Record<keyof UpdateInformationsForm["AccountUpdateInfo"], string>
>;

const ProfilePage = () => {
  const [isResetPasswordOpen, setIsResetPasswordOpen] =
    useState<boolean>(false);
  const [oldPassword, setOldPassword] = useState<string>("");
  const [newPassword, setNewPassword] = useState<string>("");
  const [confirmPassword, setConfirmPassword] = useState<string>("");
  const [resetPasswordError, setResetPasswordError] = useState<string>("");
  const [resetPasswordSuccess, setResetPasswordSuccess] = useState<string>("");

  const {
    data: accountInformation,
    isFetching: isAccountMeLoading,
    isError,
    refetch,
  } = useGetAccountInformationsQuery();

  const dispatch = useDispatch();
  const navigate = useNavigate();
  const [resetPassword, { isLoading: isResettingPassword }] =
    useUpdatePasswordMutation();

  const [updateAccountInformations, { isLoading: isUpdating }] =
    useUpdateAccountInformationsMutation();

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

  // FUNCTIONS
  const handleLogout = () => {
    localStorage.removeItem("accessToken");
    sessionStorage.removeItem("isInWebKey");
    dispatch(clearAuth());
    window.location.href = "/auth/login";
  };

  const resetFormFromAccountInfo = () => {
    if (!accountInformation) return;
    setFormState({
      AccountUpdateInfo: {
        FullName: accountInformation.Account.FullName ?? "",
        Dob: accountInformation.Account.Dob ?? "",
        Gender: accountInformation.Account.Gender ?? "Other",
        Address: accountInformation.Account.Address ?? "",
        Phone: accountInformation.Account.Phone ?? "",
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

  const handleUploadAvatarClick = () => {
    if (fileInputRef.current) {
      fileInputRef.current.click();
    }
  };

  const handleAvatarFileChange = async (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = e.target.files?.[0];
    if (!file || !accountInformation) return;

    // Validate file type
    const allowedTypes = [
      "image/jpeg",
      "image/jpg",
      "image/png",
      "image/gif",
      "image/webp",
      "image/svg+xml",
    ];
    if (!allowedTypes.includes(file.type)) {
      setUpdateErrorMessage(
        "Invalid file type. Only JPG, JPEG, PNG, GIF, WEBP, and SVG are allowed."
      );
      e.target.value = "";
      return;
    }

    // Validate file size (max 3MB)
    const maxSizeInBytes = 3 * 1024 * 1024; // 3MB
    if (file.size > maxSizeInBytes) {
      setUpdateErrorMessage(
        "File size exceeds 3MB. Please upload a smaller image."
      );
      e.target.value = "";
      return;
    }

    // Clear any previous errors
    setUpdateErrorMessage(null);

    // gọi save chỉ với file, AccountUpdateInfo lấy từ accountInformation cũ
    await handleSaveEdit({ onlyAvatar: true, file });
    // clear input để lần sau có thể chọn lại cùng file nếu muốn
    e.target.value = "";
  };

  const handleApplyToBePodcaster = () => {
    // TODO: implement apply podcaster
    navigate("/become-podcaster");
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

    // Address có thể optional nên mình không bắt buộc

    return errors;
  };

  /**
   * options.onlyAvatar = true → chỉ update avatar, AccountUpdateInfo lấy giá trị cũ
   * options.file = file avatar mới
   */
  const handleSaveEdit = async (options?: {
    onlyAvatar?: boolean;
    file?: File;
  }) => {
    if (!accountInformation) return;

    setUpdateErrorMessage(null);
    setUpdateSuccessMessage(null);

    let accountUpdateInfo: UpdateInformationsForm["AccountUpdateInfo"];

    if (options?.onlyAvatar || !formState) {
      // dùng giá trị cũ
      accountUpdateInfo = {
        FullName: accountInformation.Account.FullName ?? "",
        Dob: accountInformation.Account.Dob ?? "",
        Gender: accountInformation.Account.Gender ?? "Other",
        Address: accountInformation.Account.Address ?? "",
        Phone: accountInformation.Account.Phone ?? "",
      };
    } else {
      // merge form với giá trị cũ
      const merged: UpdateInformationsForm["AccountUpdateInfo"] = {
        FullName:
          formState.AccountUpdateInfo.FullName ||
          accountInformation.Account.FullName ||
          "",
        Dob:
          formState.AccountUpdateInfo.Dob ||
          accountInformation.Account.Dob ||
          "",
        Gender:
          formState.AccountUpdateInfo.Gender ||
          accountInformation.Account.Gender ||
          "Other",
        Address:
          formState.AccountUpdateInfo.Address ||
          accountInformation.Account.Address ||
          "",
        Phone:
          formState.AccountUpdateInfo.Phone ||
          accountInformation.Account.Phone ||
          "",
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
      formData.append("MainImageFile", mainImageFile);
    }

    try {
      const response = await updateAccountInformations({
        uploadAccountInformationsFormData: formData,
        accountId: accountInformation.Account.Id,
      }).unwrap();

      setUpdateSuccessMessage(response?.Message || "Update successfully.");
      setFieldErrors({});

      // refetch lại thông tin và dispatch data mới vào Redux
      const refetchResult = await refetch();
      if (refetchResult.data) {
        dispatch(setUser(refetchResult.data.Account));
      }

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
      setUpdateErrorMessage(msg);
    }
  };

  const getDobInputValue = () => {
    const dobIso =
      formState?.AccountUpdateInfo.Dob || accountInformation?.Account.Dob;
    if (!dobIso) return "";
    // TimeUtil.formatDate → "YYYY-MM-DD" cho input[type=date]
    return TimeUtil.formatDate(dobIso, "YYYY-MM-DD");
  };

  const handleResetPassword = () => {
    setIsResetPasswordOpen(true);
    setOldPassword("");
    setNewPassword("");
    setConfirmPassword("");
    setResetPasswordError("");
    setResetPasswordSuccess("");
  };

  const handleConfirmResetPassword = async () => {
    setResetPasswordError("");
    setResetPasswordSuccess("");

    // Validation
    if (!oldPassword.trim()) {
      setResetPasswordError("Old password is required.");
      return;
    }

    if (!newPassword.trim()) {
      setResetPasswordError("New password is required.");
      return;
    }

    if (newPassword.length < 8) {
      setResetPasswordError("New password must be at least 8 characters.");
      return;
    }

    if (newPassword !== confirmPassword) {
      setResetPasswordError("New password and confirm password do not match.");
      return;
    }

    if (oldPassword === newPassword) {
      setResetPasswordError(
        "New password must be different from old password."
      );
      return;
    }

    try {
      const result = await resetPassword({
        CurrentPassword: oldPassword,
        NewPassword: newPassword,
      }).unwrap();

      setResetPasswordSuccess(
        result?.Message || "Password reset successfully!"
      );

      // Show success message then logout and navigate
      setTimeout(() => {
        setIsResetPasswordOpen(false);
        setOldPassword("");
        setNewPassword("");
        setConfirmPassword("");
        setResetPasswordError("");
        setResetPasswordSuccess("");

        // Clear auth and navigate to login
        dispatch(clearAuth());
        localStorage.removeItem("accessToken");
        localStorage.removeItem("device_info_token");
        navigate("/auth/login");
      }, 2000);
    } catch (error: any) {
      console.error("Failed to reset password:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Failed to reset password. Please try again.";
      setResetPasswordError(msg);
    }
  };

  if (isAccountMeLoading) {
    return (
      <div className="w-full h-full flex items-center justify-center flex-col gap-5">
        <Loading />
        <p className="text-[#d9d9d9] font-poppins font-bold text-lg">
          Finding You...
        </p>
      </div>
    );
  }

  if (isError) {
    return (
      <div className="w-full h-full flex items-center justify-center">
        <p className="text-red-400 font-poppins">
          Cannot load your profile. Please try again later.
        </p>
        <ShowOnHoverButton
          Icon={MdOutlineLogout}
          text="Logout"
          bgColor="#E06C75"
          onClick={() => handleLogout()}
        />
      </div>
    );
  }

  return (
    <div className="w-full h-full flex flex-col gap-5">
      <p className="m-8 text-5xl text-white font-poppins font-bold">
        Your Profile
      </p>

      <div className="px-8  w-full flex flex-col gap-3">
        {/* Avatar */}
        <div className="p-5 flex items-center gap-10 bg-white/10 border border-white/20 shadow-2xl rounded-md">
          <AutoResolveImage
            FileKey={
              accountInformation
                ? accountInformation.Account.MainImageFileKey
                : ""
            }
            type="AccountPublicSource"
            className="w-36 aspect-square rounded-full object-cover shadow-2xl"
          />

          {/* hidden input file */}
          <input
            type="file"
            ref={fileInputRef}
            accept="image/jpeg,image/jpg,image/png,image/gif,image/webp,image/svg+xml"
            className="hidden"
            onChange={handleAvatarFileChange}
          />

          <div className="flex flex-col items-start justify-center gap-1">
            <ShowOnHoverButton
              Icon={MdUpload}
              text={isUpdating ? "Uploading..." : "Upload New Avatar"}
              onClick={handleUploadAvatarClick}
              bgColor="#4393de"
            />
            <p className="font-poppins text-[#D9D9D9] mt-2">
              Image must be{" "}
              <span className="text-mystic-green">square aspect</span> for best
              resolution.
            </p>
            <p className="font-poppins text-[#D9D9D9]">
              JPG, JPEG, PNG, GIF, WEBP, or SVG is allowed (max 3MB).
            </p>
          </div>
        </div>

        {/* Personal Info */}
        <div className="p-5 w-full rounded-md bg-white/10 border border-white/20 shadow-2xl flex flex-col gap-3 text-white font-poppins">
          <div className="w-full flex items-center justify-between">
            <p className="text-xl font-bold mb-5">Personal Info</p>

            {!isEditing ? (
              <ShowOnHoverButton
                Icon={MdEdit}
                text="Edit Profile"
                onClick={handleToggleEdit}
                bgColor="#4393de"
              />
            ) : (
              <div className="flex items-center gap-3">
                <button
                  onClick={handleToggleEdit}
                  disabled={isUpdating}
                  className="px-4 py-2 rounded-md border border-white/30 text-sm text-white/80 hover:bg-white/10 disabled:opacity-50"
                >
                  Cancel
                </button>
                <button
                  onClick={() => handleSaveEdit()}
                  disabled={isUpdating}
                  className="px-4 py-2 rounded-md bg-mystic-green text-sm font-semibold text-black hover:bg-mystic-green/80 disabled:opacity-50"
                >
                  {isUpdating ? "Saving..." : "Save Changes"}
                </button>
              </div>
            )}
          </div>

          {(updateErrorMessage || updateSuccessMessage) && (
            <div className="mb-2">
              {updateErrorMessage && (
                <p className="text-red-400 text-sm">{updateErrorMessage}</p>
              )}
              {updateSuccessMessage && (
                <p className="text-emerald-400 text-sm">
                  {updateSuccessMessage}
                </p>
              )}
            </div>
          )}

          <div className="w-full grid grid-cols-1 md:grid-cols-3 gap-5">
            {/* Full Name */}
            <div className="w-full flex flex-col items-start justify-center gap-2">
              <p className="text-[#a1a1a1] font-bold">Full Name</p>
              {isEditing ? (
                <>
                  <input
                    className="w-full bg-white/5 border border-white/20 rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-mystic-green"
                    value={formState?.AccountUpdateInfo.FullName ?? ""}
                    onChange={(e) =>
                      handleInputChange("FullName", e.target.value)
                    }
                  />
                  {fieldErrors.FullName && (
                    <p className="text-red-400 text-xs">
                      {fieldErrors.FullName}
                    </p>
                  )}
                </>
              ) : (
                <p className="text-white line-clamp-1">
                  {accountInformation?.Account.FullName}
                </p>
              )}
            </div>

            {/* Email - chỉ view */}
            <div className="w-full flex flex-col items-start justify-center gap-2">
              <p className="text-[#a1a1a1] font-bold">Email</p>
              <p className="text-white line-clamp-1">
                {accountInformation?.Account.Email}
              </p>
            </div>

            {/* Phone */}
            <div className="w-full flex flex-col items-start justify-center gap-2">
              <p className="text-[#a1a1a1] font-bold">Phone</p>
              {isEditing ? (
                <>
                  <input
                    className="w-full bg-white/5 border border-white/20 rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-mystic-green"
                    value={formState?.AccountUpdateInfo.Phone ?? ""}
                    onChange={(e) => handleInputChange("Phone", e.target.value)}
                  />
                  {fieldErrors.Phone && (
                    <p className="text-red-400 text-xs">{fieldErrors.Phone}</p>
                  )}
                </>
              ) : (
                <p className="text-white line-clamp-1">
                  {accountInformation?.Account.Phone}
                </p>
              )}
            </div>

            {/* Gender */}
            <div className="w-full flex flex-col items-start justify-center gap-2">
              <p className="text-[#a1a1a1] font-bold">Gender</p>
              {isEditing ? (
                <>
                  <select
                    className="w-full bg-white/5 border border-white/20 rounded px-3 py-2 pr-8 text-white text-sm focus:outline-none focus:ring-2 focus:ring-mystic-green [&>option]:bg-[#333] [&>option]:text-white appearance-none bg-[url('data:image/svg+xml;charset=utf-8,%3Csvg%20xmlns%3D%22http%3A%2F%2Fwww.w3.org%2F2000%2Fsvg%22%20fill%3D%22none%22%20viewBox%3D%220%200%2020%2020%22%3E%3Cpath%20stroke%3D%22%23fff%22%20stroke-linecap%3D%22round%22%20stroke-linejoin%3D%22round%22%20stroke-width%3D%221.5%22%20d%3D%22m6%208%204%204%204-4%22%2F%3E%3C%2Fsvg%3E')] bg-size-[1.25rem] bg-position-[right_0.5rem_center] bg-no-repeat"
                    value={formState?.AccountUpdateInfo.Gender ?? "Other"}
                    onChange={(e) =>
                      handleInputChange("Gender", e.target.value)
                    }
                  >
                    <option value="Male">Male</option>
                    <option value="Female">Female</option>
                    <option value="Other">Other</option>
                  </select>
                  {fieldErrors.Gender && (
                    <p className="text-red-400 text-xs">{fieldErrors.Gender}</p>
                  )}
                </>
              ) : (
                <p className="text-white line-clamp-1">
                  {accountInformation?.Account.Gender}
                </p>
              )}
            </div>

            {/* DOB */}
            <div className="w-full flex flex-col items-start justify-center gap-2">
              <p className="text-[#a1a1a1] font-bold">Date Of Birth</p>
              {isEditing ? (
                <>
                  <input
                    type="date"
                    className="w-full bg-white/5 border border-white/20 rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-mystic-green"
                    value={getDobInputValue()}
                    onChange={(e) => {
                      const dateOnly = e.target.value; // YYYY-MM-DD
                      const iso = dateOnly
                        ? new Date(dateOnly).toISOString()
                        : "";
                      handleInputChange("Dob", iso);
                    }}
                  />
                  {fieldErrors.Dob && (
                    <p className="text-red-400 text-xs">{fieldErrors.Dob}</p>
                  )}
                </>
              ) : accountInformation && accountInformation.Account.Dob ? (
                <p className="text-white line-clamp-1">
                  {TimeUtil.formatDate(
                    accountInformation.Account.Dob,
                    "DD/MM/YYYY"
                  )}
                </p>
              ) : (
                <p className="text-white line-clamp-1">Not Updated</p>
              )}
            </div>

            {/* Address */}
            <div className="w-full flex flex-col items-start justify-center gap-2 overflow-ellipsis md:col-span-1">
              <p className="text-[#a1a1a1] font-bold">Address</p>
              {isEditing ? (
                <>
                  <textarea
                    className="w-full bg-white/5 border border-white/20 rounded px-3 py-2 text-white text-sm focus:outline-none focus:ring-2 focus:ring-mystic-green resize-none"
                    rows={2}
                    value={formState?.AccountUpdateInfo.Address ?? ""}
                    onChange={(e) =>
                      handleInputChange("Address", e.target.value)
                    }
                  />
                  {fieldErrors.Address && (
                    <p className="text-red-400 text-xs">
                      {fieldErrors.Address}
                    </p>
                  )}
                </>
              ) : (
                <p className="text-white line-clamp-1">
                  {accountInformation?.Account.Address}
                </p>
              )}
            </div>

            {/* Balance */}
            <div className="w-full flex flex-col items-start justify-center gap-2 overflow-ellipsis">
              <p className="text-[#a1a1a1] font-bold">Account Balance</p>

              <div className="flex items-center gap-2">
                <p className="text-white line-clamp-1">
                  {accountInformation?.Account.Balance?.toLocaleString()}
                </p>
                <MTPCoinOutline size={16} color="white" />
                <p className="text-white">MTP Coins</p>
              </div>
            </div>
          </div>
        </div>

        {/* bottom actions */}
        <div className="w-full mt-2 flex items-center justify-end gap-5">
          {!accountInformation?.Account.IsPodcaster && (
            <ShowOnHoverButton
              Icon={FaMicrophoneAlt}
              text="Apply to be Podcaster"
              bgColor="#4F8BFF"
              onClick={() => handleApplyToBePodcaster()}
            />
          )}

          <ShowOnHoverButton
            Icon={MdLockReset}
            text="Reset Password"
            bgColor="#F4A259"
            onClick={() => handleResetPassword()}
          />
          <ShowOnHoverButton
            Icon={MdOutlineLogout}
            text="Logout"
            bgColor="#E06C75"
            onClick={() => handleLogout()}
          />
        </div>
      </div>

      {/* Reset Password Dialog */}
      <Dialog
        open={isResetPasswordOpen}
        onOpenChange={(open) => {
          if (!open) {
            setIsResetPasswordOpen(false);
            setOldPassword("");
            setNewPassword("");
            setConfirmPassword("");
            setResetPasswordError("");
            setResetPasswordSuccess("");
          }
        }}
      >
        <DialogContent className="sm:max-w-120 z-9999 border border-white/10 bg-black/80 text-white">
          <DialogHeader>
            <DialogTitle className="text-mystic-green">
              Reset Password
            </DialogTitle>
            <DialogDescription className="text-gray-200">
              Enter your old password and choose a new password.
            </DialogDescription>
          </DialogHeader>

          <div className="flex flex-col gap-4 py-4">
            {/* Old Password */}
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-white/80">
                Old Password
              </label>
              <Input
                type="password"
                placeholder="Enter old password"
                value={oldPassword}
                onChange={(e) => setOldPassword(e.target.value)}
                // disabled={isResetting}
                className="bg-white/5 border-white/20 text-white placeholder:text-white/40 focus:ring-2 focus:ring-mystic-green"
              />
            </div>

            {/* New Password */}
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-white/80">
                New Password
              </label>
              <Input
                type="password"
                placeholder="Enter new password (min 8 characters)"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                //disabled={isResetting}
                className="bg-white/5 border-white/20 text-white placeholder:text-white/40 focus:ring-2 focus:ring-mystic-green"
              />
            </div>

            {/* Confirm Password */}
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-white/80">
                Confirm New Password
              </label>
              <Input
                type="password"
                placeholder="Re-enter new password"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                //disabled={isResetting}
                className="bg-white/5 border-white/20 text-white placeholder:text-white/40 focus:ring-2 focus:ring-mystic-green"
              />
            </div>

            {/* Error/Success Messages */}
            {resetPasswordError && (
              <p className="text-red-400 text-sm">{resetPasswordError}</p>
            )}
            {resetPasswordSuccess && (
              <p className="text-emerald-400 text-sm">{resetPasswordSuccess}</p>
            )}
          </div>

          <DialogFooter className="gap-2">
            <Button
              variant="outline"
              className="border-white/20 text-black bg-white hover:bg-white/10"
              onClick={() => setIsResetPasswordOpen(false)}
              //disabled={isResetting}
            >
              Cancel
            </Button>
            <Button
              className="bg-mystic-green text-black font-semibold hover:bg-mystic-green/90"
              onClick={handleConfirmResetPassword}
            >
              {/* {isResetting ? "Resetting..." : "Reset Password"} */}
              {isResettingPassword ? "Resetting..." : "Reset Password"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default ProfilePage;
