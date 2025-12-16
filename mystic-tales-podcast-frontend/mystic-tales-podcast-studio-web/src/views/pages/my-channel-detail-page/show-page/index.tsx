import { createContext, type FC, useEffect, useMemo, useRef, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Button, CircularProgress, Grid, IconButton, Rating, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { Add } from '@mui/icons-material';
import { useNavigate, useParams } from "react-router-dom"
import { getChannelDetail } from "@/core/services/channel/channel.service"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2/login-required-axios-instance"
import Image from "@/views/components/common/image"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import ShowCreate from "./ShowCreateModal"
import { renderDescriptionHTML } from "@/core/utils/htmlRender.utils"

export const mockChannelShowList: any = {
    ChannelShowList: [
        {
            Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
            Name: "Tech Talk Weekly",
            Description: "Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuầnBản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần",
            ReleaseDate: "2025-10-01T08:00:00.000Z",
            UploadFrequency: "Weekly",
            MainImageFileKey: "techtalk.jpg",
            TotalFollow: 1200,
            ListenCount: 50000,
            RatingCount: 0,
            AverageRating: 0,
            PodcastCategory: {
                Id: 1,
                Name: "Society & Culture "
            },
            PodcastSubCategory: {
                Id: 21,
                Name: "Europe Folklore Horror",
                PodcastCategoryId: 2
            },
            PodcastShowsSubscriptionType: {
                Id: 2,
                Name: "Trả phí"
            },
            TakenDownReason: "",
            UpdatedAt: "2025-10-15T08:00:00.000Z",
            CurrentStatus: {
                Id: 0,
                Name: "Ready to Release"
            }
        },
        {
            Id: "4fa85f64-5717-4562-b3fc-2c963f66afb7",
            Name: "Chuyện đời thường",
            Description: "Những câu chuyện giản dị và sâu sắc",
            ReleaseDate: "2025-09-15T07:30:00.000Z",
            UploadFrequency: "Biweekly",
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

    ]
};
ModuleRegistry.registerModules([AllCommunityModule])

interface ChannelShowViewProps { }
interface ChannelShowViewContextProps {
    handleDataChange: () => void
    navigate?: (path: string) => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const ChannelShowViewContext = createContext<ChannelShowViewContextProps | null>(null)

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
                                    {renderDescriptionHTML(params.data.Description)}
                                </div> */}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Category", field: "PodcastCategory.Name", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            // { headerName: "SubCategory", field: "PodcastSubCategory.Name", flex: 1.5, cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem' } },
            {
                headerName: "Rating",
                field: "AverageRating",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.5rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                cellRenderer: (params: any) => {
                    const rating = params.data.AverageRating;
                    const ratingCount = params.data.RatingCount;
                    const displayRating = ratingCount === 0 ? 5 : rating;
                    return (
                        <div style={{ display: 'flex', alignItems: 'center' }}>
                            <Rating
                                name="custom-rating"
                                value={displayRating}
                                size="small"
                                precision={0.1}
                                readOnly
                                sx={{
                                    '& .MuiRating-iconFilled': {
                                        color: '#FFD700'
                                    },
                                    '& .MuiRating-iconEmpty': {
                                        color: '#d1d1d1', // màu sao khi trống
                                    },
                                }}
                            />
                            <span style={{ fontSize: '0.6rem', marginLeft: '4px' }}>
                                ({ratingCount})
                            </span>
                        </div>
                    );
                },
            },

            {
                headerName: "Subscription",
                field: "PodcastShowSubscriptionType.Name",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },

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
                cellRenderer: (params: any) => {
                    const status = params.data?.CurrentStatus?.Name || '';
                    let color = '#888';
                    let bg = 'transparent';
                    switch (status) {
                        case 'Draft':
                            color = '#9e9e9e'; bg = 'rgba(158,158,158,0.15)'; // xám trung tính sáng
                            break;
                        case 'Ready To Release':
                            color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.15)'; // vàng cam tươi
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
                    const showId = data?.Id;
                    const disabled = !showId; // episodeId required; showId optional depending on your routing

                    const handleClick = () => {
                        if (!showId) return;
                        window.location.replace(`/show/${showId}/overview`);

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



const ChannelShowView: FC<ChannelShowViewProps> = () => {
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

    const fetchChannelDetail = async () => {
        setIsLoading(true);
        try {
            const res = await getChannelDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched channel detail:", res.data.Channel);
            if (res.success && res.data) {
                const ch = res.data.Channel;
                setState(state_creator(ch.ShowList, navigate));

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
        fetchChannelDetail()
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
        <ChannelShowViewContext.Provider value={{ handleDataChange: fetchChannelDetail }}>
            <div
                className="channel-show"
            >
                <div className="channel-show__header flex items-center justify-between mb-10 ">
                    <Typography variant="h4" className="channel-show__title" >
                        Shows on Channel <span className="text-primary ">({state?.rowData?.length || 0})</span>
                    </Typography>

                    <Modal_Button
                        className="channel-show__btn--add"
                        content="New Show"
                        variant="contained"
                        size='lg'
                        startIcon={<Add />}
                    >
                        <ShowCreate />
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
                            tooltipShowDelay={0}
                        />
                    )}
                </div>
            </div>

        </ChannelShowViewContext.Provider>
    )
}

export default ChannelShowView
