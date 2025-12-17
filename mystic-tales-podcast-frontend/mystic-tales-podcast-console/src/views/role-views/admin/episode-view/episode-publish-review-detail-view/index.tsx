"use client"

import type React from "react"
import { useCallback, useContext, useEffect, useState } from "react"
import { CBadge } from "@coreui/react"
import { toast } from "react-toastify"
import { CheckCircle, XCircle, Warning, User, Calendar, FileText } from "phosphor-react"
import { getEpisodePublishDetail, createEditRequire, acceptEpisodePublish } from "@/core/services/ReviewSession/review-session.service"
import { PublishReview } from "@/core/types/publish-review"
import { useParams } from "react-router-dom"
import "./styles.scss"
import Image from "@/views/components/common/image"
import { getAudioEpisode } from "@/core/services/episode/episode.service"
import Loading from "@/views/components/common/loading"
import { formatDate } from "@/core/utils/date.util"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { EmailOutlined } from "@mui/icons-material"
import { SmartAudioPlayer } from "@/views/components/common/audio"

export const PodcastIllegalContentTypes = [
    { Id: 1, Name: "Near-exact Duplicate Content (trùng 90%)" },
    { Id: 2, Name: "Excessive Restricted Terms (nhiều restrict terms quét được)" },
    { Id: 3, Name: "Hate Speech / Hate Content" },
    { Id: 4, Name: "Harassment / Abusive Language" },
    { Id: 5, Name: "Misleading or False Information" },
    { Id: 6, Name: "Inappropriate or Explicit Sexual Content" },
    { Id: 7, Name: "Violence / Graphical / Offensive Violence" },
    { Id: 8, Name: "Self-Harm or Suicidal Content" },
    { Id: 9, Name: "Privacy Violation (personal data exposure)" },
    { Id: 10, Name: "Impersonation / False Identity" }
];


interface EpisodePublishDetailProps {

}

const EpisodePublishDetail: React.FC<EpisodePublishDetailProps> = () => {
    const { id } = useParams<{ id: string }>();
    const [PublishDetail, setPublishDetail] = useState<PublishReview | null>(null)
    const [loading, setLoading] = useState(false)
    const [showEditModal, setShowEditModal] = useState(false)
    const [note, setNote] = useState("")
    const [selectedIllegalIds, setSelectedIllegalIds] = useState<number[]>([])
    const [savingRequirement, setSavingRequirement] = useState(false)
    const [accepting, setAccepting] = useState(false)
    const [audioUrl, setAudioUrl] = useState<string | null>(null)
    const [audioSources, setAudioSources] = useState<Record<string, { url: string }>>({});

    const fetchDetail = async () => {
        setLoading(true);
        try {
            const res = await getEpisodePublishDetail(adminAxiosInstance, Number(id));
            console.log('Episode Publish Review Detail:', res.data);
            if (res.success) {
                setPublishDetail(res.data.ReviewSession);
                const fileurl = await getAudioEpisode(adminAxiosInstance, res.data.ReviewSession.PodcastEpisode.AudioFileKey);
                if (fileurl.success && fileurl.data.FileUrl) {
                    setAudioUrl(fileurl.data.FileUrl);
                }

            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch customer accounts:', error);
        } finally {
            setLoading(false);
        }
    }
    useEffect(() => {
        fetchDetail()
    }, [id])

    const fetchAudioUrl = useCallback(async (trackId: string, fileKey: string) => {
        try {
            const res = await getAudioEpisode(adminAxiosInstance, fileKey);
            if (res.success) {
                setAudioSources(prev => ({
                    ...prev,
                    [trackId]: { url: res.data.FileUrl }
                }));
            }
        } catch (e) {
            console.error('fetchAudioUrl error', e);
        }
    }, []);
    const fetchOriginalAudioUrl = useCallback(async (fileKey: string) => {
        try {
            const res = await getAudioEpisode(adminAxiosInstance, fileKey);
            if (res.success) {
                setAudioUrl(res.data.FileUrl);
            }
        } catch (e) {
            console.error('fetchAudioUrl error', e);
        }
    }, []);

    useEffect(() => {
        if (PublishDetail?.PublishDuplicateDetectedPodcastEpisodes?.length > 0) {
            PublishDetail.PublishDuplicateDetectedPodcastEpisodes.forEach((track: any) => {
                const fileKey = track.AudioFileKey;
                if (fileKey) fetchAudioUrl(track.Id, fileKey);
            });
        }
        else {
            return;
        }

    }, [PublishDetail?.PublishDuplicateDetectedPodcastEpisodes?.length]);

    const isPending = PublishDetail?.CurrentStatus?.Id === 1 // Pending Review
    const isAccepted = PublishDetail?.CurrentStatus?.Id === 3 // Accepted
    const isReadOnly = !isPending

    useEffect(() => {
        if (PublishDetail) {
            setNote(PublishDetail.Note || "")
            setSelectedIllegalIds(PublishDetail.PodcastIllegalContentTypeList?.map(t => t.Id) || [])
        }
    }, [PublishDetail])

    const toggleIllegalId = (id: number) => {
        setSelectedIllegalIds(prev => prev.includes(id) ? prev.filter(x => x !== id) : [...prev, id])
    }

    const handleSaveRequirement = async () => {
        if (isReadOnly) return
        if (!note.trim()) {
            toast.error("Note is required")
            return
        }
        setSavingRequirement(true)
        try {
            const payload = {
                EpisodePublishReviewSessionUpdateInfo: {
                    Note: note.trim(),
                    PodcastIllegalContentTypeIds: selectedIllegalIds
                }
            }
            const res = await createEditRequire(adminAxiosInstance, Number(id), payload)
            if (res.success) {
                toast.success("Requirements updated")
                setShowEditModal(false)
                await fetchDetail()
            } else {
                toast.error(typeof res.message === 'string' ? res.message : "Update failed")
            }
        } catch (e) {
            toast.error("Error updating requirements")
        } finally {
            setSavingRequirement(false)
        }
    }

    const handleAcceptReject = async (accepted: boolean) => {
        if (isReadOnly) return
        if (!note.trim()) {
            toast.error("Note is required before accepting/rejecting")
            return
        }
        setAccepting(true)
        try {
            const payload = accepted ? {
                EpisodePublishReviewSessionUpdateInfo: {
                    Note: "",
                    PodcastIllegalContentTypeIds: []
                }
            } : {
                EpisodePublishReviewSessionUpdateInfo: {
                    Note: note.trim(),
                    PodcastIllegalContentTypeIds: selectedIllegalIds
                }
            }
            const res = await acceptEpisodePublish(adminAxiosInstance, Number(id), accepted, payload)
            if (res.success) {
                toast.success(accepted ? "Episode Accepted Successfully" : "Episode Rejected Successfully")
                await fetchDetail()
            } else {
                toast.error(typeof res.message === 'string' ? res.message : "Action failed")
            }
        } catch (e) {
            toast.error("Error performing action")
        } finally {
            setAccepting(false)
        }
    }

    const getStatusBadge = (isResolved: boolean | null) => {
        if (isResolved === true) {
            return (
                <div className="show-report-detail__status-badge show-report-detail__status-badge--resolved">
                    <span className="show-report-detail__status-badge-dot"></span>
                    Resolved
                </div>
            )
        } else if (isResolved === false) {
            return (
                <div className="show-report-detail__status-badge show-report-detail__status-badge--rejected">
                    <span className="show-report-detail__status-badge-dot"></span>
                    Rejected
                </div>
            )
        } else {
            return (
                <div color="warning" className="show-report-detail__status-badge show-report-detail__status-badge--pending">
                    <span className="show-report-detail__status-badge-dot"></span>
                    Pending
                </div>
            )
        }
    }


    const renderActionPanel = () => {
        if (!isPending) return null
        return (
            <div className="publish-review-detail__actions">
                <div className="publish-review-detail__actions-group">
                    <button
                        type="button"
                        className="publish-review-detail__btn publish-review-detail__btn--edit"
                        onClick={() => setShowEditModal(true)}
                        disabled={savingRequirement || accepting}
                    >
                        <FileText size={16} className="me-1" />
                        Edit Requirements
                    </button>
                    <button
                        type="button"
                        className="publish-review-detail__btn publish-review-detail__btn--accept"
                        onClick={() => handleAcceptReject(true)}
                        disabled={accepting || savingRequirement || !note.trim()}
                    >
                        <CheckCircle size={16} className="me-1" />
                        {accepting ? "Processing..." : "Accept Episode"}
                    </button>
                    <button
                        type="button"
                        className="publish-review-detail__btn publish-review-detail__btn--reject"
                        onClick={() => handleAcceptReject(false)}
                        disabled={accepting || savingRequirement || !note.trim()}
                    >
                        <XCircle size={16} className="me-1" />
                        Reject Episode
                    </button>
                </div>
            </div>
        )
    }



    if (loading || !PublishDetail) {
        return (
            <div className="flex items-center justify-center h-100">
                <Loading />
            </div>
        )
    }

    return (
        <div className="publish-review-detail">
            {/* Header */}
            <div className="publish-review-detail__header">
                <div className="publish-review-detail__title-section mb-2">
                    <h2 className="publish-review-detail__title mt-1">{PublishDetail?.PodcastEpisode?.Name || 'Episode'}</h2>
                    <div className="publish-review-detail__subtitle flex items-center gap-2">
                        <User size={16} className="me-1" />
                        <span className="font-medium">Staff:  </span> {PublishDetail?.AssignedStaff.FullName}
                    </div>
                    <div className="publish-review-detail__subtitle flex items-center gap-2">
                        <EmailOutlined sx={{ fontSize: 16 }} className="me-1" />
                        <span className="font-medium">Email:  </span> {PublishDetail?.AssignedStaff.Email}
                    </div>
                </div>

                <div className="publish-review-detail__content-grid">
                    <div className="publish-review-detail__summary-left">
                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Status:</span>
                            <CBadge color={isPending ? 'warning' : isAccepted ? 'success' : 'danger'} className="ms-1">
                                {PublishDetail?.CurrentStatus?.Name || 'Unknown'}
                            </CBadge>
                        </div>

                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Note:</span>
                            <span className="publish-review-detail__summary-value">{PublishDetail?.Note || '---'}</span>
                        </div>

                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Re-review Count:</span>
                            <span className="publish-review-detail__summary-value">{PublishDetail?.ReReviewCount || '---'}</span>
                        </div>
                    </div>

                    <div className="publish-review-detail__summary-right">
                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Deadline:</span>
                            <span className="publish-review-detail__summary-value">{formatDate(PublishDetail?.Deadline)}</span>
                        </div>

                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Recent Updated:</span>
                            <span className="publish-review-detail__summary-value">{formatDate(PublishDetail?.UpdatedAt)}</span>
                        </div>

                        <div className="publish-review-detail__summary-item">
                            <span className="publish-review-detail__summary-label">Created At:</span>
                            <span className="publish-review-detail__summary-value">{formatDate(PublishDetail?.CreatedAt)}</span>
                        </div>
                    </div>
                </div>
            </div>

            <div className="publish-review-detail__content-cards">
                {PublishDetail.Podcaster && (
                    <div className="content-card">
                        <Image
                            mainImageFileKey={PublishDetail.Podcaster.MainImageFileKey}
                            alt={PublishDetail.Podcaster.FullName}
                            className="content-card__image"
                        />
                        <div className="content-card__body">
                            <span className="content-card__type">Podcaster</span>
                            <h3 className="content-card__title mb-0">{PublishDetail.Podcaster.FullName}</h3>
                            <p className="content-card__description">{PublishDetail.Podcaster.Email}</p>
                            <div className="content-card__stats flex flex-col gap-1">
                                <span className="content-card__stat">Violation Level: {PublishDetail.Podcaster.ViolationLevel}</span>
                                <span className="content-card__stat">Violation Point: {PublishDetail.Podcaster.ViolationPoint}</span>
                            </div>

                        </div>
                    </div>
                )}
                {PublishDetail.PodcastEpisode && (
                    <div className="content-card">
                        <Image
                            mainImageFileKey={PublishDetail.PodcastEpisode.MainImageFileKey}
                            alt={PublishDetail.PodcastEpisode.Name}
                            className="content-card__image"
                        />
                        <div className="flex flex-col w-full">
                            <div className="content-card__body">
                                <span className="content-card__type">Episode</span>
                                <h3 className="content-card__title mb-0">{PublishDetail.PodcastEpisode.Name}</h3>
                                <p className="content-card__description">Explicit Content: {PublishDetail.PodcastEpisode.ExplicitContent ? 'Yes' : 'No'}</p>
                            </div>

                            {/* Audio player full width */}
                            <div className="px-3 pb-3 w-full">
                                <SmartAudioPlayer
                                    audioId={PublishDetail.PodcastEpisode.AudioFileKey}
                                    className="w-full"
                                    fetchUrlFunction={async (fileKey) => {
                                        const result = await getAudioEpisode(adminAxiosInstance, fileKey);
                                        return {
                                            success: result.success,
                                            data: result.data ? { FileUrl: result.data.FileUrl } : undefined,
                                            message: typeof result.message === 'string' ? result.message : result.message?.content
                                        };
                                    }}
                                />
                            </div>

                            <div className="flex-1 flex items-end mb-1 mx-4">
                                <div
                                    className={`content-card__badge content-card__badge--${['Published'].includes(PublishDetail.EpisodeCurrentStatus.Name)
                                        ? 'verified'
                                        : ['Ready To Release', 'Audio Processing'].includes(PublishDetail.EpisodeCurrentStatus.Name)
                                            ? 'ready'
                                            : ['Taken Down', 'Removed'].includes(PublishDetail.EpisodeCurrentStatus.Name)
                                                ? 'rejected'
                                                : ['Pending Review', 'Pending Edit Required'].includes(PublishDetail.EpisodeCurrentStatus.Name)
                                                    ? 'pending'
                                                    : 'draft'
                                        }`}
                                >
                                    <span className="content-card__badge-dot"></span>
                                    {PublishDetail.EpisodeCurrentStatus.Name}
                                </div>
                            </div>
                        </div>
                    </div>
                )}
                {PublishDetail.PodcastChannel && (
                    <div className="content-card">
                        <Image
                            mainImageFileKey={PublishDetail.PodcastChannel.MainImageFileKey}
                            alt={PublishDetail.PodcastChannel.Name}
                            className="content-card__image"
                        />
                        <div className="flex flex-col">
                            <div className="content-card__body">
                                <span className="content-card__type">Channel</span>
                                <h3 className="content-card__title mb-0">{PublishDetail.PodcastChannel.Name}</h3>
                            </div>
                            <div className="flex-1 flex items-end mb-3 mx-3">
                                <div
                                    className={`content-card__badge content-card__badge--${['Published'].includes(PublishDetail.ChannelCurrentStatus.Name)
                                        ? 'verified'
                                        : 'pending'
                                        }`}
                                >
                                    <span className="content-card__badge-dot"></span>
                                    {PublishDetail.ChannelCurrentStatus.Name}
                                </div>
                            </div>
                        </div>
                    </div>
                )}
                {PublishDetail.PodcastShow && (
                    <div className="content-card">
                        <Image
                            mainImageFileKey={PublishDetail.PodcastShow.MainImageFileKey}
                            alt={PublishDetail.PodcastShow.Name}
                            className="content-card__image"
                        />
                        <div className="flex flex-col">
                            <div className="content-card__body">
                                <span className="content-card__type">Show</span>
                                <h3 className="content-card__title mb-0">{PublishDetail.PodcastShow.Name}</h3>
                                <p className="content-card__description"> Release Date: {PublishDetail.PodcastShow.ReleaseDate}</p>
                                {PublishDetail.PodcastChannel === null && (
                                    <p className="content-card__description">Single Show</p>
                                )}
                            </div>
                            <div className="flex-1 flex items-end mb-3 mx-3">
                                <div
                                    className={`content-card__badge content-card__badge--${['Published'].includes(PublishDetail.ShowCurrentStatus.Name)
                                        ? 'verified'
                                        : ['Ready To Release'].includes(PublishDetail.ShowCurrentStatus.Name)
                                            ? 'ready'
                                            : ['Taken Down', 'Removed'].includes(PublishDetail.ShowCurrentStatus.Name)
                                                ? 'rejected'
                                                : 'draft'
                                        }`}
                                >
                                    <span className="content-card__badge-dot"></span>
                                    {PublishDetail.ShowCurrentStatus.Name}
                                </div>
                            </div>
                        </div>
                    </div>
                )}
            </div>
            {/* Illegal Content Types Section */}
            <div className="publish-review-detail__section">
                <div className="publish-review-detail__section-header">
                    <h5 className="publish-review-detail__section-title">
                        <Warning size={20} className="me-2" />
                        Illegal Content Types ({PublishDetail?.PodcastIllegalContentTypeList?.length || 0})
                    </h5>
                </div>
                <div className="publish-review-detail__section-content">
                    {(PublishDetail?.PodcastIllegalContentTypeList || []).length > 0 ? (
                        <div className="publish-review-detail__illegal-grid">
                            {(PublishDetail?.PodcastIllegalContentTypeList || []).map((type: any, index: number) => (
                                <div key={type?.Id || index} className="publish-review-detail__illegal-card">
                                    <CBadge color="danger" className="publish-review-detail__illegal-badge">
                                        #{index + 1}
                                    </CBadge>
                                    <div className="publish-review-detail__illegal-name">{type?.Name || 'Unknown Type'}</div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="publish-review-detail__empty-state">No illegal content types detected</div>
                    )}
                </div>
            </div>

            {/* Duplicate Episodes Section */}
            <div className="publish-review-detail__section">
                <div className="publish-review-detail__section-header">
                    <h5 className="publish-review-detail__section-title">
                        <FileText size={20} className="me-2" />
                        Duplicate Episodes Detected ({PublishDetail?.PublishDuplicateDetectedPodcastEpisodes?.length || 0})
                    </h5>
                </div>
                <div className="publish-review-detail__section-content">
                    {(PublishDetail?.PublishDuplicateDetectedPodcastEpisodes || []).length > 0 ? (
                        <div className="publish-review-detail__duplicate-list">
                            {(PublishDetail?.PublishDuplicateDetectedPodcastEpisodes || []).map((episode: any, index: number) => (
                                <div key={episode?.Id || index} className="publish-review-detail__duplicate-item">
                                    <div className="publish-review-detail__duplicate-header">
                                        <CBadge color="warning" className="me-2">#{index + 1}</CBadge>
                                        <span className="publish-review-detail__duplicate-name">{episode?.Name || 'Unknown Episode'}</span>
                                    </div>
                                    <div className="publish-review-detail__duplicate-details">
                                        <div className="publish-review-detail__duplicate-info">
                                            <span className="publish-review-detail__duplicate-label">Release Date:</span>
                                            <span className="publish-review-detail__duplicate-value">
                                                {episode?.ReleaseDate ? formatDate(episode.ReleaseDate) : 'N/A'}
                                            </span>
                                        </div>
                                        <div className="publish-review-detail__duplicate-info">
                                            <span className="publish-review-detail__duplicate-label">Listen Count:</span>
                                            <span className="publish-review-detail__duplicate-value">{episode?.ListenCount || 0}</span>
                                        </div>
                                        <div className="publish-review-detail__duplicate-info">
                                            {/* <audio
                                                controls
                                                src={audioSources[episode.Id]?.url}
                                                style={{ flex: 1 }}
                                                onError={() => fetchAudioUrl(episode.Id, episode.AudioFileKey)}

                                            /> */}
                                            <SmartAudioPlayer
                                                audioId={episode.AudioFileKey}
                                                className="w-full mt-4 "
                                                fetchUrlFunction={async (fileKey) => {
                                                    const result = await getAudioEpisode(adminAxiosInstance, fileKey);
                                                    return {
                                                        success: result.success,
                                                        data: result.data ? { FileUrl: result.data.FileUrl } : undefined,
                                                        message: typeof result.message === 'string' ? result.message : result.message?.content
                                                    };
                                                }}
                                            />
                                        </div>
                                    </div>
                                    {episode?.Description && (
                                        <div className="publish-review-detail__duplicate-description">
                                            {episode.Description}
                                        </div>
                                    )}
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="publish-review-detail__empty-state">No duplicate episodes found</div>
                    )}
                </div>
            </div>

            {/* Restricted Terms Section */}
            <div className="publish-review-detail__section">
                <div className="publish-review-detail__section-header">
                    <h5 className="publish-review-detail__section-title">
                        <XCircle size={20} className="me-2" />
                        Restricted Terms Found ({PublishDetail?.RestrictedTermFoundList?.length || 0})
                    </h5>
                </div>
                <div className="publish-review-detail__section-content">
                    {(PublishDetail?.RestrictedTermFoundList || []).length > 0 ? (
                        <div className="publish-review-detail__terms-grid">
                            {(PublishDetail?.RestrictedTermFoundList || []).map((term: string, index: number) => (
                                <div key={index} className="publish-review-detail__term-item">
                                    <Warning size={16} className="me-2" color="#dc3545" />
                                    <span className="publish-review-detail__term-text">{term}</span>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="publish-review-detail__empty-state">No restricted terms found</div>
                    )}
                </div>
            </div>

            {/* Summary Panel */}


            {/* Edit Requirements Modal */}
            {showEditModal && (
                <div className="publish-review-modal-overlay" onClick={() => setShowEditModal(false)}>
                    <div className="publish-review-modal" onClick={(e) => e.stopPropagation()}>
                        <div className="publish-review-modal__header">
                            <h4 className="publish-review-modal__title">
                                <FileText size={20} className="me-2" />
                                Edit Review Requirements
                            </h4>
                            <button
                                className="publish-review-modal__close-btn"
                                onClick={() => setShowEditModal(false)}
                                type="button"
                            >
                                ×
                            </button>
                        </div>
                        <div className="publish-review-modal__body">
                            <div className="publish-review-modal__field">
                                <label className="publish-review-modal__label">
                                    Note <span className="text-danger">*</span>
                                </label>
                                <textarea
                                    className="publish-review-modal__textarea"
                                    placeholder="Enter review note for the creator..."
                                    value={note}
                                    onChange={(e) => setNote(e.target.value)}
                                    rows={5}
                                />
                            </div>
                            <div className="publish-review-modal__field">
                                <label className="publish-review-modal__label">
                                    Illegal Content Types
                                </label>
                                <div className="publish-review-modal__checkbox-grid">
                                    {(PublishDetail?.PodcastIllegalContentTypeList || []).map((type) => (
                                        <label key={type.Id} className="publish-review-modal__checkbox-item">
                                            <input
                                                type="checkbox"
                                                checked={selectedIllegalIds.includes(type.Id)}
                                                onChange={() => toggleIllegalId(type.Id)}
                                            />
                                            <span>{type.Name}</span>
                                        </label>
                                    ))}
                                </div>
                            </div>
                            <div className="publish-review-modal__actions">
                                <button
                                    type="button"
                                    className="publish-review-modal__btn publish-review-modal__btn--cancel"
                                    onClick={() => setShowEditModal(false)}
                                    disabled={savingRequirement}
                                >
                                    Cancel
                                </button>
                                <button
                                    type="button"
                                    className="publish-review-modal__btn publish-review-modal__btn--save"
                                    onClick={handleSaveRequirement}
                                    disabled={savingRequirement || !note.trim()}
                                >
                                    {savingRequirement ? "Saving..." : "Save Requirements"}
                                </button>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}



export default EpisodePublishDetail
