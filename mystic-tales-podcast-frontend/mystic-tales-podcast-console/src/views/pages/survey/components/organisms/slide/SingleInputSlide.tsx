import { useCallback } from "react";
import { useDispatch } from "react-redux";
import { handleUpdateForm } from "@/app/appSlice";
import { Box, TextField } from "@mui/material";
import Date from "../../molecules/date/Date";
import DateMonth from "../../molecules/date-month/DateMonth";
import Year from "../../molecules/year/Year";
import Time from "../../molecules/time/Time";
import FullTime from "../../molecules/full-time/FullTime";
import { useAppSelector } from "@/app/hooks";
import { HiddenCheck } from "../../molecules/hiddenCheck/HiddenCheck";

interface Props {
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  data: any;
}

const SingleInputSlide = ({ data }: Props) => {

    // console.log(data);
  const isValid = useAppSelector((state) => state.appSlice.isValid);
    const config = useAppSelector((state) => state.appSlice.infoSurvey);
    const dispatch = useDispatch();

    const handleRender = useCallback(() => {
        switch (
            data?.ValueJson?.QuestionContent?.ConfigJson?.FieldInputTypeId
        ) {
            case 1:
                return (
                    <input
                        type="text"
                        placeholder="Vui lòng nhập tại đây"
                        className="text-input w-full px-5 py-3 border-white border-[1px] outline-none rounded-[4px]"
                        // style={{
                        //     color: config?.ConfigJson?.ContentColor || "#000",
                        // }}
                        value={
                            data?.ValueJson?.QuestionResponse?.Input?.Value ||
                            ""
                        }
                        onChange={(e) => {
                           // if (isValid) return;
                            dispatch(
                                handleUpdateForm({
                                    idChoose:
                                        data?.ValueJson?.QuestionContent?.Id,
                                    type: 1,
                                    value: e.target.value || "",
                                })
                            );
                        }}
                    />
                );
            case 2:
                return (
                    <TextField
                        fullWidth
                        multiline
                        rows={4}
                        placeholder="Vui lòng nhập tại đây"
                        // variant="outlined"
                        size="small"
                        className="w-full px-5 py-3 border-white border-[1px] outline-none rounded-[4px]"
                        style={{
                            color: config?.ConfigJson?.ContentColor || "#000",
                        }}
                        value={
                            data?.ValueJson?.QuestionResponse?.Input?.Value ||
                            ""
                        }
                        onChange={(e) => {
                           // if (!isValid) return;
                            dispatch(
                                handleUpdateForm({
                                    idChoose:
                                        data?.ValueJson?.QuestionContent?.Id,
                                    type: 2,
                                    value: e.target.value || "",
                                })
                            );
                        }}
                    />
                    // <Box className="text-area w-full">
                    // </Box>
                );
            case 3:
                return (
                    // <Box className=" w-full">
                    // </Box>
                    <TextField
                        fullWidth
                        placeholder="Nhập email tại đây"
                        className="w-full px-5 py-3 border-white border-[1px] outline-none rounded-[4px]"
                        variant="outlined"
                        size="small"
                        type="email"
                        style={{
                            color: config?.ConfigJson?.ContentColor || "red",
                        }}
                        value={
                            data?.ValueJson?.QuestionResponse?.Input?.Value ||
                            ""
                        }
                        onChange={(e) => {
                            //if (!isValid) return;
                            dispatch(
                                handleUpdateForm({
                                    idChoose:
                                        data?.ValueJson?.QuestionContent?.Id,
                                    type: 3,
                                    value: e.target.value || "",
                                })
                            );
                        }}
                    />
                );
            case 4:
                return (
                    <Box className="number-input w-full">
                        <TextField
                            fullWidth
                            placeholder="Vui lòng nhập số"
                            variant="outlined"
                            size="small"
                            type="number"
                            className="w-full px-5 py-3 border-white border-[1px] outline-none rounded-[4px]"
                            style={{
                                color:
                                    config?.ConfigJson?.ContentColor || "#000",
                            }}
                            value={
                                data?.ValueJson?.QuestionResponse?.Input
                                    ?.Value || ""
                            }
                            onChange={(e) => {
                                //if (!isValid) return;
                                dispatch(
                                    handleUpdateForm({
                                        idChoose:
                                            data?.ValueJson?.QuestionContent
                                                ?.Id,
                                        type: 4,
                                        value: e.target.value || "",
                                    })
                                );
                            }}
                        />
                    </Box>
                );
            case 5:
                return (
                    <Date
                        isUpdate
                        data={data}
                        color={config?.ConfigJson?.ContentColor || "#000"}
                    />
                );
            case 6:
                return <DateMonth isUpdate data={data} />;
            case 7:
                return <Year isUpdate data={data} />;
            case 8:
                return <Time isUpdate data={data} />;
            case 9:
                return <FullTime isUpdate data={data} />;
            default:
                return <div className="">Chưa chọn type</div>;
        }
    }, [data]);

    return (
       <div className="relative w-full">
        {isValid && (
            <div 
                className="absolute inset-0 bg-gray-200/50 z-10 rounded-lg"
                style={{ cursor: 'not-allowed' }}
            />
        )}
        <div className="w-full">
            {handleRender()}
            <HiddenCheck id={data?.ValueJson.QuestionContent.QuestionTypeId} />
        </div>
    </div>
    );

};

export default SingleInputSlide;
