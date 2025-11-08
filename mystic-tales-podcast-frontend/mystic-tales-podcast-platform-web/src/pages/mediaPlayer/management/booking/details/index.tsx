import type { BookingDetails } from "@/core/types/booking";
import { useParams } from "react-router-dom";

const mockBookingDetails: BookingDetails = {
  Id: 1,
  Title: "Đặt làm voice-over cho podcast về lucid dream",
  Description:
    "Tôi cần một giọng đọc ấm áp và cuốn hút để làm voice-over cho tập podcast về lucid dream của mình.",
  AccountId: 1,
  PodcasterId: 1,
  Price: 2000000,
  Deadline: "2025-11-07T08:15:52.397Z",
  DemoAudioFileKey: "string",
  BookingManualCancelledReason: null,
  BookingAutoCancelReason: null,
  CreatedAt: "2025-11-07T08:15:52.397Z",
  UpdatedAt: "2025-11-07T08:15:52.397Z",
  BookingRequirementFileList: [
    {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      BookingId: 1,
      Name: "Đoạn 1 - Giới thiệu về lucid dream",
      Description:
        "Đoạn này cần một giọng đọc nhẹ nhàng và thu hút để giới thiệu về khái niệm lucid dream.",
      RequirementDocumentFileKey: "string",
      Order: 1,
      WordCount: 5000,
      PodcastBookingTone: {
        Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        Name: "string",
        Description: "string",
        PodcastBookingToneCategory: {
          Id: 0,
          Name: "string",
        },
        CreatedAt: "2025-11-07T08:15:52.397Z",
        UpdatedAt: "2025-11-07T08:15:52.397Z",
      },
    },
  ],
  BookingProducingRequestList: [
    {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      BookingId: 0,
      Note: null,
      Deadline: "2025-11-07",
      IsAccepted: true,
      FinishedAt: "2025-11-07T08:15:52.397Z",
      RejectReason: "string",
      CreatedAt: "2025-11-07T08:15:52.397Z",
    },
  ],
  CurrentStatus: {
    Id: 0,
    Name: "string",
  },
};

function renderDescriptionHTML(description: string | null) {
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
  let html = `<p><strong>Description</strong>: ${cleanDescription}</p>`;

  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

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

function encodeDescription(
  description: string,
  link: string | null = null,
  script: string | null = null
) {
  let encoded = description?.trim() || "";

  if (link) {
    encoded += `\n$-[link]$-${link}$-[link]$-`;
  }

  if (script) {
    encoded += `\n$-[script]$-${script}$-[script]$-`;
  }

  return encoded.trim();
}

const description =
  "Đây là tài liệu để nói về khái niệm của thuật ngữ Sublinimal, người đọc cần mở đầu hay, có liên hệ với thực tế. Giong đọc chậm, nhấn mạnh các ý";
const link =
  "https://nhathuoclongchau.com.vn/bai-viet/subliminal-la-gi-nghe-subliminal-co-tac-dung-gi.html";
const script = `
<h2 style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:20px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;letter-spacing:normal;line-height:28px;margin:16px 0px 8px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    <strong>Subliminal là gì?</strong>
</h2>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Subliminal là từ dùng để chỉ các loại thông tin, tín hiệu được truyền tải vào tâm thức con người một cách không có ý thức. Hiểu một cách đơn giản, đây là những tín hiệu mà con người không nhận thức hoặc phát hiện được bằng ý thức. Các thông tin, tín hiệu subliminal có thể tồn tại ở dạng hình ảnh, âm thanh, màu sắc,…
</p>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Subliminal được nghiên cứu và ứng dụng trong khá nhiều lĩnh vực khác nhau từ quảng cáo, giáo dục, y học, nghệ thuật, tâm lý học,… Tuy nhiên, hiệu quả và tính khoa học của subliminal vẫn đang được giới khoa học tranh luận và có nhiều ý kiến trái chiều.
</p>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Về cách thức hoạt động, subliminal là các tín hiệu được phát ra ở tần số siêu âm 20kHZ - tần số giúp các tín hiệu đi vào tiềm thức một cách tự nhiên mà con người không có chủ đích và không hay biết. Theo các nhà khoa học, subliminal chủ yếu hướng đến thị giác và thính giác.
</p>
<h2 style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:20px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;letter-spacing:normal;line-height:28px;margin:16px 0px 8px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    <strong>Subliminal music là gì?</strong>
</h2>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Khi tìm hiểu subliminal là gì thì có lẽ bạn đã biết nó có thể là những tín hiệu dạng âm thanh. Chính vì vậy có thêm một thuật ngữ là subliminal music. Vậy subliminal là gì? Subliminal music là một loại âm nhạc được tạo ra bằng các công nghệ âm thanh đặc biệt với mục đích truyền tải thông điệp subliminal vào tiềm thức của con người. Để tạo ra được loại âm nhạc đặc biệt này, người ta cần phân tích sóng não, tần số sóng âm thanh và dùng đế nhiều kỹ thuật âm thanh khác nhau.
</p>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Subliminal music được cho là có tác dụng cải thiện trí nhớ, tăng khả năng tập trung, giảm căng thẳng, giúp ngủ ngon, thư giãn và giảm đau. Thông điệp được truyền tải trong loại âm nhạc này thường là những lời khuyên theo hướng tích cực, khích lệ khẳng định bản thân, động viên tinh thần, khích lệ sự tự tin.
</p>
<p style="--tw-backdrop-blur:;--tw-backdrop-brightness:;--tw-backdrop-contrast:;--tw-backdrop-grayscale:;--tw-backdrop-hue-rotate:;--tw-backdrop-invert:;--tw-backdrop-opacity:;--tw-backdrop-saturate:;--tw-backdrop-sepia:;--tw-blur:;--tw-border-spacing-x:0;--tw-border-spacing-y:0;--tw-brightness:;--tw-contain-layout:;--tw-contain-paint:;--tw-contain-size:;--tw-contain-style:;--tw-contrast:;--tw-drop-shadow:;--tw-gradient-from-position:;--tw-gradient-to-position:;--tw-gradient-via-position:;--tw-grayscale:;--tw-hue-rotate:;--tw-invert:;--tw-numeric-figure:;--tw-numeric-fraction:;--tw-numeric-spacing:;--tw-ordinal:;--tw-pan-x:;--tw-pan-y:;--tw-pinch-zoom:;--tw-ring-color:rgba(59,130,246,.5);--tw-ring-inset:;--tw-ring-offset-color:#fff;--tw-ring-offset-shadow:0 0 #0000;--tw-ring-offset-width:0px;--tw-ring-shadow:0 0 #0000;--tw-rotate:0;--tw-saturate:;--tw-scale-x:1;--tw-scale-y:1;--tw-scroll-snap-strictness:proximity;--tw-sepia:;--tw-shadow-colored:0 0 #0000;--tw-shadow:0 0 #0000;--tw-skew-x:0;--tw-skew-y:0;--tw-slashed-zero:;--tw-translate-x:0;--tw-translate-y:0;-webkit-text-stroke-width:0px;background-color:rgb(255, 255, 255);border:0px solid rgb(229, 231, 235);box-sizing:border-box;color:rgb(2, 11, 39);font-family:Inter, &quot;Inter Fallback&quot;;font-size:16px;font-style:normal;font-variant-caps:normal;font-variant-ligatures:normal;font-weight:400;letter-spacing:normal;line-height:24px;margin:8px 0px;orphans:2;overflow-wrap:break-word;text-align:start;text-decoration-color:initial;text-decoration-style:initial;text-decoration-thickness:initial;text-indent:0px;text-transform:none;white-space:normal;widows:2;word-break:break-word;word-spacing:0px;">
    Tuy nhiên, có một thực tế mà chúng ta cần phải thừa nhận rằng, subliminal music có tác dụng tích cực với tinh thần và tâm lý con người hay không vẫn đang được giới khoa học tranh cãi.
</p>
`;

const BookingDetailsPage = () => {
  const { id } = useParams();

  return (
    <div>
      <h1>Booking Details Page</h1>
    </div>
  );
};

export default BookingDetailsPage;
