
import { type FC, useContext, useState } from "react"
import { Tabs, Tab } from "react-bootstrap"
import { DataMarketDetailViewContext } from "."
import PaymentHistoryTab from "./components/PaymentHistoryTab"
import VersionManagementTab from "./components/VersionManagementTab"

const DataMarketDetailTabs: FC = () => {
  const [activeTab, setActiveTab] = useState("payment-history")
  const context = useContext(DataMarketDetailViewContext)

  const handleTabChange = (tabKey: string | null) => {
    if (tabKey) {
      setActiveTab(tabKey)
      // Trigger data refresh when switching tabs
      if (context?.handleDataChange) {
        context.handleDataChange()
      }
    }
  }

  return (
    <div className="data-market-tabs">
      <Tabs
        id="data-market-tabs"
        activeKey={activeTab}
        onSelect={handleTabChange}
        className="data-market-tabs__navigation"
      >
        <Tab eventKey="payment-history" title="Quản Lý Lịch Sử Dòng Tiền" className="data-market-tabs__content">
          <PaymentHistoryTab />
        </Tab>

        <Tab eventKey="version-management" title="Quản Lý Version" className="data-market-tabs__content">
          <VersionManagementTab />
        </Tab>
      </Tabs>
    </div>
  )
}

export default DataMarketDetailTabs
