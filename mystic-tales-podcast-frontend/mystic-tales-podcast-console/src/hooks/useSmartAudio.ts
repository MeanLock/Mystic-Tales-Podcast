import { useRef, useEffect, useCallback, useState } from 'react';

interface UseSmartAudioOptions {
    audioId: string;
    initialUrl?: string;
    expiresIn?: number; // seconds
    fetchUrlFunction: (fileKey: string) => Promise<{ success: boolean; data?: { FileUrl: string }; message?: string }>;
    onError?: (error: Error) => void;
    onStatusChange?: (status: string) => void;
}

export const useSmartAudio = ({
    audioId,
    initialUrl,
    expiresIn = 5,
    fetchUrlFunction,
    onError,
    onStatusChange,
}: UseSmartAudioOptions) => {
    const audioRef = useRef<HTMLAudioElement>(null);
    const [currentUrl, setCurrentUrl] = useState<string | null>(initialUrl || null);
    const [urlCreatedAt, setUrlCreatedAt] = useState<number | null>(
        initialUrl ? Date.now() : null
    );
    const [isLoading, setIsLoading] = useState(false);
    const [status, setStatus] = useState<string>('Đang khởi tạo...');

    const updateStatus = useCallback((newStatus: string) => {
        setStatus(newStatus);
        onStatusChange?.(newStatus);
    }, [onStatusChange]);

    const refreshPresignedUrl = useCallback(async (preservePlayback = false): Promise<boolean> => {
        const audio = audioRef.current;
        const currentTime = audio?.currentTime || 0;
        const wasPlaying = audio && !audio.paused;

        setIsLoading(true);
        updateStatus('Đang lấy URL mới...');

        try {
            const response = await fetchUrlFunction(audioId);

            if (!response.success || !response.data?.FileUrl) {
                throw new Error(response.message || 'Failed to fetch URL');
            }

            const newUrl = response.data.FileUrl;
            setCurrentUrl(newUrl);
            setUrlCreatedAt(Date.now());
            updateStatus('✅ Sẵn sàng');

            // Nếu cần preserve playback, update audio element ngay
            if (preservePlayback && audio && newUrl) {
                audio.src = newUrl;
                audio.currentTime = currentTime;
                
                if (wasPlaying) {
                    setTimeout(() => {
                        audio.play().catch(err => console.error('Error playing audio:', err));
                    }, 100);
                }
            }

            return true;
        } catch (error) {
            const err = error as Error;
            onError?.(err);
            updateStatus(`❌ Lỗi: ${err.message}`);
            return false;
        } finally {
            setIsLoading(false);
        }
    }, [audioId, fetchUrlFunction, onError, updateStatus]);

    // Check URL có hết hạn chưa
    const isUrlExpired = useCallback((): boolean => {
        if (!urlCreatedAt) return true;
        const elapsed = (Date.now() - urlCreatedAt) / 1000;
        return elapsed >= (expiresIn - 1); // Buffer 1s
    }, [urlCreatedAt, expiresIn]);

    // Handle seeking event - chỉ refresh nếu URL hết hạn, không làm gián đoạn playback
    const handleSeeking = useCallback(async () => {
        const audio = audioRef.current;
        if (!audio) return;

        // Chỉ refresh nếu URL thực sự hết hạn
        if (isUrlExpired()) {
            updateStatus('URL hết hạn, đang refresh...');

            const seekTime = audio.currentTime;
            const wasPlaying = !audio.paused;

            const success = await refreshPresignedUrl();

            if (success) {
                // Chờ một chút để đảm bảo currentUrl đã được update
                setTimeout(() => {
                    if (audioRef.current && currentUrl) {
                        audioRef.current.currentTime = seekTime;
                        
                        if (wasPlaying) {
                            audioRef.current.play().catch(err => {
                                console.error('Error playing audio:', err);
                            });
                        }
                    }
                }, 100);
            }
        }
    }, [isUrlExpired, refreshPresignedUrl, currentUrl, updateStatus]);

    const handleError = useCallback(async () => {
        const audio = audioRef.current;
        if (!audio || !audio.error) return;
        
        if (audio.error.code === 4) {
            updateStatus('Lỗi load audio, đang thử lại...');
            await refreshPresignedUrl(true); // Preserve playback state
        }
    }, [refreshPresignedUrl, updateStatus]);

  const handlePlay = useCallback(() => {
  }, [updateStatus]);

  const handlePause = useCallback(() => {
  }, [updateStatus]);

  // Setup event listeners
  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;

    audio.addEventListener('seeking', handleSeeking);
    audio.addEventListener('error', handleError);
    audio.addEventListener('play', handlePlay);
    audio.addEventListener('pause', handlePause);

    return () => {
      audio.removeEventListener('seeking', handleSeeking);
      audio.removeEventListener('error', handleError);
      audio.removeEventListener('play', handlePlay);
      audio.removeEventListener('pause', handlePause);
    };
  }, [handleSeeking, handleError, handlePlay, handlePause]);

  useEffect(() => {
    if (!currentUrl && !isLoading) {
      refreshPresignedUrl();
    }
  }, [currentUrl, isLoading, refreshPresignedUrl]);

  return {
    audioRef,
    currentUrl,
    isLoading,
    status,
    refreshPresignedUrl,
    isUrlExpired,
  };
};