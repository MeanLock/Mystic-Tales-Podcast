
import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import {
    CButton,
    CButtonGroup,
    CCard,
    CCol,
    CRow,
    CSpinner,
    CModal,
    CModalHeader,
    CModalTitle,
    CModalBody,
    CModalFooter,
} from "@coreui/react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import { Eye } from "phosphor-react"
import { getTransactionList } from "@/core/services/transaction/transaction.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Loading from "@/views/components/common/loading"
import { getBookingHoldingList } from "@/core/services/booking/booking.service"
import { getSubscriptionHoldingList } from "@/core/services/subscription/subscription.service"

ModuleRegistry.registerModules([AllCommunityModule])


type BookingHoldingViewProps = {}

interface BookingHoldingViewContextProps {
    handleDataChange: () => void
}

interface GridState {
    columnDefs: any[]
    rowData: any[]
}

export const BookingHoldingViewContext = createContext<BookingHoldingViewContextProps | null>(null)

const state_creator = (table: any[]) => {


    const state = {
        columnDefs: [
            {
                headerName: "No.",
                flex: 0.4, 
                field: "Id"
            },
            { headerName: "Title", field: "Title", flex: 0.9 },
            { headerName: "Customer", field: "Account.Email", flex: 0.9 },
            {
                headerName: "PodcastBuddy",
                field: "PodcastBuddy.Email",
                flex: 0.8,
            },

            {
                headerName: "Total Price",
                field: "Price",
                flex: 1
            },
             {
                headerName: "Holding Amount",
                field: "HoldingAmount",
                flex: 1
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
        ],
        rowData: table,
    }
    return state
}



const BookingHoldingView: FC<BookingHoldingViewProps> = () => {
    const [state, setState] = useState<GridState | null>(null)
    const [isLoading, setIsLoading] = useState<boolean>(true);


    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const res = await getBookingHoldingList(adminAxiosInstance);
            console.log("Fetched transaction list:", res);
            if (res.success) {
                setState(state_creator(res.data.BookingList || []));
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch booking holding list:', error);
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
            cellClass: "d-flex align-items-center",
            editable: false,
        }
    }, [])

    return (
        <BookingHoldingViewContext.Provider
            value={{
                handleDataChange: handleDataChange,
            }}
        >
            <CRow>
                <h3 className="transaction__title mb-5">Booking Holding</h3>
                <CCol xs={12}>
                    {isLoading ? (
                        <div className="flex justify-center items-center h-100" >
                            <Loading />
                        </div>
                    ) : (
                        <div id="withdrawal-table" className="">
                            <AgGridReact
                                columnDefs={state.columnDefs || []}
                                rowData={state.rowData}
                                defaultColDef={defaultColDef}
                                rowHeight={70}
                                headerHeight={40}
                                pagination={true}
                                paginationPageSize={10}
                                paginationPageSizeSelector={[10, 20, 50, 100]}
                                domLayout="autoHeight"
                            />
                        </div>
                    )}
                </CCol>
            </CRow>

        </BookingHoldingViewContext.Provider>
    )
}

export default BookingHoldingView
