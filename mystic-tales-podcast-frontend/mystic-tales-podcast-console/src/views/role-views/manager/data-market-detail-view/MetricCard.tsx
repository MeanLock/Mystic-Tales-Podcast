"use client"
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Filler,
} from "chart.js"
import { Line } from "react-chartjs-2"

ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Filler)

interface MetricCardProps {
  icon: string
  title: string
  value: string
  color: "green" | "blue" | "yellow"
  chartData: number[]
}

const colorConfig = {
  green: {
    gradient: "linear-gradient(135deg, #dcfce7 0%, #bbf7d0 100%)",
    chartColor: "#4ade80",
    chartGradient: ["rgba(74, 222, 128, 0.3)", "rgba(74, 222, 128, 0.05)"],
  },
  blue: {
    gradient: "linear-gradient(135deg, #dbeafe 0%, #bfdbfe 100%)",
    chartColor: "#3b82f6",
    chartGradient: ["rgba(59, 130, 246, 0.3)", "rgba(59, 130, 246, 0.05)"],
  },
  yellow: {
    gradient: "linear-gradient(135deg, #fef3c7 0%, #fde68a 100%)",
    chartColor: "#f59e0b",
    chartGradient: ["rgba(245, 158, 11, 0.3)", "rgba(245, 158, 11, 0.05)"],
  },
}

export function MetricCard({ icon, title, value, color, chartData }: MetricCardProps) {
  const config = colorConfig[color]

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: { display: false },
      tooltip: { enabled: false },
    },
    scales: {
      x: { display: false, grid: { display: false } },
      y: { display: false, grid: { display: false } },
    },
    elements: {
      point: { radius: 0, hoverRadius: 0 },
      line: { borderWidth: 1.5, tension: 0.4 },
    },
    interaction: { intersect: false },
  }

  const data = {
    labels: chartData.map((_, index) => index),
    datasets: [
      {
        data: chartData,
        borderColor: config.chartColor,
        backgroundColor: (context: any) => {
          const ctx = context.chart.ctx
          const gradient = ctx.createLinearGradient(0, 0, 0, 60)
          gradient.addColorStop(0, config.chartGradient[0])
          gradient.addColorStop(1, config.chartGradient[1])
          return gradient
        },
        fill: true,
      },
    ],
  }

  return (
    <div className="metric-card" style={{ background: config.gradient }}>
      <div className="metric-card__header">
        <div className="metric-card__icon-container">
          <span className="metric-card__icon">{icon}</span>
        </div>
        <div className="metric-card__info">
          <div className="metric-card__title" title={title}>
            {title}
          </div>
          <div className="metric-card__value" title={value}>
            {value}
          </div>
        </div>
      </div>
      <div className="metric-card__chart">
        <Line data={data} options={options} />
      </div>
    </div>
  )
}
