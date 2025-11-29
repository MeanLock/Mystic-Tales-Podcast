import { Device } from "@capacitor/device";

export async function getCapacitorDevice() {
  const info = await Device.getInfo();
  const id = await Device.getId();

  return {
    DeviceId: id.identifier,
    Platform: info.platform,
    OSName: info.operatingSystem,
    Model: info.model,
    IsVirtual: info.isVirtual,
  };
}

export function getDeviceInfoToken() {
  return localStorage.getItem("device_info_token") || null;
}
