import { createContext, type FC, useEffect, useMemo, useRef, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { CircularProgress, IconButton, Typography } from "@mui/material"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from 'phosphor-react';
import { useNavigate } from "react-router"
import { adminAxiosInstance, loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { getBookingList } from "@/core/services/booking/booking.service"
import { RootState } from "@/redux/rootReducer"
import { useSelector } from "react-redux"
import Loading from "@/views/components/common/loading"



ModuleRegistry.registerModules([AllCommunityModule])

interface BookingPageProps { }
interface BookingPageContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
const state_creator = (table: any[], navigate: (path: string) => void) => {
    const state = {
        columnDefs: [
            {
                headerName: "No.",
                valueGetter: (params: any) => params.node.rowIndex + 1,
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 0.4,
                filter: false,
                tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`

            },
            {
                headerName: "Booking Title",
                field: "Title",
                flex: 1.5,
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`

            },
            {
                headerName: "Customer",
                field: "Account.FullName",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Price",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                cellRenderer: (params: any) => {
                    if (!params.data.Price) {
                        return (
                            <span>
                                ---
                            </span>
                        );
                    }
                    return (
                        <span>
                            {params.data.Price} Coin
                        </span>
                    );
                }
            },
            {
                headerName: "Deadline ",
                field: "Deadline",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                valueGetter: (params: any) => formatDate(params.data.Deadline),

            },
            {
                headerName: "Recently Updated",
                field: "UpdatedAt",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                valueGetter: (params: any) => formatDate(params.data.UpdatedAt),

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
                        case 'Quotation Request':
                        case 'Quotation Dealing':
                            color = '#ffb300';
                            bg = 'rgba(255, 179, 0, 0.15)'; // vàng cam
                            break;

                        case 'Pending Edit Required':
                            color = '#ffa726'; // cam nhạt
                            bg = '#ffa72626';
                            break;

                        case 'Producing':
                        case 'Track Previewing':
                        case 'Producing Requested':
                            color = '#61a7f2ff';
                            bg = '#29b6f626'; // xanh trời
                            break;

                        case 'Completed':
                            color = '#AEE339';
                            bg = 'rgba(174, 227, 57, 0.2)'; // xanh primary
                            break;

                        case 'Customer Cancel Request':
                        case 'Podcast Buddy Cancel Request':
                        case 'Quotation Rejected':
                        case 'Cancelled Automatically':
                        case 'Cancelled Manually':
                        case 'Quotation Cancelled':
                            color = '#ef5350';
                            bg = 'rgba(239, 83, 80, 0.15)'; // đỏ
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
                    const bookingId = data?.Id ?? null;
                    const disabled = !bookingId; // bookingId required; showId optional depending on your routing

                    const handleClick = () => {
                        if (!bookingId) return;
                        navigate(`/booking/${bookingId}`);

                    };

                    return (
                        <IconButton
                            onClick={handleClick}
                            disabled={disabled}
                            aria-label={disabled ? 'No bookingId' : 'Open booking'}
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
export const BookingPageContext = createContext<BookingPageContextProps | null>(null)

const BookingPage: FC<BookingPageProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const authSlice = useSelector((state: RootState) => state.auth.user);
    const [isLoading, setIsLoading] = useState<boolean>(false);
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
    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const bookingList = await getBookingList(loginRequiredAxiosInstance);
            if (bookingList.success) {
                setState(state_creator(bookingList.data.BookingList, navigate));
            } else {
                console.error('API Error:', bookingList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch booking list:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        console.log("authSlice in booking page:", authSlice);
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
        <BookingPageContext.Provider value={{ handleDataChange }}>
            <div className="booking-page">
                <div className="booking-page__header flex items-center justify-between mb-10 ">
                    <Typography variant="h4" className="booking-page__title" >
                        Booking Management <span className="text-primary "> ({state?.rowData?.length || 0})</span>
                    </Typography>
                </div>

                <div
                    id="booking-table"
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
        </BookingPageContext.Provider>
    )
}

export default BookingPage;

