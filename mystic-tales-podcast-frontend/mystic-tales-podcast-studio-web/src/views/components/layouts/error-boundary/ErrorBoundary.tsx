// src/components/ErrorBoundary.tsx
import React, { Component, ReactNode } from "react";

interface Props {
  children: ReactNode;
}

interface State {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  constructor(props: Props) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): State {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
    console.error("❌ ErrorBoundary caught an error", error, errorInfo);
    // Có thể gửi log về server tại đây
  }

  render() {
    if (this.state.hasError) {
      return (
        <div className="flex flex-col items-center justify-center h-screen text-center p-4 bg-gray-50">
          <div className="max-w-md mx-auto">
            <div className="text-6xl mb-4">😵</div>
            <h1 className="text-3xl font-bold text-red-500 mb-4">
              Oops! Something went wrong
            </h1>
            <p className="text-gray-600 mb-2">
              We encountered an unexpected error. Don't worry, it's not your
              fault!
            </p>
            <p className="text-sm text-gray-500 mb-6 bg-gray-100 p-3 rounded border-l-4 border-red-400">
              <strong>Error:</strong> {this.state.error?.message}
            </p>
            <div className="flex gap-3 justify-center">
              <button
                className="px-6 py-3 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors font-medium"
                onClick={() => window.location.reload()}
              >
                🔄 Reload Page
              </button>
              <button
                className="px-6 py-3 bg-green-600 text-white rounded-lg hover:bg-green-700 transition-colors font-medium"
                onClick={() => (window.location.href = "/")}
              >
                🏠 Go To Home
              </button>
            </div>
            <p className="text-xs text-gray-400 mt-4">
              If the problem persists, please contact support.
            </p>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
