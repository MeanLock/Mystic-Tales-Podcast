import NavigateNextIcon from "@mui/icons-material/NavigateNext";
import { Button, Tab } from "@mui/material";
import axios, { isAxiosError } from "axios";
import isEqual from "lodash/isEqual";
import { useEffect, useMemo, useRef, useState } from "react";
import { useLocation, useParams } from "react-router-dom";
import type { SweetAlertResult } from "sweetalert2";
import Swal from "sweetalert2";
import { HEADER_HEIGHT } from "../../constants/index";
import useBlocker from "@/hooks/useBlocker";
import type { SurveyType } from "@/core/types/tools";
import CompletePage from "../../components/organisms/CompletePage/CompletePage";
import EndPage from "../../components/organisms/EndPage/EndPage";
import OverlayDisable from "../../components/organisms/overlay/Overlay";
import QuestionPage from "../../components/organisms/QuestionPage/QuestionPage";
import ReportPage from "../../components/organisms/ReportPage/ReportPage";
import SharePage from "../../components/organisms/SharePage/SharePage";
import StartPage from "../../components/organisms/StartPage/StartPage";
import MainTemPlate from "../../components/templates/MainTemplate";
import "./styles.scss";

import { toast } from "react-toastify";

import { useGetSurveyDefaultBackgroundThemes } from "@/core/services/CreateSurveyTool/InputDatas/get-default-background-themes";
import { useGetSurveySecurityModes } from "@/core/services/CreateSurveyTool/InputDatas/get-security-modes";
import { useGetSurveyFieldInputTypes } from "@/core/services/CreateSurveyTool/InputDatas/get-field-input-types";
import { useGetSurveyTopics } from "@/core/services/CreateSurveyTool/InputDatas/get-topics";
import { useGetSurveySpecificTopics } from "@/core/services/CreateSurveyTool/InputDatas/get-specific-topics";
import { useGetSurveyQuestionTypes } from "@/core/services/CreateSurveyTool/InputDatas/get-survey-question-types";
import { useUpdateManualSurvey } from "@/core/services/CreateSurveyTool/EditSession/update-survey-manual";
import { useUpdateSurvey } from "@/core/services/CreateSurveyTool/EditSession/update-survey";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { managerAxiosInstance } from "@/core/api/rest-api/config/instances/v2/manager-axios-instance";

const defaultValue = {
  Id: 999,
  RequesterId: 10,
  Title: "",
  Description: "",
  MarketSurveyVersionStatusId: 2, // SurveyStatusId: 3
  SurveyTypeId: 2,
  SurveyTopicId: 2,
  SurveySpecificTopicId: 5,
  SurveyStatusId: 1, //
  SecurityModeId: 1,
  BackgroundImageBase64: "",
  IsSuccess: true,
  ConfigJson: {
    Background: "image",
    IsUseBackgroundImageBase64: false,
    // IsPause: false,
    BackgroundGradient1Color: "#ffffff",
    BackgroundGradient2Color: "#f0f0f0",
    TitleColor: "#000000",
    ContentColor: "#333333",
    ButtonBackgroundColor: "#007bff",
    ButtonContentColor: "#ffffff",
    Password: "123456",
    Brightness: 100,
    DefaultBackgroundImageId: 1,
    SkipStartPage: false,
  },
  Questions: [],
};

const SurveyNew = () => {
  const { id } = useParams();
  const location = useLocation();

  // Biến check để disable khi chưa load data xong
  const [isLoading, setIsLoading] = useState(true);
  const [activeTab, setActiveTab] = useState(0);
  const [formData, setFormData] = useState<SurveyType>(defaultValue);
  const [isSaving, setIsSaving] = useState(false);
  const [saveCountdown, setSaveCountdown] = useState(0);
  const [hasChanges, setHasChanges] = useState(false);
  const [isDisable, setIsDisable] = useState(false);
  const [isSuccess, setIsSucces] = useState(true);
  const latestDataRef = useRef(formData);
  const timeoutRef = useRef<number | null>(null);
  const countdownRef = useRef<number | null>(null);
  const [isApiCalling, setIsApiCalling] = useState(false); //Thinh thêm biến này để check api đang gọi hay không

  const [surveyId, setSurveyId] = useState<number | any>(0); // Biến lưu id của khảo sát

  const isTrigger = useMemo(() => formData?.SurveyStatusId !== 2, [formData]);

  // const { data } = useGetSurvey({ id: Number(id) || 0 });

  // DATA LẤY RA TỪ KHI MỚI LOAD
  // 1. Default Background Themes
  const { data: defaultBgThemes, isLoading: isLoadingDefaultBgThemes } =
    useGetSurveyDefaultBackgroundThemes({});
  // 2. Security Modes
  const { data: securityModes, isLoading: isLoadingSecurityModes } =
    useGetSurveySecurityModes({});
  // 3. Question Types
  const { data: questionTypes, isLoading: isLoadingQuestionTypes } =
    useGetSurveyQuestionTypes({});
  // 4. Field Input Types
  const { data: filedInputTypes, isLoading: isLoadingFieldInputTypes } =
    useGetSurveyFieldInputTypes({});
  // 5. Topics
  const { data: topics, isLoading: isLoadingTopics } = useGetSurveyTopics({});
  // 6. Specific Topics
  const { data: specificTopics, isLoading: isLoadingSpecificTopics } =
    useGetSurveySpecificTopics({});

  const handleTabClick = (tabValue: number) => {
    setActiveTab(tabValue);
  };

  const fetchSurveyData = (): Promise<SurveyType> => {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve(defaultValue as any);
      }, 500);
    });
  };

  const tabs = [
    {
      label: "Trang Bắt Đầu",
      value: 0,
      component: (
        <StartPage
          formData={formData}
          setFormData={setFormData}
          handleTabClick={handleTabClick}
          isDisable={isDisable}
          // Bỏ các input data vào
          defaultBackgroundThemes={defaultBgThemes}
          securityModes={securityModes}
          topics={topics}
          specificTopics={specificTopics}
        />
      ),
    },
    {
      label: "Bảng Hỏi",
      value: 1,
      component: (
        <QuestionPage
          formData={formData}
          setFormData={setFormData}
          isTrigger={isTrigger} // Thịnh, thêm
          // Bỏ các input data vào
          securityModes={securityModes} //Thịnh thêm dòng này
          defaultBackgroundThemes={defaultBgThemes} //Thịnh thêm dòng này
          SurveyQuestionTypes={questionTypes}
          fieldInputTypes={filedInputTypes}
        />
      ),
    },
    {
      label: "Trang Kết Thúc",
      value: 2,
      component: (
        <EndPage
          formData={formData}
          defaultBackgroundThemes={defaultBgThemes} //Thịnh thêm dòng này
        />
      ),
    },
    {
      label: "Hoàn Tất",
      value: 3,
      component: (
        <CompletePage
          formData={formData}
          defaultBackgroundThemes={defaultBgThemes} //Thịnh thêm dòng này
        />
      ),
    },
    {
      label: "Chia Sẻ",
      value: 4,
      component: (
        <SharePage
          formData={formData}
          defaultBackgroundThemes={defaultBgThemes} //Thịnh thêm dòng này
        />
      ),
    },
    {
      label: "Báo cáo",
      value: 5,
      component: <ReportPage />,
      disabled: true,
    },
  ];

  const ActiveComponent = tabs[activeTab].component;

  const { mutate: mutateAutoSave } = useUpdateSurvey({
    mutationConfig: {
      onSuccess({ responseData, sentBody }) {
        setIsApiCalling(false); //Thịnh thêm biến này để check api đang gọi hay không
        //setFormData(sentBody);
        latestDataRef.current = sentBody;
        //console.log("sentBody: ", sentBody);
        setIsSucces(responseData?.IsSuccess);
           const currentPath = location.pathname;
        //console.log("currentPath: ", currentPath);

        if (!id && currentPath.endsWith('/survey/new')) {
          window.history.pushState({}, "", `#/survey/${sentBody.Id}/editing`);
        }
      },
      onError(error) {
        setIsApiCalling(false); //Thịnh thêm biến này để check api đang gọi hay không
        if (isAxiosError(error) && error.response?.status === 403) {
          setIsDisable(true);
        }
      },
    },
  });

  // Thịnh, tách ra thêm 1 hàm cái lưu thủ công
  const { mutate: mutateManualSave } = useUpdateManualSurvey({
    mutationConfig: {
      onSuccess({ responseData, sentBody }) {
        setIsApiCalling(false);
        latestDataRef.current = sentBody;
        setIsSucces(responseData?.IsSuccess);
        const currentPath = location.pathname;
        //console.log("currentPath: ", currentPath);
        if (!id && currentPath.endsWith('/survey/new')) {
          window.history.pushState({}, "", `#/survey/${sentBody.Id}/editing`);
        }
      },
      onError(error: any) {
        setIsApiCalling(false);
        if (isAxiosError(error) && error.response?.status === 403) {
          setIsDisable(true);
        }
      },
    },
  });

  // Hàm tạo survey mới
  //   const { mutate: createSurvey } = useCreateSurvey({
  //     mutationConfig: {
  //       onSuccess: (data) => {
  //         const newId = data?.NewSurveyId;
  //         if (newId) {
  //           setSurveyId(newId);
  //         } else {
  //           toast.error("Không tạo được ID khảo sát mới.");
  //         }
  //       },
  //       onError: () => {
  //         toast.error("Tạo khảo sát mới thất bại.");
  //         setIsLoading(false);
  //       },
  //     },
  //   });

  // Lấy Edit Session
  //   const {
  //     data: editSessionData,
  //     isSuccess: isGetSuccess,
  //     isFetching: isGetFetching,
  //   } = useGetEditSession({
  //     id: surveyId || Number(location.pathname.split("/")[2]),
  //     queryConfig: {
  //       enabled: !!surveyId || location.pathname !== "/survey/new",
  //       onSuccess: (res: any) => {
  //         let initialData = res.EditingSession;

  //         if (!initialData) {
  //           fetchSurveyData().then((fallback) => setFormData(fallback));
  //           return;
  //         }

  //         // Bổ sung default config nếu thiếu
  //         if (!initialData.ConfigJson) {
  //           (initialData as any).ConfigJson = {
  //             BackgroundGradient1Color: "#FCE38A",
  //             BackgroundGradient2Color: "#F38181",
  //             TitleColor: "#FFFFFF",
  //             ContentColor: "#CCCCCC",
  //             ButtonBackgroundColor: "#007bff",
  //             ButtonContentColor: "#ffffff",
  //             Password: "",
  //             Brightness: 100,
  //           };
  //         }

  //         if (initialData.ConfigJson?.Brightness === undefined) {
  //           initialData.ConfigJson.Brightness = 100;
  //         }
  //         if (initialData.ConfigJson?.SkipStartPage === undefined) {
  //           initialData.ConfigJson.SkipStartPage = false;
  //         }
  //         if (initialData.SurveyStatusId === undefined) {
  //           initialData.SurveyStatusId = 1;
  //         }
  //         if (initialData.SecurityModeId === undefined) {
  //           initialData.SecurityModeId = 1;
  //         }

  //         setFormData(initialData);
  //         setIsLoading(false);
  //       },
  //       onError: () => {
  //         toast.error("Lỗi khi lấy dữ liệu khảo sát.");
  //         setIsLoading(false);
  //       },
  //     },
  //   });

  const handleConfirm = () => {
    if (isDisable) return;
    Swal.fire({
      title: "Bạn muốn lưu các thay đổi?",
      showCancelButton: true,
      confirmButtonText: "Save",
    }).then((result: SweetAlertResult) => {
      if (result.isConfirmed) {
        setIsApiCalling(true); //Thịnh thêm biến này để check api đang gọi hay không
        mutateManualSave({
          SurveyId: formData.Id,
          ...formData,
        });
      }
    });
  };

  const handleSave = () => {
    if (isDisable) return;
    setIsSaving(true);
    setHasChanges(false);
    let seconds = 5;
    setSaveCountdown(seconds);

    countdownRef.current = setInterval(() => {
      seconds--;
      setSaveCountdown(seconds);
      if (seconds <= 0 && countdownRef.current) {
        clearInterval(countdownRef.current);
      }
    }, 1000) as unknown as number;

    timeoutRef.current = setTimeout(() => {
      mutateAutoSave({
        SurveyId: latestDataRef.current.Id,
        ...latestDataRef.current,
      });
      setIsSaving(false);
      timeoutRef.current = null;
      countdownRef.current = null;
    }, 5000) as unknown as number;
  };

  // HOOKS

  useEffect(() => {
    setIsLoading(true);

    const loadInitialData = async () => {
      let initialData: SurveyType;
      //const savedFormData = localStorage.getItem("surveyFormData");
      //console.log("Location: ", location.pathname);
      if (location.pathname === "/survey/new") {
        //console.log("Rơi vào New");
        // Gọi API để lấy ID của khảo sát mới
        const newSurvey = await callAxiosRestApi({
          instance: managerAxiosInstance,
          method: "post",
          url: "Survey/core/filter/surveys",
        });
        if (newSurvey) {
          //console.log("Data nè: ", newSurvey);
          const NewId = newSurvey.data.NewSurveyId;
          if (NewId) {
            // Lấy Edit Session từ ID vừa tạo
            // const response = await axiosCustom.get(
            //   `Survey/session/surveys/${NewId}/editing-session`
            // );
            const response = await callAxiosRestApi({
              instance: loginRequiredAxiosInstance,
              method: "get",
              url: `Survey/session/surveys/${NewId}/editing-session`,
            });

            if (response) {
              //console.log("responsehihi", response);
              const savedFormData = response.data.EditingSession;
              if (savedFormData) {
                initialData = savedFormData;
                //  initialData = JSON.parse(savedFormData);
                if (!initialData.ConfigJson) {
                  (initialData as any).ConfigJson = {
                    BackgroundGradient1Color: "#FCE38A",
                    BackgroundGradient2Color: "#F38181",
                    TitleColor: "#FFFFFF",
                    ContentColor: "#CCCCCC",
                    ButtonBackgroundColor: "#007bff",
                    ButtonContentColor: "#ffffff",
                    Password: "",
                    Brightness: 100,
                  };
                }
                if (initialData.ConfigJson?.Brightness === undefined) {
                  initialData.ConfigJson.Brightness = 100; // Default Brightness
                }
                if (initialData.ConfigJson.SkipStartPage === undefined) {
                  initialData.ConfigJson.SkipStartPage = false;
                }
                if (initialData.SurveyStatusId === undefined) {
                  initialData.SurveyStatusId = 1; // Default to active
                }
                if (initialData.SecurityModeId === undefined) {
                  initialData.SecurityModeId = 1; // Default to no Password protection
                }
              } else {
                initialData = await fetchSurveyData();
              }
              setFormData(initialData);
              setIsLoading(false);
            }
          } else {
            toast.error("Không có ID");
          }
        }
      } else {
        const id = Number(location.pathname.split("/")[2]); //Thịnh, đổi cách lấy id do thay đường dẫn
        //console.log("Rơi vào Update", id);
        // Gọi API để lấy edit session từ id có sẵn
        const response = await callAxiosRestApi({
          instance: loginRequiredAxiosInstance,
          method: "get",
          url: `Survey/session/surveys/${id}/editing-session`,
        });

        //console.log("responsehihi update", response);
        const savedFormData = response.data.EditingSession;
        if (savedFormData) {
          initialData = savedFormData;
          if (!initialData.ConfigJson) {
            (initialData as any).ConfigJson = {
              BackgroundGradient1Color: "#FCE38A",
              BackgroundGradient2Color: "#F38181",
              TitleColor: "#FFFFFF",
              ContentColor: "#CCCCCC",
              ButtonBackgroundColor: "#007bff",
              ButtonContentColor: "#ffffff",
              Password: "",
              Brightness: 100,
            };
          }
          if (initialData.ConfigJson?.Brightness === undefined) {
            initialData.ConfigJson.Brightness = 100; // Default Brightness
          }
          if (initialData.ConfigJson.SkipStartPage === undefined) {
            initialData.ConfigJson.SkipStartPage = false;
          }
          if (initialData.SurveyStatusId === undefined) {
            initialData.SurveyStatusId = 1; // Default to active
          }
          if (initialData.SecurityModeId === undefined) {
            initialData.SecurityModeId = 1; // Default to no Password protection
          }
        } else {
          initialData = await fetchSurveyData();
        }
        setFormData(initialData);
        setIsLoading(false);
      }
    };
    loadInitialData();
  }, []);

  //   useEffect(() => {
  //     if (location.pathname === "/survey/new") {
  //       createSurvey(); // Gọi mutation để tạo survey → sẽ setSurveyId() trong onSuccess
  //     } else {
  //       const id = Number(location.pathname.split("/")[2]);
  //       setSurveyId(id); // update thì lấy id luôn
  //     }
  //   }, []);

  // useEffect(() => {
  //   if (!id || !data) return;
  //   setFormData(data.data);
  //   // setIsDisable(data?.data?.IsPause);
  //   latestDataRef.current = data.data;
  // }, [id, data]);

  useEffect(() => {
    if (!isTrigger) return;

    if (!isEqual(latestDataRef.current, formData)) {
      latestDataRef.current = formData;
      setHasChanges(true);
      if (!timeoutRef.current) {
        handleSave();
      }
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formData]);

  useBlocker(true);
  //console.log("isSaving:", isSaving, "isSuccess:", isSuccess);
  return (
    <MainTemPlate>
     {isDisable && (
        <OverlayDisable
          onClose={() => { }}
          onDelete={() => { }}
          onSwap={() => { }}
          formData={formData}
          orderCurrent={formData}
          setIsOpenOverlay={setIsDisable}
        />
      )}
      {isLoading ? (
        <div className="w-full h-[829px] flex flex-col items-center justify-center gap-3">
          <div className="spinner"></div>
          <p className="font-bold text-2xl">Đang tải dữ liệu... </p>
        </div>
      ) : (
        <div
          style={{
            maxHeight: `calc(100vh - ${HEADER_HEIGHT}px)`,
            overflow: "hidden",
          }}
          className={`flex flex-col`}
        >
          <div className="survey-header">
            <div className="survey-tabs">
              {tabs.map((tab, index) => (
                <div key={tab.value} className="tab-item">
                  <Tab
                    label={tab.label}
                    disabled={tab.disabled}
                    className={
                      activeTab === tab.value ? "tab-active" : "tab-inactive"
                    }
                    onClick={() => !tab.disabled && handleTabClick(tab.value)}
                  />
                  {index < tabs.length - 1 && (
                    <NavigateNextIcon
                      className="tab-separator"
                      fontSize="small"
                    />
                  )}
                </div>
              ))}
            </div>
            <div className="survey-actions">
              <Button //Thịnh , lưu thất bại thì nền đỏ, xử lí lưu thủ công
                variant="text"
                className={`btn-save`}
                disabled={isApiCalling}
                onClick={() => (isTrigger ? null : handleConfirm())}
                sx={{
                  backgroundColor: "#4caf50",
                  color: "#ffffff",
                  "&:hover": {
                    backgroundColor: "#43a047",
                  },
                  "&.Mui-disabled": {
                    backgroundColor: "#4caf50",
                    color: "#ffffff",
                  },
                  // ...(hasChanges &&
                  //   !isSaving && {
                  //   color: "#000000",
                  //   "&:hover": {
                  //     backgroundColor: "#bbbbbb",
                  //   },
                  // }),
                  ...(!isSaving &&
                    !isSuccess &&
                    !isTrigger && {
                    backgroundColor: "#f44336",
                    color: "#ffffff",
                    "&:hover": {
                      backgroundColor: "#d32f2f",
                    },
                  }),
                }}
              >
                {isApiCalling
                  ? "Đang lưu..." // Hiển thị khi lưu thủ công
                  : isTrigger
                    ? isSaving
                      ? `Đang lưu ... ${saveCountdown}`
                      : isSuccess
                        ? "Đã lưu"
                        : "Lưu thất bại, vui lòng thử lại"
                    : isSuccess
                      ? "Lưu"
                      : "Lưu thất bại, vui lòng thử lại"}
              </Button>
            </div>
          </div>
          <div
            className="survey-content"
            style={{
              height: `calc(100vh - ${HEADER_HEIGHT}px - 60px)`,
              overflow: "auto",
            }}
          >
            {ActiveComponent}
          </div>
        </div>
      )}
    </MainTemPlate>
  );
};

export default SurveyNew;
