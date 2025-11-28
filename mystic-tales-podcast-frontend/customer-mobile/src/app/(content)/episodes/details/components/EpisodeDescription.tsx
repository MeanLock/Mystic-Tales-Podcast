import { View } from "@/src/components/ui/View";
import { useEffect, useState } from "react";
import { WebView } from "react-native-webview";
import sanitizeHtml from "sanitize-html";

const EpisodeDescription = ({ description }: { description: string }) => {
  const [webViewHeight, setWebViewHeight] = useState(0);

  const safeHtml = sanitizeHtml(description ?? "", {
    allowedTags: false, // cho phép nhiều hơn, nhưng cân nhắc security
    allowedAttributes: false,
  });

  // Bạn có thể inject CSS để match theme app
  const html = `
    <html>
      <head>
        <meta name="viewport" content="width=device-width, initial-scale=1" />
        <style>
          body { font-family: -apple-system, Roboto, "Helvetica Neue", Arial; padding: 12px; color: #fff; background: #000; margin: 0; }
          a { color: #AEE339; }
          blockquote { border-left: 3px solid #514F4F; padding-left: 10px; opacity: .9; }
          img { max-width: 100%; height: auto; }
        </style>
      </head>
      <body>
        ${safeHtml}
        <script>
          // Gửi chiều cao của nội dung lên React Native
          function sendHeight() {
            const height = document.body.scrollHeight;
            window.ReactNativeWebView.postMessage(JSON.stringify({ type: 'setHeight', height }));
          }
          
          // Gửi chiều cao khi load xong
          window.addEventListener('load', sendHeight);
          
          // Gửi lại khi có thay đổi (ví dụ: ảnh load xong)
          const images = document.getElementsByTagName('img');
          for (let img of images) {
            img.addEventListener('load', sendHeight);
          }
          
          // Gửi ngay lập tức
          sendHeight();
        </script>
      </body>
    </html>
  `;

  const onMessage = (event: any) => {
    try {
      const data = JSON.parse(event.nativeEvent.data);
      if (data.type === "setHeight" && data.height) {
        setWebViewHeight(data.height);
      }
    } catch (error) {
      console.error("Error parsing WebView message:", error);
    }
  };

  return (
    <View style={{ height: webViewHeight || 400 }}>
      <WebView
        originWhitelist={["*"]}
        source={{ html }}
        onMessage={onMessage}
        setSupportMultipleWindows={false}
        showsVerticalScrollIndicator={false}
        scrollEnabled={false}
        backgroundColor="transparent"
      />
    </View>
  );
};

export default EpisodeDescription;
