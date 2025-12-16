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
    const retryCountRef = useRef<number>(0);
    const maxRetries = 3;
    const lastErrorTimeRef = useRef<number>(0);
    const isRetryingRef = useRef<boolean>(false);
    const hasStoppedRetryingRef = useRef<boolean>(false);

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
            
            // Reset retry count on success
            retryCountRef.current = 0;
            isRetryingRef.current = false;
            hasStoppedRetryingRef.current = false;

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
            
            // If it's a 403 error, stop retrying immediately
            if (err.message.includes('403')) {
                console.error('403 error received, stopping retries');
                retryCountRef.current = maxRetries;
                isRetryingRef.current = false;
                hasStoppedRetryingRef.current = true;
                if (audio) {
                    audio.src = '';
                    audio.removeAttribute('src');
                }
            }
            
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
        
        // If we've already stopped retrying, don't process any more errors
        if (hasStoppedRetryingRef.current) {
            return;
        }
        
        // Prevent concurrent retry attempts
        if (isRetryingRef.current) {
            console.log('Already retrying, skipping...');
            return;
        }
        
        // Prevent rapid retry loops - wait at least 2 seconds between retries
        const now = Date.now();
        if (now - lastErrorTimeRef.current < 2000) {
            console.log('Skipping retry - too soon after last error');
            return;
        }
        lastErrorTimeRef.current = now;
        
        if (audio.error.code === 4) {
            // Check if we've exceeded max retries
            if (retryCountRef.current >= maxRetries) {
                hasStoppedRetryingRef.current = true;
                updateStatus(`❌ Đã thử ${maxRetries} lần nhưng không thể tải audio. Vui lòng kiểm tra lại file.`);
                onError?.(new Error(`Failed to load audio after ${maxRetries} attempts`));
                // Clear audio src to prevent continuous error events
                audio.src = '';
                audio.removeAttribute('src');
                return;
            }
            
            retryCountRef.current += 1;
            isRetryingRef.current = true;
            updateStatus(`⚠️ Lỗi load audio, đang thử lại (${retryCountRef.current}/${maxRetries})...`);
            
            const success = await refreshPresignedUrl(true);
            
            if (!success) {
                // If refresh failed, show error
                isRetryingRef.current = false;
                updateStatus(`❌ Không thể tải URL mới (lần ${retryCountRef.current}/${maxRetries})`);
            }
        } else {
            // Not error code 4, stop retrying
            isRetryingRef.current = false;
        }
    }, [refreshPresignedUrl, updateStatus, onError, maxRetries]);

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