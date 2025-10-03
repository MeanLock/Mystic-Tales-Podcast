/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import useBlocker from "@/hooks/useBlocker";
import { useParams, useSearchParams } from "react-router-dom";
import type { SurveyType } from "@/core/types/tools";
import { useAppDispatch } from "@/app/hooks";
import { handleSetInfoSurvey, handleSetIsValid } from "@/app/appSlice";
import HandleSlide from "../../components/organisms/handleSlide/HandleSlide";
import TurnstileWidget from "@/hooks/useRecapcha";
import { useGetSlide } from "@/core/services/TakingSurvey/get-slide";
import { useGetSurveyDefaultBackgroundThemes } from "@/core/services/CreateSurveyTool/InputDatas/get-default-background-themes";

function SurveyCustomer() {
  const [isVerified, setIsVerified] = useState(false);
  const [isRefetch, setIsRefetch] = useState(false);
  const [searchParams, setSearchParams] = useSearchParams();
  const [dataResponse, setDataResponse] = useState<SurveyType | null>(null);
  const dispatch = useAppDispatch();

  const { id } = useParams();

  const takingSubject = searchParams.get("taking_subject");

  const { data: apiData, isLoading: isLoadingSlide } = useGetSlide({
    id: Number(id) || 0,
    taking_subject: takingSubject ? takingSubject : "Preview",
  });

  const { data: bgDefaultThemes } = useGetSurveyDefaultBackgroundThemes({});
  //console.log("Data: ", apiData);
  useEffect(() => {
    if (apiData && !isLoadingSlide) {
      setDataResponse(apiData);
      dispatch(handleSetInfoSurvey(apiData));

      if (apiData.data?.Version) {
        const currentParams = new URLSearchParams(searchParams);
        currentParams.set("Version", apiData.data.Version.toString());
        setSearchParams(currentParams);
      }
    }
  }, [apiData, dispatch, searchParams, setSearchParams]);

  useEffect(() => {
    if (isVerified) {
      dispatch(handleSetIsValid(false));
    } else {
      dispatch(handleSetIsValid(true));
    }
  }, [isVerified]);

  useBlocker(true);

  if (!dataResponse) return null;

  return (
    <div>
      <TurnstileWidget
        isVerified={isVerified}
        setIsVerified={setIsVerified}
        isRefetch={isRefetch}
      />
      <div
        className={`py-[60px] w-full min-h-[100vh] z-50 flex justify-center items-center`}
        style={{
          ...((dataResponse as any)?.ConfigJson.Background === "color_gradient"
            ? {
                background: `linear-gradient(${dataResponse.ConfigJson.BackgroundGradient1Color}, ${dataResponse.ConfigJson.BackgroundGradient2Color})`,
              } // Thịnh , fix hiển thị tất cả các loại background
            : (dataResponse as any)?.ConfigJson.Background === "image"
            ? {
                backgroundImage: `url(${dataResponse.BackgroundImageUrl})`,
              }
            : (dataResponse as any)?.ConfigJson.Background?.startsWith("#")
            ? {
                backgroundColor: dataResponse.ConfigJson.Background,
              }
            : {
                backgroundImage: `url(${
                  bgDefaultThemes.find(
                    (item: any) =>
                      item.id ===
                      dataResponse.ConfigJson.DefaultBackgroundImageId
                  )?.url
                })`,
              }),
          backgroundSize: "cover",
          backgroundPosition: "center",
          backgroundRepeat: "no-repeat",
          filter: `brightness(${
            dataResponse.ConfigJson.Brightness
              ? dataResponse.ConfigJson.Brightness
              : 100
          }%)`,
        }}
      >
        <div className="w-full h-[100%] flex flex-col items-center justify-center relative z-10">
          <HandleSlide
            setIsRefetch={setIsRefetch}
            dataResponse={dataResponse}
            takingSubject={takingSubject ? takingSubject : "Preview"}
          />
        </div>
      </div>
    </div>
  );
}

export default SurveyCustomer;
