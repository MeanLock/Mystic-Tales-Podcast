import "./styles.scss";
import { Button, MenuItem, Select } from "@mui/material";
import ArrowForwardIcon from "@mui/icons-material/ArrowForward";
import DeleteOutlineIcon from "@mui/icons-material/DeleteOutline";
import CloseIcon from "@mui/icons-material/Close";
import { useEffect, useState } from "react";
import { ArrowBack } from "@mui/icons-material";
import type { SurveyType } from "@/core/types/tools";

type Props = {
  onClose: () => void;
  onDelete: () => void;
  onSwap: (target: number) => void;
  formData: SurveyType;
  orderCurrent: number;
  setIsOpenOverlay: React.Dispatch<React.SetStateAction<boolean>>;
};

const Overlay = ({
  onClose,
  onDelete,
  onSwap,
  formData,
  orderCurrent,
  setIsOpenOverlay,
}: Props) => {
  const [isViewConfirm, setIsViewConfirm] = useState(false);
  const [isSwap, setIsSwap] = useState(false);
  //console.log(">>>>>>>>>Form Data mà Overlay nhận được: ", formData);
  return (
    <div className="overlay absolute top-0 right-0 left-0 bottom-0 bg-white/80 z-50 flex flex-col items-center justify-end p-4">
      {isViewConfirm ? (
        <Confirm
          formData={formData}
          onDelete={() => {
            onDelete();
            setIsOpenOverlay(false);
            setIsViewConfirm(false);
          }}
          onBack={() => {
            setIsViewConfirm(false);
            if (isSwap) {
              setIsSwap(false);
            }
          }}
          onSwap={(target: number) => {
            onSwap(target);
            setIsOpenOverlay(false);
            setIsViewConfirm(false);
          }}
          isSwap={isSwap}
          orderCurrent={orderCurrent}
        />
      ) : (
        <div className="overlay-content flex flex-col space-y-6 gap-4">
          <Button
            variant="contained"
            endIcon={<ArrowForwardIcon />}
            className="overlay-button di-chuyen"
            onClick={() => {
              setIsSwap(true);
              setIsViewConfirm(true);
            }}
          >
            Di chuyển
          </Button>
          <Button
            variant="contained"
            startIcon={<DeleteOutlineIcon />}
            className="overlay-button xoa-cau-hoi"
            onClick={() => setIsViewConfirm(true)}
          >
            Xoá câu hỏi
          </Button>
          <Button
            variant="contained"
            endIcon={<CloseIcon />}
            className="overlay-button bo-chon"
            onClick={onClose}
          >
            Bỏ chọn
          </Button>
        </div>
      )}
      <div className="overlay-text mt-6 text-sm text-gray-700 italic">
        Giữ nút Shift để chọn hàng loạt
      </div>
    </div>
  );
};

export default Overlay;

const Confirm = ({
  onDelete,
  onBack,
  onSwap,
  isSwap,
  formData,
  orderCurrent,
}: {
  onDelete: () => void;
  onBack: () => void;
  onSwap: (target: number) => void;
  isSwap: boolean;
  formData: SurveyType;
  orderCurrent: number;
}) => {
  const [value, setValue] = useState(orderCurrent);
  //console.log(">>>>>>>>>>>>>>>>>>>>>Form Data: ", formData);
  const handleVerify = () => {
    if (isSwap) {
      onSwap(value);
    } else {
      onDelete();
    }
  };
  useEffect(() => {
    setValue(orderCurrent);
  }, [orderCurrent]);
  return (
    <div className="overlay-content flex flex-col space-y-6 gap-4">
      {formData?.Questions?.length && isSwap ? (
        <Select
          value={value}
          onChange={(e) => {
            setValue(Number(e.target.value));
          }}
        >
          {/* {formData.Questions.map((item: any, index: number) => {
            return (
              <MenuItem value={item.Order} key={item.Order}>
                {item.Order}
              </MenuItem>
            );
          })} */}
          {[...(formData?.Questions || [])]
            .sort((a, b) => a.Order - b.Order) // Sort by Order before mapping
            .map((item: any) => (
              <MenuItem value={item.Order} key={item.Order}>
                {item.Order}
              </MenuItem>
            ))}
        </Select>
      ) : null}

      <Button
        variant="contained"
        endIcon={isSwap ? undefined : <DeleteOutlineIcon />}
        className="overlay-button bg-green-400"
        onClick={handleVerify}
      >
        Xác nhận
      </Button>
      {/* <Button
                variant="contained"
                startIcon={<ArrowBack />}
                className="overlay-button bg-orange-200"
                onClick={onBack}
            >
                Quay lại
            </Button> */}
    </div>
  );
};
