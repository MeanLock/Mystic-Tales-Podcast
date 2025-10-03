/* eslint-disable @typescript-eslint/no-explicit-any */
import { useCallback, useMemo } from "react";
import "./styles.scss";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { handleUpdateForm } from "@/app/appSlice";

type Props = {
  isUpdate?: boolean;
  data?: any;
};
const Year = ({ data, isUpdate }: Props) => {
  const dispatch = useAppDispatch();
  const isValid = useAppSelector((state) => state.appSlice?.isValid || true);

  const year = useMemo(
    () => data?.ValueJson?.QuestionResponse?.Input?.Value || "",
    [data]
  );

  const hadnleUpdate = useCallback(
    (value: string) => {
      if (!isUpdate || !isValid) return;

      dispatch(
        handleUpdateForm({
          idChoose: data?.ValueJson?.QuestionContent?.Id,
          type: 7,
          value: value,
        })
      );
      // console.log(">>>", type, value);
    },
    [data?.ValueJson?.QuestionContent?.Id, dispatch, isUpdate]
  );

  return (
    <div className="date-container">
      <div className="date-field">
        <label>Năm</label>
        <select value={year} onChange={(e) => hadnleUpdate(e.target.value)}>
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

export default Year;
