"use client"

import type React from "react"
import { useContext, useEffect, useState } from "react"
import { CButton, CCol, CCard, CCardHeader, CCardBody, CBadge, CRow } from "@coreui/react"
import { toast } from "react-toastify"
import { ShowReportReviewViewContext } from "."
import type { ShowReportReviewSession } from "@/core/types/show-report"
import { CheckCircle, XCircle, Clock, User, Calendar, FileText } from "phosphor-react"
import { formatDate } from "@/core/utils/date.util"
import { getShowReviewSessionDetail } from "@/core/services/report/ShowReport.Service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v1/admin-instance"


interface ShowReportDetailProps {
  podcastShowReportReviewSessionId: string
  onClose: () => void
}

const DetailForm: React.FC<ShowReportDetailProps> = ({ podcastShowReportReviewSessionId, onClose }) => {
  const [ShowReportDetail, setShowReportDetail] = useState<ShowReportReviewSession | null>(null)
  const [loading, setLoading] = useState(false)

  const fetchDetail = async (id: string) => {
    try {
      const response = await getShowReviewSessionDetail(adminAxiosInstance, id);
      if (response.success) {
        setShowReportDetail(response.data.ShowReportReviewSession);
      } else {
        console.error('API Error:', response.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch show reports:', error);
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

 
  useEffect(() => {
    fetchDetail(podcastShowReportReviewSessionId)
  }, [podcastShowReportReviewSessionId])

  if (!ShowReportDetail) {
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
          <h2 className="show-report-detail__title mt-2">{ShowReportDetail.PodcastShow.Name}</h2>
          <div className="show-report-detail__metadata mt-3">
            <div className="flex gap-3">
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Created:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(ShowReportDetail.CreatedAt)}</span>
              </div>
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Updated:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(ShowReportDetail.UpdatedAt)}</span>
              </div>
            </div>

            <div className="show-report-detail__metadata-item">
              <User size={16} className="me-2" />
              <span className="show-report-detail__metadata-item--label">Assigned to:</span>
              <span className="ms-1 show-report-detail__metadata-item--value">{ShowReportDetail.AssignedStaff.FullName}</span>
            </div>
          </div>
        </div>

        <div className="show-report-detail__verification">
          <div className="show-report-detail__verification-label">Review Status</div>
          {getStatusBadge(ShowReportDetail.IsResolved)}
        </div>
      </div>

        <div className="show-report-detail__reports">
          <div className="show-report-detail__reports-header">
            <h5 className="show-report-detail__reports-title">
              <FileText size={20} className="me-2" />
              Reports ({ShowReportDetail.ShowReportList?.length || 0})
            </h5>
          </div>
          <div className="show-report-detail__reports-container">
            {ShowReportDetail.ShowReportList.map((report: any, index: number) => (
              <div
                key={report.Id}
                className={`show-report-detail__report-item ${index !== ShowReportDetail.ShowReportList?.length - 1 ? "show-report-detail__report-item--bordered" : ""}`}
              >
                <div className="show-report-detail__report-header">
                  <div className="show-report-detail__report-meta">
                    <CBadge color="info" className="show-report-detail__report-type me-2">
                      {report.PodcastShowReportType.Name}
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

    </div>
  )
}

const ShowReportDetail: React.FC<ShowReportDetailProps> = (props) => {
  return <DetailForm {...props} />
}

export default ShowReportDetail
