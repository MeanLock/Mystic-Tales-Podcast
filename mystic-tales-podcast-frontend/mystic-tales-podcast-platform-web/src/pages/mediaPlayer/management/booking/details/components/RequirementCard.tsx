import type { PodcastBookingToneCategoryType } from "@/core/types/booking";
import { useEffect, useState } from "react";
import { FaChevronDown, FaChevronUp } from "react-icons/fa";
import { motion, AnimatePresence } from "framer-motion";
import {
  useLazyGetBookingPublicSourceQuery,
} from "@/core/services/file/file.service";

interface RequirementCardProps {
  requirement: {
    Id: string;
    BookingId: number;
    Name: string;
    Description: string;
    RequirementDocumentFileKey: string | null; // absolute url
    Order: number;
    WordCount: number;
    PodcastBookingTone: {
      Id: string;
      Name: string;
      Description: string;
      PodcastBookingToneCategory: PodcastBookingToneCategoryType;
      CreatedAt: string;
      UpdatedAt: string;
    };
  };
}

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
  let html = `<p><strong>Description: </strong>${cleanDescription}</p>`;

  if (link) {
    html += `
    <p style="margin-top: 10px"><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

  if (scriptContent) {
    html += `
    <p style="font-weight: bold; margin-top: 10px">Script:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}

const RequirementCard = ({ requirement }: RequirementCardProps) => {
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  const [isResolveLoading, setIsResolveLoading] = useState(false);
  const [fileUrl, setFileUrl] = useState<string | null>(null);

  const [triggerResolveFile] = useLazyGetBookingPublicSourceQuery();

  const handleDownload = async () => {
    try {
      if (!requirement.RequirementDocumentFileKey) return;

      // Fetch fresh URL to avoid access denied
      const resolveFile = await triggerResolveFile({
        FileKey: requirement.RequirementDocumentFileKey,
      }).unwrap();

      const freshUrl = resolveFile.FileUrl;
      if (freshUrl) {
        // Fetch as blob to force download (prevents audio/video from opening in browser)
        const response = await fetch(freshUrl);
        const blob = await response.blob();
        const blobUrl = window.URL.createObjectURL(blob);

        // Trigger download
        const link = document.createElement("a");
        link.href = blobUrl;
        link.download = requirement.Name || "requirement-document";
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        // Cleanup blob URL
        window.URL.revokeObjectURL(blobUrl);
      }
    } catch (error) {
      console.error("[RequirementCard] Failed to download file:", error);
    }
  };

  useEffect(() => {
    const resolveRequirementFile = async () => {
      if (isDetailOpen === false) return;
      setIsResolveLoading(true);
      try {
        if (!requirement.RequirementDocumentFileKey) {
          setIsResolveLoading(false);
          return;
        }
        const resolveFile = await triggerResolveFile({
          FileKey: requirement.RequirementDocumentFileKey,
        }).unwrap();
        setFileUrl(resolveFile.FileUrl || null);
        setIsResolveLoading(false);
      } catch (error) {
        console.error(
          `[RequirementCard] ❌ Failed to resolve requirement #${requirement.Order}`,
          error
        );
        setIsResolveLoading(false);
      }
    };
    resolveRequirementFile();
  }, [isDetailOpen]);

  return (
    <div className="w-full">
      {/* HEADER */}
      <div
        key={`index-${requirement.Id}`}
        className="bg-white/30 shadow-2xl w-full rounded-t-md flex flex-col p-5"
      >
        <div className="w-full flex items-center justify-between">
          <p className="font-bold text-white font-poppins text-lg">
            Requirement #{requirement.Order}
          </p>
          {!isResolveLoading && (
            <div
              onClick={() => setIsDetailOpen((prev) => !prev)}
              className="p-2 rounded-full flex items-center justify-center text-white transition-all ease-out duration-300 hover:bg-white/30 hover:-translate-y-1 cursor-pointer"
            >
              {isDetailOpen ? <FaChevronUp /> : <FaChevronDown />}
            </div>
          )}
        </div>
      </div>

      {isResolveLoading ? (
        <p>Source Loading ...</p>
      ) : (
        <AnimatePresence initial={false}>
          {isDetailOpen && (
            <motion.div
              key="details"
              initial={{ opacity: 0, height: 0 }}
              animate={{ opacity: 1, height: "auto" }}
              exit={{ opacity: 0, height: 0 }}
              transition={{ duration: 0.2, ease: "easeInOut" }}
              className="bg-white shadow-2xl rounded-b-md p-5 text-black overflow-hidden"
            >
              <p className="font-bold">Name:</p>
              <p className="mb-2">{requirement.Name}</p>
              <div
                dangerouslySetInnerHTML={{
                  __html: renderDescriptionHTML(requirement.Description),
                }}
              />

              {/* Phân loại file requirement url */}
              {/* Nếu là pdf thì dùng iframe hiển thị */}
              {/* Nếu là audio thì cho nghe và download */}
              {/* Các file khác thì chỉ download */}
              {fileUrl && fileUrl.toLowerCase().includes(".pdf") && (
                <iframe
                  src={fileUrl}
                  title="Requirement Document PDF"
                  className="w-full mt-5 min-h-[800px] border rounded-md"
                />
              )}
              {fileUrl &&
                (fileUrl.toLowerCase().includes(".mp3") ||
                  fileUrl.toLowerCase().includes(".wav") ||
                  fileUrl.toLowerCase().includes(".m4a") ||
                  fileUrl.toLowerCase().includes(".ogg") ||
                  fileUrl.toLowerCase().includes(".aac")) && (
                  <div className="mt-5 space-y-3">
                    <div className="border rounded-md p-4 bg-gray-50">
                      <p className="font-semibold mb-2 text-sm text-gray-600">
                        Audio Preview:
                      </p>
                      <audio controls className="w-full">
                        <source src={fileUrl} />
                        Your browser does not support the audio element.
                      </audio>
                    </div>
                    <button
                      onClick={handleDownload}
                      className="inline-block px-6 py-3 bg-mystic-green text-black font-semibold rounded-md hover:bg-mystic-green/80 transition-colors cursor-pointer"
                    >
                      Download Audio File
                    </button>
                  </div>
                )}
              {fileUrl &&
                !fileUrl.toLowerCase().includes(".pdf") &&
                !fileUrl.toLowerCase().includes(".mp3") &&
                !fileUrl.toLowerCase().includes(".wav") &&
                !fileUrl.toLowerCase().includes(".m4a") &&
                !fileUrl.toLowerCase().includes(".ogg") &&
                !fileUrl.toLowerCase().includes(".aac") && (
                  <div className="mt-5">
                    <button
                      onClick={handleDownload}
                      className="inline-block px-6 py-3 bg-mystic-green text-black font-semibold rounded-md hover:bg-mystic-green/80 transition-colors cursor-pointer"
                    >
                      Download Requirement Document
                    </button>
                  </div>
                )}
            </motion.div>
          )}
        </AnimatePresence>
      )}
    </div>
  );
};

export default RequirementCard;
