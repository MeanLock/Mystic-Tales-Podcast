import { View } from "@/src/components/ui/View";
import MixxingText from "@/src/components/ui/MixxingText";
import { Text } from "@/src/components/ui/Text";
import { Pressable } from "react-native";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { logoutLocal } from "@/src/features/auth/authSlice";
import { tokenStore } from "@/src/features/auth/tokenStore";

import { useRouter } from "expo-router";
import { stopAll } from "@/src/features/mediaPlayer/playerSlice";

export default function Profile() {
  const authState = useSelector((state: RootState) => state.auth);
  const dispatch = useDispatch();
  const router = useRouter();
  const handleLogout = async () => {
    try {
      // Clear tokens from secure storage
      await tokenStore.clearAll();
      // Update Redux state
      dispatch(logoutLocal());
      dispatch(stopAll());
      // Navigate to login screen
      router.replace("/(auth)/login");
    } catch (error) {
      console.error("Logout error:", error);
    }
  };

  return (
    <View className="flex-1 p-4">
      {/* Content gì thì để ở đây */}
      <Text className="text-xl font-bold mb-4">Profile Screen</Text>
      {authState.user && (
        <Text className="mb-4">
          Welcome, {authState.user.FullName || "User"}!
        </Text>
      )}

      <Pressable
        className="bg-red-500 p-4 rounded-lg mt-4 items-center"
        onPress={handleLogout}
      >
        <Text className="text-white font-semibold">Log Out</Text>
      </Pressable>
      <View style={{ height: 50 }}></View>
    </View>
  );
}
