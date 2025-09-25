import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

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

function App() {
  const [count, setCount] = useState(0)
  const [urls, setUrls] = useState<string[]>([
    "http://localhost:8032/images_test/facilities/1/main.jpg", 
    "http://localhost:8032/document_files/1.pdf",
    "http://localhost:8032/document_files/1.mp3",
    "http://localhost:8032/document_files/1.docx",
    "http://localhost:8032/document_files/1.txt",
    "http://localhost:8032/document_files/1.zip",
    "http://localhost:8032/document_files/1.xlsx",
  ])
  return (
    <>
      <div>
        <a href="https://vite.dev" target="_blank">
          <img src={viteLogo} className="logo" alt="Vite logo" />
        </a>
        <a href="https://react.dev" target="_blank">
          <img src={reactLogo} className="logo react" alt="React logo" />
        </a>
      </div>
      <div className="card">
        
      </div>
      <div className="card">
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>

      </div>
      <p className="read-the-docs">
        Click on the Vite and React logos to learn more
      </p>

      {urls.map((url, index) => (
        <div key={index}>
          <a href={url} target="_blank">{url}</a>
        </div>
      ))}
    </>
  )
}

export default App
