import { useEffect, useRef } from "react";

interface FlipBookViewerProps {
  pdfUrl: string;
}

export default function FlipBookViewer({ pdfUrl }: FlipBookViewerProps) {
  const viewerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    // @ts-ignore
    if (window.$ && window.$.fn && window.$.fn.FlipBook && viewerRef.current) {
      // Tạo FlipBook
      // @ts-ignore
      $(viewerRef.current).FlipBook({
        pdf: pdfUrl,
        template: {
          html: "https://cdn.jsdelivr.net/npm/3dflipbook@1.7.6/dist/templates/default-book-view.html",
          styles: [
            "https://cdn.jsdelivr.net/npm/3dflipbook@1.7.6/dist/css/short-white-book-view.css",
          ],
          script:
            "https://cdn.jsdelivr.net/npm/3dflipbook@1.7.6/dist/js/default-book-view.js",
        },
        propertiesCallback: (props: any) => {
          props.page.depth = 0.002;
          props.cover.padding = 0.002;
          return props;
        },
      });
    }
  }, [pdfUrl]);

  return (
    <div
      ref={viewerRef}
      style={{
        width: "100%",
        height: "90vh",
        overflow: "hidden",
        background: "#ddd",
      }}
    />
  );
}
