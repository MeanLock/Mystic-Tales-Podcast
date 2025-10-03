"use client"

import { createContext, FC, useState } from "react"
import { Container, Row, Col, Card, Nav, Button, Tabs, Tab } from "react-bootstrap"
import { RevenueChart } from "./RevenueChart"
import { SurveyProgressChart } from "./SurveyProgressChart"
import { CashFlowChart } from "./CashFlowChart"
import { ProfitDistributionCharts } from "./ProfitDistributionChart"
import { Calendar } from "phosphor-react"
import "./styles.scss"
import MetricsCards from "./MetricsCards"

interface DashboardViewProps { }
interface DashboardContextProps {
    handleDataChange: () => void
}
export const DashboardContext = createContext<DashboardContextProps | null>(null)
const DashboardView: FC<DashboardViewProps> = () => {
    const [activeTab, setActiveTab] = useState("Daily")
    // const today = new Date();
    // const todayStr = today.toISOString().slice(0, 10);
    const handleTabChange = (tabKey: string | null) => {
        if (tabKey) {
            setActiveTab(tabKey)
        }
    }
    return (
        <Container fluid className="p-4 dashboard-container">
            <div className="mb-4">
                <div className="dashboard-tabs mb-5">
                    <Tabs
                        activeKey={activeTab}
                        onSelect={handleTabChange}
                        className="flex-grow-1"
                    >
                        <Tab eventKey="Daily" title="Ngày" />
                        <Tab eventKey="Weekly" title="Tuần" />
                        <Tab eventKey="Monthly" title="Tháng" />
                        <Tab eventKey="Yearly" title="Năm" />
                        {/* <Tab eventKey="range" title="Khoảng ngày">
                            <div className="mt-3">
                                <div className="row g-3 align-items-end">
                                    <div className="col-auto">
                                        <label className="form-label mb-1">Từ ngày:</label>
                                        <input
                                            type="date"
                                            className="form-control"
                                            value={startDate}
                                            onChange={(e) => setStartDate(e.target.value)}
                                        />
                                    </div>
                                    <div className="col-auto">
                                        <label className="form-label mb-1">Đến ngày:</label>
                                        <input
                                            type="date"
                                            className="form-control"
                                            value={endDate}
                                            onChange={(e) => setEndDate(e.target.value)}
                                        />
                                    </div>
                                    <div className="col-auto">
                                        <button
                                            onClick={handleDateChange}
                                            className="btn btn-warning fw-medium "
                                        >
                                            Áp dụng
                                        </button>
                                    </div>
                                </div>
                            </div>
                        </Tab> */}
                    </Tabs>

                </div>

                {/* Metrics Cards */}
                 <MetricsCards activeTab={activeTab} />

                {/* Charts Grid */}
                <Row className="mb-4">
                    {/* Revenue Chart */}
                    <Col lg={8} className=" mb-4">
                        <Card className="dashboard-card h-100 border-0 ">
                            <Card.Header className="d-flex justify-content-between align-items-center border-bottom-0 bg-transparent ">
                                <Card.Title className="mb-1 mt-3 dashboard-card__title" >Tổng Doanh Thu</Card.Title>
                            </Card.Header>
                            <Card.Body >
                                <RevenueChart activeTab={activeTab} />
                            </Card.Body>
                        </Card>
                    </Col>

                    {/* Survey Progress Chart */}
                    <Col lg={4} className=" mb-4">
                        <Card className="dashboard-card h-100 border-0 ">
                            <Card.Header className="bg-transparent border-bottom-0">
                                <Card.Title className="mb-1 mt-3 dashboard-card__title">Tổng Số Khảo Sát</Card.Title>
                            </Card.Header>
                            <Card.Body>
                                <SurveyProgressChart activeTab={activeTab} />
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>

                {/* Cash Flow Chart */}
                <Row className="mb-4">
                    <Col xs={12}>
                        <Card className="border-0 shadow-sm">
                            <Card.Header className="bg-white border-bottom">
                                <Card.Title className="mb-0">Dòng Tiền</Card.Title>
                            </Card.Header>
                            <Card.Body>
                                <CashFlowChart activeTab={activeTab} />
                            </Card.Body>
                        </Card>
                    </Col>
                </Row>

                {/* Profit Distribution Charts */}
                {/* {(activeTab === "month" || activeTab === "year") && (
                    <Row>
                        <Col xs={12}>
                            <Card className="border-0 shadow-sm">
                                <Card.Header className="bg-white border-bottom">
                                    <Card.Title className="mb-0">Phân Phối Tỉ Trọng Lợi Nhuận</Card.Title>
                                </Card.Header>
                                <Card.Body>
                                    <ProfitDistributionCharts />
                                </Card.Body>
                            </Card>
                        </Col>
                    </Row>
                 )}  */}
            </div>
        </Container>
    )
}
export default DashboardView