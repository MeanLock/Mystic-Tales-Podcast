import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "../../../../../core/utils/date.util"
import { getBuddyReviewSession } from "@/core/services/report/BuddyReport.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import BuddyReportDetail from "./BuddyReportDetail"
import { Eye } from "phosphor-react"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import Loading from "../../../../components/common/loading"

ModuleRegistry.registerModules([AllCommunityModule])

interface BuddyReportReviewViewProps { }
interface BuddyReportReviewViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const BuddyReportReviewViewContext = createContext<BuddyReportReviewViewContextProps | null>(null)

const state_creator = (table: any[]) => {
    const state = {
        columnDefs: [
            {
                headerName: "No.",
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1; // Hiển thị số thứ tự từ 1
                },
                flex: 0.4,
                cellClass: '',
                sortable: false,
                filter: false
            },
            { headerName: "Assigned Staff", field: "AssignedStaff.FullName", flex: 1.2 },
            { headerName: "Podcast Buddy ", field: "PodcastBuddy.FullName", flex: 1.2 },
            { headerName: "Resolved Violation Point", field: "ResolvedViolationPoint", flex: 1.2 },
            {
                headerName: "Created At",
                field: "CreatedAt",
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),

            },
            {
                headerName: "Updated At",
                field: "UpdatedAt",
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),

            },
            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center',
                cellRenderer: (params: { data: any }) => {
                    let status = {
                        title: '',
                        color: '',
                    };
                    if (params.data.IsResolved) {
                        status = {
                            title: 'Resolved',
                            color: 'success',
                        };
                    } else if (params.data.IsResolved === false) {
                        status = {
                            title: 'Rejected',
                            color: 'danger',
                        };
                    } else if (params.data.IsResolved === null) {
                        status = {
                            title: 'Unresolved',
                            color: 'warning',
                        };
                    } else {
                        status = {
                            title: 'Unresolved',
                            color: 'warning',
                        };
                    }
                    return (
                        <CCard
                            textColor={`${status.color}`}
                            style={{ width: '100px' }}
                            className={`text-center fw-bold rounded-pill px-1 border-2 border-${status.color} bg-light`}
                        >
                            {status.title}
                        </CCard>
                    );
                },
            },

            {
                headerName: "",
                cellClass: 'd-flex justify-content-center py-0',
                cellRenderer: (params: { data: any }) => {
                    const Modal_props = {
                        detailForm: <BuddyReportDetail podcastBuddyReportReviewSessionId={params.data.Id} onClose={() => { }} />,
                        title: '',
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



const BuddyReportReviewView: FC<BuddyReportReviewViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const reportList = await getBuddyReviewSession(adminAxiosInstance);
            if (reportList.success) {
                setState(state_creator(reportList.data.BuddyReportReviewSessionList));
            } else {
                console.error('API Error:', reportList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch buddy reports:', error);
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
        <BuddyReportReviewViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Buddy Report Review Sessions</h4>
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


        </BuddyReportReviewViewContext.Provider>
    )
}

export default BuddyReportReviewView
