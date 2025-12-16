
import { createContext, FC, useState } from "react"
import { Tabs, Tab, Container, Card, CardHeader, CardContent, Typography } from "@mui/material"
import "./styles.scss"
import { SubscriptionRevenueChart } from "./components/SubscriptionRevenueChart"
import { BookingRevenueChart } from "./components/BookingRevenueChart"
import { CashFlowChart } from "./components/CashFlowChart"
import MetricsCards from "./components/MetricsCards"

interface DashboardViewProps { }
interface DashboardContextProps {
  handleDataChange: () => void
}
export const DashboardContext = createContext<DashboardContextProps | null>(null)
const DashboardView: FC<DashboardViewProps> = () => {
  const [activeTab, setActiveTab] = useState("Daily")

  const handleTabChange = (_: React.SyntheticEvent, value: string) => {
    setActiveTab(value)
  }
  return (
    <Container maxWidth={false} className=" ">
      <div className="dashboard-tabs  my-10">
        <Tabs
          value={activeTab}
          onChange={handleTabChange}
          className="flex-grow-1"
          TabIndicatorProps={{ style: { display: "none" } }}
          aria-label="dashboard period tabs"
        >
          <Tab className="dashboard-tabs__tab" label="Daily" value="Daily" />
          <Tab className="dashboard-tabs__tab" label="Monthly" value="Monthly" />
          <Tab className="dashboard-tabs__tab" label="Yearly" value="Yearly" />
        </Tabs>

      </div>

      {/* Metrics Cards */}
      <div className="mb-8">
        <MetricsCards activeTab={activeTab} />
      </div>
      
      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-3 mb-8">
        <div className="lg:col-span-6 mb-4">
          <Card className="dashboard-card  border-0">
            <CardHeader
              className="border-bottom-0 bg-transparent"
              title={
                <Typography variant="h6" className="mb-1 text-[#aee339] mt-3 dashboard-card__title">
                  Subscription Revenue
                </Typography>
              }
            />
            <CardContent>
              <SubscriptionRevenueChart activeTab={activeTab} />
            </CardContent>
          </Card>
        </div>
        <div className="lg:col-span-6 mb-4">
          <Card className="dashboard-card border-0">
            <CardHeader
              className="border-bottom-0 bg-transparent"
              title={
                <Typography variant="h6" className="mb-1 text-[#aee339] mt-3 dashboard-card__title">
                  Booking Revenue
                </Typography>
              }
            />
            <CardContent>
              {/* Pass empty channelId to avoid fetch until provided */}
              <BookingRevenueChart activeTab={activeTab} />
            </CardContent>
          </Card>
        </div>

      </div>

      {/* Cash Flow Chart */}
      <div className="grid grid-cols-1 gap-3 mb-4">
        <div className="col-span-1">
          <Card className="dashboard-card border-0">
            <CardHeader
              className="border-bottom-0 bg-transparent"
              title={<Typography variant="h6" className="mb-1 text-[#aee339] mt-3 dashboard-card__title">Cash Flow</Typography>}
            />
            <CardContent>
              <CashFlowChart activeTab={activeTab} />
            </CardContent>
          </Card>
        </div>
      </div>


    </Container>
  )
}
export default DashboardView