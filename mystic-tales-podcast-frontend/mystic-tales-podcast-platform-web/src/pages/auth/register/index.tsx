// @ts-nocheck

import {
  useRegisterMutation,
  useVerifyAccountMutation,
} from "@/core/services/auth/auth.service";
import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { z } from "zod";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
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
import "./styles.css";

const RegisterSchema = z
  .object({
    Email: z
      .string()
      .min(1, "Email is required")
      .email("Email format is invalid"),
    Password: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
        "Password must contain uppercase, lowercase, and number"
      ),
    ConfirmPassword: z.string().min(1, "Please confirm your password"),
    FullName: z
      .string()
      .min(2, "Full name must be at least 2 characters")
      .max(100, "Full name is too long"),
    Dob: z.string().min(1, "Date of birth is required"),
    Gender: z.enum(["Male", "Female", "Other"], {
      message: "Please select a gender",
    }),
    Phone: z
      .string()
      .min(8, "Phone number must be at least 8 digits")
      .regex(/^[0-9+\-\s]{8,20}$/, "Invalid phone number format"),
    Address: z.string().optional(),
  })
  .refine((data) => data.Password === data.ConfirmPassword, {
    message: "Passwords do not match",
    path: ["ConfirmPassword"],
  });

type RegisterFormValues = z.infer<typeof RegisterSchema>;

const RegisterPage = () => {
  // STATES
  const [serverError, setServerError] = useState<string | null>(null);
  const [avatarPreview, setAvatarPreview] = useState<string | null>(null);
  const [avatarFile, setAvatarFile] = useState<File | null>(null);
  const [verificationData, setVerificationData] = useState({
    isModalOpen: false,
    verificationCode: "",
    email: "",
    error: "",
  });

  // HOOKS
  const navigate = useNavigate();
  const [register, { isLoading: isRegistering }] = useRegisterMutation();
  const [verifyAccount, { isLoading: isVerifying }] =
    useVerifyAccountMutation();

  const form = useForm<RegisterFormValues>({
    resolver: zodResolver(RegisterSchema),
    defaultValues: {
      Email: "",
      Password: "",
      ConfirmPassword: "",
      FullName: "",
      Dob: "",
      Gender: "Male",
      Phone: "",
      Address: "",
    },
    mode: "onTouched",
  });

  // FUNCTIONS
  const handleAvatarChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      setAvatarFile(file);
      const reader = new FileReader();
      reader.onloadend = () => {
        setAvatarPreview(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  };

  const onSubmit = async (values: RegisterFormValues) => {
    setServerError(null);

    try {
      const formData = new FormData();

      // Create RegisterInfo object
      const registerInfo = {
        Email: values.Email,
        Password: values.Password,
        FullName: values.FullName,
        Dob: new Date(values.Dob).toISOString(),
        Gender: values.Gender,
        Address: values.Address || "",
        Phone: values.Phone,
      };

      formData.append("RegisterInfo", JSON.stringify(registerInfo));

      // Add avatar if selected
      if (avatarFile) {
        formData.append("MainImageFile", avatarFile);
      }

      const result = await register({ formData }).unwrap();

      // Show verification modal
      setVerificationData({
        isModalOpen: true,
        verificationCode: "",
        email: values.Email,
        error: "",
      });
    } catch (error: any) {
      console.error("Register error:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Registration failed. Please try again.";
      setServerError(msg);
    }
  };

  const closeVerificationModal = () => {
    setVerificationData({
      isModalOpen: false,
      verificationCode: "",
      email: "",
      error: "",
    });
  };

  const handleVerifyAccount = async () => {
    setVerificationData((prev) => ({ ...prev, error: "" }));

    if (verificationData.verificationCode.length !== 6) {
      setVerificationData((prev) => ({
        ...prev,
        error: "Please enter the 6-digit verification code.",
      }));
      return;
    }

    try {
      await verifyAccount({
        Email: verificationData.email,
        VerifyCode: verificationData.verificationCode,
      }).unwrap();

      // Success - show alert and auto-redirect
      alert("Account verified successfully! Redirecting to login...");
      closeVerificationModal();

      setTimeout(() => {
        navigate("/auth/login");
      }, 5000);
    } catch (error: any) {
      console.error("Verification error:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Verification failed. Please try again.";
      setVerificationData((prev) => ({ ...prev, error: msg }));
    }
  };

  return (
    <div className="w-full h-screen overflow-hidden bg-[url(/background/login4.png)] object-cover bg-cover flex items-center justify-center">
      <div
        id="glass_container"
        className="text-white px-20 py-10 min-w-[900px] max-w-[1000px]"
      >
        <div className="w-full flex items-center justify-center">
          <img
            src="/images/logo/logo3.png"
            className="w-40 h-40 rounded-full object-cover bg-cover"
            alt="Logo"
          />
        </div>
        <p className="font-bold font-poppins text-2xl tracking-tight mb-1">
          Join the Darkness
        </p>
        <p className="text-sm opacity-80 mb-8">
          Create an account to get scared!
        </p>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-5">
            {/* Two Column Grid */}
            <div className="grid grid-cols-2 gap-x-8 gap-y-4">
              {/* Left Column */}

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
                        disabled={isRegistering}
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

              {/* Full Name */}
              <FormField
                control={form.control}
                name="FullName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Full Name</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="John Doe"
                        type="text"
                        autoComplete="name"
                        disabled={isRegistering}
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
                        autoComplete="new-password"
                        disabled={isRegistering}
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

              {/* Confirm Password */}
              <FormField
                control={form.control}
                name="ConfirmPassword"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Confirm Password</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="••••••••"
                        type="password"
                        autoComplete="new-password"
                        disabled={isRegistering}
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

              {/* Phone */}
              <FormField
                control={form.control}
                name="Phone"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Phone</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="+84 123 456 789"
                        type="tel"
                        autoComplete="tel"
                        disabled={isRegistering}
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

              {/* Gender */}
              <FormField
                control={form.control}
                name="Gender"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Gender</FormLabel>
                    <FormControl>
                      <select
                        disabled={isRegistering}
                        {...field}
                        className="
                          w-full
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
                          text-white
                          py-2
                          [&>option]:bg-[#333]
                          [&>option]:text-white
                        "
                      >
                        <option value="Male">Male</option>
                        <option value="Female">Female</option>
                        <option value="Other">Other</option>
                      </select>
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Date of Birth */}
              <FormField
                control={form.control}
                name="Dob"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Date of Birth</FormLabel>
                    <FormControl>
                      <Input
                        type="date"
                        disabled={isRegistering}
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

              {/* Address */}
              <FormField
                control={form.control}
                name="Address"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Address (Optional)</FormLabel>
                    <FormControl>
                      <Input
                        placeholder="123 Street, City"
                        type="text"
                        disabled={isRegistering}
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

              {/* Avatar Upload - Full Width */}
              <div className="col-span-2">
                <div className="flex flex-col items-start gap-2">
                  <FormLabel>Avatar (Optional)</FormLabel>
                  <div className="flex items-center gap-4 w-full">
                    <div className="w-24 h-24 rounded-lg border-2 border-dashed border-mystic-green/50 flex items-center justify-center overflow-hidden bg-black/20 flex-shrink-0">
                      {avatarPreview ? (
                        <img
                          src={avatarPreview}
                          alt="Avatar preview"
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <div className="text-center p-2">
                          <p className="text-xs text-gray-400">Avatar</p>
                        </div>
                      )}
                    </div>
                    <label
                      htmlFor="avatar-upload"
                      className="cursor-pointer text-sm text-mystic-green hover:underline"
                    >
                      Choose Image
                    </label>
                    <input
                      id="avatar-upload"
                      type="file"
                      accept="image/*"
                      onChange={handleAvatarChange}
                      className="hidden"
                      disabled={isRegistering}
                    />
                  </div>
                </div>
              </div>
            </div>

            {/* Server error */}
            {serverError && (
              <div className="text-red-300 text-sm">{serverError}</div>
            )}

            <Button
              type="submit"
              className="w-full mt-2"
              disabled={isRegistering || !form.formState.isValid}
            >
              {isRegistering ? "Creating account..." : "Sign Up"}
            </Button>
          </form>
        </Form>

        {/* Navigation */}
        <div className="w-full flex flex-col items-center gap-3 py-4 mt-5">
          <div className="w-full flex items-center justify-center px-4">
            <p className="text-sm text-gray-300">
              Already have an account?{" "}
              <span
                onClick={() => navigate("/auth/login")}
                className="text-mystic-green ml-1 font-bold hover:underline cursor-pointer"
              >
                Sign In
              </span>
            </p>
          </div>
        </div>
      </div>

      {/* Verification Dialog */}
      <Dialog
        open={verificationData.isModalOpen}
        onOpenChange={(open) => {
          if (!open) closeVerificationModal();
        }}
      >
        <DialogContent className="sm:max-w-[420px] border border-white/10 bg-black/80 text-white">
          <DialogHeader>
            <DialogTitle className="text-mystic-green">
              Verify Your Account
            </DialogTitle>
            <DialogDescription className="text-gray-200">
              Enter the 6-digit verification code sent to{" "}
              {verificationData.email}
            </DialogDescription>
          </DialogHeader>

          <div className="flex w-full items-center justify-center py-2">
            <InputOTP
              maxLength={6}
              value={verificationData.verificationCode}
              onChange={(val) => {
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

          {verificationData.error && (
            <p className="text-red-400 text-sm text-center">
              {verificationData.error}
            </p>
          )}

          <DialogFooter className="gap-2">
            <Button
              variant="outline"
              className="border-white/20 text-black bg-white hover:bg-white/10"
              onClick={closeVerificationModal}
              disabled={isVerifying}
            >
              Cancel
            </Button>
            <Button
              className="bg-mystic-green text-black font-semibold hover:bg-mystic-green/90"
              onClick={handleVerifyAccount}
              disabled={
                isVerifying || verificationData.verificationCode.length !== 6
              }
            >
              {isVerifying ? "Verifying..." : "Verify"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default RegisterPage;
