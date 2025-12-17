import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "../../../../../core/utils/date.util"
import { getEpisodeReviewSession } from "@/core/services/report/EpisodeReport.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v1/admin-instance"
import EpisodeReportDetail from "./EpisodeReportDetail"
import { Eye } from "phosphor-react"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import Loading from "@/views/components/common/loading"

ModuleRegistry.registerModules([AllCommunityModule])

interface EpisodeReportReviewViewProps { }
interface EpisodeReportReviewViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const EpisodeReportReviewViewContext = createContext<EpisodeReportReviewViewContextProps | null>(null)

const state_creator = (table: any[]) => {
    const state = {
        columnDefs: [
            {
                headerName: "No.",
                flex: 0.2,
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1; // Hiển thị số thứ tự từ 1
                },
                cellClass: '',
                sortable: false,
                filter: false
            },
            { headerName: "Assigned Staff", field: "AssignedStaff.FullName", flex: 0.8 },
            { headerName: "Episode", field: "PodcastEpisode.Name", flex: 0.8 },
            {
                headerName: "Created At",
                field: "CreatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),
                comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                    const dateA = new Date(nodeA.data.CreatedAt).getTime();
                    const dateB = new Date(nodeB.data.CreatedAt).getTime();
                    return dateA - dateB;
                },
            },
            {
                headerName: "Updated At",
                field: "UpdatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),
                comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                    // So sánh theo timestamp gốc thay vì string đã format
                    const dateA = new Date(nodeA.data.UpdatedAt).getTime();
                    const dateB = new Date(nodeB.data.UpdatedAt).getTime();
                    return dateA - dateB;
                },
            },
            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center',
                flex: 0.7,
                valueGetter: (params: { data: any }) => {
                    if (params.data.IsResolved === false) return "Rejected";
                    if (params.data.IsResolved) return "Resolved";
                    return "Unresolved";
                },
                cellRenderer: (params: { data: any }) => {
                    let status = {
                        title: '',
                        color: '',
                        bg: ''
                    };
                    if (params.data.IsResolved) {
                        status = {
                            title: 'Resolved',
                            color: 'var(--secondary-green)',
                            bg: 'rgba(173, 227, 57, 0.06)'
                        };
                    } else if (params.data.IsResolved === false) {
                        status = {
                            title: 'Rejected',
                            color: '#ef5350',
                            bg: 'rgba(251, 222, 227, 0.2)',
                        };
                    } else if (params.data.IsResolved === null) {
                        status = {
                            title: 'Unresolved',
                            color: '#ffb300',
                            bg: 'rgba(255, 179, 0, 0.07)'
                        };
                    } else {
                        status = {
                            title: 'Unresolved',
                            color: '#ffb300',
                            bg: 'rgba(255, 179, 0, 0.07)'
                        };
                    }
                    return (
                        <CCard
                            style={{ width: '100px', color: `${status.color}`, background: `${status.bg}`, border: `2px solid ${status.color}` }}
                            className={`text-center fw-bold rounded-pill px-1 `}
                        >
                            {status.title}
                        </CCard>
                    );
                },
            },

            {
                headerName: "Action",
                cellClass: 'd-flex justify-content-center py-0',
                cellRenderer: (params: { data: any }) => {
                    const Modal_props = {
                        detailForm: <EpisodeReportDetail podcastEpisodeReportReviewSessionId={params.data.Id} onClose={() => { }} />,
                        title: 'Episode Report',
                        button: <Eye size={27} color='var(--secondary-green)' />,
                        update_button_color: 'white'
                    }
                    if (params.data.IsResolved !== null) return <></>;
                    return (

                        <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
                            <Modal_Button
                                disabled={false}
                                title={Modal_props.title}
                                content={Modal_props.button}
                                color={Modal_props.update_button_color} >
                                {Modal_props.detailForm}
                            </Modal_Button>
                        </CButtonGroup>
                    )

                },
            }
        ],
        rowData: table

    }
    return state
}



const EpisodeReportReviewView: FC<EpisodeReportReviewViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const reportList = await getEpisodeReviewSession(adminAxiosInstance);
            if (reportList.success) {
                setState(state_creator(reportList.data.EpisodeReportReviewSessionList));
            } else {
                console.error('API Error:', reportList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch episode reports:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        handleDataChange()
    }, [])

    const defaultColDef = useMemo(() => {
        return {
            flex: 1,
            filter: true,
            autoHeight: true,
            resizable: true,
            wrapText: true,
            cellClass: 'd-flex align-items-center',
            editable: false
        };
    }, [])

    return (
        <EpisodeReportReviewViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Episode Report Review Sessions</h4>

            </div>
            <CRow >
                <CCol xs={12}>
                    {isLoading ? (
                        <div className="flex justify-content-center align-items-center h-150" >
                            <Loading />
                        </div>
                    ) : (
                        <div
                            id="customer-table"
                        >
                            <AgGridReact
                                columnDefs={state?.columnDefs}
                                rowData={state?.rowData}
                                defaultColDef={defaultColDef}
                                rowHeight={70}
                                headerHeight={40}
                                pagination={true}
                                paginationPageSize={10}
                                paginationPageSizeSelector={[10, 20, 50, 100]}
                                domLayout='autoHeight'
                            />
                        </div>)}
                </CCol>
            </CRow>


        </EpisodeReportReviewViewContext.Provider>
    )
}

export default EpisodeReportReviewView
