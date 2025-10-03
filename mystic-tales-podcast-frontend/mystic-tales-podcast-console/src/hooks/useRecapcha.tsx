import Turnstile, { useTurnstile } from "react-turnstile";
import  { useEffect, useRef } from "react";

interface TurnstileWidgetProps {
  setIsVerified: (isVerified: boolean) => void;
  title?: string;
  subtitle?: string;
  onClose?: () => void;
  isRefetch?: boolean;
  isVerified?: boolean;
}

function TurnstileWidget({ 
  setIsVerified, 
  title = "Xác minh bảo mật",
  subtitle = "Vui lòng hoàn thành xác minh để tiếp tục",
  onClose, 
  isRefetch,
  isVerified
}: TurnstileWidgetProps) {
  const turnstile = useTurnstile();
  const turnstileRef = useRef<any>(null);

  useEffect(() => {
    if (isRefetch && turnstileRef.current) {
      turnstileRef.current.reset();
      turnstile.reset();
    }
    
    return () => {
      if (turnstileRef.current) {
        turnstileRef.current.reset();
      }
    };
  }, [setIsVerified, isRefetch, turnstile]);

  useEffect(() => {
    setIsVerified(false);
  }, [isRefetch])

  // Listen for page refresh/reload events
  useEffect(() => {
    const handleBeforeUnload = () => {
      setIsVerified(false);
    };

    const handleVisibilityChange = () => {
      if (document.visibilityState === 'visible') {
        if (turnstileRef.current) {
          turnstileRef.current.reset();
        }
      }
    };

    window.addEventListener('beforeunload', handleBeforeUnload);
    document.addEventListener('visibilitychange', handleVisibilityChange);

    return () => {
      window.removeEventListener('beforeunload', handleBeforeUnload);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
    };
  }, [setIsVerified]);

  const overlayStyle: React.CSSProperties = {
    position: "fixed",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    backgroundColor: "rgba(0, 0, 0, 0.7)",
    backdropFilter: "blur(4px)",
    justifyContent: "center",
    alignItems: "center",
    zIndex: 9999,
    animation: "fadeIn 0.3s ease-out",
    display: isVerified ? "none" : "flex"
  };

  const containerStyle: React.CSSProperties = {
    backgroundColor: "#ffffff",
    borderRadius: "16px",
    padding: "32px",
    boxShadow: "0 20px 40px rgba(0, 0, 0, 0.15)",
    maxWidth: "400px",
    width: "90%",
    textAlign: "center",
    position: "relative",
    animation: "slideUp 0.3s ease-out"
  };

  const headerStyle: React.CSSProperties = {
    marginBottom: "24px"
  };

  const titleStyle: React.CSSProperties = {
    fontSize: "24px",
    fontWeight: "600",
    color: "#1f2937",
    margin: "0 0 8px 0"
  };

  const subtitleStyle: React.CSSProperties = {
    fontSize: "14px",
    color: "#6b7280",
    margin: "0 0 24px 0",
    lineHeight: "1.5"
  };

  const closeButtonStyle: React.CSSProperties = {
    position: "absolute",
    top: "16px",
    right: "16px",
    background: "none",
    border: "none",
    fontSize: "24px",
    color: "#9ca3af",
    cursor: "pointer",
    width: "32px",
    height: "32px",
    borderRadius: "50%",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    transition: "all 0.2s ease"
  };

  const turnstileContainerStyle: React.CSSProperties = {
    display: "flex",
    justifyContent: "center",
    marginBottom: "16px"
  };

  const footerTextStyle: React.CSSProperties = {
    fontSize: "12px",
    color: "#9ca3af",
    marginTop: "16px"
  };

  const refreshNoticeStyle: React.CSSProperties = {
    fontSize: "11px",
    color: "#ef4444",
    marginTop: "8px",
    fontStyle: "italic"
  };

  const injectStyles = () => {
    const style = document.createElement('style');
    style.textContent = `
      @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
      }
      
      @keyframes slideUp {
        from { 
          opacity: 0;
          transform: translateY(20px);
        }
        to { 
          opacity: 1;
          transform: translateY(0);
        }
      }
      
      .turnstile-close-btn:hover {
        background-color: #f3f4f6 !important;
        color: #374151 !important;
      }
    `;
    document.head.appendChild(style);
    return () => {
      document.head.removeChild(style);
    };
  };

  useEffect(() => {
    const cleanup = injectStyles();
    return cleanup;
  }, []);

  const handleSuccess = (token: string) => {
    setTimeout(() => {
      setIsVerified(true);
      if (turnstileRef.current) {
        turnstileRef.current.reset();
        turnstile.reset();
      }
      onClose?.();
    }, 1500);
  };

  const handleError = (error: string) => {
    setIsVerified(false);
    setTimeout(() => {
      if (turnstileRef.current) {
        turnstileRef.current.reset();
      }
    }, 3000);
  };

  const handleExpire = () => {
    //console.log('Turnstile verification expired');
    setIsVerified(false);
    
    if (turnstileRef.current) {
      turnstileRef.current.reset();
    }
  };

  const handleLoad = () => {
  };

  return (
    <div style={overlayStyle}>
      <div style={containerStyle}>
        {onClose && (
          <button
            style={closeButtonStyle}
            className="turnstile-close-btn"
            onClick={onClose}
            aria-label="Đóng"
          >
            ×
          </button>
        )}
        
        <div style={headerStyle}>
          <h2 style={titleStyle}>{title}</h2>
          <p style={subtitleStyle}>{subtitle}</p>
        </div>

        <div style={turnstileContainerStyle}>
          <Turnstile
            sitekey="0x4AAAAAABl2UhtPeoGdQS-h"
            onSuccess={handleSuccess}
            onError={handleError}
            onExpire={handleExpire}
            onLoad={handleLoad}
            theme="light"
            size="normal"
            key={isRefetch ? Date.now() : undefined}
          />
        </div>

        <p style={footerTextStyle}>
          Được bảo vệ bởi Cloudflare Turnstile
        </p>
      </div>
    </div>
  );
}

export default TurnstileWidget;