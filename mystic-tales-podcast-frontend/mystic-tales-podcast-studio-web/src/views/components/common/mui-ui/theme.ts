import { createTheme } from "@mui/material/styles";

const theme = createTheme({
    typography: {
        fontFamily: "'Poppins', sans-serif",
    },
    components: {
        MuiTypography: {
            styleOverrides: {
                root: {
                    fontFamily: "'Poppins', sans-serif",
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {
                    fontFamily: "'Poppins', sans-serif",
                    "&:focus, &:focus-visible": {
                        outline: "none",
                        boxShadow: "none",
                    },
                },
            },
        },
         MuiIconButton: {
            styleOverrides: {
                root: {
                    "&:focus, &:focus-visible": {
                        outline: "none",
                    },
                },
            },
        },
        MuiInputBase: {
            styleOverrides: {
                root: {
                    fontFamily: "'Poppins', sans-serif",
                },
            },
        },
        MuiMenuItem: {
            styleOverrides: {
                root: {
                    fontFamily: "'Poppins', sans-serif",
                },
            },
        },
           MuiFormLabel: {
            styleOverrides: {
                asterisk: {
                    color: "#e05a5aff", // màu đỏ cho dấu *
                },
            },
        },
        // Thêm các component khác nếu cần
    },
});

export default theme;