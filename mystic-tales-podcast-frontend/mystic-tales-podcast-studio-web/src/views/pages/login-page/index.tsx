import { useEffect, useState, type FC } from 'react';
import './styles.scss'
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import Divider from '@mui/material/Divider';
import FormLabel from '@mui/material/FormLabel';
import FormControl from '@mui/material/FormControl';
import Link from '@mui/material/Link';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useDispatch, useSelector } from 'react-redux';
import { useGoogleLogin } from '@react-oauth/google';
import { CssBaseline } from '@mui/material';
import { loginRequiredAxiosInstance, publicAxiosInstance } from '../../../core/api/rest-api/config/instances/v2';
import { callAxiosRestApi } from '../../../core/api/rest-api/main/api-call';
import { clearAuthToken, setAuthToken } from '../../../redux/auth/authSlice';
import { errorAlert } from '../../../core/utils/alert.util';
import { GoogleIcon } from '../../components/common/mui-ui/MuiUiCustomIcons';
import { SignInContainer } from './SignInContainer';
import { Card } from './Card';
import logo from "../../../assets/login.png"
import './styles.scss'
import { PasswordSharp } from '@mui/icons-material';
import { login, loginGoogle } from '@/core/services/auth/auth.service';
import { toast } from 'react-toastify';
import { JwtUtil } from '@/core/utils/jwt.util';
import { useSagaPolling } from '@/core/hooks/useSagaPolling';
import { useNavigate } from 'react-router-dom';
import { get } from 'lodash';
import { getAccountProfile } from '@/core/services/account/account.service';
import { getCapacitorDevice } from '@/core/utils/device.util';
import axios from 'axios';
interface LoginPageProps {
    disableCustomTheme?: boolean;
}

const LoginPage: FC<LoginPageProps> = (props) => {
    // REDUX
    const dispatch = useDispatch();

    // STATES
    const [manualLoading, setManualLoading] = useState(false);
    const [googleLoading, setGoogleLoading] = useState(false);
    const [emailError, setMembernameError] = useState(false);
    const [emailErrorMessage, setMembernameErrorMessage] = useState('');
    const [passwordError, setPasswordError] = useState(false);
    const [passwordErrorMessage, setPasswordErrorMessage] = useState('');
    const navigate = useNavigate();
    const [user, setUser] = useState<any>(null);
    const [email, setMembername] = useState<string>('');
    const [password, setPassword] = useState<string>('');
    const { startPolling } = useSagaPolling({
        timeoutSeconds: 120,
        intervalSeconds: 0.5,
    })
    const deviceInfo = getCapacitorDevice();
    const { token } = useSelector((state: any) => state.auth);

    const validateInputs = () => {
        let isValid = true;

        if (!email) {
            setMembernameError(true);
            setMembernameErrorMessage('email or email required.');
            isValid = false;
        } else {
            setMembernameError(false);
            setMembernameErrorMessage('');
        }

        if (!password) {
            setPasswordError(true);
            setPasswordErrorMessage('Password required.');
            isValid = false;
        } else {
            setPasswordError(false);
            setPasswordErrorMessage('');
        }

        return isValid;
    };

    const handleLoginManual = async () => {
        setManualLoading(true);
        if (validateInputs() === false) {
            setManualLoading(false);
            return;
        }

        const login_info = {
            email: email,
            password: password
        }
        try {
            const response = await login(publicAxiosInstance, {
                email: login_info.email,
                password: login_info.password,
                DeviceInfo: await deviceInfo
            })

            const sagaId = response?.data?.SagaInstanceId
            if (!sagaId) {
                toast.error("Login failed.")
                setManualLoading(false);
                return
            }
            await startPolling(sagaId, publicAxiosInstance, {
                onSuccess: async (data) => {
                    const token = data?.AccessToken
                    if (!token) {
                        toast.error("Token not found.");
                        return;
                    }
                    const decode = JwtUtil.decodeToken(token)
                    if (decode?.role_id !== "1") {
                        toast.error("You do not have permission to access the Studio");
                        return;
                    }

                    dispatch(setAuthToken({ token }));
                   const tempInstance = axios.create({
    ...loginRequiredAxiosInstance.defaults,
    headers: {
        ...loginRequiredAxiosInstance.defaults.headers,
        Authorization: `Bearer ${token}`
    }
});

                    const res = await getAccountProfile(tempInstance);
console.log("res", res);
                    if (!res?.success) {
                        toast.error("Login failed.");
                        return;
                    }

                    const account = res.data?.Account;
                    if (!account || account.IsPodcaster !== true) {
                        dispatch(clearAuthToken());
                        toast.error("You do not have permission to access the Studio.");
                        return;
                    }

                    dispatch(setAuthToken({ token, user: account }));
                    setUser(account);
                    navigate("/");
                    toast.success("Login successfully!");
                },
                onFailure: (err: any) => toast.error(err || "Saga failed!"),
                onTimeout: () => toast.error("System not responding, please try again."),
            })
        } catch (err) {
            toast.error("Lỗi kết nối máy chủ.")
        } finally {
            setManualLoading(false);
        }
    }

       useEffect(() => {
        // Kiểm tra nếu đã có token thì redirect về dashboard
        if (token) {
            const decode = JwtUtil.decodeToken(token);
            if (decode && decode?.role_id === "1") {
                navigate("/dashboard");
                return;
            }
        }
        // Chỉ xóa token nếu không có token hợp lệ
        dispatch(clearAuthToken());
    }, [token, navigate, dispatch]);

    const handleLoginGoogleOAuth2 = useGoogleLogin(
        {
            flow: 'auth-code',
            onSuccess: async codeResponse => {
                // console.log('Login Successsss:', codeResponse);
                const authorizationCode = codeResponse.code;
                // const login_result = await callAxiosRestApi({
                //     instance: publicAxiosInstance,
                //     method: 'post',
                //     url: 'user-service/api/auth/login-google',
                //     data: {
                //         GoogleAuth: {
                //             AuthorizationCode: authorizationCode,
                //             RedirectUri: import.meta.env.VITE_BASE_URL
                //         },
                //         DeviceInfo: {
                //             DeviceId: 'da27b241-85ab-4269-9fa4-f44d81cd65ac',
                //             Platform: 'web',
                //             OSName: 'windows'
                //         }

                //     }
                // }, "Login with Google");

                const response = await loginGoogle(publicAxiosInstance, {
                    AuthorizationCode: authorizationCode,
                    RedirectUri: import.meta.env.VITE_BASE_URL,
                    DeviceInfo: await deviceInfo
                })

                const sagaId = response?.data?.SagaInstanceId
                if (!sagaId) {
                    toast.error("Login failed.")
                    setManualLoading(false);
                    return
                }
                await startPolling(sagaId, publicAxiosInstance, {
                    onSuccess: async (data) => {
                        const token = data?.AccessToken
                        if (!token) {
                            toast.error("Token not found.");
                            return;
                        }
                        const decode = JwtUtil.decodeToken(token)
                        if (decode?.role_id !== "1") {
                            toast.error("You do not have permission to access the Studio");
                            return;
                        }

                        dispatch(setAuthToken({ token }));
                        await new Promise((r) => setTimeout(r, 100));

                        const res = await getAccountProfile(loginRequiredAxiosInstance);

                        if (!res?.success) {
                            toast.error("Login failed.");
                            return;
                        }

                        const account = res.data?.Account;
                        if (!account || account.IsPodcaster !== true) {
                            dispatch(clearAuthToken());
                            toast.error("You do not have permission to access the Studio.");
                            return;
                        }

                        dispatch(setAuthToken({ token, user: account }));
                        setUser(account);
                        navigate("/");
                        toast.success("Login successfully!");
                    },
                    onFailure: (err: any) => toast.error(err || "Saga failed!"),
                    onTimeout: () => toast.error("System not responding, please try again."),
                })
            },
            onError: error => {
                alert("Error: " + error.error)
                console.log("Error", error)
            }
        }
    );

    const handleGoogleLogin = () => {
        setGoogleLoading(true);
        handleLoginGoogleOAuth2();
    }

    return (
        <div
            style={{
                backgroundImage: 'url("https://images.unsplash.com/photo-1660914256311-918659fae88f?ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&q=80&w=2070")',
                backgroundSize: 'cover',
                backgroundPosition: 'center',
                minHeight: '100vh',
                width: '100vw'
            }}
        >
            <CssBaseline enableColorScheme />
            <SignInContainer direction="column" justifyContent="space-between">
                <Card variant="outlined" className="login-card">
                    <img src={logo || "/placeholder.svg"} alt="Logo" />
                    <Box className="login-text my-6">
                        <p className="login-text-title">Mystic Tales Podcast</p>
                        <p className="login-text-subtitle">STUDIO</p>
                    </Box>
                    <Box
                        sx={{
                            display: 'flex',
                            flexDirection: 'column',
                            width: '100%',
                            gap: 2,
                        }}
                    >
                        <FormControl>
                            <FormLabel className="text-left" htmlFor="email">Email</FormLabel>
                            <TextField
                                error={emailError}
                                helperText={emailErrorMessage}
                                id="email"
                                type="email"
                                name="email"
                                placeholder="Please enter your email"
                                autoComplete="email"
                                autoFocus
                                required
                                fullWidth
                                variant="outlined"
                                size="small" // Thêm dòng này để thu nhỏ chiều cao
                                color={emailError ? 'error' : 'primary'}
                                onChange={(e) => setMembername(e.target.value)}
                                InputProps={{
                                    sx: { height: 42 } // Tuỳ chỉnh chiều cao nếu muốn nhỏ hơn nữa
                                }}
                            />
                        </FormControl>
                        <FormControl >
                            <FormLabel className="text-left" htmlFor="password">Password</FormLabel>
                            <TextField
                                error={passwordError}
                                helperText={passwordErrorMessage}
                                name="password"
                                placeholder="•••••••••"
                                type="password"
                                id="password"
                                autoComplete="current-password"
                                autoFocus
                                required
                                fullWidth
                                variant="outlined"
                                size="small"
                                color={passwordError ? 'error' : 'primary'}
                                onChange={(e) => setPassword(e.target.value)}
                                InputProps={{
                                    sx: { height: 42 } // Tuỳ chỉnh chiều cao nếu muốn nhỏ hơn nữa
                                }}
                            />
                        </FormControl>

                        <Button
                            fullWidth
                            variant="contained"
                            onClick={handleLoginManual}
                            disabled={manualLoading}
                        >
                            {manualLoading ? 'Signing in...' : 'Sign in'}
                        </Button>
                    </Box>
                    <Divider>or</Divider>
                    <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Button
                            fullWidth
                            variant="outlined"
                            onClick={() => handleGoogleLogin()}
                            startIcon={<GoogleIcon />}
                            disabled={googleLoading || manualLoading}
                            loading={googleLoading}
                        >
                            Sign in with Google
                        </Button>
                    </Box>
                </Card>
            </SignInContainer>
        </div>
    );
}


export default LoginPage;