import { useEffect, useState } from "react";
import { useQuill } from "react-quilljs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import PdfSigning from "./components/PdfSigning";
import { Loader2 } from "lucide-react";
import { useGetBasicFileQuery } from "@/core/services/file/commitmentFile.service";
import { accountApi } from "@/core/services/account/account.service";

export type BecomePodcasterApiPayload = {
  PodcasterCreateProfileInfo: PodcasterCreateProfileInfo;
  CommitmentDocumentFile: File;
};

export type PodcasterCreateProfileInfo = {
  Name: string;
  Description: string;
};

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
      className="bg-white/5 rounded [&_.ql-toolbar]:border-none [&_.ql-toolbar]:bg-transparent [&_.ql-editor]:text-white [&_.ql-editor]:min-h-[150px]"
    />
  );
};

const BecomePodcaster = () => {
  const [name, setName] = useState("");
  const [description, setDescription] = useState("");
  const [finalFile, setFinalFile] = useState<File | null>(null);

  // Use RTKQuery to get Basic File first
  const { data: basicFileData, isLoading: isBasicFileLoading } =
    useGetBasicFileQuery();

  // Use RTKQuery mutation for podcaster apply
  const [podcasterApply, { isLoading: isApplying }] =
    accountApi.usePodcasterApplyMutation();

  useEffect(() => {
    if (basicFileData && basicFileData.FileUrl) {
      console.log("Basic commitment file url:", basicFileData.FileUrl);
    }
  }, [basicFileData]);

  // Callback nhận file PDF đã ký từ PdfSigning component
  const handlePdfSaved = (file: File) => {
    setFinalFile(file);
    console.log("PDF đã được ký và lưu:", file);
  };

  // FUNCTIONS
  const handleSubmitApply = async () => {
    if (!name.trim()) {
      alert("Vui lòng nhập tên podcaster!");
      return;
    }

    if (!description.trim()) {
      alert("Vui lòng nhập mô tả!");
      return;
    }

    if (!finalFile) {
      alert("Vui lòng ký vào file cam kết trước khi submit!");
      return;
    }

    try {
      const formData = new FormData();
      const PodcasterProfileCreateInfo = {
        Name: name,
        Description: description,
      };
      formData.append(
        "PodcasterProfileCreateInfo",
        JSON.stringify(PodcasterProfileCreateInfo)
      );
      formData.append("CommitmentDocumentFile", finalFile);

      // Gọi RTKQuery mutation
      const result = await podcasterApply({
        applyPodcasterFormData: formData,
      }).unwrap();

      alert("Đăng ký thành công! " + result.Message);

      // Reset form sau khi thành công
      setName("");
      setDescription("");
      setFinalFile(null);
    } catch (error: any) {
      console.error("Error applying:", error);
      alert("Có lỗi xảy ra: " + (error?.message || "Không xác định"));
    }
  };

  return (
    <div className="w-full flex flex-col items-center px-5 gap-10">
      <p className="text-5xl font-bold text-white font-poppins">
        Become Our <span className="text-mystic-green">Podcasters</span>
      </p>

      <div className="w-1/2 flex flex-col gap-2">
        <p className="text-white text-xl font-poppins font-semibold">
          Your Podcaster Name
        </p>
        <Input
          placeholder="Enter your podcaster name"
          value={name}
          onChange={(e: any) => setName(e?.target?.value || "")}
          className="bg-transparent border-0 border-b-[0.5px] border-white/20 text-white"
        />
      </div>

      <div className="w-1/2 flex flex-col gap-2">
        <p className="text-white text-xl font-poppins font-semibold">
          Your Podcaster Description
        </p>
        <ScriptEditor value={description} onChange={setDescription} />
      </div>

      <div className="w-1/2 flex flex-col gap-2">
        <p className="text-white text-xl font-poppins font-semibold">
          Please read the terms in the file carefully and sign to confirm
        </p>

        {isBasicFileLoading ? (
          <div className="flex items-center gap-2 text-white">
            <Loader2 className="h-4 w-4 animate-spin" />
            <span>Đang tải file cam kết...</span>
          </div>
        ) : basicFileData ? (
          <PdfSigning
            FileUrl={basicFileData.FileUrl}
            onSave={handlePdfSaved}
          />
        ) : (
          <p className="text-red-400">Không thể tải file cam kết</p>
        )}
      </div>

      <Button
        onClick={handleSubmitApply}
        disabled={isApplying || !finalFile}
        className="bg-mystic-green hover:bg-mystic-green/80 px-8 py-6 text-lg"
      >
        {isApplying ? (
          <>
            <Loader2 className="mr-2 h-5 w-5 animate-spin" />
            Đang gửi đơn...
          </>
        ) : (
          "Submit Application"
        )}
      </Button>

      {finalFile && (
        <p className="text-mystic-green text-sm">
          ✓ File đã được ký: {finalFile.name}
        </p>
      )}
    </div>
  );
};

export default BecomePodcaster;
