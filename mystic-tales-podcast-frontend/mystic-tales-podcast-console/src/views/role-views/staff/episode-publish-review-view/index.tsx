import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "@/core/utils/date.util"
import SurveyTalkLoading from "@/views/components/common/loading"
import { Eye } from "phosphor-react"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import EpisodePublishDetail from "./EpisodePublishDetail"
export const mockReviewSessionList: any = {
    ReviewSessionList: [
        {
            Id: 1,
            AssignedStaffId: 101,
            PodcastEpisode: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Title: "ep 1"
            },
            Note: "Kiểm tra chất lượng âm thanh lần đầu.",
            ReReviewCount: 0,
            Deadline: "2025-10-12T10:31:58.311Z",
            CreatedAt: "2025-10-09T10:31:58.311Z",
            UpdatedAt: "2025-10-09T10:31:58.311Z",
        },
        {
            Id: 2,
            AssignedStaffId: 102,
            PodcastEpisode: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Title: "ep 2"
            },
            Note: "Cần xem xét lại nội dung có bản quyền.",
            ReReviewCount: 1,
            Deadline: "2025-10-15T10:31:58.311Z",
            CreatedAt: "2025-10-09T10:31:58.311Z",
            UpdatedAt: "2025-10-09T10:31:58.311Z",
        },
        {
            Id: 3,
            AssignedStaffId: 103,
            PodcastEpisode: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Title: "ep 3"
            },
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
            },
            { headerName: "Podcast Episode ", field: "PodcastEpisode.Title", flex: 0.8 },
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

            },
            {
                headerName: "Action",
                cellClass: 'd-flex justify-content-center py-0',
                cellRenderer: (params: { data: any }) => {
                    const Modal_props = {
                        detailForm: <EpisodePublishDetail podcastEpisodePublishReviewSessionId ={params.data.Id} onClose={() => { }} />,
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
            <h3 className="mb-4 fw-bold" style={{ color: 'var(--primary-grey)', borderBottom: '2px solid var(--primary-grey)', paddingBottom: '0.7rem' }}>Publish Request</h3>
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
