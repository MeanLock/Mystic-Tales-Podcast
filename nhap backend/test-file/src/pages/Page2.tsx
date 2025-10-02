import { useState } from "react";

const Page2 = () => {
    const [urls] = useState<string[]>([
        // Images - image/jpeg
        "http://localhost:8032/api/file/download/Images_1.png",

        // Documents
        "http://localhost:8032/api/file/download/Documents_1.pdf", // application/pdf
        "http://localhost:8032/api/file/download/Documents_1.docx", // application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document
        "http://localhost:8032/api/file/download/Documents_1.xlsx", // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        "http://localhost:8032/api/file/download/Documents_1.pptx", // application/vnd.openxmlformats-officedocument.presentationml.presentation

        // Audio
        "http://localhost:8032/api/file/download/Audio_1.mp3", // audio/mpeg

        // Video
        "http://localhost:8032/api/file/download/Videos_1.mp4", // video/mp4

        // Text
        "http://localhost:8032/api/file/download/Text_1.txt", // text/plain
        "http://localhost:8032/api/file/download/Text_1.csv", // text/csv
        "http://localhost:8032/api/file/download/Text_1.js", // application/javascript

        // Archives
        "http://localhost:8032/api/file/download/Archives_1.zip", // application/zip
        "http://localhost:8032/api/file/download/Archives_1.rar", // application/x-rar-compressed
        "http://localhost:8032/api/file/download/Archives_1.7z",  // application/x-7z-compressed
        "http://localhost:8032/api/file/download/Archives_1.tar", // application/x-tar
        "http://localhost:8032/api/file/download/Archives_1.gz"   // application/gzip
    ]);
    return (
        <div>
            <h1>Page 2</h1>
            <p>This is page 2 - currently empty as requested.</p>
            <div className="border rounded-lg overflow-hidden">

                <iframe
                    src={"http://localhost:8032/api/file/preview/docx/Documents_1.docx"}
                    className="w-full h-96"
                    title="Document Preview"
                />
            </div>

            <div className="urls-container">
                <h2>File URLs:</h2>
                {urls.map((url, index) => (
                    <div key={index} className="url-item">
                        <a href={url} rel="noopener noreferrer">{url}</a>
                    </div>
                ))}
            </div>
        </div>
    );
};

export default Page2;