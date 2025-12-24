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

  // Thông tin cá nhân
  const [identityCardNumber, setIdentityCardNumber] = useState("");
  const [fullName, setFullName] = useState("");
  const [phone, setPhone] = useState("");
  const [frontImageFile, setFrontImageFile] = useState<File | null>(null);
  const [backImageFile, setBackImageFile] = useState<File | null>(null);
  const [signatureImageFile, setSignatureImageFile] = useState<File | null>(
    null
  );

  const user = useSelector((state: RootState) => state.auth.user);

  const dispatch = useDispatch();
  const navigate = useNavigate();

  useEffect(() => {
    const checkUserStatus = async () => {
      if (!user) {
        dispatch(
          showAlert({
            type: "warning",
            title: "Oops!",
            description: "You need to login first to access this function!",
            isAutoClose: false,
            isClosable: false,
            functionalButtonText: "Go to login",
            isFunctional: true,
            onClickAction: () => {
              navigate("/auth/login");
            },
          })
        );
        return;
      } else if (user.IsPodcaster) {
        dispatch(
          showAlert({
            type: "info",
            title: "You're Podcaster!",
            description:
              "Your account is already a podcaster! Please go to Podcaster Studio Site if you needed.",
            isAutoClose: false,
            isClosable: false,
            functionalButtonText: "Got it",
            isFunctional: true,
            onClickAction: () => {
              navigate("/media-player/management/profile");
            },
          })
        );
        return;
      } else if (user.IsPodcasterApplying) {
        dispatch(
          showAlert({
            type: "info",
            title: "You've submitted the apply",
            description:
              "We already received your podcaster application. Please wait for our decision to continue",
            isAutoClose: false,
            isClosable: false,
            functionalButtonText: "Got it",
            isFunctional: true,
            onClickAction: () => {
              navigate("/media-player/management/profile");
            },
          })
        );
        return;
      }
    };

    checkUserStatus();
  }, [user, dispatch, navigate]);

  // Use RTKQuery to get Basic File first
  const { data: pdfBytes, isLoading } = useGetBasicFileQuery();
  const [uploadSignImage] = useUploadSignImageMutation();

  // State để lưu file PDF đã được ký từ backend
  const [signedPdfBytes, setSignedPdfBytes] = useState<ArrayBuffer | null>(
    null
  );
  const [isGettingCommitment, setIsGettingCommitment] = useState(false);

  // Use RTKQuery mutation for podcaster apply
  const [podcasterApply, { isLoading: isApplying }] =
    accountApi.usePodcasterApplyMutation();

  const handleSignChange = (signatureFile: File | null) => {
    setSignatureImageFile(signatureFile);
  };

  const validatePersonalInfo = (): string | null => {
    if (!identityCardNumber.trim()) {
      return "Vui lòng nhập số căn cước công dân!";
    }
    if (!/^\d{12}$/.test(identityCardNumber)) {
      return "Số căn cước công dân phải có 12 chữ số!";
    }
    if (!fullName.trim()) {
      return "Vui lòng nhập họ và tên!";
    }
    if (!phone.trim()) {
      return "Vui lòng nhập số điện thoại!";
    }
    if (!/^\d{10}$/.test(phone)) {
      return "Số điện thoại phải có 10 chữ số!";
    }
    if (!frontImageFile) {
      return "Vui lòng upload ảnh căn cước mặt trước!";
    }
    if (!backImageFile) {
      return "Vui lòng upload ảnh căn cước mặt sau!";
    }
    if (!signatureImageFile) {
      return "Vui lòng upload ảnh chữ ký!";
    }
    return null;
  };

  const handleGetCommitment = async () => {
    const validationError = validatePersonalInfo();
    if (validationError) {
      dispatch(
        showAlert({
          title: "Invalid Informations",
          description: validationError,
          type: "warning",
          isAutoClose: true,
          autoCloseDuration: 10,
          isClosable: true,
          isFunctional: false,
          functionalButtonText: "Got it",
        })
      );
      alert(validationError);
      return;
    }

    setIsGettingCommitment(true);
    try {
      const formData = new FormData();
      formData.append("IdentityCardNumber", identityCardNumber);
      formData.append("FullName", fullName);
      formData.append("Phone", phone);
      formData.append("IdentityCardFrontImageFile", frontImageFile!);
      formData.append("IdentityCardBackImageFile", backImageFile!);
      formData.append("SignatureImageFile", signatureImageFile!);

      console.log("Sending commitment request to backend...");
      const signedPdf = await uploadSignImage({ formData }).unwrap();
      console.log("Signed PDF received:", signedPdf);

      setSignedPdfBytes(signedPdf);
      dispatch(
        showAlert({
          type: "success",
          title: "Success!",
          description: "Lấy file cam kết đã ký thành công!",
          isAutoClose: true,
          isClosable: true,
          autoCloseDuration: 3,
        })
      );
    } catch (error: any) {
      console.error("Error getting commitment:", error);
      dispatch(
        showAlert({
          type: "error",
          title: "Opps!",
          description: "Can't get signed-commitment-file, please try again!",
          isAutoClose: true,
          autoCloseDuration: 10,
          isClosable: true,
          functionalButtonText: "Got it",
          isFunctional: false,
        })
      );
    } finally {
      setIsGettingCommitment(false);
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

      await podcasterApply({
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
          Personal Information
        </p>
        <Input
          placeholder="Họ và tên (theo CCCD)"
          value={fullName}
          onChange={(e: any) => setFullName(e?.target?.value || "")}
          className="bg-transparent border-0 border-b-[0.5px] border-white/20 text-white"
        />
        <Input
          placeholder="Số căn cước công dân (12 chữ số)"
          value={identityCardNumber}
          onChange={(e: any) => {
            const value = e?.target?.value || "";
            if (/^\d{0,12}$/.test(value)) {
              setIdentityCardNumber(value);
            }
          }}
          maxLength={12}
          className="bg-transparent border-0 border-b-[0.5px] border-white/20 text-white"
        />
        <Input
          placeholder="Số điện thoại (10 chữ số)"
          value={phone}
          onChange={(e: any) => {
            const value = e?.target?.value || "";
            if (/^\d{0,10}$/.test(value)) {
              setPhone(value);
            }
          }}
          maxLength={10}
          className="bg-transparent border-0 border-b-[0.5px] border-white/20 text-white"
        />
      </div>

      <div className="w-1/2 flex flex-col gap-4">
        <p className="text-white text-xl font-poppins font-semibold">
          Identity Card Images
        </p>

        {/* Mặt trước CCCD */}
        <div className="flex flex-col gap-2">
          <label className="text-white text-sm font-medium">
            Ảnh mặt trước CCCD:
          </label>
          <div className="relative w-full" style={{ paddingBottom: "63.08%" }}>
            {/* aspect ratio: 53.98/85.6 = 0.6308 */}
            <div className="absolute inset-0 border-2 border-dashed border-white/40 rounded-lg overflow-hidden bg-white/5 hover:border-mystic-green/60 transition-colors cursor-pointer">
              {frontImageFile ? (
                <div className="relative w-full h-full">
                  <img
                    src={URL.createObjectURL(frontImageFile)}
                    alt="CCCD mặt trước"
                    className="w-full h-full object-cover"
                  />
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      setFrontImageFile(null);
                    }}
                    className="absolute top-2 right-2 bg-red-500 hover:bg-red-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-sm"
                  >
                    ×
                  </button>
                </div>
              ) : (
                <label className="w-full h-full flex flex-col items-center justify-center cursor-pointer">
                  <svg
                    className="w-12 h-12 text-white/40 mb-2"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4v16m8-8H4"
                    />
                  </svg>
                  <span className="text-white/60 text-sm">
                    Click để chọn ảnh
                  </span>
                  <span className="text-white/40 text-xs mt-1">
                    (85.6mm × 53.98mm)
                  </span>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e: any) => {
                      const file = e?.target?.files?.[0];
                      if (file) setFrontImageFile(file);
                    }}
                    className="hidden"
                  />
                </label>
              )}
            </div>
          </div>
          {frontImageFile && (
            <span className="text-mystic-green text-xs">
              ✓ {frontImageFile.name}
            </span>
          )}
        </div>

        {/* Mặt sau CCCD */}
        <div className="flex flex-col gap-2">
          <label className="text-white text-sm font-medium">
            Ảnh mặt sau CCCD:
          </label>
          <div className="relative w-full" style={{ paddingBottom: "63.08%" }}>
            {/* aspect ratio: 53.98/85.6 = 0.6308 */}
            <div className="absolute inset-0 border-2 border-dashed border-white/40 rounded-lg overflow-hidden bg-white/5 hover:border-mystic-green/60 transition-colors cursor-pointer">
              {backImageFile ? (
                <div className="relative w-full h-full">
                  <img
                    src={URL.createObjectURL(backImageFile)}
                    alt="CCCD mặt sau"
                    className="w-full h-full object-cover"
                  />
                  <button
                    onClick={(e) => {
                      e.preventDefault();
                      setBackImageFile(null);
                    }}
                    className="absolute top-2 right-2 bg-red-500 hover:bg-red-600 text-white rounded-full w-6 h-6 flex items-center justify-center text-sm"
                  >
                    ×
                  </button>
                </div>
              ) : (
                <label className="w-full h-full flex flex-col items-center justify-center cursor-pointer">
                  <svg
                    className="w-12 h-12 text-white/40 mb-2"
                    fill="none"
                    stroke="currentColor"
                    viewBox="0 0 24 24"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 4v16m8-8H4"
                    />
                  </svg>
                  <span className="text-white/60 text-sm">
                    Click để chọn ảnh
                  </span>
                  <span className="text-white/40 text-xs mt-1">
                    (85.6mm × 53.98mm)
                  </span>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e: any) => {
                      const file = e?.target?.files?.[0];
                      if (file) setBackImageFile(file);
                    }}
                    className="hidden"
                  />
                </label>
              )}
            </div>
          </div>
          {backImageFile && (
            <span className="text-mystic-green text-xs">
              ✓ {backImageFile.name}
            </span>
          )}
        </div>
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
          <>
            <PdfSigning
              FileBytes={signedPdfBytes || pdfBytes}
              onSignChange={handleSignChange}
            />
            <Button
              onClick={handleGetCommitment}
              disabled={isGettingCommitment || !!signedPdfBytes}
              className="bg-blue-600 hover:bg-blue-700 px-6 py-4"
            >
              {isGettingCommitment ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Đang lấy file cam kết...
                </>
              ) : signedPdfBytes ? (
                "✓ Đã lấy file cam kết"
              ) : (
                "Get My Commitment"
              )}
            </Button>
          </>
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
