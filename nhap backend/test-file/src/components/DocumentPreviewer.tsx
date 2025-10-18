import React, { useState, useEffect, useRef } from 'react';
import { Download, FileText, Eye, AlertCircle, Archive, Table, File } from 'lucide-react';

// Import libraries for document processing
// Note: These would need to be installed via npm
// npm install react-pdf pdfjs-dist mammoth xlsx papaparse jszip

interface DocumentPreviewProps {
  documentUrl: string;
  fileName: string;
  fileExtension: 'pdf' | 'doc' | 'docx' | 'xls' | 'xlsx' | 'txt' | 'csv' | 'zip' | 'rar' | string;
  onDownload?: () => void;
  maxHeight?: string;
}

interface PreviewState {
  isLoading: boolean;
  error: string | null;
  content: any;
  previewType: 'pdf' | 'text' | 'table' | 'html' | 'archive' | 'unsupported';
}

const DocumentPreview: React.FC<DocumentPreviewProps> = ({
  documentUrl,
  fileName,
  fileExtension,
  onDownload,
  maxHeight = '600px'
}) => {
  const [previewState, setPreviewState] = useState<PreviewState>({
    isLoading: true,
    error: null,
    content: null,
    previewType: 'unsupported'
  });

  const containerRef = useRef<HTMLDivElement>(null);

  // Get file type icon
  const getFileIcon = (ext: string) => {
    const iconMap = {
      pdf: '📄',
      doc: '📝',
      docx: '📝',
      xls: '📊',
      xlsx: '📊',
      txt: '📄',
      csv: '📋',
      zip: '📦',
      rar: '📦'
    };
    return iconMap[ext as keyof typeof iconMap] || '📄';
  };

  // Determine preview capability
  const getPreviewCapability = (ext: string) => {
    switch (ext.toLowerCase()) {
      case 'pdf':
        return { canPreview: true, type: 'pdf', method: 'PDF.js' };
      case 'txt':
        return { canPreview: true, type: 'text', method: 'Text Reader' };
      case 'csv':
        return { canPreview: true, type: 'table', method: 'Papa Parse' };
      case 'docx':
        return { canPreview: true, type: 'html', method: 'Mammoth.js' };
      case 'xlsx':
        return { canPreview: true, type: 'table', method: 'SheetJS' };
      case 'zip':
        return { canPreview: true, type: 'archive', method: 'JSZip' };
      case 'doc':
      case 'xls':
      case 'rar':
        return { canPreview: false, type: 'unsupported', method: 'None' };
      default:
        return { canPreview: false, type: 'unsupported', method: 'None' };
    }
  };

  // PDF Preview Component
  const PDFPreview: React.FC<{ url: string }> = ({ url }) => {
    const [numPages, setNumPages] = useState<number>(0);
    const [pageNumber, setPageNumber] = useState<number>(1);

    // Note: In real implementation, you'd use react-pdf
    // import { Document, Page, pdfjs } from 'react-pdf';
    // pdfjs.GlobalWorkerOptions.workerSrc = `//cdnjs.cloudflare.com/ajax/libs/pdf.js/${pdfjs.version}/pdf.worker.min.js`;

    return (
      <div className="border rounded-lg overflow-hidden">
        <div className="bg-gray-100 px-4 py-2 flex justify-between items-center">
          <span className="text-sm text-gray-600">
            Page {pageNumber} of {numPages || '?'}
          </span>
          <div className="flex space-x-2">
            <button
              onClick={() => setPageNumber(Math.max(1, pageNumber - 1))}
              className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600"
              disabled={pageNumber <= 1}
            >
              Previous
            </button>
            <button
              onClick={() => setPageNumber(pageNumber + 1)}
              className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600"
              disabled={pageNumber >= numPages}
            >
              Next
            </button>
          </div>
        </div>
        
        {/* PDF Viewer */}
        <div className="bg-white p-4" style={{ height: maxHeight }}>
          <iframe
            src={`${url}#page=${pageNumber}&view=FitH`}
            width="100%"
            height="100%"
            style={{ border: 'none' }}
            title="PDF Preview"
          />
        </div>
      </div>
    );
  };

  // Text Preview Component
  const TextPreview: React.FC<{ content: string }> = ({ content }) => (
    <div 
      className="bg-white border rounded-lg p-4 font-mono text-sm overflow-auto"
      style={{ height: maxHeight }}
    >
      <pre className="whitespace-pre-wrap break-words">
        {content}
      </pre>
    </div>
  );

  // Table Preview Component (CSV/Excel)
  const TablePreview: React.FC<{ data: any[]; headers: string[] }> = ({ data, headers }) => (
    <div className="border rounded-lg overflow-hidden">
      <div className="bg-gray-100 px-4 py-2">
        <span className="text-sm text-gray-600">
          {data.length} rows × {headers.length} columns
        </span>
      </div>
      <div 
        className="overflow-auto bg-white"
        style={{ height: maxHeight }}
      >
        <table className="w-full text-sm">
          <thead className="bg-gray-50 sticky top-0">
            <tr>
              {headers.map((header, index) => (
                <th key={index} className="px-4 py-2 text-left font-medium text-gray-900 border-b">
                  {header}
                </th>
              ))}
            </tr>
          </thead>
          <tbody>
            {data.slice(0, 100).map((row, rowIndex) => (
              <tr key={rowIndex} className="hover:bg-gray-50">
                {headers.map((header, colIndex) => (
                  <td key={colIndex} className="px-4 py-2 border-b text-gray-700">
                    {row[header]?.toString() || ''}
                  </td>
                ))}
              </tr>
            ))}
            {data.length > 100 && (
              <tr>
                <td colSpan={headers.length} className="px-4 py-2 text-center text-gray-500 italic">
                  ... and {data.length - 100} more rows
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  );

  // HTML Preview Component (Word docs)
  const HTMLPreview: React.FC<{ html: string }> = ({ html }) => (
    <div 
      className="bg-white border rounded-lg p-6 overflow-auto prose max-w-none"
      style={{ height: maxHeight }}
      dangerouslySetInnerHTML={{ __html: html }}
    />
  );

  // Archive Preview Component
  const ArchivePreview: React.FC<{ files: any[] }> = ({ files }) => (
    <div className="border rounded-lg overflow-hidden">
      <div className="bg-gray-100 px-4 py-2">
        <span className="text-sm text-gray-600">
          {files.length} files in archive
        </span>
      </div>
      <div 
        className="bg-white overflow-auto"
        style={{ height: maxHeight }}
      >
        <div className="divide-y">
          {files.map((file, index) => (
            <div key={index} className="flex items-center px-4 py-3 hover:bg-gray-50">
              <File size={16} className="text-gray-400 mr-3" />
              <div className="flex-1">
                <div className="font-medium text-gray-900">{file.name}</div>
                <div className="text-sm text-gray-500">
                  {file.size ? `${(file.size / 1024).toFixed(1)} KB` : 'Unknown size'}
                  {file.date && ` • ${new Date(file.date).toLocaleDateString()}`}
                </div>
              </div>
              {file.isDirectory && (
                <span className="text-xs bg-blue-100 text-blue-800 px-2 py-1 rounded">
                  Folder
                </span>
              )}
            </div>
          ))}
        </div>
      </div>
    </div>
  );

  // Load document content
  useEffect(() => {
    const loadDocument = async () => {
      setPreviewState(prev => ({ ...prev, isLoading: true, error: null }));

      try {
        const capability = getPreviewCapability(fileExtension);
        
        if (!capability.canPreview) {
          setPreviewState({
            isLoading: false,
            error: null,
            content: null,
            previewType: 'unsupported'
          });
          return;
        }

        const response = await fetch(documentUrl);
        if (!response.ok) throw new Error('Failed to fetch document');

        switch (fileExtension.toLowerCase()) {
          case 'pdf':
            setPreviewState({
              isLoading: false,
              error: null,
              content: documentUrl,
              previewType: 'pdf'
            });
            break;

          case 'txt':
            const textContent = await response.text();
            setPreviewState({
              isLoading: false,
              error: null,
              content: textContent,
              previewType: 'text'
            });
            break;

          case 'csv':
            const csvContent = await response.text();
            // Note: In real implementation, use Papa.parse
            // import Papa from 'papaparse';
            // const parsed = Papa.parse(csvContent, { header: true });
            const lines = csvContent.split('\n');
            const headers = lines[0]?.split(',') || [];
            const data = lines.slice(1).map(line => {
              const values = line.split(',');
              const row: any = {};
              headers.forEach((header, index) => {
                row[header.trim()] = values[index]?.trim() || '';
              });
              return row;
            }).filter(row => Object.values(row).some(value => value));

            setPreviewState({
              isLoading: false,
              error: null,
              content: { data, headers: headers.map(h => h.trim()) },
              previewType: 'table'
            });
            break;

          case 'docx':
            // Note: In real implementation, use mammoth
            // import mammoth from 'mammoth';
            // const arrayBuffer = await response.arrayBuffer();
            // const result = await mammoth.convertToHtml({ arrayBuffer });
            setPreviewState({
              isLoading: false,
              error: null,
              content: '<div class="p-4"><h2>DOCX Preview</h2><p>Document preview would appear here with proper mammoth.js implementation.</p><p>This requires converting the DOCX file to HTML on the client side.</p></div>',
              previewType: 'html'
            });
            break;

          case 'xlsx':
            // Note: In real implementation, use SheetJS
            // import * as XLSX from 'xlsx';
            // const arrayBuffer = await response.arrayBuffer();
            // const workbook = XLSX.read(arrayBuffer, { type: 'array' });
            // const sheetName = workbook.SheetNames[0];
            // const worksheet = workbook.Sheets[sheetName];
            // const data = XLSX.utils.sheet_to_json(worksheet);
            setPreviewState({
              isLoading: false,
              error: null,
              content: {
                data: [
                  { Name: 'John Doe', Age: 30, City: 'New York' },
                  { Name: 'Jane Smith', Age: 25, City: 'Los Angeles' },
                  { Name: 'Bob Johnson', Age: 35, City: 'Chicago' }
                ],
                headers: ['Name', 'Age', 'City']
              },
              previewType: 'table'
            });
            break;

          case 'zip':
            // Note: In real implementation, use JSZip
            // import JSZip from 'jszip';
            // const arrayBuffer = await response.arrayBuffer();
            // const zip = await JSZip.loadAsync(arrayBuffer);
            const mockFiles = [
              { name: 'document.pdf', size: 1024000, date: new Date(), isDirectory: false },
              { name: 'images/', size: null, date: new Date(), isDirectory: true },
              { name: 'images/photo1.jpg', size: 512000, date: new Date(), isDirectory: false },
              { name: 'data.csv', size: 2048, date: new Date(), isDirectory: false }
            ];
            
            setPreviewState({
              isLoading: false,
              error: null,
              content: mockFiles,
              previewType: 'archive'
            });
            break;

          default:
            throw new Error(`Unsupported file type: ${fileExtension}`);
        }
      } catch (error) {
        setPreviewState({
          isLoading: false,
          error: error instanceof Error ? error.message : 'Unknown error',
          content: null,
          previewType: 'unsupported'
        });
      }
    };

    loadDocument();
  }, [documentUrl, fileExtension]);

  // Render preview content
  const renderPreviewContent = () => {
    if (previewState.isLoading) {
      return (
        <div className="flex items-center justify-center p-8" style={{ height: maxHeight }}>
          <div className="text-center">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500 mx-auto mb-4"></div>
            <div className="text-gray-600">Loading document preview...</div>
          </div>
        </div>
      );
    }

    if (previewState.error) {
      return (
        <div className="bg-red-50 border border-red-200 rounded-lg p-6 text-center">
          <AlertCircle className="h-12 w-12 text-red-400 mx-auto mb-4" />
          <div className="text-red-800 font-medium mb-2">Preview Error</div>
          <div className="text-red-600 text-sm mb-4">{previewState.error}</div>
          <button
            onClick={onDownload}
            className="bg-red-600 text-white px-4 py-2 rounded text-sm hover:bg-red-700 flex items-center space-x-2 mx-auto"
          >
            <Download size={16} />
            <span>Download File</span>
          </button>
        </div>
      );
    }

    switch (previewState.previewType) {
      case 'pdf':
        return <PDFPreview url={previewState.content} />;
      case 'text':
        return <TextPreview content={previewState.content} />;
      case 'table':
        return <TablePreview data={previewState.content.data} headers={previewState.content.headers} />;
      case 'html':
        return <HTMLPreview html={previewState.content} />;
      case 'archive':
        return <ArchivePreview files={previewState.content} />;
      case 'unsupported':
      default:
        return (
          <div className="bg-gray-50 border-2 border-dashed border-gray-300 rounded-lg p-8 text-center">
            <div className="text-4xl mb-4">{getFileIcon(fileExtension)}</div>
            <div className="text-gray-800 font-medium mb-2">{fileName}</div>
            <div className="text-gray-600 text-sm mb-4">
              {fileExtension.toUpperCase()} files cannot be previewed in browser
            </div>
            <div className="text-xs text-gray-500 mb-4">
              Supported formats: PDF, TXT, CSV, DOCX, XLSX, ZIP
            </div>
            <button
              onClick={onDownload}
              className="bg-blue-600 text-white px-4 py-2 rounded text-sm hover:bg-blue-700 flex items-center space-x-2 mx-auto"
            >
              <Download size={16} />
              <span>Download to View</span>
            </button>
          </div>
        );
    }
  };

  const capability = getPreviewCapability(fileExtension);

  return (
    <div className="w-full max-w-4xl mx-auto">
      {/* Header */}
      <div className="bg-white border border-gray-200 rounded-t-lg px-4 py-3 flex items-center justify-between">
        <div className="flex items-center space-x-3">
          <div className="text-2xl">{getFileIcon(fileExtension)}</div>
          <div>
            <div className="font-medium text-gray-900 truncate max-w-md">
              {fileName}
            </div>
            <div className="text-sm text-gray-500">
              {fileExtension.toUpperCase()} • {capability.canPreview ? `Preview via ${capability.method}` : 'Download only'}
            </div>
          </div>
        </div>
        
        <div className="flex items-center space-x-2">
          {capability.canPreview && (
            <div className="flex items-center text-green-600 text-xs">
              <Eye size={14} className="mr-1" />
              <span>Preview Available</span>
            </div>
          )}
          <button
            onClick={onDownload}
            className="text-gray-500 hover:text-gray-700 p-2"
            title="Download"
          >
            <Download size={20} />
          </button>
        </div>
      </div>

      {/* Preview Content */}
      <div className="border-l border-r border-b border-gray-200 rounded-b-lg">
        {renderPreviewContent()}
      </div>

      {/* Footer Info */}
      <div className="mt-2 text-xs text-gray-500 text-center">
        <div>
          ✅ PDF, TXT, CSV, DOCX, XLSX, ZIP | ❌ DOC, XLS, RAR (Legacy formats)
        </div>
        <div className="mt-1">
          Note: This demo uses mock data. Real implementation requires installing: react-pdf, mammoth, xlsx, papaparse, jszip
        </div>
      </div>
    </div>
  );
};

export default DocumentPreview;