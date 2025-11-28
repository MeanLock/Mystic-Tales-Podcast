import { FC, useEffect, useMemo, useRef, useState } from "react"
import RevenueChart from "./revenue-chart"
import "./styles.scss"
import { Button, CircularProgress, IconButton, Typography } from "@mui/material"
import { EmptyComponent } from "@/views/components/common/empty"
import { Add } from "@mui/icons-material"
import VerifiedIcon from '@mui/icons-material/Verified';
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { Eye } from "phosphor-react"
import { formatDate } from "@/core/utils/date.util"
export const mockdata = {
    PodcastSubscriptionList: [
        {
            Id: 1,
            Name: "Basic Plan",
            Description: "Access to general episodes with ads.Access to general episodes with ads.Access to general episodes with ads.",
            IsActive: true,
            CurrentVersion: 1,
            DeletedAt: null,
            CreatedAt: "2025-10-18T09:25:11.730Z",
            UpdatedAt: "2025-10-18T09:25:11.730Z",
            PodcastSubscriptionCycleTypePriceList: [
                {
                    PodcastSubscriptionId: 1,
                    SubscriptionCycleType: {
                        Id: 1,
                        Name: "Monthly",
                    },
                    Version: 1,
                    Price: 300000,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    SubscriptionCycleType: {
                        Id: 2,
                        Name: "Annually",
                    },
                    Version: 1,
                    Price: 1200000,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
            ],
            PodcastSubscriptionBenefitMappingList: [
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 1,
                        Name: "Non-Quota Listening",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 2,
                        Name: "Subscriber-Only Shows",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 3,
                        Name: "Subscriber-Only Episodes",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 3,
                        Name: "Subscriber-Only Episodes",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 3,
                        Name: "Shows/Episodes Early Access",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
                {
                    PodcastSubscriptionId: 1,
                    PodcastSubscriptionBenefit: {
                        Id: 3,
                        Name: "Shows/Episodes Early Access",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
            ],
        },
        {
            Id: 2,
            Name: "Premium Plan",
            Description: "Ad-free listening and bonus episodes.",
            IsActive: false,
            CurrentVersion: 1,
            DeletedAt: null,
            CreatedAt: "2025-10-18T09:25:11.730Z",
            UpdatedAt: "2025-10-18T09:25:11.730Z",
            PodcastSubscriptionCycleTypePriceList: [
                {
                    PodcastSubscriptionId: 2,
                    SubscriptionCycleType: {
                        Id: 1,
                        Name: "Monthly",
                    },
                    Version: 1,
                    Price: 500000,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
            ],
            PodcastSubscriptionBenefitMappingList: [
                {
                    PodcastSubscriptionId: 2,
                    PodcastSubscriptionBenefit: {
                        Id: 3,
                        Name: "Subscriber-Only Episodes",
                    },
                    Version: 1,
                    CreatedAt: "2025-10-18T09:25:11.730Z",
                    UpdatedAt: "2025-10-18T09:25:11.730Z",
                },
            ],
        },

    ],
}

interface Subscription {
    Id: number
    Name: string
    Description: string
    IsActive: boolean
    PodcastSubscriptionCycleTypePriceList: Array<{
        SubscriptionCycleType: { Id: number; Name: string }
        Price: number
    }>
    PodcastSubscriptionBenefitMappingList: Array<{
        PodcastSubscriptionBenefit: { Name: string }
    }>
}
ModuleRegistry.registerModules([AllCommunityModule])

interface ShowSubscriptionProps { }
interface ShowSubscriptionContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}

const state_creator = (table: any[]) => {
    const state = {
        columnDefs: [

            {
                headerName: "Subscription",
                flex: 2,
                cellClass: 'd-flex align-items-center',
                cellStyle: { display: 'flex', alignItems: 'center' },
                cellRenderer: (params: any) => {
                    return (
                        <div style={{
                            display: 'flex',
                            flexDirection: 'column',
                            textAlign: 'left'
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
                                color: 'var(--white-75)',
                                lineHeight: '1.5',
                                overflow: 'hidden',
                                textOverflow: 'ellipsis',
                                display: '-webkit-box',
                                WebkitLineClamp: 2,
                                WebkitBoxOrient: 'vertical'
                            }}>
                                {params.data.Description}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Version", field: "CurrentVersion", cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.8rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
            },
            {
                headerName: "Monthly",
                cellClass: 'd-flex align-items-center',
                cellStyle: { display: 'flex', alignItems: 'center' },
                cellRenderer: (params: any) => {
                    const monthlyPlan = params.data.PodcastSubscriptionCycleTypePriceList?.find((p: any) => p.SubscriptionCycleType.Name === "Monthly");
                    if (!monthlyPlan) return <span style={{ color: 'var(--white-75)', fontSize: '0.7rem' }}>N/A</span>;

                    return (
                        <div style={{
                            display: 'flex',
                            flexDirection: 'column',
                            textAlign: 'center',
                            alignItems: 'center'
                        }}>
                            <div style={{
                                fontSize: '0.7rem',
                                color: 'var(--white-75)',
                                lineHeight: '1.2'
                            }}>
                                Price: {monthlyPlan.Price.toLocaleString("vi-VN")}
                            </div>
                            <div style={{
                                fontSize: '0.7rem',
                                color: 'var(--white-75)',
                                lineHeight: '1.2'
                            }}>
                                Version: {monthlyPlan.Version || 1}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Annually",
                cellClass: 'd-flex align-items-center   ',
                cellStyle: { display: 'flex', alignItems: 'center', },
                cellRenderer: (params: any) => {
                    const annuallyPlan = params.data.PodcastSubscriptionCycleTypePriceList?.find((p: any) => p.SubscriptionCycleType.Name === "Annually");
                    if (!annuallyPlan) return <span style={{ color: 'var(--white-75)', fontSize: '0.7rem' }}>--</span>;

                    return (
                        <div style={{
                            display: 'flex',
                            flexDirection: 'column',
                            textAlign: 'center',
                            alignItems: 'center'
                        }}>
                            <div style={{
                                fontSize: '0.7rem',
                                color: 'var(--white-75)',
                                lineHeight: '1.2'
                            }}>
                                 Price: {annuallyPlan.Price.toLocaleString("vi-VN")} VND
                            </div>
                            <div style={{
                                fontSize: '0.7rem',
                                color: 'var(--white-75)',
                                lineHeight: '1.2'
                            }}>
                                Version: {annuallyPlan.Version || 1}
                            </div>
                        </div>
                    );
                }
            },
            {
                headerName: "Benefit Count",
                cellClass: 'd-flex align-items-center ',
                cellStyle: { display: 'flex', alignItems: 'center' },
                cellRenderer: (params: any) => {
                    const benefitCount = params.data.PodcastSubscriptionBenefitMappingList?.length || 0;
                    return (
                        <div style={{
                            fontSize: '0.8rem'
                        }}>
                            {benefitCount}
                        </div>
                    );
                }
            },
            {
                headerName: "Updated At",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize:'0.8rem' },

                valueGetter: (params: { data: any }) => formatDate(params.data.UpdatedAt),
            },
            {
                headerName: "Status",
                cellClass: 'd-flex align-items-center justify-content-center',
                cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
                cellRenderer: (params: any) => {
                    let status = {
                        title: '',
                        color: '',
                        bg: ''
                    };

                    if (params.data.IsActive) {
                        status = {
                            title: 'Actived',
                            color: 'var(--primary-green)', bg: 'rgba(174, 227, 57, 0.2)'

                        };
                    } else {
                        status = {
                            title: 'Inactived',
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
const ShowSubscription: FC<ShowSubscriptionProps> = () => {
    const [activeSubscriptions, setActiveSubscriptions] = useState<Subscription[]>([])
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);


    useEffect(() => {
        const active = mockdata.PodcastSubscriptionList.filter((sub) => sub.IsActive)
        setActiveSubscriptions(active)
        setIsLoading(false);
        setState(state_creator(mockdata.PodcastSubscriptionList));

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
    if (activeSubscriptions.length === 0) {
        return (
            <div className="pt-30">
                <EmptyComponent item="Subscription" subtitle="Try adjusting your search terms or filters" />
                <Button
                    variant="contained"
                    startIcon={<Add />}
                    sx={{
                        backgroundColor: 'var(--primary-green)',
                        color: '#000',
                        fontWeight: 'bold',
                        borderRadius: '8px',
                        textTransform: 'none',
                        '&:hover': { backgroundColor: '#8bc34a' }
                    }}
                >
                    Add Subscription
                </Button>
            </div>
        )
    }

    const activeSubscription = activeSubscriptions[0]

    const monthlyPrice =
        activeSubscription.PodcastSubscriptionCycleTypePriceList.find((p) => p.SubscriptionCycleType.Name === "Monthly")
            ?.Price || 0
    const annuallyPrice =
        activeSubscription.PodcastSubscriptionCycleTypePriceList.find((p) => p.SubscriptionCycleType.Name === "Annually")
            ?.Price || 0

    // Calculate monthly equivalent for annual plan
    const monthlyEquivalent = Math.round(annuallyPrice / 12)
    const savings = monthlyPrice * 12 - annuallyPrice



    return (
        <div>
            <div className="show-subscription">
                <Typography variant="h4" className="show-subscription__title" >
                    Show Subscriptions
                </Typography>
                <div className="show-subscription__container">
                    {/* Left Section - Subscription Details */}
                    <div className="show-subscription__left">
                        <div className="subscription-card">
                            <div className="subscription-card__header">
                                <h2 className="subscription-card__name">{activeSubscription.Name}</h2>
                                <span className="subscription-card__badge">Active</span>
                            </div>

                            <div className="subscription-card__description">{activeSubscription.Description}</div>

                            <div className="subscription-card__pricing-section">
                                <h3 className="subscription-card__pricing-title">Pricing Options</h3>
                                <div className="subscription-card__pricing-options">
                                    {/* Monthly Option */}
                                    <div className="pricing-option">
                                        <div className="pricing-option__header">
                                            <span className="pricing-option__cycle">Monthly</span>
                                        </div>
                                        <div className="pricing-option__price">
                                            <span className="pricing-option__amount">{monthlyPrice.toLocaleString("vi-VN")}</span>
                                            <span className="pricing-option__currency">VND</span>
                                        </div>
                                        <div className="pricing-option__period">per month</div>
                                    </div>

                                    {/* Annual Option */}
                                    <div className="pricing-option pricing-option--featured">
                                        <div className="pricing-option__header">
                                            <span className="pricing-option__cycle">Annually</span>
                                            {savings > 0 && (
                                                <span className="pricing-option__badge">
                                                    Save {Math.round((savings / (monthlyPrice * 12)) * 100)}%
                                                </span>
                                            )}
                                        </div>
                                        <div className="pricing-option__price">
                                            <span className="pricing-option__amount">{annuallyPrice.toLocaleString("vi-VN")}</span>
                                            <span className="pricing-option__currency">VND</span>
                                        </div>
                                        <div className="pricing-option__period">{monthlyEquivalent.toLocaleString("vi-VN")} VND/month</div>
                                    </div>
                                </div>
                            </div>

                        </div>

                        <div className="subscription-card__benefits">
                            <h3 className="subscription-card__benefits-title">Benefits</h3>
                            <ul className="subscription-card__benefits-list">
                                {activeSubscription.PodcastSubscriptionBenefitMappingList.map((benefit, idx) => (
                                    <li key={idx} className="subscription-card__benefit-item">
                                        <span className="subscription-card__benefit-icon">
                                            <VerifiedIcon fontSize="small" />
                                        </span>
                                        {benefit.PodcastSubscriptionBenefit.Name}
                                    </li>
                                ))}
                            </ul>
                        </div>
                    </div>

                    {/* Right Section - Revenue Chart */}
                    <div className="show-subscription__right">
                        <RevenueChart />
                    </div>
                </div>

            </div>
            <div
                id="subscription-table"
                style={{

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
                        <CircularProgress />
                    </div>
                ) : (
                    <AgGridReact
                        columnDefs={state?.columnDefs}
                        rowData={state?.rowData}
                        defaultColDef={defaultColDef}
                        rowHeight={80}
                        headerHeight={50}
                        pagination={true}
                        paginationPageSize={10}
                        paginationPageSizeSelector={[10, 16, 24, 32]}
                        domLayout="autoHeight"
                    />
                )}
            </div>
        </div>
    )
}

export default ShowSubscription
