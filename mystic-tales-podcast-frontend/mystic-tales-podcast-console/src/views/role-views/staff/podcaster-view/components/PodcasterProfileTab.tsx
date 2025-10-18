"use client"

import type { Account, Podcaster, PodcasterProfile } from "@/core/types"
import { formatDate } from "@/core/utils/date.util"
import PdfViewer from "@/views/components/common/pdf"
import React, { type FC, useEffect, useState } from "react"


interface PodcasterProfileProps {
  podcasterPf: PodcasterProfile
  onClose: () => void
}

const PodcasterProfileTab: FC<PodcasterProfileProps> = ({ podcasterPf, onClose }) => {
  const [podcasterProfile, setPodcasterProfile] = React.useState<PodcasterProfile>(podcasterPf)
  const [pdfViewerOpen, setPdfViewerOpen] = useState(false)
  const [currentPdfUrl, setCurrentPdfUrl] = useState("")
  const [currentPdfTitle, setCurrentPdfTitle] = useState("")

  useEffect(() => {
    setPodcasterProfile(podcasterPf)
  }, [podcasterPf])

  const handleViewDocument = (url: string, title: string) => {
    setCurrentPdfUrl(url)
    setCurrentPdfTitle(title)
    setPdfViewerOpen(true)
  }

  if (!podcasterProfile) {
    return <div className="podcaster-profile__loading">Loading podcaster profile...</div>
  }

  const storagePercentage = (podcasterProfile.UsedBookingStorageSize / podcasterProfile.OwnedBookingStorageSize) * 100


  return (
    <div className="podcaster-profile">
      <div className="podcaster-profile__header">
        <div className="podcaster-profile__title-section">
          <h2 className="podcaster-profile__title">Podcaster Profile</h2>
          <p className="podcaster-profile__subtitle">Created At: {formatDate(podcasterProfile.CreatedAt)}</p>
          <p className="podcaster-profile__subtitle">Updated At: {formatDate(podcasterProfile.UpdatedAt)}</p>
        </div>

        <div className="podcaster-profile__verification">
          <div className="podcaster-profile__verification-label">Podcaster Verification Status</div>
          <div
            className={`podcaster-profile__badge ${podcasterProfile.IsVerified === true
              ? "podcaster-profile__badge--verified"
              : podcasterProfile.IsVerified === false
                ? "podcaster-profile__badge--rejected"
                : "podcaster-profile__badge--pending"
              }`}
          >
            <span className="podcaster-profile__badge-dot"></span>
            {podcasterProfile.IsVerified === true
              ? "Verified Podcaster"
              : podcasterProfile.IsVerified === false
                ? "Rejected"
                : "Pending Verification"}
          </div>
        </div>
      </div>

      <div className="podcaster-profile__content">
        {podcasterProfile.IsVerified === true && (
          <div className="podcaster-profile__section">
            <h3 className="podcaster-profile__section-title">Description</h3>
            <div className="podcaster-profile__description">{podcasterProfile.Description}</div>
          </div>
        )}
        {/* Show rating and storage only for verified podcasters */}
        {podcasterProfile.IsVerified === true && (
          <div className="podcaster-profile__grid">

            <div className="podcaster-profile__section">
              <h3 className="podcaster-profile__section-title">Rating & Reviews</h3>
              <div className="podcaster-profile__rating-card">
                <div className="podcaster-profile__rating-main">
                  <div className="podcaster-profile__rating-score">{podcasterProfile.AverageRating.toFixed(1)}</div>
                  <div className="podcaster-profile__rating-stars">
                    {[...Array(5)].map((_, index) => (
                      <span
                        key={index}
                        className={`podcaster-profile__star ${index < Math.floor(podcasterProfile.AverageRating) ? "podcaster-profile__star--filled" : ""}`}
                      >
                        ★
                      </span>
                    ))}
                  </div>
                </div>
                <div className="podcaster-profile__rating-count">Based on {podcasterProfile.RatingCount} reviews</div>
              </div>
            </div>

            <div className="podcaster-profile__section">
              <h3 className="podcaster-profile__section-title">Storage Usage</h3>
              <div className="podcaster-profile__storage-card">
                <div className="podcaster-profile__storage-info">
                  <span className="podcaster-profile__storage-label">Used</span>
                  <span className="podcaster-profile__storage-value">{podcasterProfile.UsedBookingStorageSize} MB</span>
                </div>
                <div className="podcaster-profile__storage-bar">
                  <div className="podcaster-profile__storage-fill" style={{ width: `${storagePercentage}%` }}></div>
                </div>
                <div className="podcaster-profile__storage-info">
                  <span className="podcaster-profile__storage-label">Total</span>
                  <span className="podcaster-profile__storage-value">{podcasterProfile.OwnedBookingStorageSize} MB</span>
                </div>
                <div className="podcaster-profile__storage-percentage">{storagePercentage.toFixed(1)}% Used</div>
              </div>
            </div>
          </div>
        )}

        {/* Documents section - always show commitment, conditionally show buddy audio */}
        <div className="podcaster-profile__section">
          <h3 className="podcaster-profile__section-title">Documents & Media</h3>
          <div className="podcaster-profile__documents">
            <div className="podcaster-profile__document-card">
              <div className="podcaster-profile__document-icon">PDF</div>
              <div className="podcaster-profile__document-info">
                <div className="podcaster-profile__document-name">Commitment Document</div>
                <button
                  onClick={() => handleViewDocument(
                    podcasterProfile.CommitmentDocumentFileKey || "/sample.pdf",
                    "Commitment Document"
                  )}
                  className="podcaster-profile__document-link cursor-pointer"
                >
                  View Document
                </button>
              </div>
            </div>

            {/* Show buddy audio only for verified podcasters */}
            {podcasterProfile.IsVerified === true && (
              <div className="podcaster-profile__document-card">
                <div className="podcaster-profile__document-icon podcaster-profile__document-icon--audio">MP3</div>
                <div className="podcaster-profile__document-info">
                  <div className="podcaster-profile__document-name">Buddy Audio Introduction</div>
                  <a
                    href={podcasterProfile.BuddyAudioFileUrl}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="podcaster-profile__document-link"
                  >
                    Play Audio
                  </a>
                </div>
              </div>
            )}
          </div>
        </div>

        {/* Verification actions for pending status */}
        {podcasterProfile.IsVerified === null && (
          <div className="">
            <div className="podcaster-profile__verification-actions">
              <button
                className="podcaster-profile__btn podcaster-profile__btn--verify"
                onClick={() => {
                  // Handle verify action
                  console.log('Verify podcaster:', podcasterProfile.AccountId);
                }}
              >
                Verify Podcaster
              </button>
              <button
                className="podcaster-profile__btn podcaster-profile__btn--reject"
                onClick={() => {
                  // Handle reject action
                  console.log('Reject podcaster:', podcasterProfile.AccountId);
                }}
              >
                Reject Application
              </button>
            </div>
          </div>
        )}
      </div>
      {/* <PdfViewer
        fileUrl={currentPdfUrl}
        isOpen={pdfViewerOpen}
        onClose={() => setPdfViewerOpen(false)}
        title={currentPdfTitle}
      /> */}
    </div>
  )
}

export default PodcasterProfileTab
