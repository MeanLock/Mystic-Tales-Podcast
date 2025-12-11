import { useEffect, useState } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/redux/store";
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
import { hideAlert } from "@/redux/slices/alertSlice/alertSlice";
import { PiSealWarning, PiWarningCircle } from "react-icons/pi";
import { FaRegCircleCheck } from "react-icons/fa6";

const AlertModal = () => {
  const dispatch = useDispatch();
  const {
    isAlert,
    type,
    title,
    description,
    isAutoClose,
    isFunctional,
    functionalButtonText,
    isClosable,
    autoCloseDuration,
    onClickAction,
  } = useSelector((state: RootState) => state.alert);
  const [remainingTime, setRemainingTime] = useState<number | null>(null);

  useEffect(() => {
    if (
      isAlert &&
      !isFunctional &&
      isAutoClose &&
      autoCloseDuration &&
      autoCloseDuration > 0
    ) {
      const autoCloseMs = autoCloseDuration * 1000; // Convert seconds to milliseconds
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
        dispatch(hideAlert());
        setRemainingTime(null);
      }, autoCloseMs);

      return () => {
        clearTimeout(timer);
        clearInterval(interval);
      };
    } else if (!isAlert) {
      setRemainingTime(null);
    }
  }, [isAlert, autoCloseDuration, dispatch]);

  const handleClose = () => {
    dispatch(hideAlert());
  };

  const handleOpenChange = (open: boolean) => {
    // Chỉ đóng khi open = false (user click outside hoặc ESC)
    if (!open) {
      dispatch(hideAlert());
    }
  };

  if (type === "error") {
    return (
      // Error Modal JSX
      <AlertDialog open={isAlert} onOpenChange={handleOpenChange}>
        <AlertDialogContent className="z-9999 bg-black/60 shadow-md backdrop-blur-md border-none">
          <AlertDialogHeader className="flex flex-col items-center justify-center gap-2">
            <IoWarningOutline size={100} className="text-red-400" />
            <AlertDialogTitle className="flex items-center gap-3 text-white text-2xl font-poppins line-clamp-1">
              {title}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-red-100 text-base mt-6">
              {description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6 gap-3">
            {isClosable && (
              <AlertDialogAction
                onClick={handleClose}
                className="bg-gray-600 hover:bg-gray-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `Close (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    ).toLocaleString()}s)`
                  : `Close`}
              </AlertDialogAction>
            )}
            {isFunctional && onClickAction && (
              <AlertDialogAction
                onClick={onClickAction}
                className="bg-red-600 hover:bg-red-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `${functionalButtonText} (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    )}s)`
                  : functionalButtonText}
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }

  if (type === "info") {
    return (
      <AlertDialog open={isAlert} onOpenChange={handleOpenChange}>
        <AlertDialogContent className="z-9999 bg-black/60 shadow-md backdrop-blur-md border-none">
          <AlertDialogHeader className="flex flex-col items-center justify-center gap-2">
            <PiWarningCircle size={100} className="text-blue-400" />
            <AlertDialogTitle className="flex items-center gap-3 text-white text-2xl font-poppins line-clamp-1">
              {title}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-red-100 text-base mt-6">
              {description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6 gap-3">
            {isClosable && (
              <AlertDialogAction
                onClick={handleClose}
                className="bg-gray-600 hover:bg-gray-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `Close (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    ).toLocaleString()}s)`
                  : `Close`}
              </AlertDialogAction>
            )}
            {isFunctional && onClickAction && (
              <AlertDialogAction
                onClick={onClickAction}
                className="bg-blue-400 hover:bg-blue-500 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `${functionalButtonText} (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    )}s)`
                  : functionalButtonText}
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }

  if (type === "success") {
    return (
      <AlertDialog open={isAlert} onOpenChange={handleOpenChange}>
        <AlertDialogContent className="z-9999 bg-black/60 shadow-md backdrop-blur-md border-none">
          <AlertDialogHeader className="flex flex-col items-center justify-center gap-2">
            <FaRegCircleCheck size={100} className="text-mystic-green" />
            <AlertDialogTitle className="flex items-center gap-3 text-white text-2xl font-poppins line-clamp-1">
              {title}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-red-100 text-base mt-6">
              {description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6 gap-3">
            {isClosable && (
              <AlertDialogAction
                onClick={handleClose}
                className="bg-gray-600 hover:bg-gray-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `Close (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    ).toLocaleString()}s)`
                  : `Close`}
              </AlertDialogAction>
            )}
            {isFunctional && onClickAction && (
              <AlertDialogAction
                onClick={onClickAction}
                className="bg-mystic-green hover:bg-mystic-green/80 text-black font-semibold"
              >
                {remainingTime !== null
                  ? `${functionalButtonText} (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    )}s)`
                  : functionalButtonText}
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }

  if (type === "warning") {
    return (
      <AlertDialog open={isAlert} onOpenChange={handleOpenChange}>
        <AlertDialogContent className="z-9999 bg-black/60 shadow-md backdrop-blur-md border-none">
          <AlertDialogHeader className="flex flex-col items-center justify-center gap-2">
            <PiSealWarning size={100} className="text-yellow-400" />
            <AlertDialogTitle className="flex items-center gap-3 text-white text-2xl font-poppins line-clamp-1">
              {title}
            </AlertDialogTitle>
            <AlertDialogDescription className="text-red-100 text-base mt-6">
              {description}
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter className="mt-6 gap-3">
            {isClosable && (
              <AlertDialogAction
                onClick={handleClose}
                className="bg-gray-600 hover:bg-gray-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `Close (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    ).toLocaleString()}s)`
                  : `Close`}
              </AlertDialogAction>
            )}
            {isFunctional && onClickAction && (
              <AlertDialogAction
                onClick={onClickAction}
                className="bg-yellow-600 hover:bg-yellow-700 text-white font-semibold"
              >
                {remainingTime !== null
                  ? `${functionalButtonText} (Auto-close in ${Math.ceil(
                      remainingTime / 1000
                    )}s)`
                  : functionalButtonText}
              </AlertDialogAction>
            )}
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    );
  }
};

export default AlertModal;
