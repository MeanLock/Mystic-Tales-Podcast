"use client"

import { Doughnut } from "react-chartjs-2"
import { Chart as ChartJS, ArcElement, Tooltip, Legend } from "chart.js"
import { Row, Col } from "react-bootstrap"

ChartJS.register(ArcElement, Tooltip, Legend)

export function ProfitDistributionCharts() {
  const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun"]

  const generateChartData = (monthIndex: number) => {
    const dataVariations = [
      [30, 25, 25, 20],
      [35, 20, 30, 15],
      [25, 30, 25, 20],
      [40, 15, 25, 20],
      [20, 35, 25, 20],
      [30, 25, 20, 25],
    ]

    return {
      datasets: [
        {
          data: dataVariations[monthIndex],
          backgroundColor: [
            "#0d6efd", // Surveys - Bootstrap Primary
            "#ffc107", // Data Markets - Bootstrap Warning
            "#6f42c1", // Template Markets - Bootstrap Purple
            "#fff3cd", // Subscriptions - Light Yellow
          ],
          borderWidth: 0,
          cutout: "60%",
        },
      ],
    }
  }

  const options = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        enabled: true,
      },
    },
  }

  return (
    <div>
      <Row className="mb-4">
        {months.map((month, index) => (
          <Col key={month} xs={6} md={4} lg={2} className="mb-4">
            <div className="d-flex flex-column align-items-center">
              <div className="position-relative" style={{ width: "150px", height: "150px" }}>
                <Doughnut data={generateChartData(index)} options={options} />
                <div className="position-absolute top-50 start-50 translate-middle d-flex align-items-center justify-content-center">
                  <span className="fw-medium small">{month}</span>
                </div>
              </div>
            </div>
          </Col>
        ))}
      </Row>

      {/* Legend */}
      <Row className="justify-content-center">
        <Col xs="auto">
          <div className="d-flex flex-wrap justify-content-center gap-4">
            <div className="d-flex align-items-center">
              <div
                className="rounded-circle me-2"
                style={{ width: "12px", height: "12px", backgroundColor: "#0d6efd" }}
              ></div>
              <span className="small">Surveys</span>
            </div>
            <div className="d-flex align-items-center">
              <div
                className="rounded-circle me-2"
                style={{ width: "12px", height: "12px", backgroundColor: "#ffc107" }}
              ></div>
              <span className="small">Data Markets</span>
            </div>
            <div className="d-flex align-items-center">
              <div
                className="rounded-circle me-2"
                style={{ width: "12px", height: "12px", backgroundColor: "#6f42c1" }}
              ></div>
              <span className="small">Template Markets</span>
            </div>
            <div className="d-flex align-items-center">
              <div
                className="rounded-circle me-2"
                style={{ width: "12px", height: "12px", backgroundColor: "#fff3cd" }}
              ></div>
              <span className="small">Subscriptions</span>
            </div>
          </div>
        </Col>
      </Row>
    </div>
  )
}
