import { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
import { endError } from "@/redux/slices/errorSlice/errorSlice";
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import { IoWarningOutline } from "react-icons/io5";

const ErrorModal = () => {
  const dispatch = useDispatch();
  const { isError, message, autoClose } = useSelector(
    (state: RootState) => state.error
  );
  const [remainingTime, setRemainingTime] = useState<number | null>(null);

  useEffect(() => {
    if (isError && autoClose !== null && autoClose > 0) {
      const autoCloseMs = autoClose * 1000; // Convert seconds to milliseconds
      setRemainingTime(autoCloseMs);

      const interval = setInterval(() => {
        setRemainingTime((prev) => {
          if (prev === null || prev <= 0) {
            return 0;
          }
          return prev - 100;
        });
      }, 100);

      const timer = setTimeout(() => {
        dispatch(endError());
        setRemainingTime(null);
      }, autoCloseMs);

      return () => {
        clearTimeout(timer);
        clearInterval(interval);
      };
    } else if (!isError) {
      setRemainingTime(null);
    }
  }, [isError, autoClose, dispatch]);

  const handleClose = () => {
    dispatch(endError());
  };

  const handleOpenChange = (open: boolean) => {
    // Chỉ đóng khi open = false (user click outside hoặc ESC)
    if (!open) {
      dispatch(endError());
    }
  };

  return (
    <AlertDialog open={isError} onOpenChange={handleOpenChange}>
      <AlertDialogContent className="bg-gradient-to-br from-red-900/95 to-red-950/95 border-red-500/50 backdrop-blur-md">
        <AlertDialogHeader>
          <AlertDialogTitle className="flex items-center gap-3 text-white text-2xl">
            <IoWarningOutline size={32} className="text-red-400" />
            Error Occurred
          </AlertDialogTitle>
          <AlertDialogDescription className="text-red-100 text-base mt-4">
            {message}
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogAction
            onClick={handleClose}
            className="bg-red-600 hover:bg-red-700 text-white font-semibold"
          >
            {remainingTime !== null
              ? `Close (Auto-close in ${Math.ceil(remainingTime / 1000)}s)`
              : "Close"}
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
};

export default ErrorModal;
