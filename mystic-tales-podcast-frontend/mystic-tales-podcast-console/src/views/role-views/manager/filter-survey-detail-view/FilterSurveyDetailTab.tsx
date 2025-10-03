
import { type FC, useContext, useState } from "react"
import { Tabs, Tab } from "react-bootstrap"
import { FilterSurveyDetailViewContext } from "."
import PreviewQuestionsTab from "./components/PreviewQuestionsTab"
import ResponseAnalysisTab from "./components/ResponseAnalysisTab"


const FilterSurveyDetailTabs: FC = () => {
  const [activeTab, setActiveTab] = useState("preview-questions")
  const context = useContext(FilterSurveyDetailViewContext)

  const handleTabChange = (tabKey: string | null) => {
    if (tabKey) {
      setActiveTab(tabKey)
      if (tabKey === "preview-questions") {
        context?.handleDataChange()
      }
    }
  }

  return (
    <div className="filter-survey-detail-tabs">
      <Tabs
        activeKey={activeTab}
        onSelect={handleTabChange}
      >
        <Tab eventKey="preview-questions" title="Preview Questions" className="filter-survey-detail-tabs__content">
        <PreviewQuestionsTab />
        </Tab>

        <Tab eventKey="response-analysis" title="Response Analysis" className="filter-survey-detail-tabs__content">
        <ResponseAnalysisTab  />
        </Tab>
      </Tabs>
    </div>
  )
}

export default FilterSurveyDetailTabs
