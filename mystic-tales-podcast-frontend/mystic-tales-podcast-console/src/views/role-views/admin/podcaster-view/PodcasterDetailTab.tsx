import React, { useContext, useEffect, useState } from "react";
import {  Podcaster  } from "../../../../core/types";
import { PodcasterViewContext } from ".";
import { Tab, Tabs } from "react-bootstrap";
import AccountInfomationTab from "./components/AccountInfomationTab";
import PodcasterProfileTab from "./components/PodcasterProfileTab";

interface PodcasterUpdateProps {
  account: Podcaster;
  onClose: () => void;
}

const PodcasterDetailTab: React.FC<PodcasterUpdateProps> = (props) => {
  const [activeTab, setActiveTab] = useState("podcaster-profile");

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
        <Tab eventKey="podcaster-profile" title="Podcaster Profile" className="detail-tabs__content">
          <PodcasterProfileTab account={props.account}   />
        </Tab>
        <Tab eventKey="account-info" title="Account Information" className="detail-tabs__content">
          <AccountInfomationTab {...props} />
        </Tab>

      </Tabs>
    </div>

  );
};

export default PodcasterDetailTab;
