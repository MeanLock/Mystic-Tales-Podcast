import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import { Eye, Warning } from "phosphor-react"
import { Tabs, Tab } from "react-bootstrap"
import { useNavigate } from "react-router-dom"
import { ConfirmationDialog } from "./ConfirmationDialog"
import { getCommunitySurveys } from "../../../../core/services/survey/survey-core/survey-core.service"
import { managerAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import SurveyTalkLoading from "../../../components/common/loading"
import { formatDate } from "../../../../core/utils/date.util"

ModuleRegistry.registerModules([AllCommunityModule])

interface SurveyViewProps { }
interface SurveyViewContextProps {
    handleDataChange: () => void
}
interface ColumnActionsProps {
    navigate: (path: string) => void
    onUnpublish: (surveyId: number) => void
}
export const SurveyViewContext = createContext<SurveyViewContextProps | null>(null)

const getColumnDefs = ({ navigate, onUnpublish }: ColumnActionsProps) => [
    { headerName: "Survey ID", field: "Id", flex: 0.4 },
    {
        headerName: "Requester ID",
        field: "SurveyPrivateData.RequesterId",
        flex: 0.4,
        valueGetter: (params: any) => params.data?.SurveyPrivateData?.RequesterId || "N/A",
    },
    { headerName: "Survey Name", field: "Title", flex: 0.8 },
    {
        headerName: "Publish At",
        field: "PublishedAt",
        flex: 0.5,
        valueGetter: (params: any) => formatDate(params.data.PublishedAt),

    },
    {
        headerName: "End Date",
        field: "EndDate",
        flex: 0.4,
        valueGetter: (params: any) => formatDate(params.data.EndDate),

    },
    {
        headerName: "KPI",
        field: "SurveyPrivateData.Kpi",
        flex: 0.5,
        valueFormatter: (params: any) => `${params.value} takers`,

    },
    {
        headerName: "Question Count",
        field: "QuestionCount",
        flex: 0.4,
        valueFormatter: (params: any) => `${params.value} `,
    },
    {
        headerName: "Bonus Reward",
        field: "CurrentSurveyRewardTracking.RewardPrice",
        flex: 0.4,
        valueFormatter: (params: any) => `${params.value} đ`,
    },
    {
        headerName: "Bonus XP",
        field: "CurrentSurveyRewardTracking.RewardXp",
        flex: 0.4,
        valueFormatter: (params: any) => `${params.value} XP`,

    },
    {
        headerName: "Base Price",
        field: "TakerBaseRewardPrice",
        flex: 0.4,
        valueFormatter: (params: any) => `${params.value} đ`,
    },
    {
        headerName: "Actions",
        cellClass: "d-flex justify-content-center py-0",
        flex: 0.5,
        cellRenderer: (params: any) => {
            const surveyId = params.data.Id
            return (
                <div className="d-flex gap-2 align-items-center h-100">
                    <CButton
                        size="sm"
                        className="text-primary"
                        onClick={() => navigate("/community-survey/detail/" + surveyId)}
                    >
                        <Eye size={20} />
                    </CButton>
                    {/* <CButton
                        size="sm"
                        className="text-danger"
                        onClick={() => onUnpublish(surveyId)}
                    >
                        <Warning size={20} />
                    </CButton> */}
                </div>
            )
        },

    },
]
export enum SurveyDeadlineQueryEnum {
    OnDeadline = 1,
    NearDeadline = 2,
    LateForDeadline = 3
}
const tabConfigs = [
    {
        key: "all",
        title: "All",
        description: "Tất cả khảo sát",
        loadData: async () => {
            const response = await getCommunitySurveys(managerAxiosInstance);
            return response.success ? response.data.Surveys : [];
        },
    },
    {
        key: "active",
        title: "On Deadline",
        description: "Khảo Sát Đang Diễn Ra",
        loadData: async () => {
            const response = await getCommunitySurveys(managerAxiosInstance, SurveyDeadlineQueryEnum.OnDeadline);
            return response.success ? response.data.Surveys : [];
        },
    },
    {
        key: "near",
        title: "Near Deadline",
        description: "Khảo Sát Sắp Kết Thúc",
        loadData: async () => {
            const response = await getCommunitySurveys(managerAxiosInstance, SurveyDeadlineQueryEnum.NearDeadline);
            return response.success ? response.data.Surveys : [];
        },
    },
    {
        key: "late",
        title: "Late for Deadline",
        description: "Khảo Sát Trễ",
        loadData: async () => {
            const response = await getCommunitySurveys(managerAxiosInstance, SurveyDeadlineQueryEnum.LateForDeadline);
            return response.success ? response.data.Surveys : [];
        },
    },
    // {
    //     key: "finished",
    //     title: "Hoàn Thành",
    //     description: "Khảo Sát Đã Hoàn Thành",
    //     loadData: async () => {
    //         const response = await getCommunitySurveys(managerAxiosInstance);
    //         if (response.success) {
    //             return response.data.Surveys.filter((survey: any) => !survey.IsAvailable);
    //         }
    //         return [];
    //     },
    // },
]


const SurveyView: FC<SurveyViewProps> = () => {
    const navigate = useNavigate();
    const [tabData, setTabData] = useState<{ [key: string]: any[] }>({})
    const [key, setKey] = useState("all")
    const [showConfirmDialog, setShowConfirmDialog] = useState(false)
    const [selectedSurveyId, setSelectedSurveyId] = useState<number | null>(null)
    const [isLoading, setIsLoading] = useState(false)

    const handleUnpublishSurvey = async (surveyId: number) => {
        // const success = await surveyService.unpublishSurvey(surveyId)
        const success = true
        if (success) {
            handleDataChange()
            alert("Survey unpublished successfully!")
        } else {
            alert("Failed to unpublish survey")
        }
    }
    const confirmUnpublish = (surveyId: number) => {
        setSelectedSurveyId(surveyId)
        setShowConfirmDialog(true)
    }

    const handleConfirmUnpublish = () => {
        if (selectedSurveyId) {
            handleUnpublishSurvey(selectedSurveyId)
        }
        setShowConfirmDialog(false)
        setSelectedSurveyId(null)
    }
    const handleTabChange = async (newKey: string) => {
        setKey(newKey)
        setIsLoading(true)

        // Hiển thị loading ngay lập tức
        setTabData((prev) => ({ ...prev, [newKey]: [] }))

        try {
            const tab = tabConfigs.find((t) => t.key === newKey)
            if (tab) {
                const data = await tab.loadData()
                setTabData((prev) => ({ ...prev, [newKey]: data }))
            }
        } catch (error) {
            console.error('Error loading tab data:', error)
            setTabData((prev) => ({ ...prev, [newKey]: [] }))
        } finally {
            setIsLoading(false)
        }
    }

    const handleDataChange = async () => {
        setIsLoading(true)

        try {
            const tab = tabConfigs.find((t) => t.key === key)
            if (tab) {
                const data = await tab.loadData()
                setTabData((prev) => ({ ...prev, [key]: data }))
            }
        } catch (error) {
            console.error('Error refreshing data:', error)
            setTabData((prev) => ({ ...prev, [key]: [] }))
        } finally {
            setIsLoading(false)
        }
    }

    useEffect(() => {
        handleTabChange(key)
    }, [])

    const defaultColDef = useMemo(() => {
        return {
            flex: 1,
            filter: true,
            autoHeight: true,
            resizable: true,
            wrapText: true,
            cellClass: "d-flex align-items-center",
            editable: false,
        }
    }, [])

    const currentTab = tabConfigs.find((t) => t.key === key)

    return (
        <SurveyViewContext.Provider value={{ handleDataChange }}>
            <CRow>
                <CCol xs={12}>
                    {currentTab && (
                        <>
                            <div className="d-flex align-items-center mt-3 mb-4">
                                <Tabs
                                    id="survey-tabs"
                                    className="survey-tabs-container"
                                    activeKey={key}
                                    onSelect={(k) => handleTabChange(k!)}
                                >
                                    {tabConfigs.map((tab) => (
                                        <Tab eventKey={tab.key} title={tab.title} key={tab.key} />
                                    ))}
                                </Tabs>
                            </div>

                            <div className="tab-content-container">
                                <div className="d-flex justify-content-between align-items-center mb-4">
                                    <h4>{currentTab.description}</h4>

                                </div>

                                {isLoading ? (
                                    <SurveyTalkLoading />
                                ) : tabData[key] && tabData[key].length > 0 ? (
                                    <div className="tab-content-container__table">
                                        <AgGridReact
                                            columnDefs={getColumnDefs({ navigate, onUnpublish: confirmUnpublish })}
                                            rowData={tabData[key]}
                                            defaultColDef={defaultColDef}
                                            rowHeight={55}
                                            headerHeight={30}
                                            pagination
                                            paginationPageSize={10}
                                            domLayout="autoHeight"
                                        />
                                    </div>
                                ) : (
                                    <div className="text-center text-muted p-4">
                                        Không có dữ liệu để hiển thị
                                    </div>
                                )}
                            </div>
                        </>
                    )}
                </CCol>
            </CRow>

            <ConfirmationDialog
                show={showConfirmDialog}
                title="Confirm Unpublish"
                message="Are you sure you want to unpublish this survey?."
                onConfirm={handleConfirmUnpublish}
                onCancel={() => setShowConfirmDialog(false)}
            />
        </SurveyViewContext.Provider>
    )
}

export default SurveyView
