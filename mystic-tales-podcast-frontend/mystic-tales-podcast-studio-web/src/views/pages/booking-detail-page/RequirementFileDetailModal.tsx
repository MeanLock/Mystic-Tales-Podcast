import type React from "react"
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { Typography, Box, Button } from "@mui/material"
import { Download } from "@mui/icons-material"
import { useCallback, useEffect, useState } from "react"
import { get } from "lodash"
import { getRequirements } from "@/core/services/file/file.service"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { DocumentViewer } from "@/views/components/common/document"

interface RequirementFileDetailModalProps {
  bookingRequirementFile: any
}

// Minimal, readable, no flashy backgrounds;
// Uses plain label/value rows and simple sections.
const labelSx = {
  color: "var(--white-75)",
  fontSize: "0.75rem",
  fontWeight: 600,
  textTransform: "uppercase",
  letterSpacing: "0.5px",
  mb: 0.75,
}

const valueSx = {
  color: "#fff",
  fontSize: "0.95rem",
  lineHeight: 1.5,
  fontWeight: 500,
}

const RequirementFileDetailModal: React.FC<RequirementFileDetailModalProps> = ({ bookingRequirementFile }) => {
  if (!bookingRequirementFile) return null
  const [requirementUrl, setRequirementUrl] = useState<string>("");
  const [downloading, setDownloading] = useState(false)
  const requirementKey = bookingRequirementFile?.RequirementDocumentFileKey

  const fetchPresigned = useCallback(async (): Promise<string> => {
    if (!requirementKey) return ""
    try {
      const res = await getRequirements(loginRequiredAxiosInstance, requirementKey)
      return res?.success ? (res.data?.FileUrl || "") : ""
    } catch (e) {
      console.error(e)
      return ""
    }
  }, [requirementKey])

  useEffect(() => {
    // Lấy URL cho preview (có thể hết hạn, chỉ để xem trước)
    (async () => {
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
    <Box sx={{ color: "white", p: 3, display: "flex", flexDirection: "column", gap: 3 }}>
      {/* Title */}
      <Box>
        <Typography variant="h6" sx={{ color: "#fff", fontWeight: 700, fontSize: "1.05rem", mb: 1 }}>
          {bookingRequirementFile.Name}
        </Typography>
        {bookingRequirementFile?.WordCount > 0 && (
          <Typography sx={{ color: "var(--primary-green)", fontSize: "0.8rem", fontWeight: 600 }}>
            {bookingRequirementFile.WordCount} words
          </Typography>
        )}
      </Box>
      <Box>
          <DocumentViewer url={requirementUrl} height={400} fileKey={requirementKey} />
        
        <Box sx={{ mt: 1, display: "flex", gap: 1 }}>
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
        </Box>
      </Box>
      {/* Info Grid */}
      <Box sx={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 2 }}>

        <Box>
          <Typography sx={labelSx}>Category</Typography>
          <Typography sx={valueSx}>{toneCategory?.Name || "—"}</Typography>
        </Box>
        <Box>
          <Typography sx={labelSx}>Tone</Typography>
          <Typography sx={valueSx}>{tone?.Name || "—"}</Typography>
        </Box>
        {tone?.Description && (
          <Box>
            <Typography sx={labelSx}>Tone Description</Typography>
            <Typography sx={valueSx}>{tone.Description}</Typography>
          </Box>
        )}
      </Box>

      {/* Detailed Requirements */}
      <Box>
        <Typography sx={labelSx} >Detailed Requirements</Typography>
        <Box
          sx={{
            "& h1, & h2, & h3": { fontSize: "0.95rem", fontWeight: 600, color: "var(--primary-green)", mt: 2, mb: 1 },
            "& p": { color: "rgba(255,255,255,0.85)", fontSize: "0.85rem", lineHeight: 1.55, mb: 1 },
            "& ul, & ol": { pl: 3, mb: 1.5 },
            "& li": { color: "rgba(255,255,255,0.85)", fontSize: "0.85rem", lineHeight: 1.4, mb: 0.5 },
            "& a": { color: "var(--primary-green)", textDecoration: "none" },
            "& a:hover": { textDecoration: "underline" },
            borderLeft: "2px solid var(--secondary-grey)",
            pl: 2,
          }}
        >
          <div
            dangerouslySetInnerHTML={{
              __html: renderDescriptionHTML(bookingRequirementFile?.Description || ""),
            }}
          />
        </Box>
      </Box>
    </Box>
  )
}

export default RequirementFileDetailModal
