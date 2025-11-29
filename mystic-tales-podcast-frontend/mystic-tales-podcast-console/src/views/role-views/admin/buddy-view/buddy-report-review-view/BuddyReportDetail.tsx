"use client"

import type React from "react"
import { useContext, useEffect, useState } from "react"
import { CButton, CCol, CCard, CCardHeader, CCardBody, CBadge, CRow } from "@coreui/react"
import { toast } from "react-toastify"
import { BuddyReportReviewViewContext } from "."
import { CheckCircle, XCircle, Warning, User, Calendar, FileText } from "phosphor-react"
import { formatDate } from "@/core/utils/date.util"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v1/admin-instance"
import { getBuddyReviewSessionDetail } from "@/core/services/report/BuddyReport.service"

export const mockDetail: any = {
  BuddyReportReviewSession: {
    Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    PodcastBuddy: {
      Id: 101,
      FullName: "Nguyen Buddy A",
      Email: "string",
      MainImageFileKey: "string"
    },
    AssignedStaff: {
      Id: 501,
      FullName: "Alice Nguyen",
    },
    ResolvedViolationPoint: null,
    IsResolved: null,
    CreatedAt: "2025-10-10T12:21:26.284Z",
    UpdatedAt: "2025-10-10T13:05:10.100Z",
    BuddyReportList: [
      {
        Id: "a9b1e321-6d23-4e94-bf1d-8f59ab3f86a3",
        Content: "Contains misleading information about health topics.",
        AccountId: 301,
        PodcastBuddy: {
          Id: 101,
          FullName: "Nguyen Buddy A",
          Email: "string",
          MainImageFileKey: "string"
        },
        PodcastBuddyReportType: {
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
        PodcastBuddy: {
          Id: 101,
          FullName: "Nguyen Buddy A",
          Email: "string",
          MainImageFileKey: "string"
        },
        PodcastBuddyReportType: {
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
        PodcastBuddy: {
          Id: 101,
          FullName: "Nguyen Buddy A",
          Email: "string",
          MainImageFileKey: "string"
        },
        PodcastBuddyReportType: {
          Id: 3,
          Name: "Inappropriate Content",
        },
        ResolvedAt: "2025-10-10T12:21:26.284Z",
        CreatedAt: "2025-10-09T12:42:35.500Z",
      },
    ],
  },
}
interface BuddyReportDetailProps {
  podcastBuddyReportReviewSessionId: string
  onClose: () => void
}

const DetailForm: React.FC<BuddyReportDetailProps> = ({ podcastBuddyReportReviewSessionId, onClose }) => {
  const [BuddyReportDetail, setBuddyReportDetail] = useState<any | null>(null)
  const [loading, setLoading] = useState(false)
  const [showResolvePopup, setShowResolvePopup] = useState(false)
  const [violationPoint, setViolationPoint] = useState("")

  const fetchDetail = async (id: string) => {
    try {
      const response = await getBuddyReviewSessionDetail(adminAxiosInstance, id);
      if (response.success) {
        setBuddyReportDetail(response.data.BuddyReportReviewSession);
      } else {
        console.error('API Error:', response.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch buddy reports:', error);
    }
  }

  const getStatusBadge = (isResolved: boolean | null) => {
    if (isResolved === true) {
      return (
        <div className="buddy-report-detail__status-badge buddy-report-detail__status-badge--resolved">
          <span className="buddy-report-detail__status-badge-dot"></span>
          Resolved
        </div>
      )
    } else if (isResolved === false) {
      return (
        <div className="buddy-report-detail__status-badge buddy-report-detail__status-badge--rejected">
          <span className="buddy-report-detail__status-badge-dot"></span>
          Rejected
        </div>
      )
    } else {
      return (
        <div color="warning" className="buddy-report-detail__status-badge buddy-report-detail__status-badge--pending">
          <span className="buddy-report-detail__status-badge-dot"></span>
          Pending
        </div>
      )
    }
  }


  useEffect(() => {
    fetchDetail(podcastBuddyReportReviewSessionId)
  }, [podcastBuddyReportReviewSessionId])

  if (!BuddyReportDetail) {
    return (
      <div className="buddy-report-detail__loading d-flex justify-content-center align-items-center p-5">
        <div className="spinner-border" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    )
  }

  return (
    <div className="buddy-report-detail">
      {/* Header */}
      <div className="buddy-report-detail__header">
        <div className="buddy-report-detail__title-section">
          <h2 className="buddy-report-detail__title mt-1">{BuddyReportDetail.PodcastBuddy.FullName}</h2>
          <div className="buddy-report-detail__metadata mt-3">
            <div className="flex gap-3">
              {
                BuddyReportDetail.ResolvedViolationPoint !== null && (
                  <div className="buddy-report-detail__metadata-item">
                    <Warning size={16} className="me-1" />
                    <span className="buddy-report-detail__metadata-item--label">Violation Point: </span>
                    <span className="ms-1 buddy-report-detail__metadata-item--value">{BuddyReportDetail.ResolvedViolationPoint}</span>
                  </div>
                )
              }
              <div className="buddy-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="buddy-report-detail__metadata-item--label">Created:</span>
                <span className="ms-1 buddy-report-detail__metadata-item--value">{formatDate(BuddyReportDetail.CreatedAt)}</span>
              </div>
              <div className="buddy-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="buddy-report-detail__metadata-item--label">Updated:</span>
                <span className="ms-1 buddy-report-detail__metadata-item--value">{formatDate(BuddyReportDetail.UpdatedAt)}</span>
              </div>
            </div>

            <div className="buddy-report-detail__metadata-item">
              <User size={16} className="me-2" />
              <span className="buddy-report-detail__metadata-item--label">Assigned to:</span>
              <span className="ms-1 buddy-report-detail__metadata-item--value">{BuddyReportDetail.AssignedStaff.FullName}</span>
            </div>
          </div>
        </div>

        <div className="buddy-report-detail__verification">
          <div className="buddy-report-detail__verification-label">Review Status</div>
          {getStatusBadge(BuddyReportDetail.IsResolved)}
        </div>
      </div>

      <div className="buddy-report-detail__reports">
        <div className="buddy-report-detail__reports-header">
          <h5 className="buddy-report-detail__reports-title">
            <FileText size={20} className="me-2" />
            Reports ({BuddyReportDetail.BuddyReportList.length})
          </h5>
        </div>
        <div className="buddy-report-detail__reports-container">
          {BuddyReportDetail.BuddyReportList.map((report: any, index: number) => (
            <div
              key={report.Id}
              className={`buddy-report-detail__report-item ${index !== BuddyReportDetail.BuddyReportList.length - 1 ? "buddy-report-detail__report-item--bordered" : ""}`}
            >
              <div className="buddy-report-detail__report-header">
                <div className="buddy-report-detail__report-meta">
                  <CBadge color="info" className="buddy-report-detail__report-type me-2">
                    {report.PodcastBuddyReportType.Name}
                  </CBadge>
                  <span className="buddy-report-detail__report-id text-muted">#{index + 1}</span>
                </div>
                <div className="buddy-report-detail__report-date text-muted">{formatDate(report.CreatedAt)}</div>
              </div>
              <div className="buddy-report-detail__report-content">
                <p className="buddy-report-detail__report-text">{report.Content}</p>
                <div className="buddy-report-detail__report-info">
                  <div className="buddy-report-detail__report-reporter">
                    <User size={14} className="me-2 mb-1" />
                    Reporter: {report.Account.FullName}
                  </div>
                  {report.ResolvedAt && (
                    <div className="buddy-report-detail__report-resolved-date">
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
      <div className="buddy-report-detail__footer mt-4"> </div>
    </div>
  )
}

const BuddyReportDetail: React.FC<BuddyReportDetailProps> = (props) => {
  return <DetailForm {...props} />
}

export default BuddyReportDetail
