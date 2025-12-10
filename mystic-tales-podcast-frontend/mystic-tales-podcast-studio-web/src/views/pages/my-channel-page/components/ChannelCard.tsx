import { fetchImage, handleImgError } from '@/core/utils/image.util';
import { Favorite, PlayArrow } from '@mui/icons-material';
import { Card, CardContent, CardMedia, Chip, Skeleton, Typography } from '@mui/material';
import { FC, useEffect, useState } from 'react';
import notfound from "../../../../assets/notfound.png"
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

    return (
        <Card
            className="my-channel-page__channel-card"
            onClick={() => window.open(`/channel/${channel.Id}/overview`)}
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
        </Card>
    );
};

export default ChannelCard;