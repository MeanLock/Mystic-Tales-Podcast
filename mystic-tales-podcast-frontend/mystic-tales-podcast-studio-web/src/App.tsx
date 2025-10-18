import { Suspense, useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.scss";
import { Box, CircularProgress } from "@mui/material";
import { RouterProvider } from "react-router-dom";
import { appRouter } from "./router";
import { ToastContainer } from "react-toastify";
import { ReactQueryDevtools } from "@tanstack/react-query-devtools";
function App() {
  const [count, setCount] = useState(0);

  return (
    <Box
      className={"app-layout"}
      display={"flex"}
      flexDirection="column"
      width={"100%"}
      height={"100vh"}
    >
      {/* <h1>App Layout</h1> */}
      <Suspense
        fallback={
          <Box
            sx={{
              display: "flex",
              height: "100vh",
              justifyContent: "center",
              alignItems: "center",
            }}
          >
            <CircularProgress color="primary" />
          </Box>
        }
      >
        <RouterProvider router={appRouter} />
      </Suspense>
      <ToastContainer />
      {/* <ReactQueryDevtools initialIsOpen={false} /> */}
    </Box>
  );
}

export default App;
