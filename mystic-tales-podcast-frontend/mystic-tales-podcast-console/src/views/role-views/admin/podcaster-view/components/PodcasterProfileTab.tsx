"use client"

import type { Account, Podcaster, PodcasterProfile } from "@/core/types"
import { formatDate } from "@/core/utils/date.util"
import React, { type FC, useEffect } from "react"

export const mockPodcaster: any = {
  PodcasterProfile: {
    AccountId: 1,
    Description:
      "Podcaster chuyên về tâm lý học và phát triển bản thân, chia sẻ những câu chuyện đời thường và bài học ý nghĩa.",
    AverageRating: 4.8,
    RatingCount: 256,
    CommitmentDocumentFileKey: "https://example.com/docs/commitment1.pdf",
    BuddyAudioFileUrl: "https://example.com/audio/intro1.mp3",
    OwnedBookingStorageSize: 5000,
    UsedBookingStorageSize: 2350,
    IsVerified: false,
    CreatedAt: "2024-03-15T10:20:30Z",
    UpdatedAt: "2025-09-20T15:45:00Z",
  },
}
interface PodcasterProfileProps {
  account: Account
  active?: boolean
  refreshKey?: number
}

const PodcasterProfileTab: FC<PodcasterProfileProps> = ({ account, active, refreshKey }) => {
  const [podcasterProfile, setPodcasterProfile] = React.useState<PodcasterProfile | null>(null)

  const fetchPodcasterProfile = async () => {
    // const profileData = await getPodcasterProfile(account.id);
    setPodcasterProfile(mockPodcaster.PodcasterProfile)
  }

  useEffect(() => {
    if (active) {
      console.log("Fetching podcaster profile for account ID:", account.Id)
      fetchPodcasterProfile()
    }
  }, [active, account.Id, refreshKey])

  if (!podcasterProfile) {
    return <div className="podcaster-profile__loading">Loading podcaster profile...</div>
  }

  const storagePercentage = (podcasterProfile.UsedBookingStorageSize / podcasterProfile.OwnedBookingStorageSize) * 100


  return (
    <div className="podcaster-profile">
      <div className="podcaster-profile__header">
        <div className="podcaster-profile__title-section">
          <h2 className="podcaster-profile__title">Podcaster Profile</h2>
          <p className="podcaster-profile__subtitle">Professional podcaster verification and management</p>
        </div>

        <div className="podcaster-profile__verification">
          <div className="podcaster-profile__verification-label">Podcaster Verification Status</div>
          <div
            className={`podcaster-profile__badge podcaster-profile__badge--${podcasterProfile.IsVerified ? "verified" : "pending"}`}
          >
            <span className="podcaster-profile__badge-dot"></span>
            {podcasterProfile.IsVerified ? "Verified Podcaster" : "Pending Verification"}
          </div>
        </div>
      </div>

      <div className="podcaster-profile__content">
        <div className="podcaster-profile__section">
          <h3 className="podcaster-profile__section-title">Description</h3>
          <div className="podcaster-profile__description">{podcasterProfile.Description}</div>
        </div>

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

        <div className="podcaster-profile__section">
          <h3 className="podcaster-profile__section-title">Documents & Media</h3>
          <div className="podcaster-profile__documents">
            <div className="podcaster-profile__document-card">
              <div className="podcaster-profile__document-icon">PDF</div>
              <div className="podcaster-profile__document-info">
                <div className="podcaster-profile__document-name">Commitment Document</div>
                <a
                  href={podcasterProfile.CommitmentDocumentFileKey}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="podcaster-profile__document-link"
                >
                  View Document
                </a>
              </div>
            </div>

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
          </div>
        </div>

        <div className="podcaster-profile__section">
          <div className="podcaster-profile__timeline">
             <div className="podcaster-profile__timeline-item">
              <div className="podcaster-profile__timeline-dot"></div>
              <div className="podcaster-profile__timeline-content">
                <div className="podcaster-profile__timeline-label">Updated At</div>
                <div className="podcaster-profile__timeline-date">{podcasterProfile.UpdatedAt === null ? "N/A" : formatDate(podcasterProfile.UpdatedAt)}</div>
              </div>
            </div>
            <div className="podcaster-profile__timeline-item">
              <div className="podcaster-profile__timeline-dot"></div>
              <div className="podcaster-profile__timeline-content">
                <div className="podcaster-profile__timeline-label">Created At</div>
                <div className="podcaster-profile__timeline-date">{podcasterProfile.CreatedAt === null ? "N/A" : formatDate(podcasterProfile.CreatedAt)}</div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  )
}

export default PodcasterProfileTab
