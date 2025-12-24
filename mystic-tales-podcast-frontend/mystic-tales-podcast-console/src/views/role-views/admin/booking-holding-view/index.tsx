
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
import { getBookingTransactionList } from "@/core/services/booking/booking.service"
import './styles.scss'
import TransactionListModal from "./TransactionListModal"
import { formatDate } from "@/core/utils/date.util"
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

const state_creator = (table: any[], handleShowTransactions: (data: any) => void) => {

    const state = {
        columnDefs: [
            {
                headerName: "No.",
                flex: 0.4,
                field: "Id"
            },
            { headerName: "Title", field: "Title", flex: 0.9 },
            { headerName: "Customer", field: "Account.FullName", flex: 0.9 },
            {
                headerName: "PodcastBuddy",
                field: "PodcastBuddy.FullName",
                flex: 0.8,
            },

            {
                headerName: "Total Price",
                field: "Price",
                flex: 1,
                valueGetter: (params: any) => {
                    return params.data.Price ? params.data.Price.toLocaleString() : '---';
                }
            },
            {
                headerName: "Amount",
                field: "Amount",
                flex: 1,
valueGetter: (params: any) => {
                    return params.data.Amount ? params.data.Amount.toLocaleString() : '---';
                }
            },
            {
                headerName: "Created At",
                field: "CreatedAt",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),
                comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                    const dateA = new Date(nodeA.data.CreatedAt).getTime();
                    const dateB = new Date(nodeB.data.CreatedAt).getTime();
                    return dateA - dateB;
                },
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
                        case 'Holding':
                            color = '#61a7f2ff';
                            bg = 'rgba(41, 182, 246, 0.15)'; // xanh trời
                            break;

                        case 'Profit':
                            color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)';
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
                            {status}
                        </span>
                    );
                },
            },
            {
                headerName: '',
                cellClass: 'd-flex justify-content-center align-items-center',
                flex: 0.8,
                cellRenderer: (params: any) => {
                    return (
                        <button
                            onClick={() => handleShowTransactions(params.data)}
                            style={{
                                background: 'none',
                                border: 'none',
                                cursor: 'pointer',
                                padding: '4px 8px',
                                display: 'flex',
                                alignItems: 'center',
                                justifyContent: 'center'
                            }}
                        >
                            <Eye size={27} color='var(--secondary-green)' />
                        </button>
                    );
                }
            }
        ],
        rowData: table,
    }
    return state

}



const BookingHoldingView: FC<BookingHoldingViewProps> = () => {
    const [state, setState] = useState<GridState>({
        columnDefs: [],
        rowData: []
    })
    const [isLoading, setIsLoading] = useState<boolean>(true);
    const [selectedTransaction, setSelectedTransaction] = useState<any | null>(null);
    const [showTransactionModal, setShowTransactionModal] = useState(false);

    const handleShowTransactions = (data: any) => {
        setSelectedTransaction(data);
        setShowTransactionModal(true);
    };

    const handleCloseTransactionModal = () => {
        setShowTransactionModal(false);
        setSelectedTransaction(null);
    };


    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const res = await getBookingTransactionList(adminAxiosInstance);
            console.log("Fetched transaction list:", res);
            if (res.success && res.data && res.data.BookingList) {
                setState(state_creator(res.data.BookingList || [], handleShowTransactions));
            } else {
                console.error('API Error:', res.message);
                setState(state_creator([], handleShowTransactions));
            }
        } catch (error) {
            console.error('Lỗi khi fetch booking holding list:', error);
            setState(state_creator([], handleShowTransactions));
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
                <h3 className="transaction__title text-[#282828] mb-5">Booking</h3>
                <CCol xs={12}>
                    {isLoading ? (
                        <div className="flex justify-center items-center h-100" >
                            <Loading />
                        </div>
                    ) : (
                        <div id="withdrawal-table" className="">
                            <AgGridReact
                                columnDefs={state?.columnDefs || []}
                                rowData={state?.rowData || []}
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
            {showTransactionModal && selectedTransaction && (
                <TransactionListModal
                    transactions={selectedTransaction.BookingTransactionList || []}
                    customerName={selectedTransaction.Title || 'Unknown'}
                    onClose={handleCloseTransactionModal}
                />
            )}
        </BookingHoldingViewContext.Provider>
    )
}

export default BookingHoldingView
