import { loginRequiredAxiosInstance } from "@/core/api/rest-api/config/instances/v2";
import { callAxiosRestApi } from "@/core/api/rest-api/main/api-call";
import { RootState } from "@/redux/root-reducer";
import SurveyTalkLoading from "@/views/components/common/loading";
import { SurveyLayoutHeader } from "@/views/components/layout/survey-tool-layout/SurveyLayoutHeader";
import React, { useEffect, useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { toast } from "react-toastify";
type Props = { children: React.ReactNode };

const MainTemPlate = ({ children }: Props) => {
  // STATES
  const [member, setMember] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(false);

  // REDUX
  const auth = useSelector((state: RootState) => state.auth);

  // HOOKS
  const dispatch = useDispatch();

  // useEffect(() => {
  //   setIsLoading(true);
  //   if (!auth.token) {
  //     toast.error("Bạn cần đăng nhập để thực hiện chức năng này !");
  //     dispatch(clearAuthToken());
  //     setMember(null);
  //     window.location.href = "/login";
  //   } else {
  //     fetch();
  //   }
  // }, []);

  // // FUNCTIONS
  // const fetch = async () => {
  //   try {
  //     const response = await callAxiosRestApi({
  //       instance: loginRequiredAxiosInstance,
  //       method: "get",
  //       url: "User/accounts/me",
  //     });
  //     if (response.success) {
  //       const member = response.data;

  //       const user = {
  //         Id: member.Account.Id,
  //         RoleId: member.Account.Role.Id,
  //         FullName: member.Account.FullName,
  //         Balance: member.Account.Balance,
  //         IsVerified: member.Account.IsVerified,
  //         Xp: member.Account.Xp,
  //         Level: member.Account.Level,
  //         IsFilterSurveyRequired: member.Account.IsFilterSurveyRequired,
  //         LastFilterSurveyTakenAt: member.Account.LastFilterSurveyTakenAt,
  //         MainImageUrl: member.Account.MainImageUrl,
  //         Profile: member.Account.Profile,
  //       };
  //       dispatch(
  //         updateAuthUser({
  //           user: user,
  //         })
  //       );
  //       setMember(user);
  //       setIsLoading(false);
  //     }
  //   } catch (error) {}
  // };

  return (
    <div >
      {isLoading ? (
        <div className=" min-h-screen flex flex-col align-center justify-center ">
          <SurveyTalkLoading />
          <p className="text-2xl font-bold text-center">Đang tải ...</p>
        </div>
      ) : (
        <div>
          {children}
        </div>
      )}
    </div>
  );
};

export default MainTemPlate;
