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
import { getPeriodicProfitTransactionReport } from "../../../../core/services/statistic/transaction-statistics/transaction-statistics.service"
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2"
import { PeriodicProfit } from "../../../../core/types/statistics"
import { formatDate } from "../../../../core/utils/date.util"
import SurveyTalkLoading from "../../../components/common/loading"

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)


export function RevenueChart({ activeTab }: { activeTab: string }) {
  const [revenueData, setRevenueData] = useState<PeriodicProfit[]>([])
  const [isLoading, setIsLoading] = useState<boolean>(true)
  const getLabel = (item: PeriodicProfit, index: number) => {
    switch (activeTab) {
      case 'Daily':
      case 'Weekly':
        return formatDate(item.StartDate);
      case 'Monthly':
        return `Tuần ${index + 1}`;
      case 'Yearly':
        return `Tháng ${index + 1}`;
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

  const getTooltipLabel = (item: PeriodicProfit) => {
    const revenue = item.Revenue.toLocaleString('vi-VN') + ' VND';

    if (activeTab === 'Daily' || activeTab === 'Weekly') {
      return `Doanh thu: ${revenue}`;
    }

    return `${formatDate(item.StartDate)} - ${formatDate(item.EndDate)} - Doanh thu: ${revenue}`;
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
        const res = await getPeriodicProfitTransactionReport(adminAxiosInstance, activeTab)
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
  const dataValues = revenueData.map((item) => item.Revenue)
  const backgroundColors = revenueData.map(item =>
    isCurrentPeriod(item.StartDate, item.EndDate) ? '#3E5DAB' : '#8CA4E0'
  )

  const data = {
    labels,
    datasets: [
      {
        label: "Doanh thu",
        data: dataValues,
        backgroundColor: backgroundColors,
        borderRadius: 2,
        maxBarThickness: 60,
      },
    ],
  }

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        callbacks: {
          label: (context: any) => {
            return getTooltipLabel(revenueData[context.dataIndex]);
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          stepSize: calculateStepSize(dataValues),
          callback: (value: any) => value / 1000 + "k",
        },
        grid: {
          color: "#dee2e6",
        },
      },
      x: {
        grid: {
          display: false,
        },
      },
    },
  }

  return (
    <div style={{ height: "350px" }}>
      {isLoading ? (
        <SurveyTalkLoading />
      ) : (
        <Bar data={data} options={options} />
      )}

    </div>
  )
}