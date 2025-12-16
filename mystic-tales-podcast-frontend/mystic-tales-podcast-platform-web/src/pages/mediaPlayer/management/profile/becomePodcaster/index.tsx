// @ts-nocheck

import { useEffect, useState } from "react";
import { useQuill } from "react-quilljs";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

import PdfSigning from "./components/PdfSigning";
import { Loader2 } from "lucide-react";
import {
  useGetBasicFileQuery,
  useUploadSignImageMutation,
} from "@/core/services/file/commitmentFile.service";
import { accountApi } from "@/core/services/account/account.service";
import { useDispatch, useSelector } from "react-redux";
import type { RootState } from "@/redux/store";
import { showAlert } from "@/redux/slices/alertSlice/alertSlice";
import { useNavigate } from "react-router-dom";

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
  const [signatureFormData, setSignatureFormData] = useState<FormData | null>(
    null
  );

  const user = useSelector((state: RootState) => state.auth.user);

  const dispatch = useDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    if (user?.IsPodcaster) {
      dispatch(
        showAlert({
          type: "warning",
          title: "Oops!",
          description:
            "Your account is already a podcaster. You cannot edit the description.",
          isAutoClose: false,
          isClosable: false,
          functionalButtonText: "Got it",
          isFunctional: true,
          onClickAction: () => {
            navigate("/media-player/management/profile");
          },
        })
      );
    }
  }, []);

  // Use RTKQuery to get Basic File first
  const { data: pdfBytes, isLoading } = useGetBasicFileQuery();
  const [uploadSignImage] = useUploadSignImageMutation();

  // State để lưu file PDF đã được ký từ backend
  const [signedPdfBytes, setSignedPdfBytes] = useState<ArrayBuffer | null>(
    null
  );

  // Use RTKQuery mutation for podcaster apply
  const [podcasterApply, { isLoading: isApplying }] =
    accountApi.usePodcasterApplyMutation();

  const handleSignChange = async (data: FormData | null) => {
    if (data) {
      try {
        console.log("Sending signature to backend...");
        const signedPdf = await uploadSignImage({ formData: data }).unwrap();
        console.log("Signed PDF received:", signedPdf);

        // Lưu file PDF đã ký để hiển thị
        setSignedPdfBytes(signedPdf);

        // Tạo File từ signed PDF và lưu vào signatureFormData để submit
        const signedBlob = new Blob([signedPdf], { type: "application/pdf" });
        const signedFile = new File([signedBlob], "signed_commitment.pdf", {
          type: "application/pdf",
        });
        const finalFormData = new FormData();
        finalFormData.append("SignatureImage", signedFile);
        setSignatureFormData(finalFormData);

        console.log("Received signed PDF from backend");
        alert("Signed attach success!");
      } catch (error: any) {
        console.error("Error uploading signature image:", error);
        console.error("Error details:", {
          message: error?.message,
          status: error?.status,
          data: error?.data,
        });
        setSignatureFormData(null);
      }
    } else {
      // Nếu xoá chữ ký, reset về PDF gốc
      setSignedPdfBytes(null);
      setSignatureFormData(null);
    }
  };

  useEffect(() => {
    if (pdfBytes) {
      console.log("Basic commitment file bytes received");
      // DOWNLOAD LUÔN
      // const blob = new Blob([pdfBytes], { type: "application/pdf" });
      // const url = URL.createObjectURL(blob);
      // const a = document.createElement("a");
      // a.href = url;
      // a.download = "commitment-document.pdf";
      // a.click();
      // URL.revokeObjectURL(url);
    }
  }, [pdfBytes]);

  // Callback nhận file PDF đã ký từ PdfSigning component
  // const handlePdfSaved = (file: File) => {
  //   setFinalFile(file);
  //   console.log("PDF đã được ký và lưu:", file);
  // };

  // FUNCTIONS
  const handleSubmitApply = async () => {
    if (!name.trim()) {
      alert("Please enter your podcaster name!");
      return;
    }

    if (!description.trim()) {
      alert("Please enter the podcaster description!");
      return;
    }

    if (!signedPdfBytes) {
      alert("Please sign the commitment file before submitting!");
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

      // Tạo File từ signedPdfBytes (ArrayBuffer) để gửi về backend
      const signedPdfBlob = new Blob([signedPdfBytes], {
        type: "application/pdf",
      });
      const signedPdfFile = new File([signedPdfBlob], "signed_commitment.pdf", {
        type: "application/pdf",
      });
      formData.append("CommitmentDocumentFile", signedPdfFile);

      const result = await podcasterApply({
        applyPodcasterFormData: formData,
      }).unwrap();

      dispatch(
        showAlert({
          type: "success",
          title: "Application Submitted!",
          description:
            "Your application to become a podcaster has been submitted successfully. We will review your application and get back to you soon.",
          isAutoClose: false,
          isClosable: false,
          functionalButtonText: "OK",
          isFunctional: true,
          onClickAction: () => {
            navigate("/media-player/management/profile");
          },
        })
      );
    } catch (error: any) {
      console.error("Error applying:", error);
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

        {isLoading ? (
          <div className="flex items-center gap-2 text-white">
            <Loader2 className="h-4 w-4" />
            <span>Đang tải file cam kết...</span>
          </div>
        ) : pdfBytes ? (
          <PdfSigning
            FileBytes={signedPdfBytes || pdfBytes}
            onSignChange={handleSignChange}
          />
        ) : (
          <p className="text-red-400">Không thể tải file cam kết</p>
        )}
      </div>

      <Button
        onClick={handleSubmitApply}
        disabled={isApplying || !signedPdfBytes}
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

      {signedPdfBytes && (
        <p className="text-mystic-green text-sm">
          ✓ File cam kết đã được ký thành công
        </p>
      )}
    </div>
  );
};

export default BecomePodcaster;
