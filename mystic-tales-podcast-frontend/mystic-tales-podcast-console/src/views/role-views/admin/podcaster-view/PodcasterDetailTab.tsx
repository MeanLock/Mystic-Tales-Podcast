import React, { useContext, useEffect, useState } from "react";
import { Account } from "../../../../core/types";
import { PodcasterViewContext } from ".";
import { Tab, Tabs } from "react-bootstrap";
import AccountInfomationTab from "./components/AccountInfomationTab";
import PodcasterProfileTab from "./components/PodcasterProfileTab";

interface PodcasterUpdateProps {
  account: Account;
  onClose: () => void;
}

const PodcasterDetailTab: React.FC<PodcasterUpdateProps> = (props) => {
  const [activeTab, setActiveTab] = useState("account-info");
const [refreshKey, setRefreshKey] = useState(0);

  const handleTabChange = (tabKey: string | null) => {
    if (tabKey) {
      setActiveTab(tabKey)
       if (tabKey === "podcaster-profile") {
      setRefreshKey(prev => prev + 1); 
    }
    }
  }

  return (
    <div className="detail-tabs">
      <Tabs
        id="detail-tabs"
        activeKey={activeTab}
        onSelect={handleTabChange}
        className="detail-tabs__navigation"
      >
        <Tab eventKey="account-info" title="Account Information" className="detail-tabs__content">
          <AccountInfomationTab {...props} />
        </Tab>

        <Tab eventKey="podcaster-profile" title="Podcaster Profile" className="detail-tabs__content">
          <PodcasterProfileTab account={props.account} active={activeTab === "podcaster-profile"} refreshKey={refreshKey} />
        </Tab>
      </Tabs>
    </div>

  );
};

export default PodcasterDetailTab;
