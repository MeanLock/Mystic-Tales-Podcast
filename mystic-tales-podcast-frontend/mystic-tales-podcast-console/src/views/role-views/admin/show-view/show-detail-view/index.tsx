import { adminAxiosInstance, loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getShowDetail } from '@/core/services/show/show.service';
import { ShowDetail } from '@/core/types/show';
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Typography, Chip } from '@mui/material';
import Image from '@/views/components/common/image';
import Loading from '@/views/components/common/loading';
import { formatDate, getTimeAgo } from '@/core/utils/date.util';
import './styles.scss';
import { SmartAudioPlayer } from '@/views/components/common/audio';
import { getPublicSource } from '@/core/services/file/file.service';
import { renderDescriptionHTML } from '@/core/utils/htmlRender.utils';


const ShowDetailView = () => {
    const { id } = useParams<{ id: string }>();
    const [showDetail, setShowDetail] = React.useState<ShowDetail | null>(null);
    const [loading, setLoading] = React.useState<boolean>(false);

    const navigate = useNavigate();
    const fetchShowDetail = async () => {
        setLoading(true);
        try {
            const res = await getShowDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched show detail:", res.data.Show);
            if (res.success && res.data) {
                setShowDetail(res.data.Show);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show detail:', error);
        } finally {
            setLoading(false);
        }
    }
    const toStatusClass = (name?: string) => {
        if (!name) return 'unknown'
        const norm = name.trim().toLowerCase().replace(/\s+/g, '-') // LƯU Ý: /\s+/g
        // alias cho các biến thể API
        if (norm === 'audioprocessing') return 'audio-processing'
        return norm
    }

    useEffect(() => {
        if (id) {
            fetchShowDetail();
        }
    }, [id]);

    if (loading || !showDetail) {
        return (
            <div className="show-detail-loading">
                <Loading />
            </div>
        );
    }

    return (
        <div className="show-detail">
            {/* Header Section */}
            <div className="show-detail__header">
                <div className="show-detail__header-content">
                    <Image
                        mainImageFileKey={showDetail.MainImageFileKey}
                        alt={showDetail.Name}
                        className="show-detail__cover-image"
                    />
                    <div className="show-detail__header-info">
                        <Typography variant="h3" className="show-detail__title">
                            {showDetail.Name}
                        </Typography>
                        <div className="show-detail__meta">
                            <span className="show-detail__meta-item">
                                <strong>Category:</strong> {showDetail.PodcastCategory.Name}
                            </span>
                            <span className="show-detail__meta-item">
                                <strong>Language:</strong> {showDetail.Language}
                            </span>
                            <span className="show-detail__meta-item">
                                <strong>Status:</strong>
                                <Chip
                                    label={showDetail.CurrentStatus.Name}
                                    size="small"
                                    className="show-detail__status-chip"
                                />
                            </span>
                        </div>
                        <div className="show-detail__stats">
                            <div className="show-detail__stat">
                                <span className="show-detail__stat-value">{showDetail.TotalFollow.toLocaleString()}</span>
                                <span className="show-detail__stat-label">Followers</span>
                            </div>
                            <div className="show-detail__stat">
                                <span className="show-detail__stat-value">{showDetail.ListenCount.toLocaleString()}</span>
                                <span className="show-detail__stat-label">Listens</span>
                            </div>
                            <div className="show-detail__stat">
                                <span className="show-detail__stat-value">{showDetail.AverageRating.toFixed(1)} ⭐</span>
                                <span className="show-detail__stat-label">Rating</span>
                            </div>
                            <div className="show-detail__stat">
                                <span className="show-detail__stat-value">{showDetail.EpisodeCount}</span>
                                <span className="show-detail__stat-label">Episodes</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Taken Down Reason Alert */}
            {showDetail.TakenDownReason && (
                <div className="show-detail__alert">
                    <svg className="show-detail__alert-icon" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <div>
                        <strong>Taken Down Reason:</strong>
                        <p>{showDetail.TakenDownReason}</p>
                    </div>
                </div>
            )}

            {/* Main Content Grid */}
            <div className="show-detail__content">
                {/* Left Column */}
                <div className="show-detail__main">
                    {/* Description Section */}
                    <div className="show-detail__section">
                        <Typography variant="h5" className="show-detail__section-title">
                            About This Show
                        </Typography>
                        <div
                            className="show-detail__description"
                            dangerouslySetInnerHTML={{ __html: showDetail.Description }}
                        />
                    </div>

                    {/* Podcaster and Channel Section - Side by Side */}
                    <div className="show-detail__people-section">
                        {/* Podcaster Section */}
                        <div className="show-detail__section show-detail__section--half">
                            <Typography variant="h5" className="show-detail__section-title">
                                Podcaster
                            </Typography>
                            <div className="show-detail__podcaster">
                                <Image
                                    mainImageFileKey={showDetail.Podcaster.MainImageFileKey}
                                    alt={showDetail.Podcaster.FullName}
                                    className="show-detail__podcaster-avatar"
                                />
                                <div className="show-detail__podcaster-info">
                                    <h4>{showDetail.Podcaster.FullName}</h4>
                                    <p>{showDetail.Podcaster.Email}</p>
                                </div>
                            </div>
                        </div>

                        {/* Channel Section */}
                        <div className="show-detail__section show-detail__section--half">
                            <Typography variant="h5" className="show-detail__section-title">
                                Channel
                            </Typography>
                            {showDetail.PodcastChannel ? (
                                <div className="show-detail__channel">
                                    <Image
                                        mainImageFileKey={showDetail.PodcastChannel.MainImageFileKey}
                                        alt={showDetail.PodcastChannel.Name}
                                        className="show-detail__channel-image"
                                    />
                                    <div className="show-detail__channel-info">
                                        <h4>{showDetail.PodcastChannel.Name}</h4>
                                        <div className="mb-0" dangerouslySetInnerHTML={{
                                            __html: renderDescriptionHTML(showDetail.PodcastChannel.Description || ""),
                                        }}></div>
                                    </div>
                                </div>
                            ) : (
                                <div className="show-detail__channel show-detail__channel--single">
                                    <div className="show-detail__single-show-badge">
                                        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                                            <path d="M12 2L2 7l10 5 10-5-10-5z" />
                                            <path d="M2 17l10 5 10-5M2 12l10 5 10-5" />
                                        </svg>
                                        <h4>Single Show</h4>
                                        <p>This show is not part of any channel</p>
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Hashtags Section */}
                    {showDetail.Hashtags && showDetail.Hashtags.length > 0 && (
                        <div className="show-detail__section">
                            <Typography variant="h5" className="show-detail__section-title">
                                Hashtags
                            </Typography>
                            <div className="show-detail__hashtags mt-3">
                                {showDetail.Hashtags.map((tag) => (
                                    <Chip
                                        key={tag.Id}
                                        label={`#${tag.Name}`}
                                        className="show-detail__hashtag"
                                    />
                                ))}
                            </div>
                        </div>
                    )}
                    {showDetail.TrailerAudioFileKey !== null && (
                        <div className="show-detail__section">
                            <Typography variant="h5" className="show-detail__section-title">
                                Trailer Audio
                            </Typography>
                            <SmartAudioPlayer
                                audioId={showDetail.TrailerAudioFileKey}
                                className="w-full mt-4"
                                fetchUrlFunction={async (fileKey) => {
                                    const result = await getPublicSource(loginRequiredAxiosInstance, fileKey);
                                    return {
                                        success: result.success,
                                        data: result.data ? { FileUrl: result.data.FileUrl } : undefined,
                                        message: typeof result.message === 'string' ? result.message : result.message?.content
                                    };
                                }}
                            />
                        </div>
                    )}

                    {/* Subscription Plans */}
                    {showDetail.PodcastSubscriptionList && showDetail.PodcastSubscriptionList.filter(sub => sub.IsActive).length > 0 && (
                        <div className="show-detail__section">
                            <Typography variant="h5" className="show-detail__section-title">
                                Subscription Plans
                            </Typography>
                            <div className="show-detail__subscriptions">
                                {showDetail.PodcastSubscriptionList
                                    .filter(sub => sub.IsActive)
                                    .map((sub) => {
                                        // Latest price per cycle type by Version
                                        const latestPricesByCycle: Record<string, any> = {};
                                        (sub.PodcastSubscriptionCycleTypePriceList || []).forEach((price: any) => {
                                            const key = String(price?.SubscriptionCycleType?.Id ?? price?.SubscriptionCycleTypeId ?? 'unknown');
                                            const prev = latestPricesByCycle[key];
                                            if (!prev || (price?.Version ?? 0) > (prev?.Version ?? 0)) {
                                                latestPricesByCycle[key] = price;
                                            }
                                        });
                                        const latestPrices = Object.values(latestPricesByCycle);

                                        // Latest benefit mapping per benefit id by Version
                                        const latestBenefitsById: Record<string, any> = {};
                                        (sub.PodcastSubscriptionBenefitMappingList || []).forEach((m: any) => {
                                            const bid = String(m?.PodcastSubscriptionBenefit?.Id ?? m?.PodcastSubscriptionBenefitId ?? 'unknown');
                                            const prev = latestBenefitsById[bid];
                                            if (!prev || (m?.Version ?? 0) > (prev?.Version ?? 0)) {
                                                latestBenefitsById[bid] = m;
                                            }
                                        });
                                        const latestBenefits = Object.values(latestBenefitsById);

                                        return (
                                            <div key={sub.Id} className="show-detail__subscription-card">
                                                <h4>{sub.Name}</h4>
                                                <p>{sub.Description || 'No description'}</p>
                                                <div className="show-detail__subscription-prices">
                                                    {latestPrices.map((price: any) => (
                                                        <div key={price?.SubscriptionCycleType?.Id ?? price?.SubscriptionCycleTypeId} className="show-detail__price-item">
                                                            <span className="show-detail__price-cycle">{price?.SubscriptionCycleType?.Name ?? price?.SubscriptionCycleTypeName ?? 'Cycle'}</span>
                                                            <span className="show-detail__price-amount">{Number(price?.Price).toLocaleString()} VND</span>
                                                        </div>
                                                    ))}
                                                </div>
                                                <div className="show-detail__subscription-benefits">
                                                    {latestBenefits.map((benefitMap: any) => (
                                                        <Chip
                                                            key={benefitMap?.PodcastSubscriptionBenefit?.Id ?? benefitMap?.PodcastSubscriptionBenefitId}
                                                            label={benefitMap?.PodcastSubscriptionBenefit?.Name ?? benefitMap?.PodcastSubscriptionBenefitName}
                                                            size="small"
                                                            className="show-detail__benefit-chip"
                                                        />
                                                    ))}
                                                </div>
                                            </div>
                                        );
                                    })}
                            </div>
                        </div>
                    )}

                    {/* Episodes Table Section */}
                    {showDetail.EpisodeList && showDetail.EpisodeList.length > 0 && (
                        <div className="show-detail__section">
                            <Typography variant="h5" className="show-detail__section-title">
                                Episodes ({showDetail.EpisodeList.length})
                            </Typography>
                            <div className="show-detail__episodes-table-wrapper">
                                <table className="show-detail__episodes-table">
                                    <thead>
                                        <tr>
                                            <th style={{ width: '60px' }}>#</th>
                                            <th style={{ width: '80px' }}>Cover</th>
                                            <th>Episode Name</th>
                                            <th style={{ width: '120px' }}>Release Date</th>
                                            <th style={{ width: '100px' }}>Status</th>
                                            <th style={{ width: '100px' }}>Duration</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {showDetail.EpisodeList.map((episode) => (
                                            <tr
                                                key={episode.Id}
                                                onClick={() => navigate(`/episode/${episode.Id}`)}
                                                style={{ cursor: 'pointer' }}
                                            >
                                                <td className="show-detail__episode-order">{episode.EpisodeOrder}</td>
                                                <td>
                                                    <Image
                                                        mainImageFileKey={episode.MainImageFileKey}
                                                        alt={episode.Name}
                                                        className="show-detail__episode-table-thumb"
                                                    />
                                                </td>
                                                <td className="show-detail__episode-name">{episode.Name}</td>
                                                <td className="show-detail__episode-date">{formatDate(episode.ReleaseDate)}</td>
                                                <td>
                                                    <span
                                                        className={`show-detail__episode-status show-detail__episode-status--${toStatusClass(
                                                            episode.CurrentStatus?.Name
                                                        )}`}
                                                    >
                                                        {episode.CurrentStatus?.Name || 'Unknown'}
                                                    </span>
                                                </td>
                                                <td className="show-detail__episode-duration">
                                                    {episode.AudioLength ? `${Math.floor(episode.AudioLength / 60)}:${String(episode.AudioLength % 60).padStart(2, '0')}` : 'N/A'}
                                                </td>
                                            </tr>
                                        ))}
                                    </tbody>
                                </table>
                            </div>
                        </div>
                    )}

                </div>

                {/* Right Sidebar */}
                <div className="show-detail__sidebar">
                    <div className="show-detail__info-card">
                        <h4 className="show-detail__info-title">Show Information</h4>
                        <div className="show-detail__info-list">
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Subcategory</span>
                                <span className="show-detail__info-value">{showDetail.PodcastSubCategory.Name}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Subscription Type</span>
                                <span className="show-detail__info-value">{showDetail.PodcastShowSubscriptionType.Name}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Upload Frequency</span>
                                <span className="show-detail__info-value">{showDetail.UploadFrequency}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Copyright</span>
                                <span className="show-detail__info-value">{showDetail.Copyright}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Release Date</span>
                                <span className="show-detail__info-value">{formatDate(showDetail.ReleaseDate)}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Is Released</span>
                                <span className="show-detail__info-value">{showDetail.IsReleased ? 'Yes' : 'No'}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Created At</span>
                                <span className="show-detail__info-value">{formatDate(showDetail.CreatedAt)}</span>
                            </div>
                            <div className="show-detail__info-item">
                                <span className="show-detail__info-label">Updated At</span>
                                <span className="show-detail__info-value">{formatDate(showDetail.UpdatedAt)}</span>
                            </div>
                        </div>
                    </div>


                </div>
            </div>
        </div>
    );
};

export default ShowDetailView;