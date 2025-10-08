export const formatDate = (dateString?: string): string => {
  if (dateString === undefined || dateString === null) return '---';
  return new Date(dateString).toLocaleDateString("vi-VN");
};
