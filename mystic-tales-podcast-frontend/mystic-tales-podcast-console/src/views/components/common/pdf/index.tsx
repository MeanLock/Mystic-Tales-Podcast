import React, { useEffect, useState } from 'react'
import { Document, Page, pdfjs } from 'react-pdf'
import 'react-pdf/dist/esm/Page/AnnotationLayer.css'
import 'react-pdf/dist/esm/Page/TextLayer.css'
import './PdfViewer.scss'

// Set worker - dùng unpkg để tránh lỗi worker
pdfjs.GlobalWorkerOptions.workerSrc = `https://unpkg.com/pdfjs-dist@${pdfjs.version}/build/pdf.worker.min.js`

interface PdfViewerProps {
  fileUrl: string
  isOpen: boolean
  onClose: () => void
  title?: string
}

const PdfViewer: React.FC<PdfViewerProps> = ({ fileUrl, isOpen, onClose, title = 'PDF Document' }) => {
  const [numPages, setNumPages] = useState<number | null>(null)
  const [pageNumber, setPageNumber] = useState(1)
  const [scale, setScale] = useState(1.0)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    if (isOpen && fileUrl) {
      setNumPages(null)
      setPageNumber(1)
      setScale(1)
      setError(null)
      setLoading(true)
    }
  }, [isOpen, fileUrl])

  const onDocumentLoadSuccess = ({ numPages }: { numPages: number }) => {
    setNumPages(numPages)
    setPageNumber(1)
    setLoading(false)
    setError(null)
  }

  const onDocumentLoadError = (err: Error) => {
    console.error('PDF Load Error:', err)
    setError(`Failed to load PDF: ${err.message}`)
    setLoading(false)
  }

  const goToPrevPage = () => {
    setPageNumber(prevPageNumber => Math.max(prevPageNumber - 1, 1))
  }

  const goToNextPage = () => {
    setPageNumber(prevPageNumber => Math.min(prevPageNumber + 1, numPages || 1))
  }

  const zoomIn = () => {
    setScale(prevScale => Math.min(prevScale + 0.2, 2.0))
  }

  const zoomOut = () => {
    setScale(prevScale => Math.max(prevScale - 0.2, 0.5))
  }

  if (!isOpen) return null

  return (
    <div className="pdf-viewer-overlay">
      <div className="pdf-viewer-modal">
        <div className="pdf-viewer-header">
          <h3 className="pdf-viewer-title">{title}</h3>
          <div className="pdf-viewer-controls">
            <button 
              onClick={zoomOut} 
              disabled={scale <= 0.5} 
              className="pdf-viewer-btn"
            >
              🔍-
            </button>
            <span className="pdf-viewer-zoom">{Math.round(scale * 100)}%</span>
            <button 
              onClick={zoomIn} 
              disabled={scale >= 2.0} 
              className="pdf-viewer-btn"
            >
              🔍+
            </button>
            <button onClick={onClose} className="pdf-viewer-close">
              ✕
            </button>
          </div>
        </div>

        <div className="pdf-viewer-content">
          {loading && !error && (
            <div className="pdf-viewer-loading">
              <div className="spinner"></div>
              <p>Loading PDF...</p>
            </div>
          )}

          {error && (
            <div className="pdf-viewer-error">
              <p>{error}</p>
              <p className="pdf-viewer-url">URL: {fileUrl}</p>
              <button onClick={onClose} className="pdf-viewer-btn">Close</button>
            </div>
          )}

          {!error && fileUrl && (
            <Document
              file="https://mozilla.github.io/pdf.js/web/compressed.tracemonkey-pldi-09.pdf"
              onLoadSuccess={onDocumentLoadSuccess}
              onLoadError={onDocumentLoadError}
              onSourceError={onDocumentLoadError}
              className="pdf-document"
              loading=""
              options={{
                cMapUrl: `https://unpkg.com/pdfjs-dist@${pdfjs.version}/cmaps/`,
                cMapPacked: true,
                standardFontDataUrl: `https://unpkg.com/pdfjs-dist@${pdfjs.version}/standard_fonts/`,
              }}
            >
              <Page 
                pageNumber={pageNumber} 
                scale={scale} 
                className="pdf-page" 
                renderTextLayer={true}
                renderAnnotationLayer={true}
                loading=""
              />
            </Document>
          )}

          {!error && !fileUrl && (
            <div className="pdf-viewer-error">
              <p>No PDF file URL provided</p>
              <button onClick={onClose} className="pdf-viewer-btn">Close</button>
            </div>
          )}
        </div>

        {numPages && !error && (
          <div className="pdf-viewer-footer">
            <button 
              onClick={goToPrevPage} 
              disabled={pageNumber <= 1}
              className="pdf-viewer-btn"
            >
              Previous
            </button>
            <span className="pdf-viewer-page-info">
              Page {pageNumber} of {numPages}
            </span>
            <button 
              onClick={goToNextPage} 
              disabled={pageNumber >= numPages}
              className="pdf-viewer-btn"
            >
              Next
            </button>
          </div>
        )}
      </div>
    </div>
  )
}

export default PdfViewer