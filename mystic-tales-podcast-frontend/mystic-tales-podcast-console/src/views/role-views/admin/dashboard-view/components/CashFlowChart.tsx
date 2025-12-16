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
import { getSummaryBalance } from "@/core/services/transaction/transaction.service"
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Loading from "@/views/components/common/loading"
import { formatDate } from "@/core/utils/date.util"

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

export type PeriodicAccountBalanceCount = {
  StartDate: string;
  EndDate: string;
  DepositTransactionAmount: number;
  WithdrawalTransactionAmount: number;
};
export function CashFlowChart({ activeTab }: { activeTab: string }) {
  const [moneyFlowData, setMoneyFlowData] = useState<PeriodicAccountBalanceCount[]>([])
  const [isLoading, setIsLoading] = useState<boolean>(true)

  const getLabel = (item: PeriodicAccountBalanceCount, index: number) => {
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

  const getTooltipLabel = (item: PeriodicAccountBalanceCount) => {
    const moneyIn = item.DepositTransactionAmount.toLocaleString('vi-VN') + ' Coins';
    const moneyOut = item.WithdrawalTransactionAmount.toLocaleString('vi-VN') + ' Coins';
    console.log("Tooltip Data:", moneyIn, moneyOut);
    if (activeTab === 'Daily' || activeTab === 'Weekly') {
      //console.log("Daily or Weekly data:", moneyIn);
      return [
        `Deposit: ${moneyIn}`,
        `Withdrawal: ${moneyOut}`
      ];
    }


    return [
      `${formatDate(item.StartDate)} - ${formatDate(item.EndDate)}`,
      `Deposit: ${moneyIn} `,
      `Withdrawal: ${moneyOut}`
    ];
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
    const fetchData = async () => {
      try {
        setIsLoading(true)
        const res = await getSummaryBalance(loginRequiredAxiosInstance, activeTab)
        if (res.success && res.data) {
          setMoneyFlowData(res.data)
        }
        //console.log("Money Flow Data:", res.data)
      } catch (err) {
        console.error("Failed to fetch revenue data:", err)
      } finally {
        setIsLoading(false)
      }
    }

    fetchData()
  }, [activeTab])

  const labels = moneyFlowData.map((item, index) => getLabel(item, index))
  const inData = moneyFlowData.map((item) => item.DepositTransactionAmount)
  const outData = moneyFlowData.map((item) => item.WithdrawalTransactionAmount)

  const data = {
    labels,
    datasets: [
      {
        label: "Deposit",
        data: inData,
        backgroundColor: "#AEE339",
        borderRadius: 6,
        borderWidth: 1,
        maxBarThickness: 30,
      },
      {
        label: "Withdrawal",
        data: outData,
        backgroundColor: "#ff9800",
        borderRadius: 6,
        borderWidth: 1,
        maxBarThickness: 30,
      },
    ],
  }

  const options = {
    responsive: true,
    maintainAspectRatio: false,
      interaction: {
      mode: 'index' as const,        // quan trọng: lấy cả 2 dataset cùng index
      intersect: false
    },
    plugins: {
      legend: {
        position: "bottom" as const,
        labels: {
          usePointStyle: true,
          padding: 20,
        },
      },
      tooltip: {
        displayColors: true,
        callbacks: {
          title: (ctx: any) => {
            const i = ctx[0].dataIndex
            if (activeTab === 'Daily' || activeTab === 'Weekly') {
              return formatDate(moneyFlowData[i].StartDate)
            }
            return `${formatDate(moneyFlowData[i].StartDate)} - ${formatDate(moneyFlowData[i].EndDate)}`
          },
          label: (context: any) => {
            const item = moneyFlowData[context.dataIndex]
            const value = context.datasetIndex === 0
              ? item.DepositTransactionAmount
              : item.WithdrawalTransactionAmount
            return `${context.dataset.label}: ${value.toLocaleString('vi-VN')} Coins`
          }
        }
      }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: {
          color: '#000',
          stepSize: calculateStepSize([...inData, ...outData]),
          callback: (value: any) => value.toLocaleString('vi-VN'),
        },
        grid: { color: '#d9d9d9' },
      },
      x: {
        ticks: { color: '#000' },
        grid: { display: false },
      },
    },
  }

  return (
    <div style={{ height: "320px" }}>
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
