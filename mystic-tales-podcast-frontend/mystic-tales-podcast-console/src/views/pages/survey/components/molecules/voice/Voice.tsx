/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable @typescript-eslint/no-explicit-any */
import { Box, Switch, Typography } from "@mui/material";
import { useCallback, useEffect, useMemo } from "react";
import { toast } from "react-toastify";
import Swal from "sweetalert2";
import type {
    OptionType,
    QuestionType,
    SurveyType,
} from "@/core/types/tools";
import type { RangeSliderConfigJsonType } from "../../organisms/RangeSlider/RangeSlider";

interface SwitchCustomizeProps {
    label: React.ReactNode;
    question: any;
    securityModes: any; // Thịnh, thêm dòng này
    isPro: boolean;
    setFormData: React.Dispatch<React.SetStateAction<SurveyType>>;
    handleUpdateQuestion: (
        key: keyof QuestionType | any,
        value:
            | string
            | number
            | boolean
            | OptionType[]
            | Record<string, string | number>
            | RangeSliderConfigJsonType
            | Record<string, unknown>
    ) => void;
}
//Thịnh, trang này copy lại đi,handle chuyển về basic và advance thì tắt voice
const Voice = ({
    label,
    securityModes, // Thịnh, thêm dòng này
    isPro,
    question,
    setFormData,
    handleUpdateQuestion,
}: SwitchCustomizeProps) => {
    const checked = useMemo(() => {
        return question?.IsVoiced || false;
    }, [question]);
    useEffect(() => {
        if (!isPro && checked) {
            handleUpdateQuestion("IsVoiced", false);
        }
    }, [isPro, checked, handleUpdateQuestion]);
    const handleChange = useCallback(() => {
        if (isPro) {
            handleUpdateQuestion("IsVoiced", !checked);
            if (!isPro) {
                setFormData((prev: any) => ({ ...prev, SecurityModeId: 3 }));
            }
            if (!checked && !isPro) {
                toast("Đã cập nhật Chế độ bảo mật thành Pro");
            }
            return;
        }

        // Thịnh, tạo hàm để lấy securityModes description và name
        const generateSecurityModesHTML = () => {
            if (!securityModes || !Array.isArray(securityModes)) {
                return '<p style="font-size: 14px; color: #999;">Không có dữ liệu chế độ bảo mật</p>';
            }

            return securityModes.map((mode, index) => {
                const isProMode = mode.Id === 3 || mode.Name?.toLowerCase().includes('pro');
                const backgroundColor = isProMode ? '#4caf50' : 'transparent';
                const textColor = isProMode ? '#ffffff' : '#333';
                const marginBottom = index < securityModes.length - 1 ? '15px' : '0';
                const padding = '15px';
                const borderRadius = '8px';
                const boxShadow = isProMode ? '0 4px 6px rgba(0, 0, 0, 0.1)' : 'none';

                const descriptionLines = (mode.Description || 'Không có mô tả')
                    .split('.')
                    .filter((line: any) => line.trim() !== '')
                    .map((line: any) => `<p style="font-size: 14px; margin: 5px 0;">${line.trim()}.</p>`); 

                return `
            <div style="
                margin-bottom: ${marginBottom}; 
                background-color: ${backgroundColor}; 
                color: ${textColor}; 
                padding: ${padding}; 
                border-radius: ${borderRadius}; 
                box-shadow: ${boxShadow};
                font-family: Arial, sans-serif;
            ">
                <p style="font-size: 16px; font-weight: bold; margin: 0 0 10px 0;">${mode.Name || 'Không có tên'}</p>
                ${descriptionLines.join('')} 
            </div>
        `;
            }).join('');
        };

        Swal.fire({
            title: checked
                ? "Bạn muốn tắt chế độ ghi âm ?"
                : "Bạn muốn bật chế độ ghi âm ?",
            showCancelButton: true,
            confirmButtonText: "Yes",
            html: `
            <div style="text-align: left; padding: 15px; font-family: Arial, sans-serif;">
                ${generateSecurityModesHTML()}
            </div>
        `,
        }).then((result) => {
            if (result.isConfirmed) {
                handleUpdateQuestion("IsVoiced", !checked);
                if (!isPro) {
                    setFormData((prev: any) => ({ ...prev, SecurityModeId: 3 }));
                }
                if (!checked && !isPro) {
                    toast("Đã cập nhật Chế độ bảo mật thành Pro");
                }
            }
        });
    }, [checked, handleUpdateQuestion, isPro, setFormData]);

    return (
        <div className="">
            <Box
                sx={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                    padding: "4px 10px",
                    borderRadius: "8px",
                    minHeight: "20px",
                }}
            >
                <Typography
                    variant="body1"
                    sx={{
                        fontWeight: 600,
                        color: "#000",
                        fontSize: "14px",
                    }}
                >
                    {label}
                </Typography>

                <Switch checked={checked} onChange={handleChange} />
            </Box>
        </div>
    );
};
export default Voice;
