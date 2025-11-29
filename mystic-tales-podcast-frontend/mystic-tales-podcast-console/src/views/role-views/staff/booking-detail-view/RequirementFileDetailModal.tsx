import type React from "react"
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { Typography, Box, Button } from "@mui/material"
import { Download } from "@mui/icons-material"
import { useCallback, useEffect, useState } from "react"
import { DocumentViewer } from "@/views/components/common/document"
import { getRequirements } from "@/core/services/file/file.service"
import { staffAxiosInstance } from "@/core/api/rest-api/config/instances/v2/staff-axios-instance"

interface RequirementFileDetailModalProps {
  bookingRequirementFile: any
}

const RequirementFileDetailModal: React.FC<RequirementFileDetailModalProps> = ({ bookingRequirementFile }) => {
  if (!bookingRequirementFile) return null
  const [requirementUrl, setRequirementUrl] = useState<string>("")
  const [downloading, setDownloading] = useState(false)
  const requirementKey = bookingRequirementFile?.RequirementDocumentFileKey

  const fetchPresigned = useCallback(async (): Promise<string> => {
    if (!requirementKey) return ""
    try {
      const res = await getRequirements(staffAxiosInstance, requirementKey)
      return res?.success ? (res.data?.FileUrl || "") : ""
    } catch (e) {
      console.error(e)
      return ""
    }
  }, [requirementKey])

  useEffect(() => {
    ;(async () => {
      const url = await fetchPresigned()
      setRequirementUrl(url)
    })()
  }, [fetchPresigned])


  const handleDownload = async () => {
    if (!requirementKey) return
    try {
      setDownloading(true)
      const freshUrl = await fetchPresigned()
      if (freshUrl) {
        window.open(freshUrl, "_blank", "noopener,noreferrer")
      } else {
        console.error("Cannot get download link")
      }
    } finally {
      setDownloading(false)
    }
  }


  const tone = bookingRequirementFile?.PodcastBookingTone
  const toneCategory = tone?.PodcastBookingToneCategory

  return (
    <Box className="requirement-modal">
      <div className="requirement-modal__header">
        <h6 className="requirement-modal__title font-sans">{bookingRequirementFile.Name}</h6>
        {bookingRequirementFile?.WordCount > 0 && (
          <span className="requirement-modal__pill">{bookingRequirementFile.WordCount} words</span>
        )}
      </div>

      <div className="requirement-modal__viewer">
        <DocumentViewer url={requirementUrl} height={420} />
        <div className="requirement-modal__actions">
          <Button
            variant="outlined"
            size="small"
            className="booking-detail__download-btn"
            startIcon={<Download />}
            onClick={handleDownload}
            disabled={downloading || !requirementKey}
          >
            {downloading ? "Preparing..." : "Download"}
          </Button>
        </div>
      </div>

      <div className="requirement-modal__grid">
        <div className="requirement-modal__row">
          <span className="requirement-modal__label">Category</span>
          <span className="requirement-modal__value">{toneCategory?.Name || "—"}</span>
        </div>
        <div className="requirement-modal__row">
          <span className="requirement-modal__label">Tone</span>
          <span className="requirement-modal__value">{tone?.Name || "—"}</span>
        </div>
        {tone?.Description && (
          <div className="requirement-modal__row requirement-modal__row--full">
            <span className="requirement-modal__label">Tone Description</span>
            <span className="requirement-modal__value">{tone.Description}</span>
          </div>
        )}
      </div>

      <div className="requirement-modal__section">
        <div className="requirement-modal__section-title">Detailed Requirements</div>
        <div className="requirement-modal__description">
          <div
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(bookingRequirementFile?.Description || ""),
            }}
          />
        </div>
      </div>
    </Box>
  )
}

export default RequirementFileDetailModal
