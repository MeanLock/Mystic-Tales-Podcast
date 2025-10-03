/* eslint-disable @typescript-eslint/no-unsafe-function-type */
/* eslint-disable @typescript-eslint/no-explicit-any */
// eslint-disable-next-line @typescript-eslint/no-explicit-any
import AddCircleIcon from "@mui/icons-material/AddCircle";
import CheckCircleIcon from "@mui/icons-material/CheckCircle";
import { Button } from "@mui/material";
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { SurveyQuestionType } from "../../../constants/questions";
import type { OptionType, QuestionType, SurveyType } from "@/core/types/tools";
import FormSelectType from "../../molecules/form-select-type/FormSelectType";
import MultipleChoice from "../MultipleChoice/MultipleChoice";
import type { RangeSliderConfigJsonType } from "../RangeSlider/RangeSlider";
import RangeSlider from "../RangeSlider/RangeSlider";
import Ranking from "../Ranking/Ranking";
import Rating from "../Rating/Rating";
import SingleChoice from "../SingleChoice/SingleChoice";
import SingleInput from "../SingleInput/SingleInput";
import SingleSlider from "../SingleSlider/SingleSlider";
import Overlay from "../overlay/Overlay";
import Sidebar from "../sidebar/SideBar";
import SwitchCustomize from "./components/SwitchCustomize";
import RatingIcon from "./components/rating-icon/RatingIcon";
import "./styles.scss";
import { generateUUID } from "@/core/utils/uuid.util";
import { toast } from "react-toastify";

const questionDefault = {
  Id: "",
  QuestionTypeId: null,
  Content: "",
  Description: "",
  TimeLimit: 0,
  IsVoiced: false,
  Order: 0,
  ConfigJson: {},
  Options: [],
};

type Props = {
  formData: SurveyType;
  setFormData: React.Dispatch<React.SetStateAction<SurveyType>>;
  SurveyQuestionTypes: any;
  defaultBackgroundThemes: any; //Thịnh
  securityModes: any; //Thịnh
  fieldInputTypes: any;
  isTrigger?: boolean;
};

const QuestionPage = ({
  formData,
  setFormData,
  SurveyQuestionTypes,
  defaultBackgroundThemes, //Thịnh
  securityModes, //Thịnh
  fieldInputTypes,
  isTrigger,
}: Props) => {
  const [orderCurrent, setOrderCurrent] = useState(1);
  const [isOpenOverlay, setIsOpenOverlay] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const questionedit = useMemo(() => {
    return (formData?.Questions || []).find((item: any) => {
      return item?.Order === orderCurrent;
    });
  }, [formData, orderCurrent]);

  const handleUpdateQuestion = useCallback(
    (
      key: keyof QuestionType,
      value:
        | string
        | number
        | boolean
        | OptionType[]
        | Record<string, string | number>
        | RangeSliderConfigJsonType
        | Record<string, unknown>
    ) => {
      setFormData((prev: any) => ({
        ...prev,
        Questions: prev.Questions.map((item: any) => {
          if (questionedit?.Order && item.Order === questionedit?.Order) {
            return {
              ...item,
              [key]: value,
            };
          }
          return item;
        }),
      }));
    },
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [questionedit?.Order]
  );

  const rulesType = SurveyQuestionType.map((item) => {
    if (item.id === 1) {
      return {
        type: item.id,
        rules: [],
      };
    }

    if (item.id === 2) {
      return {
        type: item.id,
        rules: [
          {
            children: (
              <SwitchCustomize
                type="IsChooseMuitiple"
                question={questionedit}
                isMinMax
                handleUpdateQuestion={handleUpdateQuestion}
                label="Chọn nhiều trả lời"
              />
            ),
          },
          {
            children: (
              <SwitchCustomize
                type="is_auto_view_show"
                question={questionedit}
                handleUpdateQuestion={handleUpdateQuestion}
                label={
                  <div
                    style={{
                      marginTop: 10,
                    }}
                  >
                    <div>
                      Tự động chọn các câu trả lời đang hiển thị và qua câu tiếp
                      theo
                    </div>
                    <br />
                    <div
                      style={{
                        color: "#666",
                      }}
                    >
                      Bật tính năng này hệ thống sẽ tự động chọn hết các câu trả
                      lời đang hiển thị và chuyển qua câu hỏi tiếp theo nếu số
                      lượng câu trả lời đang hiển thị ít hơn hoặc bằng giá
                      trịbạn yêu cầu
                    </div>
                  </div>
                }
              />
            ),
          },
          {
            children: (
              <SwitchCustomize
                type="is_result_other"
                question={questionedit}
                handleUpdateQuestion={handleUpdateQuestion}
                label="Câu trả lời khác"
              />
            ),
          },
        ],
      };
    }

    if (item.id === 3) {
      return {
        type: item.id,
        rules: [
          {
            children: (
              <SwitchCustomize
                type="is_auto_view_show"
                question={questionedit}
                handleUpdateQuestion={handleUpdateQuestion}
                label={
                  <div
                    style={{
                      marginTop: 10,
                    }}
                  >
                    <div>
                      Tự động chọn các câu trả lời đang hiển thị và qua câu tiếp
                      theo
                    </div>
                    <br />
                    <div
                      style={{
                        color: "#666",
                      }}
                    >
                      Bật tính năng này hệ thống sẽ tự động chọn hết các câu trả
                      lời đang hiển thị và chuyển qua câu hỏi tiếp theo nếu số
                      lượng câu trả lời đang hiển thị ít hơn hoặc bằng giá
                      trịbạn yêu cầu
                    </div>
                  </div>
                }
              />
            ),
          },
        ],
      };
    }

    if (item.id === 6) {
      return {
        type: item.id,
        rules: [
          {
            children: (
              <SwitchCustomize
                type="is_auto_view_show"
                question={questionedit}
                handleUpdateQuestion={handleUpdateQuestion}
                label={
                  <div
                    style={{
                      marginTop: 10,
                    }}
                  >
                    <div>
                      Tự động chọn các câu trả lời đang hiển thị và qua câu tiếp
                      theo
                    </div>
                    <br />
                    <div
                      style={{
                        color: "#666",
                      }}
                    >
                      Bật tính năng này hệ thống sẽ tự động chọn hết các câu trả
                      lời đang hiển thị và chuyển qua câu hỏi tiếp theo nếu số
                      lượng câu trả lời đang hiển thị ít hơn hoặc bằng giá
                      trịbạn yêu cầu
                    </div>
                  </div>
                }
              />
            ),
          },
          {
            type: "rating_page",
            children:
              questionedit && handleUpdateQuestion ? (
                <RatingIcon
                  question={questionedit}
                  handleUpdateQuestion={handleUpdateQuestion}
                />
              ) : (
                <></>
              ),
          },
        ],
      };
    }

    return {
      type: item.id,
      rules: [],
    };
  });

  const handleRenderView = useCallback(
    (id: number) => {
      switch (id) {
        case 1:
          return questionedit ? (
            <SingleChoice
              formData={formData}
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;
        case 2:
          return questionedit ? (
            <MultipleChoice
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
              formData={formData}
            />
          ) : null;
        case 3:
          return questionedit ? (
            <SingleSlider
              formData={formData}
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;
        case 4:
          return questionedit ? (
            <RangeSlider
              formData={formData}
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;
        case 5:
          return questionedit ? (
            <SingleInput
              question={questionedit}
              fieldInputTypes={fieldInputTypes}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;
        case 6:
          return questionedit ? (
            <Rating
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;
        case 7:
          return questionedit ? (
            <Ranking
              formData={formData}
              question={questionedit}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          ) : null;

        default:
          return (
            <FormSelectType
              SurveyQuestionTypes={SurveyQuestionTypes}
              handleUpdateQuestion={handleUpdateQuestion}
            />
          );
      }
    },
    [handleUpdateQuestion, questionedit, formData]
  );

  const handleAddQuestion = useCallback(() => {
    if (!isTrigger) return; // Thịnh, nếu lưu thủ công thì không cho thêm câu hỏi
    setFormData((prev: any) => ({
      ...prev,
      Questions: [
        ...prev.Questions,
        {
          ...questionDefault,
          Id: generateUUID(),
          Order: (prev.Questions[prev.Questions.length - 1]?.Order || 0) + 1,
        },
      ],
    }));
    setOrderCurrent(formData?.Questions?.length + 1);
  }, [formData?.Questions?.length, setFormData]);

  const handleChangeQuestion = (Order: number) => {
    setOrderCurrent(Order);
  };

  const handleDeleteQuestion = () => {
    if (!orderCurrent) return;

    const questionToDelete = formData?.Questions.find(
      (item: any) => item.Order === orderCurrent
    );

    if (!questionToDelete) return;

    const affectedQuestions = new Set<number>(); // Track affected questions

    const newQuestions = formData?.Questions.filter(
      (item: any) => item.Order !== orderCurrent
    ).map((item: any, index: number) => {
      const updatedItem = { ...item, Order: index + 1 };

      if (updatedItem.ConfigJson?.JumpLogics?.length > 0) {
        // Filter out jump logics that reference the deleted question
        updatedItem.ConfigJson.JumpLogics =
          updatedItem.ConfigJson.JumpLogics.filter((jumpLogic: any) => {
            const hasConditionReference = jumpLogic.Conditions.some(
              (condition: any) => condition.QuestionId === questionToDelete.Id
            );
            const hasTargetReference =
              jumpLogic.TargetQuestionId === questionToDelete.Id;

            // If question is affected, add to set
            if (hasConditionReference || hasTargetReference) {
              affectedQuestions.add(updatedItem.Order);
            }

            return !hasConditionReference && !hasTargetReference;
          });

        if (updatedItem.ConfigJson.JumpLogics.length === 0) {
          delete updatedItem.ConfigJson.JumpLogics;
        }
      }

      return updatedItem;
    });

    setFormData((prev: any) => ({
      ...prev,
      Questions: newQuestions,
    }));

    // Show single alert if there were any changes
    if (affectedQuestions.size > 0) {
      const affectedQuestionsList = Array.from(affectedQuestions).sort(
        (a, b) => a - b
      );
      toast.warning(
        `Một trong số các khối điều kiện ở các câu ${affectedQuestionsList.join(
          ", "
        )} đã bị xóa, vui lòng kiểm tra lại`
      ); // Thịnh();
    }

    // Update current question selection
    if (orderCurrent > 1) {
      setOrderCurrent(orderCurrent - 1);
    } else if (newQuestions.length > 0) {
      setOrderCurrent(1);
    }
  };

  const handleSwapQuestion = (target: number) => {
    const currentOrder = orderCurrent;
    const targetOrder = target;

    if (currentOrder === targetOrder || !currentOrder || !targetOrder) {
      return; // Không hợp lệ
    }

    const questions = [...formData.Questions];
    const currentIndex = questions.findIndex(
      (item) => item.Order === currentOrder
    );
    const targetIndex = questions.findIndex(
      (item) => item.Order === targetOrder
    );

    if (currentIndex === -1 || targetIndex === -1) {
      return;
    }

    // Lấy ra câu hỏi cần di chuyển
    const [movedQuestion] = questions.splice(currentIndex, 1);
    // Chèn vào vị trí mới
    questions.splice(targetIndex, 0, movedQuestion);

    // Cập nhật lại order cho tất cả câu hỏi
    const newQuestions = questions.map((item, idx) => ({
      ...item,
      Order: idx + 1,
    }));

    setFormData((prev: any) => ({
      ...prev,
      Questions: newQuestions,
    }));

    // Đặt lại orderCurrent là vị trí mới
    setOrderCurrent(targetOrder);
  };

  useEffect(() => {
    if (!formData?.Questions?.length) {
      setFormData((prev: any) => ({
        ...prev,
        Questions: [{ ...questionDefault, Id: generateUUID(), Order: 1 }],
      }));
    }
    //console.log("formData?.Questions?.length: ", formData);
  }, [formData?.Questions?.length, setFormData]);

  const handleUploadImageBase64 = (e: any) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onload = () => {
        const base64String = reader.result;
        handleUpdateQuestion("MainImageBase64", base64String as string);
      };
      reader.onerror = (error) => {
        console.error("Error reading file:", error);
      };
      reader.readAsDataURL(file);
    }
  };

  const ref = useRef<null>(null);

  const [listBackground, setListBackground] = useState<any[]>([]);

  useEffect(() => {
    setListBackground(
      //Thịnh
      // JSON.parse(localStorage.getItem("listBackground") || "[]")
      defaultBackgroundThemes
    );
  }, []);
  //console.log("questionedit: ", questionedit);
  return (
    <div className="question-page flex flex-col h-full">
      <input type="file" hidden ref={ref} onChange={handleUploadImageBase64} />
      <div className="question-content flex flex-1 overflow-hidden relative">
        {isOpenOverlay ? (
          <Overlay
            onClose={() => setIsOpenOverlay(false)}
            onDelete={handleDeleteQuestion}
            onSwap={handleSwapQuestion}
            formData={formData}
            orderCurrent={orderCurrent}
            setIsOpenOverlay={setIsOpenOverlay}
          />
        ) : null}
        <div
          className="question-main flex-1 flex flex-col overflow-y-auto relative"
          style={{
            ...(formData?.ConfigJson?.Background === "image" && {
              backgroundImage: `url(${
                formData?.ConfigJson?.IsUseBackgroundImageBase64 &&
                formData.BackgroundImageBase64
                  ? formData.BackgroundImageBase64
                  : formData?.ConfigJson?.DefaultBackgroundImageId
                  ? listBackground.find(
                      //Thịnh
                      (item) =>
                        item.Id ===
                        formData?.ConfigJson?.DefaultBackgroundImageId
                    )?.MainImageUrl
                  : ""
              })`,
              backgroundSize: "cover",
              backgroundPosition: "center",
              backgroundRepeat: "no-repeat",
              filter: `Brightness(${formData?.ConfigJson.Brightness / 100})`,
              backgroundColor: "transparent",
            }),
            ...(formData?.ConfigJson?.Background === "color_gradient" && {
              background: `linear-gradient(to right, ${formData?.ConfigJson.BackgroundGradient1Color}, ${formData?.ConfigJson.BackgroundGradient2Color})`,
              filter: `Brightness(${formData?.ConfigJson.Brightness / 100})`,
            }),
            ...(formData?.ConfigJson?.Background?.startsWith("#") && {
              backgroundColor: formData?.ConfigJson?.Background,
              filter: `Brightness(${formData?.ConfigJson.Brightness / 100})`,
            }),
          }}
        >
          <div className="question-input-container relative z-10 flex flex-col items-center">
            {questionedit?.MainImageBase64 &&
            questionedit.ConfigJson?.ImageEndQuestion ? (
              <img
                src={questionedit?.MainImageBase64}
                className="rounded-2xl "
                alt=""
              />
            ) : (
              ""
            )}
            <Button
              onClick={() => {
                if (ref.current) {
                  (ref.current as any).click();
                }
              }}
            >
              Upload Image
            </Button>
            <input
              type="text"
              placeholder="Nhập câu hỏi tại đây"
              className="question-title-input"
              style={{
                color: `${formData?.ConfigJson?.TitleColor}`,
              }}
              value={questionedit?.Content || ""}
              onChange={(e) => handleUpdateQuestion("Content", e.target.value)}
            />
            <textarea
              placeholder="Nhập mô tả tại đây"
              rows={2}
              className="question-description-input"
              value={questionedit?.Description || ""}
              style={{
                color: `${formData?.ConfigJson?.ContentColor}`,
              }}
              onChange={(e) =>
                handleUpdateQuestion("Description", e.target.value)
              }
            ></textarea>
          </div>
          <div className="flex justify-center">
            {handleRenderView(questionedit?.QuestionTypeId || 0)}
          </div>
        </div>

        <div className="question-sidebar flex flex-col overflow-y-auto">
          <Sidebar
            securityModes={securityModes} //Thịnh, thêm dòng này
            formData={formData}
            setFormData={setFormData}
            handleUpdateQuestion={handleUpdateQuestion as any}
            question={questionedit as QuestionType}
            listComponent={
              rulesType.find(
                (item) => item.type === questionedit?.QuestionTypeId
              )?.rules as []
            }
          />
        </div>
      </div>

      <div className="question-footer flex items-center">
        <div className="footer-content flex overflow-x-scroll w-full">
          {/* {(formData?.Questions || [])?.map((item: any) => (
            <QuestionItem
              key={item.Order}
              order={item.Order}
              orderCurrent={orderCurrent}
              onChange={handleChangeQuestion}
              onOpenOverlay={() => setIsOpenOverlay(true)}
            />
          ))} */}
          {[...(formData?.Questions || [])]
            .sort((a, b) => a.Order - b.Order) // Sort by Order before mapping
            .map((item: any) => (
              <QuestionItem
                key={item.Order}
                order={item.Order}
                orderCurrent={orderCurrent}
                onChange={handleChangeQuestion}
                onOpenOverlay={() => setIsOpenOverlay(true)}
              />
            ))}
          <div
            className="add-question-btn flex flex-col items-center justify-center"
            onClick={handleAddQuestion}
          >
            <AddCircleIcon fontSize="small" className="add-icon" />
            <span className="add-text">Thêm Câu Hỏi</span>
          </div>
        </div>
      </div>
    </div>
  );
};

export default QuestionPage;

const QuestionItem = ({
  order,
  orderCurrent,
  onChange,
  onOpenOverlay,
}: {
  order: number;
  orderCurrent: number;
  onChange: (order: number) => void;
  onOpenOverlay: () => void;
}) => {
  return (
    <div
      className={`question-item flex flex-col items-center justify-center ${
        order === orderCurrent && "question-active"
      }`}
      onClick={() => onChange(order)}
    >
      <CheckCircleIcon
        fontSize="large"
        className="item-icon"
        onClick={onOpenOverlay}
      />
      <span className="item-text">{order}.</span>
    </div>
  );
};
