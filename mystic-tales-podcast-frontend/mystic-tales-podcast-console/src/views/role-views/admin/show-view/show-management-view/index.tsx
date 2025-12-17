import { createContext, type FC, useEffect, useMemo, useRef, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Button, CircularProgress, Grid, IconButton, Rating, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { Add } from '@mui/icons-material';
import { useNavigate, useParams } from "react-router-dom"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2/login-required-axios-instance"
import Image from "@/views/components/common/image"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { getShowList } from "@/core/services/show/show.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { CButton, CButtonGroup, CCol, CRow } from "@coreui/react"
import Loading from "@/views/components/common/loading"


ModuleRegistry.registerModules([AllCommunityModule])

interface ShowManagementViewProps { }
interface ShowManagementViewContextProps {
    handleDataChange: () => void
    navigate?: (path: string) => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const ShowManagementViewContext = createContext<ShowManagementViewContextProps | null>(null)

const state_creator = (table: any[], navigate?: (path: string) => void) => {
    const state = {
        columnDefs: [

            {
                headerName: "Show",
                flex: 2,
                cellClass: 'd-flex align-items-center',
                tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`,
                cellRenderer: (params: any) => {
                    return (
                        <div style={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: '12px',
                            padding: '8px 0',
                            width: '100%'
                        }}>
                            <Image
                                mainImageFileKey={params.data.MainImageFileKey}
                                alt={params.data.Name}
                                className="w-[60px] h-[60px] rounded-[8px] object-cover flex-shrink-0"
                            />
                            <div style={{
                                flex: 1,
                                minWidth: 0,
                                display: 'flex',
                                flexDirection: 'column',
                                gap: '4px',
                                textAlign: 'left'
                            }}>
                                <div style={{
                                    fontWeight: 'bold',
                                    fontSize: '0.8rem',
                                    lineHeight: '1.2'
                                }}>
                                    {params.data.Name}
                                </div>
                                {/* <div style={{
                                    fontSize: '0.6rem',
                                    color: 'var(--white-75)',
                                    lineHeight: '1.5',
                                    overflow: 'hidden',
                                    textOverflow: 'ellipsis',
                                    display: '-webkit-box',
                                    WebkitLineClamp: 2,
                                    WebkitBoxOrient: 'vertical'
                                }}>
                                    {renderDescriptionHTML(params.data.Description)}
                                </div> */}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Podcaster", field: "Podcaster.FullName", cellStyle: { overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            // { headerName: "SubCategory", field: "PodcastSubCategory.Name", flex: 1.5, cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem' } },


            {
                headerName: "Subscription",
                field: "PodcastShowSubscriptionType.Name",
                cellStyle: { display: 'flex', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },

            },
            {
                headerName: "Total Follow", field: "TotalFollow", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Listen Count", field: "ListenCount", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },

            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center justify-content-center',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 1.1,
                 valueGetter: (params: any) => {
                    return params.data?.CurrentStatus?.Name.trim() || '';
                },
                cellRenderer: (params: any) => {
                    const status = params.data?.CurrentStatus?.Name || '';
                    let color = '#888';
                    let bg = 'transparent';
                    switch (status) {
                        case 'Draft':
                            color = '#9e9e9e'; bg = 'rgba(158, 158, 158, 0.1)'; // xám trung tính sáng
                            break;
                        case 'Ready To Release':
                            color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.07)'; // vàng cam tươi
                            break;
                        case 'Published':
                            color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)'; // xanh primary của bạn
                            break;
                        case 'Audio Processing':
                            color = 'rgba(57, 184, 227, 1)'; bg = 'rgba(57, 184, 227, 0.15)'; // xanh dương dịu mắt
                            break;  
                        case 'Taken Down':
                            color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)'; // đỏ dịu mắt
                            break;
                        case 'Removed':
                            color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)'; // đỏ dịu mắt
                            break;
                        default:
                            color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)'; // đỏ dịu mắt
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
                            {status}
                        </span>
                    );
                },
            },
                {
                  headerName: "Actions",
                  cellClass: "d-flex justify-content-center py-0",
                  flex: 0.5,
                  cellRenderer: (params: any) => {
                    const showId = params.data.Id
                    return (
                      <div className="d-flex gap-2 align-items-center h-100">
                        <CButton
                          onClick={() => navigate("/show/" + showId)}
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



const ShowManagementView: FC<ShowManagementViewProps> = () => {
    const { id } = useParams<{ id: string }>();
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const navigate = useNavigate();
    const gridRef = useRef<any>(null);
    useEffect(() => {
        if (
            state &&
            gridRef.current &&
            gridRef.current.columnApi &&
            typeof gridRef.current.columnApi.getAllColumns === "function"
        ) {
            const allColumnIds = gridRef.current.columnApi.getAllColumns().map((col: any) => col.colId);
            gridRef.current.columnApi.autoSizeColumns(allColumnIds, false);
        }
    }, [state]);

    const fetchShowList = async () => {
        setIsLoading(true);
        try {
            const res = await getShowList(loginRequiredAxiosInstance);
            console.log("Fetched show list:", res.data.ShowList);
            if (res.success && res.data) {
                const shows = res.data.ShowList;
                setState(state_creator(shows, navigate));

            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch channel list:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchShowList()
    }, [])

    const defaultColDef = useMemo(() => {
        return {
            flex: 1,
            filter: true,
            autoHeight: true,
            resizable: true,
            wrapText: true,
            cellClass: 'd-flex align-items-center justify-content-center',
            editable: false
        };
    }, [])

    return (
        <ShowManagementViewContext.Provider value={{ handleDataChange: fetchShowList }}>
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

        </ShowManagementViewContext.Provider>
    )
}

export default ShowManagementView
