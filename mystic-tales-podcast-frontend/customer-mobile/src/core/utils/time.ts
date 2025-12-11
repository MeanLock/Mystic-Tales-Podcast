const formatAudioLength = (
  seconds: number,
  variant: "numberOnly" | "withText" | "minuteOnly" = "numberOnly"
): string => {
  const hours = Math.floor(seconds / 3600);
  const minutes = Math.floor((seconds % 3600) / 60);
  const remainingSeconds = Math.floor(seconds % 60);

  if (variant === "minuteOnly") {
    // Format: 17m (always show minutes, round up if has seconds)
    const totalMinutes = Math.ceil(seconds / 60);
    return `${totalMinutes}m`;
  }

  if (variant === "withText") {
    // Format: 1h2m35s or 2m35s or 35s
    if (hours > 0) {
      return `${hours}h${minutes}m${remainingSeconds}s`;
    } else if (minutes > 0) {
      return `${minutes}m${remainingSeconds}s`;
    } else {
      return `${remainingSeconds}s`;
    }
  }

  // variant === "numberOnly" (default)
  // Format: xx:yy:zz or yy:zz or 00:zz
  if (hours > 0) {
    return `${hours.toString().padStart(2, "0")}:${minutes
      .toString()
      .padStart(2, "0")}:${remainingSeconds.toString().padStart(2, "0")}`;
  } else if (minutes > 0) {
    return `${minutes.toString().padStart(2, "0")}:${remainingSeconds
      .toString()
      .padStart(2, "0")}`;
  } else {
    return `00:${remainingSeconds.toString().padStart(2, "0")}`;
  }
};

const TimeUtil = {
  formatAudioLength,
};

export default TimeUtil;
