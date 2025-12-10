import { useSmartAudio } from '@/hooks/useSmartAudio';
import React from 'react';

interface SmartAudioPlayerProps {
  audioId: string;
  initialUrl?: string;
  className?: string;
  fetchUrlFunction: (fileKey: string) => Promise<{ success: boolean; data?: { FileUrl: string }; message?: string }>;
}

export const SmartAudioPlayer: React.FC<SmartAudioPlayerProps> = ({
  audioId,
  initialUrl,
  className,
  fetchUrlFunction,
}) => {
  const {
    audioRef,
    currentUrl,
    isLoading,
    status,
  } = useSmartAudio({
    audioId,
    initialUrl,
    expiresIn: 5,
    fetchUrlFunction,
    onError: (error) => {
      console.error('Audio error:', error);
    },
    onStatusChange: (status) => {
      console.log('Status changed:', status);
    },
  });

  return (
    <div className={`audio-container ${className || ''}`}>
      <audio
        ref={audioRef}
        src={currentUrl || ''}
        controls
        controlsList="nodownload"
        style={{ width: '100%' }}
      />
      <div className="status" style={{ marginTop: 10, fontSize: 12, color: '#666' }}>
        {/* {isLoading ? '⏳ Đang tải...' :''} */}
      </div>
    </div>
  );
};