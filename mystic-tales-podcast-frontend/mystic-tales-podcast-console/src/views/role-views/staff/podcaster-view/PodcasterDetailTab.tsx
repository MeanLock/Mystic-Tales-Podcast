import React, { useContext, useEffect, useState } from "react";
import { Account, Podcaster } from "../../../../core/types";
import { PodcasterViewContext } from ".";
import { Tab, Tabs } from "react-bootstrap";
import AccountInfomationTab from "./components/AccountInfomationTab";
import PodcasterProfileTab from "./components/PodcasterProfileTab";

interface PodcasterUpdateProps {
  podcaster: Podcaster;
  onClose: () => void;
}

const PodcasterDetailTab: React.FC<PodcasterUpdateProps> = (props) => {
  const [activeTab, setActiveTab] = useState("account-info");

  const handleTabChange = (tabKey: string | null) => {
    if (tabKey) {
      setActiveTab(tabKey)
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
          <AccountInfomationTab account={props.podcaster.Account} />
        </Tab>

        <Tab eventKey="podcaster-profile" title="Podcaster Profile" className="detail-tabs__content">
          <PodcasterProfileTab podcasterPf={props.podcaster.PodcasterProfile} onClose={props.onClose}  />
        </Tab>
      </Tabs>
    </div>

  );
};

export default PodcasterDetailTab;
