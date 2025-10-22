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
        // Thêm các component khác nếu cần
    },
});

export default theme;