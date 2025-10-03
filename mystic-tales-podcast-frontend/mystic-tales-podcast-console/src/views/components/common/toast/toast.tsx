import React, { FC, useEffect, useRef, useState } from 'react'
import {
  CCard,
  CCardHeader,
  CCardBody,
  CButton,
  CRow,
  CCol,
  CToast,
  CToastBody,
  CToastClose,
  CToastHeader,
  CToaster,
} from '@coreui/react'


export interface ToastProps {
  color: string
  title: string
  mess: string
}

export const toastFormat : FC<ToastProps> =({color, title,mess})=> (
    <CToast color={color}>
      <CToastHeader   closeButton>
        <svg
          className="rounded me-2"
          width="20"
          height="20"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="xMidYMid slice"
          focusable="false"
          role="img"
        >
          <rect width="100%" height="100%" fill="#007aff"></rect>
        </svg>
        <strong className="me-auto">{title}</strong>
        <small>7 min ago</small>
      </CToastHeader>
      <CToastBody>{mess}</CToastBody>
    </CToast>
  )

