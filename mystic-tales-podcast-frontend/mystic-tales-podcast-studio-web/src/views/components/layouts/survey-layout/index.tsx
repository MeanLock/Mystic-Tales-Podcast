import "./styles.scss"
import { Box, CircularProgress, Container } from "@mui/material"
import { Suspense } from "react"
import { Outlet } from "react-router-dom"
import { SurveyLayoutContent } from "./SurveyLayoutContent"
import { SurveyLayoutHeader } from "./SurveyLayoutHeader"
import { SurveyLayoutFooter } from "./SurveyLayoutFooter"

const SurveyLayout = () => {
    return (
        <Box className={"survey-layout"} display={'flex'} flexDirection='column' width={'100%'} height={'100vh'}>
            <SurveyLayoutHeader />
            <Container maxWidth="xl" className="survey-layout-content-container" sx={{ height: '100vh', width: '100%' }}>
                <SurveyLayoutContent />
            </Container>
            <SurveyLayoutFooter />
        </Box>
    )
}

export default SurveyLayout
