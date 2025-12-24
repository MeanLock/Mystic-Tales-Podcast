import { createContext, type FC, use, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from "phosphor-react"
import Loading from "@/views/components/common/loading"
import { getEpisodePublishList } from "@/core/services/ReviewSession/review-session.service"
import { staffAxiosInstance } from "@/core/api/rest-api/config/instances/v2/staff-axios-instance"
import { useNavigate } from "react-router-dom"

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

const state_creator = (table: any[], navigate: any) => {
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
            { headerName: "Podcast Episode ", field: "PodcastEpisode.Name", flex: 1.5 },
            { headerName: "Note", field: "Note", flex: 0.8 ,valueGetter: (params: any) => (params.data.Note ? params.data.Note : '---')},
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
                flex: 0.7,
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),
                comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                    const dateA = new Date(nodeA.data.CreatedAt).getTime();
                    const dateB = new Date(nodeB.data.CreatedAt).getTime();
                    return dateA - dateB;
                },
            },
            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center justify-content-center',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 1.1,
                valueGetter: (params: any) => params.data?.CurrentStatus?.Name.trim() || '',
                cellRenderer: (params: any) => {
                    const status = params.data?.CurrentStatus?.Name || '';
                    let color = '#888';
                    let bg = 'transparent';

                    switch (status) {
                        case 'Pending Review':
                            color = '#ffb300';
                            bg = 'rgba(255, 179, 0, 0.07)';
                            break;
                        case 'Accepted':
                            color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)';
                            break;

                        case 'Discard':
                        case 'Rejected':
                            color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)';
                            break;

                        default:
                            color = '#9e9e9e';
                            bg = 'rgba(158, 158, 158, 0.15)'; // xám nếu không khớp
                    }

                    return (
                        <span
                            style={{
                                display: 'inline-block',
                                minWidth: 100,
                                padding: '0 10px',
                                borderRadius: 50,
                                fontWeight: 700,
                                fontSize: '0.75rem',
                                color,
                                background: bg,
                                textAlign: 'center',
                                border: `2px solid ${color}`,
                            }}
                        >
                            {status === 'Podcast Buddy Cancel Request' ? 'Buddy Cancel Request' : status}
                        </span>
                    );
                },
            },
           {
        headerName: "Actions",
        cellClass: "d-flex justify-content-center py-0",
        flex: 0.5,
        cellRenderer: (params: any) => {
          const PublishReviewSessionId = params.data.Id
          return (
            <div className="d-flex gap-2 align-items-center h-100">
              <CButton
                onClick={() => navigate("/staff/publish-review-sessions/" + PublishReviewSessionId)}
              >
                <Eye size={27} color='var(--secondary-green)' />
              </CButton>
            </div>
          )
        },
      },
        ],
        rowData: table

    }
    return state
}



const EpisodePublishRequestReviewView: FC<EpisodePublishRequestReviewViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const navigate = useNavigate();
    const handleDataChange = async () => {
      setIsLoading(true);
      try {
        const res = await getEpisodePublishList(staffAxiosInstance);
        console.log('Episode Publish Review Sessions:', res.data);
        if (res.success) {
          setState(state_creator(res.data.ReviewSessionList,navigate));
        } else {
          console.error('API Error:', res.message);
        }
      } catch (error) {
        console.error('Lỗi khi fetch customer accounts:', error);
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
        <EpisodePublishRequestReviewViewContext.Provider value={{ handleDataChange }}>
            <h3 className="mb-4 fw-bold" style={{ color: 'var(--primary-grey)', borderBottom: '2px solid var(--primary-grey)', paddingBottom: '0.7rem' }}>Publish Request</h3>
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


        </EpisodePublishRequestReviewViewContext.Provider>
    )
}

export default EpisodePublishRequestReviewView
