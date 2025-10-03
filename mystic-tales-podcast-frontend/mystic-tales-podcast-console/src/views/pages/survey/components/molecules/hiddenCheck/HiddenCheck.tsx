import { useCallback } from "react";
import { useAppDispatch } from "@/app/hooks";
import { handleUpdateInvalidReason } from "@/app/appSlice";

type Props = {
  id: string;
};

export const HiddenCheck = ({ id }: Props) => {
  const dispatch = useAppDispatch();

  const handleChange = useCallback(() => {
    if (id) {
      dispatch(handleUpdateInvalidReason({ questionId: id }));
    }
  }, [dispatch, id]);
  return (
    <div className="hidden">
      <input type="checkbox" onChange={handleChange} />
      <input type="text" onChange={handleChange} />
      <select onChange={handleChange}></select>
    </div>
  );
};
