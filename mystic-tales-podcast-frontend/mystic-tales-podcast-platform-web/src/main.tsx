import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import "./index.css";
import { GoogleOAuthProvider } from "@react-oauth/google";
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
import TrendingPage from "./pages/mediaPlayer/trending/index.tsx";
import CategoryPage from "./pages/mediaPlayer/category/index.tsx";
import CategoryDetailsPage from "./pages/mediaPlayer/category/details/index.tsx";
import SearchPage from "./pages/mediaPlayer/search/index.tsx";
import PodcastersPage from "./pages/mediaPlayer/podcasters/index.tsx";
import PodcasterDetailsPage from "./pages/mediaPlayer/podcasters/details/index.tsx";
import RecentPage from "./pages/mediaPlayer/library/recent/index.tsx";
import SavedPage from "./pages/mediaPlayer/library/saved/index.tsx";
import SubscribedChannelsPage from "./pages/mediaPlayer/library/subscribed-channels/index.tsx";
import SubscribedShowsPage from "./pages/mediaPlayer/library/subscribed-shows/index.tsx";
import FollowedPodcastersPage from "./pages/mediaPlayer/library/followed-podcasters/index.tsx";
import BookingsPage from "./pages/mediaPlayer/management/booking/index.tsx";
import BookingDetailsPage from "./pages/mediaPlayer/management/booking/details/index.tsx";
import TopUpPage from "./pages/mediaPlayer/management/transaction/top-up/index.tsx";
import WithDrawPage from "./pages/mediaPlayer/management/transaction/withdraw/index.tsx";
import ManagementSubscriptionsPage from "./pages/mediaPlayer/management/transaction/subscriptions/index.tsx";
import ProfilePage from "./pages/mediaPlayer/management/profile/index.tsx";
import ChannelDetailsPage from "./pages/mediaPlayer/channels/details/index.tsx";
import ShowDetailsPage from "./pages/mediaPlayer/shows/details/index.tsx";
import CreateBookingPage from "./pages/mediaPlayer/management/booking/create/index.tsx";
import BecomePodcaster from "./pages/mediaPlayer/management/profile/becomePodcaster/index.tsx";
import PaymentResultPage from "./pages/mediaPlayer/management/transaction/payment-result/index.tsx";
import CompletedBookingsPage from "./pages/mediaPlayer/management/booking/completed/index.tsx";
import CompletedBookingDetailsPage from "./pages/mediaPlayer/management/booking/completed/details/index.tsx";
import EpisodeListPage from "./pages/mediaPlayer/episodes/index.tsx";
import EpisodeDetailsPage from "./pages/mediaPlayer/episodes/details/index.tsx";
import AlertModal from "./components/alert/AlertModal.tsx";
import AccountBalanceChangePage from "./pages/mediaPlayer/management/transaction/account-balance/index.tsx";
import ProtectedRoute from "./route/ProtectedRoute.tsx";

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <GoogleOAuthProvider clientId={import.meta.env.VITE_GOOGLE_CLIENT_ID}>
      <Provider store={store}>
        <PersistGate loading={null} persistor={persistor}>
          {/* <ErrorModal /> */}
          <AlertModal />
          <BrowserRouter>
            <Routes>
              {/* 1️⃣ NORMAL LAYOUT */}
              <Route element={<NormalLayout />}>
                <Route path="/" element={<Navigate to="/home" replace />} />
                <Route path="/home" element={<HomePage />} />
                <Route path="/faqs" element={<FAQPage />} />
                <Route path="/about" element={<AboutPage />} />
                <Route element={<ProtectedRoute />}>
                  <Route
                    path="/become-podcaster"
                    element={<BecomePodcaster />}
                  />
                </Route>
              </Route>

              {/* 2️⃣ AUTH LAYOUT */}
              <Route path="/auth" element={<AuthLayout />}>
                <Route path="login" element={<LoginPage />} />
                <Route path="register" element={<RegisterPage />} />
                <Route
                  path="forgot-password"
                  element={<ForgotPasswordPage />}
                />
              </Route>

              <Route path="/media-player" element={<MediaPlayerLayout />}>
                {/* Normal */}
                <Route path="discovery" element={<DiscoveryPage />} />
                <Route path="trending" element={<TrendingPage />} />
                <Route path="categories" element={<CategoryPage />} />
                <Route
                  path="categories/:id"
                  element={<CategoryDetailsPage />}
                />
                <Route path="search" element={<SearchPage />} />
                <Route path="podcasters" element={<PodcastersPage />} />
                <Route
                  path="podcasters/:id"
                  element={<PodcasterDetailsPage />}
                />
                <Route path="channels/:id" element={<ChannelDetailsPage />} />
                <Route path="shows/:id" element={<ShowDetailsPage />} />

                <Route path="episodes" element={<EpisodeListPage />} />
                <Route path="episodes/:id" element={<EpisodeDetailsPage />} />
                {/* Protected Route */}
                <Route element={<ProtectedRoute />}>
                  <Route
                    path="library/listening-history"
                    element={<RecentPage />}
                  />
                  <Route path="library/saved" element={<SavedPage />} />
                  <Route
                    path="library/subscribed-channels"
                    element={<SubscribedChannelsPage />}
                  />
                  <Route
                    path="library/subscribed-shows"
                    element={<SubscribedShowsPage />}
                  />
                  <Route
                    path="library/followed-podcasters"
                    element={<FollowedPodcastersPage />}
                  />
                  <Route
                    path="management/bookings"
                    element={<BookingsPage />}
                  />
                  <Route
                    path="management/completed-bookings"
                    element={<CompletedBookingsPage />}
                  />
                  <Route
                    path="management/bookings/create"
                    element={<CreateBookingPage />}
                  />
                  <Route path="management/profile" element={<ProfilePage />} />
                  <Route
                    path="management/bookings/:id"
                    element={<BookingDetailsPage />}
                  />
                  <Route
                    path="management/completed-bookings/:id"
                    element={<CompletedBookingDetailsPage />}
                  />
                  <Route
                    path="management/transactions/top-up"
                    element={<TopUpPage />}
                  />
                  <Route
                    path="management/transactions/payment-result"
                    element={<PaymentResultPage />}
                  />
                  <Route
                    path="management/transactions/withdraw"
                    element={<WithDrawPage />}
                  />
                  <Route
                    path="management/transactions/subscriptions"
                    element={<ManagementSubscriptionsPage />}
                  />
                  <Route
                    path="management/transactions/subscriptions"
                    element={<ManagementSubscriptionsPage />}
                  />
                  <Route
                    path="management/transactions/account-balance"
                    element={<AccountBalanceChangePage />}
                  />
                </Route>
              </Route>
            </Routes>
          </BrowserRouter>
        </PersistGate>
      </Provider>
    </GoogleOAuthProvider>
  </StrictMode>
);
