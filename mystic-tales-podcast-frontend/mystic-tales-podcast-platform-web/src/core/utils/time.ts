type formatDateInput =
  | "DD/MM/YYYY"
  | "MM/DD/YYYY"
  | "YYYY-MM-DD"
  | "mm:ssDD/MMM/YYYY"
  | "hh:mm:ssDD/MM/YYYY"
  | "hh:mmDD/MM/YYYY";

export const formatDate = (dateString: string, format: formatDateInput) => {
  if (!dateString) return "Not yet";
  const date = new Date(dateString);
  if (isNaN(date.getTime())) return "";

  // Lấy các thành phần thời gian
  const dd = String(date.getDate()).padStart(2, "0");
  const mm = String(date.getMonth() + 1).padStart(2, "0");
  const yyyy = date.getFullYear();
  const hh = String(date.getHours()).padStart(2, "0");
  const min = String(date.getMinutes()).padStart(2, "0");
  const ss = String(date.getSeconds()).padStart(2, "0");

  // Tên tháng rút gọn (Jan, Feb, Mar, …)
  const monthNames = [
    "Jan",
    "Feb",
    "Mar",
    "Apr",
    "May",
    "Jun",
    "Jul",
    "Aug",
    "Sep",
    "Oct",
    "Nov",
    "Dec",
  ];
  const mmm = monthNames[date.getMonth()];

  switch (format) {
    case "DD/MM/YYYY":
      return `${dd}/${mm}/${yyyy}`;
    case "MM/DD/YYYY":
      return `${mm}/${dd}/${yyyy}`;
    case "YYYY-MM-DD":
      return `${yyyy}-${mm}-${dd}`;
    case "mm:ssDD/MMM/YYYY":
      return `${min}:${ss} ${dd}/${mmm}/${yyyy}`;
    case "hh:mm:ssDD/MM/YYYY":
      return `${hh}:${min}:${ss} ${dd}/${mm}/${yyyy}`;
    case "hh:mmDD/MM/YYYY":
      return `${hh}:${min} ${dd}/${mm}/${yyyy}`;
    default:
      return "";
  }
};

const formatAudioLength = (lengthInSeconds: number) => {
  if (isNaN(lengthInSeconds) || lengthInSeconds < 0) return "00:00";
  const hours = Math.floor(lengthInSeconds / 3600);
  const minutes = Math.floor((lengthInSeconds % 3600) / 60);
  const seconds = Math.floor(lengthInSeconds % 60);
  const hh = String(hours).padStart(2, "0");
  const mm = String(minutes).padStart(2, "0");
  const ss = String(seconds).padStart(2, "0");
  return hours > 0 ? `${hh}:${mm}:${ss}` : minutes > 0 ? `${mm}:${ss}` : `00:${ss}`;
};

const TimeUtil = {
  formatDate,
  formatAudioLength,
};

export { TimeUtil };
