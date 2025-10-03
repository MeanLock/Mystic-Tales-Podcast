/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useMemo } from "react";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import "./styles.scss";
import { handleUpdateForm } from "@/app/appSlice";

type Props = {
  isUpdate?: boolean;
  data?: any;
};
const DateMonth = ({ data, isUpdate }: Props) => {
  const dispatch = useAppDispatch();
  const isValid = useAppSelector((state) => state.appSlice?.isValid || true);

  const date = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value?.split("/")[0] || "",
    [data]
  );
  const month = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value?.split("/")[1] || "",
    [data]
  );

  const hadnleUpdate = useCallback(
    (type: "date" | "month", value: string) => {
      if (!isUpdate || !isValid) return;
      let result = "";
      if (type === "date") {
        result = `${value}/${month}`;
      }
      if (type == "month") {
        result = `${date}/${value}`;
      }

      dispatch(
        handleUpdateForm({
          idChoose: data?.ValueJson?.QuestionContent?.Id,
          type: 6,
          value: result,
        })
      );
      // console.log(">>>", type, value);
    },
    [data?.ValueJson?.QuestionContent?.Id, date, dispatch, isUpdate, month]
  );

  return (
    <div className="date-container">
      <div className="date-field">
        <label>Ngày</label>
        <select
          value={date}
          onChange={(e) => hadnleUpdate("date", e.target.value)}
        >
          {/* Options for Day */}
          {[...Array(31)].map((_, i) => (
            <option key={i + 1} value={i + 1}>
              {i + 1}
            </option>
          ))}
        </select>
      </div>
      <div className="date-field">
        <label>Tháng</label>
        <select
          value={month}
          onChange={(e) => hadnleUpdate("month", e.target.value)}
        >
          {/* Options for Month */}
          {[...Array(12)].map((_, i) => (
            <option key={i + 1} value={i + 1}>
              {i + 1}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};

export default DateMonth;
