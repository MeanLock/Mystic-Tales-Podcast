import { Box, CircularProgress, Container } from "@mui/material"
import "./styles.scss"
import { createContext, Suspense } from "react"
import { AuthLayoutContent } from "./AuthLayoutContent";
import { AuthLayoutHeader } from "./AuthLayoutHeader";
import { AuthLayoutFooter } from "./AuthLayoutFooter";

interface AuthLayoutContextProps { }

export const AuthLayoutContext = createContext<AuthLayoutContextProps>({});

const AuthLayout = () => {
    return (
        <AuthLayoutContext.Provider value={{}}>
            <Box className={"auth-layout"} display={'flex'} flexDirection='column' width={'100%'} >
                <AuthLayoutHeader />
                <Container maxWidth="xl" className="auth-layout-content-container" sx={{ height: '100vh', width: '100%' }}>
                    <AuthLayoutContent />
                </Container>
                <AuthLayoutFooter />
            </Box>
        </AuthLayoutContext.Provider>



    )
}

export default AuthLayout
