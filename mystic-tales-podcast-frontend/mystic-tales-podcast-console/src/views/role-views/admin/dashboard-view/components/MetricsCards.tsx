import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { getTotalBooking } from "@/core/services/booking/booking.service";
import { getTotalSubscription } from "@/core/services/subscription/subscription.service";
import { FC, useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";

// Constants
const TIME_PERIODS = {
    Daily: "yesterday",
    Monthly: "last month",
    Yearly: "last year"
} as const;

type TimePeriod = keyof typeof TIME_PERIODS;

interface MetricItem {
    title: string;
    value: string | number;
    change: {
        percent: string;
        comparison: string;
    };
    changeType: "increase" | "decrease" | "neutral";
}

const formatValue = (value: number): string => {
    return value.toLocaleString("vi-VN");
};

const getChangeType = (percentChange?: number): "increase" | "decrease" | "neutral" => {
    if (percentChange === undefined) return "neutral";
    return percentChange >= 0 ? "increase" : "decrease";
};

const createMetric = (
    title: string,
    value: number | undefined,
    percentChange: number | undefined,
    period: string
): MetricItem => {
    const formattedValue = value !== undefined ? `${formatValue(value)} Coins` : "Loading...";

    return {
        title,
        value: formattedValue,
        change: {
            percent: percentChange !== undefined ? `${Math.abs(percentChange).toFixed(2)}%` : "Loading...",
            comparison: ` compared to ${period}`
        },
        changeType: getChangeType(percentChange)
    };
};

const getMetricsForPeriod = (
    period: TimePeriod,
    totalBooking: any | null,
    totalSubscription: any | null
): MetricItem[] => {
    const comparisonPeriod = TIME_PERIODS[period];
    const titlePrefix = period === "Daily" ? "Today" :
            period === "Monthly" ? "This month" : "This year";

    return [
         createMetric(
            `Total Subscription Income ${titlePrefix}`,
            totalSubscription?.TotalPodcastSubscriptionIncomeAmount,
            totalSubscription?.TotalPodcastSubscriptionIncomePercentChange,
            comparisonPeriod
        ),
        createMetric(
            `Total Booking Income ${titlePrefix}`,
            totalBooking?.TotalBookingIncomeAmount,
            totalBooking?.TotalBookingIncomePercentChange,
            comparisonPeriod
        )
       
    ];
};

const MetricsCards: FC<{ activeTab: any }> = ({ activeTab }) => {
    const [totalBooking, setTotalBooking] = useState<any | null>(null);
    const [totalSubscription, setTotalSubscription] = useState<any | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                const [totalBookingRes, totalSubscriptionRes] = await Promise.all([
                    getTotalBooking(adminAxiosInstance, activeTab),
                    getTotalSubscription(adminAxiosInstance, activeTab),
                ]);
                setTotalBooking(totalBookingRes.success ? totalBookingRes.data.TotalBookingStatisticReport : null);
                setTotalSubscription(totalSubscriptionRes.success ? totalSubscriptionRes.data.TotalPodcastSubscriptionStatisticReport : null);
            } catch (error) {
                console.error("Error fetching metrics data:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [activeTab]);

    const metrics = loading
        ? [
            {
                title: "Loading...",
                value: "Loading...",
                change: { percent: "Loading...", comparison: "" },
                changeType: "neutral" as const
            },
            {
                title: "Loading...",
                value: "Loading...",
                change: { percent: "Loading...", comparison: "" },
                changeType: "neutral" as const
            }
        ]
        : getMetricsForPeriod(activeTab, totalBooking, totalSubscription);

    return (
        <Row className="mb-4">
            {metrics.map((metric, index) => (
                <Col key={index} xs={12} sm={6} lg={6} className="mb-3">
                    <Card className="MetricsCards__card">
                        <Card.Body>
                            <div className="d-flex flex-column">
                                <small className="mb-4 fw-semibold">{metric.title}</small>
                                <h3 className="mb-2 fw-bold mt-3">{metric.value}</h3>
                                <small className="d-flex align-items-center">
                                    {metric.changeType === "increase" && <span className="text-success me-1">↗</span>}
                                    {metric.changeType === "decrease" && <span className="text-danger me-1">↘</span>}
                                    <span className={metric.changeType === "increase" ? "text-success" :
                                        metric.changeType === "decrease" ? "text-danger" : ""}>
                                        {metric.change.percent}
                                    </span>
                                    <span className="ms-2">{metric.change.comparison}</span>
                                </small>
                            </div>
                        </Card.Body>
                    </Card>
                </Col>
            ))}
        </Row>
    );
};

export default MetricsCards;