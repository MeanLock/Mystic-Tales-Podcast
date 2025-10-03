/* eslint-disable @typescript-eslint/no-explicit-any */
import ChevronRightIcon from "@mui/icons-material/ChevronRight";
import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type Dispatch,
  type SetStateAction,
} from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { handleSetIsValid, setSurveyData } from "@/app/appSlice";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { routesMap } from "../../../routes/routes";
import type { SurveyType } from "@/core/types/tools";
import Action from "../../molecules/action/Action";
import Slide from "../slide/Slide";
import "./styles.scss";
import { useUpdateSurveyPro } from "@/core/services/CreateSurveyTool/EditSession/update-survey-pro";
import { v4 as uuidv4 } from "uuid";
import { NextButton } from "../../atoms/Buttons/NextButton";

type JumpLogic = {
  Conditions: {
    QuestionId: string;
    Conjunction: "AND" | "OR" | null;
    Operator: string;
    OptionId: string;
    CompareValue: number;
  }[];
  TargetQuestionId: string;
};

type Props = {
  dataResponse: SurveyType | null;
  setIsRefetch: Dispatch<SetStateAction<boolean>>;
  takingSubject: string;
};


const HandleSlide = ({ dataResponse, setIsRefetch, takingSubject }: Props) => {
  const [current, setCurrent] = useState("");
  const [currentIndex, setCurrentIndex] = useState(0);
  const [searchParams] = useSearchParams();
  const taken_subject = useMemo(
    () => searchParams.get("taking_subject"),
    [searchParams]
  );

  // Get the survey data from the app state
  const surveyData = useAppSelector((state) => state.appSlice.surveyData);
  //console.log("surveyData", surveyData);
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const { id } = useParams();
  // Function to update the survey data on the server
  const { mutate } = useUpdateSurveyPro({
    mutationConfig: {
      // Do nothing on success
      onSuccess() { },
    },
  });

  /**
   * Handle the next button click
   */
  const handleNext = useCallback(() => {
    // Set the question as valid
    dispatch(handleSetIsValid(true));

    // If the random number is greater than 900, refetch the survey data
    if (Math.random() * 1000 > 900) {
      setIsRefetch((prev) => !prev);
    }

    // If there is no survey data or the current question is not valid, return
    if (!surveyData?.SurveyResponses) return;

    // Get the current question
    const question = surveyData?.SurveyResponses[currentIndex];

    // If the question is not valid, return
    // if (!question?.IsValid) return;

    // Get the config json of the current question
    const configJson = question?.ValueJson?.QuestionContent
      ?.ConfigJson as Record<string, any>;
    // Get the jump logics of the current question
    const jump: JumpLogic[] = (configJson?.JumpLogics || []) as JumpLogic[];
    //console.log("jump nè", configJson);
    // If there are jump logics, go through each logic and check if the conditions are met
    if (jump.length) {
      for (const logic of jump) {
        let result: boolean | null = null;
        //console.log("logic", logic);
        // Go through each condition of the logic
        for (let i = 0; i < logic.Conditions.length; i++) {
          const cond = logic.Conditions[i];

          // Get the question response of the condition
          const q = surveyData.SurveyResponses.find(
            (item) => item.ValueJson.QuestionContent.Id === cond.QuestionId
          );
          const questionResponse = q?.ValueJson?.QuestionResponse as Record<
            string,
            any
          >;

          let isValid = false;

          // Check if the condition is met
          if (
            q?.ValueJson.QuestionContent.QuestionTypeId === 1 ||
            q?.ValueJson.QuestionContent.QuestionTypeId === 2
          ) {
            const selected = questionResponse?.SingleChoice;
            if (cond.Operator === "Chọn") {
              isValid = selected === cond.OptionId;
            }
            if (cond.Operator === "Không Chọn") {
              isValid = selected !== cond.OptionId;
            }
          }

          if (q?.ValueJson.QuestionContent.QuestionTypeId === 6) {
            const value = questionResponse?.Input?.Value;
            if (cond.Operator === "=") {
              isValid = value === cond.CompareValue;
            }
            if (cond.Operator === ">=") {
              isValid = value >= cond.CompareValue;
            }
            if (cond.Operator === "<=") {
              isValid = value <= cond.CompareValue;
            }
            if (cond.Operator === ">") {
              isValid = value > cond.CompareValue;
            }
            if (cond.Operator === "<") {
              isValid = value < cond.CompareValue;
            }
          }

          // If it's the first condition, set the result to the isValid
          if (i === 0) {
            result = isValid;
          } else {
            // If it's not the first condition, use the conjunction to combine the results
            if (cond.Conjunction === "AND") {
              result = result && isValid;
            } else if (cond.Conjunction === "OR") {
              result = result || isValid;
            } else {
              result = result && isValid;
            }
          }
        }
        if (result) {
          let targetIndex = -1;
          if (logic.TargetQuestionId === null) {
            handleEnd();
            return;
          } else {
            for (let i = surveyData.SurveyResponses.length - 1; i >= 0; i--) {
              if (
                surveyData.SurveyResponses[i].ValueJson.QuestionContent.Id ===
                logic.TargetQuestionId
              ) {
                targetIndex = i;
                break;
              }
            }
          }


          if (targetIndex !== -1) {
            const target = surveyData.SurveyResponses[targetIndex];
            setCurrent(target.ValueJson.QuestionContent.Id);
            setCurrentIndex(targetIndex);
            return;
          }
        }
      }
    }

    if (
      currentIndex === -1 ||
      currentIndex === surveyData.SurveyResponses.length - 1
    )
      return;

    const nextIndex = currentIndex + 1;
    const nextQuestion = surveyData.SurveyResponses[nextIndex];
    setCurrentIndex(nextIndex);
    setCurrent(nextQuestion?.ValueJson?.QuestionContent?.Id ?? "");
  }, [surveyData, currentIndex]);

  const handleEnd = useCallback(() => {
    if (!surveyData || !id) return;
    const dataBuider = {
      ...surveyData,
      surveyId: id,
      taken_subject: taken_subject,
      SurveyResponses: surveyData?.SurveyResponses?.filter(
        (i) => !i.parentId
      ).map((i) => ({
        IsValid: i.IsValid,
        ValueJson: {
          ...i.ValueJson,
          QuestionContent: {
            Id: i.ValueJson.QuestionContent.Id,
            MainImageUrl: i.ValueJson.QuestionContent.MainImageUrl || null,
            QuestionTypeId: i.ValueJson.QuestionContent.QuestionTypeId,
            Content: i.ValueJson.QuestionContent.Content,
            Description: i.ValueJson.QuestionContent.Description,
            ConfigJson: (() => {
              // eslint-disable-next-line @typescript-eslint/no-unused-vars
              const { JumpLogics, ...rest } = (i.ValueJson.QuestionContent
                .ConfigJson || {}) as {
                  [key: string]: any;
                };
              return rest;
            })(),
            Options: i.ValueJson.QuestionContent.Options,
          },
        },
      })),
    };
    if (taken_subject === "Preview") {
      navigate(routesMap.EndSurveyCustomer.replace("/:id/end", `/${id}/end`));
      return;
    }
    mutate(dataBuider);
    navigate(routesMap.EndSurveyCustomer.replace("/:id/end", `/${id}/end`)); // Thịnh, đổi đường dẫn từ /survey/end/:id sang /survey/:id/end
  }, [id, mutate, navigate, surveyData, taken_subject]);

  if (!surveyData?.SurveyResponses?.length) {
    return (
      <Start
        dataResponse={dataResponse}
        setCurrent={setCurrent}
        setCurrentIndex={setCurrentIndex}
        takingSubject={takingSubject}
      />
    );
  }

  return (
    <div className="w-[60%]">
      <Slide currentQuestionId={current} />
      <Action
        onNext={handleNext}
        currentIndex={currentIndex}
        onEnd={handleEnd}
      />
    </div>
  );
};

export default HandleSlide;

const Start = ({
  dataResponse,
  setCurrent,
  setCurrentIndex,
  takingSubject,
}: {
  dataResponse: SurveyType | null;
  setCurrent: Dispatch<SetStateAction<string>>;
  setCurrentIndex: Dispatch<SetStateAction<number>>;
  takingSubject: string;
}) => {
  const data = useAppSelector((state) => state.appSlice.infoSurvey);
  const dispatch = useAppDispatch();

  const buttonBgColor = useMemo(
    () => data?.ConfigJson?.ButtonBackgroundColor || "#007bff",
    [data?.ConfigJson?.ButtonBackgroundColor]
  );
  const buttonTextColor = useMemo(
    () => data?.ConfigJson?.ButtonContentColor || "#ffffff",
    [data?.ConfigJson?.ButtonContentColor]
  );

  const handleStart = () => {
    //console.log("Handle Start...");
    if (dataResponse) {
      let dataStore = (dataResponse?.Questions || []).map(
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        (i: any) => ({
          isEnd: false,
          isNext: false,
          IsValid: true,
          ValueJson: {
            QuestionContent: {
              Id: i?.Id || null,
              MainImageUrl: i?.MainImageUrl || "",
              QuestionTypeId: i?.QuestionTypeId || 0,
              Content: i?.Content || "",
              Description: i?.Description || "",
              ConfigJson: i?.ConfigJson || {},
              Options: i?.Options || [],
              TimeLimit: i?.TimeLimit || 0,
              ...(i?.IsVoiced && {
                SpeechText: i?.SpeechText || "",
              }),
              IsVoiced: i?.IsVoiced || false,
            },
            QuestionResponse: {
              Input: null,
              Range: null,
              Ranking: null,
              SingleChoice: null,
              MultipleChoice: null,
              SpeechText: null,
            },
          },
        })
      );

      if (dataStore.length > 5) {
        const duplicateCount = Math.floor(dataStore.length * 0.2);
        const getRandomUniqueIndices = (maxIndex: number, count: number) => {
          const indices = new Set<number>();
          while (indices.size < count && indices.size < maxIndex) {
            indices.add(Math.floor(Math.random() * maxIndex));
          }
          return Array.from(indices);
        };
        const duplicateIndices = getRandomUniqueIndices(
          dataStore.length,
          duplicateCount
        );

        const duplicatedItems = duplicateIndices.map(
          (index) => dataStore[index]
        );
        const newData = duplicatedItems.map((i, indexChild) => {
          const duplicatedQuestion = {
            ...i,
            parentId: i?.ValueJson?.QuestionContent?.Id,
            ValueJson: {
              ...i.ValueJson,
              QuestionContent: {
                ...i?.ValueJson?.QuestionContent,
                Id: uuidv4(), // Tạo ID mới
              },
              QuestionResponse: {
                Input: null,
                Range: null,
                Ranking: null,
                SingleChoice: null,
                MultipleChoice: null,
                SpeechText: null, // Reset dữ liệu câu trả lời
              },
            },
          };

          // Nếu là câu hỏi cuối cùng trong danh sách duplicate
          if (indexChild === duplicatedItems.length - 1) {
            return {
              ...duplicatedQuestion,
              isUnPost: true,
              isEnd: true, // Đánh dấu là câu hỏi kết thúc
            };
          }

          // Các câu hỏi khác trong danh sách duplicate
          return duplicatedQuestion;
        });
        dataStore = [...dataStore, ...newData];
      }
      //console.log("dataStore >>>", dataStore);
      // Set isEnd: true for the last question of the entire survey
      if (dataStore.length > 0) {
        dataStore[dataStore.length - 1] = {
          ...dataStore[dataStore.length - 1],
          isEnd: true,
        };
      }

      dispatch(
        setSurveyData({
          taken_subject: takingSubject,
          InvalidReason: "",
          SurveyResponses: dataStore,
        })
      );

      if (dataResponse?.Questions?.length && dataResponse?.Questions[0]?.Id) {
        setCurrent(dataResponse?.Questions[0]?.Id);
        setCurrentIndex(0);
      }
    }
  };
  return (
    <div className="w-full flex flex-col items-center gap-5">
      <div className="w-1/2 flex items-center justify-center bg-white/10 backdrop-blur-md border border-white/20 rounded-xl shadow-lg p-6">
        <p
          className="text-[32px] text-center font-bold"
          style={{ color: data?.ConfigJson?.TitleColor || "#FFFFFF" }}
        >
          {data?.Title}
        </p>
      </div>
      <div className="w-8/12 flex items-center justify-center bg-white/10 backdrop-blur-md border border-white/20 rounded-xl shadow-lg p-6">
        <p
          style={{ color: data?.ConfigJson?.ContentColor || "#CCCCCC" }}
          className="text-[20px] text-center"
        >
          {data?.Description}
        </p>
      </div>

      {/* <button
        onClick={handleStart}
        className="startpage-btn group cursor-pointer"
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
      </button> */}

      <div onClick={handleStart} className="flex items-center justify-center">
        <NextButton
          {...(() => {
            if (buttonBgColor.startsWith("linear-gradient")) {
              // dùng regex tách màu
              const matches = buttonBgColor.match(/#(?:[0-9a-fA-F]{3}){1,2}/g);
              return {
                bgColor1: matches?.[0] || "#ffffff",
                bgColor2: matches?.[1] || null,
              };
            } else {
              return {
                bgColor1: buttonBgColor,
                bgColor2: null,
              };
            }
          })()}
          textColor={buttonTextColor}
          label="Bắt Đầu"
        />
      </div>
    </div>
  );
};
