// app/index.tsx
import { Redirect, Stack } from "expo-router";

// Tắt animation riêng cho index bằng cách khai báo options ngay trong file
export const unstable_settings = {};
export default function Index() {
  return <Redirect href="/(tabs)/home" />; // replace, không push
}

// Để chắc ăn trên native, có thể (tùy chọn) export cấu hình screen tại chỗ:
Index.options = { animation: "none", headerShown: false } as const;
