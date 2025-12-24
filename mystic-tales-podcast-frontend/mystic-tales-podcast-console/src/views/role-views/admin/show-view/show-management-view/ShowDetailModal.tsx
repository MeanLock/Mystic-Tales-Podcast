import React, { useEffect, useState, createContext } from 'react';
import {
    Typography,
    TextField,
    Chip,
} from '@mui/material';
import Slider from 'react-slick';
import './styles.scss';
import {  useParams } from 'react-router-dom';
import {  getShowDetail } from '@/core/services/show/show.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { useSelector } from 'react-redux';
import { HashtagOption } from '@/core/types';
import Loading from '@/views/components/common/loading';
import { toast } from 'react-toastify';
import { formatDate, getTimeAgo } from '@/core/utils/date.util';
import Image from '@/views/components/common/image';
import { fetchImage } from '@/core/utils/image.util';
import { ShowDetail } from '@/core/types/show';


interface Props {
    id: string;
}
interface ShowInfoViewContextProps {
    handleDataChange: () => void
    channel: any | null
}


const ShowInfo = ({ id }: Props) => {
    const [showDetail, setShowDetail] = useState<ShowDetail | null>(null);
    const [loading, setLoading] = useState<boolean>(false);




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


    useEffect(() => {
        fetchShowDetail()
    }, [id]);


    if (loading || !showDetail) {
        return (
            <div className="flex justify-center items-center h-100">
                <Loading />
            </div>
        );
    }

    return (
            <div className="show-info-page">
                {showDetail.TakenDownReason && (
                    <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3" style={{ width: "fit-content" }}>
                        <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                        </svg>
                        <span className="text-xs text-red-700 font-medium">
                            <strong>Taken Down Reason:</strong> {showDetail.TakenDownReason}
                        </span>
                    </div>
                )}

                <div className="show-info-page__content">
                    <div className="show-info-page__form">
                        <div className="show-info-page__row">
                            <TextField
                                label="Name"
                                value={showDetail.Name}
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--name"
                            />

                            <TextField
                                id="filled-read-only-input"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Status"
                                value={showDetail.CurrentStatus.Name}
                                className="show-info-page__input show-info-page__input--status"

                            />
                        </div>
                        <div className="show-info-page__row">
                            <TextField
                                id="filled-read-only-input"
                                variant="filled"
                                label="Channel"
                                value={showDetail.PodcastChannel?.Name || 'Single Show'}
                                className="show-info-page__input show-info-page__input--status"
                            />

                            <TextField
                                label="Language"
                                variant="filled"
                                value={showDetail.Language}
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--status"
                            />
                            <TextField
                                label="Upload Frequency"
                                variant="filled"
                                value={showDetail.UploadFrequency}
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--status"
                            />

                        </div>

                        {/* Category and Subcategory Row */}
                        <div className="show-info-page__row">
                            <TextField
                                label="Category"
                                variant="filled"
                                value={showDetail.PodcastCategory?.Name || ''}
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--status"
                            />
                            <TextField
                                label="Subcategory"
                                variant="filled"
                                value={showDetail.PodcastSubCategory?.Name || ''}
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--status"
                            />
                            <TextField
                                label="Subscription Type"
                                variant="filled"
                                value={showDetail.PodcastShowSubscriptionType?.Name || 'Free'}
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                className="show-info-page__input show-info-page__input--status"
                            />
                        </div>

                        {/* Dates and Numbers Row */}
                        <div className="show-info-page__row">
                            {showDetail.ReleaseDate !== null && (
                                <TextField
                                    variant="filled"
                                    slotProps={{
                                        input: {
                                            readOnly: true,
                                        },
                                    }}
                                    label="Release Date"
                                    value={formatDate(showDetail.ReleaseDate)}
                                    className="show-info-page__input-small"

                                />
                            )}
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Is Released"
                                value={showDetail.IsReleased ? 'Yes' : 'No'}
                                className="show-info-page__input-small"

                            />
                            <TextField
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Created At"
                                value={formatDate(showDetail.CreatedAt)}
                                className="show-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Updated At"
                                value={formatDate(showDetail.UpdatedAt)}
                                className="show-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Total Followers"
                                value={showDetail.TotalFollow}
                                className="show-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Listen Count"
                                value={showDetail.ListenCount}
                                className="show-info-page__input-small"

                            />
                            <TextField
                                id="filled-helperText"
                                variant="filled"
                                slotProps={{
                                    input: {
                                        readOnly: true,
                                    },
                                }}
                                label="Rating Average"
                                value={`${showDetail.AverageRating} ⭐`}
                                className="show-info-page__input-small"

                            />


                        </div>

                        <TextField
                            label="Copyright"
                            value={showDetail.Copyright}
                            variant="filled"
                            slotProps={{
                                input: {
                                    readOnly: true,
                                },
                            }}
                            className="show-info-page__input show-info-page__input--name"
                        />

                        <div className="show-info-page__hashtags">
                            <Typography variant="body2" className="show-info-page__description-label">
                                Hashtags
                            </Typography>
                            <div className="show-info-page__hashtag-chips">
                                {showDetail.Hashtags?.map((tag, index) => (
                                    <Chip
                                        key={index}
                                        label={tag.Name}
                                        size="small"
                                        sx={{
                                            backgroundColor: 'var(--primary-green)',
                                            color: 'black',
                                            margin: '2px',
                                            '& .MuiChip-deleteIcon': {
                                                color: 'black',
                                                '&:hover': { color: '#444' }
                                            },
                                            padding: '6px 4px',
                                            boxShadow: '2px 6px 6px rgba(0, 0, 0, 0.7)'
                                        }}
                                    />
                                ))}
                            </div>
                        </div>

                        {/* Description */}
                        <div className="show-info-page__description">
                            <Typography variant="body2" className="show-info-page__description-label">
                                Description
                            </Typography>
                            <Typography 
                                variant="body2" 
                                className="show-info-page__preview-subtitle"
                                dangerouslySetInnerHTML={{
                                    __html: showDetail.Description || 'No description available'
                                }}
                            />
                        </div>
                    </div>

                    {/* Preview Section */}
                    <div className="show-info-page__preview">
                        <div className="show-info-page__main-image-container">
                            <Image
                                mainImageFileKey={showDetail.MainImageFileKey}
                                alt={showDetail.Name}
                                className="show-info-page__main-image-file"
                            />
                        </div>
                    </div>
                </div>
                {showDetail.ReviewList && showDetail.ReviewList.length > 0 && (
                    <div style={{ marginTop: '2rem' }}>
                        <Typography variant="h6" style={{ color: 'white', marginBottom: '1rem', fontWeight: 'bold' }}>
                            Customer Reviews
                        </Typography>
                        <Slider
                            dots={true}
                            infinite={false}
                            speed={500}
                            slidesToShow={3}
                            slidesToScroll={1}
                            arrows={true}
                            responsive={[
                                {
                                    breakpoint: 1024,
                                    settings: {
                                        slidesToShow: 2,
                                        slidesToScroll: 1,
                                    }
                                },
                                {
                                    breakpoint: 600,
                                    settings: {
                                        slidesToShow: 1,
                                        slidesToScroll: 1,
                                    }
                                }
                            ]}
                        >
                            {showDetail.ReviewList.map((review) => (
                                <div key={review.Id} style={{ padding: '0 8px' }}>
                                    <div
                                        className="rounded-2xl p-6 h-full"
                                        style={{ backgroundColor: "rgba(255, 255, 255, 0.1)" }}
                                    >
                                        <div className="flex justify-between items-start mb-2">
                                            <div className="text-xs text-gray-200">
                                                {getTimeAgo(review.UpdatedAt)}
                                            </div>

                                            <div className="flex mb-4">
                                                {Array.from({ length: 5 }).map((_, i) => (
                                                    <svg
                                                        key={i}
                                                        className={`w-4 h-4 ${i < Math.floor(review.Rating)
                                                            ? "text-yellow-400"
                                                            : "text-gray-400"
                                                            }`}
                                                        fill="currentColor"
                                                        viewBox="0 0 20 20"
                                                    >
                                                        <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                                    </svg>
                                                ))}
                                            </div>
                                        </div>
                                        <div className='flex'>
                                            <div className='flex flex-col items-center mr-6'>
                                               
                                                <Image
                                                    mainImageFileKey={review.Account.MainImageFileKey}
                                                    alt={review.Account.FullName}
                                                    className="w-12 h-12 rounded-full object-cover mb-4"
                                                />
                                                 <p className="text-xs text-gray-200">
                                                    {review.Account.FullName}
                                                </p>
                                            </div>

                                            <div>
                                                <h4 className="font-semibold  text-left  text-white text-base mb-2">
                                                    {review.Title}
                                                </h4>
                                                <p className="text-gray-100 text-sm text-left leading-relaxed">
                                                    {review.Content}
                                                </p>
                                            </div>
                                        </div>

                                    </div>
                                </div>
                            ))}
                        </Slider>
                    </div>
                )}
            </div>
    );
};

export default ShowInfo;
