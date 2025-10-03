/* eslint-disable react-hooks/exhaustive-deps */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { handleSetIsValid, handleUpdateSpeechText } from "@/app/appSlice";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import MultiChooseSlide from "./MultiChooseSlide";
import RankingSlide from "./RankingSlide";
import RangeSlideSlide from "./RangeSlideSlide";
import RatingSlide from "./RatingSlide";
import SingleChooseSlide from "./SingleChooseSlide";
import SingleInputSlide from "./SingleInputSlide";
import SingleSliderSlide from "./SingleSliderSlide";

type Props = {
  currentQuestionId: string;
};

const Slide = ({ currentQuestionId }: Props) => {
  const surveyData = useAppSelector((state) => state.appSlice.surveyData);
  const config = useAppSelector((state) => state.appSlice.infoSurvey);

  const dispatch = useAppDispatch();
  const data = useMemo(
    () =>
      (surveyData?.SurveyResponses || []).find(
        (i) => i.ValueJson.QuestionContent.Id === currentQuestionId
      ),
    [currentQuestionId, surveyData?.SurveyResponses]
  );

  const [timer, setTimer] = useState(-1);
  //const refAudio = useRef<SpeechRecognition | null>(null);
  const [isActiveAudio, setIsActiveAudio] = useState<boolean>(false);
  const [isTimerInitialized, setIsTimerInitialized] = useState(false);
  const [inputText, setInputText] = useState<string>("");

  useEffect(() => {
    setInputText((data?.ValueJson.QuestionContent as any)?.SpeechText || "");
    dispatch(handleSetIsValid(true));
    const timeLimit = (data?.ValueJson.QuestionContent as any)?.TimeLimit || 0;
    setTimer(timeLimit);
    setIsTimerInitialized(true);
    {
      // eslint-disable-next-line react-hooks/exhaustive-deps
    }
  }, [currentQuestionId]);

  const isValid = useAppSelector((state) => state.appSlice.isValid);

  //   useEffect(() => {
  //     // https://developer.mozilla.org/en-US/docs/Web/API/Web_Speech_API/Using_the_Web_Speech_API
  //     const SpeechRecognition =
  //       window.SpeechRecognition || window.webkitSpeechRecognition;
  //     const recognition = new SpeechRecognition();
  //     refAudio.current = recognition;
  //     recognition.continuous = false;
  //     recognition.lang = "vi";
  //     recognition.interimResults = false;
  //     recognition.maxAlternatives = 1;

  //     recognition.onresult = function (event: any) {
  //       setIsActiveAudio(false);
  //       setInputText(event.results[0][0].transcript);
  //     };

  //     recognition.onspeechend = function () {
  //       recognition.stop();
  //     };
  //     recognition.onspeechend = function (e: any) {
  //       console.log(e);
  //     };

  //     recognition.onerror = function (event: any) {
  //       console.log(event);
  //       console.log(event.error);
  //     };
  //   }, []);

  // LỖI !!!!!!!!!!!!!!!
  useEffect(() => {
    if (typeof window === "undefined") return;

    // const SpeechRecognition =
    //   window.SpeechRecognition || window.webkitSpeechRecognition;

    // if (!SpeechRecognition) {
    //   console.warn("Trình duyệt không hỗ trợ SpeechRecognition.");
    //   return;
    // }

    // const recognition = new SpeechRecognition();
    // refAudio.current = recognition;

    // recognition.continuous = false;
    // recognition.lang = "vi-VN";
    // recognition.interimResults = false;
    // recognition.maxAlternatives = 1;

    // recognition.onresult = (event: SpeechRecognitionEvent) => {
    //   setIsActiveAudio(false);
    //   setInputText(event.results[0][0].transcript);
    // };

    // recognition.onspeechend = () => {
    //   recognition.stop();
    //   console.log("Người dùng ngưng nói");
    // };

    // recognition.onerror = (event: any) => {
    //   console.error("Lỗi nhận diện:", event.error);
    // };

    // return () => {
    //   recognition.stop(); // đảm bảo cleanup khi unmount
    // };
  }, []);

  useEffect(() => {
    if (inputText) {
      dispatch(
        handleUpdateSpeechText({
          text: inputText,
          questionId: data?.ValueJson.QuestionContent.Id || "",
        })
      );
    }
  }, [inputText]);

  useEffect(() => {
    if (timer > 0 && isTimerInitialized) {
      const interval = setTimeout(() => {
        setTimer(timer - 1);
      }, 1000);
      return () => clearTimeout(interval);
    } else if (timer === 0 && isTimerInitialized) {
      setIsTimerInitialized(false);
      setTimer(-1);
      dispatch(handleSetIsValid(false));
    }
  }, [timer, isTimerInitialized]);

  const handleToggleMicrophone = () => {
    if (isValid) {
      alert("Vui lòng trả lời câu hỏi trước khi sử dụng micro!");
      return;
    }

    // if (!refAudio.current) return;

    // if (isActiveAudio) {
    //   refAudio.current.stop();
    //   setIsActiveAudio(false);
    // } else {
    //   refAudio.current.start();
    //   setIsActiveAudio(true);
    // }
  };

  const handleRender = useCallback(() => {
    switch (data?.ValueJson.QuestionContent.QuestionTypeId) {
      case 1:
        return (
          <SingleChooseSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 2:
        return (
          <MultiChooseSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 3:
        return (
          <SingleSliderSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 4:
        return (
          <RangeSlideSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 5:
        return (
          <SingleInputSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 6:
        return (
          <RatingSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      case 7:
        return (
          <RankingSlide
            data={{
              ...data,
              ValueJson: {
                ...data?.ValueJson,
                QuestionContent: {
                  ...data?.ValueJson.QuestionContent,
                  SpeechText: inputText,
                },
              },
            }}
          />
        );
      default:
        return <div className="">Chưa chọn type</div>;
    }
  }, [data]);


  return (
    <div className="">
      {/* Timer overlay - chỉ hiển thị khi timer > 0 */}
      {timer > 0 && (
        <div className="fixed top-6 right-6 z-50">
          <div className="bg-gradient-to-r from-blue-500 to-purple-600 text-white px-6 py-4 rounded-2xl shadow-2xl border border-white/20 backdrop-blur-sm">
            <div className="flex items-center space-x-3">
              <div className="w-3 h-3 bg-red-400 rounded-full animate-pulse"></div>
              <div className="text-center">
                <div className="text-sm font-medium opacity-90">
                  Có thể trả lời sau {timer}s
                </div>
                <div className="text-2xl font-bold">{timer}s</div>
              </div>
            </div>
          </div>
        </div>
      )}

      <div className="">
        <div className="mb-[20px] flex flex-col items-center gap-3">
          <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-xl shadow-lg p-2">
            <p
              className="text-center text-[28px]"
              style={{
                color: config?.ConfigJson?.TitleColor || "#000",
              }}
            >
              {data?.ValueJson?.QuestionContent?.Content || ""}
            </p>
          </div>
          {data?.ValueJson?.QuestionContent?.Description !== "" && (
            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-xl shadow-lg p-2">
              <p
                className="text-center text-[15px] italic"
                style={{
                  color: config?.ConfigJson?.ContentColor || "#000",
                }}
              >
                {data?.ValueJson?.QuestionContent?.Description || ""}
              </p>
            </div>
          )}

          {data?.ValueJson?.QuestionContent?.MainImageUrl &&
          !data?.ValueJson?.QuestionContent?.MainImageUrl.includes(
            "unknown.jpg"
          ) ? (
            <img src={data?.ValueJson?.QuestionContent?.MainImageUrl} alt="" />
          ) : null}
        </div>
      </div>

      {/* Microphone Button */}
      {(data?.ValueJson.QuestionContent as any).IsVoiced && (
        <div className="flex justify-center mb-4">
          <button
            onClick={handleToggleMicrophone}
            className={`w-16 h-16 rounded-full flex items-center justify-center transition-all duration-300 shadow-lg`}
            style={{
              background: isActiveAudio
                ? "#ef4444" // red-500
                : config?.ConfigJson?.ButtonBackgroundColor || "#3b82f6", // blue-500 default
              boxShadow: isActiveAudio
                ? "0 10px 15px -3px rgba(239, 68, 68, 0.5)" // shadow-red-500/50
                : `0 10px 15px -3px ${
                    config?.ConfigJson?.ButtonBackgroundColor || "#3b82f6"
                  }80`, // shadow với opacity
            }}
          >
            <svg
              className="w-8 h-8 text-white"
              fill="currentColor"
              viewBox="0 0 24 24"
            >
              <path d="M12 14c1.66 0 3-1.34 3-3V5c0-1.66-1.34-3-3-3S9 3.34 9 5v6c0 1.66 1.34 3 3 3z" />
              <path d="M17 11c0 2.76-2.24 5-5 5s-5-2.24-5-5H5c0 3.53 2.61 6.43 6 6.92V21h2v-3.08c3.39-.49 6-3.39 6-6.92h-2z" />
            </svg>
          </button>
        </div>
      )}

      {/* Display transcribed text */}
      {/* {inputText &&
                (data?.ValueJson?.QuestionContent as any)?.IsVoiced && (
                    <div className="text-center mb-4 p-4 bg-gray-100 rounded-lg">
                        <p className="text-gray-700 font-medium">
                            Văn bản ghi âm:
                        </p>
                        <p className="text-gray-900">{inputText}</p>
                    </div>
                )} */}

      {handleRender()}
    </div>
  );
};

export default Slide;
