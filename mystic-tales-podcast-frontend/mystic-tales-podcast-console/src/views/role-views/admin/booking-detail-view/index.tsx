import React, { createContext, FC, useEffect, useMemo, useState } from 'react';
import { Typography, Card, CardContent, Chip, IconButton, Button } from '@mui/material';
import { Add, ArrowBack } from '@mui/icons-material';
import { CheckCircle, Eye, X } from 'phosphor-react';
import { formatDate } from '@/core/utils/date.util';
import './styles.scss';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import { confirmAlert } from '@/core/utils/alert.util';
import { toast } from 'react-toastify';
import { useNavigate, useParams } from 'react-router-dom';
import Loading from '@/views/components/common/loading';
import { useSagaPolling } from '@/hooks/useSagaPolling';
import { cancelRequest, getBookingDetail } from '@/core/services/booking/booking.service';
import { renderDescriptionHTML } from '@/core/utils/htmlRender.utils';
import AvatarInput from '@/views/components/common/avatar';
import { getPublicSource } from '@/core/services/file/file.service';
import { adminAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import Image from '@/views/components/common/image';
import RequirementFileDetailModal from './RequirementFileDetailModal';
import { set } from 'lodash';
import { CButton } from '@coreui/react';


ModuleRegistry.registerModules([AllCommunityModule])

interface BookingDetailViewProps { }
interface BookingDetailViewContextProps {
    handleDataChange: () => void;
    CurrentStatus: string;
}
export const BookingDetailViewContext = createContext<BookingDetailViewContextProps | null>(null);

const useProducingRequestColumnDefs = () => {
    const columnDefs: ColDef[] = useMemo(() => ([
        {
            headerName: "No.",
            valueGetter: (params: any) => params.node.rowIndex + 1,
            cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            flex: 0.5,
            filter: false
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
            }
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
            comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                const dateA = new Date(nodeA.data.CreatedAt).getTime();
                const dateB = new Date(nodeB.data.CreatedAt).getTime();
                return dateA - dateB;
            },
        },
        {
            headerName: "Accept",
            cellClass: 'd-flex align-items-center justify-content-center',
            cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            valueGetter: (params: { data: any }) => {
                if (params.data.IsAccepted) return 0;
                if (!params.data.IsAccepted === false) return 1;
                return 2;
            },
            cellRenderer: (params: any) => {
                let status = {
                    title: '',
                    color: '',
                    bg: ''
                };

                if (params.data.IsAccepted) {
                    status = {
                        title: 'Accepted',
                        color: 'var(--secondary-green)',
                        bg: 'rgba(173, 227, 57, 0.06)'

                    };
                } else if (params.data.IsAccepted === false) {
                    status = {
                        title: 'Rejected',
                        color: '#ef5350',
                        bg: 'rgba(251, 222, 227, 0.2)',

                    };
                } else {
                    status = {
                        title: 'Pending',
                        color: '#ffb300', bg: 'rgba(255, 179, 0, 0.07)'
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
        // {
        //     headerName: "",
        //     cellClass: 'd-flex justify-content-center py-0',
        //     cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
        //     cellRenderer: (params: { data: any }) => {
        //         const Modal_props = {
        //             updateForm: <ProducingRequestModal
        //                 bookingProducingRequestId={params.data?.Id}
        //                 onClose={() => { }}
        //             />,
        //             button: <Eye size={27} color='var(--white-75)' />,
        //         }
        //         return (
        //             <IconButton >
        //                 <Modal_Button
        //                     className="booking-detail__view-btn"
        //                     disabled={false}
        //                     content={Modal_props.button}
        //                 >
        //                     {Modal_props.updateForm}
        //                 </Modal_Button>
        //             </IconButton>

        //         )
        //     },
        //     flex: 0.5,
        //     filter: false,
        //     resizable: false,
        //     sortable: false,
        // }
    ]), []);
    return columnDefs;
}
const BookingDetailView: FC<BookingDetailViewProps> = () => {
    const Id = useParams().id || '';
    const [loading, setLoading] = useState(false)
    const [showPopup, setShowPopup] = useState<boolean>(false);
    const [Booking, setBooking] = useState<any | null>(null);
    const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
    const [depositRefundRate, setDepositRefundRate] = useState<number>(0);
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
                const bookingDetail = await getBookingDetail(adminAxiosInstance, Number(Id));
                if (!alive) return;
                console.log('Booking Detail:', bookingDetail);
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
            case 'Customer Cancel Request':
            case 'Podcast Buddy Cancel Request':
            case 'Quotation Rejected':
            case 'Cancelled Automatically':
            case 'Cancelled Manually':
            case 'Quotation Rejected':
                return { color: '#f2545b', bg: 'rgba(242, 84, 91, 0.15)' };
            default:
                return { color: '#9e9e9e', bg: 'rgba(158,158,158,0.15)' };
        }
    };
    const statusColors = getStatusColor(Booking.CurrentStatus?.Name || '');

    const handleClosePopup = () => {
        setShowPopup(false)
        setDepositRefundRate(0)
    }


    return (
        <BookingDetailViewContext.Provider value={{ handleDataChange: fetchBookingDetail, CurrentStatus: Booking.CurrentStatus?.Name || '' }}>
            <div className="booking-detail">
                <div className="booking-detail__header flex justify-between items-center">

                    <Typography variant="h4" className="booking-detail__title " sx={{ fontFamily: 'inter' }}>
                        {Booking.Title}
                    </Typography>



                </div>

                <div className="booking-detail__content">
                    <div className="booking-detail__info-layout">
                        <div className="booking-detail__info-left">
                            <div className="booking-detail__status-row">
                            </div>
                            <div className="booking-detail__plain-grid">
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Price</span>
                                    <span className="booking-detail__plain-value booking-detail__plain-value--price">{Booking.Price ? `${Booking.Price} Coins` : '---'} </span>
                                </div>
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Deadline</span>
                                    <span className="booking-detail__plain-value">{formatDate(Booking.Deadline)}</span>
                                </div>
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Updated</span>
                                    <span className="booking-detail__plain-value">{formatDate(Booking.UpdatedAt)}</span>
                                </div>
                                <div className="booking-detail__plain-item">
                                    <span className="booking-detail__plain-label">Created</span>
                                    <span className="booking-detail__plain-value">{formatDate(Booking.CreatedAt)}</span>
                                </div>
                                <div className="booking-detail__plain-item bg-transparent border-0 ">
                                    <span className="booking-detail__plain-label">Status</span>
                                    <Chip
                                        label={Booking.CurrentStatus.Name}
                                        className="booking-detail__status-chip"
                                        sx={{
                                            color: `${statusColors.color} !important`,
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
                    {(Booking?.BookingAutoCancelledReason || Booking?.BookingManualCancelledReason || Booking.CurrentStatus?.Name === 'Customer Cancel Request' || Booking.CurrentStatus?.Name === 'Podcast Buddy Cancel Request') && (
                        <div className="booking-detail__cancel-section">
                            {Booking?.BookingAutoCancelledReason && (
                                <div className="booking-detail__cancel-reason booking-detail__cancel-reason--auto">
                                    <strong>Auto Cancel Reason:</strong> {Booking.BookingAutoCancelledReason}
                                </div>
                            )}
                            {Booking?.BookingManualCancelledReason && (
                                <div className="booking-detail__cancel-reason booking-detail__cancel-reason--manual">
                                    <strong>Manual Cancel Reason:</strong> {Booking.BookingManualCancelledReason}
                                </div>
                            )}

                        </div>

                    )}
                    <div className="booking-detail__people-cards">
                        {/* Customer Card */}
                        <div className="booking-detail__person-card">
                            <div className="booking-detail__person-header">
                                <Image size={64} mainImageFileKey={Booking.Account.MainImageFileKey} />
                                <div className="booking-detail__person-heading">
                                    <div className="booking-detail__person-name">{Booking?.Account?.FullName || 'Customer'}</div>
                                    <div className="booking-detail__person-role">Customer</div>
                                </div>
                            </div>
                            <div className="booking-detail__person-meta">
                                {Booking?.Account?.Email && (
                                    <div className="booking-detail__person-meta-row">
                                        <span className="booking-detail__person-meta-label">Email</span>
                                        <span className="booking-detail__person-meta-value">{Booking.Account.Email}</span>
                                    </div>
                                )}
                            </div>
                        </div>

                        {/* Podcast Buddy Card */}
                        <div className="booking-detail__person-card">
                            <div className="booking-detail__person-header">
                                <AvatarInput size={64} fileKey={Booking?.PodcastBuddy?.MainImageFileKey || ''} />
                                <div className="booking-detail__person-heading">
                                    <div className="booking-detail__person-name">{Booking?.PodcastBuddy?.FullName || 'Podcast Buddy'}</div>
                                    <div className="booking-detail__person-role">Podcast Buddy</div>
                                </div>
                            </div>
                            <div className="booking-detail__person-meta">
                                {Booking?.PodcastBuddy?.Email && (
                                    <div className="booking-detail__person-meta-row">
                                        <span className="booking-detail__person-meta-label">Email</span>
                                        <span className="booking-detail__person-meta-value">{Booking.PodcastBuddy.Email}</span>
                                    </div>
                                )}
                            </div>
                        </div>
                    </div>

                    <div className="booking-detail__section mb-2">
                        <Typography variant="h5" className="booking-detail__section-title">
                            Requirements
                        </Typography>
                        <div className="booking-detail__requirements-grid mt-4">
                            {Booking.BookingRequirementFileList.map((req, index) => (
                                <Card key={req.Id} className="booking-detail__requirement-card">
                                    <CardContent className="booking-detail__card-content">
                                        <div className="booking-detail__requirement-header">
                                            <div className="booking-detail__requirement-number">
                                                {index + 1}
                                            </div>
                                            <Typography className="booking-detail__requirement-name ">
                                                {req.Name}
                                            </Typography>
                                            <Modal_Button
                                                className="booking-detail__view-btn"
                                                size='lg'
                                                content={<Eye />}
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
                                />
                            </div>
                        </>
                    )}


                </div>
            </div>


        </BookingDetailViewContext.Provider>

    );
};

export default BookingDetailView;


