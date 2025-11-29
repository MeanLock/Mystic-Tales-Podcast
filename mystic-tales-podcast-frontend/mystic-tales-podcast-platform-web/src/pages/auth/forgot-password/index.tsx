import { useResetForgotPasswordMutation } from "@/core/services/auth/auth.service";
import { useEffect, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
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

const ResetPasswordSchema = z
  .object({
    NewPassword: z
      .string()
      .min(8, "Password must be at least 8 characters")
      .regex(
        /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)/,
        "Password must contain uppercase, lowercase, and number"
      ),
    ConfirmPassword: z.string().min(1, "Please confirm your password"),
  })
  .refine((data) => data.NewPassword === data.ConfirmPassword, {
    message: "Passwords do not match",
    path: ["ConfirmPassword"],
  });

type ResetPasswordFormValues = z.infer<typeof ResetPasswordSchema>;

const ForgotPasswordPage = () => {
  const [searchParams] = useSearchParams();
  const email = searchParams.get("email");
  const token = searchParams.get("token");

  const [serverError, setServerError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // HOOKS
  const navigate = useNavigate();
  const [resetPassword, { isLoading }] = useResetForgotPasswordMutation();

  const form = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(ResetPasswordSchema),
    defaultValues: { NewPassword: "", ConfirmPassword: "" },
    mode: "onTouched",
  });

  useEffect(() => {
    if (!email || !token) {
      navigate("/auth/login");
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const onSubmit = async (values: ResetPasswordFormValues) => {
    if (!email || !token) return;

    setServerError(null);
    setSuccessMessage(null);

    try {
      const result = await resetPassword({
        Email: email,
        ResetPasswordToken: token,
        NewPassword: values.NewPassword,
      }).unwrap();

      setSuccessMessage(
        result?.Message ||
          "Password reset successfully! Redirecting to login..."
      );

      // Navigate to login after 2 seconds
      setTimeout(() => {
        navigate("/auth/login");
      }, 2000);
    } catch (error: any) {
      console.error("Reset password error:", error);
      const msg =
        error?.data?.Message ||
        error?.message ||
        "Failed to reset password. Please try again or request a new reset link.";
      setServerError(msg);
    }
  };

  return (
    <div className="w-full h-screen overflow-hidden bg-[url(/background/login4.png)] object-cover bg-cover flex items-center justify-center">
      <div
        id="glass_container"
        className="text-white px-20 py-10 min-w-[500px]"
        style={{
          background: "rgba(0, 0, 0, 0.4)",
          backdropFilter: "blur(10px)",
          borderRadius: "20px",
          border: "1px solid rgba(255, 255, 255, 0.1)",
        }}
      >
        <div className="w-full flex items-center justify-center">
          <img
            src="/images/logo/logo3.png"
            className="w-52 h-52 rounded-full object-cover bg-cover"
            alt="Logo"
          />
        </div>
        <p className="font-bold font-poppins text-2xl tracking-tight mb-1 text-center">
          Reset Your Password
        </p>
        <p className="text-sm opacity-80 mb-10 text-center">
          Enter your new password below
        </p>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {/* New Password */}
            <FormField
              control={form.control}
              name="NewPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>New Password</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Enter new password"
                      type="password"
                      autoComplete="new-password"
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

            {/* Confirm Password */}
            <FormField
              control={form.control}
              name="ConfirmPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Confirm Password</FormLabel>
                  <FormControl>
                    <Input
                      placeholder="Re-enter new password"
                      type="password"
                      autoComplete="new-password"
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

            {/* Success message */}
            {successMessage && (
              <div className="text-emerald-400 text-sm">{successMessage}</div>
            )}

            <Button
              type="submit"
              className="w-full"
              disabled={isLoading || !form.formState.isValid}
            >
              {isLoading ? "Resetting..." : "Reset Password"}
            </Button>
          </form>
        </Form>

        {/* Navigation */}
        <div className="w-full flex flex-col items-center gap-3 py-4 mt-5">
          <div className="w-full flex items-center justify-center px-4">
            <p className="text-sm text-gray-300">
              Remember your password?{" "}
              <span
                onClick={() => navigate("/auth/login")}
                className="text-mystic-green ml-1 font-bold hover:underline cursor-pointer"
              >
                Back to Login
              </span>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ForgotPasswordPage;
