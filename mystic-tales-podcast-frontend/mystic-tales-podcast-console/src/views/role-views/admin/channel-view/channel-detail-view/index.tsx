import { adminAxiosInstance, loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getChannelDetail } from '@/core/services/channel/channel.service';
import Loading from '@/views/components/common/loading';
import React, { useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { Typography, Chip } from '@mui/material';
import Image from '@/views/components/common/image';
import { formatDate } from '@/core/utils/date.util';
import './styles.scss';

const ChannelDetailView = () => {
    const { id } = useParams<{ id: string }>();
    const [channelDetail, setChannelDetail] = React.useState<any | null>(null);
    const [loading, setLoading] = React.useState<boolean>(false);
    const navigate = useNavigate();
    
    const fetchChannelDetail = async () => {
        setLoading(true);
        try {
            const res = await getChannelDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched channel detail:", res.data.Channel);
            if (res.success && res.data) {
                setChannelDetail(res.data.Channel);
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch channel detail:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        if (id) {
            fetchChannelDetail();
        }
    }, [id]);

    if (loading || !channelDetail) {
        return (
            <div className="channel-detail-loading">
                <Loading />
            </div>
        );
    }

    return (
        <div className="channel-detail">
            {/* Header Section */}
            <div className="channel-detail__header">
                {channelDetail.BackgroundImageFileKey && (
                    <div className="channel-detail__header-bg">
                        <Image
                            mainImageFileKey={channelDetail.BackgroundImageFileKey}
                            alt={channelDetail.Name}
                            className="channel-detail__bg-image"
                        />
                    </div>
                )}
                <div className="channel-detail__header-content">
                    <Image
                        mainImageFileKey={channelDetail.MainImageFileKey}
                        alt={channelDetail.Name}
                        className="channel-detail__cover-image"
                    />
                    <div className="channel-detail__header-info">
                        <Typography variant="h3" className="channel-detail__title">
                            {channelDetail.Name}
                        </Typography>
                        <div className="channel-detail__meta">
                            <span className="channel-detail__meta-item">
                                <strong>Category:</strong> {channelDetail.PodcastCategory.Name}
                            </span>
                            <span className="channel-detail__meta-item">
                                <strong>Subcategory:</strong> {channelDetail.PodcastSubCategory.Name}
                            </span>
                            <span className="channel-detail__meta-item">
                                <strong>Status:</strong>
                                <Chip
                                    label={channelDetail.CurrentStatus.Name}
                                    size="small"
                                    className="channel-detail__status-chip"
                                />
                            </span>
                        </div>
                        <div className="channel-detail__stats">
                            <div className="channel-detail__stat">
                                <span className="channel-detail__stat-value">{channelDetail.TotalFavorite.toLocaleString()}</span>
                                <span className="channel-detail__stat-label">Favorites</span>
                            </div>
                            <div className="channel-detail__stat">
                                <span className="channel-detail__stat-value">{channelDetail.ListenCount.toLocaleString()}</span>
                                <span className="channel-detail__stat-label">Listens</span>
                            </div>
                            <div className="channel-detail__stat">
                                <span className="channel-detail__stat-value">{channelDetail.ShowCount}</span>
                                <span className="channel-detail__stat-label">Shows</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            {/* Main Content Grid */}
            <div className="channel-detail__content">
                {/* Left Column */}
                <div className="channel-detail__main">
                    {/* Description Section */}
                    <div className="channel-detail__section">
                        <Typography variant="h5" className="channel-detail__section-title">
                            About This Channel
                        </Typography>
                        
                        <div
                            className="channel-detail__description"
                            dangerouslySetInnerHTML={{ __html: channelDetail.Description }}
                        />
                    </div>

                    {/* Podcaster Section */}
                    <div className="channel-detail__section">
                        <Typography variant="h5" className="channel-detail__section-title">
                            Podcaster
                        </Typography>
                        <div className="channel-detail__podcaster">
                            <Image
                                mainImageFileKey={channelDetail.Podcaster.MainImageFileKey}
                                alt={channelDetail.Podcaster.FullName}
                                className="channel-detail__podcaster-avatar"
                            />
                            <div className="channel-detail__podcaster-info">
                                <h4>{channelDetail.Podcaster.FullName}</h4>
                                <p>{channelDetail.Podcaster.Email}</p>
                            </div>
                        </div>
                    </div>

                    {/* Hashtags Section */}
                    {channelDetail.Hashtags && channelDetail.Hashtags.length > 0 && (
                        <div className="channel-detail__section">
                            <Typography variant="h5" className="channel-detail__section-title">
                                Hashtags
                            </Typography>
                            <div className="channel-detail__hashtags mt-4">
                                {channelDetail.Hashtags.map((tag: any) => (
                                    <Chip
                                        key={tag.Id}
                                        label={`#${tag.Name}`}
                                        className="channel-detail__hashtag"
                                    />
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Subscription Plans */}
                    {channelDetail.PodcastSubscriptionList && channelDetail.PodcastSubscriptionList.filter((sub: any) => sub.IsActive).length > 0 && (
                        <div className="channel-detail__section">
                            <Typography variant="h5" className="channel-detail__section-title">
                                Subscription Plans
                            </Typography>
                            <div className="channel-detail__subscriptions">
                                {channelDetail.PodcastSubscriptionList
                                    .filter((sub: any) => sub.IsActive)
                                    .map((sub: any) => {
                                        // Helper: pick latest version per cycle type
                                        const latestPricesByCycle: Record<string, any> = {};
                                        (sub.PodcastSubscriptionCycleTypePriceList || []).forEach((price: any) => {
                                            const key = String(price.SubscriptionCycleType?.Id ?? price.SubscriptionCycleTypeId ?? 'unknown');
                                            const prev = latestPricesByCycle[key];
                                            if (!prev || (price.Version ?? 0) > (prev.Version ?? 0)) {
                                                latestPricesByCycle[key] = price;
                                            }
                                        });
                                        const latestPrices = Object.values(latestPricesByCycle);

                                        // Helper: pick latest version per benefit id
                                        const latestBenefitsById: Record<string, any> = {};
                                        (sub.PodcastSubscriptionBenefitMappingList || []).forEach((m: any) => {
                                            const bid = String(m.PodcastSubscriptionBenefit?.Id ?? m.PodcastSubscriptionBenefitId ?? 'unknown');
                                            const prev = latestBenefitsById[bid];
                                            if (!prev || (m.Version ?? 0) > (prev.Version ?? 0)) {
                                                latestBenefitsById[bid] = m;
                                            }
                                        });
                                        const latestBenefits = Object.values(latestBenefitsById);

                                        return (
                                            <div key={sub.Id} className="channel-detail__subscription-card">
                                                <h4>{sub.Name}</h4>
                                                <p>{sub.Description || 'No description'}</p>
                                                <div className="channel-detail__subscription-prices">
                                                    {latestPrices.map((price: any) => (
                                                        <div key={price.SubscriptionCycleType?.Id ?? price.SubscriptionCycleTypeId} className="channel-detail__price-item">
                                                            <span className="channel-detail__price-cycle">{price.SubscriptionCycleType?.Name ?? price.SubscriptionCycleTypeName ?? 'Cycle'}</span>
                                                            <span className="channel-detail__price-amount">{Number(price.Price).toLocaleString()} VND</span>
                                                        </div>
                                                    ))}
                                                </div>
                                                <div className="channel-detail__subscription-benefits">
                                                    {latestBenefits.map((benefitMap: any) => (
                                                        <Chip
                                                            key={benefitMap.PodcastSubscriptionBenefit?.Id ?? benefitMap.PodcastSubscriptionBenefitId}
                                                            label={benefitMap.PodcastSubscriptionBenefit?.Name ?? benefitMap.PodcastSubscriptionBenefitName}
                                                            size="small"
                                                            className="channel-detail__benefit-chip"
                                                        />
                                                    ))}
                                                </div>
                                            </div>
                                        );
                                    })}
                            </div>
                        </div>
                    )}

                    {/* Shows Table Section */}
                    {channelDetail.ShowList && channelDetail.ShowList.length > 0 && (
                        <div className="channel-detail__section">
                            <Typography variant="h5" className="channel-detail__section-title">
                                Shows ({channelDetail.ShowList.length})
                            </Typography>
                            <div className="channel-detail__shows-table-wrapper">
                                <table className="channel-detail__shows-table">
                                    <thead>
                                        <tr>
                                            <th style={{ width: '80px' }}></th>
                                            <th>Show Name</th>
                                            <th style={{ width: '120px' }}>Release Date</th>
                                            <th style={{ width: '100px' }}>Episodes</th>
                                            <th style={{ width: '100px' }}>Followers</th>
                                            <th style={{ width: '100px' }}>Status</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        {channelDetail.ShowList.map((show: any) => (
                                            <tr 
                                                key={show.Id}
                                                onClick={() => navigate(`/show/${show.Id}`)}
                                                style={{ cursor: 'pointer' }}
                                            >
                                                <td>
                                                    <Image
                                                        mainImageFileKey={show.MainImageFileKey}
                                                        alt={show.Name}
                                                        className="channel-detail__show-table-thumb"
                                                    />
                                                </td>
                                                <td className="channel-detail__show-name">{show.Name}</td>
                                                <td className="channel-detail__show-date">{formatDate(show.ReleaseDate)}</td>
                                                <td className="channel-detail__show-episodes">{show.EpisodeCount}</td>
                                                <td className="channel-detail__show-followers">{show.TotalFollow.toLocaleString()}</td>
                                                <td>
                                                    <span className={`channel-detail__show-status channel-detail__show-status--${show.CurrentStatus?.Name.trim().toLowerCase().replace(/\s+/g, '-') || 'unknown'}`}>
                                                        {show.CurrentStatus?.Name || 'Unknown'}
                                                    </span>
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
                <div className="channel-detail__sidebar">
                    {/* Channel Information */}
                    <div className="channel-detail__info-card">
                        <h4 className="channel-detail__info-title">Channel Information</h4>
                        <div className="channel-detail__info-list">
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Category</span>
                                <span className="channel-detail__info-value">{channelDetail.PodcastCategory.Name}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Subcategory</span>
                                <span className="channel-detail__info-value">{channelDetail.PodcastSubCategory.Name}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Total Shows</span>
                                <span className="channel-detail__info-value">{channelDetail.ShowCount}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Total Favorites</span>
                                <span className="channel-detail__info-value">{channelDetail.TotalFavorite.toLocaleString()}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Listen Count</span>
                                <span className="channel-detail__info-value">{channelDetail.ListenCount.toLocaleString()}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Created At</span>
                                <span className="channel-detail__info-value">{formatDate(channelDetail.CreatedAt)}</span>
                            </div>
                            <div className="channel-detail__info-item">
                                <span className="channel-detail__info-label">Updated At</span>
                                <span className="channel-detail__info-value">{formatDate(channelDetail.UpdatedAt)}</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ChannelDetailView;