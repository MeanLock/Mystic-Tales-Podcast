import React from 'react';

type Props = { url?: string; height?: number; className?: string };

const getExt = (url?: string) => {
  if (!url) return '';
  try {
    const u = new URL(url);
    const path = u.pathname.toLowerCase();
    const i = path.lastIndexOf('.');
    return i >= 0 ? path.slice(i + 1) : '';
  } catch {
    const path = (url || '').toLowerCase();
    const i = path.lastIndexOf('.');
    return i >= 0 ? path.slice(i + 1) : '';
  }
};

export const DocumentViewer: React.FC<Props> = ({ url, height = 420, className }) => {
  if (!url) return null;
  const ext = getExt(url);

  if (['png','jpg','jpeg','gif','webp','bmp','svg'].includes(ext)) {
    return <img src={url} alt="document" style={{ width: '100%', height, objectFit: 'contain', borderRadius: 8 }} className={className} />;
  }

  if (['mp3','wav','ogg','m4a','aac','flac'].includes(ext)) {
    return <audio controls src={url} style={{ width: '100%' }} />;
  }

  if (['mp4','webm','ogg','mov','mkv'].includes(ext)) {
    return <video controls src={url} style={{ width: '100%', height }} />;
  }

  if (ext === 'pdf') {
    return (
      <object data={url} type="application/pdf" width="100%" height={height}>
        <iframe src={url} style={{ width: '100%', height }} />
        <a href={url} target="_blank" rel="noopener noreferrer">Open PDF</a>
      </object>
    );
  }

  if (['doc','docx','ppt','pptx','xls','xlsx'].includes(ext)) {
    const officeSrc = `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(url)}`;
    return <iframe src={officeSrc} style={{ width: '100%', height }} allowFullScreen className={className} />;
  }

  return (
    <div>
      <p style={{ opacity: 0.75, fontSize: 14 }}>Preview not supported.</p>
    </div>
  );
};