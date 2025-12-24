import { fetchImage, handleImgError } from '@/core/utils/image.util';
import { Favorite, PlayArrow } from '@mui/icons-material';
import { Card, CardContent, CardMedia, Chip, Skeleton, Typography } from '@mui/material';
import { FC, useEffect, useState } from 'react';
import notfound from "../../../../assets/notfound.png"
import { Link } from 'react-router-dom';
const ChannelCard: FC<{ channel: any }> = ({ channel }) => {
    const [imageUrl, setImageUrl] = useState<string>("");

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const url = await fetchImage(channel.MainImageFileKey);
                if (alive) setImageUrl(url || notfound);
            } catch {
                if (alive) setImageUrl(notfound);
            }
        })();
        return () => { alive = false; };
    }, [channel.MainImageFileKey]);
    const handleCardClick = () => {
        console.log("Channel clicked:", channel.Id);
        window.open(`/channel/${channel.Id}/overview`, "_blank");
    };
    return (
        <Link
            className="my-channel-page__channel-card"
            to={`/channel/${channel.Id}/overview`}
            target="_blank"
            rel="noopener noreferrer"
        >
            <div className="my-channel-page__channel-image-container relative">
                {imageUrl ? (
                    <CardMedia
                        component="img"
                        height="200"
                        image={imageUrl}
                        onError={handleImgError}
                        className="my-channel-page__channel-image"
                    />
                ) : (
                    <Skeleton variant="rectangular" height={200} />
                )}

                <div className="my-channel-page__channel-stats absolute top-2 left-2 flex flex-col gap-1">
                    <div className="my-channel-page__channel-stat text-xs ">
                        <PlayArrow /> {channel.TotalShow} Shows
                    </div>
                    <div className="my-channel-page__channel-stat text-xs ">
                        <Favorite /> {channel.TotalFavorite.toLocaleString()}
                    </div>
                </div>
            </div>

            <CardContent className="my-channel-page__channel-info ">
                <Typography variant="h6" className="my-channel-page__channel-title ">
                    {channel.Name}
                </Typography>
                <div className="my-channel-page__channel-status">
                    <Chip
                        label={channel.CurrentStatus.Name}
                        size="small"
                        className={`my-channel-page__status-chip my-channel-page__status-chip--${channel.CurrentStatus.Name.toLowerCase()}`}
                    />
                </div>
            </CardContent>
        </Link>
    );
};

export default ChannelCard;