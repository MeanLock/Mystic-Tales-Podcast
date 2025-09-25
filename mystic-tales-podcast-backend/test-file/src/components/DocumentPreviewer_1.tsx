import React from 'react';
    import DocViewer, { DocViewerRenderers } from '@cyntler/react-doc-viewer';

    const DocumentPreviewer_1: React.FC = ({documents} : any) => {
      // const documents = [
      //   { uri: 'https://example.com/document.pdf' },
      //   { uri: 'https://example.com/image.png' },
      //   // Add more documents as needed
      // ];

      return (
        <DocViewer documents={documents} pluginRenderers={DocViewerRenderers} />
      );
    };

    export default DocumentPreviewer_1;