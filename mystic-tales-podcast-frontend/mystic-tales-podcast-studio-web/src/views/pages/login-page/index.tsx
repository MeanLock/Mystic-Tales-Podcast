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


interface LoginPageProps {
    disableCustomTheme?: boolean;
}

const LoginPage : FC<LoginPageProps> = (props) => {
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
        if(validateInputs() === false) {
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
        <AppTheme {...props}>
            <CssBaseline enableColorScheme />
            <SignInContainer direction="column" justifyContent="space-between">
                <Card variant="outlined">
                    <Typography
                        component="h1"
                        variant="h4"
                        sx={{ width: '100%', fontSize: 'clamp(2rem, 10vw, 2.15rem)' }}
                    >
                        Sign in
                    </Typography>
                    <Box
                        sx={{
                            display: 'flex',
                            flexDirection: 'column',
                            width: '100%',
                            gap: 2,
                        }}
                    >
                        <FormControl>
                            <FormLabel htmlFor="email">membername or email</FormLabel>
                            <TextField
                                error={membernameError}
                                helperText={membernameErrorMessage}
                                id="email"
                                type="email"
                                name="email"
                                placeholder="handsomeboy"
                                autoComplete="email"
                                autoFocus
                                required
                                fullWidth
                                variant="outlined"
                                color={membernameError ? 'error' : 'primary'}
                                onChange={(e) => setMembername(e.target.value)}
                            />
                        </FormControl>
                        <FormControl>
                            <FormLabel htmlFor="password">Password</FormLabel>
                            <TextField
                                error={passwordError}
                                helperText={passwordErrorMessage}
                                name="password"
                                placeholder="••••••"
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
                        <Typography sx={{ textAlign: 'center' }}>
                            Don&apos;t have an account?{' '}
                            <Link
                                href="/register"
                                variant="body2"
                                sx={{ alignSelf: 'center' }}
                            >
                                Create account
                            </Link>
                        </Typography>
                    </Box>
                </Card>
            </SignInContainer>
        </AppTheme>
    );
}


export default LoginPage;