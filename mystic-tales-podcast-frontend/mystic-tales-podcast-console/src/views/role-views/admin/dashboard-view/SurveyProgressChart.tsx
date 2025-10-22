import { useEffect, useState } from "react";
import { ProgressBar } from "react-bootstrap"
import { CommunitySurveySummaryCount } from "../../../../core/types/statistics";
import { getSummaryCountCommunitySurvey } from "../../../../core/services/statistic/survey-statistics/survey-statistics.service";
import { adminAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
import SurveyTalkLoading from "../../../components/common/loading";

export function SurveyProgressChart({ activeTab }: { activeTab: string }) {
  const [surveyCount, setSurveyCount] = useState<CommunitySurveySummaryCount | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  useEffect(() => {
    const fetchData = async () => {
      try {
        setIsLoading(true);
        const res = await getSummaryCountCommunitySurvey(adminAxiosInstance, activeTab);
        if (res.success && res.data) {
          setSurveyCount(res.data);
        }
      } catch (err) {
        console.error("Failed to fetch survey data:", err);
      } finally {
        setIsLoading(false);
      }
    };

    fetchData();
  }, [activeTab]);

  if (isLoading || !surveyCount) return <SurveyTalkLoading />;

  const surveyData = [
    { label: "Đã phát hành", value: surveyCount.Published, variant: "primary" },
    { label: "Đang tiến hành", value: surveyCount.OnDeadline, variant: "info" },
    { label: "Gần đến hạn", value: surveyCount.NearDeadline, variant: "warning" },
    { label: "Trễ hạn", value: surveyCount.LateForDeadline, variant: "danger" },
    { label: "Đã hoàn thành", value: surveyCount.Achieved, variant: "success" },
  ];

  const total = surveyData.reduce((sum, item) => sum + item.value, 0);

  return (
    <div className="d-flex flex-column gap-3">
      <div className="position-relative mb-2">
        <ProgressBar
          now={100}
          style={{ height: "24px", borderRadius: "20px" }}
        />
        <div
          className="position-absolute d-flex align-items-center"
          style={{
            right: "10px",
            top: 0,
            bottom: 0,
          }}
        >
          <span className="fw-bold text-white">{total}</span>
        </div>
      </div>

      {surveyData.map((item, index) => (
        <div key={index} className="mb-3">
          <div className="d-flex justify-content-between align-items-center mb-2">
            <span className="text-muted small fw-medium">{item.label}</span>
            <span className="fw-medium small">{item.value}</span>
          </div>
          <ProgressBar
            now={(item.value / total) * 100}
            variant={item.variant}
            style={{ height: "10px" }}
          />
        </div>
      ))}
    </div>
  );
}
