import { type FC, useContext, useState } from "react"
import { Tabs, Tab } from "react-bootstrap"
import { CommunitySurveyDetailViewContext } from "."
import PreviewQuestionsTab from "./components/PreviewQuestionsTab"
import TakenHistoryTab from "./components/TakenHistoryTab"
import RewardTrackingTab from "./components/RewardTrackingTab"

const CommunitySurveyDetailTabs: FC = () => {
  const context = useContext(CommunitySurveyDetailViewContext);
  if (!context) {
    return null;
  }
  const { activeTab, onTabChange } = context;

  const handleTabSelect = (tabKey: string | null) => {
    if (tabKey && tabKey !== activeTab) {
      onTabChange(tabKey);
    }
  };

  return (
    <div className="community-survey-detail-tabs">
      <Tabs
        activeKey={activeTab}
        onSelect={handleTabSelect}
        className="mb-3"
      >
        <Tab eventKey="preview-questions" title="Preview Questions">
          <div className="community-survey-detail-tabs__content">
            {activeTab === "preview-questions" && <PreviewQuestionsTab />}
          </div>
        </Tab>

        <Tab eventKey="taken-history" title="Taken History">
          <div className="community-survey-detail-tabs__content">
            {activeTab === "taken-history" && <TakenHistoryTab />}
          </div>
        </Tab>

        <Tab eventKey="reward-tracking" title="Reward Tracking">
          <div className="community-survey-detail-tabs__content">
            {activeTab === "reward-tracking" && <RewardTrackingTab />}
          </div>
        </Tab>
      </Tabs>
    </div>
  )
}

export default CommunitySurveyDetailTabs