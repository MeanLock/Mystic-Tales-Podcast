import { createContext, type FC, use, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CButtonGroup, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "@/core/utils/date.util"
import { Eye } from "phosphor-react"
import Loading from "@/views/components/common/loading"
import { getBookingList } from "@/core/services/booking/booking.service"
import IconButton from "@mui/material/IconButton"
import { useNavigate } from "react-router-dom"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"

ModuleRegistry.registerModules([AllCommunityModule])

interface BuddyReportReviewViewProps { }
interface BuddyReportReviewViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const BuddyReportReviewViewContext = createContext<BuddyReportReviewViewContextProps | null>(null)

const state_creator = (table: any[], navigate?: (path: string) => void) => {
    const state = {
        columnDefs: [
            {
                headerName: "No.",
                valueGetter: (params: any) => params.node.rowIndex + 1,
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 0.4,
                filter: false
            },
            {
                headerName: "Booking Title",
                field: "Title",
                flex: 1.5,
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Customer",
                field: "Account.FullName",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Podcast Buddy",
                field: "PodcastBuddy.FullName",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Staff",
                field: "AssignedStaff.Email",
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
                            {params.data.Price} Coins
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
                comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                    const dateA = new Date(nodeA.data.UpdatedAt).getTime();
                    const dateB = new Date(nodeB.data.UpdatedAt).getTime();
                    return dateA - dateB;
                },
            },

            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center justify-content-center',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                flex: 1.1,
                valueGetter: (params: any) => {
                    return params.data?.CurrentStatus?.Name?.trim() || '';
                },
                cellRenderer: (params: any) => {
                    const status = params.data?.CurrentStatus?.Name?.trim() || '';
                    let color = '#888';
                    let bg = 'transparent';

                    switch (status) {
                        case 'Quotation Request':
                        case 'Quotation Dealing':
                        case 'Quotation Cancelled':
                            color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.07)';
                            break;

                        case 'Pending Edit Required':
                            color = '#ffa726'; // cam nhạt
                            bg = 'rgba(255, 167, 38, 0.15)';
                            break;

                        case 'Producing':
                        case 'Track Previewing':
                        case 'Producing Requested':
                            color = '#61a7f2ff';
                            bg = 'rgba(41, 182, 246, 0.15)'; // xanh trời
                            break;

                        case 'Completed':
                            color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)';
                            break;

                        case 'Customer Cancel Request':
                        case 'Podcast Buddy Cancel Request':
                        case 'Quotation Rejected':
                        case 'Cancelled Automatically':
                        case 'Cancelled Manually':
                            color = '#ef5350';
                            bg = 'rgba(251, 222, 227, 0.2)'; // đỏ
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
                            <Eye size={27} color='var(--secondary-green)' />
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



const BookingView: FC<BuddyReportReviewViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const navigate = useNavigate();
    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const reportList = await getBookingList(adminAxiosInstance);
            console.log("Fetched buddy reports:", reportList);
            if (reportList.success) {
                setState(state_creator(reportList.data.BookingList, navigate));
            } else {
                console.error('API Error:', reportList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch buddy reports:', error);
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
        <BuddyReportReviewViewContext.Provider value={{ handleDataChange }}>
            {/* <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}> Booking Request</h4>
            </div> */}
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


        </BuddyReportReviewViewContext.Provider>
    )
}

export default BookingView
