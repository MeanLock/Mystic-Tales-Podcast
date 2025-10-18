import { useState } from "react";
import AudioPreview from "../components/AudioPreviewer";
import DocumentPreview from "../components/DocumentPreviewer";
import DocumentPreviewer_1 from "../components/DocumentPreviewer_1";

type UrlType = {
    default: string;
    download: string;
    preview: string;
};

const LocalStorage = () => {
    //     jpg
    // jpeg
    // png
    // gif
    // webp
    // svg
    // wav
    // flac  
    // mp3 
    // m4a
    // aac
    // pdf
    // doc
    // docx
    // xls
    // xlsx
    // txt
    // csv
    // zip 
    // rar

    const [imageUrls] = useState<UrlType[]>([
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.jpg",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.jpg",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.jpg"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.jpeg",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.jpeg",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.jpeg"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.png",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.png",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.png"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.gif",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.gif",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.gif"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.webp",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.webp",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.webp"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.svg",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.svg",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.svg"
        }
    ]);

    const [audioUrls] = useState<UrlType[]>([
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.wav",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.flac",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.mp3",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.m4a",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.aac"
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.wav",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.wav",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.wav"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.flac",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.flac",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.flac"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.mp3",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.mp3",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.mp3"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.m4a",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.m4a",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.m4a"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.aac",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.aac",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.aac"
        }
    ]);

    const [documentUrls] = useState<UrlType[]>([
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.pdf",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.doc",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.docx",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.xls",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.xlsx",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.txt",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.csv"
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.pdf",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.pdf",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.pdf"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.doc",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.doc",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.doc"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.docx",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.docx",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.docx"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.xls",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.xls",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.xls"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.xlsx",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.xlsx",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.xlsx"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.txt",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.txt",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.txt"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.csv",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.csv",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.csv"
        }
    ]);
    const [archiveUrls] = useState<UrlType[]>([
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.zip",
        // "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.rar"
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.zip",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.zip",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.zip"
        },
        {
            default: "http://localhost:8036/api/Misc/file-io-test/generate-url/file_extension_test/1.rar",
            download: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.rar",
            preview: "http://localhost:8036/api/Misc/file-io-test/download/file_extension_test/1.rar"
        }
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
                {imageUrls.map((url, index) => (
                    <div key={index} className="url-item">
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Default url:
                            <a href={url.default} target="_blank" rel="noopener noreferrer">{url.default}</a>
                        </span>
                        <br />
                        <div>
                            Default Preview:
                            <img src={url.default} alt="" width={500} height={500} />
                        </div>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Download url:
                            <a href={url.download} rel="noopener noreferrer">{url.download}</a>
                        </span>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Custom Preview:
                            <a href={url.preview} target="_blank" rel="noopener noreferrer">{url.preview}</a>
                        </span>

                    </div>
                ))}
            </div>

            <div className="urls-container">
                <h2>Audio File URLs:</h2>
                {audioUrls.map((url, index) => (
                    <div key={index} className="url-item">
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Default url:
                            <a href={url.default} target="_blank" rel="noopener noreferrer">{url.default}</a>
                        </span>
                        <br />
                        <div>
                            Default Preview:
                            <audio controls>
                                <source src={url.default} />
                                Your browser does not support the audio element.
                            </audio>
                            <AudioPreview
                                audioUrl={url.default}
                                fileName={url.default.split('/').pop() || 'audio'}
                                fileExtension={(url.default.split('.').pop() || 'mp3').toLowerCase()}
                                showDownload={true}
                            />
                        </div>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Download url:
                            <a href={url.download} rel="noopener noreferrer">{url.download}</a>
                        </span>
                        
                    </div>
                ))}
            </div>

            <div className="urls-container">
                <h2>Document File URLs:</h2>
                {documentUrls.map((url, index) => (
                    <div key={index} className="url-item">
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Default url:
                            <a href={url.default} target="_blank" rel="noopener noreferrer">{url.default}</a>
                        </span>
                        <br />
                        <div>
                            Default Preview:
                            <div className="border rounded-lg overflow-hidden">

                            </div>
                        </div>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Download url:
                            <a href={url.download} rel="noopener noreferrer">{url.download}</a>
                        </span>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Custom Preview:
                            <span>
                                File Name: {url.preview.split('/').pop()}
                                File Extension: {(url.preview.split('.').pop() || 'pdf').toLowerCase()}
                            </span>
                            {/* <DocumentPreview documentUrl={url.default} fileName={url.preview.split('/').pop() || 'document'} fileExtension={(url.preview.split('.').pop() || 'pdf').toLowerCase()} /> */}
                            <DocumentPreviewer_1 documents={[{uri: url.default}]} />
                        </span>

                    </div>
                ))}
            </div>

            <div className="urls-container">
                <h2>Archive File URLs:</h2>
                {archiveUrls.map((url, index) => (
                    <div key={index} className="url-item">
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Default url:
                            <a href={url.default} target="_blank" rel="noopener noreferrer">{url.default}</a>
                        </span>
                        <br />
                        <div>
                            Note: Archives cannot be previewed. Please use the link above to download.
                        </div>
                        <br />
                        <span style={{ color: "blue", fontWeight: "bold" }}>
                            Download url:
                            <a href={url.download} rel="noopener noreferrer">{url.download}</a>
                        </span>
                        <br />
                    </div>
                ))}
            </div>
        </div>
    );
};

export default LocalStorage;