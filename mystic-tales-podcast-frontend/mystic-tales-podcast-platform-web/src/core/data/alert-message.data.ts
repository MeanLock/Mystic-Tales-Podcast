import type { AlertMessage } from "../types/alert";

export const alertMessages: AlertMessage[] = [
  // Lỗi chưa đăng nhập
  {
    id: "unauthorized",
    type: "error",
    title: "Login Required",
    description: "You need to login to perform this action.",
  },
  {
    id: "login-success",
    type: "success",
    title: "Login Successful",
    description: "Login Success, welcome back!",
  },
  // Các trường hợp lỗi đăng nhập khác
  {
    id: "login-failed-1",
    type: "error",
    title: "Login Failed",
    description: "Something went wrong. Please try again.",
  },
  // Lỗi sai password hoặc email (account not found, invalid password)
  {
    id: "login-failed-2",
    type: "error",
    title: "Login Failed",
    description: "Invalid email or password. Please try again.",
  },
  // Lỗi account bị deactivated
  {
    id: "login-failed-3",
    type: "error",
    title: "Login Failed",
    description: "Your account has been deactivated. Please contact support.",
  },
  // Lỗi account chưa verified
  {
    id: "login-failed-4",
    type: "error",
    title: "Login Failed",
    description:
      "Your account hasn't been verified. Please create account again and verify it.",
  },
  // Sai role
  {
    id: "login-failed-5",
    type: "error",
    title: "Login Failed",
    description: "You do not have permission to access this platform.",
  },
  // Lỗi chung khi đăng ký tài khoản
  {
    id: "register-failed-1",
    type: "error",
    title: "Registration Failed",
    description: "Something went wrong. Please try again.",
  },
  // Lỗi chung khi listen
  {
    id: "listen-failed-1",
    type: "error",
    title: "Listen Failed",
    description: "Something went wrong while loading the audio. Please try again.",
  },
  // Lỗi khi episode bị deactivated
  {
    id: "listen-failed-2",
    type: "error",
    title: "Listen Failed",
    description: "This episode is no longer available. Please choose another episode.",
  },
  // Lỗi khi chưa đủ điều kiện nghe và không có gói subscription available, chưa đăng ký
  {
    id: "listen-failed-3",
    type: "error",
    title: "Listen Failed",
    description: "You need to subscribe to listen to this episode.",
  },
  // Lỗi khi chưa đủ điều kiện nghe và có gói subscription available, chưa đăng ký
  {
    id: "listen-failed-4",
    type: "error",
    title: "Listen Failed",
    description:
      "This audio requires a subscription plan to listen.",
  },
  // Lỗi khi chưa đủ điều kiện nghe, có gói subscription available, có registration, nhưng gói không có đủ quyền mà episode yêu cầu
  {
    id: "listen-failed-5",
    type: "error",
    title: "Listen Failed",
    description:
      "Your current subscription plan does not have access to this episode. Because: ",
  },
];

export const MissingBenefitMessageTransforms: Record<string, string> = {
    "ShowsEpisodesEarlyAccess": "This episode is available for early access members only.",
    "NonQuotaListening": "You are out of listening quota",
    "SubscriberOnlyShows": "This episode belongs to subscriber-only shows",
    "SubscriberOnlyEpisodes": "This episode is available for subscribers only",
    "BonusEpisodes": "This is a bonus episode available for bonus access members only",
    "ArchiveEpisodesAccess": "This episode is part of the archive episodes available for archive access members only",
}
