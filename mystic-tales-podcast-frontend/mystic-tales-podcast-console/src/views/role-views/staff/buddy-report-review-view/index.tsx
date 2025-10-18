import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "@/core/utils/date.util"
import SurveyTalkLoading from "@/views/components/common/loading"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import { Eye } from "phosphor-react"
import BuddyReportDetail from "./BuddyReportDetail"
export const mockList: any = {
    BuddyReportReviewSessionList: [
        {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            PodcastBuddy: {
                Id: 101,
                FullName: "Nguyen Buddy A",
                Email: "string",
                MainImageFileKey: "string"
            },
            AssignedStaff: {
                Id: 201,
                FullName: "Nguyen Van Thinh",
                Email: "string",
                MainImageFileKey: "string"
            },
            ResolvedViolationPoint: 2,
            IsResolved: true,
            CreatedAt: "2025-10-10T10:24:13.336Z",
            UpdatedAt: "2025-10-10T10:26:42.100Z",
        },
        {
            Id: "e7b13b77-8f10-4d95-9e54-2f00a8b0a888",
            PodcastBuddy: {
                Id: 101,
                FullName: "Nguyen Buddy B",
                Email: "string",
                MainImageFileKey: "string"
            },
            AssignedStaff: {
                Id: 201,
                FullName: "Nguyen Van B",
                Email: "string",
                MainImageFileKey: "string"
            },
            ResolvedViolationPoint: 0,
            IsResolved: false,
            CreatedAt: "2025-10-09T16:30:05.120Z",
            UpdatedAt: "2025-10-09T18:02:10.000Z",
        },
        {
            Id: "b8f64a92-dc23-4b1e-97c1-9b9186f27d44",
            PodcastBuddy: {
                Id: 101,
                FullName: "Nguyen Buddy C",
                Email: "string",
                MainImageFileKey: "string"
            },
            AssignedStaff: {
                Id: 203,
                FullName: "Nguyen Van C",
                Email: "string",
                MainImageFileKey: "string"
            },
            ResolvedViolationPoint: null,
            IsResolved: null,
            CreatedAt: "2025-10-08T09:12:44.230Z",
            UpdatedAt: "2025-10-08T10:00:00.000Z",
        },
    ],
};
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
                flex: 0.2,
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1;
                },
                cellClass: '',
                sortable: false,
                filter: false
            },
            { headerName: "Podcast Buddy ", field: "PodcastBuddy.FullName" },
            { headerName: "Resolved Violation Point", field: "ResolvedViolationPoint" },
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
                flex: 0.7,
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
                headerName: "Action",
                cellClass: 'd-flex justify-content-center py-0',
                cellRenderer: (params: { data: any }) => {
                    const Modal_props = {
                        detailForm: <BuddyReportDetail podcastBuddyReportReviewSessionId={params.data.Id} onClose={() => { }} />,
                        title: '',
                        button: <Eye size={27} color='var(--secondary-green)' />,
                        update_button_color: 'white'
                    }
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

    // const handleDataChange = async () => {
    //   setIsLoading(true);
    //   try {
    //     const accountList = await getCustomerAccounts(adminAxiosInstance);
    //     if (accountList.success) {
    //       setState(state_creator(accountList.data.Accounts));
    //     } else {
    //       console.error('API Error:', accountList.message);
    //     }
    //   } catch (error) {
    //     console.error('Lỗi khi fetch customer accounts:', error);
    //   } finally {
    //     setIsLoading(false);
    //   }
    // }
    const handleDataChange = async () => {
        setIsLoading(false);
        setState(state_creator(mockList.BuddyReportReviewSessionList));

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
                        <SurveyTalkLoading />
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
