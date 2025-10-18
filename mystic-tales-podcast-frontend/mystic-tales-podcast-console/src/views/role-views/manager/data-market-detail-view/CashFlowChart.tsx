import React from 'react';
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend,
} from 'chart.js';
import { Bar } from 'react-chartjs-2';
import "./styles.scss"
ChartJS.register(
    CategoryScale,
    LinearScale,
    BarElement,
    Title,
    Tooltip,
    Legend
);

const CashFlowChart = () => {
    const chartData = [
        { month: 'T1', income: 6, expense: 4 },
        { month: 'T2', income: 8, expense: 5 },
        { month: 'T3', income: 10, expense: 6 },
        { month: 'T4', income: 7, expense: 5 },
        { month: 'T5', income: 9, expense: 6 },
        { month: 'T6', income: 4, expense: 3 },
        { month: 'T7', income: 3, expense: 2 },
        { month: 'T8', income: 6, expense: 4 },
        { month: 'T9', income: 8, expense: 5 },
        { month: 'T10', income: 9, expense: 6 },
        { month: 'T11', income: 5, expense: 4 },
        { month: 'T12', income: 11, expense: 7 },
        { month: 'T13', income: 12, expense: 8 },
        { month: 'T14', income: 8, expense: 6 }
    ];

    const data = {
        labels: chartData.map(item => item.month),
        datasets: [
            {
                label: 'Tiền vào',
                data: chartData.map(item => item.income),
                backgroundColor: '#3E5DAB',
                borderColor: '#3E5DAB',
                borderWidth: 0,
                borderRadius: 2,
                borderSkipped: false,
                barThickness: 12,
                categoryPercentage: 0.8,
                barPercentage: 0.9,
            },
            {
                label: 'Tiền ra',
                data: chartData.map(item => item.expense),
                backgroundColor: '#FFC40D',
                borderColor: '#FFC40D',
                borderWidth: 0,
                borderRadius: 2,
                borderSkipped: false,
                barThickness: 12,
                categoryPercentage: 0.8,
                barPercentage: 0.9,
            },
        ],
    };

    const options = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false, // Tắt legend mặc định để dùng custom legend
            },
            tooltip: {
                backgroundColor: 'rgba(0, 0, 0, 0.8)',
                titleColor: '#fff',
                bodyColor: '#fff',
                borderColor: 'rgba(255, 255, 255, 0.1)',
                borderWidth: 1,
                cornerRadius: 0,
                displayColors: true,
                callbacks: {
                    title: function (context: any) {
                        return `Tháng ${context[0].label}`;
                    },
                    label: function (context: any) {
                        return `${context.dataset.label}: ${context.parsed.y}`;
                    }
                }
            },
        },
        scales: {
            x: {
                grid: {
                    display: false,
                },
                border: {
                    display: false,
                },
                ticks: {
                    color: '#6b7280',
                    font: {
                        size: 12,
                    },
                },
            },
            y: {
                beginAtZero: true,
                grid: {
                    color: 'rgba(107, 114, 128, 0.1)',
                    borderDash: [2, 2],
                },
                border: {
                    display: false,
                },
                ticks: {
                    display: false, // Ẩn các số trên trục Y
                },
            },
        },
        interaction: {
            intersect: false,
            mode: 'index',
        },
    };

    return (
        <div className="chart-section">
            <h3 className="chart-section__title mb-4">Dòng tiền</h3>
            <div style={{ height: '200px' }}>
                <Bar data={data} options={options as any} />
            </div>

            <div className="chart-section__legend">
                <div className="chart-section__legend-item">
                    <span className="chart-section__legend-color chart-section__legend-color--blue"></span>
                    <span>Tiền vào</span>
                </div>
                <div className="chart-section__legend-item">
                    <span className="chart-section__legend-color chart-section__legend-color--yellow"></span>
                    <span>Tiền ra</span>
                </div>
            </div>
        </div>

    );
};

export default CashFlowChart;