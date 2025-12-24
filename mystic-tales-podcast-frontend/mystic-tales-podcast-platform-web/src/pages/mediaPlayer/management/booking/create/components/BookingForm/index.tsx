import React, { useEffect, useRef, useState } from "react";
import { useQuill } from "react-quilljs";
import "quill/dist/quill.snow.css";
import type { PodcastBookingTone } from "@/core/types/booking";
import type { PodcastBuddyDetails } from "@/core/types/podcaster";
import { useGetBookingTonesOfPodcastBuddyQuery } from "@/core/services/booking/booking.service";
import { LiquidButton } from "@/components/ui/shadcn-io/liquid-button";

type BookingRequirementInfo = {
  Name: string;
  Description: string;
  Order: number;
  PodcastBookingToneId: string;
  // optional fields used by the form
  ContentType: "link" | "script" | "file";
  ContentValue?: string;
};

interface BookingFormProps {
  selectedBuddy: PodcastBuddyDetails;
  Title: string;
  Description: string;
  // callbacks should accept the new value so parent setState receives it
  onTitleChange: (val: string) => void;
  onDescriptionChange: (val: string) => void;
  BookingRequirementInfos: BookingRequirementInfo[];
  BookingDeadlineDayCount: number;
  onDeadlineDayCountChange: (n: number) => void;
  // notify parent with the newly created requirement so parent can keep its copy
  onCreateNewRequirementInfo: (newReq: BookingRequirementInfo) => void;
  // parent expects a single-updated requirement so we notify with the updated item
  onUpdateRequirementInfo: (updatedReq: BookingRequirementInfo) => void;
  // notify parent when a requirement is deleted
  onDeleteRequirementInfo: (order: number) => void;
  BookingRequirementFiles: File[];
  onUploadNewFile: (file: File | null) => void;
  // (removed unused onUpdateFile prop)
  onSubmit: () => void;
  isCreating: boolean;
}

// Script Editor Component
const ScriptEditor = ({
  value,
  onChange,
}: {
  value: string;
  onChange: (val: string) => void;
}) => {
  const { quill, quillRef } = useQuill({
    modules: {
      toolbar: [["bold", "italic"], [{ list: "bullet" }]],
    },
    theme: "snow",
  });

  useEffect(() => {
    // sync editor content when `value` prop changes
    if (quill) {
      // only update DOM if it actually differs to avoid clobbering user input
      try {
        const current = quill.root.innerHTML || "";
        if ((value || "") !== current) quill.root.innerHTML = value || "";
      } catch (e) {
        // fall back to direct set if something odd happens
        quill.root.innerHTML = value || "";
      }
    }
  }, [quill, value]);

  useEffect(() => {
    if (!quill) return;
    const handleChange = () => onChange(quill.root.innerHTML);
    quill.on("text-change", handleChange);
    return () => {
      // cleanup handler
      try {
        quill.off && (quill.off("text-change", handleChange) as any);
      } catch (e) {
        /* ignore */
      }
    };
  }, [quill, onChange]);

  return (
    <div
      ref={quillRef}
      className="bg-white/5 rounded [&_.ql-toolbar]:border-none [&_.ql-toolbar]:bg-transparent [&_.ql-editor]:text-white [&_.ql-editor]:min-h-[150px]"
    />
  );
};

// Description Editor Component
const DescriptionEditor = ({
  value,
  onChange,
}: {
  value: string;
  onChange: (val: string) => void;
}) => {
  const { quill, quillRef } = useQuill({
    modules: {
      toolbar: [["bold", "italic"], [{ list: "bullet" }]],
    },
    theme: "snow",
  });

  useEffect(() => {
    // sync editor content when `value` prop changes
    if (quill) {
      try {
        const current = quill.root.innerHTML || "";
        if ((value || "") !== current) quill.root.innerHTML = value || "";
      } catch (e) {
        quill.root.innerHTML = value || "";
      }
    }
  }, [quill, value]);

  useEffect(() => {
    if (!quill) return;
    const handleChange = () => onChange(quill.root.innerHTML);
    quill.on("text-change", handleChange);
    return () => {
      try {
        quill.off && (quill.off("text-change", handleChange) as any);
      } catch (e) {
        /* ignore */
      }
    };
  }, [quill, onChange]);

  return (
    <div
      ref={quillRef}
      className="bg-white/5 rounded [&_.ql-toolbar]:border-none [&_.ql-toolbar]:bg-transparent [&_.ql-editor]:text-white [&_.ql-editor]:min-h-[100px]"
    />
  );
};

// Requirement Item Component
const RequirementItem = ({
  requirement,
  index: _index,
  availableTones,
  onUpdate,
  onDelete,
  onFileUpload,
  existingFile,
}: any) => {
  const [isEditing, setIsEditing] = useState(true);
  const [localData, setLocalData] =
    useState<BookingRequirementInfo>(requirement);
  const [contentType, setContentType] = useState<
    "link" | "script" | "file" | null
  >(requirement.ContentType || null);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const fileInputRef = useRef<HTMLInputElement>(null);

  const validate = () => {
    const newErrors: Record<string, string> = {};

    if (!localData.Name.trim()) {
      newErrors.Name = "Name is required";
    }
    if (
      !localData.Description.trim() ||
      localData.Description === "<p><br></p>"
    ) {
      newErrors.Description = "Description is required";
    }
    if (!localData.PodcastBookingToneId) {
      newErrors.PodcastBookingToneId = "Tone is required";
    }
    if (!contentType) {
      newErrors.ContentType = "Content type is required";
    }
    if (contentType === "link" && !localData.ContentValue?.trim()) {
      newErrors.ContentValue = "Link is required";
    }
    if (
      contentType === "script" &&
      (!localData.ContentValue?.trim() ||
        localData.ContentValue === "<p><br></p>")
    ) {
      newErrors.ContentValue = "Script is required";
    }
    if (contentType === "file" && !existingFile) {
      newErrors.File = "File is required";
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSave = () => {
    if (validate()) {
      onUpdate({ ...localData, ContentType: contentType });
      setIsEditing(false);
    }
  };

  const handleContentTypeChange = (type: "link" | "script" | "file") => {
    if (type !== contentType) {
      setLocalData((prev) => ({ ...prev, ContentValue: "" }));
      if (type !== "file" && existingFile) {
        onFileUpload(null);
      }
    }
    setContentType(type);
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Validate file type
    const allowedExtensions = [
      "pdf",
      "doc",
      "docx",
      "xls",
      "xlsx",
      "txt",
      "csv",
      "wav",
      "flac",
      "mp3",
      "zip",
      "rar",
    ];
    const ext = file.name.split(".").pop()?.toLowerCase();
    if (!ext || !allowedExtensions.includes(ext)) {
      setErrors({
        ...errors,
        File: "Invalid file type. Allowed: PDF, DOC, DOCX, XLS, XLSX, TXT, CSV, WAV, FLAC, MP3, ZIP, RAR",
      });
      e.target.value = "";
      return;
    }

    // Validate file size (max 50MB)
    const maxSizeInBytes = 50 * 1024 * 1024; // 50MB
    if (file.size > maxSizeInBytes) {
      setErrors({
        ...errors,
        File: "File size exceeds 50MB. Please upload a smaller file.",
      });
      e.target.value = "";
      return;
    }

    // Clear file error if validation passes
    const newErrors = { ...errors };
    delete newErrors.File;
    setErrors(newErrors);

    const newFileName = `${localData.Order}.${ext}`;
    const renamedFile = new File([file], newFileName, { type: file.type });
    onFileUpload(renamedFile);
  };

  return (
    <div className="bg-white/10 p-6 rounded-lg border border-white/20">
      <div className="flex justify-between items-center mb-4">
        <h3 className="text-xl font-bold text-white">
          Requirement {localData.Order}
        </h3>
        <div className="flex gap-2">
          {isEditing ? (
            <button
              onClick={handleSave}
              className="px-4 py-2 bg-green-500/20 text-green-100 rounded hover:bg-green-600 transition"
            >
              Save
            </button>
          ) : (
            <button
              onClick={() => setIsEditing(true)}
              className="px-4 py-2 bg-blue-500/20 text-blue-100 rounded hover:bg-blue-600 transition"
            >
              Update
            </button>
          )}
          <button
            onClick={() => onDelete(localData.Order)}
            className="px-4 py-2 bg-red-500/20 text-red-100 rounded hover:bg-red-600 transition"
          >
            Delete
          </button>
        </div>
      </div>

      {isEditing ? (
        <div className="space-y-4">
          <div>
            <label className="text-white font-semibold text-sm mb-1 block">
              Name
            </label>
            <input
              type="text"
              value={localData.Name}
              onChange={(e) =>
                setLocalData({ ...localData, Name: e.target.value })
              }
              className="w-full p-2 bg-white/10 border border-white/30 rounded text-white outline-none"
              placeholder="Enter requirement name"
            />
            {errors.Name && (
              <p className="text-red-400 text-xs mt-1">{errors.Name}</p>
            )}
          </div>

          <div>
            <label className="text-white font-semibold text-sm mb-2 block">
              Description
            </label>
            <DescriptionEditor
              value={localData.Description}
              onChange={(val) =>
                setLocalData({ ...localData, Description: val })
              }
            />
            {errors.Description && (
              <p className="text-red-400 text-xs mt-1">{errors.Description}</p>
            )}
          </div>

          <div>
            <label className="text-white font-semibold text-sm mb-1 block">
              Tone
            </label>
            <select
              value={localData.PodcastBookingToneId}
              onChange={(e) =>
                setLocalData({
                  ...localData,
                  PodcastBookingToneId: e.target.value,
                })
              }
              className="w-full p-2 bg-white/10 border border-white/30 rounded text-white outline-none"
            >
              <option value="">Select a tone</option>
              {availableTones.map((tone: PodcastBookingTone) => (
                <option key={tone.Id} value={tone.Id} className="bg-gray-800">
                  {tone.Name}
                </option>
              ))}
            </select>
            {errors.PodcastBookingToneId && (
              <p className="text-red-400 text-xs mt-1">
                {errors.PodcastBookingToneId}
              </p>
            )}
          </div>

          <div>
            <label className="text-white font-semibold text-sm mb-2 block">
              Content Type
            </label>
            <div className="flex gap-4 mb-3">
              {["link", "script", "file"].map((type) => (
                <button
                  key={type}
                  onClick={() => handleContentTypeChange(type as any)}
                  className={`px-4 py-2 rounded transition ${
                    contentType === type
                      ? "bg-blue-500 text-white"
                      : "bg-white/10 text-white/70 hover:bg-white/20"
                  }`}
                >
                  {type.charAt(0).toUpperCase() + type.slice(1)}
                </button>
              ))}
            </div>
            {errors.ContentType && (
              <p className="text-red-400 text-xs mt-1">{errors.ContentType}</p>
            )}

            {contentType === "link" && (
              <div>
                <input
                  type="text"
                  value={localData.ContentValue || ""}
                  onChange={(e) =>
                    setLocalData({ ...localData, ContentValue: e.target.value })
                  }
                  className="w-full p-2 bg-white/10 border border-white/30 rounded text-white outline-none"
                  placeholder="Enter URL"
                />
                {errors.ContentValue && (
                  <p className="text-red-400 text-xs mt-1">
                    {errors.ContentValue}
                  </p>
                )}
              </div>
            )}

            {contentType === "script" && (
              <div>
                <ScriptEditor
                  value={localData.ContentValue || ""}
                  onChange={(val) =>
                    setLocalData({ ...localData, ContentValue: val })
                  }
                />
                {errors.ContentValue && (
                  <p className="text-red-400 text-xs mt-1">
                    {errors.ContentValue}
                  </p>
                )}
              </div>
            )}

            {contentType === "file" && (
              <div>
                <input
                  ref={fileInputRef}
                  type="file"
                  accept=".pdf,.doc,.docx,.xls,.xlsx,.txt,.csv,.wav,.flac,.mp3,.zip,.rar"
                  onChange={handleFileChange}
                  className="hidden"
                />
                <button
                  onClick={() => fileInputRef.current?.click()}
                  className="px-4 py-2 bg-white/10 border border-white/30 rounded text-white hover:bg-white/20 transition"
                >
                  {existingFile
                    ? `Change file: ${existingFile.name}`
                    : "Upload file"}
                </button>
                <p className="text-white/60 text-xs mt-2">
                  Allowed: PDF, DOC, DOCX, XLS, XLSX, TXT, CSV, WAV, FLAC, MP3,
                  ZIP, RAR (Max 50MB)
                </p>
                {errors.File && (
                  <p className="text-red-400 text-xs mt-1">{errors.File}</p>
                )}
              </div>
            )}
          </div>
        </div>
      ) : (
        <div className="space-y-2 text-white/80">
          <p>
            <strong>Name:</strong> {localData.Name}
          </p>
          <p>
            <strong>Tone:</strong>{" "}
            {
              availableTones.find(
                (t: any) => t.Id === localData.PodcastBookingToneId
              )?.Name
            }
          </p>
          <p>
            <strong>Content Type:</strong> {contentType}
          </p>
          {existingFile && (
            <p>
              <strong>File:</strong> {existingFile.name}
            </p>
          )}
        </div>
      )}
    </div>
  );
};

// Main BookingForm Component
const BookingForm = ({
  selectedBuddy,
  Title,
  onTitleChange,
  Description,
  onDescriptionChange,
  BookingRequirementInfos,
  BookingDeadlineDayCount,
  onDeadlineDayCountChange,
  onCreateNewRequirementInfo,
  onUpdateRequirementInfo,
  onDeleteRequirementInfo,
  BookingRequirementFiles,
  onUploadNewFile,
  onSubmit,
  isCreating,
}: BookingFormProps) => {
  const [title, setTitle] = useState(Title);
  const [deadline, setDeadline] = useState<number>(
    // initialize from prop or default to 1
    typeof BookingDeadlineDayCount === "number" && BookingDeadlineDayCount > 0
      ? BookingDeadlineDayCount
      : 1
  );
  const [requirements, setRequirements] = useState<BookingRequirementInfo[]>(
    BookingRequirementInfos
  );
  const [files, setFiles] = useState<File[]>(BookingRequirementFiles);

  const { quill, quillRef } = useQuill({
    modules: {
      toolbar: [["bold", "italic"], [{ list: "bullet" }]],
    },
    theme: "snow",
  });

  // HOOKS
  const { data: availableBookingTonesOfPodcastBuddy } =
    useGetBookingTonesOfPodcastBuddyQuery(
      { PodcastBuddyId: selectedBuddy.PodcastBuddyProfile.AccountId! },
      { skip: !selectedBuddy }
    );

  useEffect(() => {
    if (quill) {
      try {
        const current = quill.root.innerHTML || "";
        if ((Description || "") !== current)
          quill.root.innerHTML = Description || "";
      } catch (e) {
        quill.root.innerHTML = Description || "";
      }
    }
  }, [quill, Description]);

  useEffect(() => {
    if (!quill) return;
    const handle = () => onDescriptionChange(quill.root.innerHTML);
    quill.on("text-change", handle);
    return () => {
      try {
        quill.off && (quill.off("text-change", handle) as any);
      } catch (e) {
        /* ignore */
      }
    };
  }, [quill, onDescriptionChange]);

  // keep local title in sync when parent changes it
  useEffect(() => {
    setTitle(Title || "");
  }, [Title]);

  useEffect(() => {
    // Clear local form state when the component mounts so the form always
    // starts empty when displayed. Keep cleanup on unmount as well.
    setTitle("");
    setDeadline(
      typeof BookingDeadlineDayCount === "number" ? BookingDeadlineDayCount : 1
    );
    setRequirements([]);
    setFiles([]);

    return () => {
      setTitle("");
      setDeadline(1);
      setRequirements([]);
      setFiles([]);
    };
  }, []);

  // keep local deadline in sync when parent changes it
  useEffect(() => {
    if (
      typeof BookingDeadlineDayCount === "number" &&
      BookingDeadlineDayCount > 0
    ) {
      setDeadline(BookingDeadlineDayCount);
    }
  }, [BookingDeadlineDayCount]);

  const handleAddRequirement = () => {
    const newOrder =
      requirements.length > 0
        ? Math.max(...requirements.map((r) => r.Order)) + 1
        : 1;
    const newReq: BookingRequirementInfo = {
      Name: "",
      Description: "",
      Order: newOrder,
      PodcastBookingToneId: "",
      ContentType: "file",
      ContentValue: "",
    };
    setRequirements([...requirements, newReq]);
    // inform parent of the newly created requirement so parent can track it too
    onCreateNewRequirementInfo(newReq);
  };

  const handleUpdateRequirement = (updatedReq: BookingRequirementInfo) => {
    setRequirements(
      requirements.map((r) => (r.Order === updatedReq.Order ? updatedReq : r))
    );
    onUpdateRequirementInfo(updatedReq);
  };

  const handleDeleteRequirement = (order: number) => {
    setRequirements(requirements.filter((r) => r.Order !== order));
    setFiles(files.filter((f) => !f.name.startsWith(`${order}.`)));
    // Notify parent to sync deletion
    onDeleteRequirementInfo(order);
  };

  const handleFileUpload = (order: number, file: File | null) => {
    if (file) {
      const filteredFiles = files.filter(
        (f) => !f.name.startsWith(`${order}.`)
      );
      setFiles([...filteredFiles, file]);
      onUploadNewFile(file);
    } else {
      setFiles(files.filter((f) => !f.name.startsWith(`${order}.`)));
    }
  };

  // Helper function to check if a requirement is complete
  const isRequirementComplete = (req: BookingRequirementInfo): boolean => {
    if (!req.Name.trim()) return false;
    if (!req.Description.trim() || req.Description === "<p><br></p>")
      return false;
    if (!req.PodcastBookingToneId) return false;
    if (!req.ContentType) return false;

    if (req.ContentType === "link") {
      return !!req.ContentValue?.trim();
    }
    if (req.ContentType === "script") {
      return !!req.ContentValue?.trim() && req.ContentValue !== "<p><br></p>";
    }
    if (req.ContentType === "file") {
      return !!files.find((f) => f.name.startsWith(`${req.Order}.`));
    }

    return false;
  };

  // Check if form is valid for submission
  const isFormValid = (): boolean => {
    // Check if basic fields are filled
    if (!title.trim()) return false;
    if (!Description.trim() || Description === "<p><br></p>") return false;
    if (deadline < 1) return false;

    // Check if at least one requirement is complete
    if (requirements.length === 0) return false;
    return requirements.some((req) => isRequirementComplete(req));
  };

  return (
    <div className="flex flex-col w-full p-8 gap-5">
      <p className="text-3xl font-bold text-white">Booking Requirement Form</p>

      <div className="w-full flex flex-col gap-6">
        <div className="w-full flex items-center justify-between gap-16">
          <div className="flex-1">
            <p className="text-white font-poppins font-semibold text-sm mb-1">
              Title
            </p>
            <input
              type="text"
              value={title}
              onChange={(e) => {
                setTitle(e.target.value);
                onTitleChange(e.target.value);
              }}
              placeholder="Enter title..."
              className="w-3/4 pb-2 border-b-white border-b-[1px] bg-transparent text-white placeholder:text-gray-400 outline-none"
            />
          </div>

          <div className="flex-1">
            <p className="text-white font-poppins font-semibold text-sm mb-1">
              Deadline Day Count
            </p>
            <input
              type="number"
              min={1}
              value={deadline}
              onChange={(e) => {
                const raw = e.target.value;
                const parsed = Number(raw);
                const v = Number.isNaN(parsed)
                  ? 1
                  : Math.max(1, Math.floor(parsed));
                setDeadline(v);
                try {
                  onDeadlineDayCountChange(v);
                } catch (err) {
                  /* ignore if parent handler not provided */
                }
              }}
              className="w-3/4 pb-2 border-b-white border-b-[1px] bg-transparent text-white placeholder:text-gray-400 outline-none"
            />
          </div>
        </div>

        <div className="w-full">
          <p className="text-white font-poppins font-semibold text-sm mb-2">
            Description
          </p>
          <div
            ref={quillRef}
            className="bg-white/5 rounded [&_.ql-toolbar]:border-none [&_.ql-toolbar]:bg-transparent [&_.ql-toolbar]:mb-2 [&_.ql-editor]:text-white [&_.ql-editor]:caret-white [&_.ql-editor]:min-h-[150px] [&_.ql-container]:bg-transparent"
          />
        </div>

        <div className="mt-20">
          <div className="flex flex-col mb-4 gap-3">
            <div className="w-full flex items-center justify-between">
              <h2 className="text-2xl font-bold text-white">Requirements</h2>
              {/* Only appear when form is valid */}
              {isFormValid() && (
                // <button
                //   onClick={onSubmit}
                //   className="px-6 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition"
                // >
                //   SUBMIT
                // </button>
                <LiquidButton
                  disabled={isCreating}
                  onClick={onSubmit}
                  variant="submit"
                >
                  {isCreating ? <p>SUBMITING...</p> : <p>SUBMIT</p>}
                </LiquidButton>
              )}
            </div>
            <div>
              <button
                onClick={handleAddRequirement}
                className="px-6 font-poppins py-2 text-white rounded transition-all duration-700 ease-out bg-gradient-to-r from-[#56CCF2]/40 to-[#2F80ED]/40 hover:from-[#56CCF2]/60 hover:to-[#2F80ED]/60 hover:-translate-y-0.5"
              >
                + Add Requirement
              </button>
            </div>
          </div>

          <div className="space-y-4">
            {[...requirements]
              .sort((a, b) => b.Order - a.Order)
              .map((req, idx) => (
                <RequirementItem
                  key={req.Order}
                  requirement={req}
                  index={idx}
                  availableTones={
                    availableBookingTonesOfPodcastBuddy?.PodcastBookingToneList ??
                    []
                  }
                  onUpdate={handleUpdateRequirement}
                  onDelete={handleDeleteRequirement}
                  onFileUpload={(file: File | null) =>
                    handleFileUpload(req.Order, file)
                  }
                  existingFile={files.find((f) =>
                    f.name.startsWith(`${req.Order}.`)
                  )}
                />
              ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default BookingForm;
