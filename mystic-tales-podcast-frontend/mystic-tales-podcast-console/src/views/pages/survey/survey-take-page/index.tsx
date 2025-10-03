// import { useState, useEffect, useCallback } from "react";
// import { Button } from "@/components/ui/button";
// import { Card } from "@/components/ui/card";
// import { Progress } from "@/components/ui/progress";
// import { Slider } from "@/components/ui/slider";
// import { Textarea } from "@/components/ui/textarea";
// import { Star, Check } from "lucide-react";
// import { toast } from "react-toastify";
// import { useDispatch, useSelector } from "react-redux";
// import type { RootState } from "@/redux/rootReducer";
// import {
//   createRating,
//   getAllRatings,
// } from "@/core/services/survey/platform-rating.service";
// import { useNavigate, useParams, useSearchParams } from "react-router-dom";
// import { SurveyTakingSessionData } from "@/core/mockData/mockData";
// import SurveyTalkLoading from "@/views/components/common/loading";
// import { updateAuthUser } from "@/redux/auth/authSlice";

// // Mock data based on your requirements
// const mockSurveyData = {
//   TakingAllow: true,
//   Message: "Get Session Success!",
//   TakingSession: {
//     Id: 1,
//     RequesterId: 6,
//     Title: "Khảo sát về Biến đổi khí hậu",
//     Description:
//       "Khảo sát này nhằm tìm hiểu nhận thức và hành động của công chúng về biến đổi khí hậu.",
//     SurveyTypeId: 2,
//     SurveyTopicId: 9,
//     SurveySpecificTopicId: 1,
//     Background: "color_gradient",
//     MainImageUrl:
//       "https://i.pinimg.com/736x/60/f6/16/60f616cfaf04b49ead39616a658dd3cf.jpg",
//     BackgroundImageUrl: null,
//     SurveyStatusId: 2,
//     Version: null,
//     MarketSurveyVersionStatusId: null,
//     SecurityModelId: 1,
//     ConfigJson: {
//       BackgroundGradient1Color: "#007CB3",
//       BackgroundGradient2Color: "#FFF7AE",
//       TitleColor: "#FFFFFF",
//       ContentColor: "#FFFFFF",
//       ButtonBackgroundColor: "#3E5DAB",
//       ButtonContentColor: "#FFFFFF",
//       Password: null,
//       Brightness: 100,
//       IsPause: false,
//       SkipStartPage: false,
//       Background: "color_gradient",
//     },
//     Questions: [
//       {
//         Id: 1,
//         QuestionTypeId: 1,
//         Version: null,
//         Content:
//           "Bạn có tin rằng biến đổi khí hậu chủ yếu do con người gây ra không?",
//         Description: null,
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 1,
//         ConfigJson: null,
//         RequiredAnswer: true,
//         Options: [
//           { Id: 1, Content: "Có", Order: 1, MainImageUrl: null },
//           { Id: 2, Content: "Không", Order: 2, MainImageUrl: null },
//           { Id: 3, Content: "Không chắc", Order: 3, MainImageUrl: null },
//         ],
//       },
//       {
//         Id: 2,
//         QuestionTypeId: 2,
//         Version: null,
//         Content:
//           "Bạn thực hiện những hành động nào để giảm lượng khí thải carbon? Chọn tối đa 5 đáp án.",
//         Description: "Bạn có thể chọn nhiều câu hỏi cho đáp án này. Tối đa 5",
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 2,
//         ConfigJson: { MinChoiceCount: 1, MaxChoiceCount: 5 },
//         RequiredAnswer: true,
//         Options: [
//           {
//             Id: 1,
//             Content: "Sử dụng phương tiện giao thông công cộng",
//             Order: 1,
//             MainImageUrl: null,
//           },
//           {
//             Id: 2,
//             Content: "Giảm tiêu thụ thịt",
//             Order: 2,
//             MainImageUrl: null,
//           },
//           { Id: 3, Content: "Tái chế", Order: 3, MainImageUrl: null },
//           {
//             Id: 4,
//             Content: "Sử dụng thiết bị tiết kiệm năng lượng",
//             Order: 4,
//             MainImageUrl: null,
//           },
//           { Id: 5, Content: "Trồng cây", Order: 5, MainImageUrl: null },
//           {
//             Id: 6,
//             Content: "Không thực hiện hành động nào",
//             Order: 6,
//             MainImageUrl: null,
//           },
//         ],
//       },
//       {
//         Id: 3,
//         QuestionTypeId: 3,
//         Version: null,
//         Content:
//           "Trên thang điểm từ 1 đến 10, bạn quan tâm đến tác động của biến đổi khí hậu ở mức độ nào?",
//         Description: "Kéo thanh để chọn giá trị bạn mong muốn",
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 3,
//         ConfigJson: { Min: 1, Max: 10, Step: 1, Unit: "mức độ quan tâm" },
//         RequiredAnswer: true,
//         Options: null,
//       },
//       {
//         Id: 4,
//         QuestionTypeId: 6,
//         Version: null,
//         Content:
//           "Bạn đánh giá thế nào về nỗ lực của chính phủ trong việc chống biến đổi khí hậu?",
//         Description: null,
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 4,
//         ConfigJson: { RatingLength: 5 },
//         RequiredAnswer: true,
//         Options: null,
//       },
//       {
//         Id: 5,
//         QuestionTypeId: 7,
//         Version: null,
//         Content:
//           "Xếp hạng các hành động bảo vệ môi trường mà bạn coi là hiệu quả nhất (từ 1-6)",
//         Description: "Nhấn lần lượt vào các lựa chọn để xếp hạng chúng",
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 5,
//         ConfigJson: null,
//         RequiredAnswer: true,
//         Options: [
//           {
//             Id: 7,
//             Content: "Xử lý hiệu quả rác thải môi trường",
//             Order: 1,
//             MainImageUrl: null,
//           },
//           {
//             Id: 8,
//             Content: "Sử dụng nguyên liệu xanh",
//             Order: 2,
//             MainImageUrl: null,
//           },
//           {
//             Id: 9,
//             Content: "Loại bỏ xe hơi truyền thống",
//             Order: 3,
//             MainImageUrl: null,
//           },
//           {
//             Id: 10,
//             Content: "Tổ chức các buổi tình nguyện dọn rác",
//             Order: 4,
//             MainImageUrl: null,
//           },
//           {
//             Id: 11,
//             Content: "Trồng cây gây rừng",
//             Order: 5,
//             MainImageUrl: null,
//           },
//           {
//             Id: 12,
//             Content: "Bảo vệ hệ sinh thái trong sách đỏ",
//             Order: 6,
//             MainImageUrl: null,
//           },
//         ],
//       },
//       {
//         Id: 6,
//         QuestionTypeId: 5,
//         Version: null,
//         Content:
//           "Bạn nghĩ thế nào về việc tổ chức một buổi Workshop về bảo vệ môi trường?",
//         Description: "Điền cảm nghĩ của bạn nhé!",
//         TimeLimit: 3,
//         IsVoiced: false,
//         Order: 6,
//         ConfigJson: null,
//         RequiredAnswer: true,
//         Options: null,
//       },
//     ],
//   },
// };

// interface Question {
//   Id: string;
//   QuestionTypeId: number;
//   Content: string;
//   Description?: string;
//   TimeLimit: number;
//   ConfigJson?: any;
//   RequiredAnswer: boolean;
//   Options?: Array<{
//     Id: string;
//     Content: string;
//     Order: number;
//     MainImageUrl?: string;
//   }>;
// }

// interface SurveyData {
//   TakingAllow: boolean;
//   Message: string;
//   TakingSession: {
//     Title: string;
//     Description: string;
//     MainImageUrl: string;
//     Background: string;
//     BackgroundImageUrl?: string;
//     ConfigJson: {
//       BackgroundGradient1Color: string;
//       BackgroundGradient2Color: string;
//       TitleColor: string;
//       ContentColor: string;
//       ButtonBackgroundColor: string;
//       ButtonContentColor: string;
//       Background: string;
//     };
//     Questions: Question[];
//   };
// }

// export default function SurveyPage() {
//   const [surveyData, setSurveyData] = useState<SurveyData | null>(null);
//   const [currentStep, setCurrentStep] = useState<
//     "start" | "question" | "complete" | "rating"
//   >("start");
//   const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
//   const [shuffledQuestions, setShuffledQuestions] = useState<Question[]>([]);
//   const [answers, setAnswers] = useState<Record<number, any>>({});
//   const [timeLeft, setTimeLeft] = useState(0);
//   const [canProceed, setCanProceed] = useState(false);
//   const [finalRating, setFinalRating] = useState(0);

//   const [searchParams] = useSearchParams();
//   const id = searchParams.get("id");

//   // HOOKS
//   const dispatch = useDispatch();
//   const navigate = useNavigate();
//   const user = useSelector((state: RootState) => state.auth.user);
//   // Fetch survey data
//   useEffect(() => {
//     const fetchSurveyData = async () => {
//       try {
//         // Simulate API call
//         console.log("ID: ", id);
//         await new Promise((resolve) => setTimeout(resolve, 1000));
//         const surveyFromMock = SurveyTakingSessionData.filter(
//           (s) => s.TakingSession.Id === Number(id)
//         )[0];
//         if (surveyFromMock) {
//           setSurveyData(surveyFromMock);
//           console.log("Survey: ", surveyFromMock);
//           if (!surveyFromMock.TakingAllow) {
//             toast.error("Không được phép làm khảo sát này");
//             // Navigate to /Home would go here
//             navigate("/home");
//             return;
//           } else {
//             const shuffled = [...surveyFromMock.TakingSession.Questions].sort(
//               () => Math.random() - 0.5
//             );
//             setShuffledQuestions(shuffled);
//           }
//         } else {
//           setSurveyData(mockSurveyData);
//           if (!mockSurveyData.TakingAllow) {
//             toast.error("Không được phép làm khảo sát này");
//             // Navigate to /Home would go here
//             navigate("/home");
//             return;
//           } else {
//             const shuffled = [...mockSurveyData.TakingSession.Questions].sort(
//               () => Math.random() - 0.5
//             );
//             setShuffledQuestions(shuffled);
//           }
//         }
//         // Shuffle questions
//       } catch (error) {
//         toast.error("Lỗi khi tải dữ liệu khảo sát");
//       }
//     };

//     fetchSurveyData();
//   }, []);

//   // Timer countdown
//   useEffect(() => {
//     if (currentStep === "question" && timeLeft > 0) {
//       const timer = setTimeout(() => {
//         setTimeLeft(timeLeft - 1);
//       }, 1000);
//       return () => clearTimeout(timer);
//     } else if (currentStep === "question" && timeLeft === 0) {
//       setCanProceed(true);
//     }
//   }, [timeLeft, currentStep]);

//   // Start question timer
//   const startQuestionTimer = useCallback((timeLimit: number) => {
//     setTimeLeft(timeLimit);
//     setCanProceed(false);
//   }, []);

//   // Handle answer change
//   const handleAnswerChange = (questionId: string, answer: any) => {
//     setAnswers((prev) => ({
//       ...prev,
//       [questionId]: answer,
//     }));
//   };

//   // Check if current question is answered
//   const isCurrentQuestionAnswered = () => {
//     const currentQuestion = shuffledQuestions[currentQuestionIndex];
//     if (!currentQuestion) return false;

//     const answer = answers[currentQuestion.Id];
//     if (!answer) return false;

//     switch (currentQuestion.QuestionTypeId) {
//       case 1: // Single choice
//         return answer !== null && answer !== undefined;
//       case 2: // Multiple choice
//         return (
//           Array.isArray(answer) &&
//           answer.length >= (currentQuestion.ConfigJson?.MinChoiceCount || 1)
//         );
//       case 3: // Slider
//       case 4: // Range slider
//         return answer !== null && answer !== undefined;
//       case 5: // Text area
//         return typeof answer === "string" && answer.trim().length > 0;
//       case 6: // Rating
//         return answer > 0;
//       case 7: // Ranking
//         return (
//           Array.isArray(answer) &&
//           answer.length === currentQuestion.Options?.length
//         );
//       default:
//         return false;
//     }
//   };

//   // Start survey
//   const startSurvey = () => {
//     setCurrentStep("question");
//     if (shuffledQuestions.length > 0) {
//       startQuestionTimer(shuffledQuestions[0].TimeLimit);
//     }
//   };

//   // Next question
//   const nextQuestion = () => {
//     if (currentQuestionIndex < shuffledQuestions.length - 1) {
//       const nextIndex = currentQuestionIndex + 1;
//       setCurrentQuestionIndex(nextIndex);
//       startQuestionTimer(shuffledQuestions[nextIndex].TimeLimit);
//     } else {
//       setCurrentStep("complete");
//     }
//   };

//   // Complete survey
//   const completeSurvey = async () => {
//     dispatch(
//       updateAuthUser({
//         Balance: user?.Balance + 2000,
//         Xp: user?.Xp + 50,
//       })
//     );
//     const response = await getAllRatings();
//     if (response) {
//       const check = response.filter((r) => r.userId === user.Id)[0];
//       if (check) {
//         toast.success("Cám ơn bạn đã tham gia khảo sát!");
//         navigate("/home");
//       } else {
//         setCurrentStep("rating");
//       }
//     }
//   };

//   // Handle rating
//   const handleRating = async (rating: number) => {
//     console.log("Chạy hàm");
//     const data = {
//       rating: rating,
//       userId: user?.Id ? user?.Id : "1",
//     };
//     console.log("Gọi hàm với data: ", data);
//     const response = await createRating(data);
//     console.log("Hàm chạy xong: ", response);
//     if (response) {
//       toast.success("Cám ơn bạn đã tham gia khảo sát!");
//       navigate("/home");
//     }
//     // Navigate away or reset survey
//   };

//   if (!surveyData) {
//     return (
//       <div className="min-h-screen flex flex-col items-center justify-center">
//         <SurveyTalkLoading />
//         <p className="font-bold text-black text-2xl">
//           Đang lấy khảo sát cho bạn!
//         </p>
//       </div>
//     );
//   }

//   const { TakingSession } = surveyData;
//   const { ConfigJson } = TakingSession;

//   // Background style
//   const backgroundStyle =
//     TakingSession.Background === "color_gradient"
//       ? {
//           background: `linear-gradient(45deg, ${ConfigJson.BackgroundGradient1Color}, ${ConfigJson.BackgroundGradient2Color})`,
//         }
//       : {
//           backgroundImage: `url(${
//             TakingSession.BackgroundImageUrl || "https://picsum.photos/1200/800"
//           })`,
//           backgroundSize: "cover",
//           backgroundPosition: "center",
//           backgroundRepeat: "no-repeat",
//           height: "100vh",
//           width: "100%",
//         };

//   return (
//     <div
//       className="min-h-screen p-4 flex flex-col items-center justify-center relative"
//       style={backgroundStyle}
//     >
//       {/* Overlay for image background */}
//       {TakingSession.Background === "image" && (
//         <div className="absolute inset-0 bg-black bg-opacity-30" />
//       )}

//       <div className="relative z-10 w-full max-w-4xl">
//         {currentStep === "start" && (
//           <StartPage survey={TakingSession} onStart={startSurvey} />
//         )}

//         {currentStep === "question" && shuffledQuestions.length > 0 && (
//           <QuestionPage
//             question={shuffledQuestions[currentQuestionIndex]}
//             questionNumber={currentQuestionIndex + 1}
//             totalQuestions={shuffledQuestions.length}
//             timeLeft={timeLeft}
//             canProceed={canProceed && isCurrentQuestionAnswered()}
//             answer={answers[shuffledQuestions[currentQuestionIndex].Id]}
//             onAnswerChange={(answer) =>
//               handleAnswerChange(
//                 shuffledQuestions[currentQuestionIndex].Id,
//                 answer
//               )
//             }
//             onNext={nextQuestion}
//             config={ConfigJson}
//           />
//         )}

//         {currentStep === "complete" && (
//           <CompletePage onComplete={completeSurvey} config={ConfigJson} />
//         )}

//         {currentStep === "rating" && (
//           <RatingPage
//             rating={finalRating}
//             onRatingChange={setFinalRating}
//             onSubmit={() => handleRating(finalRating)}
//             config={ConfigJson}
//           />
//         )}
//       </div>
//     </div>
//   );
// }

// // Start Page Component
// function StartPage({ survey, onStart }: { survey: any; onStart: () => void }) {
//   return (
//     <div className="flex flex-col lg:flex-row items-center gap-8 max-w-6xl mx-auto">
//       {/* Image */}
//       <div className="flex-1">
//         <Card className="bg-white/20 backdrop-blur-md border-white/30 p-8">
//           <img
//             src={survey.MainImageUrl}
//             alt="Survey illustration"
//             className="w-full h-auto rounded-lg"
//           />
//         </Card>
//       </div>

//       {/* Content */}
//       <div className="flex-1 space-y-6">
//         <Card className="bg-white/20 backdrop-blur-md border-white/30 p-6">
//           <h1
//             className="text-3xl font-bold text-center mb-4"
//             style={{ color: survey.ConfigJson.TitleColor }}
//           >
//             {survey.Title}
//           </h1>
//         </Card>

//         <Card className="bg-white/20 backdrop-blur-md border-white/30 p-6">
//           <p
//             className="text-lg text-center leading-relaxed"
//             style={{ color: survey.ConfigJson.ContentColor }}
//           >
//             {survey.Description}
//           </p>
//         </Card>

//         <div className="flex justify-center">
//           <Button
//             onClick={onStart}
//             className="px-8 py-3 text-lg font-semibold rounded-lg"
//             style={{
//               backgroundColor: survey.ConfigJson.ButtonBackgroundColor,
//               color: survey.ConfigJson.ButtonContentColor,
//             }}
//           >
//             Vào làm khảo sát
//           </Button>
//         </div>
//       </div>
//     </div>
//   );
// }

// // Question Page Component
// function QuestionPage({
//   question,
//   questionNumber,
//   totalQuestions,
//   timeLeft,
//   canProceed,
//   answer,
//   onAnswerChange,
//   onNext,
//   config,
// }: {
//   question: Question;
//   questionNumber: number;
//   totalQuestions: number;
//   timeLeft: number;
//   canProceed: boolean;
//   answer: any;
//   onAnswerChange: (answer: any) => void;
//   onNext: () => void;
//   config: any;
// }) {
//   const progress = (questionNumber / totalQuestions) * 100;

//   return (
//     <div className="space-y-6">
//       {/* Progress Bar */}
//       <div className="w-full">
//         <Progress value={progress} className="h-2" />
//         <div className="text-center mt-2 text-white text-sm">
//           Câu {questionNumber} / {totalQuestions}
//         </div>
//       </div>

//       {/* Question Card */}
//       <Card className="bg-white/20 backdrop-blur-md border-white/30 p-8">
//         <div className="space-y-6">
//           {/* Question Content */}
//           <div className="text-center">
//             <h2
//               className="text-xl font-semibold mb-4"
//               style={{ color: config.TitleColor }}
//             >
//               {question.Content}
//             </h2>
//             {question.Description && (
//               <p
//                 className="text-sm opacity-80"
//                 style={{ color: config.ContentColor }}
//               >
//                 {question.Description}
//               </p>
//             )}
//           </div>

//           {/* Question Input */}
//           <div className="min-h-[200px] flex items-center justify-center">
//             <QuestionInput
//               question={question}
//               answer={answer}
//               onAnswerChange={onAnswerChange}
//               config={config}
//             />
//           </div>

//           {/* Timer and Next Button */}
//           <div className="flex justify-between items-center">
//             <div className="text-white text-sm">
//               {timeLeft > 0
//                 ? `Thời gian còn lại: ${timeLeft}s`
//                 : "Có thể tiếp tục"}
//             </div>
//             <Button
//               onClick={onNext}
//               disabled={!canProceed}
//               className="px-6 py-2"
//               style={{
//                 backgroundColor: canProceed
//                   ? config.ButtonBackgroundColor
//                   : "#666",
//                 color: config.ButtonContentColor,
//               }}
//             >
//               Next
//             </Button>
//           </div>
//         </div>
//       </Card>
//     </div>
//   );
// }

// // Question Input Component
// function QuestionInput({
//   question,
//   answer,
//   onAnswerChange,
//   config,
// }: {
//   question: Question;
//   answer: any;
//   onAnswerChange: (answer: any) => void;
//   config: any;
// }) {
//   switch (question.QuestionTypeId) {
//     case 1: // Single choice
//       return (
//         <div className="space-y-3 w-full">
//           {question.Options?.map((option) => (
//             <Card
//               key={option.Id}
//               className={`p-4 cursor-pointer transition-all border-2 ${
//                 answer === option.Id
//                   ? "bg-white/30 border-white/50"
//                   : "bg-white/10 border-white/20 hover:bg-white/20"
//               }`}
//               onClick={() => onAnswerChange(option.Id)}
//             >
//               <div className="flex items-center space-x-3">
//                 <div
//                   className={`w-5 h-5 rounded-full border-2 flex items-center justify-center ${
//                     answer === option.Id
//                       ? "border-white bg-white"
//                       : "border-white/50"
//                   }`}
//                 >
//                   {answer === option.Id && (
//                     <Check className="w-3 h-3 text-blue-600" />
//                   )}
//                 </div>
//                 <span className="text-white font-medium">{option.Content}</span>
//               </div>
//             </Card>
//           ))}
//         </div>
//       );

//     case 2: // Multiple choice
//       const selectedOptions = answer || [];
//       const maxChoices = question.ConfigJson?.MaxChoiceCount || 999;

//       return (
//         <div className="space-y-3 w-full">
//           {question.Options?.map((option) => {
//             const isSelected = selectedOptions.includes(option.Id);
//             const canSelect = isSelected || selectedOptions.length < maxChoices;

//             return (
//               <Card
//                 key={option.Id}
//                 className={`p-4 cursor-pointer transition-all border-2 ${
//                   isSelected
//                     ? "bg-white/30 border-white/50"
//                     : canSelect
//                     ? "bg-white/10 border-white/20 hover:bg-white/20"
//                     : "bg-white/5 border-white/10 opacity-50 cursor-not-allowed"
//                 }`}
//                 onClick={() => {
//                   if (!canSelect && !isSelected) return;

//                   const newSelected = isSelected
//                     ? selectedOptions.filter((id: number) => id !== option.Id)
//                     : [...selectedOptions, option.Id];
//                   onAnswerChange(newSelected);
//                 }}
//               >
//                 <div className="flex items-center space-x-3">
//                   <div
//                     className={`w-5 h-5 rounded border-2 flex items-center justify-center ${
//                       isSelected ? "border-white bg-white" : "border-white/50"
//                     }`}
//                   >
//                     {isSelected && <Check className="w-3 h-3 text-blue-600" />}
//                   </div>
//                   <span className="text-white font-medium">
//                     {option.Content}
//                   </span>
//                 </div>
//               </Card>
//             );
//           })}
//           <div className="text-center text-white/70 text-sm">
//             Đã chọn: {selectedOptions.length} / {maxChoices}
//           </div>
//         </div>
//       );

//     case 3: // Single slider
//       const sliderValue = answer || question.ConfigJson?.Min || 1;

//       return (
//         <div className="w-full space-y-4">
//           <div className="text-center text-white text-sm mb-2">
//             Kéo thanh để chọn giá trị bạn mong muốn
//           </div>
//           <div className="px-4">
//             <Slider
//               value={[sliderValue]}
//               onValueChange={(value) => onAnswerChange(value[0])}
//               min={question.ConfigJson?.Min || 1}
//               max={question.ConfigJson?.Max || 10}
//               step={question.ConfigJson?.Step || 1}
//               className="w-full"
//             />
//             <div className="flex justify-between text-white/70 text-sm mt-2">
//               <span>{question.ConfigJson?.Min || 1}</span>
//               <span className="font-bold text-white text-lg">
//                 {sliderValue}
//               </span>
//               <span>{question.ConfigJson?.Max || 10}</span>
//             </div>
//             <div className="text-center text-white/70 text-sm mt-2">
//               ({question.ConfigJson?.Unit || "điểm"})
//             </div>
//           </div>
//         </div>
//       );

//     case 5: // Text area
//       return (
//         <div className="w-full">
//           <Textarea
//             value={answer || ""}
//             onChange={(e) => onAnswerChange(e.target.value)}
//             placeholder="Nhập câu trả lời của bạn..."
//             className="min-h-[120px] bg-white/20 border-white/30 text-white placeholder:text-white/50"
//             rows={5}
//           />
//         </div>
//       );

//     case 6: // Rating
//       const rating = answer || 0;
//       const ratingLength = question.ConfigJson?.RatingLength || 5;

//       return (
//         <div className="flex justify-center space-x-2">
//           {Array.from({ length: ratingLength }, (_, i) => (
//             <Star
//               key={i}
//               className={`w-12 h-12 cursor-pointer transition-colors ${
//                 i < rating
//                   ? "fill-yellow-400 text-yellow-400"
//                   : "text-white/50 hover:text-yellow-400"
//               }`}
//               onClick={() => onAnswerChange(i + 1)}
//             />
//           ))}
//         </div>
//       );

//     case 7: // Ranking
//       const rankings = answer || [];

//       return (
//         <div className="space-y-3 w-full">
//           {question.Options?.map((option) => {
//             const rank =
//               rankings.findIndex((id: number) => id === option.Id) + 1;

//             return (
//               <Card
//                 key={option.Id}
//                 className={`p-4 cursor-pointer transition-all border-2 ${
//                   rank > 0
//                     ? "bg-white/30 border-white/50"
//                     : "bg-white/10 border-white/20 hover:bg-white/20"
//                 }`}
//                 onClick={() => {
//                   if (rank > 0) {
//                     // Remove from ranking
//                     const newRankings = rankings.filter(
//                       (id: number) => id !== option.Id
//                     );
//                     onAnswerChange(newRankings);
//                   } else {
//                     // Add to ranking
//                     const newRankings = [...rankings, option.Id];
//                     onAnswerChange(newRankings);
//                   }
//                 }}
//               >
//                 <div className="flex items-center space-x-3">
//                   <div
//                     className={`w-8 h-8 rounded-full border-2 flex items-center justify-center ${
//                       rank > 0
//                         ? "border-white bg-white text-blue-600"
//                         : "border-white/50"
//                     }`}
//                   >
//                     {rank > 0 ? rank : ""}
//                   </div>
//                   <span className="text-white font-medium">
//                     {option.Content}
//                   </span>
//                 </div>
//               </Card>
//             );
//           })}
//           <div className="text-center text-white/70 text-sm">
//             Nhấn vào các lựa chọn để xếp hạng (đã xếp: {rankings.length})
//           </div>
//         </div>
//       );

//     default:
//       return <div className="text-white">Loại câu hỏi không được hỗ trợ</div>;
//   }
// }

// // Complete Page Component
// function CompletePage({
//   onComplete,
//   config,
// }: {
//   onComplete: () => void;
//   config: any;
// }) {
//   return (
//     <div className="text-center space-y-6">
//       <Card className="bg-white/20 backdrop-blur-md border-white/30 p-8">
//         <h2
//           className="text-3xl font-bold mb-6"
//           style={{ color: config.TitleColor }}
//         >
//           🎉 Chúc mừng bạn đã hoàn thành khảo sát!
//         </h2>

//         <div className="space-y-4 mb-8">
//           <div className="text-xl" style={{ color: config.ContentColor }}>
//             <div className="mb-2">
//               Số điểm thưởng nhận được:{" "}
//               <span className="font-bold text-yellow-400">2,000 points</span>
//             </div>
//             <div>
//               Số kinh nghiệm nhận được:{" "}
//               <span className="font-bold text-green-400">50 XP</span>
//             </div>
//           </div>
//         </div>

//         <Button
//           onClick={onComplete}
//           className="px-8 py-3 text-lg font-semibold"
//           style={{
//             backgroundColor: config.ButtonBackgroundColor,
//             color: config.ButtonContentColor,
//           }}
//         >
//           Xác nhận thoát
//         </Button>
//       </Card>
//     </div>
//   );
// }

// // Rating Page Component
// function RatingPage({
//   rating,
//   onRatingChange,
//   onSubmit,
//   config,
// }: {
//   rating: number;
//   onRatingChange: (rating: number) => void;
//   onSubmit: () => void;
//   config: any;
// }) {
//   return (
//     <div className="text-center">
//       <Card className="bg-white/20 backdrop-blur-md border-white/30 p-8">
//         <h3
//           className="text-2xl font-bold mb-6"
//           style={{ color: config.TitleColor }}
//         >
//           Bạn đánh giá thế nào về trang web?
//         </h3>

//         <div className="flex justify-center space-x-2 mb-8">
//           {Array.from({ length: 5 }, (_, i) => (
//             <Star
//               key={i}
//               className={`w-12 h-12 cursor-pointer transition-colors ${
//                 i < rating
//                   ? "fill-yellow-400 text-yellow-400"
//                   : "text-white/50 hover:text-yellow-400"
//               }`}
//               onClick={() => onRatingChange(i + 1)}
//             />
//           ))}
//         </div>

//         <Button
//           onClick={onSubmit}
//           disabled={rating === 0}
//           className="px-6 py-2"
//           style={{
//             backgroundColor: rating > 0 ? config.ButtonBackgroundColor : "#666",
//             color: config.ButtonContentColor,
//           }}
//         >
//           Gửi đánh giá
//         </Button>
//       </Card>
//     </div>
//   );
// }
