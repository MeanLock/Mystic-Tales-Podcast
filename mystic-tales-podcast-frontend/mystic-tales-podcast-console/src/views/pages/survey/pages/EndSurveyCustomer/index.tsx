import { useAppSelector } from "@/app/hooks";
import { useNavigate } from "react-router-dom";
import { useGetSurveyDefaultBackgroundThemes } from "@/core/services/CreateSurveyTool/InputDatas/get-default-background-themes";
import { use, useEffect } from "react";
import SurveyTalkLoading from "@/views/components/common/loading";
import { useRefetchUser } from "@/hooks/useRefetchUser";

const EndSurveyCustomer = () => {
  const state = useAppSelector((state) => state);
  // Hooks cập nhật User Informations
  const refetchUser = useRefetchUser();

  //console.log("StateEndSurveyCustomer: ", state);

  const survey = useAppSelector((state) => state.appSlice.surveyData);
  const info = useAppSelector((state) => state.appSlice.infoSurvey);
  const navigate = useNavigate();
  const { data: bgDefaultThemes } = useGetSurveyDefaultBackgroundThemes({});

  if (!bgDefaultThemes || !Array.isArray(bgDefaultThemes)) {
    return <SurveyTalkLoading />;
  }
//console.log("bgDefaultThemes: ", info);
  // const handleNavigateToHome = async () => {
  //   const check = await refetchUser();
  //   if (check) {
  //     navigate("/");
  //   }
  // };

  return (
    <div
      className={`fixed top-0 left-0 w-full h-full bg-white z-50 px-[100px]`}
      style={{
        ...((info as any)?.ConfigJson.Background === "color_gradient"
          ? {
              background: `linear-gradient(${info.ConfigJson.BackgroundGradient1Color}, ${info.ConfigJson.BackgroundGradient2Color})`,
            } // Thịnh , fix hiển thị tất cả các loại background
          : (info as any)?.ConfigJson.Background === "image"
          ? {
              backgroundImage: `url(${info.BackgroundImageUrl})`,
            }
          : (info as any)?.ConfigJson.Background?.startsWith("#")
          ? {
              backgroundColor: info.ConfigJson.Background,
            }
          : {
              backgroundImage: `url(${
                bgDefaultThemes.find(
                  (item: any) =>
                    item.id === info.ConfigJson.DefaultBackgroundImageId
                )?.url
              })`,
            }),
        backgroundSize: "cover",
        backgroundPosition: "center",
        backgroundRepeat: "no-repeat",
        filter: `brightness(${
          info.ConfigJson.Brightness ? info.ConfigJson.Brightness : 100
        }%)`,
      }}
    >
      <div className="w-full h-full flex flex-col items-center justify-center relative z-10">
        {survey && typeof survey.InvalidReason === "string" ? (
          survey.InvalidReason.trim() !== "" ? (
            <>
              {survey.InvalidReason.split(". ").map((i, index) =>
                i.trim() !== "" ? (
                  <div
                    className="w-full px-5 py-3 rounded-[8px] bg-red-500 text-white mb-4"
                    key={index}
                  >
                    {i}
                  </div>
                ) : null
              )}
              <button
                className="mt-4 px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
                onClick={() => navigate("/filter-survey/table")}
              >
                Quay về trang chủ
              </button>
            </>
          ) : (
            <>
              {" "}
              <div
                className=" text-xl font-semibold"
                style={{ color: info.ConfigJson.TitleColor }}
              >
                Cảm ơn bạn đã hoàn thành khảo sát!
              </div>
              <button
                className="mt-4 px-6 py-2  text-white rounded hover:bg-blue-700 transition"
                style={{
                  background: info.ConfigJson.ButtonBackgroundColor,
                  color: info.ConfigJson.ButtonContentColor,
                }}
                onClick={() => navigate("/filter-survey/table")}
              >
                Quay về trang chủ
              </button>
            </>
          )
        ) : (
          <>
            <div className="text-gray-500">
              Không có dữ liệu hoặc lý do kết thúc khảo sát.
            </div>
            <button
              className="mt-4 px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 transition"
              onClick={() => navigate("/")}
            >
              Quay về trang chủ
            </button>
          </>
        )}
      </div>
    </div>
  );
};

export default EndSurveyCustomer;
