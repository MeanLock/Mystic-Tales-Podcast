import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Button, CircularProgress, Grid, IconButton, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { Add} from '@mui/icons-material';

export const mockChannelShowList: any = {
    ChannelShowList: [
        {
            Name: "Tech Talk Weekly",
            Description: "Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuầnBản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần Bản tin công nghệ cập nhật mỗi tuần",
            ReleaseDate: "2025-10-01T08:00:00.000Z",
            UploadFrequency: "Weekly",
            MainImageFileKey: "techtalk.jpg",
            TotalFollow: 1200,
            ListenCount: 50000,
            PodcastCategory: {
                Id: 1,
                Name: "Công nghệ"
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
            UpdatedAt: "2025-10-15T08:00:00.000Z"
        },
        {
            Name: "Chuyện đời thường",
            Description: "Những câu chuyện giản dị và sâu sắc",
            ReleaseDate: "2025-09-15T07:30:00.000Z",
            UploadFrequency: "Biweekly",
            MainImageFileKey: "life.jpg",
            TotalFollow: 800,
            ListenCount: 15000,
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
            UpdatedAt: "2025-10-10T07:30:00.000Z"
        }
    ]
};
ModuleRegistry.registerModules([AllCommunityModule])

interface ChannelShowViewProps { }
interface ChannelShowViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const ChannelShowViewContext = createContext<ChannelShowViewContextProps | null>(null)

const state_creator = (table: any[]) => {
    const state = {
        columnDefs: [

            {
                headerName: "Show",
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
                                gap: '4px'
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
                                    lineHeight: '1.3',
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
            { headerName: "Category", field: "PodcastCategory.Name", },
            { headerName: "SubCategory", field: "PodcastSubCategory.Name" },
            {
                headerName: "Release Date",
                field: "ReleaseDate",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.ReleaseDate),

            },
            {
                headerName: "Recenly Update",
                field: "UpdatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),

            },
            { headerName: "Total Follow", field: "TotalFollow" },
            { headerName: "Listen Count", field: "ListenCount" },

            // {
            //     headerName: "Status",
            //     cellClass: 'd-flex align-items-center',
            //     flex: 0.7,
            //     cellRenderer: (params: { data: any }) => {
            //         let status = {
            //             title: '',
            //             color: '',
            //         };
            //         if (params.data.ResolvedAt !== null && params.data.ResolvedAt !== "") {
            //             status = {
            //                 title: 'Resolved',
            //                 color: 'success',
            //             };
            //         } else {
            //             status = {
            //                 title: 'Unresolved',
            //                 color: 'warning',
            //             };
            //         }
            //         return (
            //             <Cả
            //                 textColor={`${status.color}`}
            //                 style={{ width: '100px' }}
            //                 className={`text-center fw-bold rounded-pill px-1 border-2 border-${status.color} bg-light`}
            //             >
            //                 {status.title}
            //             </CCard>
            //         );
            //     },
            // },
            {
                headerName: "",
                cellClass: 'd-flex justify-content-center py-0',
                cellRenderer: (params: { data: any }) => {
                    return (
                        <IconButton >
                            <Eye size={27} color='var(--white-75)' />
                        </IconButton>

                    )

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
        setState(state_creator(mockChannelShowList.ChannelShowList));

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
        <ChannelShowViewContext.Provider value={{ handleDataChange }}>
            <Typography variant="h4" className="channel-show__title" >
                Shows on Channel <span className="text-primary ">({state?.rowData?.length || 0})</span>
            </Typography>
            <div className="flex justify-content-end mb-4">
                <Button
                    variant="contained"
                    className="channel-show__btn--add"
                                            startIcon={<Add />}
                    
                >
                    Add Show
                </Button>
            </div>

            <Grid  >
                <Grid  >
                    {isLoading ? (
                        <CircularProgress />
                    ) : (
                        <div
                            id="show-table"
                            style={{
                                height: "calc(100vh - 140px)", // Trừ header và margin

                                background: "#232323",
                                borderRadius: 12,
                            }}
                        >
                            <AgGridReact
                                columnDefs={state?.columnDefs}
                                rowData={state?.rowData}
                                defaultColDef={defaultColDef}
                                rowHeight={90}
                                headerHeight={50}
                                pagination={true}
                                paginationPageSize={8}
                                paginationPageSizeSelector={[8, 16, 24, 32]}
                                domLayout='normal'
                            />
                        </div>
                    )}
                </Grid>
            </Grid>


        </ChannelShowViewContext.Provider>
    )
}

export default ChannelShowView
