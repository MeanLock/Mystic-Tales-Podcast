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
import { useDispatch } from 'react-redux';
import { useGoogleLogin } from '@react-oauth/google';
import { CssBaseline } from '@mui/material';
import { publicAxiosInstance } from '../../../core/api/rest-api/config/instances/v2';
import { callAxiosRestApi } from '../../../core/api/rest-api/main/api-call';
import { setAuthToken } from '../../../redux/auth/authSlice';
import { errorAlert } from '../../../core/utils/alert.util';
import AppTheme from '../../components/common/mui-ui/AppTheme';
import { GoogleIcon } from '../../components/common/mui-ui/MuiUiCustomIcons';
import { SignInContainer } from './SignInContainer';
import { Card } from './Card';
import logo from "../../../assets/login.png"
import './styles.scss'
interface LoginPageProps {
    disableCustomTheme?: boolean;
}

const LoginPage: FC<LoginPageProps> = (props) => {
    // REDUX
    const dispatch = useDispatch();

    // STATES
    const [manualLoading, setManualLoading] = useState(false);
    const [googleLoading, setGoogleLoading] = useState(false);
    const [membernameError, setMembernameError] = useState(false);
    const [membernameErrorMessage, setMembernameErrorMessage] = useState('');
    const [passwordError, setPasswordError] = useState(false);
    const [passwordErrorMessage, setPasswordErrorMessage] = useState('');

    const [membername, setMembername] = useState<string>('');
    const [password, setPassword] = useState<string>('');

    const validateInputs = () => {
        let isValid = true;

        if (!membername) {
            setMembernameError(true);
            setMembernameErrorMessage('membername or email required.');
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
            membername: membername,
            password: password
        }
        const login_result = await callAxiosRestApi({
            instance: publicAxiosInstance,
            method: 'post',
            url: '/Member/login',
            data: login_info
        }, "Login Manual");

        if (login_result.success) {
            const token = login_result.data.auth.token;
            const user = login_result.data.auth.member;

            const redirectUrl = localStorage.getItem('redirectUrl');
            if (redirectUrl) {
                localStorage.removeItem('redirectUrl');
                window.location.href = redirectUrl;
            } else {
                window.location.href = '/';
            }

            dispatch(setAuthToken({
                token: token,
                user: user,
            }))
        } else if (!login_result.isAppError) {
            errorAlert(login_result.message.content || "Login failed. Please try again.");
        }
        setManualLoading(false);

    }

    const handleLoginGoogleOAuth2 = useGoogleLogin(
        {
            flow: 'auth-code',
            onSuccess: async codeResponse => {
                // console.log('Login Successsss:', codeResponse);
                const authorizationCode = codeResponse.code;

                // console.log("authorization_code", authorizationCode)

                const login_result = await callAxiosRestApi({
                    instance: publicAxiosInstance,
                    method: 'post',
                    url: '/auth/google/login-authorization-code-flow',
                    data: {
                        authorizationCode: authorizationCode,
                        redirectUri: import.meta.env.VITE_BASE_URL
                    }
                }, "Login with Google");



                if (login_result.success) {

                    const token = login_result.data.auth.token;
                    const user = login_result.data.auth.member;

                    const redirectUrl = localStorage.getItem('redirectUrl');
                    if (redirectUrl) {
                        localStorage.removeItem('redirectUrl');
                        window.location.href = redirectUrl;
                    } else {
                        window.location.href = '/';
                    }
                    dispatch(setAuthToken({
                        token: token,
                        user: user,
                    }))
                } else {
                    // instantAlertMaker('error', 'Login failed', login_result.error);
                    console.log("ERROR", login_result.message.content)
                }

                setGoogleLoading(false)

            },
            onError: error => {
                // instantAlertMaker('error', 'Login failed', error);
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
        >        <AppTheme {...props}>
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
                                    error={membernameError}
                                    helperText={membernameErrorMessage}
                                    id="email"
                                    type="email"
                                    name="email"
                                    placeholder="Please enter your email"
                                    autoComplete="email"
                                    autoFocus
                                    required
                                    fullWidth
                                    variant="outlined"
                                    color={membernameError ? 'error' : 'primary'}
                                    onChange={(e) => setMembername(e.target.value)}
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
                                    color={passwordError ? 'error' : 'primary'}
                                    onChange={(e) => setPassword(e.target.value)}
                                />
                            </FormControl>

                            <Button
                                fullWidth
                                variant="contained"
                                onClick={handleLoginManual}
                                loading={manualLoading}
                            >
                                Sign in
                            </Button>
                        </Box>
                        <Divider>or</Divider>
                        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
                        <Button
                            fullWidth
                            variant="outlined"
                            onClick={() => handleGoogleLogin()}
                            startIcon={<GoogleIcon />}
                            loading={googleLoading}
                        >
                            Sign in with Google
                        </Button>
                    </Box>
                    </Card>
                </SignInContainer>
            </AppTheme>
        </div>
    );
}


export default LoginPage;