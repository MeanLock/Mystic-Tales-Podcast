"use client"

import { useContext, useEffect, useState } from "react"
import { FilterSurveyDetailViewContext } from ".."
import { calculateResponseData } from "./responseCalculator"
import { getFilterSurveySummary } from "../../../../../core/services/survey/survey-response/survey-response.service"
import { managerAxiosInstance } from "../../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler,
} from "chart.js"
import { Line } from "react-chartjs-2"

ChartJS.register(
  CategoryScale,
  LinearScale,
  BarElement,
  LineElement,
  PointElement,
  Title,
  Tooltip,
  Legend,
  ArcElement,
  Filler,
)

const ResponsesAnalysisTab = () => {
  const context = useContext(FilterSurveyDetailViewContext)
  const [processedData, setProcessedData] = useState<{ [key: string]: any }>({})
  const [loading, setLoading] = useState(false)

  // Get questions from context (these have complete Options data)
  const questions = context?.survey?.Questions?.sort((a, b) => a.Order - b.Order) || []

  useEffect(() => {
    const fetchSummary = async () => {
      if (!context?.survey?.Id) return

      setLoading(true)
      try {
        const response = await getFilterSurveySummary(managerAxiosInstance, context.survey.Id)
        //console.log("Fetching survey summary for ID:", context.survey.Id)

        //console.log("API Response:", response.data)

        if (response.data?.QuestionResponseSummaryLists) {
          const calculatedData = calculateResponseData(
            context?.survey?.Questions || [],
            response.data.QuestionResponseSummaryLists,
          )
          setProcessedData(calculatedData)
        }
      } catch (error) {
        console.error("Error fetching survey summary:", error)
      } finally {
        setLoading(false)
      }
    }

    fetchSummary()
  }, [context?.survey?.Id])

  const renderChoiceAnalysis = (question: any, index: number) => {
    const responseData = processedData[question.Id]
    if (!responseData || !("responses" in responseData)) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-warning">Không có dữ liệu phản hồi</div>
          </div>
        </div>
      )
    }

    return (
      <div key={question.Id} className="mb-5">
        <h5 className="mb-4 text-muted">
          Câu {index + 1}: {question.Content}
        </h5>
        <div className="ms-3">
          <div className="mb-3">
            <span className="badge bg-secondary me-2">Tổng phản hồi: {responseData.totalResponses}</span>
          </div>
          {responseData.responses.map((response: any, idx: number) => (
            <div key={idx} className="mb-3">
              <div className="d-flex justify-content-between align-items-center mb-1">
                <div className="d-flex align-items-center gap-2">
                  {response.MainImageUrl && (
                    <img
                      src={response.MainImageUrl || "/placeholder.svg"}
                      alt={response.content}
                      className="rounded"
                      style={{
                        width: "30px",
                        height: "30px",
                        objectFit: "cover",
                      }}
                    />
                  )}
                  <span className="text-dark">{response.content}</span>
                </div>
                <span className="text-muted small">
                  {response.count}/{responseData.totalResponses} ({response.percentage}%)
                </span>
              </div>
              <div className="progress" style={{ height: "8px" }}>
                <div
                  className="progress-bar bg-primary"
                  role="progressbar"
                  style={{ width: `${response.percentage}%` }}
                  aria-valuenow={response.percentage}
                  aria-valuemin={0}
                  aria-valuemax={100}
                ></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  const renderRatingAnalysis = (question: any, index: number) => {
    const responseData = processedData[question.Id]
    if (!responseData || !("averageRating" in responseData)) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-warning">Không có dữ liệu phản hồi</div>
          </div>
        </div>
      )
    }

    return (
      <div key={question.Id} className="mb-5">
        <h5 className="mb-4 text-muted">
          Câu {index + 1}: {question.Content}
        </h5>
        <div className="ms-3">
          <div className="mb-3">
            <span className="badge bg-success me-2">Điểm trung bình: {responseData.averageRating}/5</span>
            <span className="badge bg-secondary me-2">Tổng phản hồi: {responseData.totalResponses}</span>
          </div>
          {responseData.responses.map((response: any, idx: number) => (
            <div key={idx} className="mb-3">
              <div className="d-flex justify-content-between align-items-center mb-1">
                <span className="text-dark">
                  {response.rating} sao
                  <span className="text-warning ms-1">
                    {"★".repeat(response.rating)}
                    {"☆".repeat(5 - response.rating)}
                  </span>
                </span>
                <span className="text-muted small">
                  {response.count}/{responseData.totalResponses} ({response.percentage}%)
                </span>
              </div>
              <div className="progress" style={{ height: "8px" }}>
                <div
                  className="progress-bar bg-warning"
                  role="progressbar"
                  style={{ width: `${response.percentage}%` }}
                  aria-valuenow={response.percentage}
                  aria-valuemin={0}
                  aria-valuemax={100}
                ></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    )
  }

  const renderSliderAnalysis = (question: any, index: number) => {
    const responseData = processedData[question.Id]
    if (!responseData) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-warning">Không có dữ liệu phản hồi</div>
          </div>
        </div>
      )
    }

    // Single Slider
    if (responseData.type === "single") {
      // Create smooth area chart from distribution
      const chartData = {
        labels: responseData.distribution.map((item: any) => item.label),
        datasets: [
          {
            label: "Phân bố phản hồi",
            data: responseData.distribution.map((item: any) => item.count),
            fill: true,
            backgroundColor: "rgba(54, 162, 235, 0.3)",
            borderColor: "rgba(54, 162, 235, 1)",
            borderWidth: 3,
            pointRadius: 0, // Hide points for smoother look
            pointHoverRadius: 6,
            tension: 0.5, // Make it more curved/smooth
          },
        ],
      }

      const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false,
          },
          title: {
            display: true,
            text: "Phân bố giá trị",
            font: {
              size: 16,
            },
          },
          tooltip: {
            callbacks: {
              label: (context: any) => {
                const percentage =
                  responseData.totalResponses > 0
                    ? Math.round((context.parsed.y / responseData.totalResponses) * 100)
                    : 0
                return `${context.parsed.y} phản hồi (${percentage}%)`
              },
            },
          },
        },
        scales: {
          x: {
            title: {
              display: true,
              text: "Giá trị",
              font: {
                size: 14,
              },
            },
            grid: {
              display: false, // Clean look
            },
            ticks: {
              maxTicksLimit: 10, // Limit number of labels
            },
          },
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Số lượng phản hồi",
              font: {
                size: 14,
              },
            },
            grid: {
              display: true,
              color: "rgba(0, 0, 0, 0.05)",
            },
            ticks: {
              stepSize: 1,
            },
          },
        },
        elements: {
          line: {
            tension: 0.5, // Smooth curves
          },
        },
      }

      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="mb-3">
              <span className="badge bg-secondary me-2">
                Khoảng: {responseData.configMin} - {responseData.configMax}
              </span>
              <span className="badge bg-info me-2">Giá trị trung bình: {responseData.averageValue}</span>
              <span className="badge bg-primary me-2">Tổng phản hồi: {responseData.totalResponses}</span>
            </div>
            <div style={{ height: "400px", width: "100%" }}>
              <Line data={chartData} options={chartOptions} />
            </div>
          </div>
        </div>
      )
    }

    // Range Slider
    if (responseData.type === "range") {
      // Create smooth area chart from distribution
      const chartData = {
        labels: responseData.distribution.map((item: any) => item.label),
        datasets: [
          {
            label: "Phân bố phản hồi",
            data: responseData.distribution.map((item: any) => item.count),
            fill: true,
            backgroundColor: "rgba(54, 162, 235, 0.3)",
            borderColor: "rgba(54, 162, 235, 1)",
            borderWidth: 3,
            pointRadius: 0, // Hide points for smoother look
            pointHoverRadius: 6,
            tension: 0.5, // Make it more curved/smooth
          },
        ],
      }

      const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false,
          },
          title: {
            display: true,
            text: "Phân bố Range Values",
            font: {
              size: 16,
            },
          },
          tooltip: {
            callbacks: {
              label: (context: any) => {
                const percentage =
                  responseData.totalResponses > 0
                    ? Math.round((context.parsed.y / responseData.totalResponses) * 100)
                    : 0
                return `${context.parsed.y} phản hồi (${percentage}%)`
              },
            },
          },
        },
        scales: {
          x: {
            title: {
              display: true,
              text: "Khoảng giá trị",
              font: {
                size: 14,
              },
            },
            grid: {
              display: false, // Clean look
            },
            ticks: {
              maxTicksLimit: 8, // Limit number of labels
            },
          },
          y: {
            beginAtZero: true,
            title: {
              display: true,
              text: "Số lượng phản hồi",
              font: {
                size: 14,
              },
            },
            grid: {
              display: true,
              color: "rgba(0, 0, 0, 0.05)",
            },
            ticks: {
              stepSize: 1,
            },
          },
        },
        elements: {
          line: {
            tension: 0.5, // Smooth curves
          },
        },
      }

      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="mb-3">
              <span className="badge bg-secondary me-2">
                Khoảng: {responseData.configMin.toLocaleString()} - {responseData.configMax.toLocaleString()}
              </span>
              <span className="badge bg-danger me-2">TB Min: {responseData.averageMinValue.toLocaleString()}</span>
              <span className="badge bg-info me-2">TB Max: {responseData.averageMaxValue.toLocaleString()}</span>
              <span className="badge bg-primary me-2">Tổng phản hồi: {responseData.totalResponses}</span>
            </div>

            {/* Smooth Area Chart */}
            <div style={{ height: "400px", width: "100%" }}>
              <Line data={chartData} options={chartOptions} />
            </div>
          </div>
        </div>
      )
    }

    return (
      <div key={question.Id} className="mb-5">
        <h5 className="mb-4 text-muted">
          Câu {index + 1}: {question.Content}
        </h5>
        <div className="ms-3">
          <div className="alert alert-warning">Loại slider không được hỗ trợ</div>
        </div>
      </div>
    )
  }

  const renderRankingAnalysis = (question: any, index: number) => {
    const responseData = processedData[question.Id]
    if (!responseData || !("averageRankings" in responseData)) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-warning">Không có dữ liệu phản hồi</div>
          </div>
        </div>
      )
    }

    return (
      <div key={question.Id} className="mb-5">
        <h5 className="mb-4 text-muted">
          Câu {index + 1}: {question.Content}
        </h5>
        <div className="ms-3">
          <div className="mb-3">
            <span className="badge bg-secondary me-2">Tổng phản hồi: {responseData.totalResponses}</span>
          </div>
          {responseData.averageRankings
            .sort((a: any, b: any) => a.averageRank - b.averageRank)
            .map((ranking: any, idx: number) => (
              <div key={idx} className="mb-3">
                <div className="d-flex justify-content-between align-items-center mb-1">
                  <span className="text-dark">
                    <span className="badge bg-secondary me-2">#{idx + 1}</span>
                    {ranking.content}
                  </span>
                  <span className="text-muted small">Xếp hạng TB: {ranking.averageRank.toFixed(1)}</span>
                </div>
                <div className="progress" style={{ height: "8px" }}>
                  <div
                    className="progress-bar bg-success"
                    role="progressbar"
                    style={{ width: `${((6 - ranking.averageRank) / 5) * 100}%` }}
                  ></div>
                </div>
              </div>
            ))}
        </div>
      </div>
    )
  }

  const renderTextAnalysis = (question: any, index: number) => {
    const responseData = processedData[question.Id]
    if (!responseData || !("textResponses" in responseData)) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-warning">Không có dữ liệu phản hồi</div>
          </div>
        </div>
      )
    }

    return (
      <div key={question.Id} className="mb-5">
        <h5 className="mb-4 text-muted">
          Câu {index + 1}: {question.Content}
        </h5>
        <div className="ms-3">
          <div className="mb-3">
            <span className="badge bg-secondary me-2">Tổng phản hồi: {responseData.responseCount}</span>
          </div>
          <div className="border rounded p-3" style={{ maxHeight: "400px", overflowY: "auto" }}>
            <h6 className="mb-3 text-muted">Phản hồi từ người dùng:</h6>
            {responseData.textResponses.map((response: any, idx: number) => (
              <div key={response.id} className="mb-3 p-3 bg-light rounded">
                <div className="d-flex justify-content-between align-items-start mb-2">
                  <small className="text-muted">Phản hồi #{response.id}</small>
                  <small className="text-muted">{response.timestamp}</small>
                </div>
                <p className="mb-0 text-dark" style={{ fontSize: "14px", lineHeight: "1.5" }}>
                  {response.content}
                </p>
              </div>
            ))}
            {responseData.textResponses.length < responseData.responseCount && (
              <div className="text-center mt-3">
                <small className="text-muted">
                  Hiển thị {responseData.textResponses.length} trong số {responseData.responseCount} phản hồi
                </small>
              </div>
            )}
          </div>
        </div>
      </div>
    )
  }

  const renderQuestionAnalysis = (question: any, index: number) => {
    // Check if data is available for this question
    if (!processedData[question.Id]) {
      return (
        <div key={question.Id} className="mb-5">
          <h5 className="mb-4 text-muted">
            Câu {index + 1}: {question.Content}
          </h5>
          <div className="ms-3">
            <div className="alert alert-info">{loading ? "Đang tải dữ liệu..." : "Không có dữ liệu phản hồi"}</div>
          </div>
        </div>
      )
    }

    switch (question.QuestionTypeId) {
      case 1: // Single Choice
      case 2: // Multiple Choice
        return renderChoiceAnalysis(question, index)
      case 6: // Rating
        return renderRatingAnalysis(question, index)
      case 3: // Single Slider
      case 4: // Range Slider
        return renderSliderAnalysis(question, index)
      case 5: // Text Input
        return renderTextAnalysis(question, index)
      case 7: // Ranking
        return renderRankingAnalysis(question, index)
      default:
        return (
          <div key={question.Id} className="mb-5">
            <h5 className="mb-4 text-muted">
              Câu {index + 1}: {question.Content}
            </h5>
            <div className="ms-3">
              <div className="alert alert-secondary">Loại câu hỏi này chưa hỗ trợ phân tích</div>
            </div>
          </div>
        )
    }
  }

  if (loading) {
    return (
      <div className="container-fluid">
        <div className="bg-white p-4 rounded text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="mt-2">Đang tải dữ liệu phân tích...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="container-fluid">
      <div className="bg-white p-4 rounded">
        {questions?.map((question: any, index: number) => renderQuestionAnalysis(question, index))}
      </div>
    </div>
  )
}

export default ResponsesAnalysisTab
