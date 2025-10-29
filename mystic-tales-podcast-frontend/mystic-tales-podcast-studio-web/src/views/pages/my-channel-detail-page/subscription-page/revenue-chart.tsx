"use client"
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts"

const RevenueChart = () => {
  // Mock revenue data for the last 3 months
  const chartData = [
    { date: "Apr 6", revenue: 3000000 },
    { date: "Apr 8", revenue: 4200000 },
    { date: "Apr 10", revenue: 3800000 },
    { date: "Apr 12", revenue: 5100000 },
    { date: "Apr 14", revenue: 4500000 },
    { date: "Apr 16", revenue: 6200000 },
    { date: "Apr 18", revenue: 5800000 },
    { date: "Apr 20", revenue: 7100000 },
    { date: "Apr 22", revenue: 6500000 },
    { date: "Apr 24", revenue: 8200000 },
    { date: "Apr 26", revenue: 7800000 },
    { date: "Apr 28", revenue: 9100000 },
  ]

  return (
    <div className="revenue-chart">
      <div className="revenue-chart__header">
        <h3 className="revenue-chart__title">Revenue Statistics from this Channel</h3>
        <p className="revenue-chart__subtitle">Showing total visitors for the last 3 months</p>
      </div>

      <div className="revenue-chart__stats">
        <div className="revenue-chart__stat-item">
          <span className="revenue-chart__stat-label">This Month</span>
          <span className="revenue-chart__stat-value">3,000,000 VND</span>
        </div>
        <div className="revenue-chart__stat-item">
          <span className="revenue-chart__stat-label">3 Months Recently</span>
          <span className="revenue-chart__stat-value">12,000,000 VND</span>
        </div>
      </div>

      <div className="revenue-chart__graph">
        <ResponsiveContainer width="100%" height={300}>
          <BarChart data={chartData} margin={{ top: 20, right: 30, left: 0, bottom: 20 }}>
            <CartesianGrid strokeDasharray="3 3" stroke="#444" />
            <XAxis dataKey="date" stroke="#999" style={{ fontSize: "12px" }} />
            <YAxis stroke="#999" style={{ fontSize: "12px" }} />
            <Tooltip
              contentStyle={{
                backgroundColor: "#1a1a1a",
                border: "1px solid #666666",
                borderRadius: "4px",
              }}
              formatter={(value) => `${(value as number).toLocaleString("vi-VN")} VND`}
            />
            <Bar dataKey="revenue" fill="#AEE339" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  )
}

export default RevenueChart
