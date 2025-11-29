import { Search } from '@mui/icons-material';
import { Button, Typography } from '@mui/material';
import React from 'react';
import './styles.scss';

interface EmptyProps {
    item?: string;
    subtitle?: string;
}

export const EmptyComponent: React.FC<EmptyProps> = ({ item, subtitle }) => {
    return (
        <div className="empty-state">
            <Search className="empty-state-icon" />
            {item &&
                <Typography variant="h5" className="empty-state-title">
                No {item} found
            </Typography>
            }
            {subtitle && (
                <Typography className="empty-state-description">
                    {subtitle}
                </Typography>
            )}

        </div>
    );
};
