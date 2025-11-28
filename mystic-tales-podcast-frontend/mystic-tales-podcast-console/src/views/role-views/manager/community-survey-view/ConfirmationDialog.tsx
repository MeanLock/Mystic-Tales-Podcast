import type React from "react"
import { CButton } from "@coreui/react"

interface ConfirmationDialogProps {
  show: boolean
  title: string
  message: string
  onConfirm: () => void
  onCancel: () => void
  confirmText?: string
  cancelText?: string
  confirmColor?: string
}

export const ConfirmationDialog: React.FC<ConfirmationDialogProps> = ({
  show,
  title,
  message,
  onConfirm,
  onCancel,
  confirmText = "Xác nhận",
  cancelText = "Hủy",
  confirmColor = "danger",
}) => {
  if (!show) return null

  return (
    <div className="modal show d-block" style={{ backgroundColor: "rgba(0,0,0,0.5)" }}>
      <div className="modal-dialog modal-dialog-centered">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">{title}</h5>
          </div>
          <div className="modal-body">
            <p>{message}</p>
          </div>
          <div className="modal-footer">
            <CButton color="secondary" onClick={onCancel}>
              {cancelText}
            </CButton>
            <CButton color={confirmColor} onClick={onConfirm} style={{ color: "white" }}>
              {confirmText}
            </CButton>
          </div>
        </div>
      </div>
    </div>
  )
}
