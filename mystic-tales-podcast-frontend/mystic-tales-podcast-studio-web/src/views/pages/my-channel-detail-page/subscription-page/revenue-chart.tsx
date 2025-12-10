import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from "recharts"
import { useEffect, useMemo, useState } from "react"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import {Tooltip as TooltipMui} from "@mui/material";
import { Question } from "phosphor-react";
import { getChannelSubscriptionTransaction } from "@/core/services/subscription/subscription.service";
interface RevenueChartProps {
  channelId: string;
}

interface RevenueData {
  Last30DayList: Array<{
    Date: string;
    Amount: number;
  }>;
  LastMonthTotalAmount: number;
  Last3MonthTotalAmount: number;
}

const RevenueChart = ({ channelId }: RevenueChartProps) => {
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [revenueData, setRevenueData] = useState<RevenueData | null>(null);

  const fetchSubscriptionTransactionList = async () => {
    setIsLoading(true);
    try {
      const res = await getChannelSubscriptionTransaction(loginRequiredAxiosInstance, channelId);
      console.log("Fetched subscription revenue:", res.data);
      if (res.success && res.data) {
        setRevenueData(res.data);
      } else {
        console.error('API Error:', res.message);
      }
    } catch (error) {
      console.error('Error fetching revenue data:', error);
    } finally {
      setIsLoading(false);
    }
  }

  useEffect(() => {
    if (channelId) {
      fetchSubscriptionTransactionList();
    }
  }, [channelId])

  const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(value);
  }

  const chartData = useMemo(() => {
    if (!revenueData?.Last30DayList) return [];
    return revenueData.Last30DayList.map(item => ({
      date: new Date(item.Date).toLocaleDateString('vi-VN', { month: 'short', day: 'numeric' }),
      revenue: item.Amount
    }));
  }, [revenueData]);

  const thisMonthRevenue = revenueData?.LastMonthTotalAmount || 0;
  const threeMonthsRevenue = revenueData?.Last3MonthTotalAmount || 0;

  if (isLoading) {
    return (
      <div className="revenue-chart" style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        height: '400px'
      }}>
        <div style={{ color: '#999', fontSize: '14px' }}>Loading revenue data...</div>
      </div>
    );
  }

  return (
    <div className="revenue-chart">
      <div className="revenue-chart__header">
        <div className="flex justify-center gap-2 items-center">
          <h3 className="revenue-chart__title ">Revenue Statistics from this Channel</h3>
          <TooltipMui placement="top-end" title="After 7 days of a successful subscription payment, the revenue will be credited to your account.">
            <Question color="var(--third-grey)" size={16} />
          </TooltipMui >
        </div>
        <p className="revenue-chart__subtitle">Showing total revenue for the last 30 days</p>
      </div>

      <div className="revenue-chart__stats">
        <div className="revenue-chart__stat-item">
          <span className="revenue-chart__stat-label">Last Month</span>
          <span className="revenue-chart__stat-value">{formatCurrency(thisMonthRevenue)}</span>
        </div>
        <div className="revenue-chart__stat-item">
          <span className="revenue-chart__stat-label">Last 3 Months</span>
          <span className="revenue-chart__stat-value">{formatCurrency(threeMonthsRevenue)}</span>
        </div>
      </div>

      <div className="revenue-chart__graph">
        {chartData.length === 0 ? (
          <div style={{
            display: 'flex',
            justifyContent: 'center',
            alignItems: 'center',
            height: '300px',
            color: '#666',
            fontSize: '14px'
          }}>
            No revenue data available for the last 30 days
          </div>
        ) : (
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
                formatter={(value) => formatCurrency(value as number)}
              />
              <Bar dataKey="revenue" fill="#AEE339" radius={[4, 4, 0, 0]} />
            </BarChart>
          </ResponsiveContainer>
        )}
      </div>
    </div>
  )
}

export default RevenueChart
