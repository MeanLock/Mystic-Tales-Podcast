import type { PodcastBookingToneCategoryType } from "@/core/types/booking";
import { useState } from "react";
import { FaChevronDown, FaChevronUp } from "react-icons/fa";
import { motion, AnimatePresence } from "framer-motion";
// import { useLazyGetBookingPublicSourceQuery } from "@/core/services/file/file.service";

interface RequirementCardProps {
  requirement: {
    Id: string;
    BookingId: number;
    Name: string;
    Description: string;
    RequirementFile: string | null; // absolute url
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
  let html = `<strong>${cleanDescription}</strong>`;

  if (link) {
    html += `
    <p><strong>Link</strong>: <a href="${link}" target="_blank" rel="noopener noreferrer">${link}</a></p>`;
  }

  if (scriptContent) {
    html += `
    <p>• Script:</p>
    <div style="margin-top: 10px; border: 1px solid #ccc; padding: 10px; border-radius: 5px; background-color: #f9f9f9;">
      ${scriptContent}
    </div>
    `;
  }

  return html.trim();
}

const RequirementCardWithWordCount = ({
  requirement,
}: RequirementCardProps) => {
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  // const [triggerResolveFile] = useLazyGetBookingPublicSourceQuery();

  const handleDownload = async () => {
    try {
      if (!requirement.RequirementFile) return;

      // Fetch as blob to force download (prevents audio/video from opening in browser)
      const response = await fetch(requirement.RequirementFile);
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
    } catch (error) {
      console.error(
        "[RequirementCardWithWordCount] Failed to download file:",
        error
      );
    }
  };

  return (
    <div className="w-full">
      {/* HEADER */}
      <div
        key={`index-${requirement.Id}`}
        className="bg-white/10 shadow-2xl w-full flex flex-col p-5"
      >
        <div className="w-full flex items-center justify-between">
          <p className="font-bold text-white font-poppins text-lg">
            Requirement #{requirement.Order}:{" "}
            <span className="text-mystic-green">
              {requirement.WordCount.toLocaleString()}
            </span>{" "}
            words
          </p>
          <div
            onClick={() => setIsDetailOpen((prev) => !prev)}
            className="p-2 rounded-full flex items-center justify-center text-white transition-all ease-out duration-300 hover:bg-white/30 hover:-translate-y-1 cursor-pointer"
          >
            {isDetailOpen ? <FaChevronUp /> : <FaChevronDown />}
          </div>
        </div>
      </div>

      {/* DETAILS (animated) */}
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
            <p className="font-semibold mb-2">{requirement.Name}</p>
            <div
              dangerouslySetInnerHTML={{
                __html: renderDescriptionHTML(requirement.Description),
              }}
            />

            {/* Hiển thị PDF trong iframe, audio cho nghe và download, các file khác cho download */}
            {requirement.RequirementFile &&
              requirement.RequirementFile.toLowerCase().includes(".pdf") && (
                <iframe
                  src={requirement.RequirementFile}
                  title="Requirement Document PDF"
                  className="w-full mt-10 min-h-[800px] border rounded-md"
                />
              )}
            {requirement.RequirementFile &&
              (requirement.RequirementFile.toLowerCase().includes(".mp3") ||
                requirement.RequirementFile.toLowerCase().includes(".wav") ||
                requirement.RequirementFile.toLowerCase().includes(".m4a") ||
                requirement.RequirementFile.toLowerCase().includes(".ogg") ||
                requirement.RequirementFile.toLowerCase().includes(".aac")) && (
                <div className="mt-5 space-y-3">
                  <div className="border rounded-md p-4 bg-gray-50">
                    <p className="font-semibold mb-2 text-sm text-gray-600">
                      Audio Preview:
                    </p>
                    <audio controls className="w-full">
                      <source src={requirement.RequirementFile} />
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
            {requirement.RequirementFile &&
              !requirement.RequirementFile.toLowerCase().includes(".pdf") &&
              !requirement.RequirementFile.toLowerCase().includes(".mp3") &&
              !requirement.RequirementFile.toLowerCase().includes(".wav") &&
              !requirement.RequirementFile.toLowerCase().includes(".m4a") &&
              !requirement.RequirementFile.toLowerCase().includes(".ogg") &&
              !requirement.RequirementFile.toLowerCase().includes(".aac") && (
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
    </div>
  );
};

export default RequirementCardWithWordCount;
