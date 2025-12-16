import { fetchImage, handleImgError } from '@/core/utils/image.util';
import { Favorite, PersonAdd, PlayArrow } from '@mui/icons-material';
import { Card, CardContent, CardMedia, Chip, Skeleton, Typography } from '@mui/material';
import { FC, useEffect, useState } from 'react';
import notfound from "../../../../assets/notfound.png"
const ShowCard: FC<{ show: any }> = ({ show }) => {
    const [imageUrl, setImageUrl] = useState<string>("");
    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const url = await fetchImage(show.MainImageFileKey);
                if (alive) setImageUrl(url || notfound);
            } catch {
                if (alive) setImageUrl(notfound);
            }
        })();
        return () => { alive = false; };
    }, [show.MainImageFileKey]);

    return (
        
        <Card
            key={show.Id}
            className="my-show-page__show-card"
            onClick={() => {
                window.open(`/show/${show.Id}/overview`);
            }}
        >
            <div className="my-show-page__show-image-container relative">
                <CardMedia
                    component="img"
                    image={imageUrl}
                    alt={show.Name}
                    className="my-show-page__show-image"
                    onError={handleImgError}

                />

                {/* Overlay Stats */}
                <div className="my-show-page__show-stats absolute top-2 left-2 flex flex-col gap-1">
                    <div className="my-show-page__show-stat text-xs">
                        <PlayArrow />
                        {show.EpisodeCount} Episodes
                    </div>
                    <div className="my-show-page__show-stat-follow text-xs">
                        <PersonAdd />
                        {show.TotalFollow.toLocaleString()}
                    </div>
                </div>
            </div>

            <CardContent className="my-show-page__show-info">
                <Typography
                    variant="h6"
                    className="my-show-page__show-title"
                >
                    {show.Name}
                </Typography>

                <div className="my-show-page__show-status">
                    <Chip
                        label={show.CurrentStatus.Name}
                        size="small"
                        className={`my-show-page__status-chip my-show-page__status-chip--${show.CurrentStatus.Name.toLowerCase().replace(/\s+/g, '-')}`}
                    />
                </div>
            </CardContent>
        </Card>
    );
};

export default ShowCard;