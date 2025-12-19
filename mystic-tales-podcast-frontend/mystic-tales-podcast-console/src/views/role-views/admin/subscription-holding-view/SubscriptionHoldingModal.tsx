import React from 'react';
import { CTable, CTableHead, CTableRow, CTableHeaderCell, CTableBody, CTableDataCell } from '@coreui/react';
import './styles.scss';

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
}

interface SubscriptionHoldingModalProps {
    transaction: SubscriptionRegistration[];
    onClose?: () => void;
}

const SubscriptionHoldingModal: React.FC<SubscriptionHoldingModalProps> = ({ transaction }) => {
    const formatDate = (dateString: string) => {
        if (!dateString) return '---';
        return new Date(dateString).toLocaleString('vi-VN');
    };

    const formatCurrency = (amount: number) => {
        return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
    };

    return (
        <div className="subscription-holding-modal">
            <h5>Subscription Registration Details</h5>
            {transaction && transaction.length > 0 ? (
                <CTable bordered hover responsive className="table">
                    <CTableHead>
                        <CTableRow>
                            <CTableHeaderCell>Customer</CTableHeaderCell>
                            <CTableHeaderCell>Email</CTableHeaderCell>
                            <CTableHeaderCell>Cycle Type</CTableHeaderCell>
                            <CTableHeaderCell>Version</CTableHeaderCell>
                            <CTableHeaderCell>Holding Amount</CTableHeaderCell>
                            <CTableHeaderCell>Last Paid</CTableHeaderCell>
                            <CTableHeaderCell>Created At</CTableHeaderCell>
                        </CTableRow>
                    </CTableHead>
                    <CTableBody>
                        {transaction.map((item) => (
                            <CTableRow key={item.Id}>
                                <CTableDataCell className="fw-semibold">{item.Account?.FullName || '---'}</CTableDataCell>
                                <CTableDataCell>{item.Account?.Email || '---'}</CTableDataCell>
                                <CTableDataCell>{item.SubscriptionCycleType?.Name || '---'}</CTableDataCell>
                                <CTableDataCell className="text-center">{item.CurrentVersion}</CTableDataCell>
                                <CTableDataCell className="fw-semibold">{formatCurrency(item.HoldingAmount)}</CTableDataCell>
                            
                                <CTableDataCell>{formatDate(item.LastPaidAt)}</CTableDataCell>
                                <CTableDataCell>{formatDate(item.CreatedAt)}</CTableDataCell>
                            </CTableRow>
                        ))}
                    </CTableBody>
                </CTable>
            ) : (
                <div className="no-data">No subscription registrations found</div>
            )}
        </div>
    );
};

export default SubscriptionHoldingModal;