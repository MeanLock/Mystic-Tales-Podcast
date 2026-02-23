import React, { useMemo, useState } from 'react';
import { CButtonGroup } from '@coreui/react';
import { Eye, FileText } from 'phosphor-react';
import TransactionListModal from './TransactionListModal';
import { AgGridReact } from 'ag-grid-react';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import './styles.scss';
import { formatDate } from '@/core/utils/date.util';

ModuleRegistry.registerModules([AllCommunityModule]);

interface SubscriptionRegistration {
    Id: string;
    Account: {
        Id: number;
        FullName: string;
        Email: string;
    };
    PodcastSubscriptionId: number;
    SubscriptionCycleType: {
        Id: number;
        Name: string;
    };
    CurrentVersion: number;
    IsAcceptNewestVersionSwitch: boolean;
    IsIncomeTaken: boolean;
    LastPaidAt: string;
    CancelledAt: string;
    CreatedAt: string;
    UpdatedAt: string;
    HoldingAmount: number;
    ProfitAmount: number;
    PodcastSubscriptionTransactionList: PodcastSubscriptionTransactionList[];
}
interface PodcastSubscriptionTransactionList {
    Id: string;
    Amount: number;
    Profit: number;
    PodcastSubscriptionId: number;
    TransactionType: {
        Id: number;
        Name: string;
    }
    TransactionStatus: {
        Id: number;
        Name: string;
    }
    CreatedAt: string;
    UpdatedAt: string;

}
interface SubscriptionHoldingModalProps {
    transaction: SubscriptionRegistration[];
    onClose?: () => void;
}

const SubscriptionHoldingModal: React.FC<SubscriptionHoldingModalProps> = ({ transaction, onClose }) => {
    const [selectedTransaction, setSelectedTransaction] = useState<SubscriptionRegistration | null>(null);
    const [showTransactionModal, setShowTransactionModal] = useState(false);

    const handleShowTransactions = (data: SubscriptionRegistration) => {
        setSelectedTransaction(data);
        setShowTransactionModal(true);
    };

    const handleCloseTransactionModal = () => {
        setShowTransactionModal(false);
        setSelectedTransaction(null);
    };

    const columnDefs: any = useMemo(() => [
    {
                headerName: "No.",
                flex: 0.4,
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1; // Hiển thị số thứ tự từ 1
                },
                cellClass: '',
                sortable: false,
                filter: false
            },
        {
            headerName: 'Customer',
            field: 'Account.Email',
            flex: 1.5,
        },
        {
            headerName: 'Cycle Type',
            field: 'SubscriptionCycleType.Name',
            flex: 0.8,
        },
        {
            headerName: 'Version',
            field: 'CurrentVersion',
            flex: 0.6,
            cellStyle: { textAlign: 'center' }
        },
        {
            headerName: 'Holding Amount',
            flex: 1,
            cellStyle: { fontWeight: 600 },
           valueGetter: (params: any) => {
                    return params.data.HoldingAmount ? params.data.HoldingAmount.toLocaleString() : '---';
                }
        },
        {
            headerName: 'Profit Amount',
            flex: 1,
            cellStyle: { fontWeight: 600 },
             valueGetter: (params: any) => {
                    return params.data.ProfitAmount ? params.data.ProfitAmount.toLocaleString() : '---';
                }
        },
        {
            headerName: 'Last Paid',
            flex: 1.2,
            valueGetter: (params: any) => formatDate(params.data.LastPaidAt),
             comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                const dateA = new Date(nodeA.data.LastPaidAt).getTime();
                const dateB = new Date(nodeB.data.LastPaidAt).getTime();
                return dateA - dateB;
            },
        },
        {
            headerName: 'Created At',
            flex: 1.2,
            valueGetter: (params: any) => formatDate(params.data.CreatedAt),
             comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                const dateA = new Date(nodeA.data.CreatedAt).getTime();
                const dateB = new Date(nodeB.data.CreatedAt).getTime();
                return dateA - dateB;
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
    ], []);

    const defaultColDef = useMemo(() => ({
        flex: 1,
        filter: true,
        resizable: true,
        wrapText: true,
        autoHeight: true,
        cellClass: 'd-flex align-items-center',
        editable: false,
    }), []);

    return (
        <div className="subscription-modal-overlay" onClick={onClose}>
            <div className="subscription-modal-content" onClick={(e) => e.stopPropagation()}>
                {transaction && transaction.length > 0 ? (
                    <div className="ag-theme-alpine" style={{ width: '100%' }}>
                        <AgGridReact
                            columnDefs={columnDefs}
                            rowData={transaction}
                            defaultColDef={defaultColDef}
                            rowHeight={60}
                            headerHeight={40}
                            pagination={true}
                            paginationPageSize={10}
                            paginationPageSizeSelector={[10, 20, 50]}
                            domLayout="autoHeight"
                        />
                    </div>
                ) : (
                    <div className="no-data">No subscription registrations found</div>
                )}
                
                {showTransactionModal && selectedTransaction && (
                    <TransactionListModal
                        transactions={selectedTransaction.PodcastSubscriptionTransactionList || []}
                        customerName={selectedTransaction.Account?.FullName || 'Unknown'}
                        onClose={handleCloseTransactionModal}
                    />
                )}
            </div>
        </div>
    );
};

export default SubscriptionHoldingModal;