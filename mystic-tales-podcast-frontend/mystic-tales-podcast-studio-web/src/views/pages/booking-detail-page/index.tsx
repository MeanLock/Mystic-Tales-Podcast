import React, { createContext, FC, useEffect, useMemo, useState } from 'react';
import { Typography, Card, CardContent, Chip, IconButton, Button } from '@mui/material';
import { Add, ArrowBack } from '@mui/icons-material';
import { Eye, X } from 'phosphor-react';
import { formatDate } from '@/core/utils/date.util';
import './styles.scss';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import SubmitAudioModal from './SubmitAudioModal';
import RequirementFileDetailModal from './RequirementFileDetailModal';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import ProducingRequestModal from './ProducingRequestModal';
import DealingModal from './DealingModal';
import { confirmAlert } from '@/core/utils/alert.util';
import { toast } from 'react-toastify';
import { useNavigate, useParams } from 'react-router-dom';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2/login-required-axios-instance';
import { cancelProducing, cancelQuotation, getBookingDetail } from '@/core/services/booking/booking.service';
import Loading from '@/views/components/common/loading';
import { renderDescriptionHTML } from '@/core/utils/htmlRender.utils';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';

ModuleRegistry.registerModules([AllCommunityModule])

interface BookingDetailPageProps { }
interface BookingDetailPageContextProps {
    handleDataChange: () => void;
    CurrentStatus: string;
}
export const BookingDetailPageContext = createContext<BookingDetailPageContextProps | null>(null);

// Memoized grid column definitions
const useProducingRequestColumnDefs = () => {
    const columnDefs: ColDef[] = useMemo(() => ([
        {
            headerName: "No.",
            valueGetter: (params: any) => params.node.rowIndex + 1,
            cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            flex: 0.5,
            filter: false,
            tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`

        },
        {
            headerName: "Note",
            field: "Note",
            cellStyle: {
                display: 'flex',
                alignItems: 'center',
                fontSize: '0.8rem',
                overflow: 'hidden',
                textOverflow: 'ellipsis',
                whiteSpace: 'nowrap'
            },
            cellRenderer: (params: any) => {
                const value = params.value || '';
                return value.length > 40 ? value.slice(0, 40) + '...' : value;
            },
            tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`

        },
        {
            headerName: "Deadline",
            cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.8rem' },
            valueGetter: (params: { data: any }) => formatDate(params.data.Deadline),
        },
        {
            headerName: "Finished At",
            cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.8rem' },
            valueGetter: (params: { data: any }) => formatDate(params.data.FinishedAt),
        },
        {
            headerName: "Created At",
            cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.8rem' },
            valueGetter: (params: { data: any }) => formatDate(params.data.CreatedAt),
        },
        {
            headerName: "Accept",
            cellClass: 'd-flex align-items-center justify-content-center',
            cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            cellRenderer: (params: any) => {
                let status = {
                    title: '',
                    color: '',
                    bg: ''
                };

                if (params.data.IsAccepted) {
                    status = {
                        title: 'Accepted',
                        color: 'var(--primary-green)', bg: 'rgba(174, 227, 57, 0.2)'

                    };
                } else if (params.data.IsAccepted === false) {
                    status = {
                        title: 'Rejected',
                        color: '#ef5350', bg: 'rgba(227, 57, 57, 0.2)'

                    };
                } else {
                    status = {
                        title: 'Pending',
                        color: '#f3da35ff', bg: 'rgba(255, 179, 0, 0.15)'
                    };
                }
                return (
                    <span

                        style={{
                            display: 'inline-block',
                            minWidth: 100,
                            padding: '0 10px',
                            borderRadius: 50,
                            fontWeight: 700,
                            color: status.color,
                            fontSize: '0.75rem',
                            background: status.bg,
                            textAlign: 'center',
                            border: `1.5px solid ${status.color}`,
                        }}
                    >
                        {status.title}
                    </span>
                );
            },
        },
        {
            headerName: "",
            cellClass: 'd-flex justify-content-center py-0',
            cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            cellRenderer: (params: { data: any }) => {
                const Modal_props = {
                    updateForm: <ProducingRequestModal
                        bookingProducingRequestId={params.data?.Id}
                        onClose={() => { }}
                    />,
                    button: <Eye size={27} color='var(--white-75)' />,
                }
                return (
                    <IconButton >
                        <Modal_Button
                            className="booking-detail__view-btn"
                            disabled={false}
                            content={Modal_props.button}
                            size="md"
                        >
                            {Modal_props.updateForm}
                        </Modal_Button>
                    </IconButton>

                )
            },
            flex: 0.5,
            filter: false,
            resizable: false,
            sortable: false,
        }
    ]), []);
    return columnDefs;
}
const BookingDetailPage: FC<BookingDetailPageProps> = () => {
    const Id = useParams().id || '';
    const [loading, setLoading] = useState(false)
    const [cancelReason, setCancelReason] = useState<string>('');
    const [Booking, setBooking] = useState<any | null>(null);
    const navigate = useNavigate();
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 60,
        intervalSeconds: 2,
    })
    const fetchBookingDetail = async () => {
        let alive = true;
        (async () => {
            setLoading(true);
            try {
                const bookingDetail = await getBookingDetail(loginRequiredAxiosInstance, Number(Id));
                if (!alive) return;
                if (bookingDetail.success) {
                    setBooking(bookingDetail.data.Booking);
                } else {
                    console.error('API Error:', bookingDetail.message);
                }
            } catch (error) {
                if (alive) console.error('Error fetching booking detail:', error);
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    };
    useEffect(() => {
        fetchBookingDetail();
    }, [Id]);

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
    const producingRequestColumnDefs = useProducingRequestColumnDefs();

    if (!Booking || loading) {
        return (
            <div className="flex justify-center items-center h-100 ">
                <Loading />
            </div>
        );
    }
    const isProducing = Booking.CurrentStatus?.Name === "Producing";
    const isQuotation = Booking.CurrentStatus?.Name === "Quotation Request" || Booking.CurrentStatus?.Name === "Quotation Dealing";
    const isQuotationDealing = Booking.CurrentStatus?.Name === "Quotation Dealing";
    const isQuotationRequest = Booking.CurrentStatus?.Name === "Quotation Request";
    const getStatusColor = (status: string) => {
        switch (status) {
            case 'Producing':
            case 'Track Previewing':
            case 'Producing Requested':
                return { color: '#61a7f2ff', bg: 'rgba(41, 182, 246, 0.15)' };
            case 'Completed':
                return { color: '#AEE339', bg: 'rgba(174, 227, 57, 0.2)' };
            case 'Quotation Request':
            case 'Quotation Dealing':
                return { color: '#ffb300', bg: 'rgba(255, 179, 0, 0.15)' };
            case 'Quotation Cancelled':
                case 'Cancelled Manually':
                case 'Podcast Buddy Cancel Request':
            case 'Quotation Rejected':
                return { color: '#f2545b', bg: 'rgba(242, 84, 91, 0.15)' };
            default:
                return { color: '#9e9e9e', bg: 'rgba(158,158,158,0.15)' };
        }
    };
    const statusColors = getStatusColor(Booking.CurrentStatus?.Name || '');

    const handleCancel = async (id: number) => {
        const alert = await confirmAlert("Are you sure to reject this quotation?");
        if (!alert.isConfirmed) return;
        try {
            const response = await cancelQuotation(loginRequiredAxiosInstance, id);
            if (response && response.success) {
                toast.success(`Canceled booking successfully`);
                await new Promise((r) => setTimeout(r, 100));
                navigate(0);
            }
        } catch {
            toast.error('Error canceling booking')
        }

    };
    const handleCancelManual = async () => {
        if (!cancelReason.trim()) {
            toast.error("Please enter cancel reason")
            return
        }
        try {
            setLoading(true);
            const res = await cancelProducing(loginRequiredAxiosInstance, Number(Id), cancelReason);
            const sagaId = res?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Cancel failed, please try again.")
                return
            }
            await startPolling(sagaId, loginRequiredAxiosInstance, {
                onSuccess: () => {
                    toast.success(`Cancel Successfully, please wait Staff to review`);
                    //navigate(0);
                },
                onFailure: (err) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (error) {
            toast.error("Error processing request");
        } finally {
            setLoading(false);
        }
    };
    return (
        <BookingDetailPageContext.Provider value={{ handleDataChange: fetchBookingDetail, CurrentStatus: Booking.CurrentStatus?.Name || '' }}>
            <div className="flex justify-start align-center" style={{ padding: '10px 0 20px 40px' }}>
                <IconButton className="booking-detail__back-button" onClick={() => window.history.back()}>
                    <ArrowBack sx={{ fontSize: '0.8rem', marginRight: '6px' }} /> Back
                </IconButton>
            </div>
            <div className="booking-detail">
                <div className="booking-detail__header">

                    <Typography variant="h4" className="booking-detail__title">
                        {Booking.Title}
                    </Typography>
                      {Booking.BookingManualCancelledReason && (
                <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                    <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-xs text-red-700 font-medium">
                        <strong>Cancel Reason:</strong> {Booking.BookingManualCancelledReason}
                    </span>
                </div>
            )}
                          {Booking.BookingAutoCancelledReason && (
                <div className="flex items-center gap-2 bg-red-100 border border-red-400  rounded px-3 py-2 mb-3 " style={{ width: "fit-content" }}>
                    <svg className="w-5 h-5 text-red-500 shrink-0" fill="none" stroke="currentColor" strokeWidth={2} viewBox="0 0 24 24">
                        <path strokeLinecap="round" strokeLinejoin="round" d="M12 9v2m0 4h.01M21 12c0 4.97-4.03 9-9 9s-9-4.03-9-9 4.03-9 9-9 9 4.03 9 9z" />
                    </svg>
                    <span className="text-xs text-red-700 font-medium">
                        <strong>Cancel Reason:</strong> {Booking.BookingAutoCancelledReason}
                    </span>
                </div>
            )}
               
                    {isProducing && (
                        <div className="flex gap-6">
                            <Modal_Button
                                className="booking-detail__reject-btn warning-button"
                                content="Cancel"
                                variant="outlined"
                                size='sm'
                                startIcon={<X />}
                            >
                                <div className="booking-detail__cancel-modal">
                                    <label className="booking-detail__label">
                                        Cancel Reason <span className="text-red-500">*</span>
                                    </label>
                                    <input
                                        type="text"
                                        className="booking-detail__input"
                                        placeholder="Enter cancel reason"
                                        value={cancelReason}
                                        onChange={(e) => setCancelReason(e.target.value)}
                                        onKeyPress={(e) => {
                                            if (e.key === 'Enter') {
                                                handleCancelManual()
                                            }
                                        }}
                                    />
                                    <button
                                        type="button"
                                        className="booking-detail__btn booking-detail__btn--resolve "
                                        onClick={handleCancelManual}
                                        disabled={loading}
                                    >
                                        {loading ? "Cancelling..." : "Confirm"}
                                    </button>
                                </div>


                            </Modal_Button>
                            <Modal_Button
                                className="booking-detail__submit-btn"
                                content="Submit Audio"
                                variant="contained"
                                size='md'
                                startIcon={<Add />}
                            >
                                <SubmitAudioModal booking={Booking} onClose={() => { }} />
                            </Modal_Button>
                        </div>
                    )}
                    {/* {isQuotationDealing && (
                        <div className="flex gap-6">

                            <Button
                                className="booking-detail__reject-btn warning-button"
                                variant="outlined"
                                onClick={() => handleCancel(Booking.Id)}
                            >
                                <X className='mr-3' /> Reject
                            </Button>
                        </div>
                    )} */}
                    {isQuotationRequest && (
                        <div className="flex gap-6">

                            <Button
                                className="booking-detail__reject-btn warning-button"
                                variant="outlined"
                                onClick={() => handleCancel(Booking.Id)}
                            >
                                <X className='mr-3' /> Reject
                            </Button>

                            <Modal_Button
                                className="booking-detail__submit-btn"
                                content="Deal Quotation"
                                variant="contained"
                                title="Deal Quotation"
                                size='md'
                                startIcon={<Add />}
                            >
                                <DealingModal booking={Booking} onClose={() => { }} />
                            </Modal_Button>
                        </div>
                    )}
                </div>
                <div className="booking-detail__content">
                    <div className="booking-detail__info-layout">
                        <div className="booking-detail__info-left">
                            <div className="booking-detail__status-row">
                            </div>
                            <div className="booking-detail__plain-grid">

                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Customer</span>
                                    <span className="booking-detail__plain-value booking-detail__plain-value--avatar">
                                        {Booking.Account.FullName}
                                    </span>
                                </div>
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Price</span>
                                    <span className="booking-detail__plain-value booking-detail__plain-value--price">{Booking.Price ? `${Booking.Price.toLocaleString('vi-VN')} Coins` : '---'} </span>
                                </div>
                                {(isProducing) && (
                                    <div className="booking-detail__plain-item">
                                        <span className="booking-detail__plain-label">Deadline</span>
                                        <span className="booking-detail__plain-value">{formatDate(Booking.Deadline)}</span>
                                    </div>
                                )}
                                {(isQuotation || Booking.CurrentStatus.Name === "Quotation Rejected") && (
                                    <div className="booking-detail__plain-item">
                                        <span className="booking-detail__plain-label">Deadline</span>
                                        <span className="booking-detail__plain-value">{Booking.DeadlineDays} days</span>
                                    </div>
                                )}
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Status</span>
                                    <Chip
                                        label={Booking.CurrentStatus.Name}
                                        className="booking-detail__status-chip"
                                        sx={{
                                            color: statusColors.color,
                                            backgroundColor: statusColors.bg,
                                            border: `1.5px solid ${statusColors.color}`,
                                            fontWeight: 700
                                        }}
                                    />
                                </div>
                            </div>
                        </div>
                        <div className="booking-detail__description-right">
                            <h5 className="booking-detail__description-title">Description</h5>
                            <div
                                className="booking-detail__description-body"
                                dangerouslySetInnerHTML={{
                                    __html: renderDescriptionHTML(Booking?.Description || ""),
                                }}
                            />
                        </div>
                    </div>

                    {/* Requirements Section */}
                    <div className="booking-detail__section mb-12">
                        <Typography variant="h5" className="booking-detail__section-title">
                            Requirements
                        </Typography>
                        <div className="booking-detail__requirements-grid">
                            {Booking.BookingRequirementFileList.map((req, index) => (
                                <Card key={req.Id} className="booking-detail__requirement-card">
                                    <CardContent className="booking-detail__card-content">
                                        <div className="booking-detail__requirement-header">
                                            <div className="booking-detail__requirement-number">
                                                {index + 1}
                                            </div>
                                            <Typography className="booking-detail__requirement-name">
                                                {req.Name}
                                            </Typography>
                                            <Modal_Button
                                                className="booking-detail__view-btn"
                                                size='md'
                                                startIcon={<Eye />}
                                            >
                                                <RequirementFileDetailModal bookingRequirementFile={req} />
                                            </Modal_Button>
                                        </div>
                                    </CardContent>
                                </Card>
                            ))}
                        </div>
                    </div>


                    {Booking.BookingProducingRequestList?.length > 0 && (
                        <>
                            <Typography variant="h5" className="booking-detail__section-title ">
                                Producing Requests
                            </Typography>
                            <div
                                id="request-table"
                                style={{

                                }}
                            >
                                <AgGridReact
                                    columnDefs={producingRequestColumnDefs}
                                    rowData={Booking.BookingProducingRequestList}
                                    defaultColDef={defaultColDef}
                                    rowHeight={80}
                                    headerHeight={50}
                                    pagination={true}
                                    paginationPageSize={10}
                                    paginationPageSizeSelector={[10, 16, 24, 32]}
                                    domLayout="autoHeight"
                                    tooltipShowDelay={0}
                                />
                            </div>
                        </>
                    )}


                </div>
            </div>
        </BookingDetailPageContext.Provider>

    );
};

export default BookingDetailPage;


