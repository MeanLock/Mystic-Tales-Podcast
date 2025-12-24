export const formatAudioLength = (length: number): string => {
  // Làm tròn số giây
  const totalSeconds = Math.floor(length);

  // Tính số giờ, phút, giây
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const seconds = totalSeconds % 60;

  // Format với padding 2 chữ số
  const pad = (num: number) => num.toString().padStart(2, "0");

  // Tạo chuỗi kết quả
  if (hours > 0) {
    // Có giờ: "xx h yy m zz s"
    return `${pad(hours)}:${pad(minutes)}:${pad(seconds)}`;
  } else if (minutes > 0) {
    // Chỉ có phút: "yy m zz s"
    return `${pad(minutes)}:${pad(seconds)}`;
  } else {
    // Chỉ có giây: "zz s"
    return `00:${pad(seconds)}`;
  }
};

export const formatDateRange = (date: string): string => {
  // Parse ISO string thành Date object
  const inputDate = new Date(date);
  const now = new Date();

  // Tính khoảng cách thời gian (milliseconds)
  const diffMs = now.getTime() - inputDate.getTime();
  const diffHours = diffMs / (1000 * 60 * 60);
  const diffDays = diffMs / (1000 * 60 * 60 * 24);

  // Chưa tới 1 ngày (< 24 giờ): "xx hours ago"
  if (diffDays < 1) {
    const hours = Math.floor(diffHours);
    if (hours === 0) {
      const diffMinutes = Math.floor(diffMs / (1000 * 60));
      if (diffMinutes === 0) {
        return "just now";
      }
      return `${diffMinutes} ${diffMinutes === 1 ? "minute" : "minutes"} ago`;
    }
    return `${hours} ${hours === 1 ? "hour" : "hours"} ago`;
  }

  // Từ 1 - 5 ngày: "xx days ago"
  if (diffDays >= 1 && diffDays <= 5) {
    const days = Math.floor(diffDays);
    return `${days} ${days === 1 ? "day" : "days"} ago`;
  }

  // Hơn 5 ngày: "DD/MM/YYYY"
  const day = inputDate.getDate().toString().padStart(2, "0");
  const month = (inputDate.getMonth() + 1).toString().padStart(2, "0");
  const year = inputDate.getFullYear();

  return `${day}/${month}/${year}`;
};
