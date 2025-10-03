"use client"

import type React from "react"
import { useEffect, useRef, useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { useDispatch, useSelector } from "react-redux"
import type { RootState } from "../../../redux/root-reducer"
import { clearAuthToken, setAuthToken } from "../../../redux/auth/auth.slice"
import { JwtUtil } from "../../../core/utils/jwt.util"
import { publicAxiosInstance } from "../../../core/api/rest-api/config/instances/v2"
import { login } from "../../../core/services/auth/auth.service"
import { toast } from "react-toastify"
import logo from "../../../assets/brand/logo.png"
import "./styles.scss"

const Login = () => {
  const email = useRef<HTMLInputElement>(null)
  const password = useRef<HTMLInputElement>(null)
  const dispatch = useDispatch()
  const navigate = useNavigate()
  const authSlice = useSelector((state: RootState) => state.auth)
  const [validated, setValidated] = useState<boolean>(false)
  const [disabled, setDisabled] = useState(false)

  const handleLogout = () => {
    dispatch(clearAuthToken())
  }

  useEffect(() => {
    handleLogout()
  }, [])

  useEffect(() => {
    if (authSlice && authSlice.token != null && JwtUtil.isTokenNotExpired(authSlice.token)) {
      const user = JwtUtil.decodeToken(authSlice.token)
      if (user.role_id == 1) {
        navigate("/dashboard")
      } else if (user.role_id == 2) {
        navigate("/community-survey")
      }
    }
  }, [authSlice])

  const handleSubmit = async (event: React.FormEvent<HTMLFormElement>) => {
    setValidated(true)
    const form = event.currentTarget
    if (form.checkValidity() === false) {
      event.preventDefault()
      event.stopPropagation()
    } else {
      event.preventDefault()
      setDisabled(true)
      if (email.current?.value == "" || password.current?.value == "") {
        setDisabled(false)
        return
      }

      const login_information = {
        email: email.current?.value,
        password: password.current?.value,
      }

      const response = await login(publicAxiosInstance, login_information)
      if (response.success && response.data) {
        const user = JwtUtil.decodeToken(response.data.AccessToken)
        dispatch(
          setAuthToken({
            token: response.data.AccessToken,
            user: user,
          }),
        )
        if (user.role_id == 1) {
          navigate("/dashboard")
          toast.success("Đăng nhập thành công !")
        } else if (user.role_id == 2) {
          navigate("/community-survey")
          toast.success("Đăng nhập thành công !")
        }
      } else {
        //console.log(response.message)
        toast.error(response.message.content || "Đăng nhập thất bại, vui lòng thử lại !")
      }
      setDisabled(false)
    }
  }

  return (
    <div className="login-page">
      <div className="login-page__background">
        <div className="login-page__floating-element login-page__floating-element--primary"></div>
        <div className="login-page__floating-element login-page__floating-element--secondary"></div>
        <div className="login-page__floating-element login-page__floating-element--tertiary"></div>
      </div>

      <div className="login-page__container">
        <div className="login-form">
          <div className="login-form__header">
            <img src={logo || "/placeholder.svg"} alt="Survey Talk" className="login-form__logo" />
            <p className="login-form__subtitle">Đăng nhập để quản lý Survey Talk</p>
          </div>

          <form
            onSubmit={handleSubmit}
            className={`login-form__form needs-validation ${validated ? "was-validated" : ""}`}
            noValidate
          >
            <div className="login-form__field">
              <div className="login-form__input-group">
                <span className="login-form__input-icon">
                  <svg className="login-form__icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                    />
                  </svg>
                </span>
                <input
                  ref={email}
                  type="email"
                  className="login-form__input form-control"
                  placeholder="Nhập email"
                  required
                />
              </div>
            </div>

            <div className="login-form__field">
              <div className="login-form__input-group">
                <span className="login-form__input-icon">
                  <svg className="login-form__icon" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"
                    />
                  </svg>
                </span>
                <input
                  ref={password}
                  type="password"
                  className="login-form__input form-control"
                  placeholder="Nhập mật khẩu"
                  required
                />
              </div>
            </div>

            <div className="login-form__actions">
              <button
                type="submit"
                disabled={disabled}
                className={`login-form__submit-btn btn w-100 ${disabled ? "login-form__submit-btn--loading" : ""}`}
              >
                {disabled ? (
                  <div className="login-form__loading">
                    <div className="spinner-border spinner-border-sm me-2" role="status">
                      <span className="visually-hidden">Đang tải...</span>
                    </div>
                    Đang đăng nhập...
                  </div>
                ) : (
                  "Đăng nhập"
                )}
              </button>
            </div>

            {/* <div className="login-form__footer">
              <Link to="/register" className="login-form__register-link">
                Chưa có tài khoản? Đăng ký ngay
              </Link>
            </div> */}
          </form>
        </div>
      </div>
    </div>
  )
}

export default Login
