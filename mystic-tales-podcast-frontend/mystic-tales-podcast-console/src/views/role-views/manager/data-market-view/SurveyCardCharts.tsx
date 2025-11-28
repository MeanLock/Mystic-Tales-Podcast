import React from 'react';
import { Doughnut } from 'react-chartjs-2';
import {
    Chart as ChartJS,
    ArcElement,
    Tooltip,
    Legend
} from 'chart.js';
import './styles.scss';
// Register Chart.js components
ChartJS.register(ArcElement, Tooltip, Legend);

const SurveyCardCharts = () => {
    // Sample data - replace with your actual data
    const data = {
        dataOwnerPercentage: 30,
        dataMarketPercentage: 15,
        platformPercentage: 4
    };

    // Common chart options
    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                enabled: false
            }
        },
        cutout: '50%',
        borderWidth: 0,
        elements: {
            arc: {
                borderWidth: 0
            }
        }
    };

    // Chart data configurations
    const createChartData = (percentage: number, colors: { main: string, remaining: string }) => ({
        datasets: [{
            data: [percentage, 100 - percentage],
            backgroundColor: [colors.main, colors.remaining],
            borderWidth: 0
        }]
    });

    const charts = [
        {
            data: createChartData(data.dataOwnerPercentage, {
                main: '#3E5DAB',      // Blue
                remaining: '#9eadd5'  // Light blue
            }),
            percentage: data.dataOwnerPercentage,
            label: 'So với Data cung chủ đề',
            color: '#3E5DAB'
        },
        {
            data: createChartData(data.dataMarketPercentage, {
                main: '#FFC40D',      // Yellow
                remaining: '#FFEAA8'  // Light yellow
            }),
            percentage: data.dataMarketPercentage,
            label: 'So với Data Market',
            color: '#FFC40D'
        },
        {
            data: createChartData(data.platformPercentage, {
                main: '#50A350',      // Green
                remaining: '#BEE1BE'  // Light green
            }),
            percentage: data.platformPercentage,
            label: 'So với Platform',
            color: '#50A350'
        }
    ];

    return (
        <div className="survey-card__chart">
            {charts.map((chart, index) => (
                <div key={index} className="survey-card__chart-item">
                    <div className="survey-card__pie-chart">
                        <Doughnut
                            data={chart.data}
                            options={chartOptions}
                        />
                        <div className="survey-card__chart-percentage" style={{ color: chart.color }}>
                            {chart.percentage}%
                        </div>
                    </div>
                    <div className="survey-card__chart-label">
                        {chart.label}
                    </div>
                </div>
            ))}


        </div>
    );
};

export default SurveyCardCharts;