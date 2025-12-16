import { useEffect, useState } from "react"
import { Bar } from "react-chartjs-2"
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
} from "chart.js"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Loading from "@/views/components/common/loading"
import { getSummaryBooking } from "@/core/services/booking/booking.service"
import { formatDate } from "@/core/utils/date.util"

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

export type BookingProfit = {
    StartDate: string;
    EndDate: string;
    Amount: number;
};

export function BookingRevenueChart({ activeTab }: { activeTab: string }) {
    const [revenueData, setRevenueData] = useState<BookingProfit[]>([])
    const [isLoading, setIsLoading] = useState<boolean>(true)
    const getLabel = (item: BookingProfit, index: number) => {
        switch (activeTab) {
            case 'Daily':
            case 'Weekly':
                return formatDate(item.StartDate);
            case 'Monthly':
                return `Week ${index + 1}`;
            case 'Yearly':
                return new Date(item.StartDate).toLocaleString('en-US', { month: 'short' });
            default:
                return '';
        }
    }

    const isCurrentPeriod = (startDate: string, endDate: string) => {
        const currentDate = new Date();
        const start = new Date(startDate);
        const end = new Date(endDate);

        if (activeTab === 'Daily' || activeTab === 'Weekly') {
            return currentDate.toDateString() === start.toDateString();
        }
        start.setHours(0, 0, 0, 0);
        end.setHours(23, 59, 59, 999);
        return currentDate >= start && currentDate <= end;
    }

    const getTooltipLabel = (item: BookingProfit) => {
        const revenue = item.Amount.toLocaleString('vi-VN') + ' Coins';

        if (activeTab === 'Daily' || activeTab === 'Weekly') {
            return `Revenue: ${revenue}`;
        }

        return `${formatDate(item.StartDate)} - ${formatDate(item.EndDate)} - Revenue: ${revenue}`;
    }

    const calculateStepSize = (values: number[]) => {
        if (values.length === 0) return 10000;

        const maxValue = Math.max(...values);

        let stepSize = Math.ceil(maxValue / 7);

        const magnitude = Math.pow(10, Math.floor(Math.log10(stepSize)));
        stepSize = Math.ceil(stepSize / magnitude) * magnitude;

        return stepSize;
    };

    useEffect(() => {
        const fetchRevenue = async () => {
            try {
                setIsLoading(true)
                const res = await getSummaryBooking(loginRequiredAxiosInstance, activeTab)
                if (res.success && res.data) {
                    setRevenueData(res.data)
                }
            } catch (err) {
                console.error("Failed to fetch revenue data:", err)
            } finally {
                setIsLoading(false)
            }
        }

        fetchRevenue()
    }, [activeTab])

    const labels = revenueData.map((item, index) => getLabel(item, index))
    const dataValues = revenueData.map((item) => item.Amount)
    const backgroundColors = revenueData.map(item =>
        isCurrentPeriod(item.StartDate, item.EndDate) ? '#AEE339' : '#AEE339'
    )

    const data = {
        labels,
        datasets: [
            {
                label: "Revenue",
                data: dataValues,
                backgroundColor: backgroundColors,
                borderRadius: 6,
                maxBarThickness: 56,
                borderWidth: 1,
            },
        ],
    }

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: { display: false },
            tooltip: {
                backgroundColor: 'rgba(40, 40, 40, 1)',
                titleColor: '#AEE339',
                bodyColor: '#d9d9d9',
                borderWidth: 1,
                callbacks: {
                    label: (context: any) => getTooltipLabel(revenueData[context.dataIndex])
                }
            }
        },
        scales: {
            y: {
                beginAtZero: true,
                ticks: {
                    stepSize: calculateStepSize(dataValues),
                    color: '#262626',
                    callback: (value: any) => value / 1000 + 'k',
                },
                grid: {
                    color: '#d9d9d9',
                },
            },
            x: {
                ticks: { color: '#262626' },
                grid: { display: false },
            },
        },
    }

    return (
        <div style={{ height: '350px', position: 'relative' }}>
            {isLoading ? (
                <div className="flex justify-center items-center " >
                    <Loading />
                </div>
            ) : (
                <Bar data={data} options={options} />
            )}

        </div>
    )
}