import { AlertType } from "../features/alert/alertSlice";

type AlertMessage = {
  title: string;
  description: string;
  type: AlertType;
};

export const alertMessages: Record<string, AlertMessage> = {
  // LISTEN
  // 1. Fall back chung cho các lỗi không xác định khi nghe
  "listen-failed-1": {
    title: "Listening Failed",
    description:
      "Unable to start listening to the audio. Please try again later.",
    type: "error",
  },
  // 2. Episode đã bị gỡ khỏi hệ thống
  "listen-failed-2": {
    title: "Episode Unavailable",
    description: "This episode is no longer available for listening.",
    type: "error",
  },
  // 3. Chưa đăng ký gói trả phí và hiện show chứa episode này có gói để đăng ký
  "listen-failed-3": {
    title: "Subscription Required",
    description:
      "You need to subscribe to a paid plan to listen to this episode.",
    type: "error",
  },
  // 4. Chưa đăng ký gói trả phí và hiện show chứa episode này chưa có gói để đăng ký
  "listen-failed-4": {
    title: "Subscription Required",
    description: "This episode requires a subscription to be played.",
    type: "error",
  },
  // 5. Thiếu điều kiện để nghe (gói mà người dùng đăng ký chưa đủ quyền)
  "listen-failed-5": {
    title: "Insufficient Subscription",
    description:
      "Your current subscription does not grant access to this episode. Reasons: \n",
    type: "error",
  },

  // LOGIN
  // 1. Lỗi chung khi đăng nhập thất bại
  "login-failed-1": {
    title: "Login Failed",
    description: "Unable to login. Please try again.",
    type: "error",
  },
  // 2. Tài khoản sai thông tin hoặc không tồn tại
  "login-failed-2": {
    title: "Login Failed",
    description: "Incorrect email or password.",
    type: "error",
  },
  // 3. Tài khoản bị khóa
  "login-failed-3": {
    title: "Account Deactivated",
    description:
      "Your account has been deactivated. Please contact support for assistance.",
    type: "error",
  },
  // 4. Tài khoản chưa được xác thực
  "login-failed-4": {
    title: "Account Not Verified",
    description:
      "Your account is not verified. Please re-create your account and verify via your email.",
    type: "warning",
  },
  // 5. Không phải tài khoản khách hàng
  "login-failed-5": {
    title: "Invalid Account",
    description: "This account is not registered as a customer account. ",
    type: "error",
  },
};

export const benefitTransformDescriptions: Record<string, string> = {
  ShowsEpisodesEarlyAccess: "This Episode Is Only For Early Access Members",
  NonQuotaListening: "You're out of listen slot, please wait for renewal.",
  SubscriberOnlyShows: "This Show Is Only For Subscriber-Only Members",
  SubscriberOnlyEpisodes: "This Episode Is Only For Subscriber-Only Members",
  BonusEpisodes: "This Episode Is Only For Bonus-Episode Members",
  ArchiveEpisodesAccess: "This Episode Is Only For Archive-Episode Members",
};
