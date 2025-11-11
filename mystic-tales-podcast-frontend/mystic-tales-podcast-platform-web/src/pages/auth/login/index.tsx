import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { FcGoogle } from "react-icons/fc";
import { useLoginMutation } from "@/core/services/auth/auth.service";
import "./styles.css";

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

import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { BackgroundGradient } from "@/components/ui/shadcn-io/background-gradient";
import { useNavigate } from "react-router-dom";
import { loginInfoMap, mockUsers } from "@/core/mockData/user.mockdata";
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

type ErrorResponse = {
  isError: boolean;
  message: string;
  isUnVerified?: boolean;
};

const LoginPage = () => {
  // STATES
  const [serverError, setServerError] = useState<string | null>(null);
  const [login, { isLoading }] = useLoginMutation();

  const [responseError, setResponseError] = useState<ErrorResponse>({
    isError: false,
    message: "",
    isUnVerified: false,
  });

  const [verificationData, setVerificationData] = useState({
    isModalOpen: false,
    verificationCode: "",
  });

  // HOOKS
  const navigate = useNavigate();

  // HELPERS
  const resetResponseError = () =>
    setResponseError({ isError: false, message: "", isUnVerified: false });

  const handleVerifyMyAccount = () => {
    resetResponseError();
    setVerificationData((prev) => ({ ...prev, isModalOpen: true }));
  };

  // Fake logic here
  const userData = mockUsers;

  // const fakeLoginSubmitLogic = async (values: LoginFormValues) => {
  //   const account = loginInfoMap.filter(
  //     (info) => info.email === values.email
  //   )[0];
  //   if (account) {
  //     if (account.password === values.password) {
  //       const accountInformation = userData.filter(
  //         (u) => u.Id === account.id
  //       )[0];
  //       if (accountInformation && !accountInformation.DeactivatedAt) {
  //         if (accountInformation.IsVerified === true) {
  //           dispatch(
  //             setAuthToken(
  //               "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEiLCJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiRFhCIiwiaHR0cDovL3NjaGVtYXMueG1sc29hcC5vcmcvd3MvMjAwNS8wNS9pZGVudGl0eS9jbGFpbXMvZW1haWxhZGRyZXNzIjoiZHhiYWNoMjAwNEBnbWFpbC5jb20iLCJpZCI6IjEiLCJyb2xlX2lkIjoiMSIsImh0dHA6Ly9zY2hlbWFzLm1pY3Jvc29mdC5jb20vd3MvMjAwOC8wNi9pZGVudGl0eS9jbGFpbXMvcm9sZSI6IkN1c3RvbWVyIiwiYmFsYW5jZSI6IjAuMDAiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3NlcmlhbG51bWJlciI6IjhjZGJlMWU2LTEwNDMtNDFmYS05ZGZlLWExYzBlYzY5NzQyMSIsImV4cCI6Nzc2MjY4NzMyMywiaXNzIjoibG9jYWxob3N0IiwiYXVkIjoibG9jYWxob3N0In0.Wt4b5heKvHBgc33ekDruYfvrule-ViKGMbe7u43wDlGZjaJ52Q8EpA9PbrEksHs8t6Osxa5qWAYi8OpeTnMvIEE0cp9uN9flYkoE8FFCx_Id5SULGczFiC99MclYjLEz_bLUPV40NpWTrS89oo3gZ3XrOal23jBVFidKClz0E-tvAIciWEat82MDt36zDYd1NIeogHDzu7TBFRtI2yocgsMIYrSNpFT3TC0L-nuiVRZ0TgL1D4MtQs79deJ2nYEN4bIHocdSEuApc6z1UhftLxPBCi7fxi9LD2_r8Wpli2v1hxv3VnRrUUN--Yh7jOIaXvfgFHAZVru5XeiD4JxN9w"
  //             )
  //           );
  //           dispatch(setUser(accountInformation));

  //           navigate("/media-player/discovery");
  //         } else {
  //           setResponseError({
  //             isError: true,
  //             message:
  //               "Your Account Hasn't Verified Yet, Please Verified To Sign In",
  //             isUnVerified: true,
  //           });
  //         }
  //       } else {
  //         setResponseError({
  //           isError: true,
  //           message: "Your Account Has Been Deactivated!",
  //         });
  //       }
  //     } else {
  //       setResponseError({
  //         isError: true,
  //         message: "Incorrect Password!",
  //       });
  //     }
  //   } else {
  //     setResponseError({
  //       isError: true,
  //       message: "Seems like your account doesn't exists!",
  //     });
  //   }
  // };

  // đóng modal + reset mã

  const closeVerifyModal = () => {
    setVerificationData({ isModalOpen: false, verificationCode: "" });
  };

  const handleSubmitVerifyCode = async () => {
    // ví dụ: mã đúng = "123456"
    if (verificationData.verificationCode.length !== 6) {
      setResponseError({
        isError: true,
        message: "Please enter the 6-digit verification code.",
      });
      return;
    }

    try {
      // TODO: gọi API verify ở đây
      // await verifyAccountAPI(verificationData.verificationCode);

      // demo logic:
      if (verificationData.verificationCode === "123456") {
        closeVerifyModal();
        // ví dụ: navigate sau verify
        navigate("/home");
      } else {
        setResponseError({
          isError: true,
          message: "Invalid verification code. Please try again.",
        });
      }
    } catch (e: any) {
      setResponseError({
        isError: true,
        message: e?.message || "Verification failed. Try again later.",
      });
    }
  };

  // FUNCTIONS
  const form = useForm<LoginFormValues>({
    resolver: zodResolver(LoginSchema),
    defaultValues: { Email: "", Password: "" },
    mode: "onTouched", // validate sớm khi blur field
  });

  const onSubmit = async (values: LoginFormValues) => {
    // reset errors
    setServerError(null);
    resetResponseError();

    try {
      const payload = {
        ManualLoginInfo: { Email: values.Email, Password: values.Password },
      };

      const result = await login(payload).unwrap();

      if (!result) {
        setServerError("Empty response from server");
        return;
      }

      if (result.isError) {
        // show error dialog
        setResponseError({
          isError: true,
          message: result.message || "Login failed",
          isUnVerified: !!result.isUnVerified,
        });
        return;
      }

      // success path
      if (result.isUnVerified) {
        setResponseError({
          isError: true,
          message: result.message || "Account not verified",
          isUnVerified: true,
        });
        return;
      }

      // login service already stores token and sets user in redux
      // navigate to discovery/home
      navigate("/media-player/discovery");
    } catch (e: any) {
      setServerError(e?.message || "Something went wrong. Please try again.");
    }
  };

  const handleOAuth2Login = async () => {};

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
          <p className="text-xs text-gray-400">-----OR-----</p>

          <BackgroundGradient className="cursor-pointer rounded-3xl w-full px-4 py-2 bg-white flex items-center justify-center gap-3">
            <FcGoogle />
            <p className="text-black font-bold">Sign in with Google</p>
          </BackgroundGradient>

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
    </div>
  );
};

export default LoginPage;
