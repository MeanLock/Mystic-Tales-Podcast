import React, { Suspense, useEffect } from "react";
import { Box, CircularProgress } from "@mui/material";
import { Navigate, Route, Routes, useNavigate } from "react-router-dom";
import routes from "../../../../router/default.routes";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/redux/rootReducer";

const DefaultLayoutContent = () => {
  const dispatch = useDispatch()
      const authSlice = useSelector((state: RootState) => state.auth)
      const navigate = useNavigate();

  return (
    <Box>
        <Suspense 
          fallback={
            <Box display="flex" justifyContent="center" alignItems="center" minHeight="200px">
              <CircularProgress color="primary" />
            </Box>
          }
        >
          <Routes>
            {routes.map((route, idx) => {
              return (
                route.element && (
                  <Route
                    key={idx}
                    path={route.path}
                    element={<route.element />}
                  />
                )
              )
            })}
            <Route path="/" element={<Navigate to="/dashboard" replace />} />
          </Routes>
        </Suspense>
    </Box>
  )
}

export default React.memo(DefaultLayoutContent)