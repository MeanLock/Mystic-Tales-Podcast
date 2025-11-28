// src/components/GlobalAlert.tsx
import React from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/src/store/store"; // sửa path
import { hideAlert } from "@/src/features/alert/alertSlice";
import { SuccessAlert, ErrorAlert } from "./index"; // file Alert.tsx bạn đã tạo

export const GlobalAlert = () => {
  const dispatch = useDispatch();
  const { visible, type, message, seconds } = useSelector(
    (state: RootState) => state.alert
  );

  if (!type) return null;

  const handleClose = () => {
    dispatch(hideAlert());
  };

  if (type === "success") {
    return (
      <SuccessAlert
        visible={visible}
        message={message}
        seconds={seconds}
        onClose={handleClose}
      />
    );
  }

  return (
    <ErrorAlert
      visible={visible}
      message={message}
      seconds={seconds}
      onClose={handleClose}
    />
  );
};
