import { useState } from "react";

const Page3 = () => {
    const [files] = useState<string[]>([
        // Images - image/jpeg
        "Images_1.png",

        // Documents
        "Documents_1.pdf", // application/pdf
        "Documents_1.docx", // application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document
        "Documents_1.xlsx", // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        "Documents_1.pptx", // application/vnd.openxmlformats-officedocument.presentationml.presentation

        // Audio
        "Audio_1.mp3", // audio/mpeg

        // Video
        "Videos_1.mp4", // video/mp4

        // Text
        "Text_1.txt", // text/plain
        "Text_1.csv", // text/csv
        "Text_1.js", // application/javascript

        // Archives
        "Archives_1.zip", // application/zip
        "Archives_1.rar", // application/x-rar-compressed
        "Archives_1.7z",  // application/x-7z-compressed
        "Archives_1.tar", // application/x-tar
        "Archives_1.gz"   // application/gzip
    ]);
    return (
        <div>
            <h1>Page 3</h1>
            <p>This is page 3 - currently empty as requested.</p>

            <div className="urls-container">
                <h2>File Names: </h2>
                {files.map((file, index) => (
                    <div key={index} className="url-item">
                        <span>{file}</span>
                        <div>
                            Preview:
                            <div className="border rounded-lg overflow-hidden">

                                <iframe
                                    src={"http://localhost:8032/api/file/preview/"+ file}
                                    className="w-full h-96"
                                    title="Document Preview"
                                />
                            </div>
                        </div>
                        <div>
                            Download Link:
                            <a href={"http://localhost:8032/api/file/download/"+ file} rel="noopener noreferrer">
                                {"http://localhost:8032/api/file/download/"+ file}
                            </a>
                        </div>

                    </div>
                ))}
            </div>
        </div>
    );
};

export default Page3;