import { Document, Page, pdfjs } from "react-pdf";
pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.mjs`;
//pdfjs.GlobalWorkerOptions.workerSrc = `//unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.js`;

import { useEffect, useRef, useState } from "react";
import SignaturePad from "signature_pad";
import { PDFDocument } from "pdf-lib";
import { removeBackground } from "@imgly/background-removal";
import { Button } from "@/components/ui/button";
import { Loader2 } from "lucide-react";

interface PdfSigningProps {
  FileUrl: string;
  onSave?: (file: File) => void;
}

const PdfSigning = ({ FileUrl, onSave }: PdfSigningProps) => {
  const [numPages, setNumPages] = useState<number>();
  const [pageNumber, setPageNumber] = useState<number>(1);
  const [activeSigner, setActiveSigner] = useState<"A" | "B" | null>(null);
  const [signatureA, setSignatureA] = useState<string | null>(null);
  const [signatureB, setSignatureB] = useState<string | null>(null);
  const [pdfBytes, setPdfBytes] = useState<ArrayBuffer | null>(null);
  const [isLoadingUpload, setIsLoadingUpload] = useState(false);
  const [isSaving, setIsSaving] = useState(false);

  const canvasRef = useRef<HTMLCanvasElement | null>(null);
  const sigPadRef = useRef<SignaturePad | null>(null);

  // Fetch PDF từ FileUrl
  useEffect(() => {
    (async () => {
      try {
        const res = await fetch(FileUrl);
        const buffer = await res.arrayBuffer();
        setPdfBytes(buffer);
      } catch (error) {
        console.error("Error loading PDF:", error);
      }
    })();
  }, [FileUrl]);

  function onDocumentLoadSuccess({ numPages }: { numPages: number }): void {
    setNumPages(numPages);
  }

  function goToPrevPage() {
    setPageNumber((prev) => Math.max(prev - 1, 1));
  }

  function goToNextPage() {
    setPageNumber((prev) => (numPages ? Math.min(prev + 1, numPages) : prev));
  }

  // Init signature pad
  useEffect(() => {
    if (activeSigner && canvasRef.current) {
      sigPadRef.current = new SignaturePad(canvasRef.current, {
        backgroundColor: "white",
        penColor: "black",
      });
    }
  }, [activeSigner]);

  const clearSignature = () => sigPadRef.current?.clear();

  const saveSignature = (img?: string) => {
    let dataURL: string | null = null;

    if (img) {
      dataURL = img; // ảnh upload đã remove background
    } else if (sigPadRef.current && !sigPadRef.current.isEmpty()) {
      dataURL = sigPadRef.current.toDataURL("image/png");
    }

    if (!dataURL) {
      alert("Bạn chưa ký hoặc chưa upload!");
      return;
    }

    if (activeSigner === "A") setSignatureA(dataURL);
    if (activeSigner === "B") setSignatureB(dataURL);

    setActiveSigner(null);
  };

  // Upload + remove background
  const handleUploadSignature = async (
    e: React.ChangeEvent<HTMLInputElement>
  ) => {
    if (!e.target.files?.[0]) return;
    const file = e.target.files[0];

    setIsLoadingUpload(true);
    try {
      const blob = await removeBackground(file);
      const dataUrl = await new Promise<string>((resolve) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result as string);
        reader.readAsDataURL(blob);
      });

      saveSignature(dataUrl);
    } catch (err) {
      console.error("Remove background failed:", err);
      alert("Không thể xử lý ảnh. Vui lòng thử lại!");
    } finally {
      setIsLoadingUpload(false);
    }

    e.target.value = "";
  };

  // Tạo file PDF đã ký và callback về component cha
  const handleSavePdf = async () => {
    if (!pdfBytes) {
      alert("Không có tệp PDF!");
      return;
    }

    if (!signatureA || !signatureB) {
      alert("Vui lòng ký đầy đủ cả 2 chữ ký!");
      return;
    }

    setIsSaving(true);
    try {
      const pdfDoc = await PDFDocument.load(pdfBytes);
      const page = pdfDoc.getPage(pdfDoc.getPageCount() - 1);
      const { width } = page.getSize();

      if (signatureA) {
        const pngA = await pdfDoc.embedPng(signatureA);
        page.drawImage(pngA, { x: 80, y: 273, width: 200, height: 50 });
      }
      if (signatureB) {
        const pngB = await pdfDoc.embedPng(signatureB);
        page.drawImage(pngB, {
          x: width - 280,
          y: 273,
          width: 200,
          height: 50,
        });
      }

      const pdfBytesOut = await pdfDoc.save();
      // pdfBytesOut is a Uint8Array; cast to any to satisfy TS BlobPart type
      const pdfBlob = new Blob([pdfBytesOut as any], {
        type: "application/pdf",
      });
      const file = new File([pdfBlob], "signed_commitment.pdf", {
        type: "application/pdf",
      });

      // Callback về component cha
      onSave?.(file);
      alert("Lưu chữ ký thành công!");
    } catch (error) {
      console.error("Error saving PDF:", error);
      alert("Lỗi khi lưu PDF!");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <div className="flex flex-col gap-4">
      <Document file={FileUrl} onLoadSuccess={onDocumentLoadSuccess}>
        <div style={{ position: "relative" }}>
          <Page
            pageNumber={pageNumber}
            renderTextLayer={false}
            renderAnnotationLayer={false}
            width={600}
            className="page-no-interaction"
          />

          {numPages && pageNumber === numPages && (
            <>
              <div
                style={{
                  position: "absolute",
                  bottom: "273px",
                  left: "80px",
                  width: "200px",
                  height: "30px",
                  border: "2px dashed #333",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  background: "rgba(255,255,255,0.6)",
                  cursor: "pointer",
                  color: "black",
                }}
                onClick={() => setActiveSigner("A")}
              >
                {signatureA ? (
                  <img
                    src={signatureA}
                    alt="Signature A"
                    style={{ maxHeight: "100%" }}
                  />
                ) : (
                  "Bên A ký tại đây"
                )}
              </div>

              <div
                style={{
                  position: "absolute",
                  bottom: "273px",
                  right: "80px",
                  width: "200px",
                  height: "30px",
                  border: "2px dashed #333",
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  background: "rgba(255,255,255,0.6)",
                  cursor: "pointer",
                  color: "black",
                }}
                onClick={() => setActiveSigner("B")}
              >
                {signatureB ? (
                  <img
                    src={signatureB}
                    alt="Signature B"
                    style={{ maxHeight: "100%" }}
                  />
                ) : (
                  "Bên B ký tại đây"
                )}
              </div>
            </>
          )}
        </div>
      </Document>

      <div className="flex items-center gap-4">
        <p className="text-white">
          Page {pageNumber} of {numPages}
        </p>
        <Button
          onClick={goToPrevPage}
          disabled={pageNumber <= 1}
          variant="outline"
          size="sm"
        >
          Previous
        </Button>
        <Button
          onClick={goToNextPage}
          disabled={numPages ? pageNumber >= numPages : true}
          variant="outline"
          size="sm"
        >
          Next
        </Button>
      </div>

      <Button
        onClick={handleSavePdf}
        disabled={!signatureA || !signatureB || isSaving}
        className="bg-mystic-green hover:bg-mystic-green/80"
      >
        {isSaving ? (
          <>
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            Đang lưu...
          </>
        ) : (
          "Lưu chữ ký"
        )}
      </Button>

      {/* Popup ký */}
      {activeSigner && (
        <div
          style={{
            position: "fixed",
            top: 0,
            left: 0,
            right: 0,
            bottom: 0,
            background: "rgba(0,0,0,0.5)",
            display: "flex",
            justifyContent: "center",
            alignItems: "center",
            zIndex: 1000,
          }}
        >
          <div
            style={{
              background: "white",
              padding: "20px",
              borderRadius: "8px",
              display: "flex",
              flexDirection: "column",
              alignItems: "center",
              color: "black",
            }}
          >
            <h3 className="mb-4 font-semibold">Ký cho bên {activeSigner}</h3>
            {isLoadingUpload ? (
              <div className="flex items-center gap-2">
                <Loader2 className="h-4 w-4 animate-spin" />
                <p>Đang xử lý ảnh, vui lòng chờ...</p>
              </div>
            ) : (
              <canvas
                ref={canvasRef}
                width={400}
                height={200}
                style={{ border: "1px solid #000", marginBottom: "10px" }}
              />
            )}
            <div style={{ marginBottom: "10px" }}>
              <input
                type="file"
                accept="image/*"
                onChange={handleUploadSignature}
              />
            </div>
            <div className="flex gap-2">
              <Button onClick={clearSignature} variant="outline" size="sm">
                Xóa
              </Button>
              <Button onClick={() => saveSignature()} size="sm">
                Lưu
              </Button>
              <Button
                onClick={() => setActiveSigner(null)}
                variant="destructive"
                size="sm"
              >
                Hủy
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default PdfSigning;
