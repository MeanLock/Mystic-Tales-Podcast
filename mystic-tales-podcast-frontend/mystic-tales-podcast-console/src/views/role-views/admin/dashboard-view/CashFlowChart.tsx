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
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2"
import { formatDate } from "../../../../core/utils/date.util"
import { PeriodicAccountBalanceCount } from "../../../../core/types/statistics"
import { getPeriodicAmountAccountBalance } from "../../../../core/services/statistic/transaction-statistics/transaction-statistics.service"

ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend)

export function CashFlowChart({ activeTab }: { activeTab: string }) {
  const [moneyFlowData, setMoneyFlowData] = useState<PeriodicAccountBalanceCount[]>([])
  const [isLoading, setIsLoading] = useState<boolean>(true)

  const getLabel = (item: PeriodicAccountBalanceCount, index: number) => {
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

 const getTooltipLabel = (item: PeriodicAccountBalanceCount) => {
  const moneyIn = item.DepositTransactionAmount.toLocaleString('vi-VN') + ' VND';
  const moneyOut = item.WithdrawalTransactionAmount.toLocaleString('vi-VN') + ' VND';
  if (activeTab === 'Daily' || activeTab === 'Weekly') {
    //console.log("Daily or Weekly data:", moneyIn);
    return [
      `Nạp: ${moneyIn}`,
      `Rút: ${moneyOut}`
    ];
  }
  
  return [
    `${formatDate(item.StartDate)} - ${formatDate(item.EndDate)}`,
    `Nạp: ${moneyIn}`,
    `Rút: ${moneyOut}`
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
        const res = await getPeriodicAmountAccountBalance(adminAxiosInstance, activeTab)
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
      label: "Nạp",
      data: inData,
      backgroundColor: "#ffc40d",
      borderRadius: 2,
      maxBarThickness: 30,
    },
    {
      label: "Rút",
      data: outData,
      backgroundColor: "#3E5DAB",
      borderRadius: 2,
      maxBarThickness: 30,
    },
  ],
}

 const options = {
  responsive: true,
  maintainAspectRatio: false,
  plugins: {
    legend: {
      position: "bottom" as const,
      labels: {
        usePointStyle: true,
        padding: 20,
      },
    },
    tooltip: {
      callbacks: {
        label: (context: any) => {
 const tooltipLabels = getTooltipLabel(moneyFlowData[context.dataIndex]);
          // Return the appropriate label based on dataset index (0 for deposits, 1 for withdrawals)
          return context.datasetIndex === 0 ? tooltipLabels[0] : tooltipLabels[1];        },
        title: (context: any) => {
          const labels = getTooltipLabel(moneyFlowData[context[0].dataIndex]);
          return activeTab === 'Daily' || activeTab === 'Weekly' ? 
            formatDate(moneyFlowData[context[0].dataIndex].StartDate) : 
            labels[0];
        }
      }
    }
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        stepSize: calculateStepSize([...inData, ...outData]),
        callback: (value: any) => value.toLocaleString('vi-VN'),
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
    <div style={{ height: "320px" }}>
      <Bar data={data} options={options} />
    </div>
  )
}
