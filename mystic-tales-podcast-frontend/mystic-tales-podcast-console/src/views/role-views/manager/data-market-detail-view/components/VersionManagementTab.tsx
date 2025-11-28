

import { type FC, useContext, useEffect, useState } from "react"
import { Tabs, Tab } from "react-bootstrap"
import { DataMarketDetailViewContext } from ".."

interface SurveyVersion {
  Survey: {
    Id: number
    RequesterId: number
    Title: string
    Description: string
    SurveyTypeId: number
    SurveyTopicId: number
    SurveySpecificTopicId: number
    StartDate: string
    EndDate: string
    Kpi: number
    SecurityModeId: number
    TheoryPrice: number
    ExtraPrice: number
    TakerBaseRewardPrice: number
    ProfitPrice: number
    AllocBaseAmount: number
    AllocTimeAmount: number
    AllocLevelAmount: number
    MaxXp: number
    IsAvailable: boolean
    ConfigJsonString: string
    PublishedAt: string
    DeletedAt: string | null
    CreatedAt: string
    UpdatedAt: string
    SurveyStatusId: number
    MainImageUrl: string
    BackgroundImageUrl: string
    Version: number
    SurveyVersionStatusId: number
    Questions: Array<{
      Id: number
      SurveyId: number
      QuestionTypeId: number
      Content: string
      Description: string
      TimeLimit: number
      IsVoiced: boolean
      Order: number
      ConfigJsonString: string
      DeletedAt: string | null
      Version: number
      MainImageUrl: string
      Options: Array<{
        Id: number
        SurveyQuestionId: string
        Content: string
        Order: number
        MainImageUrl: string
      }>
    }>
  }
}

interface VersionStats {
  questionCount: number
  currentPrice: string
  responsePrice: string
  contributorCount: number
  purchaseCount: number
  publishDate: string
  contributionData: Array<{ day: string; value: number }>
  purchaseData: Array<{ day: string; value: number }>
  responseAnalysis: Array<{
    question: string
    responses: Array<{ label: string; count: number; percentage: number }>
  }>
}

const VersionManagementTab: FC = () => {
  const [activeVersionTab, setActiveVersionTab] = useState("version-1")
  const [versions, setVersions] = useState<SurveyVersion[]>([])
  const [versionStats, setVersionStats] = useState<{ [key: string]: VersionStats }>({})
  const [loading, setLoading] = useState(false)
  const context = useContext(DataMarketDetailViewContext)

  // Mock survey versions data
  const mockVersions: SurveyVersion[] = [
    {
      Survey: {
        Id: 1,
        RequesterId: 1,
        Title: "Sức Khỏe Cơ Bản v1.0",
        Description: "Khảo sát về sức khỏe cơ bản của người dân",
        SurveyTypeId: 1,
        SurveyTopicId: 1,
        SurveySpecificTopicId: 1,
        StartDate: "2024-05-20",
        EndDate: "2024-12-31",
        Kpi: 1000,
        SecurityModeId: 1,
        TheoryPrice: 10000000,
        ExtraPrice: 0,
        TakerBaseRewardPrice: 10000,
        ProfitPrice: 5000000,
        AllocBaseAmount: 1000000,
        AllocTimeAmount: 500000,
        AllocLevelAmount: 300000,
        MaxXp: 100,
        IsAvailable: true,
        ConfigJsonString: "{}",
        PublishedAt: "2024-05-20T00:00:00.000Z",
        DeletedAt: null,
        CreatedAt: "2024-05-20T00:00:00.000Z",
        UpdatedAt: "2024-05-20T00:00:00.000Z",
        SurveyStatusId: 1,
        MainImageUrl: "https://example.com/image1.jpg",
        BackgroundImageUrl: "https://example.com/bg1.jpg",
        Version: 1,
        SurveyVersionStatusId: 1,
        Questions: [
          {
            Id: 1,
            SurveyId: 1,
            QuestionTypeId: 1,
            Content: "Bạn thường tập thể dục bao nhiêu lần 1 tuần?",
            Description: "Câu hỏi về tần suất tập thể dục",
            TimeLimit: 30,
            IsVoiced: false,
            Order: 1,
            ConfigJsonString: "{}",
            DeletedAt: null,
            Version: 1,
            MainImageUrl: "",
            Options: [
              { Id: 1, SurveyQuestionId: "1", Content: "Tôi không tập thể dục", Order: 1, MainImageUrl: "" },
              { Id: 2, SurveyQuestionId: "1", Content: "1 - 2 lần / tuần", Order: 2, MainImageUrl: "" },
              { Id: 3, SurveyQuestionId: "1" , Content: "3 - 4 lần / tuần", Order: 3, MainImageUrl: "" },
              { Id: 4, SurveyQuestionId: "1" , Content: "5 - 6 lần / tuần", Order: 4, MainImageUrl: "" },
              { Id: 5, SurveyQuestionId: "1" , Content: "Mỗi ngày", Order: 5, MainImageUrl: "" },
            ],
          },
          {
            Id: 2,
            SurveyId: 1,
            QuestionTypeId: 2,
            Content: "Bạn thích tập ở ",
            Description: "Câu hỏi về địa điểm tập thể dục",
            TimeLimit: 30,
            IsVoiced: false,
            Order: 2,
            ConfigJsonString: "{}",
            DeletedAt: null,
            Version: 1,
            MainImageUrl: "",
            Options: [
              {
                Id: 6,
              SurveyQuestionId: "2",
                Content: "Phòng tập hiện đại",
                Order: 1,
                MainImageUrl: "https://app.gak.vn/storage/uploads/2hgISshQgnlJ2WRSJ5tm07Tp89gptJsMoX1mAjMq.jpg",
              },
              {
                Id: 7,
              SurveyQuestionId: "2",
                Content: "Phòng tập sang trọng",
                Order: 2,
                MainImageUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRXZNNjMQ4oPHiE9xvoWUheBG2bOAqTP3W_Pg&s",
              },
            ],
          },
        ],
      },
    },
    {
      Survey: {
        Id: 2,
        RequesterId: 1,
        Title: "Sức Khỏe Cơ Bản v2.0",
        Description: "Phiên bản cập nhật của khảo sát sức khỏe",
        SurveyTypeId: 1,
        SurveyTopicId: 1,
        SurveySpecificTopicId: 1,
        StartDate: "2024-06-01",
        EndDate: "2024-12-31",
        Kpi: 1500,
        SecurityModeId: 1,
        TheoryPrice: 12000000,
        ExtraPrice: 0,
        TakerBaseRewardPrice: 12000,
        ProfitPrice: 6000000,
        AllocBaseAmount: 1200000,
        AllocTimeAmount: 600000,
        AllocLevelAmount: 400000,
        MaxXp: 120,
        IsAvailable: true,
        ConfigJsonString: "{}",
        PublishedAt: "2024-06-01T00:00:00.000Z",
        DeletedAt: null,
        CreatedAt: "2024-06-01T00:00:00.000Z",
        UpdatedAt: "2024-06-01T00:00:00.000Z",
        SurveyStatusId: 1,
        MainImageUrl: "https://example.com/image2.jpg",
        BackgroundImageUrl: "https://example.com/bg2.jpg",
        Version: 2,
        SurveyVersionStatusId: 1,
        Questions: [
          {
            Id: 3,
            SurveyId: 2,
            QuestionTypeId: 1,
            Content: "Bạn thường tập thể dục bao nhiêu lần 1 tuần?",
            Description: "Câu hỏi về tần suất tập thể dục - phiên bản 2",
            TimeLimit: 30,
            IsVoiced: false,
            Order: 1,
            ConfigJsonString: "{}",
            DeletedAt: null,
            Version: 2,
            MainImageUrl: "",
            Options: [
              { Id: 8, SurveyQuestionId:"3", Content: "Tôi không tập thể dục", Order: 1, MainImageUrl: "" },
              { Id: 9, SurveyQuestionId:"3", Content: "1 - 2 lần / tuần", Order: 2, MainImageUrl: "" },
              { Id: 10, SurveyQuestionId:"3", Content: "3 - 4 lần / tuần", Order: 3, MainImageUrl: "" },
              { Id: 11, SurveyQuestionId:"3", Content: "5 - 6 lần / tuần", Order: 4, MainImageUrl: "" },
              { Id: 12, SurveyQuestionId:"3", Content: "Mỗi ngày", Order: 5, MainImageUrl: "" },
            ],
          },
        ],
      },
    },

    {
      Survey: {
        Id: 3,
        RequesterId: 1,
        Title: "Sức Khỏe Cơ Bản v3.0",
        Description: "Phiên bản mới nhất của khảo sát sức khỏe",
        SurveyTypeId: 1,
        SurveyTopicId: 1,
        SurveySpecificTopicId: 1,
        StartDate: "2024-07-01",
        EndDate: "2024-12-31",
        Kpi: 2000,
        SecurityModeId: 1,
        TheoryPrice: 15000000,
        ExtraPrice: 0,
        TakerBaseRewardPrice: 15000,
        ProfitPrice: 7500000,
        AllocBaseAmount: 1500000,
        AllocTimeAmount: 750000,
        AllocLevelAmount: 500000,
        MaxXp: 150,
        IsAvailable: true,
        ConfigJsonString: "{}",
        PublishedAt: "2024-07-01T00:00:00.000Z",
        DeletedAt: null,
        CreatedAt: "2024-07-01T00:00:00.000Z",
        UpdatedAt: "2024-07-01T00:00:00.000Z",
        SurveyStatusId: 1,
        MainImageUrl: "https://app.gak.vn/storage/uploads/2hgISshQgnlJ2WRSJ5tm07Tp89gptJsMoX1mAjMq.jpg",
        BackgroundImageUrl: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRXZNNjMQ4oPHiE9xvoWUheBG2bOAqTP3W_Pg&s",
        Version: 3,
        SurveyVersionStatusId: 1,
        Questions: [],
      },
    },
  ]

  // Mock stats data for each version
  const mockVersionStats: { [key: string]: VersionStats } = {
    "version-1": {
      questionCount: 5,
      currentPrice: "10,000,000đ",
      responsePrice: "10,000đ",
      contributorCount: 1000,
      purchaseCount: 350,
      publishDate: "20/05/2024",
      contributionData: [
        { day: "Thứ 2", value: 20 },
        { day: "Thứ 3", value: 45 },
        { day: "Thứ 4", value: 30 },
        { day: "Thứ 5", value: 85 },
        { day: "Thứ 6", value: 60 },
        { day: "Thứ 7", value: 25 },
        { day: "Chủ Nhật", value: 15 },
      ],
      purchaseData: [
        { day: "Thứ 2", value: 75 },
        { day: "Thứ 3", value: 55 },
        { day: "Thứ 4", value: 40 },
        { day: "Thứ 5", value: 45 },
        { day: "Thứ 6", value: 30 },
        { day: "Thứ 7", value: 20 },
        { day: "Chủ Nhật", value: 10 },
      ],
      responseAnalysis: [
        {
          question: "Câu 1: Bạn thường tập thể dục bao nhiêu lần 1 tuần?",
          responses: [
            { label: "Tôi không tập thể dục", count: 200, percentage: 20 },
            { label: "1 - 2 lần / tuần", count: 200, percentage: 20 },
            { label: "3 - 4 lần / tuần", count: 200, percentage: 20 },
            { label: "5 - 6 lần / tuần", count: 200, percentage: 20 },
            { label: "Mỗi ngày", count: 200, percentage: 20 },
          ],
        },
        {
          question: "Câu 2: Bạn thích tập ở đâu?",
          responses: [
            { label: "Phòng tập hiện đại", count: 200, percentage: 50 },
            { label: "Phòng tập sang trọng", count: 200, percentage: 50 },
          ],
        },
      ],
    },
    "version-2": {
      questionCount: 8,
      currentPrice: "12,000,000đ",
      responsePrice: "12,000đ",
      contributorCount: 1200,
      purchaseCount: 420,
      publishDate: "01/06/2024",
      contributionData: [
        { day: "Thứ 2", value: 30 },
        { day: "Thứ 3", value: 55 },
        { day: "Thứ 4", value: 40 },
        { day: "Thứ 5", value: 90 },
        { day: "Thứ 6", value: 70 },
        { day: "Thứ 7", value: 35 },
        { day: "Chủ Nhật", value: 25 },
      ],
      purchaseData: [
        { day: "Thứ 2", value: 85 },
        { day: "Thứ 3", value: 65 },
        { day: "Thứ 4", value: 50 },
        { day: "Thứ 5", value: 55 },
        { day: "Thứ 6", value: 40 },
        { day: "Thứ 7", value: 30 },
        { day: "Chủ Nhật", value: 20 },
      ],
      responseAnalysis: [
        {
          question: "Câu 1: Bạn thường tập thể dục bao nhiêu lần 1 tuần?",
          responses: [
            { label: "Tôi không tập thể dục", count: 240, percentage: 20 },
            { label: "1 - 2 lần / tuần", count: 240, percentage: 20 },
            { label: "3 - 4 lần / tuần", count: 240, percentage: 20 },
            { label: "5 - 6 lần / tuần", count: 240, percentage: 20 },
            { label: "Mỗi ngày", count: 240, percentage: 20 },
          ],
        },
      ],
    },
    "version-3": {
      questionCount: 12,
      currentPrice: "15,000,000đ",
      responsePrice: "15,000đ",
      contributorCount: 1500,
      purchaseCount: 580,
      publishDate: "01/07/2024",
      contributionData: [
        { day: "Thứ 2", value: 40 },
        { day: "Thứ 3", value: 65 },
        { day: "Thứ 4", value: 50 },
        { day: "Thứ 5", value: 95 },
        { day: "Thứ 6", value: 80 },
        { day: "Thứ 7", value: 45 },
        { day: "Chủ Nhật", value: 35 },
      ],
      purchaseData: [
        { day: "Thứ 2", value: 95 },
        { day: "Thứ 3", value: 75 },
        { day: "Thứ 4", value: 60 },
        { day: "Thứ 5", value: 65 },
        { day: "Thứ 6", value: 50 },
        { day: "Thứ 7", value: 40 },
        { day: "Chủ Nhật", value: 30 },
      ],
      responseAnalysis: [
        {
          question: "Câu 1: Bạn thường tập thể dục bao nhiêu lần 1 tuần?",
          responses: [
            { label: "Tôi không tập thể dục", count: 300, percentage: 20 },
            { label: "1 - 2 lần / tuần", count: 300, percentage: 20 },
            { label: "3 - 4 lần / tuần", count: 300, percentage: 20 },
            { label: "5 - 6 lần / tuần", count: 300, percentage: 20 },
            { label: "Mỗi ngày", count: 300, percentage: 20 },
          ],
        },
      ],
    },
  }

  const loadVersionData = async () => {
    setLoading(true)
    try {
      // Simulate API call
      await new Promise((resolve) => setTimeout(resolve, 500))
      setVersions(mockVersions)
      setVersionStats(mockVersionStats)
    } catch (error) {
      console.error("Error loading version data:", error)
    } finally {
      setLoading(false)
    }
  }

  const handleVersionTabChange = (tabKey: string | null) => {
    if (tabKey) {
      setActiveVersionTab(tabKey)
    }
  }

  const handleCreateVersion = () => {
    //console.log("Creating new version...")
  }

  const handleStopVersion = (versionId: string) => {
    //console.log("Stopping version:", versionId)
  }

  useEffect(() => {
    loadVersionData()
  }, [])

  const currentStats = versionStats[activeVersionTab]
  const maxContributionValue = currentStats ? Math.max(...currentStats.contributionData.map((d) => d.value)) : 100
  const maxPurchaseValue = currentStats ? Math.max(...currentStats.purchaseData.map((d) => d.value)) : 100

  if (loading) {
    return (
      <div className="version-management-tab__loading">
        <div className="spinner"></div>
        <span>Đang tải dữ liệu version...</span>
      </div>
    )
  }


  return (
    <div className="version-tabs">
      <div className="version-tabs__nav d-flex gap-3 flex-wrap mb-4 ">
        {versions.map((version, index) => (
          <button
            key={`version-${version.Survey.Version}`}
            className={`nav-link ${activeVersionTab === `version-${version.Survey.Version}` ? 'active' : ''}`}
            onClick={() => handleVersionTabChange(`version-${version.Survey.Version}`)}
          >
            {`Version ${version.Survey.Version}`}
          </button>
        ))}
        <button
          className="version-tabs__create-version"
          onClick={handleCreateVersion}
        >
          + Tạo Version
        </button>
      </div>
      {currentStats && (
        <div className="p-5 bg-white ">
          {/* Stats Grid */}
          <div className="d-flex gap-5 align-items-start mb-5">
            <div className="version-stats-grid">
              <div className="version-stat-card version-stat-card--blue">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Số Câu Hỏi</div>
                  <div className="version-stat-card__value">{currentStats.questionCount}</div>
                </div>
                <div className="version-stat-card__icon">❓</div>
              </div>

              <div className="version-stat-card version-stat-card--yellow">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Giá Hiện Tại</div>
                  <div className="version-stat-card__value">{currentStats.currentPrice}</div>
                </div>
                <div className="version-stat-card__icon">💰</div>
              </div>

              <div className="version-stat-card version-stat-card--gray">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Giá 1 Response</div>
                  <div className="version-stat-card__value">{currentStats.responsePrice}</div>
                </div>
                <div className="version-stat-card__icon">💵</div>
              </div>

              <div className="version-stat-card version-stat-card--pink">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Số lượng Contributor</div>
                  <div className="version-stat-card__value">{currentStats.contributorCount.toLocaleString()}</div>
                </div>
                <div className="version-stat-card__icon">👥</div>
              </div>

              <div className="version-stat-card version-stat-card--purple">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Số lượt mua</div>
                  <div className="version-stat-card__value">{currentStats.purchaseCount}</div>
                </div>
                <div className="version-stat-card__icon">🛒</div>
              </div>

              <div className="version-stat-card version-stat-card--green">
                <div className="version-stat-card__content">
                  <div className="version-stat-card__label">Ngày đăng</div>
                  <div className="version-stat-card__value">{currentStats.publishDate}</div>
                </div>
                <div className="version-stat-card__icon">📅</div>
              </div>
            </div>
            <button className="version-stop-btn" onClick={() => handleStopVersion(activeVersionTab)}>
              Dừng Version
            </button>
          </div>
          {/* Charts Section */}
          <div className="version-charts-section">
            <div className="version-chart-container">
              <h4 className="version-chart-title">Đóng Góp Phần Tích</h4>
              <div className="version-chart">
                {currentStats.contributionData.map((data, index) => (
                  <div key={index} className="version-chart__bar-group">
                    <div
                      className="version-chart__bar version-chart__bar--blue"
                      style={{ height: `${(data.value / maxContributionValue) * 100}%` }}
                    ></div>
                    <span className="version-chart__label">{data.day}</span>
                  </div>
                ))}
              </div>
            </div>

            <div className="version-chart-container">
              <h4 className="version-chart-title">Mua Phân Tích</h4>
              <div className="version-chart">
                {currentStats.purchaseData.map((data, index) => (
                  <div key={index} className="version-chart__bar-group">
                    <div
                      className="version-chart__bar version-chart__bar--yellow"
                      style={{ height: `${(data.value / maxPurchaseValue) * 100}%` }}
                    ></div>
                    <span className="version-chart__label">{data.day}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Responses Analysis */}
          <div className="version-responses-section mt-5 mb-5">
            <h3 className="version-responses-title">Responses Analysis</h3>

            {currentStats.responseAnalysis.map((analysis, questionIndex) => (
              <div key={questionIndex} className="version-question-analysis m-5">
                <h4 className="version-question-title">{analysis.question}</h4>

                <div className="version-responses-list">
                  {analysis.responses.map((response, responseIndex) => (
                    <div key={responseIndex} className="version-response-item">
                      <div className="version-response-content">
                        {response.label.includes("Phòng tập") && (
                          <img
                            src="https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRXZNNjMQ4oPHiE9xvoWUheBG2bOAqTP3W_Pg&s"
                            alt={response.label}
                            className="version-response-image"
                          />
                        )}
                        <span className="version-response-label">{response.label}</span>
                      </div>
                      <div className="version-response-bar">
                        <div
                          className="version-response-bar-fill"
                          style={{ width: `${response.percentage}%` }}
                        ></div>
                      </div>
                      <span className="version-response-count">{response.count}/1000</span>
                    </div>
                  ))}
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

    </div>
  )
}

export default VersionManagementTab
