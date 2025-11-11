import type { PodcastBookingToneCategoryType } from "@/core/types/booking";
import { useState } from "react";
import { FaChevronDown, FaChevronUp } from "react-icons/fa";
import { motion, AnimatePresence } from "framer-motion";

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

const RequirementCard = ({ requirement }: RequirementCardProps) => {
  const [isDetailOpen, setIsDetailOpen] = useState(false);

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

            {requirement.RequirementFile && (
              <iframe
                src={requirement.RequirementFile}
                rel="noopener noreferrer"
                className="w-full mt-10 min-h-[800px]"
              />
            )}
          </motion.div>
        )}
      </AnimatePresence>
    </div>
  );
};

export default RequirementCard;
