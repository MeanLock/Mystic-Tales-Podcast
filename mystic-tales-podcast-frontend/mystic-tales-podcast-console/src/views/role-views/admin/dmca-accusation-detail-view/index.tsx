
import type { Account, DMCAAccusationDetail } from "@/core/types"
import type { PodcastEpisode } from "@/core/types/podcast-episode"
import type { PodcastShow } from "@/core/types/podcast-show"
import CircularProgress from "@mui/material/CircularProgress"
import { createContext, type FC, useEffect, useState } from "react"
import { useParams } from "react-router-dom"
import "./styles.scss"

export const mockDMCAAccusation: any = {
    DMCAAccusation: {
        Id: 1,
        PodcastShowId: "a7cbe7d1-91f2-4e8a-b345-7f3b9b123456",
        PodcastEpisodeId: "b4c9f3e1-23a5-4cd1-a912-9e3c8a654321",
        AssignedStaff: null,
        LastLawsuitCheckingAlertAt: "2025-10-06T07:19:13.588Z",
        CreatedAt: "2025-09-30T09:45:00.000Z",
        UpdatedAt: "2025-10-06T07:19:13.588Z",

        DMCANotice: {
            Id: "d1f3b5c6-1234-4bcd-8ef0-112233445566",
            PodcastShowId: "a7cbe7d1-91f2-4e8a-b345-7f3b9b123456",
            PodcastEpisodeId: "b4c9f3e1-23a5-4cd1-a912-9e3c8a654321",
            AccountId: 201,
            AccountEmail: "claimant@mystictales.com",
            AccountPhone: "+1-202-555-0173",
            GoodFaithStatement: "I have a good faith belief that this episode infringes my copyrighted material.",
            WorkClaimed: "Original background music used without permission.",
            Signature: "John Doe",
            IsValid: true,
            InValidReason: "",
            ValidatedBy: 301,
            ValidatedAt: "2025-10-01T12:00:00.000Z",
            DmcaAccusationId: 1,
            CreatedAt: "2025-09-29T08:30:00.000Z",
            UpdatedAt: "2025-10-01T12:00:00.000Z",
            DMCANoticeAttachFileList: [
                {
                    Id: "file-1",
                    AttachFileKey: "https://cdn.mystictales.com/dmca/notice/audio-proof.mp3",
                    CreatedAt: "2025-09-29T08:31:00.000Z",
                },
                {
                    Id: "file-2",
                    AttachFileKey: "https://cdn.mystictales.com/dmca/notice/screenshot-proof.png",
                    CreatedAt: "2025-09-29T08:31:30.000Z",
                },
            ],
        },

        CounterNotice: {
            Id: "e2c5d6f7-5678-4abc-9def-223344556677",
            AccountId: 202,
            AccountEmail: "creator@mystictales.com",
            AccountPhone: "+1-202-555-0188",
            StatementPerjury: "I swear under penalty of perjury that the material was removed due to a mistake.",
            Signature: "Jane Smith",
            DmcaAccusationId: 1,
            Jurisdiction: "California District Court",
            EvidenceFileKey: "https://cdn.mystictales.com/dmca/counter/evidence.pdf",
            IsValid: true,
            InValidReason: "",
            ValidatedBy: 302,
            ValidatedAt: "2025-10-03T10:00:00.000Z",
            FiledDate: "2025-10-02",
            CreatedAt: "2025-10-02T08:00:00.000Z",
            UpdatedAt: "2025-10-03T10:00:00.000Z",
            CounterNoticeAttachFileList: [
                {
                    Id: "counter-1",
                    AttachFileKey: "https://cdn.mystictales.com/dmca/counter/audio-license.pdf",
                    CreatedAt: "2025-10-02T08:05:00.000Z",
                },
            ],
        },

        LawsuitProof: {
            Id: "f3e7a8b9-9101-4def-8c23-334455667788",
            AccountId: 201,
            GoodFaithStatement: "The claimant has filed a legal action to resolve the copyright dispute.",
            CourtName: "Los Angeles Superior Court",
            CaseNumber: "LA-DC-2025-1042",
            FilingDate: "2025-10-04",
            Signature: "John Doe",
            DmcaAccusationId: 1,
            IsValid: true,
            InValidReason: "",
            ValidatedBy: 303,
            ValidatedAt: "2025-10-05T09:30:00.000Z",
            JudgmentDetails: "The court ruled that the content constitutes fair use and dismissed the claim.",
            DateResolved: "2025-10-05",
            Outcome: "Dismissed - fair use confirmed",
            RulingDocumentFileKey: "https://cdn.mystictales.com/dmca/lawsuit/ruling.pdf",
            IsDefendantWon: true,
            CreatedAt: "2025-10-04T10:00:00.000Z",
            UpdatedAt: "2025-10-05T09:30:00.000Z",
            LawsuitProofAttachFileList: [
                {
                    Id: "lawsuit-1",
                    AttachFileKey: "https://cdn.mystictales.com/dmca/lawsuit/court-proof.jpg",
                    CreatedAt: "2025-10-04T10:05:00.000Z",
                },
            ],
        },
    },
}
export const mockPodcastShow: any = {
    PodcastShow: {
        Id: "a7cbe7d1-91f2-4e8a-b345-7f3b9b123456",
        Name: "The Dark Whispers",
        Description: "A chilling podcast exploring the mysteries of the supernatural world.",
        CreatedAt: "2025-10-06T07:45:33.331Z",
        ListenCount: 125000,
        MainImageFileKey: "https://i1.sndcdn.com/artworks-qJYg1hXbykqZS6a5-hbrOLw-t500x500.jpg",
    }
}
export const mockPodcastEpisode: any = {
    PodcastEpisode: {
        Id: "b4c9f3e1-23a5-4cd1-a912-9e3c8a654321",
        Title: "Echoes in the Forest",
        Description: "An eerie tale of a haunted forest where whispers of the past come alive.",
        CreatedAt: "2025-10-06T07:45:33.331Z",
        ListenCount: 125000,
        MainImageFileKey: "https://i1.sndcdn.com/artworks-WiyjbM8XLy9X8tra-7gi9yw-t500x500.jpg",
    }
}
export const mockAssignedStaff: any = {
    Account: {
        Id: 301,
        Fullname: "Jane Smith",
        Email: "jane.smith@example.com",
        Phone: "+1-202-555-0188",
        Gender: "Female",
        CreatedAt: "2025-10-01T12:00:00.000Z",
        MainImageFileUrl: "https://picsum.photos/200/200?3",
    },
}
export const mockStaffList: any = {
    StaffList: [
        {
            Id: 1,
            Email: "user1@example.com",
            Role: { Id: 1, Name: "Customer" },
            Fullname: "Nguyen Van A",
            Dob: "1995-05-20",
            Gender: "Male",
            Address: "123 Main Street, Hanoi",
            Phone: "0123456789",
            Balance: 100000,
            MainImageFileUrl: "https://picsum.photos/200/200?1",
        },
        {
            Id: 2,
            Email: "user2@example.com",
            Role: { Id: 2, Name: "Admin" },
            Fullname: "Tran Thi B",
            Dob: "1998-12-15",
            Gender: "Female",
            Address: "456 Nguyen Trai, HCMC",
            Phone: "0987654321",
            Balance: 250000,
            MainImageFileUrl: "https://picsum.photos/200/200?2",
        },
    ],
}
type DMCAAccusationDetailViewProps = {}
type DMCAAccusationDetailViewContextProps = {}
export const DMCAAccusationDetailViewContext = createContext<DMCAAccusationDetailViewContextProps | null>(null)

const DMCAAccusationDetailView: FC<DMCAAccusationDetailViewProps> = () => {
    const { id } = useParams<{ id: string }>()
    const [DMCAAccusation, setDMCAAccusation] = useState<DMCAAccusationDetail | null>(null)
    const [podcastShow, setPodcastShow] = useState<PodcastShow | null>(null)
    const [podcastEpisode, setPodcastEpisode] = useState<PodcastEpisode | null>(null)
    const [assignedStaff, setAssignedStaff] = useState<Account | null>(null)
    const [staffList, setStaffList] = useState<Account[]>([])
    const [loading, setLoading] = useState(true)
    const [selectedStaffId, setSelectedStaffId] = useState<number | null>(null)

    // const fetchDMCAAccusation = async () => {
    //     if (id) {
    //         setLoading(true)
    //         try {
    //             const response = await getCommunitySurveyDetail(managerAxiosInstance, Number(id));
    //             if (response.success && response.data) {
    //                 setSurveyData(response.data.Survey);
    //    await Promise.all([
    //   fetchAssignedStaff(mockDMCAAccusation.DMCAAccusation.AssignedStaff),
    //   fetchPodcastShow(mockDMCAAccusation.DMCAAccusation.PodcastShowId),
    //   fetchPodcastEpisode(mockDMCAAccusation.DMCAAccusation.PodcastEpisodeId),
    // ]);
    //             } else {
    //                 console.error('API Error:', response.message);
    //             }
    //         } catch (error) {
    //             console.error("Error fetching accusation data:", error)
    //         } finally {
    //             setLoading(false)
    //         }
    //     }
    // }

    const fetchDMCAAccusation = async () => {
        if (id) {
            setDMCAAccusation(mockDMCAAccusation.DMCAAccusation)
            const assignedStaffId = mockDMCAAccusation.DMCAAccusation.AssignedStaff

            if (assignedStaffId != null && assignedStaffId !== "") {
                await Promise.all([
                    fetchAssignedStaff(assignedStaffId),
                    fetchPodcastShow(mockDMCAAccusation.DMCAAccusation.PodcastShowId),
                    fetchPodcastEpisode(mockDMCAAccusation.DMCAAccusation.PodcastEpisodeId),
                ])
            } else {
                await fetchStaffListToAssign()
                await Promise.all([
                    fetchPodcastShow(mockDMCAAccusation.DMCAAccusation.PodcastShowId),
                    fetchPodcastEpisode(mockDMCAAccusation.DMCAAccusation.PodcastEpisodeId),
                ])
            }

            setLoading(false)
        }
    }
    const fetchStaffListToAssign = async () => {
        setStaffList(mockStaffList.StaffList)
    }
    const fetchAssignedStaff = async (id: number) => {
        if (id) {
            setAssignedStaff(mockAssignedStaff.Account)
        }
    }
    const fetchPodcastShow = async (id: number) => {
        if (id) {
            setPodcastShow(mockPodcastShow.PodcastShow)
        }
    }
    const fetchPodcastEpisode = async (id: number) => {
        if (id) {
            setPodcastEpisode(mockPodcastEpisode.PodcastEpisode)
        }
    }

    const handleAssignStaff = () => {
        if (selectedStaffId) {
            console.log("Assigning staff with ID:", selectedStaffId)
            // API call would go here
        }
    }

    const formatDate = (dateString: string) => {
        return new Date(dateString).toLocaleDateString("en-US", {
            year: "numeric",
            month: "long",
            day: "numeric",
            hour: "2-digit",
            minute: "2-digit",
        })
    }

    useEffect(() => {
        fetchDMCAAccusation()
    }, [id])

    if (loading) {
        return (
            <div className="loading-spinner">
                <CircularProgress />
            </div>
        )
    }
    if (!DMCAAccusation) {
        return <div className="text-center text-danger">No DMCA accusation data found</div>
    }

    return (
        <div className="dmca-detail">
            <div className="dmca-detail__header">
                <h1 className="dmca-detail__title">DMCA Accusation #{DMCAAccusation.Id}</h1>
                <div className="dmca-detail__meta">
                    <span className="dmca-detail__meta-item">Created At: {formatDate(DMCAAccusation.CreatedAt)}</span>
                    <span className="dmca-detail__meta-item">Last Updated: {formatDate(DMCAAccusation.UpdatedAt || DMCAAccusation.CreatedAt)}</span>
                </div>
            </div>

            {/* Assigned Staff Section */}
            <div className="dmca-detail__section">
                <h2 className="dmca-detail__section-title">Staff Assignment</h2>
                <div className="dmca-detail__card">
                    {assignedStaff ? (
                        <div className="staff-info">
                            <div className="staff-info__header">
                                <img
                                    src={assignedStaff.MainImageFileKey || "/placeholder.svg"}
                                    alt={assignedStaff.Fullname}
                                    className="staff-info__avatar"
                                />
                                <div className="staff-info__details">
                                    <h3 className="staff-info__name">{assignedStaff.Fullname}</h3>
                                    <p className="staff-info__role">Assigned Staff Member</p>
                                </div>
                            </div>
                            <div className="staff-info__contact">
                                <div className="staff-info__contact-item">
                                    <span className="staff-info__label">Email:</span>
                                    <span className="staff-info__value">{assignedStaff.Email}</span>
                                </div>
                                <div className="staff-info__contact-item">
                                    <span className="staff-info__label">Phone:</span>
                                    <span className="staff-info__value">{assignedStaff.Phone}</span>
                                </div>
                                <div className="staff-info__contact-item">
                                    <span className="staff-info__label">Gender:</span>
                                    <span className="staff-info__value">{assignedStaff.Gender}</span>
                                </div>
                            </div>
                        </div>
                    ) : (
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
                                        <img
                                            src={staff.MainImageFileKey || "/placeholder.svg"}
                                            alt={staff.Fullname}
                                            className="staff-assign__avatar"
                                        />
                                        <div className="staff-assign__info">
                                            <h4 className="staff-assign__name">{staff.Fullname}</h4>
                                            <p className="staff-assign__email">{staff.Email}</p>
                                            <p className="staff-assign__role">Id: #{staff.Id}</p>
                                        </div>
                                    </div>
                                ))}
                            </div>
                            <button className="staff-assign__button" onClick={handleAssignStaff} disabled={!selectedStaffId}>
                                Assign Selected Staff
                            </button>
                        </div>
                    )}
                </div>
            </div>

            {/* Podcast Content Section */}
            <div className="dmca-detail__section">
                <h2 className="dmca-detail__section-title">Accused Content</h2>
                <div className="dmca-detail__content-grid">
                    {podcastShow && (
                        <div className="content-card">
                            <img
                                src={podcastShow.MainImageFileKey || "/placeholder.svg"}
                                alt={podcastShow.Name}
                                className="content-card__image"
                            />
                            <div className="content-card__body">
                                <span className="content-card__type">Show</span>
                                <h3 className="content-card__title">{podcastShow.Name}</h3>
                                <p className="content-card__description">{podcastShow.Description}</p>
                                <div className="content-card__stats">
                                    <span className="content-card__stat">{podcastShow.ListenCount.toLocaleString()} listens</span>
                                </div>
                            </div>
                        </div>
                    )}
                    {podcastEpisode && (
                        <div className="content-card">
                            <img
                                src={podcastEpisode.MainImageFileKey || "/placeholder.svg"}
                                alt={podcastEpisode.Title}
                                className="content-card__image"
                            />
                            <div className="content-card__body">
                                <span className="content-card__type">Episode</span>
                                <h3 className="content-card__title">{podcastEpisode.Title}</h3>
                                <p className="content-card__description">{podcastEpisode.Description}</p>
                                <div className="content-card__stats">
                                    <span className="content-card__stat">{podcastEpisode.ListenCount.toLocaleString()} listens</span>
                                </div>
                            </div>
                        </div>
                    )}
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
                                    className={`notice__status notice__status--${DMCAAccusation.DMCANotice.IsValid ? "valid" : "invalid"}`}
                                >
                                    {DMCAAccusation.DMCANotice.IsValid ? "Valid Notice" : "Invalid Notice"}
                                </span>
                                <span className="notice__id">ID: {DMCAAccusation.DMCANotice.Id}</span>
                            </div>

                            <div className="notice__grid">
                                <div className="notice__field">
                                    <span className="notice__label">Claimant Email:</span>
                                    <span className="notice__value">{DMCAAccusation.DMCANotice.AccountEmail}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Claimant Phone:</span>
                                    <span className="notice__value">{DMCAAccusation.DMCANotice.AccountPhone}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Work Claimed:</span>
                                    <span className="notice__value">{DMCAAccusation.DMCANotice.WorkClaimed}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Signature:</span>
                                    <span className="notice__value">{DMCAAccusation.DMCANotice.Signature}</span>
                                </div>
                            </div>

                            <div className="notice__statement">
                                <span className="notice__label">Good Faith Statement:</span>
                                <p className="notice__text">{DMCAAccusation.DMCANotice.GoodFaithStatement}</p>
                            </div>

                            {DMCAAccusation.DMCANotice.DMCANoticeAttachFileList &&
                                DMCAAccusation.DMCANotice.DMCANoticeAttachFileList.length > 0 && (
                                    <div className="notice__attachments">
                                        <span className="notice__label">Attachments:</span>
                                        <div className="attachments">
                                            {DMCAAccusation.DMCANotice.DMCANoticeAttachFileList.map((file: any) => (
                                                <a
                                                    key={file.Id}
                                                    href={file.AttachFileKey}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="attachments__item"
                                                >
                                                    {file.AttachFileKey.split("/").pop()}
                                                </a>
                                            ))}
                                        </div>
                                    </div>
                                )}

                            <div className="notice__validation">
                                <span className="notice__label mr-2">Validated:</span>
                                <span className="notice__value">
                                    {formatDate(DMCAAccusation.DMCANotice.ValidatedAt)} by Staff #{DMCAAccusation.DMCANotice.ValidatedBy}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Counter Notice Section */}
            {DMCAAccusation.CounterNotice && (
                <div className="dmca-detail__section">
                    <h2 className="dmca-detail__section-title">Counter Notice</h2>
                    <div className="dmca-detail__card">
                        <div className="notice">
                            <div className="notice__header">
                                <span
                                    className={`notice__status notice__status--${DMCAAccusation.CounterNotice.IsValid ? "valid" : "invalid"}`}
                                >
                                    {DMCAAccusation.CounterNotice.IsValid ? "Valid Counter Notice" : "Invalid Counter Notice"}
                                </span>
                                <span className="notice__id">ID: {DMCAAccusation.CounterNotice.Id}</span>
                            </div>

                            <div className="notice__grid">
                                <div className="notice__field">
                                    <span className="notice__label">Respondent Email:</span>
                                    <span className="notice__value">{DMCAAccusation.CounterNotice.AccountEmail}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Respondent Phone:</span>
                                    <span className="notice__value">{DMCAAccusation.CounterNotice.AccountPhone}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Jurisdiction:</span>
                                    <span className="notice__value">{DMCAAccusation.CounterNotice.Jurisdiction}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Filed Date:</span>
                                    <span className="notice__value">{DMCAAccusation.CounterNotice.FiledDate}</span>
                                </div>
                                <div className="notice__field">
                                    <span className="notice__label">Signature:</span>
                                    <span className="notice__value">{DMCAAccusation.CounterNotice.Signature}</span>
                                </div>
                            </div>

                            <div className="notice__statement">
                                <span className="notice__label">Statement Under Perjury:</span>
                                <p className="notice__text">{DMCAAccusation.CounterNotice.StatementPerjury}</p>
                            </div>

                            {DMCAAccusation.CounterNotice.EvidenceFileKey && (
                                <div className="notice__evidence">
                                    <span className="notice__label mr-2">Evidence Document:</span>
                                    <a
                                        href={DMCAAccusation.CounterNotice.EvidenceFileKey}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="notice__link"
                                    >
                                        View Evidence Document
                                    </a>
                                </div>
                            )}

                            {DMCAAccusation.CounterNotice.CounterNoticeAttachFileList &&
                                DMCAAccusation.CounterNotice.CounterNoticeAttachFileList.length > 0 && (
                                    <div className="notice__attachments">
                                        <span className="notice__label">Attachments:</span>
                                        <div className="attachments">
                                            {DMCAAccusation.CounterNotice.CounterNoticeAttachFileList.map((file: any) => (
                                                <a
                                                    key={file.Id}
                                                    href={file.AttachFileKey}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="attachments__item"
                                                >
                                                    {file.AttachFileKey.split("/").pop()}
                                                </a>
                                            ))}
                                        </div>
                                    </div>
                                )}

                            <div className="notice__validation">
                                <span className="notice__label mr-2">Validated:</span>
                                <span className="notice__value">
                                    {formatDate(DMCAAccusation.CounterNotice.ValidatedAt)} by Staff #
                                    {DMCAAccusation.CounterNotice.ValidatedBy}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            )}

            {/* Lawsuit Proof Section */}
            {DMCAAccusation.LawsuitProof && (
                <div className="dmca-detail__section">
                    <h2 className="dmca-detail__section-title">Lawsuit Proof</h2>
                    <div className="dmca-detail__card">
                        <div className="lawsuit">
                            <div className="lawsuit__header">
                                <span
                                    className={`lawsuit__status lawsuit__status--${DMCAAccusation.LawsuitProof.IsValid ? "valid" : "invalid"}`}
                                >
                                    {DMCAAccusation.LawsuitProof.IsValid ? "Valid Lawsuit" : "Invalid Lawsuit"}
                                </span>
                                {DMCAAccusation.LawsuitProof.IsDefendantWon !== null && (
                                    <span
                                        className={`lawsuit__outcome lawsuit__outcome--${DMCAAccusation.LawsuitProof.IsDefendantWon ? "defendant" : "claimant"}`}
                                    >
                                        {DMCAAccusation.LawsuitProof.IsDefendantWon ? "Defendant Won" : "Claimant Won"}
                                    </span>
                                )}
                            </div>

                            <div className="lawsuit__grid">
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Court Name:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.CourtName}</span>
                                </div>
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Case Number:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.CaseNumber}</span>
                                </div>
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Filing Date:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.FilingDate}</span>
                                </div>
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Date Resolved:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.DateResolved}</span>
                                </div>
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Outcome:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.Outcome}</span>
                                </div>
                                <div className="lawsuit__field">
                                    <span className="lawsuit__label">Signature:</span>
                                    <span className="lawsuit__value">{DMCAAccusation.LawsuitProof.Signature}</span>
                                </div>
                            </div>

                            <div className="lawsuit__statement">
                                <span className="lawsuit__label">Good Faith Statement:</span>
                                <p className="lawsuit__text">{DMCAAccusation.LawsuitProof.GoodFaithStatement}</p>
                            </div>

                            {DMCAAccusation.LawsuitProof.JudgmentDetails && (
                                <div className="lawsuit__judgment">
                                    <span className="lawsuit__label">Judgment Details:</span>
                                    <p className="lawsuit__text">{DMCAAccusation.LawsuitProof.JudgmentDetails}</p>
                                </div>
                            )}

                            {DMCAAccusation.LawsuitProof.RulingDocumentFileUrl && (
                                <div className="lawsuit__ruling">
                                    <span className="lawsuit__label">Ruling Document:</span>
                                    <a
                                        href={DMCAAccusation.LawsuitProof.RulingDocumentFileUrl}
                                        target="_blank"
                                        rel="noopener noreferrer"
                                        className="lawsuit__link"
                                    >
                                        View Ruling Document
                                    </a>
                                </div>
                            )}

                            {DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList &&
                                DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList.length > 0 && (
                                    <div className="lawsuit__attachments">
                                        <span className="lawsuit__label">Attachments:</span>
                                        <div className="attachments">
                                            {DMCAAccusation.LawsuitProof.LawsuitProofAttachFileList.map((file: any) => (
                                                <a
                                                    key={file.Id}
                                                    href={file.AttachFileKey}
                                                    target="_blank"
                                                    rel="noopener noreferrer"
                                                    className="attachments__item"
                                                >
                                                    {file.AttachFileKey.split("/").pop()}
                                                </a>
                                            ))}
                                        </div>
                                    </div>
                                )}

                            <div className="lawsuit__validation">
                                <span className="lawsuit__label mr-2">Validated:</span>
                                <span className="lawsuit__value">
                                    {formatDate(DMCAAccusation.LawsuitProof.ValidatedAt)} by Staff #
                                    {DMCAAccusation.LawsuitProof.ValidatedBy}
                                </span>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}

export default DMCAAccusationDetailView
