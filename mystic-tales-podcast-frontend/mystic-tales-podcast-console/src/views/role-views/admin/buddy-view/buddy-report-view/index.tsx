import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { managerAxiosInstance } from "../../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import SurveyTalkLoading from "../../../../components/common/loading"
import { formatDate } from "../../../../../core/utils/date.util"
export const mockList: any = {
    BuddyReportList: [
        {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            Content: "This episode contains inappropriate language.",
            AccountId: 101,
            PodcastBuddyId: "c7e3c9e0-12b4-45d1-bb25-91ffb44d8f6a",
            PodcastBuddyReportType: {
                Id: 1,
                Name: "Scam / Fraud"
            },
            ResolvedAt: "2025-10-10T10:07:05.532Z",
            CreatedAt: "2025-10-10T09:45:12.210Z",
        },
        {
            Id: "c2a34b12-0b92-4f78-9fd3-914e0d567a01",
            Content: "Reported due to misleading information.",
            AccountId: 102,
            PodcastBuddyId: "a7b2e6b9-5f41-4a1e-80c2-26d37f6a6a90",
            PodcastBuddyReportType: {
                Id: 2,
                Name: "Spam"
            },
            ResolvedAt: "2025-10-09T15:22:40.100Z",
            CreatedAt: "2025-10-09T13:00:00.000Z",
        },
        {
            Id: "f6c71c2e-8a93-47df-bd51-f60aee1df6c9",
            Content: "Contains copyrighted background music.",
            AccountId: 103,
            PodcastBuddyId: "9c73b3d0-1c41-4a8e-9d8c-11c5e6c74a01",
            PodcastBuddyReportType: {
                Id: 4,
                Name: "Hate Speech"
            },
            ResolvedAt: "",
            CreatedAt: "2025-10-10T08:15:42.200Z",
        },
    ],
};
ModuleRegistry.registerModules([AllCommunityModule])

interface BuddyReportViewProps { }
interface BuddyReportViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const BuddyReportViewContext = createContext<BuddyReportViewContextProps | null>(null)

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
            { headerName: "Content", field: "Content", flex: 0.8 },
            { headerName: "Account ID", field: "AccountId", flex: 0.8 },
            { headerName: "Podcast Buddy ID", field: "PodcastBuddyId", flex: 0.8 },
            { headerName: "Podcast Show Report Type ", field: "PodcastBuddyReportType.Name", flex: 0.8 },
            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center',
                flex: 0.7,
                cellRenderer: (params: { data: any }) => {
                    let status = {
                        title: '',
                        color: '',
                    };
                    if (params.data.ResolvedAt !== null && params.data.ResolvedAt !== "") {
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
        ],
        rowData: table

    }
    return state
}



const BuddyReportView: FC<BuddyReportViewProps> = () => {
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
        setState(state_creator(mockList.BuddyReportList));

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
        <BuddyReportViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Buddy Reports</h4>
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


        </BuddyReportViewContext.Provider>
    )
}

export default BuddyReportView
