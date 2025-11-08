import React, { useEffect, useRef } from "react";
import { useQuill } from "react-quilljs";
import "quill/dist/quill.snow.css";

type BookingRequirementInfo = {
  Name: string;
  Description: string;
  Order: number;
  PodcastBookingToneId: string;
};

interface BookingFormProps {
  Title: string;
  onTitleChange: () => void;
  Description: string;
  onDescriptionChange: () => void;
  BookingRequirementInfos: BookingRequirementInfo[];
  onCreateNewRequirementInfo: () => void;
  onUpdateRequirementInfo: (data: BookingRequirementInfo) => void;
  BookingRequirementFiles: File[];
  onUploadNewFile: (file: File) => void;
  onUpdateFile: (file: File) => void;
}

const BookingForm = () => {
  const { quill, quillRef } = useQuill({
    modules: {
      toolbar: [["bold", "italic"], [{ list: "bullet" }]],
    },
    // placeholder: "Nhập mô tả...",
    theme: "snow",
  });

  useEffect(() => {
    if (quill) {
      quill.on("text-change", () => {
        console.log("Nội dung:", quill.root.innerHTML);
      });
    }
  }, [quill]);
  return (
    <div className="flex flex-col w-full p-8 gap-5">
      <p className="text-3xl font-bold text-white">Booking Requirement Form</p>

      <div className="w-full flex flex-col gap-6">
        {/* Title + Deadline */}
        <div className="w-full flex items-center justify-between gap-16">
          {/* Title */}
          <div className="flex-1">
            <p className="text-white font-poppins font-semibold text-sm mb-1">
              Title
            </p>
            <input
              type="text"
              placeholder="Enter title..."
              className="w-3/4 pb-2 border-b-white border-b-[1px] bg-transparent text-white placeholder:text-gray-400 outline-none"
            />
          </div>

          {/* Deadline */}
          <div className="flex-1">
            <p className="text-white font-poppins font-semibold text-sm mb-1">
              Deadline
            </p>
            <input
              type="date"
              className="w-3/4 pb-2 border-b-white border-b-[1px] bg-transparent text-white placeholder:text-gray-400 outline-none"
            />
          </div>
        </div>

        {/* Description */}
        <div className="w-full">
          <p className="text-white font-poppins font-semibold text-sm mb-2">
            Description
          </p>
          <div className="bg-transparent">
            <div
              ref={quillRef}
              className="relative text-white
          [&_.ql-toolbar]:border-none 
          [&_.ql-toolbar]:bg-transparent 
          [&_.ql-toolbar]:mb-2
          [&_.ql-editor]:text-white 
          [&_.ql-editor]:caret-white 
          [&_.ql-editor]:min-h-[150px] 
          [&_.ql-container]:bg-transparent"
            />
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookingForm;
