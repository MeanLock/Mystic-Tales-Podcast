import { useState } from 'react';

const FILE_BEHAVIOR_MATRIX = {
    // Always Preview
    ALWAYS_PREVIEW: [
        { extensions: ['jpg', 'jpeg', 'png', 'gif', 'webp', 'svg', 'bmp'], reason: 'Native image support' },
        { extensions: ['txt', 'csv', 'json', 'xml', 'html', 'css', 'js'], reason: 'Text content' }
    ],

    // Browser Dependent Preview
    CONDITIONAL_PREVIEW: [
        { extensions: ['pdf'], condition: 'PDF plugin available', fallback: 'download' },
        { extensions: ['mp4', 'webm', 'ogg'], condition: 'Video codec support', fallback: 'download' },
        { extensions: ['mp3', 'wav', 'ogg', 'aac'], condition: 'Audio codec support', fallback: 'download' }
    ],

    // Always Download
    ALWAYS_DOWNLOAD: [
        { extensions: ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'], reason: 'Requires office app' },
        { extensions: ['zip', 'rar', '7z', 'tar', 'gz'], reason: 'Archive file' },
        { extensions: ['exe', 'msi', 'deb', 'rpm'], reason: 'Executable file' },
        { extensions: ['dmg', 'iso', 'img'], reason: 'Disk image' }
    ],

    // External Viewer
    EXTERNAL_VIEWER: [
        { extensions: ['dwg', 'cad'], reason: 'CAD file - requires specialized viewer' },
        { extensions: ['psd', 'ai'], reason: 'Adobe file - requires specialized viewer' }
    ]
};


const getMimeTypeFromExtension = (extension: string): string => {
    const mimeTypes: Record<string, string> = {
        // Images
        'jpg': 'image/jpeg',
        'jpeg': 'image/jpeg',
        'png': 'image/png',
        'gif': 'image/gif',
        'webp': 'image/webp',
        'svg': 'image/svg+xml',
        'bmp': 'image/bmp',
        'ico': 'image/x-icon',

        // Videos
        'mp4': 'video/mp4',
        'webm': 'video/webm',
        'ogg': 'video/ogg',
        'avi': 'video/x-msvideo',
        'mov': 'video/quicktime',
        'wmv': 'video/x-ms-wmv',
        'flv': 'video/x-flv',
        '3gp': 'video/3gpp',

        // Audio
        'mp3': 'audio/mpeg',
        'wav': 'audio/wav',
        'ogg': 'audio/ogg',
        'aac': 'audio/aac',
        'flac': 'audio/flac',
        'm4a': 'audio/mp4',
        'wma': 'audio/x-ms-wma',

        // Documents
        'pdf': 'application/pdf',
        'doc': 'application/msword',
        'docx': 'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
        'xls': 'application/vnd.ms-excel',
        'xlsx': 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
        'ppt': 'application/vnd.ms-powerpoint',
        'pptx': 'application/vnd.openxmlformats-officedocument.presentationml.presentation',

        // Text
        'txt': 'text/plain',
        'csv': 'text/csv',
        'json': 'application/json',
        'xml': 'application/xml',
        'html': 'text/html',
        'css': 'text/css',
        'js': 'application/javascript',

        // Archives
        'zip': 'application/zip',
        'rar': 'application/x-rar-compressed',
        '7z': 'application/x-7z-compressed',
        'tar': 'application/x-tar',
        'gz': 'application/gzip'
    };

    return mimeTypes[extension] || 'application/octet-stream';
};
const Page1 = () => {
    const [count, setCount] = useState(0);
    const [urls] = useState<string[]>([
        // Images - image/jpeg
        "http://localhost:8032/document_files/Images/1.png",

        // Documents
        "http://localhost:8032/document_files/Documents/1.pdf", // application/pdf
        "http://localhost:8032/document_files/Documents/1.docx", // application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document
        "http://localhost:8032/document_files/Documents/1.xlsx", // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
        "http://localhost:8032/document_files/Documents/1.pptx", // application/vnd.openxmlformats-officedocument.presentationml.presentation

        // Audio
        "http://localhost:8032/document_files/Audio/1.mp3", // audio/mpeg

        // Video
        "http://localhost:8032/document_files/Videos/1.mp4", // video/mp4

        // Text
        "http://localhost:8032/document_files/Text/1.txt", // text/plain
        "http://localhost:8032/document_files/Text/1.csv", // text/csv
        "http://localhost:8032/document_files/Text/1.js", // application/javascript

        // Archives
        "http://localhost:8032/document_files/Archives/1.zip", // application/zip
        "http://localhost:8032/document_files/Archives/1.rar", // application/x-rar-compressed
        "http://localhost:8032/document_files/Archives/1.7z",  // application/x-7z-compressed
        "http://localhost:8032/document_files/Archives/1.tar", // application/x-tar
        "http://localhost:8032/document_files/Archives/1.gz"   // application/gzip
    ]);

    return (
        <div>
            <h1>Page 1 - URLs Display</h1>

            <div className="card">
                <button onClick={() => setCount((count) => count + 1)}>
                    count is {count}
                </button>
            </div>

            <div className="urls-container">
                <h2>File URLs:</h2>
                {urls.map((url, index) => (
                    <div key={index} className="url-item">
                        <a href={url} target="_blank" rel="noopener noreferrer">{url}</a>
                    </div>
                ))}
            </div>

            <div className="file-behavior-info">
                <h2>File Behavior Matrix:</h2>
                <pre>{JSON.stringify(FILE_BEHAVIOR_MATRIX, null, 2)}</pre>
            </div>

            <object data="http://africau.edu/images/default/sample.pdf" type="application/pdf" width="100%" height="100%">
                <p>Alternative text - include a link <a href="http://africau.edu/images/default/sample.pdf">to the PDF!</a></p>
            </object>
        </div>
    );
};

export default Page1;