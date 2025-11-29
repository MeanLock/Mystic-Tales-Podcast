"use client"

import type React from "react"
import { useContext, useEffect, useState } from "react"
import { CButton, CCol, CCard, CCardHeader, CCardBody, CBadge, CRow } from "@coreui/react"
import { toast } from "react-toastify"
import { formatDate } from "../../../../core/utils/date.util"
import { BuddyReportReviewViewContext } from "."
import { CheckCircle, XCircle, Warning, User, Calendar, FileText } from "phosphor-react"
import { getBuddyReviewSessionDetail, resolveBuddyReviewSession } from "@/core/services/report/BuddyReport.service"
import { staffAxiosInstance } from "@/core/api/rest-api/config/instances/v2/staff-axios-instance"
import { useSagaPolling } from "@/hooks/useSagaPolling"

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
  const context = useContext(BuddyReportReviewViewContext)
  const [BuddyReportDetail, setBuddyReportDetail] = useState<any | null>(null)
  const [loading, setLoading] = useState(false)
  const [showResolvePopup, setShowResolvePopup] = useState(false)
  const [violationPoint, setViolationPoint] = useState("")
  const { startPolling } = useSagaPolling({
    timeoutSeconds: 120,
    intervalSeconds: 0.5,
  })
  const fetchDetail = async (id: string) => {
    try {
      const response = await getBuddyReviewSessionDetail(staffAxiosInstance, id);
      console.log("Fetched buddy reports:", response);

      if (response.success) {
        setBuddyReportDetail(response.data.BuddyReportReviewSession);
      } else {
        console.error('API Error:', response.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch buddy reports:', error);
    }
  }

  const handleAction = async (isResolved: boolean) => {
    if (isResolved) {
      setShowResolvePopup(true)
      return
    }

    setLoading(true)
    try {
      const res = await resolveBuddyReviewSession(staffAxiosInstance, podcastBuddyReportReviewSessionId, isResolved, 0);
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.success(`Report ${isResolved ? "resolved" : "rejected"} failed, please try again.`)
        return
      }
      await startPolling(sagaId, staffAxiosInstance, {
        onSuccess: () => {
          onClose();
          context?.handleDataChange();
          toast.success(`Report ${isResolved ? "resolved" : "rejected"} successfully`)
        },
        onFailure: (err: any) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })

    } catch (error) {
      toast.error("Failed to update report status")
    } finally {
      setLoading(false)
    }
  }

  const handleResolveSubmit = async () => {
    if (!violationPoint.trim()) {
      toast.error("Please enter violation point")
      return
    }

    const violationPointNum = Number(violationPoint)
    if (isNaN(violationPointNum) || violationPointNum < 0) {
      toast.error("Please enter a valid violation point")
      return
    }

    setLoading(true)
    try {
      const res = await resolveBuddyReviewSession(staffAxiosInstance, podcastBuddyReportReviewSessionId, true, violationPointNum);
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.success(`Report resolved failed, please try again.`)
        return
      }
      await startPolling(sagaId, staffAxiosInstance, {
        onSuccess: () => {
          setShowResolvePopup(false)
          setViolationPoint("")
          onClose();
          context?.handleDataChange();
          toast.success(`Report resolved successfully`)
        },
        onFailure: (err: any) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })

    } catch (error) {
      toast.error("Failed to resolve report")
    } finally {
      setLoading(false)
    }
  }

  const handleClosePopup = () => {
    setShowResolvePopup(false)
    setViolationPoint("")
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
    if (BuddyReportDetail?.IsResolved === null) {
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
            Resolve
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
    fetchDetail(podcastBuddyReportReviewSessionId)
  }, [podcastBuddyReportReviewSessionId])

  if (!BuddyReportDetail) {
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
          <h2 className="show-report-detail__title mt-1">{BuddyReportDetail.PodcastBuddy.FullName}</h2>
          <div className="show-report-detail__metadata mt-3">
            <div className="flex gap-3">
              {
                BuddyReportDetail.ResolvedViolationPoint !== null && (
                  <div className="show-report-detail__metadata-item">
                    <Warning size={16} className="me-1" />
                    <span className="show-report-detail__metadata-item--label">Violation Point: </span>
                    <span className="ms-1 show-report-detail__metadata-item--value">{BuddyReportDetail.ResolvedViolationPoint}</span>
                  </div>
                )
              }
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Created:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(BuddyReportDetail.CreatedAt)}</span>
              </div>
              <div className="show-report-detail__metadata-item">
                <Calendar size={16} className="me-1" />
                <span className="show-report-detail__metadata-item--label">Updated:</span>
                <span className="ms-1 show-report-detail__metadata-item--value">{formatDate(BuddyReportDetail.UpdatedAt)}</span>
              </div>
            </div>

            <div className="show-report-detail__metadata-item">
              <User size={16} className="me-2" />
              <span className="show-report-detail__metadata-item--label">Assigned to:</span>
              <span className="ms-1 show-report-detail__metadata-item--value">{BuddyReportDetail.AssignedStaff.FullName}</span>
            </div>
          </div>
        </div>

        <div className="show-report-detail__verification">
          <div className="show-report-detail__verification-label">Review Status</div>
          {getStatusBadge(BuddyReportDetail.IsResolved)}
        </div>
      </div>

      <div className="show-report-detail__reports">
        <div className="show-report-detail__reports-header">
          <h5 className="show-report-detail__reports-title">
            <FileText size={20} className="me-2" />
            Reports ({BuddyReportDetail.BuddyReportList.length})
          </h5>
        </div>
        <div className="show-report-detail__reports-container">
          {BuddyReportDetail.BuddyReportList.map((report: any, index: number) => (
            <div
              key={report.Id}
              className={`show-report-detail__report-item ${index !== BuddyReportDetail.BuddyReportList.length - 1 ? "show-report-detail__report-item--bordered" : ""}`}
            >
              <div className="show-report-detail__report-header">
                <div className="show-report-detail__report-meta">
                  <CBadge color="info" className="show-report-detail__report-type me-2">
                    {report.PodcastBuddyReportType.Name}
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

      {/* Resolve Popup */}
      {showResolvePopup && (
        <div className="resolve-popup-overlay" onClick={handleClosePopup}>
          <div className="resolve-popup" onClick={(e) => e.stopPropagation()}>
            <div className="resolve-popup__header">
              <h4 className="resolve-popup__title">Resolve Report</h4>
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
                  Violation Point <span className="text-danger">*</span>
                </label>
                <input
                  type="number"
                  min="0"
                  step="1"
                  className="resolve-popup__input"
                  placeholder="Enter violation point"
                  value={violationPoint}
                  onChange={(e) => setViolationPoint(e.target.value)}
                  onKeyPress={(e) => {
                    if (e.key === 'Enter') {
                      handleResolveSubmit()
                    }
                  }}
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
                  onClick={handleResolveSubmit}
                  disabled={loading}
                >
                  {loading ? "Resolving..." : "Resolve"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

const BuddyReportDetail: React.FC<BuddyReportDetailProps> = (props) => {
  return <DetailForm {...props} />
}

export default BuddyReportDetail
