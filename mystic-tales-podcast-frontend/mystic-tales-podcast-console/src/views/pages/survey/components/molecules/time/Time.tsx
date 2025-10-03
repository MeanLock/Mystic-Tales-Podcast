/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useMemo } from "react";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import "./styles.scss";
import { handleUpdateForm } from "@/app/appSlice";

type Props = {
  isUpdate?: boolean;
  data?: any;
};
const Time = ({ data, isUpdate }: Props) => {
  const dispatch = useAppDispatch();
  const isValid = useAppSelector((state) => state.appSlice?.isValid || true);

  const hour = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value?.split(":")[0] || "",
    [data]
  );
  const minute = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value?.split(":")[1] || "",
    [data]
  );

  const hadnleUpdate = useCallback(
    (type: "hour" | "minute", value: string) => {
      if (!isUpdate || !isValid) return;
      let result = "";
      if (type === "hour") {
        result = `${value}/${minute}`;
      }
      if (type == "minute") {
        result = `${hour}/${value}`;
      }

      dispatch(
        handleUpdateForm({
          idChoose: data?.ValueJson?.QuestionContent?.Id,
          type: 8,
          value: result,
        })
      );
      // console.log(">>>", type, value);
    },
    [data?.ValueJson?.QuestionContent?.Id, dispatch, hour, isUpdate, minute]
  );
  return (
    <div className="date-container">
      <div className="date-field">
        <label>Giờ</label>
        <select
          value={hour}
          onChange={(e) => hadnleUpdate("hour", e.target.value)}
        >
          {/* Options for Day */}
          {[...Array(23)].map((_, i) => (
            <option key={i} value={i}>
              {i}
            </option>
          ))}
        </select>
      </div>
      <div className="date-field">
        <label>Phút</label>
        <select
          value={minute}
          onChange={(e) => hadnleUpdate("minute", e.target.value)}
        >
          {/* Options for Month */}
          {[...Array(59)].map((_, i) => (
            <option key={i} value={i}>
              {i}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};

export default Time;
