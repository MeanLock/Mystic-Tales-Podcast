import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";

// Redux
import { Provider } from "react-redux";
import { PersistGate } from "redux-persist/integration/react";

import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import NormalLayout from "./layouts/normal/index.tsx";
import HomePage from "./pages/normal/home/index.tsx";
import FAQPage from "./pages/normal/faqs/index.tsx";
import AboutPage from "./pages/normal/about/index.tsx";
import AuthLayout from "./layouts/auth/index.tsx";
import LoginPage from "./pages/auth/login/index.tsx";
import RegisterPage from "./pages/auth/register/index.tsx";
import ForgotPasswordPage from "./pages/auth/forgot-password/index.tsx";
import { store, persistor } from "./redux/store.ts";
import MediaPlayerLayout from "./layouts/mediaPlayer/index.tsx";
import DiscoveryPage from "./pages/mediaPlayer/discovery/index.tsx";

// Hệ thống route
// 1. Normal Layout: có header sticky ở giữa.
// - /home (HomePage): Trang Home chính, landing page

// - /faqs (FAQPage): Trang FAQs, các câu hỏi thường gặp

// - /explore (ExplorePage): Trang Explore, khám phá

// - /about (AboutPage): Trang About Us.

//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------

// 2. Auth Layout: Layout cơ bản, không tùy chỉnh gì cả
// - /auth/login (LoginPage): Trang Đăng Nhập.

// - /auth/register (RegisterPage): Trang Đăng Ký, handle Verification Code trong đây luôn.

// - /auth/forgot-password/step-[ste] (ForgotPasswordPage): Trang Forgot Password.

//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------
//-----------------------------------------------------------------------------------------

// 3. Media Player Layout: Layout gồm sidebar bên trái, MediaPlayerControlBar ở dưới, content ở bên phải
// - /media-player/discovery (DiscoveryPage): Trang Discovery, Gợi ý các channels/shows/episodes/podcasters từ các hành vi cũ

// - /media-player/trending (TrendingPage): Trang để các trending shows, các top shows/episodes/categories.

// - /media-player/search

// - /media-player/categories (categoriesPage): Trang để search & filter, đi từ filter categories trước sau đó mới đến search.
//    -- /media-player/categories/[id] (categoriesDetailsPage): Trang show các sub-categories thuộc categories đó và các episodes liên quan.

// - /media-player/podcasters (PodcasterPage): Trang show tất cả các Podcasters, filter theo categories và search, sort
//    -- /media-player/podcasters/[id] (PodcasterDetailsPage): Tại đây có nút booking, danh sách các channels, shows của podcaster.

// - /media-player/channels (ChannelsPage): Danh sách các shows của kênh, vẫn có thể lọc được, search & sort
//    -- /media-player/channels/[id] (ChannelDetailsPage): Danh sách các shows bên trong, giá subscription, informations, ...
//        --- /media-player/channels/[id]/shows

// - /media-player/shows (ShowsPage): Danh sách các shows, vẫn có thể lọc được, search & sort
//    -- /media-player/shows/[id] (ShowDetailsPage): Danh sách các episodes bên trong, giá subscription, ...
//        --- /media-player/shows/[id]/episodes

// - /media-player/episodes (EpisodesPage): Cần phải biết được từ 1 show nào hoặc top episodes mới qua được.
//    -- /media-player/episodes/[id] (EpisodeDetailsPage): Nhấn vào có thông tin, review...

// - /media-player/library/recent (RecentPage): Danh sách tiếp tục xem các shows, ...

// - /media-player/library/saved (SavedEpisodesPage): Danh sách các episodes đã saved.

// - /media-player/library/subscribed-channels (subscribedChannelsPage): Danh sách các channels đã subscribed.

// - /media-player/library/subscribed-shows (subscribedShowsPage): Danh sách các shows đã subscribed.

// - /media-player/library/followed-podcasters (FollowedPodcastersPage): Danh sách các podcasters đã followed

// - /media-player/management/bookings (BookingsPage): Danh sách các bookings của bản thân.
//    -- /media-player/management/bookings/[id] (BookingDetailsPage): Chi tiết thông tin của 1 booking.

// - /media-player/management/transactions (TransactionsLayout): Layout nhỏ để quản lý các transactions, có các tabs và content ở bên dưới ứng với tab
//    -- /media-player/management/transactions/payment-confirmation (PaymentConfirmationPage): Trang hiển thị thông tin chi tiết của payment và đợi xác nhận thanh toán để thanh toán
//    -- /media-player/management/transactions/payment-result (PaymentResultPage): Trang hiển thị kết quả giao dịch
//    -- /media-player/management/transactions/top-up (TopUpPage): Nạp tiền, coi lịch sử nạp vào ví
//    -- /media-player/management/transactions/withdraw (WithDrawPage): Tạo yêu cầu rút tiền, coi lịch sử & trạng thái của các requests
//    -- /media-player/management/transactions/subscriptions (subscriptionsPaymentPage): Quản lý các gói subscriptions, un-scribe 1 gói, xem thông tin các gói sắp tới phải trả, xem thay đổi của 1 gói subscription.

// - /media-player/management/profile (ProfilePage): Quản lý profile

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <Provider store={store}>
      <PersistGate loading={null} persistor={persistor}>
        <BrowserRouter>
          <Routes>
            {/* 1️⃣ NORMAL LAYOUT */}
            <Route element={<NormalLayout />}>
              <Route path="/" element={<Navigate to="/home" replace />} />
              <Route path="/home" element={<HomePage />} />
              <Route path="/faqs" element={<FAQPage />} />
              <Route path="/about" element={<AboutPage />} />
            </Route>

            {/* 2️⃣ AUTH LAYOUT */}
            <Route path="/auth" element={<AuthLayout />}>
              <Route path="login" element={<LoginPage />} />
              <Route path="register" element={<RegisterPage />} />
              <Route
                path="forgot-password/step-:step"
                element={<ForgotPasswordPage />}
              />
            </Route>

            <Route path="/media-player" element={<MediaPlayerLayout />}>
              <Route path="discovery" element={<DiscoveryPage />} />
            </Route>
          </Routes>
        </BrowserRouter>
      </PersistGate>
    </Provider>
  </StrictMode>
);
