import { FC, useEffect, useState } from "react";
import { Row, Col, Card } from "react-bootstrap";
import { getSummaryCountAccountBalance, getSummaryProfitTransactionReport } from "../../../../core/services/statistic/transaction-statistics/transaction-statistics.service";
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
import { SummaryAccountBalanceCount, SummaryAccountRegistrationCount, SummaryProfit } from "../../../../core/types/statistics";
import { getPlatformFeedback, getSummaryCountAccountRegistration } from "../../../../core/services/statistic/user-statistics/user-statistics.service";
import { PlatformFeedback } from "@/core/types";

// Constants
const TIME_PERIODS = {
    Daily: "hôm qua",
    Weekly: "tuần trước",
    Monthly: "tháng trước",
    Yearly: "năm trước"
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
    period: string,
    formatter?: (val: number) => string,
    hideComparison?: boolean,
    customMessage?: string
): MetricItem => {
    const formattedValue = value !== undefined
        ? (formatter ? formatter(value) : value.toString())
        : "Đang tải...";

    return {
        title,
        value: formattedValue,
        change: {
            percent: percentChange !== undefined ? `${Math.abs(percentChange)}%` :
                hideComparison ? (customMessage || "Đánh giá tổng thể") : "Đang tải...",
            comparison: hideComparison ? "" : ` so với ${period}`
        },
        changeType: getChangeType(percentChange)
    };
};

const calculateAverageRating = (platformFeedback: PlatformFeedback[] | null): number => {
    if (!platformFeedback || platformFeedback.length === 0) return 0;

    const totalRating = platformFeedback.reduce((sum, feedback) => sum + feedback.RatingScore, 0);
    const average = totalRating / platformFeedback.length;
    return Number(average.toFixed(2)); // Làm tròn đến 2 chữ số thập phân
};
const getMetricsForPeriod = (
    period: TimePeriod,
    {
        summaryProfit,
        registrationCount,
        accountBalance,
        platformFeedback,
        fake
    }: {
        summaryProfit: SummaryProfit | null;
        registrationCount: SummaryAccountRegistrationCount | null;
        accountBalance: SummaryAccountBalanceCount | null;
        platformFeedback: PlatformFeedback[] | null;
        fake: any;
    }
): MetricItem[] => {
    const comparisonPeriod = TIME_PERIODS[period];
    const titlePrefix = period === "Daily" ? "hôm nay" :
        period === "Weekly" ? "tuần này" :
            period === "Monthly" ? "tháng này" : "năm hiện tại";
    const totalTransactions = (accountBalance?.DepositTransactionCount || 0) +
        (fake|| 0);
    const avgTransactionPercentChange = accountBalance
        ? ((accountBalance.DepositTransactionPercentChange || 0) +
            (accountBalance.WithdrawalTransactionPercentChange || 0)) / 2
        : undefined;
    const averageRating = calculateAverageRating(platformFeedback);

    return [
        createMetric(
            `Doanh thu ${titlePrefix}`,
            summaryProfit?.TotalRevenue,
            summaryProfit?.PercentChange,
            comparisonPeriod,
            (val) => `${formatValue(val)} VND`
        ),
        createMetric(
            `Người dùng mới ${titlePrefix}`,
            registrationCount?.NewRegistrationCount,
            registrationCount?.PercentChange,
            comparisonPeriod,
            formatValue
        ),
        createMetric(
            "Tổng số lượng giao dịch",
            totalTransactions,
            avgTransactionPercentChange,
            comparisonPeriod,
            formatValue
        ),
        createMetric(
            "Số lượng giao dịch nạp",
            accountBalance?.DepositTransactionCount,
            accountBalance?.DepositTransactionPercentChange,
            comparisonPeriod,
            formatValue
        ),
        createMetric(
            "Số lượng giao dịch rút",
            fake ,
            accountBalance?.WithdrawalTransactionPercentChange,
            comparisonPeriod,
            formatValue
        ),
        createMetric(
            "Đánh giá nền tảng",
            averageRating,
            undefined,
            "",
            (val) => val > 0 ? `${val}/5 ⭐` : "Chưa có đánh giá",
            true,
            platformFeedback && platformFeedback.length > 0
                ? `${platformFeedback.length} lượt đánh giá`
                : "Khuyến khích người dùng đánh giá"
        )
    ];
};

const MetricsCards: FC<{ activeTab: any }> = ({ activeTab }) => {
    const [summaryProfit, setSummaryProfit] = useState<SummaryProfit | null>(null);
    const [accountBalance, setAccountBalance] = useState<SummaryAccountBalanceCount | null>(null);
    const [registrationCount, setRegistrationCount] = useState<SummaryAccountRegistrationCount | null>(null);
    const [platformFeedback, setPlatformFeedback] = useState<PlatformFeedback[] | null>(null);
    const [loading, setLoading] = useState<boolean>(true);
    const [fake, setFake] = useState<any>(null);
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            try {
                const [summaryProfitRes, accountBalanceRes, registrationCountRes, platformFeedbackRes] = await Promise.all([
                    getSummaryProfitTransactionReport(adminAxiosInstance, activeTab),
                    getSummaryCountAccountBalance(adminAxiosInstance, activeTab),
                    getSummaryCountAccountRegistration(adminAxiosInstance, activeTab),
                    getPlatformFeedback(adminAxiosInstance, activeTab),
                ]);
                const apiUrl = "https://6857de4821f5d3463e566b36.mockapi.io/withdrawalRequests";
                const res = await fetch(apiUrl);
                const data = await res.json();
                setFake(data.length);
                setSummaryProfit(summaryProfitRes.success ? summaryProfitRes.data : null);
                setAccountBalance(accountBalanceRes.success ? accountBalanceRes.data : null);
                setRegistrationCount(registrationCountRes.success ? registrationCountRes.data : null);
                setPlatformFeedback(platformFeedbackRes.success ? platformFeedbackRes.data : null);
            } catch (error) {
                console.error("Error fetching metrics data:", error);
            } finally {
                setLoading(false);
            }
        };

        fetchData();
    }, [activeTab]);

    const metrics = loading
        ? [{
            title: "Đang tải...",
            value: "Đang tải...",
            change: { percent: "Đang tải...", comparison: "" },
            changeType: "neutral"
        }]
        : getMetricsForPeriod(activeTab, { summaryProfit, registrationCount, accountBalance, platformFeedback, fake });

    return (
        <Row className="mb-4">
            {metrics.map((metric, index) => (
                <Col key={index} xs={12} sm={6} lg={3} className="mb-3">
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
                                    <span className=" ms-2">{metric.change.comparison}</span>
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