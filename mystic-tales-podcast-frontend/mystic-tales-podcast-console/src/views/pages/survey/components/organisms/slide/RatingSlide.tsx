import { useCallback, useMemo } from "react";
import StarBorderIcon from "@mui/icons-material/StarBorder";
import FavoriteIcon from "@mui/icons-material/Favorite";
import ThumbUpIcon from "@mui/icons-material/ThumbUp";
import FreeBreakfastIcon from "@mui/icons-material/FreeBreakfast";
import CircleIcon from "@mui/icons-material/Circle";
import CheckIcon from "@mui/icons-material/Check";
import RocketLaunchIcon from "@mui/icons-material/RocketLaunch";
import RadioButtonCheckedIcon from "@mui/icons-material/RadioButtonChecked";
import { Box, Typography } from "@mui/material";
import { useDispatch } from "react-redux";
import { handleUpdateRating } from "@/app/appSlice";
import { useAppSelector } from "@/app/hooks";
import { HiddenCheck } from "../../molecules/hiddenCheck/HiddenCheck";

interface Props {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    data: any;
}

const RatingSlide = ({ data }: Props) => {
    const config = useAppSelector((state) => state.appSlice.infoSurvey);
        const isValid = useAppSelector((state) => state.appSlice.isValid);

    const buttonBgColor = useMemo(
        () => config?.ConfigJson?.ButtonBackgroundColor || "#007bff",
        [config?.ConfigJson?.ButtonBackgroundColor]
    );

    // Function để extract màu từ gradient hoặc trả về màu đơn
    const getColorFromGradient = useCallback((colorValue: string) => {
        if (
            colorValue?.startsWith("linear-gradient") ||
            colorValue?.startsWith("radial-gradient")
        ) {
            // Nếu là gradient, extract màu đầu tiên
            const match = colorValue.match(
                /#[0-9a-fA-F]{6}|#[0-9a-fA-F]{3}|rgb\([^)]+\)|rgba\([^)]+\)/
            );
            return match ? match[0] : "#007bff";
        }
        return colorValue;
    }, []);

    const length = useMemo(
        () => data?.ValueJson?.QuestionContent?.ConfigJson?.RatingLength || 0,
        [data]
    );
    const selected = useMemo(
        () => data?.ValueJson?.QuestionResponse?.Input?.Value || 0,
        [data]
    );
    const dispatch = useDispatch();

    const handleRenderIcons = useCallback(
        (isSelected: boolean) => {
            const type =
                data?.ValueJson?.QuestionContent?.ConfigJson?.RatingIcon || "";

            const iconColor = isSelected
                ? getColorFromGradient(buttonBgColor)
                : "#6b7280";

            const iconStyle = {
                color: iconColor,
                cursor: "pointer",
            };

            switch (type) {
                case "FavoriteIcon":
                    return <FavoriteIcon fontSize="large" style={iconStyle} />;
                case "ThumbUpIcon":
                    return <ThumbUpIcon fontSize="large" style={iconStyle} />;
                case "FreeBreakfastIcon":
                    return (
                        <FreeBreakfastIcon fontSize="large" style={iconStyle} />
                    );
                case "CircleIcon":
                    return <CircleIcon fontSize="large" style={iconStyle} />;
                case "CheckIcon":
                    return <CheckIcon fontSize="large" style={iconStyle} />;
                case "RocketLaunchIcon":
                    return (
                        <RocketLaunchIcon fontSize="large" style={iconStyle} />
                    );
                case "RadioButtonCheckedIcon":
                    return (
                        <RadioButtonCheckedIcon
                            fontSize="large"
                            style={iconStyle}
                        />
                    );
                default:
                    return (
                        <StarBorderIcon fontSize="large" style={iconStyle} />
                    );
            }
        },
        [data, buttonBgColor, getColorFromGradient]
    );

    const handleSelect = (index: number) => {
        //if (isValid) return;
        dispatch(
            handleUpdateRating({
                idChoose: data?.ValueJson?.QuestionContent?.Id,
                value: index + 1,
            })
        );
    };

    return (
        <div className="relative"> {/* Add relative positioning */}
        {isValid && (
            <div 
                className="absolute inset-0 bg-gray-200/50 z-10 rounded-lg"
                style={{ cursor: 'not-allowed' }}
            />
        )}
        <Box className="flex justify-center space-x-4">
            {Array.from({ length: length }).map((_, index) => (
                <Box
                    key={index}
                    className="flex flex-col items-center cursor-pointer"
                    onClick={() => handleSelect(index)}
                >
                    {handleRenderIcons(selected >= index + 1)}
                    <Typography
                        variant="body2"
                        style={{
                            color:
                                selected >= index + 1
                                    ? getColorFromGradient(buttonBgColor)
                                    : "#6b7280",
                            marginTop: "4px",
                        }}
                    >
                        {index + 1}
                    </Typography>
                </Box>
            ))}
        </Box>
        <HiddenCheck id={data?.ValueJson.QuestionContent.QuestionTypeId} />
    </div>
    );
};

export default RatingSlide;
