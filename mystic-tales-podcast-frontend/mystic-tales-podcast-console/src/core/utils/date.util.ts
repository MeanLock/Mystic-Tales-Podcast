export const formatDate = (dateString?: string): string => {
  if (!dateString || dateString === '' || dateString === null) return '---';

  const date = new Date(dateString);

  const day = String(date.getUTCDate()).padStart(2, '0');
  const month = String(date.getUTCMonth() + 1).padStart(2, '0'); // Tháng 0-indexed
  const year = date.getUTCFullYear();

  return `${day}-${month}-${year}`;
};
export const getTimeAgo = (dateString: string): string => {
  console.log("Date String: ", dateString);
  const date = new Date(dateString);
  const now = new Date();
  const diffInDays = Math.floor(
    (now.getTime() - date.getTime()) / (1000 * 60 * 60 * 24)
  );

  if (diffInDays === 0) return "Today";
  if (diffInDays === 1) return "1 day ago";
  return `${diffInDays} days ago`;
};
export const fromInputDateToISO = (value?: string | null) =>
  value && value.length === 10 ? `${value}T00:00:00.000Z` : null