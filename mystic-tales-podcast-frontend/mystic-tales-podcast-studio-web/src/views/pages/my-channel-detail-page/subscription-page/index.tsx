import { createContext, FC, useEffect, useMemo, useRef, useState } from "react"
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
import Modal_Button from "@/views/components/common/modal/ModalButton"
import SubscriptionModal from "./SubscriptionModal"
import { getChannelDetail } from "@/core/services/channel/channel.service"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import { useParams } from "react-router-dom"
import { useSagaPolling } from "@/core/hooks/useSagaPolling"
import Loading from "@/views/components/common/loading"
import { getTruncatedDescription, renderDescriptionHTML } from "@/core/utils/htmlRender.utils"
import { get } from "lodash"


interface Subscription {
    Id: number
    Name: string
    Description: string
    IsActive: boolean
    CurrentVersion: number
    PodcastSubscriptionCycleTypePriceList: Array<{
        SubscriptionCycleType: { Id: number; Name: string }
        Price: number
        Version: number
    }>
    PodcastSubscriptionBenefitMappingList: Array<{
        PodcastSubscriptionBenefit: { Id: number; Name: string }
        Version: number
    }>
    PodcastSubscriptionRegistrationList: Array<{
        Id: string
        AccountId: number
        PodcastSubscriptionId: number
        SubscriptionCycleType: { Id: number; Name: string }
        CurrentVersion: number
        IsAcceptNewestVersionSwitch: boolean
        IsIncomeTaken: boolean
        LastPaidAt: string | null
        CancelledAt: string | null
        CreatedAt: string
        UpdatedAt: string
    }>
}
ModuleRegistry.registerModules([AllCommunityModule])

interface ChannelSubscriptionProps { }
interface ChannelSubscriptionContextProps {
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
                tooltipValueGetter: (params: any) => `Id: ${params.data?.Id ?? ''}`,
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
                            }}
                            
                                 dangerouslySetInnerHTML={{
                                        __html: getTruncatedDescription(params.data.Description || "", 100),
                                    }}
                            >
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
                    const monthlyPlans = params.data.PodcastSubscriptionCycleTypePriceList?.filter((p: any) => p.SubscriptionCycleType.Name === "Monthly") || [];
                    if (monthlyPlans.length === 0) return <span style={{ color: 'var(--white-75)', fontSize: '0.7rem' }}>N/A</span>;
                    const maxVersion = Math.max(...monthlyPlans.map((p: any) => p.Version));
                    const monthlyPlan = monthlyPlans.find((p: any) => p.Version === maxVersion);
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
                    const annuallyPlans = params.data.PodcastSubscriptionCycleTypePriceList?.filter((p: any) => p.SubscriptionCycleType.Id === 2) || [];
                    if (annuallyPlans.length === 0) return <span style={{ color: 'var(--white-75)', fontSize: '0.7rem' }}>--</span>;
                    const maxVersion = Math.max(...annuallyPlans.map((p: any) => p.Version));
                    const annuallyPlan = annuallyPlans.find((p: any) => p.Version === maxVersion);
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
                    const benefits = params.data.PodcastSubscriptionBenefitMappingList || [];
                    if (benefits.length === 0) return <div style={{ fontSize: '0.8rem' }}>0</div>;
                    const maxVersion = Math.max(...benefits.map((b: any) => b.Version));
                    const currentBenefits = benefits.filter((b: any) => b.Version === maxVersion);
                    return (
                        <div style={{
                            fontSize: '0.8rem'
                        }}>
                            {currentBenefits.length}
                        </div>
                    );
                }
            },
            {
                headerName: "Updated At",
                cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.8rem' },

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
                    const Modal_props = {
                        updateForm: <SubscriptionModal
                            subscription={params.data}
                            onClose={() => { }}

                        />,
                        button: <Eye size={27} color='var(--white-75)' />,
                    }
                    return (
                        <IconButton >
                            <Modal_Button
                                className="bg-none"
                                disabled={false}
                                content={Modal_props.button}
                                size="lg"
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

        ],
        rowData: table

    }
    return state
}
export const ChannelSubscriptionContext = createContext<ChannelSubscriptionContextProps | null>(null)
const ChannelSubscription: FC<ChannelSubscriptionProps> = () => {
    const { id } = useParams<{ id: string }>();
    const [subscriptions, setSubscriptions] = useState<Subscription[]>([])
    const [activeSubscription, setActiveSubscription] = useState<Subscription | null>(null)
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const fetchSubscriptionList = async () => {
        setIsLoading(true);
        try {
            const res = await getChannelDetail(loginRequiredAxiosInstance, id);
            console.log("Fetched subscription list:", res.data.Channel.PodcastSubscriptionList);
            if (res.success && res.data) {
                const ch = res.data.Channel;
                setSubscriptions(ch.PodcastSubscriptionList);
                setState(state_creator(ch.PodcastSubscriptionList));
                const active = ch.PodcastSubscriptionList.find((sub) => sub.IsActive)
                setActiveSubscription(active)
            } else {
                console.error('API Error:', res.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch channel list:', error);
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        fetchSubscriptionList();
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

    if (isLoading) {
        return (
            <div style={{ display: "flex", alignItems: "center", justifyContent: "center", height: "400px" }}>
                <Loading />
            </div>
        );
    }
    if (!isLoading && subscriptions.length === 0) {
        return (
            <ChannelSubscriptionContext.Provider value={{ handleDataChange: fetchSubscriptionList }}>

                <div className="pt-30">
                    <EmptyComponent item="Subscription" subtitle="Try adjusting your search terms or filters" />
                    <Modal_Button
                        className=" channel-subscription__btn h-1/2 text-black font-bold rounded-lg normal-case "
                        content="Add Subscription"
                        variant="contained"
                        size="md"
                        startIcon={<Add />}
                    >
                        <SubscriptionModal
                            podcastChannelId={id}
                            onClose={() => { }}
                        />
                    </Modal_Button>

                </div>
            </ChannelSubscriptionContext.Provider>

        )
    }



    const getMaxVersionPrice = (cycleTypeName: string) => {
        const prices = activeSubscription?.PodcastSubscriptionCycleTypePriceList.filter(
            (p) => p.SubscriptionCycleType.Name === cycleTypeName
        ) || [];
        if (prices.length === 0) return 0;
        const maxVersion = Math.max(...prices.map(p => p.Version));
        return prices.find(p => p.Version === maxVersion)?.Price || 0;
    };

    const monthlyPrice = getMaxVersionPrice("Monthly");
    const annuallyPrice = getMaxVersionPrice("Annually");

    const monthlyEquivalent = Math.round(annuallyPrice / 12)
    const savings = monthlyPrice * 12 - annuallyPrice



    return (
        <ChannelSubscriptionContext.Provider value={{ handleDataChange: fetchSubscriptionList }}>
            <div className="channel-subscription">
                <div className="flex justify-between  ">
                    <Typography variant="h4" className="channel-subscription__title " >
                        Channel Subscriptions
                    </Typography>
                    <Modal_Button
                        className=" channel-subscription__btn h-1/2 text-black font-bold rounded-lg normal-case  "
                        content="Add Subscription"
                        variant="contained"
                        size="md"
                        startIcon={<Add />}
                    >
                        <SubscriptionModal
                            podcastChannelId={id}
                            onClose={() => { }}

                        />
                    </Modal_Button>


                </div>

                <div className="channel-subscription__container">
                    {activeSubscription ? (
                        <div className="channel-subscription__left">
                            <div className="subscription-card">
                                <div className="subscription-card__header">
                                    <h2 className="subscription-card__name">{activeSubscription.Name}</h2>
                                    <span className="subscription-card__badge">Active</span>
                                </div>

                                <div className="subscription-card__description"
                                    dangerouslySetInnerHTML={{
                                        __html: renderDescriptionHTML(activeSubscription?.Description || ""),
                                    }}
                                ></div>

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
                                    {(() => {
                                        const benefits = activeSubscription.PodcastSubscriptionBenefitMappingList || [];
                                        if (benefits.length === 0) return null;
                                        const maxVersion = Math.max(...benefits.map(b => b.Version));
                                        const currentBenefits = benefits.filter(b => b.Version === maxVersion);
                                        return currentBenefits.map((benefit, idx) => (
                                            <li key={idx} className="subscription-card__benefit-item">
                                                <span className="subscription-card__benefit-icon">
                                                    <VerifiedIcon fontSize="small" />
                                                </span>
                                                {benefit.PodcastSubscriptionBenefit.Name}
                                            </li>
                                        ));
                                    })()}
                                </ul>
                            </div>
                        </div>
                    ) : (
                        <div className="channel-subscription__left">
                            <EmptyComponent subtitle="No Active Subscription Is Available" />
                        </div>
                    )}

                    {/* Right Section - Revenue Chart */}
                    <div className="channel-subscription__right">
                        <RevenueChart channelId={id || ''} />
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
                        <Loading />
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
                        tooltipShowDelay={0}
                    />
                )}
            </div>
        </ChannelSubscriptionContext.Provider >
    )
}

export default ChannelSubscription
