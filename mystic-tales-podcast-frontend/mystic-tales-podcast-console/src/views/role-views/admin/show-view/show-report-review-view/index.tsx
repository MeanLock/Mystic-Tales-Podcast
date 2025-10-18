import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { managerAxiosInstance } from "../../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import SurveyTalkLoading from "../../../../components/common/loading"
import { formatDate } from "../../../../../core/utils/date.util"
export const mockList: any = {
    ShowReportReviewSession: [
        {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            PodcastShowId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            AssignedStaff: 201,
            IsResolved: true,
            CreatedAt: "2025-10-10T10:24:13.336Z",
            UpdatedAt: "2025-10-10T10:26:42.100Z",
        },
        {
            Id: "e7b13b77-8f10-4d95-9e54-2f00a8b0a888",
            PodcastShowId: "c7e3c9e0-12b4-45d1-bb25-91ffb44d8f6a",
            AssignedStaff: 202,
            IsResolved: false,
            CreatedAt: "2025-10-09T16:30:05.120Z",
            UpdatedAt: "2025-10-09T18:02:10.000Z",
        },
        {
            Id: "b8f64a92-dc23-4b1e-97c1-9b9186f27d44",
            PodcastShowId: "9c73b3d0-1c41-4a8e-9d8c-11c5e6c74a01",
            AssignedStaff: 203,
            IsResolved: true,
            CreatedAt: "2025-10-08T09:12:44.230Z",
            UpdatedAt: "2025-10-08T10:00:00.000Z",
        },
    ],
};
ModuleRegistry.registerModules([AllCommunityModule])

interface ShowReportReviewViewProps { }
interface ShowReportReviewViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const ShowReportReviewViewContext = createContext<ShowReportReviewViewContextProps | null>(null)

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
            { headerName: "Assigned Staff", field: "AssignedStaff", flex: 0.8 },
            { headerName: "Podcast Episode ID", field: "PodcastShowId", flex: 0.8 },
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
                headerName: "Created At",
                field: "CreatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),

            },
              {
                headerName: "Updated At",
                field: "UpdatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),

            },
        ],
        rowData: table

    }
    return state
}



const ShowReportReviewView: FC<ShowReportReviewViewProps> = () => {
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
        setState(state_creator(mockList.ShowReportReviewSession));

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
        <ShowReportReviewViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Show Report Review Sessions</h4>
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


        </ShowReportReviewViewContext.Provider>
    )
}

export default ShowReportReviewView
