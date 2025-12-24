import React from 'react';
import { CTable, CTableHead, CTableRow, CTableHeaderCell, CTableBody, CTableDataCell, CBadge } from '@coreui/react';
import './styles.scss';
import { formatDate } from '@/core/utils/date.util';

interface PodcastSubscriptionTransaction {
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

interface TransactionListModalProps {
    transactions: PodcastSubscriptionTransaction[];
    customerName: string;
    onClose?: () => void;
}

const TransactionListModal: React.FC<TransactionListModalProps> = ({ transactions, customerName, onClose }) => {
    const getStatusColor = (statusId: number) => {
        switch (statusId) {
            case 1: return 'warning'; // Pending
            case 2: return 'success'; // Completed
            case 3: return 'danger';  // Failed
            default: return 'danger';
        }
    };

    const getTypeColor = (typeId: number) => {
        switch (typeId) {
            default: return 'info';
        }
    };

    return (
        <div className="transaction-modal-overlay" onClick={onClose}>
            <div className="transaction-modal-wrapper" onClick={(e) => e.stopPropagation()}>
                <div className="transaction-modal-header">
                    <h5>Transaction History - {customerName}</h5>
                    <button className="transaction-modal-close" onClick={onClose}>
                        ×
                    </button>
                </div>
                <div className="transaction-list-modal">
                    <div className="mb-3 text-muted">
                        Total Transactions: <strong>{transactions?.length || 0}</strong>
                    </div>
                    {transactions && transactions.length > 0 ? (
                        <CTable bordered hover responsive className="table">
                            <CTableHead>
                                <CTableRow>
                                    <CTableHeaderCell>No</CTableHeaderCell>
                                    <CTableHeaderCell>Amount</CTableHeaderCell>
                                    <CTableHeaderCell>Profit</CTableHeaderCell>
                                    <CTableHeaderCell>Type</CTableHeaderCell>
                                    <CTableHeaderCell>Status</CTableHeaderCell>
                                    <CTableHeaderCell>Created At</CTableHeaderCell>
                                    <CTableHeaderCell>Updated At</CTableHeaderCell>
                                </CTableRow>
                            </CTableHead>
                            <CTableBody>
                                {transactions.map((transaction, index) => (
                                    <CTableRow key={transaction.Id}>
                                        <CTableDataCell className="text-muted small">{index + 1}</CTableDataCell>
                                   <CTableDataCell className="fw-semibold">{(transaction.Amount? transaction.Amount.toLocaleString() : '---') }</CTableDataCell>
                                                                          <CTableDataCell className="fw-semibold">{(transaction.Profit? transaction.Profit.toLocaleString() : '---')}</CTableDataCell>

                                        <CTableDataCell>
                                            <CBadge color={getTypeColor(transaction.TransactionType?.Id)}>
                                                {transaction.TransactionType?.Name || '---'}
                                            </CBadge>
                                        </CTableDataCell>
                                        <CTableDataCell>
                                            <CBadge color={getStatusColor(transaction.TransactionStatus?.Id)}>
                                                {transaction.TransactionStatus?.Name || '---'}
                                            </CBadge>
                                        </CTableDataCell>
                                        <CTableDataCell>{formatDate(transaction.CreatedAt)}</CTableDataCell>
                                        <CTableDataCell>{formatDate(transaction.UpdatedAt)}</CTableDataCell>
                                    </CTableRow>
                                ))}
                            </CTableBody>
                        </CTable>
                    ) : (
                        <div className="no-data">No transactions found</div>
                    )}
                </div>
            </div>
        </div>
    );
};

export default TransactionListModal;
