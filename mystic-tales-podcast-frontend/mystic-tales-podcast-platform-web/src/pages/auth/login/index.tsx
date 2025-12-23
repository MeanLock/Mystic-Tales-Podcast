import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { FcGoogle } from "react-icons/fc";
import {
  useLoginMutation,
  useLoginGoogleMutation,
  useSendForgotPasswordRequestMutation,
} from "@/core/services/auth/auth.service";
import "./styles.css";
import { getCapacitorDevice } from "@/core/utils/device";
// shadcn/ui
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import {
  InputOTP,
  InputOTPGroup,
  InputOTPSlot,
} from "@/components/ui/input-otp";

import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";

import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogAction,
  AlertDialogCancel,
} from "@/components/ui/alert-dialog";
import { useGoogleLogin } from "@react-oauth/google";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { BackgroundGradient } from "@/components/ui/shadcn-io/background-gradient";
import { useNavigate } from "react-router-dom";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
// no direct redux dispatch needed here; auth service handles storing token/user

/** Zod schema: email hợp lệ, password tối thiểu 8 ký tự,
 *  có thể siết chặt thêm (chứa chữ hoa, số...) bằng refine/superRefine nếu muốn
 */
const LoginSchema = z.object({
  Email: z
    .string()
    .min(1, "Email is required")
    .email("Email format is invalid"),
  Password: z.string().min(1, "Password must be at least 8 characters"),
});

type LoginFormValues = z.infer<typeof LoginSchema>;

const LoginPage = () => {
  // STATES
  const [login, { isLoading }] = useLoginMutation();
  const [loginGoogle, { isLoading: isGoogleLoading }] =
    useLoginGoogleMutation();
  const [deviceInfo, setDeviceInfo] = useState<any>(null);
  const user = useSelector((state: RootState) => state.auth.user);

  // HOOKS
  const dispatch = useDispatch();

  useEffect(() => {
    getCapacitorDevice().then(setDeviceInfo);
    if (user) {
      // nếu đã login thì chuyển hướng sang trang chính
      navigate("/media-player/discovery");
    }
  }, []);

  const [forgotPasswordData, setForgotPasswordData] = useState({
    isModalOpen: false,
    email: "",
    error: "",
    success: "",
  });

  const [serverError, setServerError] = useState<string | null>(null);
  const [responseError, setResponseError] = useState({
    isError: false,
    message: "",
    isUnVerified: false,
  });
  const [verificationData, setVerificationData] = useState({
    isModalOpen: false,
    verificationCode: "",
  });

  const resetResponseError = () => {
    setResponseError({ isError: false, message: "", isUnVerified: false });
  };

  const handleVerifyMyAccount = () => {
    setResponseError({ isError: false, message: "", isUnVerified: false });
    setVerificationData({ isModalOpen: true, verificationCode: "" });
  };

  const closeVerifyModal = () => {
    setVerificationData({ isModalOpen: false, verificationCode: "" });
  };

  const handleSubmitVerifyCode = () => {
    // TODO: Implement verification logic
    console.log("Verify code:", verificationData.verificationCode);
    closeVerifyModal();
  };

  // HOOKS
  const navigate = useNavigate();
  const [forgotPassword, { isLoading: isForgotLoading }] =
    useSendForgotPasswordRequestMutation();

  // HELPER

  // FUNCTIONS
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(LoginSchema),
    defaultValues: { Email: "", Password: "" },
    mode: "onTouched", // validate sớm khi blur field
  });

  const onSubmit = async (values: LoginFormValues) => {
    try {
      const payload = {
        ManualLoginInfo: { Email: values.Email, Password: values.Password },
        DeviceInfo: {
          DeviceId: deviceInfo?.DeviceId || "unknown",
          Platform: deviceInfo?.Platform || "web",
          OSName: deviceInfo?.OSName || "unknown",
        },
      };

      const result = await login(payload).unwrap();

      if (!result) {
        dispatch(
          showAlert({
            type: "error",
            title: "Login Failed",
            description: "Something went wrong, please try again later",
            isAutoClose: true,
            isClosable: true,
            autoCloseDuration: 10,
            isFunctional: false,
          })
        );
        return;
      }

      if (result.isError) {
        // show error dialog
        dispatch(
          showAlert({
            type: "error",
            title: result.message.title,
            description: result.message.description,
            isAutoClose: true,
            autoCloseDuration: 10,
            isFunctional: true,
            isClosable: true,
          })
        );
        return;
      } else {
        dispatch(
          showAlert({
            type: result.message.type,
            description: result.message.description,
            title: result.message.title,
            isAutoClose: false,
            isFunctional: true,
            isClosable: false,
            functionalButtonText: "Ok",
            onClickAction() {
              navigate("/media-player/discovery");
            },
          })
        );
      }
    } catch (e: any) {
      setServerError(e?.message || "Something went wrong. Please try again.");
    }
  };

  const handleLoginGoogleOAuth2 = useGoogleLogin({
    flow: "auth-code",
    onSuccess: async (codeResponse) => {
      const authorizationCode = codeResponse.code;

      try {
        // reset errors
        setServerError(null);
        resetResponseError();

        const payload = {
          GoogleAuth: {
            AuthorizationCode: authorizationCode,
            RedirectUri: window.location.origin,
          },
          DeviceInfo: {
            DeviceId: deviceInfo?.DeviceId || "unknown",
            Platform: deviceInfo?.Platform || "web",
            OSName: deviceInfo?.OSName || "unknown",
          },
        };

        const result = await loginGoogle(payload).unwrap();

        if (!result) {
          setServerError("Empty response from server");
          return;
        }

        if (result.isError) {
          setResponseError({
            isError: true,
            message: result.message?.description || "Google login failed",
            isUnVerified: false,
          });
          return;
        }

        // login service already stores token and sets user in redux
        navigate("/media-player/discovery");
      } catch (e: any) {
        setServerError(e?.message || "Google login failed. Please try again.");
      }
    },
    onError: (error) => {
      setServerError("Google OAuth error: " + error.error);
      console.log("Google OAuth Error", error);
    },
  });

  const handleGoogleLogin = () => {
    handleLoginGoogleOAuth2();
  };

  const handleForgotPassword = () => {
    setForgotPasswordData({
      isModalOpen: true,
      email: "",
      error: "",
      success: "",
    });
  };

  const closeForgotPasswordModal = () => {
    setForgotPasswordData({
      isModalOpen: false,
      email: "",
      error: "",
      success: "",
    });
  };

  const handleSubmitForgotPassword = async () => {
    setForgotPasswordData((prev) => ({ ...prev, error: "", success: "" }));

    // Validate email
    if (!forgotPasswordData.email.trim()) {
      setForgotPasswordData((prev) => ({
        ...prev,
        error: "Email is required.",
      }));
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(forgotPasswordData.email.trim())) {
      setForgotPasswordData((prev) => ({
        ...prev,
        error: "Invalid email format.",
      }));
      return;
    }

    try {
      await forgotPassword({ Email: forgotPasswordData.email.trim() }).unwrap();

      // Show success message
      setForgotPasswordData((prev) => ({
        ...prev,
        success: `A password reset link has been sent to ${forgotPasswordData.email}. Please check your inbox.`,
      }));

      // Auto close after 3 seconds
      setTimeout(() => {
        closeForgotPasswordModal();
      }, 3000);
    } catch (error: any) {
      console.error("Forgot password error:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Failed to send reset password email. Please try again.";
      setForgotPasswordData((prev) => ({ ...prev, error: msg }));
    }
  };

  return (
    <div className="w-full h-screen overflow-hidden bg-[url(/background/login4.png)] object-cover bg-cover flex items-center justify-center">
      <div
        id="glass_container"
        className="text-white px-20 py-10 min-w-[500px]"
      >
        <div className="w-full flex items-center justify-center">
          <img
            src="/images/logo/logo3.png"
            className="w-52 h-52 rounded-full object-cover bg-cover"
          />
        </div>
        <p className="font-bold font-poppins text-2xl tracking-tight mb-1">
          Welcome Back
        </p>
        <p className="text-sm opacity-80 mb-10">Sign in to to get scared!</p>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* Email */}
            <FormField
              control={form.control}
              name="Email"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Email</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="you@example.com"
                      type="email"
                      autoComplete="email"
                      disabled={isLoading}
                      {...field}
                      className="
                        bg-transparent
                        border-0
                        border-b-[0.3px]
                        border-[#d9d9d9]
                        focus:border-b-[1px]
                        focus:border-white
                        focus:outline-none
                        focus-visible:outline-none
                        focus:ring-0
                        focus-visible:ring-0
                        rounded-none
                        transition-all
                        duration-200
                      "
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Password */}
            <FormField
              control={form.control}
              name="Password"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Password</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="••••••••"
                      type="password"
                      autoComplete="current-password"
                      disabled={isLoading}
                      {...field}
                      className="
                        bg-transparent
                        border-0
                        border-b-[0.3px]
                        border-[#d9d9d9]
                        focus:border-b-[1px]
                        focus:border-white
                        focus:outline-none
                        focus-visible:outline-none
                        focus:ring-0
                        focus-visible:ring-0
                        rounded-none
                        transition-all
                        duration-200
                      "
                    />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {/* Server error */}
            {serverError && (
              <div className="text-red-300 text-sm">{serverError}</div>
            )}

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading || !form.formState.isValid}
            >
              {isLoading ? "Signing in..." : "Sign in"}
            </Button>
          </form>
        </Form>

        {/* Navigation & Oauth2 */}
        <div className="w-full flex flex-col items-center gap-5 py-2">
          <div className="w-full flex items-center justify-end">
            <p
              onClick={() => handleForgotPassword()}
              className="text-[#D9D9D9] italic hover:underline cursor-pointer"
            >
              Forgot password ?
            </p>
          </div>
          <p className="text-xs text-gray-400">-----OR-----</p>
          <div
            className="w-full flex items-center justify-center"
            onClick={() => handleGoogleLogin()}
          >
            <BackgroundGradient className="cursor-pointer rounded-3xl w-full px-4 py-2 bg-white flex items-center justify-center gap-3">
              <FcGoogle />
              <p className="text-black font-bold">
                {isGoogleLoading ? "Loading..." : "Sign in with Google"}
              </p>
            </BackgroundGradient>
          </div>

          <div className="w-full flex items-center justify-center px-4">
            <p className="text-sm text-gray-300">
              Never been scared before?{" "}
              <span
                onClick={() => navigate("/auth/register")}
                className="text-mystic-green ml-1 font-bold hover:underline cursor-pointer"
              >
                Sign Up now!
              </span>
            </p>
          </div>
        </div>
      </div>

      <AlertDialog
        open={responseError.isError}
        onOpenChange={(open) => {
          // Nếu user đóng dialog (click ra ngoài/Cancel/Ok), reset state
          if (!open) resetResponseError();
        }}
      >
        <AlertDialogContent className="sm:max-w-[420px] border border-white/10 bg-black/80 text-white">
          <AlertDialogHeader>
            <AlertDialogTitle className="text-mystic-green">
              Something went wrong :(
            </AlertDialogTitle>
            <AlertDialogDescription className="text-gray-200">
              {responseError.message || "Unexpected error occurred."}
            </AlertDialogDescription>
          </AlertDialogHeader>

          <AlertDialogFooter>
            {/* Nút OK: luôn có */}
            <AlertDialogCancel
              onClick={resetResponseError}
              className="bg-transparent border border-white/20 text-white hover:bg-white/10"
            >
              Ok
            </AlertDialogCancel>

            {/* Nút Verify: chỉ hiển thị khi isUnVerified */}
            {responseError.isUnVerified && (
              <AlertDialogAction
                onClick={handleVerifyMyAccount}
                className="bg-[#AAE339] text-black hover:bg-[#AAE339]/90"
              >
                Verify My Account
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      <Dialog
        open={verificationData.isModalOpen}
        onOpenChange={(open) => {
          if (!open) closeVerifyModal();
        }}
      >
        <DialogContent className="sm:max-w-[420px] border border-white/10 bg-black/80 text-white">
          <DialogHeader>
            <DialogTitle className="text-[#AAE339]">
              Verify your account
            </DialogTitle>
            <DialogDescription className="text-gray-200">
              Enter the 6-digit code sent to your email.
            </DialogDescription>
          </DialogHeader>

          {/* OTP Input */}
          <div className="flex w-full items-center justify-center py-2">
            <InputOTP
              maxLength={6}
              value={verificationData.verificationCode}
              onChange={(val) => {
                // chỉ nhận ký tự số
                const digits = val.replace(/\D/g, "");
                setVerificationData((prev) => ({
                  ...prev,
                  verificationCode: digits,
                }));
              }}
            >
              <InputOTPGroup>
                <InputOTPSlot index={0} />
                <InputOTPSlot index={1} />
                <InputOTPSlot index={2} />
                <InputOTPSlot index={3} />
                <InputOTPSlot index={4} />
                <InputOTPSlot index={5} />
              </InputOTPGroup>
            </InputOTP>
          </div>

          <DialogFooter className="gap-2">
            <Button
              variant="outline"
              className="border-white/20 text-black hover:bg-white/10"
              onClick={closeVerifyModal}
            >
              Cancel
            </Button>
            <Button
              className="bg-[#AAE339] text-black hover:bg-[#AAE339]/90"
              onClick={handleSubmitVerifyCode}
              disabled={verificationData.verificationCode.length !== 6}
            >
              Verify
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Forgot Password Dialog */}
      <Dialog
        open={forgotPasswordData.isModalOpen}
        onOpenChange={(open) => {
          if (!open) closeForgotPasswordModal();
        }}
      >
        <DialogContent className="sm:max-w-[440px] border border-white/10 bg-black/80 text-white">
          <DialogHeader>
            <DialogTitle className="text-mystic-green">
              Forgot Password
            </DialogTitle>
            <DialogDescription className="text-gray-200">
              Enter your email address and we'll send you a password reset link.
            </DialogDescription>
          </DialogHeader>

          <div className="flex flex-col gap-4 py-4">
            <div className="flex flex-col gap-2">
              <label className="text-sm font-medium text-white/80">
                Email Address
              </label>
              <Input
                type="email"
                placeholder="you@example.com"
                value={forgotPasswordData.email}
                onChange={(e) =>
                  setForgotPasswordData((prev) => ({
                    ...prev,
                    email: e.target.value,
                  }))
                }
                disabled={isForgotLoading}
                className="bg-white/5 border-white/20 text-white placeholder:text-white/40 focus:ring-2 focus:ring-mystic-green"
              />
            </div>

            {/* Error Message */}
            {forgotPasswordData.error && (
              <p className="text-red-400 text-sm">{forgotPasswordData.error}</p>
            )}

            {/* Success Message */}
            {forgotPasswordData.success && (
              <p className="text-mystic-green text-sm">
                {forgotPasswordData.success}
              </p>
            )}
          </div>

          <DialogFooter className="gap-2">
            <Button
              variant="outline"
              className="border-white/20 text-black hover:bg-white/10"
              onClick={closeForgotPasswordModal}
              disabled={isForgotLoading}
            >
              Cancel
            </Button>
            <Button
              className="bg-mystic-green text-black font-semibold hover:bg-mystic-green/90"
              onClick={handleSubmitForgotPassword}
              disabled={isForgotLoading || !forgotPasswordData.email.trim()}
            >
              {isForgotLoading ? "Sending..." : "Send Reset Link"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default LoginPage;
