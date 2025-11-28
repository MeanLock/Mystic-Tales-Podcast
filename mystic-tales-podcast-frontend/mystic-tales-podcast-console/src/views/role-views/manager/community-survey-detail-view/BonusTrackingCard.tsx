import type React from "react"
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
} from "chart.js"
import { Line } from "react-chartjs-2"
import { useContext } from "react"
import { CommunitySurveyDetailViewContext } from "."

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

interface BonusTrackingCardProps { }

export const BonusTrackingCard: React.FC<BonusTrackingCardProps> = () => {
    const context = useContext(CommunitySurveyDetailViewContext);
    const data = context?.surveyData?.SurveyRewardTrackings || [];
    const latestTracking = data.reduce((latest, item) => {
        return !latest || new Date(item.CreatedAt) > new Date(latest.CreatedAt) ? item : latest;
    }, null as typeof data[0] | null);

   const calculateCurrentBonus = () => {
    if (data.length === 0) {
        return context?.surveyData?.TakerBaseRewardPrice ?? 0;
    }
    return (latestTracking?.RewardPrice ?? 0) - (context?.surveyData?.TakerBaseRewardPrice ?? 0);
};
    const currentBonus = calculateCurrentBonus();
    const chartData = {
        labels: data.map((item) => new Date(item.CreatedAt).toLocaleDateString("vi-VN")),
        datasets: [
            {
                label: "Bonus Tracking",
                data: data.map((item) => item.RewardPrice - (context?.surveyData?.TakerBaseRewardPrice ?? 0)),
                borderColor: "rgb(255, 99, 132)",
                backgroundColor: "rgba(255, 99, 132, 0.2)",
                tension: 0.1,
            },
        ],
    }

    const options = {
        responsive: true,
        plugins: {
            legend: {
                display: false,
            },
            title: {
                display: false,
            },
        },
        scales: {
            y: {
                beginAtZero: true,
            },
        },
    }

    return (
        <div className="bonus-tracking-chart">
            <div className="mb-4">
                <h3 className="fw-bold text-success">{currentBonus}đ</h3>
            </div>
            <div style={{ height: "180px" }}>
                <Line data={chartData} options={options} />
            </div>

        </div>
    )
}
