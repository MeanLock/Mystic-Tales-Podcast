import {  loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getAudioEpisode, getEpisodeDetail } from '@/core/services/episode/episode.service';
import { getPublicSource } from '@/core/services/file/file.service';
import Loading from '@/views/components/common/loading';
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Typography, Chip } from '@mui/material';
import Image from '@/views/components/common/image';
import { formatDate } from '@/core/utils/date.util';
import { SmartAudioPlayer } from '@/views/components/common/audio';
import './styles.scss';

const EpisodeDetailView = () => {
    const { id } = useParams<{ id: string }>();
    const [episodeDetail, setEpisodeDetail] = React.useState<any | null>(null);
    const [loading, setLoading] = React.useState<boolean>(false);
    const navigate = useNavigate();
    
    const fetchEpisodeDetail = async () => {
        setLoading(true);
        try {
            const res = await getEpisodeDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched episode detail:", res.data.Episode);
            if (res.success && res.data) {
                setEpisodeDetail(res.data.Episode);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch episode detail:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        if (id) {
            fetchEpisodeDetail();
        }
    }, [id]);

    if (loading || !episodeDetail) {
        return (
            <div className="episode-detail-loading">
                <Loading />
            </div>
        );
    }

    return (
        <div className="episode-detail">
            {/* Header Section */}
            <div className="episode-detail__header">
                <div className="episode-detail__header-content">
                    <Image
                        mainImageFileKey={episodeDetail.MainImageFileKey}
                        alt={episodeDetail.Name}
                        className="episode-detail__cover-image"
                    />
                    <div className="episode-detail__header-info">
                        <Typography variant="h3" className="episode-detail__title">
                            {episodeDetail.Name}
                        </Typography>
                        <div className="episode-detail__meta">
                            <span className="episode-detail__meta-item">
                                <strong>Episode:</strong> #{episodeDetail.EpisodeOrder}
                            </span>
                            {episodeDetail.SeasonNumber > 0 && (
                                <span className="episode-detail__meta-item">
                                    <strong>Season:</strong> {episodeDetail.SeasonNumber}
                                </span>
                            )}
                            <span className="episode-detail__meta-item">
                                <strong>Status:</strong>
                                <Chip
                                    label={episodeDetail.CurrentStatus.Name}
                                    size="small"
                                    className="episode-detail__status-chip"
                                />
                            </span>
                            <span className="episode-detail__meta-item">
                                <strong>Type:</strong> {episodeDetail.PodcastEpisodeSubscriptionType.Name}
                            </span>
                        </div>
                        <div className="episode-detail__stats">
                            <div className="episode-detail__stat">
                                <span className="episode-detail__stat-value">{episodeDetail.ListenCount.toLocaleString()}</span>
                                <span className="episode-detail__stat-label">Listens</span>
                            </div>
                            <div className="episode-detail__stat">
                                <span className="episode-detail__stat-value">{episodeDetail.TotalSave.toLocaleString()}</span>
                                <span className="episode-detail__stat-label">Saves</span>
                            </div>
                            <div className="episode-detail__stat">
                                <span className="episode-detail__stat-value">
                                    {episodeDetail.AudioLength ? `${Math.floor(episodeDetail.AudioLength / 60)}:${String(episodeDetail.AudioLength % 60).padStart(2, '0')}` : 'N/A'}
                                </span>
                                <span className="episode-detail__stat-label">Duration</span>
                            </div>
                            <div className="episode-detail__stat">
                                <span className="episode-detail__stat-value">{episodeDetail.IsReleased ? 'Yes' : 'No'}</span>
                                <span className="episode-detail__stat-label">Released</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Taken Down Reason Alert */}
            {episodeDetail.TakenDownReason && (
                <div className="episode-detail__alert">
                    <svg className="episode-detail__alert-icon" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <div>
                        <strong>Taken Down Reason:</strong>
                        <p>{episodeDetail.TakenDownReason}</p>
                    </div>
                </div>
            )}

            {/* Main Content Grid */}
            <div className="episode-detail__content">
                {/* Left Column */}
                <div className="episode-detail__main">
                    {/* Description Section */}
                    <div className="episode-detail__section">
                        <Typography variant="h5" className="episode-detail__section-title">
                            About This Episode
                        </Typography>
                        <div
                            className="episode-detail__description"
                            dangerouslySetInnerHTML={{ __html: episodeDetail.Description }}
                        />
                    </div>

                    {/* Podcaster and Show Section - Side by Side */}
                    <div className="episode-detail__people-section">
                        {/* Podcaster Section */}
                        <div className="episode-detail__section episode-detail__section--half">
                            <Typography variant="h5" className="episode-detail__section-title">
                                Podcaster
                            </Typography>
                            <div className="episode-detail__podcaster">
                                <Image
                                    mainImageFileKey={episodeDetail.Podcaster.MainImageFileKey}
                                    alt={episodeDetail.Podcaster.FullName}
                                    className="episode-detail__podcaster-avatar"
                                />
                                <div className="episode-detail__podcaster-info">
                                    <h4>{episodeDetail.Podcaster.FullName}</h4>
                                    <p>{episodeDetail.Podcaster.Email}</p>
                                </div>
                            </div>
                        </div>

                        {/* Show Section */}
                        <div 
                            className="episode-detail__section episode-detail__section--half episode-detail__section--clickable"
                            onClick={() => navigate(`/show/${episodeDetail.PodcastShow.Id}`)}
                        >
                            <Typography variant="h5" className="episode-detail__section-title">
                                Show
                            </Typography>
                            <div className="episode-detail__show">
                                <Image
                                    mainImageFileKey={episodeDetail.PodcastShow.MainImageFileKey}
                                    alt={episodeDetail.PodcastShow.Name}
                                    className="episode-detail__show-image"
                                />
                                <div className="episode-detail__show-info">
                                    <h4>{episodeDetail.PodcastShow.Name}</h4>
                                    <p>{episodeDetail.PodcastShow.IsReleased ? 'Released' : 'Not Released'}</p>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Hashtags Section */}
                    {episodeDetail.Hashtags && episodeDetail.Hashtags.length > 0 && (
                        <div className="episode-detail__section">
                            <Typography variant="h5" className="episode-detail__section-title">
                                Hashtags
                            </Typography>
                            <div className="episode-detail__hashtags">
                                {episodeDetail.Hashtags.map((tag: any) => (
                                    <Chip
                                        key={tag.Id}
                                        label={`#${tag.Name}`}
                                        className="episode-detail__hashtag"
                                    />
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Audio Player Section */}
                    {episodeDetail.AudioFileKey && (
                        <div className="episode-detail__section">
                            <Typography variant="h5" className="episode-detail__section-title">
                                Audio 
                            </Typography>
                            <SmartAudioPlayer
                                audioId={episodeDetail.AudioFileKey}
                                className="w-full mt-4"
                                fetchUrlFunction={async (fileKey) => {
                                    const result = await getAudioEpisode(loginRequiredAxiosInstance, fileKey);
                                    return {
                                        success: result.success,
                                        data: result.data ? { FileUrl: result.data.FileUrl } : undefined,
                                        message: typeof result.message === 'string' ? result.message : result.message?.content
                                    };
                                }}
                            />
                        </div>
                    )}
                </div>

                {/* Right Sidebar */}
                <div className="episode-detail__sidebar">
                    {/* Episode Information */}
                    <div className="episode-detail__info-card">
                        <h4 className="episode-detail__info-title">Episode Information</h4>
                        <div className="episode-detail__info-list">
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Episode Order</span>
                                <span className="episode-detail__info-value">#{episodeDetail.EpisodeOrder}</span>
                            </div>
                            {episodeDetail.SeasonNumber > 0 && (
                                <div className="episode-detail__info-item">
                                    <span className="episode-detail__info-label">Season Number</span>
                                    <span className="episode-detail__info-value">{episodeDetail.SeasonNumber}</span>
                                </div>
                            )}
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Subscription Type</span>
                                <span className="episode-detail__info-value">{episodeDetail.PodcastEpisodeSubscriptionType.Name}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Explicit Content</span>
                                <span className="episode-detail__info-value">{episodeDetail.ExplicitContent ? 'Yes' : 'No'}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Release Date</span>
                                <span className="episode-detail__info-value">{formatDate(episodeDetail.ReleaseDate)}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Is Released</span>
                                <span className="episode-detail__info-value">{episodeDetail.IsReleased ? 'Yes' : 'No'}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Audio Publishable</span>
                                <span className="episode-detail__info-value">{episodeDetail.IsAudioPublishable ? 'Yes' : 'No'}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Audio Size</span>
                                <span className="episode-detail__info-value">{(episodeDetail.AudioFileSize / 1024 / 1024).toFixed(2)} MB</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Created At</span>
                                <span className="episode-detail__info-value">{formatDate(episodeDetail.CreatedAt)}</span>
                            </div>
                            <div className="episode-detail__info-item">
                                <span className="episode-detail__info-label">Updated At</span>
                                <span className="episode-detail__info-value">{formatDate(episodeDetail.UpdatedAt)}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default EpisodeDetailView;