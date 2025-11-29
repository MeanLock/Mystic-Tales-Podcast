export function renderDescriptionHTML(description: string | null) {
  if (!description) return "";

  // --- Tách link và script với custom markers ---
  const linkRegex = /\$-\[link\]\$-([\s\S]*?)\$-\[link\]\$-/;
  const linkMatch = description.match(linkRegex);
  const customLink = linkMatch ? linkMatch[1].trim() : null;

  const scriptRegex = /\$-\[script\]\$-([\s\S]*?)\$-\[script\]\$-/;
  const scriptMatch = description.match(scriptRegex);
  const scriptContent = scriptMatch ? scriptMatch[1].trim() : null;

  // --- Loại bỏ các phần đặc biệt khỏi phần mô tả ---
  let mainContent = description
    .replace(linkRegex, "")
    .replace(scriptRegex, "")
    .trim();

  // --- Kiểm tra xem content có phải là HTML hợp lệ không ---
  const hasHTMLTags = /<[^>]+>/.test(mainContent);
  
  // Nếu không có HTML tags và không có custom link/script, return plain text
  if (!hasHTMLTags && !customLink && !scriptContent) {
    return mainContent;
  }

  // --- Xử lý HTML content ---
  let html = '';
  
  if (hasHTMLTags) {
    // Content đã có HTML tags, giữ nguyên
    html = mainContent;
  } else {
    // Plain text, wrap trong <p>
    html = `<p>${mainContent}</p>`;
  }

  // --- Thêm custom link nếu có ---
  if (customLink) {
    html += `
    <p><strong>Link</strong>: <a href="${customLink}" target="_blank" rel="noopener noreferrer">${customLink}</a></p>`;
  }

  // --- Thêm script content nếu có ---
  if (scriptContent) {
    html += `
    <p><strong>Script</strong>:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}