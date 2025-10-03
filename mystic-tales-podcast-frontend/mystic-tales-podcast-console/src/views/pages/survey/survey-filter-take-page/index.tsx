// "use client";

// import { useState, useEffect } from "react";
// import {
//   Button,
//   Card,
//   CardContent,
//   FormControl,
//   FormControlLabel,
//   Radio,
//   RadioGroup,
//   InputLabel,
//   Select,
//   MenuItem,
//   Typography,
//   CircularProgress,
//   type SelectChangeEvent,
// } from "@mui/material";
// // import { ChevronLeft, ChevronRight, Star } from "lucide-react";
// import ChevronLeftIcon from "@mui/icons-material/ChevronLeft";
// import ChevronRightIcon from "@mui/icons-material/ChevronRight";
// import StarIcon from "@mui/icons-material/Star";
// import "./styles.scss";
// import axios from "axios";
// import { callAxiosRestApi } from "../../../../core/api/rest-api/main/api-call";
// import { loginRequiredAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2";
// import { toast } from "react-toastify";
// import SurveyTalkLoading from "../../../components/common/loading";
// import { useSelector } from "react-redux";
// import type { RootState } from "../../../../redux/rootReducer";
// import type { SurveyTopic } from "../../../../core/types";
// import useBlocker from "../../../../hooks/useBlocker";

// type RatedTopic = {
//   SurveyTopicId: number;
//   FavoriteScore: number;
// };

// type Ward = {
//   name: string;
//   code: number;
//   codename: string;
//   division_type: string;
//   short_codename: string;
// };

// type District = {
//   name: string;
//   code: number;
//   codename: string;
//   division_type: string;
//   short_codename: string;
//   wards: Ward[];
// };

// type Province = {
//   name: string;
//   code: number;
//   codename: string;
//   division_type: string;
//   phone_code: number;
//   districts: District[];
// };

// type Region = {
//   name: string;
//   firstProvinceCode: number;
//   lastProvinceCode: number;
// };

// const RegionList: Region[] = [
//   {
//     name: "Miền Bắc",
//     firstProvinceCode: 1,
//     lastProvinceCode: 37,
//   },
//   {
//     name: "Miền Trung",
//     firstProvinceCode: 38,
//     lastProvinceCode: 60,
//   },
//   {
//     name: "Miền Nam",
//     firstProvinceCode: 62,
//     lastProvinceCode: 96,
//   },
// ];

// const SurveyTopics = [
//   { id: 1, name: "Ẩm Thực" },
//   { id: 2, name: "Giáo Dục" },
//   { id: 3, name: "Sức Khỏe & Thể Dục" },
//   { id: 4, name: "Công Nghệ" },
//   { id: 5, name: "Tài Chính Cá Nhân" },
//   { id: 6, name: "Mua sắm & Thương Mại" },
//   { id: 7, name: "Du Lịch & Giải Trí" },
//   { id: 8, name: "Xã Hội & Hành Vi" },
//   { id: 9, name: "Môi Trường" },
//   { id: 10, name: "Chính Trị & Pháp Luật" },
// ];

// const EducationQuestions = {
//   question: "Trình độ học vấn của bạn?",
//   options: [
//     "Tiểu học",
//     "Trung học cơ sở",
//     "Trung học phổ thông",
//     "Cao học (Cao đẳng, Học nghề)",
//     "Đại học",
//     "Sau đại học (Thạc sĩ, Tiến sĩ, ...)",
//   ],
// };

// const JobFieldQuestions = {
//   question:
//     "Công việc bạn đang làm/Ngành học của bạn liên quan đến lĩnh vực nào?",
//   options: [
//     "Công nghệ thông tin & Trí tuệ nhân tạo",
//     "Y tế & Chăm sóc sức khỏe",
//     "Giáo dục & Đào tạo",
//     "Kinh doanh, Marketing & Thương mại điện tử",
//     "Kỹ thuật, Cơ điện & Tự động hóa",
//     "Tài chính, Ngân hàng & Bảo hiểm",
//     "Luật pháp, Hành chính & Chính phủ",
//     "Nông nghiệp, Lâm nghiệp & Môi trường",
//     "Nghệ thuật, Thiết kế & Truyền thông",
//     "Dịch vụ, Du lịch & Khách sạn - Nhà hàng",
//     "Xây dựng, Kiến trúc & Quy hoạch",
//     "Logistics, Chuỗi cung ứng & Xuất nhập khẩu",
//     "Nhân sự & Quản trị doanh nghiệp",
//     "Tâm lý học, Xã hội học & Công tác xã hội",
//     "Thể thao, Giải trí & Sự kiện",
//     "Ngôn ngữ, Biên phiên dịch & Văn hóa",
//     "Nghiên cứu, Khoa học & Phát triển công nghệ",
//     "Bất động sản & Quản lý tài sản",
//     "An ninh mạng & Viễn thông",
//     "Toán học, Thống kê & Khoa học dữ liệu",
//   ],
// };

// const AverageIncomeQuestions = {
//   question: "Thu nhập trung bình của bạn?",
//   options: [
//     "< 3 triệu / tháng",
//     "3 - 7 triệu/tháng",
//     "7 - 15 triệu/tháng",
//     "15 - 30 triệu/tháng",
//     "> 30 triệu/tháng",
//   ],
// };

// const MaritalStatusQuestions = {
//   question: "Tình trạng hôn nhân của bạn?",
//   options: ["Độc Thân", "Kết Hôn", "Ly Hôn (Ly dị, Ly thân)", "Góa"],
// };

// export default function SurveyFilterPage() {
//   // REDUX
//   const user = useSelector((state: RootState) => state.auth.user);

//   // STATES
//   const [isLoading, setIsLoading] = useState(false);
//   const [isFinished, setIsFinished] = useState(false);
//   const [accountId, setAccountId] = useState("");
//   const [topics, setTopics] = useState<SurveyTopic[] | null>(null);

//   // HOOKS
//   useBlocker(!isFinished);

//   // States for survey data
//   const [currentStep, setCurrentStep] = useState(1);
//   const [isSubmitting, setIsSubmitting] = useState(false);
//   const [EducationLevel, setEducationLevel] = useState("");
//   const [JobField, setJobField] = useState("");
//   const [AverageIncome, setAverageIncome] = useState("");
//   const [MaritalStatus, setMaritalStatus] = useState("");
//   const [CountryRegion, setCountryRegion] = useState("");
//   const [ProvinceCode, setProvinceCode] = useState(0);
//   const [DistrictCode, setDistrictCode] = useState(0);
//   const [WardCode, setWardCode] = useState(0);
//   const [SurveyTopicFavorites, setSurveyTopicFavorites] = useState<
//     RatedTopic[]
//   >([]);

//   // Mock address data (in real app, this would come from API)
//   const [addressList, setAddressList] = useState<Province[]>([]);
//   const [availableProvinces, setAvailableProvinces] = useState<Province[]>([]);
//   const [availableDistricts, setAvailableDistricts] = useState<District[]>([]);
//   const [availableWards, setAvailableWards] = useState<Ward[]>([]);

//   // Initialize topic ratings
//   useEffect(() => {
//     setIsFinished(false);
//     setIsLoading(true);
//     fetch();
//   }, []);

//   const fetch = async () => {
//     try {
//       const response = await callAxiosRestApi({
//         instance: loginRequiredAxiosInstance,
//         method: "get",
//         url: "User/accounts/me",
//       });
//       if (response.success) {
//         // SET CÁC THÔNG TIN CẦN ĐIỀN ĐỂ CHECK XEM CÓ GIÁ TRỊ CHƯA
//         setAccountId(response.data.Account.Id);
//         setCountryRegion(response.data.Account.Profile.CountryRegion || "");
//         setMaritalStatus(response.data.Account.Profile.MaritalStatus || "");
//         setAverageIncome(response.data.Account.Profile.AverageIncome || "");
//         setEducationLevel(response.data.Account.Profile.EducationLevel || "");
//         setJobField(response.data.Account.Profile.JobField || "");
//         setProvinceCode(response.data.Account.Profile.ProvinceCode || 0);
//         setDistrictCode(response.data.Account.Profile.DistrictCode || 0);
//         setWardCode(response.data.Account.Profile.WardCode || 0);

//         // GỌI API LẤY ĐỊA CHỈ
//         await axios
//           .get("https://provinces.open-api.vn/api/?depth=3")
//           .then((response) => {
//             setAddressList(response.data); // Dữ liệu các tỉnh/thành
//           })
//           .catch((error) => {
//             console.error("Lỗi khi gọi API", error.message);
//           });

//         // SET TOPICS CHO USER CHỌN
//         const topicList = SurveyTopics;
//         setTopics(topicList);

//         // SAU KHI LẤY XONG HẾT DỮ LIỆU SET IsLoading thành false
//         setIsLoading(false);
//       } else {
//         toast.error("Lỗi khi fetch thông tin cá nhân của bạn");
//       }
//     } catch (error) {
//       console.log("Error while fetching data/me: ", error);
//     }
//   };

//   // Handle region selection
//   const handleRegionSelect = (regionName: string, addressData?: Province[]) => {
//     setCountryRegion(regionName);
//     const dataToUse = addressData || addressList;
//     const selectedRegion = RegionList.find((r) => r.name === regionName);
//     if (selectedRegion && dataToUse.length > 0) {
//       const provinces = dataToUse.filter(
//         (p) =>
//           p.code >= selectedRegion.firstProvinceCode &&
//           p.code <= selectedRegion.lastProvinceCode
//       );
//       setAvailableProvinces(provinces);
//     }
//     // Only reset if this is a new selection (not initial load)
//     if (!addressData) {
//       setProvinceCode(0);
//       setDistrictCode(0);
//       setWardCode(0);
//       setAvailableDistricts([]);
//       setAvailableWards([]);
//     }
//   };

//   // Handle province selection
//   const handleProvinceSelect = (
//     provinceCode: string,
//     addressData?: Province[]
//   ) => {
//     const code = Number.parseInt(provinceCode);
//     setProvinceCode(code);
//     const dataToUse = addressData || addressList;
//     const provincesToSearch = addressData ? dataToUse : availableProvinces;
//     const selectedProvince = provincesToSearch.find((p) => p.code === code);
//     if (selectedProvince) {
//       setAvailableDistricts(selectedProvince.districts);
//     }
//     // Only reset if this is a new selection (not initial load)
//     if (!addressData) {
//       setDistrictCode(0);
//       setWardCode(0);
//       setAvailableWards([]);
//     }
//   };

//   // Handle district selection
//   const handleDistrictSelect = (
//     districtCode: string,
//     addressData?: Province[]
//   ) => {
//     const code = Number.parseInt(districtCode);
//     setDistrictCode(code);
//     const selectedDistrict = availableDistricts.find((d) => d.code === code);
//     if (selectedDistrict) {
//       setAvailableWards(selectedDistrict.wards);
//     }
//     // Only reset if this is a new selection (not initial load)
//     if (!addressData) {
//       setWardCode(0);
//     }
//   };

//   // Handle topic rating
//   // Handle topic rating (phiên bản ngắn gọn)
//   const handleTopicRating = (topicId: number, score: number) => {
//     setSurveyTopicFavorites((prev) => {
//       const existingItem = prev.find((item) => item.SurveyTopicId === topicId);

//       if (existingItem) {
//         // Cập nhật nếu đã tồn tại
//         return prev.map((item) =>
//           item.SurveyTopicId === topicId
//             ? { ...item, FavoriteScore: score }
//             : item
//         );
//       } else {
//         // Thêm mới nếu chưa tồn tại
//         return [...prev, { SurveyTopicId: topicId, FavoriteScore: score }];
//       }
//     });
//   };

//   // Check if step is completed
//   const isStepCompleted = (stepNumber: number) => {
//     switch (stepNumber) {
//       case 1:
//         return EducationLevel !== "";
//       case 2:
//         return JobField !== "";
//       case 3:
//         return AverageIncome !== "";
//       case 4:
//         return MaritalStatus !== "";
//       case 5:
//         return (
//           CountryRegion !== "" &&
//           ProvinceCode > 0 &&
//           DistrictCode > 0 &&
//           WardCode > 0
//         );
//       case 6:
//         return (
//           SurveyTopicFavorites.every((item) => item.FavoriteScore > 0) &&
//           SurveyTopicFavorites.length === 10
//         );
//       default:
//         return false;
//     }
//   };

//   // Navigation functions
//   const canGoNext = () => {
//     return isStepCompleted(currentStep);
//   };

//   const handleNext = () => {
//     if (canGoNext() && currentStep < 6) {
//       setCurrentStep(currentStep + 1);
//     }
//   };

//   const handlePrevious = () => {
//     if (currentStep > 1) {
//       setCurrentStep(currentStep - 1);
//     }
//   };

//   const handleSubmit = async () => {
//     try {
//       setIsSubmitting(true);
//       const response = await callAxiosRestApi({
//         instance: loginRequiredAxiosInstance,
//         method: "put",
//         url: `User/accounts/${accountId}/update-profile`,
//         data: {
//           AccountProfile: {
//             CountryRegion: CountryRegion,
//             MaritalStatus: MaritalStatus,
//             AverageIncome: AverageIncome,
//             EducationLevel: EducationLevel,
//             JobField: JobField,
//             ProvinceCode: ProvinceCode,
//             DistrictCode: DistrictCode,
//             WardCode: WardCode,
//           },
//           SurveyTopicFavorites: SurveyTopicFavorites,
//         },
//       });
//       if (response.success) {
//         setIsFinished(true);
//         window.location.href = "/";
//       }
//     } catch (error) {
//       toast.error("Có lỗi xảy ra khi lưu thông tin!");
//     } finally {
//       setIsSubmitting(false);
//     }
//   };

//   const steps = [
//     { number: 1, label: "Trình độ học vấn" },
//     { number: 2, label: "Nghề nghiệp" },
//     { number: 3, label: "Thu nhập cá nhân" },
//     { number: 4, label: "Tình trạng hôn nhân" },
//     { number: 5, label: "Vị trí địa lý hiện tại" },
//     { number: 6, label: "Mức độ yêu thích" },
//   ];

//   const renderStepContent = () => {
//     switch (currentStep) {
//       case 1:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               {EducationQuestions.question}
//             </Typography>
//             <FormControl component="fieldset" className="options-form">
//               <RadioGroup
//                 value={EducationLevel}
//                 onChange={(e) => setEducationLevel(e.target.value)}
//               >
//                 <div className="options-grid">
//                   {EducationQuestions.options.map((option, index) => (
//                     <div key={index} className="option-item">
//                       <FormControlLabel
//                         value={option}
//                         control={<Radio />}
//                         label={option}
//                         className="option-label"
//                       />
//                     </div>
//                   ))}
//                 </div>
//               </RadioGroup>
//             </FormControl>
//           </div>
//         );

//       case 2:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               {JobFieldQuestions.question}
//             </Typography>
//             <FormControl component="fieldset" className="options-form">
//               <RadioGroup
//                 value={JobField}
//                 onChange={(e) => setJobField(e.target.value)}
//               >
//                 <div className="options-grid gap-3">
//                   {JobFieldQuestions.options.map((option, index) => (
//                     <div key={index} className="option-item">
//                       <FormControlLabel
//                         value={option}
//                         control={<Radio />}
//                         label={option}
//                         className="option-label"
//                       />
//                     </div>
//                   ))}
//                 </div>
//               </RadioGroup>
//             </FormControl>
//           </div>
//         );

//       case 3:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               {AverageIncomeQuestions.question}
//             </Typography>
//             <FormControl component="fieldset" className="options-form">
//               <RadioGroup
//                 value={AverageIncome}
//                 onChange={(e) => setAverageIncome(e.target.value)}
//               >
//                 <div className="options-grid">
//                   {AverageIncomeQuestions.options.map((option, index) => (
//                     <div key={index} className="option-item">
//                       <FormControlLabel
//                         value={option}
//                         control={<Radio />}
//                         label={option}
//                         className="option-label"
//                       />
//                     </div>
//                   ))}
//                 </div>
//               </RadioGroup>
//             </FormControl>
//           </div>
//         );

//       case 4:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               {MaritalStatusQuestions.question}
//             </Typography>
//             <FormControl component="fieldset" className="options-form">
//               <RadioGroup
//                 value={MaritalStatus}
//                 onChange={(e) => setMaritalStatus(e.target.value)}
//               >
//                 <div className="options-grid">
//                   {MaritalStatusQuestions.options.map((option, index) => (
//                     <div key={index} className="option-item">
//                       <FormControlLabel
//                         value={option}
//                         control={<Radio />}
//                         label={option}
//                         className="option-label"
//                       />
//                     </div>
//                   ))}
//                 </div>
//               </RadioGroup>
//             </FormControl>
//           </div>
//         );

//       case 5:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               Thông tin địa chỉ của bạn
//             </Typography>
//             <div className="address-form">
//               <div className="form-group">
//                 <Typography variant="subtitle1" className="form-label">
//                   Miền sinh sống
//                 </Typography>
//                 <FormControl component="fieldset">
//                   <RadioGroup
//                     value={CountryRegion}
//                     onChange={(e) => handleRegionSelect(e.target.value)}
//                   >
//                     <div className="region-options gap-5 w-full flex items-center justify-around">
//                       {RegionList.map((region, index) => (
//                         <div key={index} className="option-item">
//                           <FormControlLabel
//                             value={region.name}
//                             control={<Radio />}
//                             label={region.name}
//                             className="option-label"
//                           />
//                         </div>
//                       ))}
//                     </div>
//                   </RadioGroup>
//                 </FormControl>
//               </div>

//               {CountryRegion && (
//                 <div className="form-group">
//                   <FormControl fullWidth>
//                     <InputLabel>Tỉnh/Thành phố</InputLabel>
//                     <Select
//                       value={ProvinceCode.toString()}
//                       label="Tỉnh/Thành phố"
//                       onChange={(e: SelectChangeEvent) =>
//                         handleProvinceSelect(e.target.value)
//                       }
//                     >
//                       {availableProvinces.map((province) => (
//                         <MenuItem
//                           key={province.code}
//                           value={province.code.toString()}
//                         >
//                           {province.name}
//                         </MenuItem>
//                       ))}
//                     </Select>
//                   </FormControl>
//                 </div>
//               )}

//               {ProvinceCode > 0 && (
//                 <div className="form-group">
//                   <FormControl fullWidth>
//                     <InputLabel>Quận/Huyện</InputLabel>
//                     <Select
//                       value={DistrictCode.toString()}
//                       label="Quận/Huyện"
//                       onChange={(e: SelectChangeEvent) =>
//                         handleDistrictSelect(e.target.value)
//                       }
//                     >
//                       {availableDistricts.map((district) => (
//                         <MenuItem
//                           key={district.code}
//                           value={district.code.toString()}
//                         >
//                           {district.name}
//                         </MenuItem>
//                       ))}
//                     </Select>
//                   </FormControl>
//                 </div>
//               )}

//               {DistrictCode > 0 && (
//                 <div className="form-group">
//                   <FormControl fullWidth>
//                     <InputLabel>Phường/Xã</InputLabel>
//                     <Select
//                       value={WardCode.toString()}
//                       label="Phường/Xã"
//                       onChange={(e: SelectChangeEvent) =>
//                         setWardCode(Number.parseInt(e.target.value))
//                       }
//                     >
//                       {availableWards.map((ward) => (
//                         <MenuItem key={ward.code} value={ward.code.toString()}>
//                           {ward.name}
//                         </MenuItem>
//                       ))}
//                     </Select>
//                   </FormControl>
//                 </div>
//               )}
//             </div>
//           </div>
//         );

//       case 6:
//         return (
//           <div className="step-content">
//             <Typography variant="h5" className="question-title">
//               Đánh giá mức độ yêu thích các chủ đề
//             </Typography>
//             <Typography variant="body2" className="rating-subtitle">
//               Vui lòng đánh giá từ 1-5 sao (5 sao là thích nhất)
//             </Typography>
//             <div className="topics-rating">
//               {SurveyTopics.map((topic) => {
//                 const currentRating =
//                   SurveyTopicFavorites.find(
//                     (item) => item.SurveyTopicId === topic.id
//                   )?.FavoriteScore || 0;
//                 return (
//                   <div key={topic.id} className="topic-rating-item">
//                     <div className="topic-name">{topic.name}</div>
//                     <div className="rating-stars">
//                       {[1, 2, 3, 4, 5].map((star) => (
//                         <button
//                           key={star}
//                           type="button"
//                           className={`star-button ${
//                             star <= currentRating ? "active" : ""
//                           }`}
//                           onClick={() => handleTopicRating(topic.id, star)}
//                         >
//                           <StarIcon className="star-icon" />
//                         </button>
//                       ))}
//                     </div>
//                   </div>
//                 );
//               })}
//             </div>
//           </div>
//         );

//       default:
//         return null;
//     }
//   };

//   return (
//     <div className="w-full">
//       {isLoading ? (
//         <div className="w-full h-[90vh] flex flex-col items-center justify-center gap-4">
//           <SurveyTalkLoading />
//           <p className="font-bold text-2xl">
//             Chờ chút nha, tui đang kiếm câu hỏi dành cho bạn!
//           </p>
//         </div>
//       ) : (
//         <div className="survey-filter-page">
//           <Card className="survey-card">
//             <CardContent className="survey-content">
//               <div className="survey-header flex flex-col">
//                 <Typography variant="h4" className="survey-title">
//                   Cập Nhật Thông Tin Cá Nhân Của Bạn
//                 </Typography>

//                 {/* Progress Steps */}
//                 <div className="progress-steps">
//                   {steps.map((step, index) => (
//                     <div key={step.number} className="step-wrapper">
//                       <div
//                         className={`step-circle ${
//                           isStepCompleted(step.number) ? "completed" : ""
//                         } ${currentStep >= step.number ? "active" : ""} ${
//                           currentStep === step.number ? "current" : ""
//                         }`}
//                       >
//                         {step.number}
//                       </div>
//                       <div className="step-label">{step.label}</div>
//                       {index < steps.length - 1 && (
//                         <div
//                           className={`step-connector ${
//                             isStepCompleted(step.number) ? "completed" : ""
//                           }`}
//                         />
//                       )}
//                     </div>
//                   ))}
//                 </div>
//               </div>

//               {/* Content Area */}
//               <div className="content-area mt-52">{renderStepContent()}</div>

//               {/* Navigation */}
//               <div className="navigation">
//                 <Button
//                   variant="outlined"
//                   onClick={handlePrevious}
//                   disabled={currentStep === 1 || isSubmitting}
//                   className="nav-button prev-button"
//                   startIcon={<ChevronLeftIcon />}
//                 >
//                   Previous
//                 </Button>

//                 {currentStep < 6 ? (
//                   <Button
//                     variant="contained"
//                     onClick={handleNext}
//                     disabled={!canGoNext() || isSubmitting}
//                     className="nav-button next-button"
//                     endIcon={<ChevronRightIcon />}
//                   >
//                     Next
//                   </Button>
//                 ) : (
//                   <Button
//                     variant="contained"
//                     onClick={handleSubmit}
//                     disabled={!canGoNext() || isSubmitting}
//                     className="nav-button submit-button"
//                     startIcon={
//                       isSubmitting ? (
//                         <CircularProgress size={20} color="inherit" />
//                       ) : null
//                     }
//                   >
//                     {isSubmitting ? "Đang lưu..." : "Lưu thông tin"}
//                   </Button>
//                 )}
//               </div>
//             </CardContent>
//           </Card>
//         </div>
//       )}
//     </div>
//   );
// }
