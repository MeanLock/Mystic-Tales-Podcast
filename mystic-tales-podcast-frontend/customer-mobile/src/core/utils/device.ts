import { Platform } from "react-native";
import * as Device from "expo-device";
import * as SecureStore from "expo-secure-store";
import "react-native-get-random-values";
import { v4 as uuidv4 } from "uuid";

const DEVICE_ID_KEY = "device_id"; // giống tên key trên web

function generateRandomDeviceId() {
  // Sử dụng UUID v4 để tạo device ID unique
  return uuidv4();
}

/**
 * Lấy DeviceId:
 * - Nếu đã có trong SecureStore -> dùng lại
 * - Nếu chưa có -> random mới rồi lưu
 */
async function getStoredDeviceId() {
  let deviceId = await SecureStore.getItemAsync(DEVICE_ID_KEY);
  if (!deviceId) {
    deviceId = generateRandomDeviceId();
    await SecureStore.setItemAsync(DEVICE_ID_KEY, deviceId);
  }
  return deviceId;
}

export async function getCapacitorDevice() {
  const DeviceId = await getStoredDeviceId(); // <--- chỗ này thay vì ""
  const PlatformName = Platform.OS;
  const OSName = Device.osName ?? PlatformName;

  return {
    DeviceId,
    Platform: PlatformName,
    OSName,
  };
}

export async function getDeviceInfoToken() {
  return await SecureStore.getItemAsync("device_info_token");
}
