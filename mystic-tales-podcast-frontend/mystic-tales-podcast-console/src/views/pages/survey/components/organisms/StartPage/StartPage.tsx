/* eslint-disable @typescript-eslint/no-explicit-any */
import CheckIcon from "@mui/icons-material/Check";
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import SettingsIcon from "@mui/icons-material/Settings";
import CloudUploadIcon from "@mui/icons-material/CloudUpload";
import { FormControl, MenuItem, Select, Slider } from "@mui/material";
import React, { useEffect, useState } from "react";
import "react-datepicker/dist/react-datepicker.css";
import { handleSelectBackground } from "../../../helpers/handleSelectBackground";
import type { PageProps, SurveyType } from "@/core/types/tools";
// import OverlayDisable from "../../molecules/overlay-disable/OverlayDisable";
import ColorPickerModal from "./Components/ColorPickerModal";
import SecurityModal from "./Components/SecurityModal";
import "./styles.scss";

const backgrounds = Array.from(
  { length: 11 },
  (_, index) => `start${index + 1}`
);

const mockSurveyData = {
  Id: 1,
  RequesterId: 1,
  MarketSurveyVersionStatusId: 1,
  SurveyTypeId: 1,
  SurveyTopicId: null,
  SurveySpecificTopicId: null,
  SurveyStatusId: 1,
  SecurityModeId: 1,
  Background: "start1",
  ConfigJson: {
    BackgroundGradient1Color: "#FCE38A",
    BackgroundGradient2Color: "#F38181",
    TitleColor: "#FFFFFF",
    ContentColor: "#CCCCCC",
    ButtonBackgroundColor: "#007bff",
    ButtonContentColor: "#ffffff",
    Password: "",
    Brightness: 100,
    SkipStartPage: false,
  },
  Description: "Mô tả khảo sát mặc định",
  Title: "Tiêu đề khảo sát mặc định",
  Questions: [],
};

const fetchSurveyData = (): Promise<SurveyType> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve(mockSurveyData as any);
    }, 500);
  });
};

// eslint-disable-next-line @typescript-eslint/no-unused-vars
const saveSurveyData = (_data: SurveyType): Promise<void> => {
  return new Promise((resolve) => {
    setTimeout(() => {
      resolve();
    }, 500);
  });
};

const StartPage = ({
  formData,
  setFormData,
  handleTabClick,
  defaultBackgroundThemes,
  securityModes,
  topics,
  specificTopics,
}: // isDisable,
// isTrigger,
PageProps) => {
  const handleInputChange = (
    field: keyof SurveyType,
    value: string | boolean
  ) => {
    setFormData((prev: any) => ({ ...prev, [field]: value }));
  };

  const handleToggleSkipStartPage = (checked: boolean) => {
    setSkipStartPage(checked);
    // handleInputChange("SkipStartPage", checked);
    setFormData((prev: any) => ({
      ...prev,
      ConfigJson: { ...prev.ConfigJson, SkipStartPage: checked },
    }));
  };

  const handleStartSurvey = () => {
    saveSurveyData(formData)
      .then(() => {
        handleTabClick(1);
      })
      .catch((error) => {
        console.error("Error saving survey data (simulated):", error);
      });
  };

  const [SkipStartPage, setSkipStartPage] = useState(false);
  const [Brightness, setBrightness] = useState<number>(
    formData?.ConfigJson?.Brightness || 100
  );
  const [backgroundMode, setBackgroundMode] = useState<"image" | "color">(
    "image"
  );
  const [TitleColor, setTitleColor] = useState<string>(
    formData?.ConfigJson?.TitleColor || "#FFFFFF"
  );
  const [ContentColor, setContentColor] = useState<string>(
    formData?.ConfigJson?.ContentColor || "#CCCCCC"
  );
  const [buttonBgColor, setButtonBgColor] = useState<string>(
    formData?.ConfigJson?.ButtonBackgroundColor || "#007bff"
  );
  const [buttonTextColor, setButtonTextColor] = useState<string>(
    formData?.ConfigJson?.ButtonContentColor || "#ffffff"
  );
  const [showPasswordModal, setShowPasswordModal] = useState(false);
  const [showColorModal, setShowColorModal] = useState(false);
  const [activeColorSetter, setActiveColorSetter] = useState<React.Dispatch<
    React.SetStateAction<string>
  > | null>(null);
  const [pickerForBackground, setPickerForBackground] = useState(false);
  const [selectedSurveyTopic, setSelectedSurveyTopic] = useState<number>(
    formData?.SurveyTopicId
  );
  const [selectedSurveySpecificTopic, setSelectedSurveySpecificTopic] =
    useState<number>(formData?.SurveySpecificTopicId);
  const [surveyStatusChecked, setSurveyStatusChecked] = useState<boolean>(
    formData?.SurveyStatusId === 1
  );
  const [selectedSecurityMode, setSelectedSecurityMode] = useState<number>(
    formData?.SecurityModeId
  );

  useEffect(() => {
    localStorage.setItem("surveyFormData", JSON.stringify(formData));
  }, [formData]);

  useEffect(() => {
    setSkipStartPage(formData.ConfigJson.SkipStartPage || false);
    setSurveyStatusChecked(formData.SurveyStatusId === 1);
    setSelectedSecurityMode(
      formData.SecurityModeId == null ? 1 : formData.SecurityModeId
    );
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    setSelectedSurveyTopic(formData?.SurveyTopicId);
    setSelectedSurveySpecificTopic(formData?.SurveySpecificTopicId);
    setBrightness(formData?.ConfigJson?.Brightness);
    setSurveyStatusChecked(formData?.SurveyStatusId === 1);
    setSelectedSecurityMode(
      formData?.SecurityModeId == null ? 1 : formData.SecurityModeId
    );

    if (formData?.ConfigJson?.Background === "custom") {
      setBackgroundMode("image");
    } else if (backgrounds.includes(formData?.ConfigJson?.Background)) {
      setBackgroundMode("image");
    } else if (
      formData?.ConfigJson?.Background?.startsWith("#") ||
      formData?.ConfigJson?.Background === "color_gradient"
    ) {
      setBackgroundMode("color");
    }

    setTitleColor(formData?.ConfigJson?.TitleColor);
    setContentColor(formData?.ConfigJson?.ContentColor);
    setButtonBgColor(formData?.ConfigJson?.ButtonBackgroundColor);
    setButtonTextColor(formData?.ConfigJson?.ButtonContentColor);

    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [formData, backgrounds]);

  const handleBrightnessChange = (
    _event: Event,
    newValue: number | number[]
  ) => {
    const newBrightness = newValue as number;
    setBrightness(newBrightness);
    setFormData((prev: any) => ({
      ...prev,
      ConfigJson: {
        ...prev.ConfigJson,
        Brightness: newBrightness,
      },
    }));
  };

  const handleBackgroundUpload = (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        const imageUrl = reader.result as string;
        setFormData((prev: any) => ({
          ...prev,
          ConfigJson: {
            ...prev.ConfigJson,
            IsUseBackgroundImageBase64: true,
            Background: "custom",
          },
          BackgroundImageBase64: imageUrl,
        }));
        setBackgroundMode("image");
        const defaultConfig = handleSelectBackground("default_color");
        setTitleColor(defaultConfig.colors.TitleColor);
        setContentColor(defaultConfig.colors.ContentColor);
        setButtonBgColor(defaultConfig.colors.ButtonBackgroundColor);
        setButtonTextColor(defaultConfig.colors.ButtonContentColor);
      };
      reader.readAsDataURL(file);
    }
  };

  const handleSelectColorBackground = () => {
    setBackgroundMode("color");
    setFormData((prev: any) => ({
      ...prev,
      ConfigJson: {
        ...prev.ConfigJson,
        Background: "color_gradient",
      },
    })); // Set background to a color type
    setPickerForBackground(true);
    setShowColorModal(true);
  };

  const handleCustomizePassword = () => {
    setShowPasswordModal(true);
  };

  return (
    <div
      className="startpage-root flex"
      style={{ height: "100vh", overflow: "hidden" }}
    >
      {/* {isDisable ? <OverlayDisable /> : null} */}
      <div
        className="relative flex-1 flex items-center justify-center"
        style={{
          ...(formData?.ConfigJson?.Background === "color_gradient" && {
            background: `linear-gradient(to right, ${formData?.ConfigJson.BackgroundGradient1Color}, ${formData?.ConfigJson.BackgroundGradient2Color})`,
            overflowY: "auto",
          }),
        }}
      >
        {defaultBackgroundThemes &&
          formData?.ConfigJson.Background === "image" && (
            <div
              className="absolute inset-0"
              style={{
                backgroundImage: `url(${
                  formData?.ConfigJson?.IsUseBackgroundImageBase64 &&
                  formData.BackgroundImageBase64
                    ? formData.BackgroundImageBase64
                    : formData?.ConfigJson?.DefaultBackgroundImageId
                    ? defaultBackgroundThemes.find(
                        (item: any) =>
                          item.Id ===
                          formData?.ConfigJson?.DefaultBackgroundImageId
                      )?.MainImageUrl
                    : ""
                })`,
                backgroundSize: "cover",
                backgroundPosition: "center",
                backgroundRepeat: "no-repeat",
                filter: `Brightness(${(Brightness ? Brightness : 100) / 100})`,
                backgroundColor: "transparent",
              }}
            ></div>
          )}
        <div className="relative z-10 flex flex-col items-center w-full max-w-2xl px-8">
          <div className="startpage-content w-full text-center">
            <input
              type="text"
              value={formData?.Title}
              onChange={(e) => handleInputChange("Title", e.target.value)}
              className="startpage-title-input"
              placeholder="Nhập tiêu đề"
              style={{ color: TitleColor }}
            />
            <input
              value={formData?.Description}
              onChange={(e) => handleInputChange("Description", e.target.value)}
              className="startpage-desc-input"
              placeholder="Nhập mô tả tại đây"
              style={{ color: ContentColor }}
            />
            <button
              onClick={handleStartSurvey}
              className="startpage-btn group"
              style={{
                background:
                  buttonBgColor?.startsWith("linear-gradient") ||
                  buttonBgColor?.startsWith("radial-gradient")
                    ? buttonBgColor
                    : "",
                backgroundColor: !(
                  buttonBgColor?.startsWith("linear-gradient") ||
                  buttonBgColor?.startsWith("radial-gradient")
                )
                  ? buttonBgColor
                  : "",
                color: buttonTextColor,
              }}
            >
              <span>Bắt đầu</span>
              <span className="startpage-icon-wrapper">
                <ChevronRightIcon />
              </span>
            </button>
          </div>
        </div>
      </div>
      <div className="startpage-options w-[420px] bg-white h-full overflow-y-auto shadow-lg p-8">
        <div className="space-y-6">
          <div className="config-section">
            <h3 className="config-title">BỎ QUA TRANG BẮT ĐẦU</h3>
            <div className="flex items-center justify-between">
              <span className="text-gray-600">Bỏ qua</span>
              <label className="toggle-switch">
                <input
                  type="checkbox"
                  checked={SkipStartPage}
                  onChange={(e) => handleToggleSkipStartPage(e.target.checked)}
                  aria-label="Bỏ qua trang bắt đầu"
                />
                <span className="toggle-slider"></span>
              </label>
            </div>
          </div>
          <ToppicSurvey
            selectedSurveyTopic={selectedSurveyTopic}
            setSelectedSurveyTopic={setSelectedSurveyTopic}
            topics={topics}
            specificTopics={specificTopics}
            // isTrigger={isTrigger}
            selectedSurveySpecificTopic={selectedSurveySpecificTopic}
            setSelectedSurveySpecificTopic={setSelectedSurveySpecificTopic}
            setFormData={setFormData}
          />
          <SurveyStatus
            surveyStatusChecked={surveyStatusChecked}
            setSurveyStatusChecked={setSurveyStatusChecked}
            setFormData={setFormData}
          />
          <div className="config-section">
            <h3 className="config-title">UPLOAD ẢNH</h3>
            <div className="upload-container">
              <input
                type="file"
                id="imageUpload"
                accept="image/*"
                className="hidden"
                onChange={(event) => {
                  const file = event.target.files?.[0];
                  if (file) {
                    const reader = new FileReader();
                    reader.onloadend = () => {
                      const base64 = reader.result as string;
                      setFormData((prev: any) => ({
                        ...prev,
                        MainImageBase64: base64,
                      }));
                    };
                    reader.readAsDataURL(file);
                  }
                }}
              />
              <label htmlFor="imageUpload" className="upload-button">
                <CloudUploadIcon className="upload-icon" />
                <span className="upload-text">
                  {formData?.MainImageBase64 ? "Thay đổi ảnh" : "Chọn ảnh"}
                </span>
              </label>
              {formData?.MainImageBase64 && (
                <div className="image-preview">
                  <img
                    src={formData.MainImageBase64}
                    alt="Preview"
                    className="preview-image"
                  />
                  <button
                    className="remove-image-btn"
                    onClick={() => {
                      setFormData((prev: any) => ({
                        ...prev,
                        MainImageBase64: null,
                      }));
                    }}
                  >
                    ×
                  </button>
                </div>
              )}
            </div>
          </div>
          <div className="config-section">
            <h3 className="config-title">KHẢO SÁT TRÊN NỀN AUDIO</h3>
            <div className="flex items-center justify-between mb-3">
              <span className="text-gray-600">Bật</span>
              <button className="buy-now-button">Mua đi nè</button>
            </div>
          </div>
          {/* Lộc Bỏ Cái Này */}
          {/* <CustomizePassword
            formData={formData}
            setFormData={setFormData}
            Add
            commentMore
            actions
            handleCustomizePassword={handleCustomizePassword}
          /> */}
          {securityModes && (
            <SecurityMode
              selectedSecurityMode={selectedSecurityMode}
              setSelectedSecurityMode={setSelectedSecurityMode}
              setFormData={setFormData}
              securityModes={securityModes}
            />
          )}
          <BackgroundMode
            backgroundMode={backgroundMode}
            setBackgroundMode={setBackgroundMode}
            formData={formData}
            handleBrightnessChange={handleBrightnessChange}
            handleBackgroundUpload={handleBackgroundUpload}
            handleSelectColorBackground={handleSelectColorBackground}
            setFormData={setFormData}
            Brightness={Brightness}
          />
          <div className="w-full max-w-md mx-auto bg-white">
            <SurveyContentTextColor
              TitleColor={TitleColor}
              ContentColor={ContentColor}
              setTitleColor={setTitleColor}
              setContentColor={setContentColor}
              setShowColorModal={setShowColorModal}
              setActiveColorSetter={setActiveColorSetter}
            />
            <ButtonColor
              buttonBgColor={buttonBgColor}
              setButtonBgColor={setButtonBgColor}
              buttonTextColor={buttonTextColor}
              setButtonTextColor={setButtonTextColor}
              setShowColorModal={setShowColorModal}
              setActiveColorSetter={setActiveColorSetter}
            />
            <DesignSuggestions
              formData={formData}
              backgrounds={backgrounds}
              setFormData={setFormData}
              setBackgroundMode={setBackgroundMode}
              backgroundMode={backgroundMode}
              defaultBackgroundThemes={defaultBackgroundThemes}
            />
          </div>
        </div>
      </div>
      {showPasswordModal && (
        <SecurityModal
          open={showPasswordModal}
          onClose={() => setShowPasswordModal(false)}
          onSavePassword={(newPassword: any) => {
            setFormData((prev: any) => ({
              ...prev,
              // SecurityModeId: newPassword ? 2 : 1,
              ConfigJson: {
                ...prev.ConfigJson,
                Password: newPassword,
              },
            }));
          }}
          initialPassword={formData?.ConfigJson?.Password || ""}
        />
      )}
      {showColorModal && (
        <ColorPickerModal
          open={showColorModal}
          onClose={() => {
            setShowColorModal(false);
            setActiveColorSetter(null);
            setPickerForBackground(false);
          }}
          onSelectColors={({ color1, color2 }) => {
            if (pickerForBackground) {
              setFormData((prev: any) => ({
                ...prev,
                ConfigJson: {
                  ...prev.ConfigJson,
                  BackgroundGradient1Color: color1,
                  BackgroundGradient2Color: color2,
                  Background: color1 !== color2 ? "color_gradient" : color1,
                },
              }));
            } else if (activeColorSetter) {
              if (activeColorSetter === setButtonBgColor) {
                if (color1 !== color2) {
                  activeColorSetter(
                    `linear-gradient(to right, ${color1}, ${color2})`
                  );
                } else {
                  activeColorSetter(color1);
                }
                setFormData((prev: any) => ({
                  ...prev,
                  ConfigJson: {
                    ...prev.ConfigJson,
                    ButtonBackgroundColor:
                      color1 !== color2
                        ? `linear-gradient(to right, ${color1}, ${color2})`
                        : color1,
                  },
                }));
              } else {
                activeColorSetter(color1);
                setFormData((prev: any) => {
                  const newConfig = {
                    ...prev.ConfigJson,
                  };
                  if (activeColorSetter === setTitleColor) {
                    newConfig.TitleColor = color1;
                  } else if (activeColorSetter === setContentColor) {
                    newConfig.ContentColor = color1;
                  } else if (activeColorSetter === setButtonTextColor) {
                    newConfig.ButtonContentColor = color1;
                  }
                  return {
                    ...prev,
                    ConfigJson: newConfig,
                  };
                });
              }
            }
            setShowColorModal(false);
            setActiveColorSetter(null);
            setPickerForBackground(false);
          }}
          initialColors={(() => {
            let initialColor1 = "#FCE38A";
            let initialColor2 = "#F38181";

            if (pickerForBackground) {
              initialColor1 =
                formData?.ConfigJson?.BackgroundGradient1Color || "#FCE38A";
              initialColor2 =
                formData?.ConfigJson?.BackgroundGradient2Color || "#F38181";
            } else if (activeColorSetter === setTitleColor) {
              initialColor1 = TitleColor;
              initialColor2 = TitleColor;
            } else if (activeColorSetter === setContentColor) {
              initialColor1 = ContentColor;
              initialColor2 = ContentColor;
            } else if (activeColorSetter === setButtonTextColor) {
              initialColor1 = buttonTextColor;
              initialColor2 = buttonTextColor;
            } else if (activeColorSetter === setButtonBgColor) {
              if (buttonBgColor?.startsWith("linear-gradient")) {
                const colors = buttonBgColor.match(
                  /#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{3})/g
                );
                if (colors && colors.length >= 2) {
                  initialColor1 = colors[0];
                  initialColor2 = colors[1];
                } else {
                  initialColor1 = "#FCE38A";
                  initialColor2 = "#F38181";
                }
              } else if (buttonBgColor?.startsWith("#")) {
                initialColor1 = buttonBgColor;
                initialColor2 = buttonBgColor;
              } else {
                initialColor1 = "#FCE38A";
                initialColor2 = "#F38181";
              }
            }
            return [initialColor1, initialColor2];
          })()}
        />
      )}
    </div>
  );
};

export default StartPage;

function ToppicSurvey({
  selectedSurveyTopic,
  setSelectedSurveyTopic,
  selectedSurveySpecificTopic,
  setSelectedSurveySpecificTopic,
  setFormData,
  topics,
  specificTopics,
}: // isTrigger, //Thịnh , fix lỗi lưu về của function này
any) {
  return (
    <>
      <div className="config-section">
        <h6 className="text-left" >CHỦ ĐỀ KHẢO SÁT</h6>
        <FormControl
          // disabled={isTrigger}
          fullWidth
          sx={{
            ".MuiOutlinedInput-root": {
              height: "48px",
              borderRadius: "8px",
              border: "1px solid #D1D5DB",
              "& fieldset": { border: "none" },
              "&:hover fieldset": { border: "none" },
              "&.Mui-focused fieldset": { border: "none" },
            },
            ".MuiInputLabel-root": {
              transform: "translate(14px, 14px) scale(1)",
              "&.Mui-focused": {
                transform: "translate(14px, -9px) scale(0.75)",
              },
              "&.MuiInputLabel-shrink": {
                transform: "translate(14px, -9px) scale(0.75)",
              },
            },
            ".MuiSelect-select": {
              padding: "12px 14px",
              display: "flex",
              alignItems: "center",
            },
            ".MuiSelect-icon": {
              right: "14px",
              color: "#6B7280",
            },
          }}
        >
          <Select
            labelId="survey-topic-select-label"
            id="survey-topic-select"
            value={selectedSurveyTopic ?? "default"}
            label="Chọn chủ đề"
            onChange={(e) => {
              const newTopicId =
                e.target.value === "default" ? null : e.target.value;
              setSelectedSurveyTopic(newTopicId);
              setFormData((prev: any) => ({
                ...prev,
                SurveyTopicId: newTopicId,
                SurveySpecificTopicId: null,
              }));
              setSelectedSurveySpecificTopic(null);
            }}
          >
            <MenuItem value="default">Chọn chủ đề</MenuItem>
            {topics &&
              topics.map((topic: any) => (
                <MenuItem key={topic.Id} value={topic.Id}>
                  {topic.Name}
                </MenuItem>
              ))}
          </Select>
        </FormControl>
      </div>
      <div className="config-section">
        <h6 className="text-left" >CHỦ ĐỀ KHẢO SÁT CỤ THỂ</h6>
        <FormControl
          fullWidth
          sx={{
            ".MuiOutlinedInput-root": {
              height: "48px",
              borderRadius: "8px",
              border: "1px solid #D1D5DB",
              "& fieldset": { border: "none" },
              "&:hover fieldset": { border: "none" },
              "&.Mui-focused fieldset": { border: "none" },
            },
            ".MuiInputLabel-root": {
              transform: "translate(14px, 14px) scale(1)",
              "&.Mui-focused": {
                transform: "translate(14px, -9px) scale(0.75)",
              },
              "&.MuiInputLabel-shrink": {
                transform: "translate(14px, -9px) scale(0.75)",
              },
            },
            ".MuiSelect-select": {
              padding: "12px 14px",
              display: "flex",
              alignItems: "center",
            },
            ".MuiSelect-icon": {
              right: "14px",
              color: "#6B7280",
            },
          }}
          disabled={!selectedSurveyTopic}
        >
          <Select
            // disabled={isTrigger}
            labelId="survey-specific-topic-select-label"
            id="survey-specific-topic-select"
            value={selectedSurveySpecificTopic ?? "default"}
            label="Chọn chủ đề cụ thể"
            onChange={(e) => {
              const newSpecificTopicId =
                e.target.value === "default" ? null : e.target.value;
              setSelectedSurveySpecificTopic(newSpecificTopicId);
              setFormData((prev: any) => ({
                ...prev,
                SurveySpecificTopicId: newSpecificTopicId,
              }));
            }}
          >
            <MenuItem value="default">Chọn chủ đề cụ thể</MenuItem>
            {specificTopics &&
              specificTopics
                .filter(
                  (topic: any) => topic.SurveyTopicId === selectedSurveyTopic
                )
                .map((topic: any) => (
                  <MenuItem key={topic.Id} value={topic.Id}>
                    {topic.Name}
                  </MenuItem>
                ))}
          </Select>
        </FormControl>
      </div>
    </>
  );
}

function SurveyStatus({
  surveyStatusChecked: surveyStatusChecked,
  setSurveyStatusChecked: setSurveyStatusChecked,
  setFormData: setFormData,
}: any) {
  return (
    <>
      <div className="config-section">
        <div className="flex items-center mb-3">
          <h3 className="config-title">TRẠNG THÁI KHẢO SÁT</h3>
        </div>
        <div className="flex items-center justify-between">
          <label className="toggle-switch">
            <input
              type="checkbox"
              checked={surveyStatusChecked}
              onChange={(e) => {
                const newCheckedStatus = e.target.checked;
                setSurveyStatusChecked(newCheckedStatus);
                setFormData((prev: any) => ({
                  ...prev,
                  SurveyStatusId: newCheckedStatus ? 1 : 2,
                }));
              }}
              aria-label="Trạng thái khảo sát"
            />
            <span className="toggle-slider"></span>
          </label>
        </div>
      </div>
    </>
  );
}

function CustomizePassword({
  formData,
  setFormData,
  handleCustomizePassword,
}: any) {
  const hasPassword =
    formData?.ConfigJson?.Password !== null &&
    formData?.ConfigJson?.Password !== undefined;

  return (
    <div className="config-section">
      <div className="flex items-center mb-3">
        <h3 className="config-title">ĐẶT MẬT KHẨU CHO KHẢO SÁT</h3>
      </div>
      <div className="flex items-center justify-between">
        <span className="text-gray-600">Bật</span>
        <label className="toggle-switch">
          <input
            type="checkbox"
            checked={hasPassword}
            onChange={(e) => {
              const on = e.target.checked;
              setFormData((prev: any) => ({
                ...prev,
                ConfigJson: {
                  ...prev.ConfigJson,
                  // khi bật: để chuỗi rỗng để hiển thị nút Tùy chỉnh
                  // khi tắt: set về null để ẩn nút Tùy chỉnh
                  Password: on ? "" : null,
                },
              }));
            }}
            aria-label="Đặt mật khẩu cho khảo sát"
          />
          <span className="toggle-slider"></span>
        </label>
      </div>
      {hasPassword && (
        <button className="customize-button" onClick={handleCustomizePassword}>
          <SettingsIcon fontSize="small" />
          <span>Tùy chỉnh</span>
        </button>
      )}
    </div>
  );
}

function SecurityMode({
  selectedSecurityMode,
  setSelectedSecurityMode,
  setFormData,
  securityModes,
}: any) {
  return (
    <div className="config-section">
      <h3 className="config-title">CHẾ ĐỘ BẢO MẬT</h3>

      <FormControl fullWidth>
        <Select
          value={selectedSecurityMode ?? ""}
          onChange={(e) => {
            const newMode = Number(e.target.value);
            setSelectedSecurityMode(newMode);
            setFormData((prev: any) => ({
              ...prev,
              SecurityModeId: newMode,
            }));
          }}
          displayEmpty
          renderValue={(selected) => {
            if (selected === "") return "Chọn chế độ";
            const selectedMode = securityModes.find(
              (mode: any) => mode.Id === selected
            );
            return selectedMode?.Name || "Không xác định";
          }}
        >
          <MenuItem disabled value="">
            Chọn chế độ
          </MenuItem>
          {Array.isArray(securityModes) &&
            securityModes.map((mode: any) => (
              <MenuItem key={mode.Id} value={mode.Id}>
                {mode.Name}
              </MenuItem>
            ))}
        </Select>
      </FormControl>
    </div>
  );
}

function BackgroundMode({
  backgroundMode: backgroundMode,
  formData: formData,
  handleBrightnessChange: handleBrightnessChange,
  handleBackgroundUpload: handleBackgroundUpload,
  handleSelectColorBackground: handleSelectColorBackground,
  Brightness: Brightness,
  setFormData: setFormData,
}: any) {
  console.log("check formData: ", formData?.ConfigJson?.Background);

  return (
    <>
      <div>
        <h6 className="text-left">SỬ DỤNG HÌNH NỀN</h6>
        <div
          className={`background-main-preview ${
            formData?.ConfigJson?.Background === "image" ? "active" : ""
          }`}
          style={{
            backgroundImage: `url(${formData?.BackgroundImageBase64})`,
            backgroundSize: "cover",
            backgroundPosition: "center",
          }}
        >
          <div
            onClick={() => {
              setFormData((prev: any) => ({
                ...prev,
                ConfigJson: {
                  ...prev.ConfigJson,
                  Background: "image",
                },
              }));

              document.getElementById("backgroundInput")?.click();
            }}
            className="absolute bottom-4 left-4 z-10"
            style={{
              color: "white",
              fontSize: "0.8rem",
              opacity: 1,
            }}
          >
            Click để đổi hình
          </div>
          <CheckIcon
            style={{
              color:
                formData?.ConfigJson?.IsUseBackgroundImageBase64 &&
                formData?.ConfigJson?.Background === "image"
                  ? "blue"
                  : "#ccc",
            }}
            onClick={() => {
              setFormData((prev: any) => ({
                ...prev,
                ConfigJson: {
                  ...prev.ConfigJson,
                  IsUseBackgroundImageBase64:
                    !prev?.ConfigJson?.IsUseBackgroundImageBase64,
                  Background: "image",
                },
              }));
            }}
            className="absolute main-check-icon z-10 text-[blue]"
          />
          <div className="absolute inset-0 bg-black opacity-30 z-0"></div>
        </div>
        <input
          type="file"
          id="backgroundInput"
          accept="image/*"
          className="hidden"
          onChange={handleBackgroundUpload}
          aria-label="Tải lên hình nền"
        />
      </div>
      {backgroundMode === "image" && (
        <div>
          <h6  className="text-left" >ĐỘ SÁNG HÌNH NỀN</h6>
          <Slider
            value={Brightness}
            onChange={handleBrightnessChange}
            aria-labelledby="Brightness-slider"
            valueLabelDisplay="auto"
            min={0}
            max={100}
            sx={{
              color: "grey",
              marginTop: "20px",
              "& .MuiSlider-valueLabel": {
                borderRadius: "50%",
                backgroundColor: "grey",
                color: "white",
                fontSize: "0.5rem",
              },
            }}
          />
        </div>
      )}
      <div>
        <h6 className="text-left">SỬ DỤNG MÀU NỀN</h6>
        <div
          className={`background-main-preview ${
            formData?.ConfigJson?.Background === "color_gradient"
              ? "active"
              : ""
          }`}
          style={{
            background:
              formData?.ConfigJson?.Background === "color_gradient"
                ? `linear-gradient(to right, ${formData?.ConfigJson.BackgroundGradient1Color}, ${formData?.ConfigJson.BackgroundGradient2Color})`
                : formData?.ConfigJson?.Background?.startsWith("#")
                ? formData?.ConfigJson?.Background
                : "#cccccc",
            backgroundSize: "cover",
            backgroundPosition: "center",
          }}
          onClick={() => {
            handleSelectColorBackground();
          }}
        >
          <div
            style={{
              color: "white",
              fontSize: "0.8rem",
              opacity: 1,
            }}
            // onClick={(e) => {
            //     // console.log("run");
            //     // e.stopPropagation();
            //     handleSelectColorBackground();
            // }}
          >
            Click để đổi màu
          </div>
          <CheckIcon
            style={{
              color:
                formData?.ConfigJson?.Background === "color_gradient"
                  ? "blue"
                  : "#ccc",
            }}
            className="absolute main-check-icon z-10"
            // onClick={() => {
            //     console.log("run >>>>>>>>>>>>");
            //     setFormData((prev: any) => ({
            //         ...prev,
            //         Background:
            //             formData?.ConfigJson?.Background ===
            //             "color_gradient"
            //                 ? "image"
            //                 : "color_gradient",
            //     }));
            // }}
            onClick={(e) => {
              e.stopPropagation(); // CHẶN nổi bọt lên cha
              setFormData({
                ...formData,
                ConfigJson: {
                  ...formData?.ConfigJson,
                  Background: "color_gradient",
                },
              });
            }}
          />
          <div className="absolute inset-0 bg-black opacity-30 z-0"></div>
        </div>
      </div>
    </>
  );
}

function SurveyContentTextColor({
  TitleColor: TitleColor,
  ContentColor: ContentColor,
  setTitleColor: setTitleColor,
  setContentColor: setContentColor,
  setShowColorModal: setShowColorModal,
  setActiveColorSetter: setActiveColorSetter,
}: any) {
  return (
    <>
      <div className="mb-8">
        <h3 className="text-sm font-semibold text-gray-800 uppercase tracking-wide mb-6 text-left"  >
          MÀU CHỮ CỦA NỘI DUNG KHẢO SÁT
        </h3>

        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500 w-24 text-left">Màu tiêu đề</span>
            <div className="flex-1 max-w-20">
              <div
                className="w-full h-8 rounded border border-gray-300 cursor-pointer"
                style={{ backgroundColor: TitleColor }}
                onClick={() => {
                  setShowColorModal(true);
                  setActiveColorSetter(() => setTitleColor);
                }}
              ></div>
            </div>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500 w-24 text-left">Màu nội dung</span>
            <div className="flex-1 max-w-20">
              <div
                className="w-full h-8 rounded border border-gray-300 cursor-pointer"
                style={{ backgroundColor: ContentColor }}
                onClick={() => {
                  setShowColorModal(true);
                  setActiveColorSetter(() => setContentColor);
                }}
              ></div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

function ButtonColor({
  buttonBgColor: buttonBgColor,
  setButtonBgColor: setButtonBgColor,
  buttonTextColor: buttonTextColor,
  setButtonTextColor: setButtonTextColor,
  setShowColorModal: setShowColorModal,
  setActiveColorSetter: setActiveColorSetter,
}: any) {
  return (
    <>
      <div>
        <h3 className="text-sm font-semibold text-gray-800 uppercase tracking-wide mb-6 text-left">
          MÀU SẮC NÚT BẤM
        </h3>

        <div className="space-y-4">
          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500 w-24 text-left">Màu nền</span>
            <div className="flex-1 max-w-20">
              <div
                className="w-full h-8 rounded border border-gray-300 cursor-pointer"
                style={{
                  background:
                    buttonBgColor?.startsWith("linear-gradient") ||
                    buttonBgColor?.startsWith("radial-gradient")
                      ? buttonBgColor
                      : "",
                  backgroundColor: !(
                    buttonBgColor?.startsWith("linear-gradient") ||
                    buttonBgColor?.startsWith("radial-gradient")
                  )
                    ? buttonBgColor
                    : "",
                }}
                onClick={() => {
                  setShowColorModal(true);
                  setActiveColorSetter(() => setButtonBgColor);
                }}
              ></div>
            </div>
          </div>

          <div className="flex items-center justify-between">
            <span className="text-sm text-gray-500 w-24 text-left">Màu chữ</span>
            <div className="flex-1 max-w-20">
              <div
                className="w-full h-8 rounded border border-gray-300 cursor-pointer"
                style={{ backgroundColor: buttonTextColor }}
                onClick={() => {
                  setShowColorModal(true);
                  setActiveColorSetter(() => setButtonTextColor);
                }}
              ></div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}

function DesignSuggestions({
  formData,
  setFormData,
  setBackgroundMode,
  backgroundMode,
  defaultBackgroundThemes,
}: {
  formData: any;
  backgrounds: any[];
  setFormData: any;
  setBackgroundMode: any;
  backgroundMode: "image" | "color";
  defaultBackgroundThemes: any;
}) {
  // const [listBackground, setListBackground] = useState<
  //   {
  //     id: number;
  //     TitleColor: string;
  //     ContentColor: string;
  //     ButtonBackgroundColor: string;
  //     ButtonContentColor: string;
  //     url: string;
  //   }[]
  // >([]);

  // useEffect(() => {
  //   const fetchBackgrounds = async () => {
  //     const response = await axios.get("/survey/all-bg");
  //     setListBackground(response.data.data);
  //     localStorage.setItem(
  //       "listBackground",
  //       JSON.stringify(response.data.data)
  //     );
  //   };
  //   fetchBackgrounds();
  // }, []);
    if (!defaultBackgroundThemes || !Array.isArray(defaultBackgroundThemes)) {
    return ( //Thịnh redux
      <div className="config-section">
        <h3 className="config-title">GỢI Ý THIẾT KẾ</h3>
        <div className="background-preview">
          <div className="background-thumbnail">
            <div className="grid grid-cols-5 gap-4">
              <div className="col-span-5 text-center py-4">
                Đang tải gợi ý thiết kế...
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }
  return (
    <>
      <div className="config-section">
        <h3 className="config-title">GỢI Ý THIẾT KẾ</h3>
        <div className="background-preview">
          <div className="background-thumbnail">
            <div className="grid grid-cols-5 gap-4">
              {defaultBackgroundThemes.map((item: any, index: any) => {
                return (
                  <div
                    key={item.Id}
                    className={`background-thumbnail-item ${
                      formData?.ConfigJson?.DefaultBackgroundImageId ===
                        item.Id && backgroundMode === "image"
                        ? "active"
                        : ""
                    }`}
                    onClick={() => {
                      setFormData((prev: any) => ({
                        ...prev,
                        ConfigJson: {
                          ...prev.ConfigJson,
                          DefaultBackgroundImageId: item.Id,
                          TitleColor: item.ConfigJson.TitleColor, // Thịnh, 4 cái dưới này thiếu .ConfigJson
                          ContentColor: item.ConfigJson.ContentColor,
                          ButtonBackgroundColor:
                            item.ConfigJson.ButtonBackgroundColor,
                          ButtonContentColor:
                            item.ConfigJson.ButtonContentColor,
                          IsUseBackgroundImageBase64: false, // thêm dòng này
                        },
                      }));
                      setBackgroundMode("image");
                    }}
                  >
                    <img
                      src={item.MainImageUrl}
                      alt="background"
                      className="w-full h-full object-cover"
                    />
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </div>
    </>
  );
}
