import React, { useState, useRef, useEffect } from 'react';
import { Play, Pause, Volume2, Download, SkipBack, SkipForward } from 'lucide-react';

interface AudioPreviewProps {
  audioUrl: string;
  fileName: string;
  fileExtension: 'wav' | 'flac' | 'mp3' | 'm4a' | 'aac' | any;
  onDownload?: () => void;
  autoPlay?: boolean;
  showDownload?: boolean;
}

interface AudioMetadata {
  duration: number;
  title?: string;
  artist?: string;
  album?: string;
}

const AudioPreview: React.FC<AudioPreviewProps> = ({
  audioUrl,
  fileName,
  fileExtension,
  onDownload,
  autoPlay = false,
  showDownload = true
}) => {
  const audioRef = useRef<HTMLAudioElement>(null);
  const [isPlaying, setIsPlaying] = useState(false);
  const [currentTime, setCurrentTime] = useState(0);
  const [duration, setDuration] = useState(0);
  const [volume, setVolume] = useState(1);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [metadata, setMetadata] = useState<AudioMetadata | null>(null);

  // Format time helper
  const formatTime = (time: number): string => {
    if (isNaN(time)) return '0:00';
    const minutes = Math.floor(time / 60);
    const seconds = Math.floor(time % 60);
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
  };

  // Get file type icon
  const getFileIcon = (ext: string) => {
    const iconMap = {
      wav: '🎵',
      flac: '🎼',
      mp3: '🎧',
      m4a: '🎶',
      aac: '🔊'
    };
    return iconMap[ext as keyof typeof iconMap] || '🎵';
  };

  // Audio event handlers
  useEffect(() => {
    const audio = audioRef.current;
    if (!audio) return;

    const handleLoadStart = () => setIsLoading(true);
    const handleCanPlay = () => setIsLoading(false);
    const handleLoadedMetadata = () => {
      setDuration(audio.duration);
      setMetadata({
        duration: audio.duration,
        title: fileName,
        // Note: Web Audio API doesn't provide metadata directly
        // You might need a library like music-metadata-browser for full metadata
      });
    };
    
    const handleTimeUpdate = () => setCurrentTime(audio.currentTime);
    const handleEnded = () => setIsPlaying(false);
    const handleError = () => {
      setError(`Cannot load ${fileExtension.toUpperCase()} file`);
      setIsLoading(false);
    };

    audio.addEventListener('loadstart', handleLoadStart);
    audio.addEventListener('canplay', handleCanPlay);
    audio.addEventListener('loadedmetadata', handleLoadedMetadata);
    audio.addEventListener('timeupdate', handleTimeUpdate);
    audio.addEventListener('ended', handleEnded);
    audio.addEventListener('error', handleError);

    return () => {
      audio.removeEventListener('loadstart', handleLoadStart);
      audio.removeEventListener('canplay', handleCanPlay);
      audio.removeEventListener('loadedmetadata', handleLoadedMetadata);
      audio.removeEventListener('timeupdate', handleTimeUpdate);
      audio.removeEventListener('ended', handleEnded);
      audio.removeEventListener('error', handleError);
    };
  }, [audioUrl, fileExtension, fileName]);

  // Play/Pause toggle
  const togglePlayPause = async () => {
    const audio = audioRef.current;
    if (!audio) return;

    try {
      if (isPlaying) {
        audio.pause();
        setIsPlaying(false);
      } else {
        await audio.play();
        setIsPlaying(true);
      }
    } catch (err) {
      setError('Playback failed. This format might not be supported.');
    }
  };

  // Seek functionality
  const handleSeek = (e: React.MouseEvent<HTMLDivElement>) => {
    const audio = audioRef.current;
    if (!audio || !duration) return;

    const rect = e.currentTarget.getBoundingClientRect();
    const percent = (e.clientX - rect.left) / rect.width;
    const newTime = percent * duration;
    
    audio.currentTime = newTime;
    setCurrentTime(newTime);
  };

  // Volume control
  const handleVolumeChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newVolume = parseFloat(e.target.value);
    setVolume(newVolume);
    if (audioRef.current) {
      audioRef.current.volume = newVolume;
    }
  };

  // Skip functions
  const skipBackward = () => {
    const audio = audioRef.current;
    if (audio) {
      audio.currentTime = Math.max(0, audio.currentTime - 10);
    }
  };

  const skipForward = () => {
    const audio = audioRef.current;
    if (audio) {
      audio.currentTime = Math.min(duration, audio.currentTime + 10);
    }
  };

  // Download handler
  const handleDownload = () => {
    if (onDownload) {
      onDownload();
    } else {
      // Default download behavior
      const link = document.createElement('a');
      link.href = audioUrl;
      link.download = fileName;
      link.click();
    }
  };

  if (error) {
    return (
      <div className="bg-red-50 border border-red-200 rounded-lg p-4">
        <div className="flex items-center space-x-3">
          <div className="text-2xl">⚠️</div>
          <div>
            <div className="text-red-800 font-medium">Audio Preview Error</div>
            <div className="text-red-600 text-sm">{error}</div>
          </div>
          {showDownload && (
            <button
              onClick={handleDownload}
              className="ml-auto bg-red-600 text-white px-3 py-1 rounded text-sm hover:bg-red-700 flex items-center space-x-1"
            >
              <Download size={14} />
              <span>Download</span>
            </button>
          )}
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-200 rounded-lg shadow-sm p-4 max-w-md mx-auto">
      {/* Hidden audio element */}
      <audio
        ref={audioRef}
        src={audioUrl}
        preload="metadata"
        autoPlay={autoPlay}
      />

      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <div className="flex items-center space-x-3">
          <div className="text-2xl">{getFileIcon(fileExtension)}</div>
          <div>
            <div className="font-medium text-gray-900 truncate max-w-48">
              {metadata?.title || fileName}
            </div>
            <div className="text-sm text-gray-500 uppercase">
              {fileExtension} • {formatTime(duration)}
            </div>
          </div>
        </div>
        
        {showDownload && (
          <button
            onClick={handleDownload}
            className="text-gray-500 hover:text-gray-700 p-1"
            title="Download"
          >
            <Download size={20} />
          </button>
        )}
      </div>

      {/* Controls */}
      <div className="space-y-4">
        {/* Progress Bar */}
        <div className="space-y-2">
          <div 
            className="bg-gray-200 rounded-full h-2 cursor-pointer"
            onClick={handleSeek}
          >
            <div 
              className="bg-blue-500 h-2 rounded-full transition-all duration-100"
              style={{ width: duration ? `${(currentTime / duration) * 100}%` : '0%' }}
            />
          </div>
          
          <div className="flex justify-between text-xs text-gray-500">
            <span>{formatTime(currentTime)}</span>
            <span>{formatTime(duration)}</span>
          </div>
        </div>

        {/* Play Controls */}
        <div className="flex items-center justify-center space-x-4">
          <button
            onClick={skipBackward}
            className="text-gray-600 hover:text-gray-800"
            title="Skip back 10s"
          >
            <SkipBack size={20} />
          </button>
          
          <button
            onClick={togglePlayPause}
            disabled={isLoading}
            className="bg-blue-500 hover:bg-blue-600 disabled:bg-gray-300 text-white rounded-full p-3 transition-colors"
            title={isPlaying ? 'Pause' : 'Play'}
          >
            {isLoading ? (
              <div className="animate-spin rounded-full h-5 w-5 border-2 border-white border-t-transparent" />
            ) : isPlaying ? (
              <Pause size={20} />
            ) : (
              <Play size={20} className="ml-0.5" />
            )}
          </button>
          
          <button
            onClick={skipForward}
            className="text-gray-600 hover:text-gray-800"
            title="Skip forward 10s"
          >
            <SkipForward size={20} />
          </button>
        </div>

        {/* Volume Control */}
        <div className="flex items-center space-x-2">
          <Volume2 size={16} className="text-gray-500" />
          <input
            type="range"
            min="0"
            max="1"
            step="0.1"
            value={volume}
            onChange={handleVolumeChange}
            className="flex-1 h-1 bg-gray-200 rounded-lg appearance-none cursor-pointer slider"
          />
          <span className="text-xs text-gray-500 w-8">
            {Math.round(volume * 100)}%
          </span>
        </div>
      </div>

      {/* Format Support Info */}
      <div className="mt-3 pt-3 border-t border-gray-100">
        <div className="text-xs text-gray-500 text-center">
          {fileExtension.toLowerCase() === 'flac' && 'High Quality Lossless Audio'}
          {fileExtension.toLowerCase() === 'wav' && 'Uncompressed Audio'}
          {fileExtension.toLowerCase() === 'mp3' && 'Compressed Audio'}
          {fileExtension.toLowerCase() === 'm4a' && 'AAC Audio'}
          {fileExtension.toLowerCase() === 'aac' && 'Advanced Audio Codec'}
        </div>
      </div>

      {/* Browser Support Warning */}
      {(fileExtension === 'flac' || fileExtension === 'wav') && (
        <div className="mt-2 p-2 bg-yellow-50 border border-yellow-200 rounded text-xs text-yellow-800">
          ⚠️ {fileExtension.toUpperCase()} support varies by browser. Download if playback fails.
        </div>
      )}
    </div>
  );
};


export default AudioPreview;