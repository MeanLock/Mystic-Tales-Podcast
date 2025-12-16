import { createContext, type FC, useEffect, useMemo, useRef, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Button, CircularProgress, Grid, IconButton, Rating, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { Add } from '@mui/icons-material';
import { useNavigate, useParams } from "react-router"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import EpisodeCreate from "./EpisodeCreate"
import { getShowDetail } from "@/core/services/show/show.service"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Image from "@/views/components/common/image"
import Loading from "@/views/components/common/loading"


ModuleRegistry.registerModules([AllCommunityModule])

interface ShowEpisodeViewProps { }
interface ShowEpisodeViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const ShowEpisodeViewContext = createContext<ShowEpisodeViewContextProps | null>(null)

const state_creator = (table: any[], navigate: (path: string) => void) => {
    const state = {
        columnDefs: [

            {
                headerName: "Episode",
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
                                    color: 'var(--primary-green)',
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
                                    {params.data.Description}
                                </div> */}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Explicit Content", field: "ExplicitContent", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            // { headerName: "SubCategory", field: "PodcastSubCategory.Name", flex: 1.5, cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem' } },
            {
                headerName: "Season", field: "SeasonNumber", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Subscription",
                field: "PodcastEpisodeSubscriptionType.Name",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },

            },
            {
                headerName: "Total Save", field: "TotalSave", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Listen Count", field: "ListenCount", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },

            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center justify-content-center',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 1.1,
                cellRenderer: (params: any) => {
                    const status = params.data?.CurrentStatus?.Name || '';
                    let color = '#888';
                    let bg = 'transparent';
                    switch (status) {
                        case 'Draft':
                            color = '#9e9e9e'; bg = 'rgba(158,158,158,0.15)'; // xám trung tính sáng
                            break;
                        case 'Pending Edit Required':
                        case 'Pending Review':
                            color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.15)'; // vàng cam tươi
                            break;
                        case 'Audio Processing':
                        case 'Ready To Release':
                            color = '#61a7f2ff'; bg = 'rgba(41, 182, 246, 0.15)'; // xanh trời tươi
                            break;
                        case 'Published':
                            color = '#AEE339'; bg = 'rgba(174, 227, 57, 0.2)'; // xanh primary của bạn
                            break;
                        case 'Taken Down':
                            color = '#ef5350'; bg = 'rgba(255, 234, 237, 0.15)'; // đỏ dịu mắt
                            break;
                        case 'Removed':
                            color = '#ef5350'; bg = 'rgba(255, 234, 237, 0.15)'; // đỏ dịu mắt
                            break;
                        default:
                            color = '#ef5350'; bg = 'rgba(239, 83, 80, 0.15)'; // đỏ dịu mắt
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
                                border: `1.5px solid ${color}`,
                            }}
                        >
                            {status}
                        </span>
                    );
                },
            },
            {
                headerName: "",
                cellClass: 'd-flex justify-content-center py-0',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                cellRenderer: (params: { data: any }) => {
                    const data = params.data || {};
                    const showId = data?.PodcastShow?.Id ?? data?.PodcastShowId ?? null;
                    const episodeId = data?.Id ?? data?.EpisodeId ?? null;
                    const disabled = !episodeId; // episodeId required; showId optional depending on your routing

                    const handleClick = () => {
                        if (!episodeId) return;
                        navigate(`${episodeId}`);

                    };

                    return (
                        <IconButton
                            onClick={handleClick}
                            disabled={disabled}
                            aria-label={disabled ? 'No episode' : 'Open episode'}
                            sx={{ color: disabled ? 'rgba(255,255,255,0.25)' : 'var(--white-75)' }}
                        >
                            <Eye size={27} />
                        </IconButton>
                    );
                },
                flex: 0.5,
                filter: false,
                resizable: false,
                sortable: false,
            }
        ],
        rowData: table

    }
    return state
}



const ShowEpisodeView: FC<ShowEpisodeViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const { id } = useParams<{ id: string }>();
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const gridRef = useRef<any>(null);
    const navigate = useNavigate();
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
    const fetchShowDetail = async () => {
        setIsLoading(true);
        try {
            const res = await getShowDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched show detail:", res.data.Show);
            if (res.success && res.data) {
                const ch = res.data.Show;
                setState(state_creator(ch.EpisodeList, navigate));

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
        fetchShowDetail()
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
        <ShowEpisodeViewContext.Provider value={{ handleDataChange: fetchShowDetail }}>
            <div
                className="show-episode"
            >
                <div className="show-episode__header flex items-center justify-between mb-10 ">
                    <Typography variant="h4" className="show-episode__title" >
                        Episodes on Show <span className="text-primary ">({state?.rowData?.length || 0})</span>
                    </Typography>
                    <Modal_Button
                        className="show-episode__btn--add"
                        content="New Episode"
                        variant="contained"
                        size='lg'
                        startIcon={<Add />}
                    >
                        <EpisodeCreate />
                    </Modal_Button>

                </div>

                <div
                    id="show-table"
                    style={{
                        flex: 1, // chiếm toàn bộ phần còn lại
                        overflow: "hidden", // chặn scroll ngoài
                    }}
                >
                    {isLoading ? (
                        <div
                            style={{
                                display: "flex",
                                alignItems: "center",
                                justifyContent: "center",
                                height: "100%",
                            }}
                        >
                            <Loading />
                        </div>
                    ) : (
                        <AgGridReact
                            ref={gridRef}
                            columnDefs={state?.columnDefs}
                            rowData={state?.rowData}
                            defaultColDef={defaultColDef}
                            rowHeight={90}
                            headerHeight={50}
                            pagination={true}
                            paginationPageSize={10}
                            paginationPageSizeSelector={[10, 16, 24, 32]}
                            domLayout="normal"
                            tooltipShowDelay={0}
                        />
                    )}
                </div>
            </div>

        </ShowEpisodeViewContext.Provider>
    )
}

export default ShowEpisodeView
