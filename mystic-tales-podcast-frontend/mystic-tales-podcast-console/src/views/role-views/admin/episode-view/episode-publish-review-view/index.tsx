import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { managerAxiosInstance } from "../../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import SurveyTalkLoading from "../../../../components/common/loading"
import { formatDate } from "../../../../../core/utils/date.util"
export const mockReviewSessionList: any = {
    ReviewSessionList: [
        {
            Id: 1,
            AssignedStaffId: 101,
            PodcastEpisodeId: "ep_001",
            Note: "Kiểm tra chất lượng âm thanh lần đầu.",
            ReReviewCount: 0,
            Deadline: "2025-10-12T10:31:58.311Z",
            CreatedAt: "2025-10-09T10:31:58.311Z",
            UpdatedAt: "2025-10-09T10:31:58.311Z",
        },
        {
            Id: 2,
            AssignedStaffId: 102,
            PodcastEpisodeId: "ep_002",
            Note: "Cần xem xét lại nội dung có bản quyền.",
            ReReviewCount: 1,
            Deadline: "2025-10-15T10:31:58.311Z",
            CreatedAt: "2025-10-09T10:31:58.311Z",
            UpdatedAt: "2025-10-09T10:31:58.311Z",
        },
        {
            Id: 3,
            AssignedStaffId: 103,
            PodcastEpisodeId: "ep_003",
            Note: "Rà soát lại transcript để tránh lỗi chính tả.",
            ReReviewCount: 2,
            Deadline: "2025-10-20T10:31:58.311Z",
            CreatedAt: "2025-10-09T10:31:58.311Z",
            UpdatedAt: "2025-10-09T10:31:58.311Z",
        },
    ],
};
ModuleRegistry.registerModules([AllCommunityModule])

interface EpisodePublishRequestReviewViewProps { }
interface EpisodePublishRequestReviewViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const EpisodePublishRequestReviewViewContext = createContext<EpisodePublishRequestReviewViewContextProps | null>(null)

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
            }, { headerName: "Assigned Staff ID", field: "AssignedStaffId", flex: 0.8 },
            { headerName: "Podcast Episode ID", field: "PodcastEpisodeId", flex: 0.8 },
            { headerName: "Note", field: "Note", flex: 0.8 },
            { headerName: "Re-Review Count", field: "ReReviewCount", flex: 0.8 },
            {
                headerName: "Deadline",
                field: "Deadline",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.Deadline),

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

            }
        ],
        rowData: table

    }
    return state
}



const EpisodePublishRequestReviewView: FC<EpisodePublishRequestReviewViewProps> = () => {
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
        setState(state_creator(mockReviewSessionList.ReviewSessionList));

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
        <EpisodePublishRequestReviewViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Episode Publish Request Review Sessions</h4>

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


        </EpisodePublishRequestReviewViewContext.Provider>
    )
}

export default EpisodePublishRequestReviewView
