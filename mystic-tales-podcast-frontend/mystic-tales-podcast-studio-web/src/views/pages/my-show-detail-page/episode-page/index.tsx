import { createContext, type FC, useEffect, useMemo, useRef, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Button, CircularProgress, Grid, IconButton, Rating, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { Add } from '@mui/icons-material';
import { useNavigate } from "react-router"

export const mockShowEpisodeList: any = {
    EpisodeList: [
        {
            Id: "3fa85f64-5717-4562-fjdf43-2c963f66afa6",
            Name: "Tech Talk Weekly",
            Description: "Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuầnBản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần",
            ReleaseDate: "2025-10-01T08:00:00.000Z",
            ExplicitContent: false,
            MainImageFileKey: "techtalk.jpg",
            TotalSave: 1200,
            ListenCount: 50000,
            SeasonNumber: 1,
            PodcastShowsSubscriptionType: {
                Id: 2,
                Name: "Trả phí"
            },
            PodcastShow: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "string",
                MainImageFileKey: "string"
            },
            TakenDownReason: "",
            UpdatedAt: "2025-10-15T08:00:00.000Z",
            CurrentStatus: {
                Id: 0,
                Name: "Ready to Release"
            }
        },
        {
            Id: "ep002",
            Name: "Chuyện đời thường",
            Description: "Những câu chuyện giản dị và sâu sắc",
            ReleaseDate: "2025-09-15T07:30:00.000Z",
            UploadFrequency: "Biweekly",
            ExplicitContent: true,
            MainImageFileKey: "life.jpg",
            TotalFollow: 800,
            ListenCount: 15000,
            RatingCount: 10,
            AverageRating: 3.5,
            PodcastCategory: {
                Id: 2,
                Name: "Đời sống"
            },
            PodcastSubCategory: {
                Id: 21,
                Name: "Tâm sự",
                PodcastCategoryId: 2
            },
            PodcastShowsSubscriptionType: {
                Id: 2,
                Name: "Trả phí"
            },
            TakenDownReason: "",
            UpdatedAt: "2025-10-10T07:30:00.000Z",
            CurrentStatus: {
                Id: 0,
                Name: "Published "
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Draft"
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Taken Down"
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Removed"
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Pending Review "
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Published "
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Pending Edit Required "
            }
        },
        {
            CurrentStatus: {
                Id: 0,
                Name: "Published "
            }
        }, {
            CurrentStatus: {
                Id: 0,
                Name: "Published "
            }
        },
    ]
};
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
                cellRenderer: (params: any) => {
                    return (
                        <div style={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: '12px',
                            padding: '8px 0',
                            width: '100%'
                        }}>
                            <img
                                src={`https://picsum.photos/300/300?random=${params.node.rowIndex}`}
                                alt={params.data.Name}
                                style={{
                                    width: '70px',
                                    height: '70px',
                                    borderRadius: '8px',
                                    objectFit: 'cover',
                                    flexShrink: 0
                                }}
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
                                <div style={{
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
                                </div>
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
                headerName: "Recently Updated",
                field: "UpdatedAt",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),

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
                    const status = params.data?.CurrentStatus?.Name?.trim() || '';
                    let color = '#888';
                    let bg = 'transparent';
                    switch (status) {
                        case 'Draft':
                            color = '#9e9e9e'; bg = 'rgba(158,158,158,0.15)'; // xám trung tính sáng
                            break;
                        case 'Pending Review':
                            color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.15)'; // vàng cam tươi
                            break;
                        case 'Pending Edit Required':
                            color = '#f06292'; bg = 'rgba(240, 98, 146, 0.15)'; // hồng sáng
                            break;
                        case 'Ready to Release':
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
        setState(state_creator(mockShowEpisodeList.EpisodeList, navigate));

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
            cellClass: 'd-flex align-items-center justify-content-center',
            editable: false
        };
    }, [])

    return (
        <ShowEpisodeViewContext.Provider value={{ handleDataChange }}>
            <div
                className="show-episode"
            >
                <div className="show-episode__header flex items-center justify-between mb-10 ">
                    <Typography variant="h4" className="show-episode__title" >
                        Episodes on Show <span className="text-primary ">({state?.rowData?.length || 0})</span>
                    </Typography>
                    <Button
                        variant="contained"
                        className="show-episode__btn--add"
                        startIcon={<Add />}
                    >
                        Add Episode
                    </Button>
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
                            <CircularProgress />
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
                        />
                    )}
                </div>
            </div>

        </ShowEpisodeViewContext.Provider>
    )
}

export default ShowEpisodeView
