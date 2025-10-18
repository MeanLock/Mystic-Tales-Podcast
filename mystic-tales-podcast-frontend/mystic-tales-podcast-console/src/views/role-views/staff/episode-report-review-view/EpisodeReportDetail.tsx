"use client"

import type React from "react"
import { useContext, useEffect, useState } from "react"
import { CButton, CCol, CCard, CCardHeader, CCardBody, CBadge, CRow } from "@coreui/react"
import { toast } from "react-toastify"
import { formatDate } from "../../../../core/utils/date.util"
import { EpisodeReportReviewViewContext } from "."
import { CheckCircle, XCircle, Clock, User, Calendar, FileText } from "phosphor-react"

export const mockDetail: any = {
  EpisodeReportReviewSession: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    PodcastEpisode: {
      Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      Title: "Mindful Talks - Season 1",
    },
    AssignedStaff: {
      Id: 501,
      FullName: "Alice Nguyen",
    },
    IsResolved: null,
    CreatedAt: "2025-10-10T12:21:26.284Z",
    UpdatedAt: "2025-10-10T13:05:10.100Z",
    EpisodeReportList: [
      {
        Id: "a9b1e321-6d23-4e94-bf1d-8f59ab3f86a3",
        Content: "Contains misleading information about health topics.",
        AccountId: 301,
        PodcastEpisode: {
          Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          Title: "Mindful Talks - Season 1",
        },
        PodcastEpisodeReportType: {
          Id: 2,
          Name: "Misinformation",
        },
        ResolvedAt: "2025-10-10T12:21:26.284Z",
        CreatedAt: "2025-10-09T09:15:20.000Z",
      },
      {
        Id: "b7c22b80-3f41-47a9-94b0-42d5df6c3b55",
        Content: "Inappropriate advertisement inserted in mid-episode.",
        AccountId: 302,
        PodcastEpisode: {
          Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          Title: "Mindful Talks - Season 1",
        },
        PodcastEpisodeReportType: {
          Id: 3,
          Name: "Inappropriate Content",
        },
        ResolvedAt: "2025-10-10T12:21:26.284Z",
        CreatedAt: "2025-10-09T12:42:35.500Z",
      },
      {
        Id: "b7c22b80-3f41-47a9-94b0-42d5df6c3b55",
        Content: "Inappropriate advertisement inserted in mid-episode.",
        AccountId: 302,
        PodcastEpisode: {
          Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          Title: "Mindful Talks - Season 1",
        },
        PodcastEpisodeReportType: {
          Id: 3,
          Name: "Inappropriate Content",
        },
        ResolvedAt: "2025-10-10T12:21:26.284Z",
        CreatedAt: "2025-10-09T12:42:35.500Z",
      },
    ],
  },
}
interface EpisodeReportDetailProps {
  podcastEpisodeReportReviewSessionId: string
  onClose: () => void
}

const DetailForm: React.FC<EpisodeReportDetailProps> = ({ podcastEpisodeReportReviewSessionId, onClose }) => {
  const context = useContext(EpisodeReportReviewViewContext)
  const [EpisodeReportDetail, setEpisodeReportDetail] = useState<any | null>(null)
  const [loading, setLoading] = useState(false)

  const fetchDetail = async (id: string) => {
    setEpisodeReportDetail(mockDetail.EpisodeReportReviewSession)
  }

  const handleAction = async (isResolved: boolean) => {
    setLoading(true)
    try {
      // API call to update status
      toast.success(`Report ${isResolved ? "resolved" : "rejected"} successfully`)
      // Update local state
      setEpisodeReportDetail((prev: any) => (prev ? { ...prev, IsResolved: isResolved } : null))
    } catch (error) {
      toast.error("Failed to update report status")
    } finally {
      setLoading(false)
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

  const renderActionButtons = () => {
    if (EpisodeReportDetail?.IsResolved === null) {
      return (
        <div className="show-report-detail__actions">
          <CButton
            color="success"
            variant="outline"
            className="show-report-detail__action-btn show-report-detail__action-btn--resolve me-2"
            onClick={() => handleAction(true)}
            disabled={loading}
          >
            <CheckCircle size={16} className="me-1" />
            Remove
          </CButton>
          <CButton
            color="danger"
            variant="outline"
            className="show-report-detail__action-btn show-report-detail__action-btn--reject"
            onClick={() => handleAction(false)}
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
    fetchDetail(podcastEpisodeReportReviewSessionId)
  }, [podcastEpisodeReportReviewSessionId])

  if (!EpisodeReportDetail) {
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
          <h2 className="show-report-detail__title mt-2">{EpisodeReportDetail.PodcastEpisode.Title}</h2>
          <div className="show-report-detail__metadata mt-3">
            <div className="flex gap-3">
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Created:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(EpisodeReportDetail.CreatedAt)}</span>
              </div>
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Updated:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(EpisodeReportDetail.UpdatedAt)}</span>
              </div>
            </div>

            <div className="show-report-detail__metadata-item">
              <User size={16} className="me-2" />
              <span className="show-report-detail__metadata-item--label">Assigned to:</span>
              <span className="ms-1 show-report-detail__metadata-item--value">{EpisodeReportDetail.AssignedStaff.FullName}</span>
            </div>
          </div>
        </div>

        <div className="show-report-detail__verification">
          <div className="show-report-detail__verification-label">Review Status</div>
          {getStatusBadge(EpisodeReportDetail.IsResolved)}
        </div>
      </div>

      <div className="show-report-detail__reports">
        <div className="show-report-detail__reports-header">
          <h5 className="show-report-detail__reports-title">
            <FileText size={20} className="me-2" />
            Reports ({EpisodeReportDetail.EpisodeReportList.length})
          </h5>
        </div>
        <div className="show-report-detail__reports-container">
          {EpisodeReportDetail.EpisodeReportList.map((report: any, index: number) => (
            <div
              key={report.Id}
              className={`show-report-detail__report-item ${index !== EpisodeReportDetail.EpisodeReportList.length - 1 ? "show-report-detail__report-item--bordered" : ""}`}
            >
              <div className="show-report-detail__report-header">
                <div className="show-report-detail__report-meta">
                  <CBadge color="info" className="show-report-detail__report-type me-2">
                    {report.PodcastEpisodeReportType.Name}
                  </CBadge>
                  <span className="show-report-detail__report-id text-muted">#{index + 1}</span>
                </div>
                <div className="show-report-detail__report-date text-muted">{formatDate(report.CreatedAt)}</div>
              </div>
              <div className="show-report-detail__report-content">
                <p className="show-report-detail__report-text">{report.Content}</p>
                <div className="show-report-detail__report-info">
                  <div className="show-report-detail__report-reporter">
                    <User size={14} className="me-2 mb-1" />
                    Reporter: Account #{report.AccountId}
                  </div>
                  {report.ResolvedAt && (
                    <div className="show-report-detail__report-resolved-date">
                      <CheckCircle size={14} className="me-2 text-success" />
                      Resolved: {formatDate(report.ResolvedAt)}
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="show-report-detail__footer mt-4">
        {renderActionButtons()}
      </div>
    </div>
  )
}

const EpisodeReportDetail: React.FC<EpisodeReportDetailProps> = (props) => {
  return <DetailForm {...props} />
}

export default EpisodeReportDetail
