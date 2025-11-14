import DeviceDetector from "device-detector-js";

const WithDrawPage = () => {
  const deviceDetector = new DeviceDetector();
  const device = deviceDetector.parse(navigator.userAgent);

  console.log(device);
  const deviceInformations = {
    DeviceId: "1111",
    Platform: device.device?.type,
    OSName: device.os?.name,
  };
  console.log("Informations: ", deviceInformations);
  return (
    <div>
      <h1>Withdraw Page</h1>
    </div>
  );
};

export default WithDrawPage;
