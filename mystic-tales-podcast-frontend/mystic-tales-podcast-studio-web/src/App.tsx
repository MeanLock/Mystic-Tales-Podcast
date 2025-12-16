import { Suspense, useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.scss";
import { Box, CircularProgress } from "@mui/material";
import { RouterProvider } from "react-router-dom";
import { appRouter } from "./router";
import { ToastContainer } from "react-toastify";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
import Loading from "./views/components/common/loading";
function App() {

  return (
    <Box
      className={"app-layout"}
      display={"flex"}
      flexDirection="column"
      width={"100%"}
      height={"100vh"}
    >
      <Suspense
        fallback={
          <Box
            sx={{
              display: "flex",
              height: "100vh",
              justifyContent: "center",
              alignItems: "center",
              backgroundColor: "var(--primary-grey)",
            }}
          >
            <Loading  />
          </Box>
        }
      >
        <RouterProvider router={appRouter} />
      </Suspense>
      <ToastContainer />
    </Box>
  );
}

export default App;
