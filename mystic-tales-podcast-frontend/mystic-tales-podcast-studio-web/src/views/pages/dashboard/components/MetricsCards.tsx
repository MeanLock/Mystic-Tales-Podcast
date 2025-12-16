import { FC, useEffect, useMemo, useState } from "react";
import { Card, CardContent, Typography } from "@mui/material";
import { getChannelList } from "@/core/services/channel/channel.service";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { getShowList } from "@/core/services/show/show.service";

type ChannelItem = {
    Id: string;
    Name: string;
    TotalFavorite: number;
    ListenCount: number;
    ShowCount: number;
    CurrentStatus?: { Id: number; Name: string } | null;
};

type ShowItem = {
    Id: string;
    Name: string;
    TotalFollow: number;
    ListenCount: number;
    EpisodeCount: number;
    RatingCount: number;
    AverageRating: number;
    CurrentStatus?: { Id: number; Name: string } | null;
};

interface MetricsCardsProps {

}

const fmt = (n: number) => n.toLocaleString("vi-VN");

const glassCardSx = {
    background: "rgba(255,255,255,0.06)",
    border: "1px solid rgba(255,255,255,0.12)",
    backdropFilter: "blur(12px)",
    borderRadius: "16px",
    boxShadow: "0 10px 30px rgba(0,0,0,0.35)",
};

const MetricsCards: FC<MetricsCardsProps> = () => {
    const [channels, setChannels] = useState<ChannelItem[]>([]);
    const [shows, setShows] = useState<ShowItem[]>([]);
    const [isLoading, setIsLoading] = useState<boolean>(false);


    useEffect(() => {
        const fetchData = async () => {
            try {
                setIsLoading(true)
                const [showRes, channelRes] = await Promise.all([
                    getShowList(loginRequiredAxiosInstance),
                    getChannelList(loginRequiredAxiosInstance),
                ]);
                setShows(showRes.success ? showRes.data.ShowList : []);
                setChannels(channelRes.success ? channelRes.data.ChannelList : []);
            } catch (err) {
                console.error("Failed to fetch revenue data:", err)
            } finally {
                setIsLoading(false)
            }
        }

        fetchData()
    }, [])

    const stats = useMemo(() => {
        const totalChannels = channels.length;
        const totalShows = shows.length;
        const totalChannelListens = channels.reduce((s, c) => s + (c.ListenCount || 0), 0);
        const totalShowListens = shows.reduce((s, sh) => s + (sh.ListenCount || 0), 0);
        const totalListens = totalChannelListens + totalShowListens;

        const totalFavorites = channels.reduce((s, c) => s + (c.TotalFavorite || 0), 0);
        const totalFollowers = shows.reduce((s, sh) => s + (sh.TotalFollow || 0), 0);
        const totalEpisodes = shows.reduce((s, sh) => s + (sh.EpisodeCount || 0), 0);

        // Rating TB (có trọng số theo số lượt đánh giá)
        const ratingWeightSum = shows.reduce((s, sh) => s + (sh.RatingCount || 0), 0);
        const ratingWeighted =
            ratingWeightSum > 0
                ? shows.reduce((s, sh) => s + (sh.AverageRating || 0) * (sh.RatingCount || 0), 0) / ratingWeightSum
                : 0;

        return [
            { title: "Total channels", value: fmt(totalChannels) },
            { title: "Total shows", value: fmt(totalShows) },
            { title: "Total listens", value: fmt(totalListens) },
            { title: "Total favorites (channels)", value: fmt(totalFavorites) },
            { title: "Total followers (shows)", value: fmt(totalFollowers) },
            { title: "Total episodes", value: fmt(totalEpisodes) },
            { title: "Average rating (shows)", value: ratingWeighted ? `${ratingWeighted.toFixed(2)}/5 ⭐` : "N/A" },
        ];
    }, [channels, shows]);

    return (
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
            {stats.map((m, idx) => (
                <div key={idx}>
                    <Card sx={glassCardSx} className="dashboard-card">
                        <CardContent>
                            <Typography variant="caption" sx={{ color: "#bdbdbd", letterSpacing: 0.3 }}>
                                {m.title}
                            </Typography>
                            <Typography variant="h6" sx={{ mt: 1.5, fontWeight: 800, color: "#AEE339" }}>
                                {m.value}
                            </Typography>
                        </CardContent>
                    </Card>
                </div>
            ))}
        </div>
    );
};

export default MetricsCards;