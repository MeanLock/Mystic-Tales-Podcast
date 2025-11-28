"use client"

import type React from "react"
import { useContext, useEffect, useState } from "react"
import { CButton, CCol, CCard, CCardHeader, CCardBody, CBadge, CRow } from "@coreui/react"
import { toast } from "react-toastify"
import { formatDate } from "../../../../core/utils/date.util"
import { EpisodePublishRequestReviewViewContext } from "."
import { CheckCircle, XCircle, Warning, User, Calendar, FileText } from "phosphor-react"

export const mockList: any = {
  EpisodeReviewSession: {
    Id: 1,
    AssignedStaffId: 12,
    PodcastEpisode: {
      Id: "f47ac10b-58cc-4372-a567-0e02b2c3d479",
      Title: "Bí mật của thành công trong thời đại số",
    },
    Note: "Cần xem lại phần âm thanh ở phút thứ 15, có tiếng nền hơi lớn.",
    ReReviewCount: null,
    Deadline: null,
    CreatedAt: "2025-10-10T08:30:00.000Z",
    UpdatedAt: "2025-10-12T14:45:00.000Z",
    PodcastEpisodeIllegalContentTypeMarkingList: [
      {
        PodcastEpisodeId: "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        PodcastIllegalContentTypeId: 3,
        MarkerId: 45,
        CreatedAt: "2025-10-12T09:00:00.000Z",
      },
      {
        PodcastEpisodeId: "f47ac10b-58cc-4372-a567-0e02b2c3d479",
        PodcastIllegalContentTypeId: 5,
        MarkerId: 46,
        CreatedAt: "2025-10-12T09:05:00.000Z",
      },
    ],
  },
};

interface EpisodePublishDetailProps {
  podcastEpisodePublishReviewSessionId: string
  onClose: () => void
}

const DetailForm: React.FC<EpisodePublishDetailProps> = ({ podcastEpisodePublishReviewSessionId, onClose }) => {
  const context = useContext(EpisodePublishRequestReviewViewContext)
  const [PublishDetail, setPublishDetail] = useState<any | null>(null)
  const [loading, setLoading] = useState(false)
  const [ShowPopup, setShowPopup] = useState(false)
  const [note, setNote] = useState("")
  const [deadline, setDeadline] = useState("")

  const fetchDetail = async (id: string) => {
    setPublishDetail(mockList.EpisodeReviewSession)
  }

  const handleAction = async (IsAccepted : boolean, IsOpen: boolean) => {
    if (IsOpen) {
      setShowPopup(true)
      return
    }

    setLoading(true)
    try {
      toast.success(`Publish Review ${IsAccepted  ? "accepted" : "rejected"} successfully`)
    } catch (error) {
      toast.error("Failed to update report status")
    } finally {
      setLoading(false)
    }
  }

  const handleSubmit = async () => {
    if (!note.trim()) {
      toast.error("Please enter note")
      return
    }

    if (!deadline.trim()) {
      toast.error("Please select deadline")
      return
    }

    setLoading(true)
    try {
      // API call to resolve with note and deadline
      toast.success("Publish request re-check successfully")
      // Update local state
      setPublishDetail((prev: any) =>
        prev ? {
          ...prev,
          Note: note,
          Deadline: deadline
        } : null
      )
      setShowPopup(false)
      setNote("")
      setDeadline("")
    } catch (error) {
      toast.error("Failed to resolve report")
    } finally {
      setLoading(false)
    }
  }

  const handleClosePopup = () => {
    setShowPopup(false)
    setNote("")
    setDeadline("")
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

  const renderActionButtons = () => {
    if (PublishDetail?.Note === null || PublishDetail?.Deadline === null) {
      return (
        <div className="show-report-detail__actions">
          <CButton
            color="success"
            variant="outline"
            className="show-report-detail__action-btn show-report-detail__action-btn--resolve me-2"
            onClick={() => handleAction(true, false)}
            disabled={loading}
          >
            <CheckCircle size={16} className="me-1" />
            Accept
          </CButton>
            <CButton
            color="warning"
            variant="outline"
            className="show-report-detail__action-btn show-report-detail__action-btn--recheck"
            onClick={() => handleAction(true , true)}
            disabled={loading}
          >
            <Warning size={16} className="me-1" />
            Re-Check
          </CButton>
          <CButton
            color="danger"
            variant="outline"
            className="show-report-detail__action-btn show-report-detail__action-btn--reject"
            onClick={() => handleAction(false , false)}
            disabled={loading}
          >
            <XCircle size={16} className="me-1" />
            Reject
          </CButton>
        </div>
      )
    }
    return null
  }

  useEffect(() => {
    fetchDetail(podcastEpisodePublishReviewSessionId)
  }, [podcastEpisodePublishReviewSessionId])

  if (!PublishDetail) {
    return (
      <div className="show-report-detail__loading d-flex justify-content-center align-items-center p-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    )
  }

  return (
    <div className="show-report-detail">
      {/* Header */}
      <div className="show-report-detail__header">
        <div className="show-report-detail__title-section">
          <h2 className="show-report-detail__title mt-1">{PublishDetail.PodcastEpisode.Title}</h2>
          <div className="show-report-detail__metadata mt-3">
            <div className="flex gap-3">
              {
                PublishDetail.Deadline !== null && (
                  <div className="show-report-detail__metadata-item">
                    <Warning size={16} className="me-1" />
                    <span className="show-report-detail__metadata-item--label">Deadline: </span>
                    <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(PublishDetail.Deadline)}</span>
                  </div>
                )
              }
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Created:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(PublishDetail.CreatedAt)}</span>
              </div>
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Updated:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(PublishDetail.UpdatedAt)}</span>
              </div>
            </div>
            {
              PublishDetail.Note !== null && (
                <div className="show-report-detail__metadata-item">
                  <Warning size={16} className="me-1" />
                  <span className="show-report-detail__metadata-item--label">Note: </span>
                  <span className="ms-1 show-report-detail__metadata-item--value">{PublishDetail.Note}</span>
                </div>
              )
            }
                {
              PublishDetail.ReReviewCount !== null && (
                <div className="show-report-detail__metadata-item">
                  <Warning size={16} className="me-1" />
                  <span className="show-report-detail__metadata-item--label">Re-Review Count : </span>
                  <span className="ms-1 show-report-detail__metadata-item--value">{PublishDetail.ReReviewCount}</span>
                </div>
              )
            }
          </div>
        </div>

        {/* <div className="show-report-detail__verification">
          <div className="show-report-detail__verification-label">Review Status</div>
          {getStatusBadge(PublishDetail.IsResolved)}
        </div> */}
      </div>

      <div className="show-report-detail__reports">
        <div className="show-report-detail__reports-header">
          <h5 className="show-report-detail__reports-title">
            <FileText size={20} className="me-2" />
            Illegal Content ({PublishDetail.PodcastEpisodeIllegalContentTypeMarkingList.length})
          </h5>
        </div>
        <div className="show-report-detail__reports-container">
          {PublishDetail.PodcastEpisodeIllegalContentTypeMarkingList.map((report: any, index: number) => (
            <div
              key={report.PodcastEpisodeId}
              className={`show-report-detail__report-item ${index !== PublishDetail.PodcastEpisodeIllegalContentTypeMarkingList.length - 1 ? "show-report-detail__report-item--bordered" : ""}`}
            >
              <div className="show-report-detail__report-header">
                <div className="show-report-detail__report-meta">
                  <CBadge color="info" className="show-report-detail__report-type me-2">
                    {report.PodcastIllegalContentTypeId}
                  </CBadge>
                  <span className="show-report-detail__report-id text-muted">#{index + 1}</span>
                </div>
                <div className="show-report-detail__report-date text-muted">{formatDate(report.CreatedAt)}</div>
              </div>
              <div className="show-report-detail__report-content">
                <p className="show-report-detail__report-text">{report.Content}</p>
                <div className="show-report-detail__report-info">
                  <div className="show-report-detail__report-reporter">
                    MarkerId: Account #{report.MarkerId}
                  </div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="show-report-detail__footer mt-4">
        {renderActionButtons()}
      </div>

      {/* Resolve Popup */}
      {ShowPopup && (
        <div className="resolve-popup-overlay" onClick={handleClosePopup}>
          <div className="resolve-popup" onClick={(e) => e.stopPropagation()}>
            <div className="resolve-popup__header">
              <h4 className="resolve-popup__title">Publish Request Review Session</h4>
              <button
                className="resolve-popup__close-btn"
                onClick={handleClosePopup}
                type="button"
              >
                ×
              </button>
            </div>
            <div className="resolve-popup__body">
              <div className="resolve-popup__field">
                <label className="resolve-popup__label">
                  Note <span className="text-danger">*</span>
                </label>
                <input
                  type="text"
                  className="resolve-popup__input"
                  placeholder="Enter note"
                  value={note}
                  onChange={(e) => setNote(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleSubmit()
                    }
                  }}
                />
              </div>
              <div className="resolve-popup__field">
                <label className="resolve-popup__label">
                  Deadline <span className="text-danger">*</span>
                </label>
                <input
                  type="datetime-local"
                  className="resolve-popup__input"
                  value={deadline}
                  onChange={(e) => setDeadline(e.target.value)}
                  min={new Date().toISOString().slice(0, 16)}
                />
              </div>
              <div className="resolve-popup__actions">
                <button
                  type="button"
                  className="resolve-popup__btn resolve-popup__btn--cancel"
                  onClick={handleClosePopup}
                  disabled={loading}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className="resolve-popup__btn resolve-popup__btn--resolve"
                  onClick={handleSubmit}
                  disabled={loading}
                >
                  {loading ? "Saving..." : "Save"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

const EpisodePublishDetail: React.FC<EpisodePublishDetailProps> = (props) => {
  return <DetailForm {...props} />
}

export default EpisodePublishDetail
