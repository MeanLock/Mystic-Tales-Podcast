import { Chip } from '@mui/material';
import { FC } from 'react';

interface StatusChipProps {
    statusId: number;
    statusName: string;
    type: 'show' | 'episode';
}

const STATUS_COLORS = {
    show: {
        1: { // Draft
            bg: 'rgba(158, 158, 158, 0.2)',
            border: '#9e9e9e',
            color: '#9e9e9e'
        },
        2: { // Ready To Release
            bg: 'rgba(33, 150, 243, 0.2)',
            border: '#2196f3',
            color: '#2196f3'
        },
        3: { // Published
            bg: 'rgba(174, 227, 57, 0.2)',
            border: 'var(--primary-green)',
            color: 'var(--primary-green)'
        },
        4: { // Taken Down
            bg: 'rgba(244, 67, 54, 0.2)',
            border: '#f44336',
            color: '#f44336'
        },
        5: { // Removed
            bg: 'rgba(244, 67, 54, 0.2)',
            border: '#f44336',
            color: '#f44336'
        }
    },
    episode: {
        1: { // Draft
            bg: 'rgba(158, 158, 158, 0.2)',
            border: '#9e9e9e',
            color: '#9e9e9e'
        },
        2: { // Pending Review
            bg: 'rgba(255, 152, 0, 0.2)',
            border: '#ff9800',
            color: '#ff9800'
        },
        3: { // Pending Edit Required
            bg: 'rgba(255, 193, 7, 0.2)',
            border: '#ffc107',
            color: '#ffc107'
        },
        4: { // Ready To Release
            bg: 'rgba(33, 150, 243, 0.2)',
            border: '#2196f3',
            color: '#2196f3'
        },
        5: { // Published
          bg: 'rgba(174, 227, 57, 0.2)',
            border: 'var(--primary-green)',
            color: 'var(--primary-green)'
        },
        6: { // Taken Down
            bg: 'rgba(244, 67, 54, 0.2)',
            border: '#f44336',
            color: '#f44336'
        },
        7: { // Removed
          bg: 'rgba(244, 67, 54, 0.2)',
            border: '#f44336',
            color: '#f44336'
        },
        8: { // Audio Processing
          bg: 'rgba(33, 150, 243, 0.2)',
            border: '#2196f3',
            color: '#2196f3'
        }
    }
};

const StatusChip: FC<StatusChipProps> = ({ statusId, statusName, type }) => {
    const style = STATUS_COLORS[type][statusId] || {
        bg: 'rgba(158, 158, 158, 0.2)',
        border: '#9e9e9e',
        color: '#9e9e9e'
    };

    const isAudioProcessing = type === 'episode' && statusId === 8;

    return (
        <>
            <Chip
                label={
                    isAudioProcessing ? (
                        <span>
                            {statusName}
                            <span style={{ 
                                animation: 'blink 1.5s infinite',
                                marginLeft: '2px'
                            }}>.</span>
                            <span style={{ 
                                animation: 'blink 1.5s infinite',
                                animationDelay: '0.3s',
                                marginLeft: '2px'
                            }}>.</span>
                            <span style={{ 
                                animation: 'blink 1.5s infinite',
                                animationDelay: '0.6s',
                                marginLeft: '2px'
                            }}>.</span>
                        </span>
                    ) : statusName
                }
                sx={{
                    background: style.bg,
                    backdropFilter: 'blur(10px)',
                    WebkitBackdropFilter: 'blur(10px)',
                    minWidth: 150,
                    padding: '0 10px',
                    borderRadius: 50,
                    fontWeight: 700,
                    fontSize: '0.9rem',
                    border: `1.5px solid ${style.border}`,
                    color: style.color,
                    height: '44px',
                    '& .MuiChip-label': {
                        padding: '0 12px',
                        textShadow: '0 2px 6px rgba(0, 0, 0, 0.3)',
                    },
                }}
            />
            {isAudioProcessing && (
                <style>
                    {`
                        @keyframes blink {
                            0%, 100% {
                                opacity: 0;
                            }
                            50% {
                                opacity: 1;
                            }
                        }
                    `}
                </style>
            )}
        </>
    );
};

export default StatusChip;