import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { updateLevel, verifyPodcaster } from "@/core/services/account/account.service"
import { getBuddyCommitment, getPublicSource, getPublicSourcePodcast } from "@/core/services/file/file.service"
import type { Account, Podcaster, PodcasterProfile } from "@/core/types"
import { confirmAlert } from "@/core/utils/alert.util"
import { formatDate } from "@/core/utils/date.util"
import { useSagaPolling } from "@/hooks/useSagaPolling"
import React, { type FC, useEffect } from "react"
import { toast } from "react-toastify"
import { PodcasterViewContext } from ".."
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { SmartAudioPlayer } from "@/views/components/common/audio"


interface PodcasterProfileProps {
  account: Podcaster
  active?: boolean
  onClose?: () => void
}

const PodcasterProfileTab: FC<PodcasterProfileProps> = ({ account, active, onClose }) => {
  const context = React.useContext(PodcasterViewContext);
  const [podcasterProfile, setPodcasterProfile] = React.useState<PodcasterProfile | null>(null)
  const [buddyCommitment, setBuddyCommitment] = React.useState<string | null>(null);
  const [showPdf, setShowPdf] = React.useState(false);
  const [verifying, setVerifying] = React.useState(false);
  const [showAudioPlayer, setShowAudioPlayer] = React.useState(false);
  const [showResolvePopup, setShowResolvePopup] = React.useState(false)
  const [violationLevel, setViolationLevel] = React.useState(account.ViolationLevel.toString())
  const [loading, setLoading] = React.useState(false)
  const [loadingPdf, setLoadingPdf] = React.useState(false)
  const { startPolling } = useSagaPolling({
    timeoutSeconds: 15,
    intervalSeconds: 0.5,
  })

  useEffect(() => {
    const profile = account.PodcasterProfile;
    if (!profile) {
      setPodcasterProfile(null);
      return;
    }
    setPodcasterProfile(profile);
  }, [account.Id])

  const handleViewPdf = async () => {
    if (!podcasterProfile?.CommitmentDocumentFileKey) {
      toast.error('No document available');
      return;
    }

    if (buddyCommitment) {
      setShowPdf(true);
      return;
    }

    setLoadingPdf(true);
    try {
      const commitment = await getBuddyCommitment(adminAxiosInstance, podcasterProfile.CommitmentDocumentFileKey);
      if (commitment.success) {
        setBuddyCommitment(commitment.data.FileUrl);
        setShowPdf(true);
      } else {
        console.error('API Error:', commitment.message);
        toast.error('Failed to load document');
      }
    } catch (error) {
      console.error('Error fetching commitment document:', error);
      toast.error('Failed to load document');
    } finally {
      setLoadingPdf(false);
    }
  }

  const handleVerify = async (isVerify: boolean) => {
    const alert = confirmAlert(`Are you sure to ${isVerify ? "verify" : "reject"} this Podcaster Apply?`);
    if (!(await alert).isConfirmed) return;
    try {
      const res = await verifyPodcaster(adminAxiosInstance, account.Id, isVerify);
      const sagaId = res?.data?.SagaInstanceId
      if (!sagaId) {
        toast.error("Verify Podcaster Apply failed, please try again.")
        return
      }
      await startPolling(sagaId, adminAxiosInstance, {
        onSuccess: () => {
          onClose?.()
          context?.handleDataChange();
          toast.success(`Podcaster Apply ${isVerify ? 'verified' : 'rejected'} successfully!`);
        },
        onFailure: (err) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })
    } catch (err) {
      console.error("Error verifying Podcaster Apply:", err);
      toast.error("Error verifying Podcaster Apply");
    }
  };

  const handleResolveSubmit = async () => {

    if (!violationLevel.trim()) {
      toast.error("Please enter violation level")
      return
    }

    const violationLevelNum = Number(violationLevel)
    if (isNaN(violationLevelNum) || violationLevelNum < 0) {
      toast.error("Please enter a valid violation level")
      return
    }
    setLoading(true)
    try {
      const res = await updateLevel(adminAxiosInstance, account.Id, violationLevelNum);
      const sagaId = res?.data?.SagaInstanceId

      await startPolling(sagaId, adminAxiosInstance, {
        onSuccess: () => {
        
          onClose?.()
          context?.handleDataChange();
            toast.success(`Set violation level successfully`)
          setShowResolvePopup(false)
          setViolationLevel("")
        },
        onFailure: (err: any) => toast.error(err || "Saga failed!"),
        onTimeout: () => toast.error("System not responding, please try again."),
      })

    } catch (error) {
      console.error("Failed to set violation level:", error)
    } finally {
      setLoading(false)
    }
  }

  const handleClosePopup = () => {
    setShowResolvePopup(false)
    setViolationLevel("")
  }

  if (!podcasterProfile) {
    return <div className="podcaster-profile__loading">
      Loading podcaster profile...</div>
  }



  return (
    <div className="podcaster-profile">
      <div className="podcaster-profile__header">
        <div className="podcaster-profile__title-section">
          <h2 className="podcaster-profile__title">{podcasterProfile.Name}</h2>
          <p className="podcaster-profile__subtitle text-xs pb-1">🔔 Total Follows: {podcasterProfile.TotalFollow}</p>
          <p className="podcaster-profile__subtitle text-xs">🎧 Total Listens: {podcasterProfile.ListenCount}</p>
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
        <div className="podcaster-profile__grid">
          <div className="podcaster-profile__section">
            <h3 className="podcaster-profile__section-title">Description</h3>
            <div className="podcaster-profile__description"
              dangerouslySetInnerHTML={{ __html: renderDescriptionHTML(podcasterProfile.Description) }}
            ></div>
          </div>
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
        </div>

        <h3 className="podcaster-profile__section-title mt-4">Documents & Media</h3>
        <div className="podcaster-profile__document-card">
          <div className="podcaster-profile__document-icon podcaster-profile__document-icon--audio">MP3</div>
          <div className="podcaster-profile__document-info">
            <div className="podcaster-profile__document-name">Buddy Audio Introduction</div>
            {podcasterProfile.BuddyAudioFileKey ? (
              <>
                {!showAudioPlayer ? (
                  <button
                    onClick={() => setShowAudioPlayer(true)}
                    className="podcaster-profile__document-link flex justify-start items-center"
                  >
                    Play Audio
                  </button>
                ) : (
                  <div className="mt-2">
                    <SmartAudioPlayer
                      audioId={podcasterProfile.BuddyAudioFileKey}
                      className="w-full"
                      fetchUrlFunction={async (fileKey) => {
                        const result = await getPublicSourcePodcast(adminAxiosInstance, fileKey);
                        return {
                          success: result.success,
                          data: result.data ? { FileUrl: result.data.FileUrl } : undefined,
                          message: typeof result.message === 'string' ? result.message : result.message?.content
                        };
                      }}
                    />
                  </div>
                )}
              </>
            ) : (
              <span className="text-sm italic">No audio available</span>
            )}
          </div>
        </div>
        <div className="podcaster-profile__document-card">
          {showPdf ? (
            <button
              onClick={() => setShowPdf(false)}
              className="podcaster-profile__document-icon--close"
            >
              x
            </button>
          ) : (
            <div className="podcaster-profile__document-icon">PDF</div>
          )}
          <div className="podcaster-profile__document-info">
            <div className="podcaster-profile__document-name">Commitment Document</div>
            {podcasterProfile.CommitmentDocumentFileKey ? (
              <>
                {!showPdf ? (
                  <span>
                    <button
                      onClick={handleViewPdf}
                      className="podcaster-profile__document-link"
                      disabled={loadingPdf}
                    >
                      {loadingPdf ? 'Loading...' : 'View Document'}
                    </button>
                  </span>
                ) : (
                  <div className="mt-2 border rounded-lg overflow-hidden">
                    <iframe
                      src={buddyCommitment || ''}
                      title="Commitment Document"
                      width="100%"
                      height="600px"
                    />
                  </div>
                )}
              </>
            ) : (
              <span className="text-sm italic">No document available</span>
            )}
          </div>
        </div>



      </div>

      {podcasterProfile.IsVerified === null ? (
        <div className="mt-6 ">
          <div className="podcaster-profile__verification-actions flex gap-4">
            <button
              className="podcaster-profile__btn podcaster-profile__btn--update"
              onClick={() => handleVerify(true)}
              disabled={verifying}
            >
              Accept
            </button>
            <button
              className="podcaster-profile__btn podcaster-profile__btn--deactivate"
              onClick={() => handleVerify(false)}
              disabled={verifying}

            >
              Reject Application
            </button>
          </div>
        </div>
      ) : (
        <div className="mt-6 ">
          <div className="podcaster-profile__verification-actions flex gap-4">
            <button
              className="podcaster-profile__btn podcaster-profile__btn--update"
              onClick={() => setShowResolvePopup(true)}
              disabled={verifying}
            >
              Update Violation Level
            </button>
          </div>
        </div>
      )}

      {showResolvePopup && (
        <div className="resolve-popup-overlay" >
          <div className="resolve-popup" onClick={(e) => e.stopPropagation()}>
            <div className="resolve-popup__header">
              <h4 className="resolve-popup__title">Update Violation Level</h4>
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
                  Violation Level <span className="text-danger">*</span>
                </label>
                <input
                  type="number"
                  min="0"
                  max="4"
                  step="1"
                  className="resolve-popup__input"
                  placeholder="Enter violation level"
                  value={violationLevel}
                  onChange={(e) => setViolationLevel(e.target.value)}
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
                  {loading ? "Submitting..." : "Submit"}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default PodcasterProfileTab
