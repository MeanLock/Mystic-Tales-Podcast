/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useMemo } from "react";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import "./styles.scss";
import { handleUpdateForm } from "@/app/appSlice";

type Props = {
  isUpdate?: boolean;
  data?: any;
};
const FullTime = ({ data, isUpdate }: Props) => {
  const dispatch = useAppDispatch();
  const isValid = useAppSelector((state) => state.appSlice?.isValid || true);

  const date = useMemo(
    () =>
      data?.ValueJson?.QuestionResponse?.Input?.Value?.split(" ")[0].split(
        "/"
      )[0] || "",
    [data]
  );
  const month = useMemo(
    () =>
      data?.ValueJson?.QuestionResponse?.Input?.Value?.split(" ")[0].split(
        "/"
      )[1] || "",
    [data]
  );
  const year = useMemo(
    () =>
      data?.ValueJson?.QuestionResponse?.Input?.Value?.split(" ")[0].split(
        "/"
      )[2] || "",
    [data]
  );

  const hour = useMemo(
    () =>
      data?.ValueJson?.QuestionResponse?.Input?.Value?.split(" ")[1].split(
        ":"
      )[0] || "",
    [data]
  );
  const minute = useMemo(
    () =>
      data?.ValueJson?.QuestionResponse?.Input?.Value?.split(" ")[1].split(
        ":"
      )[1] || "",
    [data]
  );

  const hadnleUpdate = useCallback(
    (type: "date" | "month" | "year" | "hour" | "minute", value: string) => {
      if (!isUpdate || !isValid) return;
      let result = "";
      if (type === "date") {
        result = `${value}/${month}/${year} ${hour}:${minute}`;
      }
      if (type == "month") {
        result = `${date}/${value}/${year} ${hour}:${minute}`;
      }
      if (type === "year") {
        result = `${date}/${month}/${value} ${hour}:${minute}`;
      }
      if (type === "hour") {
        result = `${date}/${month}/${year} ${value}:${minute}`;
      }
      if (type === "minute") {
        result = `${date}/${month}/${year} ${hour}:${value}`;
      }
      dispatch(
        handleUpdateForm({
          idChoose: data?.ValueJson?.QuestionContent?.Id,
          type: 9,
          value: result,
        })
      );
    },
    [
      data?.ValueJson?.QuestionContent?.Id,
      date,
      dispatch,
      hour,
      isUpdate,
      minute,
      month,
      year,
    ]
  );
  return (
    <div className="">
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
    </div>
  );
};

export default FullTime;
