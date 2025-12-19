/**
 * Lấy description ngắn gọn với dấu "..."
 * @param description - Mô tả đầy đủ
 * @param maxLength - Độ dài tối đa (mặc định: 100)
 * @returns Description đã được cắt ngắn
 */
export function getTruncatedDescription(
  description: string | null,
  maxLength: number = 100
): string {
  if (!description) return "";

  // --- Loại bỏ các phần đặc biệt (link, script) ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  
  let cleanDescription = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();

  // --- Loại bỏ HTML tags ---
  cleanDescription = cleanDescription.replace(/<[^>]*>/g, "");

  // --- Loại bỏ khoảng trắng thừa ---
  cleanDescription = cleanDescription.replace(/\s+/g, " ").trim();

  // --- Cắt ngắn và thêm dấu "..." ---
  if (cleanDescription.length <= maxLength) {
    return cleanDescription;
  }

  return cleanDescription.substring(0, maxLength).trim() + "...";
}

export function renderDescriptionHTML(description: string | null) {
  if (!description) return "";


  // --- Tách link ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const linkMatch = description.match(linkRegex);
  const link = linkMatch ? linkMatch[1].trim() : null;


  // --- Tách script ---
  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  const scriptMatch = description.match(scriptRegex);
  const scriptContent = scriptMatch ? scriptMatch[1].trim() : null;


  // --- Loại bỏ các phần đặc biệt khỏi phần mô tả còn lại ---
  let cleanDescription = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();


  // --- Tạo HTML ---
  let html = ` ${cleanDescription}`;


  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }


  if (scriptContent) {
    html += `
    <p><strong>Script</strong>:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px;">
      ${scriptContent}
    </div>
    `;
  }


  return html.trim();
}