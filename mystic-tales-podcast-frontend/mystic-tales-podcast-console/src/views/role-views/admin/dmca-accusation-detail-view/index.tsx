
import type { Account, DMCAAccusationDetail } from "@/core/types"
import { createContext, type FC, useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import "./styles.scss"
import Loading from "@/views/components/common/loading"
import { assignStaff, getCounterNoticeFile, getDMCADetail, getDMCANoticeFile, getDMCAReport, getLawsuitProofFile, validateReport } from "@/core/services/dmca/dmca.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { getStaffAccounts } from "@/core/services/account/account.service"
import Image from "@/views/components/common/image"
import { formatDate } from "@/core/utils/date.util"
import { useSagaPolling } from "@/hooks/useSagaPolling"
import { toast } from "react-toastify"
import { set } from "lodash"
import { confirmAlert } from "@/core/utils/alert.util"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import { Plus } from "phosphor-react"
import CounterModal from "./CounterModal"
import LawsuitModal from "./LawsuitModal"



type DMCAAccusationDetailViewProps = {}
interface DMCAAccusationDetailViewContextProps {
    handleDataChange: () => void;
    dmcaAccusationId: number
}
export const DMCAAccusationDetailViewContext = createContext<DMCAAccusationDetailViewContextProps | null>(null)

const DMCAAccusationDetailView: FC<DMCAAccusationDetailViewProps> = () => {
    const { id } = useParams<{ id: string }>()
    const { type } = useParams<{ type: string }>()
    const [DMCAAccusation, setDMCAAccusation] = useState<DMCAAccusationDetail | null>(null)
    const [assignedStaff, setAssignedStaff] = useState<any | null>(null)
    const [reportList, setReportList] = useState<any[]>([])
    const [staffList, setStaffList] = useState<Account[]>([])
    const [loading, setLoading] = useState(true)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [selectedStaffId, setSelectedStaffId] = useState<number | null>(null)
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 5,
        intervalSeconds: 0.5,
    })
    const [viewingNoticeFile, setViewingNoticeFile] = useState<{ id: number, url: string } | null>(null)
    const [viewingCounterFile, setViewingCounterFile] = useState<{ id: number, url: string } | null>(null)
    const [viewingLawsuitFile, setViewingLawsuitFile] = useState<{ id: number, url: string } | null>(null)

    const fetchDMCAAccusation = async () => {
        if (id) {
            setLoading(true)
            try {
                const response = await getDMCADetail(adminAxiosInstance, Number(id));
                if (response.success && response.data) {
                    setDMCAAccusation(response.data.DMCAAccusation);
                    console.log("Fetched DMCA Accusation:", response.data.DMCAAccusation);
                } else {
                    console.error('API Error:', response.message);
                    return;
                }
                const assignedStaff = response.data.DMCAAccusation?.AssignedStaff;
                if (assignedStaff !== null) {
                    setAssignedStaff(assignedStaff)
                } else {
                    await fetchStaffListToAssign()
                }
                const res = await getDMCAReport(adminAxiosInstance, Number(id));
                console.log("Fetched DMCA :", res);
                if (res.success && res.data) {
                    setReportList(res.data.DMCAAccusationConclusionReportList || []);
                } else {
                    console.error('API Error:', response.message);
                    return;
                }
            } catch (error) {
                console.error("Error fetching accusation data:", error)
            } finally {
                setLoading(false)
            }
        }
    }

    const fetchStaffListToAssign = async () => {
        try {
            const response = await getStaffAccounts(adminAxiosInstance);
            if (response.success && response.data) {
                setStaffList(response.data.StaffList);
            }
        } catch (error) {
            console.error("Error fetching accusation data:", error)
        }
    }

    const handleAssignStaff = async () => {
        setIsSubmitting(true)
        try {
            if (selectedStaffId && DMCAAccusation) {
                const response = await assignStaff(adminAxiosInstance, selectedStaffId, DMCAAccusation.Id)
                const sagaId = response?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Assign staff failed, please try again.")
                    return
                }
                await startPolling(sagaId, adminAxiosInstance, {
                    onSuccess: () => {
                        toast.success('Assign staff successfully')
                        fetchDMCAAccusation()
                    },
                    onFailure: (err: any) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            }
        } catch (error) {
            console.error('Error assigning staff:', error)
            toast.error('Failed to assign staff')
        } finally {
            setIsSubmitting(false)
        }

    }

    const handleViewNoticeFile = async (fileId: number, fileKey: string) => {
        try {
            const response = await getDMCANoticeFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                setViewingNoticeFile({ id: fileId, url: response.data.FileUrl })
            }
        } catch (error) {
            console.error('Error fetching PDF:', error)
            toast.error('Failed to load PDF')
        }
    }

    const handleViewCounterFile = async (fileId: number, fileKey: string) => {
        try {
            const response = await getCounterNoticeFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                setViewingCounterFile({ id: fileId, url: response.data.FileUrl })
            }
        } catch (error) {
            console.error('Error fetching PDF:', error)
            toast.error('Failed to load PDF')
        }
    }

    const handleViewLawsuitFile = async (fileId: number, fileKey: string) => {
        try {
            const response = await getLawsuitProofFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                setViewingLawsuitFile({ id: fileId, url: response.data.FileUrl })
            }
        } catch (error) {
            console.error('Error fetching PDF:', error)
            toast.error('Failed to load PDF')
        }
    }
    const handleVerifyReport = async (isValid: boolean) => {
        const alert = await confirmAlert("Are you sure ?");
        if (!alert.isConfirmed) return;
        setIsSubmitting(true)
        try {
            const response = await validateReport(adminAxiosInstance, reportList[0].Id, isValid)
            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Verify report failed, please try again.")
                return
            }
            await startPolling(sagaId, adminAxiosInstance, {
                onSuccess: () => {
                    toast.success('Verify report successfully')
                    fetchDMCAAccusation()
                },
                onFailure: (err: any) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            console.error('Error verifying report :', error)
            toast.error('Failed to verify report ')
        } finally {
            setIsSubmitting(false)
        }
    }
    const handleDownloadDMCANotice = async (fileKey: string, fileName: string) => {
        try {
            const response = await getDMCANoticeFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                const link = document.createElement('a')
                link.href = response.data.FileUrl
                link.download = fileName
                link.target = '_blank'
                document.body.appendChild(link)
                link.click()
                document.body.removeChild(link)
            }
        } catch (error) {
            console.error('Error downloading file:', error)
            toast.error('Failed to download file')
        }
    }
    const handleDownloadCounterNotice = async (fileKey: string, fileName: string) => {
        try {
            const response = await getCounterNoticeFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                const link = document.createElement('a')
                link.href = response.data.FileUrl
                link.download = fileName
                link.target = '_blank'
                document.body.appendChild(link)
                link.click()
                document.body.removeChild(link)
            }
        } catch (error) {
            console.error('Error downloading file:', error)
            toast.error('Failed to download file')
        }
    }
    const handleDownloadLawsuit = async (fileKey: string, fileName: string) => {
        try {
            const response = await getLawsuitProofFile(adminAxiosInstance, fileKey)
            if (response.success && response.data) {
                const link = document.createElement('a')
                link.href = response.data.FileUrl
                link.download = fileName
                link.target = '_blank'
                document.body.appendChild(link)
                link.click()
                document.body.removeChild(link)
            }
        } catch (error) {
            console.error('Error downloading file:', error)
            toast.error('Failed to download file')
        }
    }
    useEffect(() => {
        fetchDMCAAccusation()
    }, [id])

    if (loading) {
        return (
            <div className="flex justify-content-center align-items-center h-150" >
                <Loading />
            </div>
        )
    }
    if (!DMCAAccusation) {
        return <div className="text-center text-danger">No DMCA accusation data found</div>
    }

    return (
        <DMCAAccusationDetailViewContext.Provider value={{ handleDataChange: fetchDMCAAccusation, dmcaAccusationId: DMCAAccusation.Id }}>
            <div className="dmca-detail">
                <div className="dmca-detail__header flex justify-content-between align-items-center ">
                    <div>
                        <h1 className="dmca-detail__title">DMCA Accusation #{DMCAAccusation.Id}</h1>
                        <div className="dmca-detail__meta">
                            <span className="dmca-detail__meta-item">Created At: {formatDate(DMCAAccusation.CreatedAt)}</span>
                            <span className="dmca-detail__meta-item">Last Updated: {formatDate(DMCAAccusation.UpdatedAt || DMCAAccusation.CreatedAt)}</span>
                        </div>
                    </div>
                    <div className="dmca-detail__verification">
                        <div
                            className={`dmca-detail__badge dmca-detail__badge--${['Valid DMCA Notice', 'Valid Counter Notice', 'Valid Lawsuit Proof', 'Podcaster Lawsuit Win', 'Accuser Lawsuit Win'].includes(DMCAAccusation.CurrentStatus.Name)
                                ? 'verified'
                                : ['Pending DMCA Notice Review'].includes(DMCAAccusation.CurrentStatus.Name)
                                    ? 'pending'
                                    : 'invalid'
                                }`}
                        >
                            <span className="dmca-detail__badge-dot"></span>
                            {DMCAAccusation.CurrentStatus.Name}
                        </div>
                    </div>
                </div>
                {DMCAAccusation.DismissReason && (
                    <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                        <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                            <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                        </svg>
                        <span className="text-xs text-red-700 font-medium">
                            <strong>Dismiss Reason:</strong> {DMCAAccusation.DismissReason}
                        </span>
                    </div>
                )}
                {/* Assigned Staff Section */}
                <div className="dmca-detail__section">
                    <h2 className="dmca-detail__section-title">Staff Assignment</h2>
                    <div className="dmca-detail__card">
                        {assignedStaff ? (
                            <div className="staff-info">
                                <div className="staff-info__header">
                                    <Image
                                        mainImageFileKey={assignedStaff.MainImageFileKey}
                                        alt={assignedStaff.FullName}
                                        className="staff-info__avatar"
                                    />
                                    <div className="staff-info__details">
                                        <h3 className="staff-info__name">{assignedStaff.FullName}</h3>
                                        <div className="staff-info__contact-item">
                                            <span className="staff-info__label">Email:</span>
                                            <span className="staff-info__value">{assignedStaff.Email}</span>
                                        </div>
                                    </div>
                                </div>

                            </div>
                        ) : (
                            <>
                                {
                                    DMCAAccusation.CurrentStatus.Id !== 10 && DMCAAccusation.CurrentStatus.Id !== 11 && (
                                        <div className="staff-assign">
                                            <h3 className="staff-assign__title">Assign Staff Member</h3>
                                            <p className="staff-assign__description">
                                                This case has not been assigned yet. Please select a staff member to handle this DMCA accusation.
                                            </p>
                                            <div className="staff-assign__list">
                                                {staffList.map((staff) => (
                                                    <div
                                                        key={staff.Id}
                                                        className={`staff-assign__item ${selectedStaffId === staff.Id ? "staff-assign__item--selected" : ""}`}
                                                        onClick={() => setSelectedStaffId(staff.Id)}
                                                    >
                                                        <Image
                                                            mainImageFileKey={staff.MainImageFileKey}
                                                            alt={staff.FullName}
                                                            className="staff-assign__avatar"
                                                        />
                                                        <div className="staff-assign__info">
                                                            <h4 className="staff-assign__name">{staff.FullName}</h4>
                                                            <p className="staff-assign__email">{staff.Email}</p>
                                                            <p className="staff-assign__role">Id: #{staff.Id}</p>
                                                        </div>
                                                    </div>
                                                ))}
                                            </div>
                                            <button className="staff-assign__button" onClick={handleAssignStaff} disabled={!selectedStaffId || isSubmitting}>
                                                {isSubmitting ? "Assigning..." : "Assign Selected Staff"}
                                            </button>
                                        </div>
                                    )
                                }
                            </>
                        )}
                    </div>
                </div>

                {/* Podcast Content Section */}
                <div className="dmca-detail__section">
                    <div className="dmca-detail__content-grid">
                        <div>
                            <h2 className="dmca-detail__section-title">Accuser</h2>
                            <div className="content-card">
                                <div className="content-card__body w-full">
                                    <div className=" flex justify-between border-b border-[#d9d9d9]  mb-4">
                                        <span className="content-card__stat text-black">Full Name: </span>
                                        <p className="content-card__description">{DMCAAccusation.AccuserFullName}</p>
                                    </div>
                                    <div className=" flex justify-between border-b border-[#d9d9d9]  mb-4">
                                        <span className="content-card__stat text-black">Email: </span>
                                        <p className="content-card__description">{DMCAAccusation.AccuserEmail}</p>
                                    </div>
                                    <div className=" flex justify-between border-b border-[#d9d9d9]">
                                        <span className="content-card__stat text-black">Phone: </span>
                                        <p className="content-card__description">{DMCAAccusation.AccuserPhone}</p>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div>
                            <h2 className="dmca-detail__section-title">Accused Content</h2>
                            {type === "show" && DMCAAccusation.PodcastShow && (
                                <div className="content-card">
                                    <Image
                                        mainImageFileKey={DMCAAccusation.PodcastShow.MainImageFileKey}
                                        alt={DMCAAccusation.PodcastShow.Name}
                                        className="content-card__image"
                                    />
                                    <div className="content-card__body">
                                        <span className="content-card__type">Show</span>
                                        <h3 className="content-card__title">{DMCAAccusation.PodcastShow.Name}</h3>
                                        {/* <p className="content-card__description">{DMCAAccusation.PodcastShow.Description}</p> */}
                                        {/* <div className="content-card__stats">
                                    <span className="content-card__stat">{DMCAAccusation.PodcastShow.ListenCount.toLocaleString()} listens</span>
                                </div> */}
                                    </div>
                                </div>
                            )}
                            {DMCAAccusation.PodcastEpisode && type === "episode" && (
                                <div className="content-card">
                                    <Image
                                        mainImageFileKey={DMCAAccusation.PodcastEpisode.MainImageFileKey}
                                        alt={DMCAAccusation.PodcastEpisode.Name}
                                        className="content-card__image"
                                    />
                                    <div className="content-card__body">
                                        <span className="content-card__type">Episode</span>
                                        <h3 className="content-card__title">{DMCAAccusation.PodcastEpisode.Name}</h3>
                                        {/* <p className="content-card__description">{DMCAAccusation.PodcastEpisode.Description}</p> */}
                                        {/* <div className="content-card__stats">
                                    <span className="content-card__stat">{podcastEpisode.ListenCount.toLocaleString()} listens</span>
                                </div> */}
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>

                {/* DMCA Notice Section */}
                {DMCAAccusation.DMCANotice && (
                    <div className="dmca-detail__section">
                        <h2 className="dmca-detail__section-title">DMCA Notice</h2>
                        <div className="dmca-detail__card">
                            <div className="notice">
                                <div className="notice__header">
                                    <span
                                        className={`notice__status notice__status--${DMCAAccusation.DMCANotice.IsValid ? "valid" : DMCAAccusation.DMCANotice.IsValid === false ? "invalid" : "pending"}`}
                                    >
                                        {DMCAAccusation.DMCANotice.IsValid ? "Valid Notice"
                                            : DMCAAccusation.DMCANotice.IsValid === false ? "Invalid Notice" : "Pending Review"}
                                    </span>
                                    <span className="notice__id">ID: {DMCAAccusation.DMCANotice.Id}</span>
                                </div>
                                {reportList.length > 0 && reportList[0].IsRejected === null && reportList[0].CancelledAt === null && reportList[0].DmcaAccusationConclusionReportType.Id === 1 ? (
                                    <>
                                        <div className="flex justify-around items-center mt-2">
                                            <div className="notice__field">
                                                <span className="notice__label ">Invalid Reason</span>
                                                <span className="notice__value">{reportList[0].InvalidReason}</span>
                                            </div>
                                            <div className="notice__field">
                                                <span className="notice__label">Description</span>
                                                <span className="notice__value">{reportList[0].Description || "---"}</span>
                                            </div>
                                        </div>
                                        <div className="w-full flex flex-col gap-4 justify-center items-center border-t border-[#f0f0f0] pt-4 mt-3">
                                            <span className="notice_value">Please verify Staff Report</span>
                                            <div className="flex gap-4">
                                                <button
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                    onClick={() => handleVerifyReport(true)}
                                                    disabled={isSubmitting}
                                                >
                                                    Accept
                                                </button>
                                                <button
                                                    onClick={() => handleVerifyReport(false)}
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                    disabled={isSubmitting}
                                                >
                                                    Reject
                                                </button>
                                            </div>
                                        </div>
                                    </>
                                ) : (
                                    <div className="notice__grid">
                                        <div className="notice__field">
                                            <span className="notice__label">Invalid Reason :</span>
                                            <span className="notice__value">{DMCAAccusation.DMCANotice.InValidReason || "---"}</span>
                                        </div>
                                        <div className="notice__field">
                                            <span className="notice__label">Validated By:</span>
                                            <span className="notice__value">{DMCAAccusation.DMCANotice.ValidatedBy || "---"}</span>
                                        </div>
                                        <div className="notice__field">
                                            <span className="notice__label">Validated At:</span>
                                            <span className="notice__value">{formatDate(DMCAAccusation.DMCANotice.ValidatedAt) || "---"}</span>
                                        </div>
                                    </div>

                                )}

                                <div className="notice__attachments">
                                    <span className="notice__label">Attachments:</span>
                                    {DMCAAccusation.DMCANotice.DMCANoticeAttachFileList &&
                                        DMCAAccusation.DMCANotice.DMCANoticeAttachFileList.length > 0 ? (
                                        <div className="attachments">
                                            {DMCAAccusation.DMCANotice.DMCANoticeAttachFileList.map((file: any, index: number) => (
                                                <div key={file.Id} className="dmca-detail__document-card">
                                                    {viewingNoticeFile?.id === file.Id ? (
                                                        <div className="dmca-detail__document-actions">
                                                            <button
                                                                onClick={() => setViewingNoticeFile(null)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                            >
                                                                Close
                                                            </button>
                                                            <button
                                                                onClick={() => handleDownloadDMCANotice(file.AttachFileKey, `DMCA_NOTICE_${index + 1}.pdf`)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                            >
                                                                Download
                                                            </button>
                                                        </div>
                                                    ) : (
                                                        <div className="dmca-detail__document-icon">PDF</div>
                                                    )}
                                                    <div className="dmca-detail__document-info">
                                                        <div className="dmca-detail__document-name">DMCA Notice Attachment {index + 1}</div>
                                                        {viewingNoticeFile?.id === file.Id ? (
                                                            <div className="mt-2 border rounded-lg overflow-hidden">
                                                                <iframe
                                                                    src={viewingNoticeFile?.url}
                                                                    title={`DMCA Notice Attachment ${index + 1}`}
                                                                    width="100%"
                                                                    height="400px"
                                                                />
                                                            </div>
                                                        ) : (
                                                            <button
                                                                onClick={() => handleViewNoticeFile(file.Id, file.AttachFileKey)}
                                                                className="dmca-detail__document-link"
                                                            >
                                                                View Document
                                                            </button>
                                                        )}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <span className="text-muted ml-2">No attachments provided</span>
                                    )}
                                </div>

                            </div>
                        </div >
                    </div >
                )}
                {/* Counter Notice Section */}
                {DMCAAccusation.CounterNotice && (
                    <div className="dmca-detail__section">
                        <h2 className="dmca-detail__section-title">Counter Notice</h2>
                        <div className="dmca-detail__card">
                            <div className="notice">
                                <div className="notice__header">
                                    <span
                                        className={`notice__status notice__status--${DMCAAccusation.CounterNotice.IsValid ? "valid" : DMCAAccusation.CounterNotice.IsValid === false ? "invalid" : "pending"}`}
                                    >
                                        {DMCAAccusation.CounterNotice.IsValid ? "Valid Notice"
                                            : DMCAAccusation.CounterNotice.IsValid === false ? "Invalid Notice" : "Pending Review"}
                                    </span>
                                    <span className="notice__id">ID: {DMCAAccusation.CounterNotice.Id}</span>
                                </div>
                                {reportList.length > 0 && reportList[0].IsRejected === null && reportList[0].CancelledAt === null && reportList[0].DmcaAccusationConclusionReportType.Id === 2 ? (
                                    <>
                                        <div className="flex justify-around items-center mt-2">
                                            <div className="notice__field">
                                                <span className="notice__label ">Invalid Reason</span>
                                                <span className="notice__value">{reportList[0].InvalidReason}</span>
                                            </div>
                                            <div className="notice__field">
                                                <span className="notice__label">Description</span>
                                                <span className="notice__value">{reportList[0].Description || "---"}</span>
                                            </div>
                                        </div>
                                        <div className="w-full flex flex-col gap-4 justify-center items-center border-t border-[#f0f0f0] pt-4 mt-3">
                                            <span className="notice_value">Please verify Staff Report</span>
                                            <div className="flex gap-4">
                                                <button
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                    onClick={() => handleVerifyReport(true)}
                                                    disabled={isSubmitting}
                                                >
                                                    Accept
                                                </button>
                                                <button
                                                    onClick={() => handleVerifyReport(false)}
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                    disabled={isSubmitting}
                                                >
                                                    Reject
                                                </button>
                                            </div>
                                        </div>
                                    </>
                                ) : (
                                    <div className="notice__grid">
                                        <div className="notice__field">
                                            <span className="notice__label">Invalid Reason :</span>
                                            <span className="notice__value">{DMCAAccusation.CounterNotice.InValidReason || "---"}</span>
                                        </div>
                                        <div className="notice__field">
                                            <span className="notice__label">Validated By:</span>
                                            <span className="notice__value">{DMCAAccusation.CounterNotice.ValidatedBy || "---"}</span>
                                        </div>
                                        <div className="notice__field">
                                            <span className="notice__label">Validated At:</span>
                                            <span className="notice__value">{formatDate(DMCAAccusation.CounterNotice.ValidatedAt) || "---"}</span>
                                        </div>
                                    </div>

                                )}

                                <div className="notice__attachments">
                                    <span className="notice__label">Attachments:</span>
                                    {DMCAAccusation.CounterNotice.CounterNoticeAttachFileList &&
                                        DMCAAccusation.CounterNotice.CounterNoticeAttachFileList.length > 0 ? (
                                        <div className="attachments">
                                            {DMCAAccusation.CounterNotice.CounterNoticeAttachFileList.map((file: any, index: number) => (
                                                <div key={file.Id} className="dmca-detail__document-card">
                                                    {viewingCounterFile?.id === file.Id ? (
                                                        <div className="dmca-detail__document-actions">
                                                            <button
                                                                onClick={() => setViewingCounterFile(null)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                            >
                                                                Close
                                                            </button>
                                                            <button
                                                                onClick={() => handleDownloadCounterNotice(file.AttachFileKey, `COUNTER_NOTICE_${index + 1}.pdf`)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                            >
                                                                Download
                                                            </button>
                                                        </div>
                                                    ) : (
                                                        <div className="dmca-detail__document-icon">PDF</div>
                                                    )}
                                                    <div className="dmca-detail__document-info">
                                                        <div className="dmca-detail__document-name">Counter Notice Attachment {index + 1}</div>
                                                        {viewingCounterFile?.id === file.Id ? (
                                                            <div className="mt-2 border rounded-lg overflow-hidden">
                                                                <iframe
                                                                    src={viewingCounterFile?.url}
                                                                    title={`Counter Notice Attachment ${index + 1}`}
                                                                    width="100%"
                                                                    height="400px"
                                                                />
                                                            </div>
                                                        ) : (
                                                            <button
                                                                onClick={() => handleViewCounterFile(file.Id, file.AttachFileKey)}
                                                                className="dmca-detail__document-link"
                                                            >
                                                                View Document
                                                            </button>
                                                        )}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <span className="text-muted ml-2">No attachments provided</span>
                                    )}
                                </div>

                            </div>
                        </div >
                    </div >
                )
                }

                {/* Lawsuit Proof Section */}
                {DMCAAccusation.LawsuitProof && (
                    <div className="dmca-detail__section">
                        <h2 className="dmca-detail__section-title">Lawsuit Proof</h2>
                        <div className="dmca-detail__card">
                            <div className="notice">
                                <div className="notice__header">
                                    <span
                                        className={`notice__status notice__status--${DMCAAccusation.LawsuitProof.IsValid ? "valid" : DMCAAccusation.LawsuitProof.IsValid === false ? "invalid" : "pending"}`}
                                    >
                                        {DMCAAccusation.LawsuitProof.IsValid ? "Valid Lawsuit"
                                            : DMCAAccusation.LawsuitProof.IsValid === false ? "Invalid Lawsuit" : "Pending Review"}
                                    </span>
                                    <span className="notice__id">ID: {DMCAAccusation.LawsuitProof.Id}</span>
                                </div>
                                {reportList.length > 0 && reportList[0].IsRejected === null && reportList[0].CancelledAt === null && reportList[0].DmcaAccusationConclusionReportType.Id === 3 ? (
                                    <>
                                        <div className="flex justify-around items-center mt-2">
                                            <div className="notice__field">
                                                <span className="notice__label ">Invalid Reason</span>
                                                <span className="notice__value">{reportList[0].InvalidReason}</span>
                                            </div>
                                            <div className="notice__field">
                                                <span className="notice__label">Description</span>
                                                <span className="notice__value">{reportList[0].Description || "---"}</span>
                                            </div>
                                        </div>
                                        <div className="w-full flex flex-col gap-4 justify-center items-center border-t border-[#f0f0f0] pt-4 mt-3">
                                            <span className="notice_value">Please verify Staff Report</span>
                                            <div className="flex gap-4">
                                                <button
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                    onClick={() => handleVerifyReport(true)}
                                                    disabled={isSubmitting}
                                                >
                                                    Accept
                                                </button>
                                                <button
                                                    onClick={() => handleVerifyReport(false)}
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                    disabled={isSubmitting}
                                                >
                                                    Reject
                                                </button>
                                            </div>
                                        </div>
                                    </>
                                ) : (
                                    <div className="notice__grid">
                                        {DMCAAccusation.CurrentStatus.Id === 8 || DMCAAccusation.CurrentStatus.Id === 9 ? (
                                            <>
                                                <div className="notice__field">
                                                    <span className="notice__label">Result :</span>
                                                    <span className="notice__value">{DMCAAccusation.CurrentStatus.Id === 8 ? "Podcaster Win" : "Accuser Win"}</span>
                                                </div>
                                            </>
                                        ) : (
                                            <div className="notice__field">
                                                <span className="notice__label">Invalid Reason :</span>
                                                <span className="notice__value">{DMCAAccusation.LawsuitProof.InValidReason || "---"}</span>
                                            </div>
                                        )}

                                        <div className="notice__field">
                                            <span className="notice__label">Validated By:</span>
                                            <span className="notice__value">{DMCAAccusation.LawsuitProof.ValidatedBy || "---"}</span>
                                        </div>
                                        <div className="notice__field">
                                            <span className="notice__label">Validated At:</span>
                                            <span className="notice__value">{formatDate(DMCAAccusation.LawsuitProof.ValidatedAt) || "---"}</span>
                                        </div>
                                    </div>

                                )}

                                <div className="notice__attachments">
                                    <span className="notice__label">Attachments:</span>
                                    {DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList &&
                                        DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList.length > 0 ? (
                                        <div className="attachments">
                                            {DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList.map((file: any, index: number) => (
                                                <div key={file.Id} className="dmca-detail__document-card">
                                                    {viewingLawsuitFile?.id === file.Id ? (
                                                        <div className="dmca-detail__document-actions">
                                                            <button
                                                                onClick={() => setViewingLawsuitFile(null)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                            >
                                                                Close
                                                            </button>
                                                            <button
                                                                onClick={() => handleDownloadLawsuit(file.AttachFileKey, `LAWSUIT_PROOF_${index + 1}.pdf`)}
                                                                className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                            >
                                                                Download
                                                            </button>
                                                        </div>
                                                    ) : (
                                                        <div className="dmca-detail__document-icon">PDF</div>
                                                    )}
                                                    <div className="dmca-detail__document-info">
                                                        <div className="dmca-detail__document-name">Lawsuit Proof Attachment {index + 1}</div>
                                                        {viewingLawsuitFile?.id === file.Id ? (
                                                            <div className="mt-2 border rounded-lg overflow-hidden">
                                                                <iframe
                                                                    src={viewingLawsuitFile?.url}
                                                                    title={`Lawsuit Proof Attachment ${index + 1}`}
                                                                    width="100%"
                                                                    height="400px"
                                                                />
                                                            </div>
                                                        ) : (
                                                            <button
                                                                onClick={() => handleViewLawsuitFile(file.Id, file.AttachFileKey)}
                                                                className="dmca-detail__document-link"
                                                            >
                                                                View Document
                                                            </button>
                                                        )}
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    ) : (
                                        <span className="text-muted ml-2">No attachments provided</span>
                                    )}
                                </div>
                                {DMCAAccusation.CurrentStatus.Id === 7 && reportList.length > 0 && reportList[0].IsRejected === null && reportList[0].CancelledAt === null && (
                                    <>
                                        <div className="w-full flex flex-col gap-4 justify-center items-center border-t border-[#f0f0f0] pt-4 mt-3">
                                            <div className="flex items-center flex-col gap-2">
                                                <span className="text-sm !font-light !none ">Please verify result</span>
                                                <span className="notice__value !font-medium !text-lg">{reportList[0].DmcaAccusationConclusionReportType.Name}</span>
                                            </div>
                                            <div className="flex gap-4">
                                                <button
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--download"
                                                    onClick={() => handleVerifyReport(true)}
                                                    disabled={isSubmitting}
                                                >
                                                    Accept
                                                </button>
                                                <button
                                                    onClick={() => handleVerifyReport(false)}
                                                    className="dmca-detail__document-btn dmca-detail__document-btn--close"
                                                    disabled={isSubmitting}
                                                >
                                                    Reject
                                                </button>
                                            </div>
                                        </div>
                                    </>

                                )}
                            </div>
                        </div >
                    </div >
                )
                }
                {
                    DMCAAccusation.CounterNotice === null && DMCAAccusation.CurrentStatus.Id === 3 && (
                        <div className="dmca-detail__section">
                            <h2 className="dmca-detail__section-title">Counter Notice</h2>
                            <div className="flex justify-center items-center">
                                <Modal_Button
                                    color="success"
                                    className="dmca-detail__document-btn dmca-detail__document-btn--download "
                                    size="lg"
                                    title=''
                                    content={
                                        <div className="flex align-items-center p-2">
                                            <Plus size={20} weight="bold" className="me-2" />  Add Counter Notice
                                        </div>
                                    }
                                >
                                    <CounterModal onClose={() => { }} />
                                </Modal_Button>

                            </div>
                        </div>
                    )
                }
                {
                    DMCAAccusation.LawsuitProof === null && DMCAAccusation.CurrentStatus.Id === 5 && (
                        <div className="dmca-detail__section">
                            <h2 className="dmca-detail__section-title">Lawsuit Proof</h2>
                            <div className="flex justify-center items-center">
                                <Modal_Button
                                    color="success"
                                    className="dmca-detail__document-btn dmca-detail__document-btn--download "
                                    size="lg"
                                    title=''
                                    content={
                                        <div className="flex align-items-center p-2">
                                            <Plus size={20} weight="bold" className="me-2" />  Add Lawsuit Proof
                                        </div>
                                    }
                                >
                                    <LawsuitModal onClose={() => { }} />
                                </Modal_Button>

                            </div>
                        </div>
                    )
                }
            </div >
        </DMCAAccusationDetailViewContext.Provider >
    )
}

export default DMCAAccusationDetailView
