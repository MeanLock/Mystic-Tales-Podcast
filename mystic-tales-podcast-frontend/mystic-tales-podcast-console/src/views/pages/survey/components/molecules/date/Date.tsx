/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useMemo } from "react";
import "./styles.scss";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { handleUpdateForm } from "@/app/appSlice";

type Props = {
  isUpdate?: boolean;
  data?: any;
  color?: string;
};
const Date = ({ isUpdate, data }: Props) => {
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
  const year = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value?.split("/")[2] || "",
    [data]
  );

  const hadnleUpdate = useCallback(
    (type: "date" | "month" | "year", value: string) => {
      if (!isUpdate || !isValid) return;
      let result = "";
      if (type === "date") {
        result = `${value}/${month}/${year}`;
      }
      if (type == "month") {
        result = `${date}/${value}/${year}`;
      }
      if (type === "year") {
        result = `${date}/${month}/${value}`;
      }
      dispatch(
        handleUpdateForm({
          idChoose: data?.ValueJson?.QuestionContent?.Id,
          type: 5,
          value: result,
        })
      );
      // console.log(">>>", type, value);
    },
    [
      data?.ValueJson?.QuestionContent?.Id,
      date,
      dispatch,
      isUpdate,
      isValid,
      month,
      year,
    ]
  );

  return (
    <div className="date-container">
      <div className="date-field">
        <label>Ngày</label>
        <select
          value={date}
          onChange={(e) => hadnleUpdate("date", e.target.value)}
          // style={{ color: color }}
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
      <div className="date-field">
        <label>Năm</label>
        <select
          value={year}
          onChange={(e) => hadnleUpdate("year", e.target.value)}
        >
          {/* Options for Year (example range) */}
          {[...Array(10)].map((_, i) => (
            <option key={2020 + i} value={2020 + i}>
              {2020 + i}
            </option>
          ))}
        </select>
      </div>
    </div>
  );
};

export default Date;
