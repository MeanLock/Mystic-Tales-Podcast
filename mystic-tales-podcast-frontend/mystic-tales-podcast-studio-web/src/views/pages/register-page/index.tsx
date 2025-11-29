import { useEffect, useState, type FC } from 'react';
import './styles.scss';
import Box from '@mui/material/Box';
import Button from '@mui/material/Button';
import FormControlLabel from '@mui/material/FormControlLabel';
import Divider from '@mui/material/Divider';
import FormLabel from '@mui/material/FormLabel';
import FormControl from '@mui/material/FormControl';
import Link from '@mui/material/Link';
import TextField from '@mui/material/TextField';
import Typography from '@mui/material/Typography';
import { useDispatch } from 'react-redux';
import { CssBaseline, Radio, RadioGroup } from '@mui/material';
import { callAxiosRestApi } from '../../../core/api/rest-api/main/api-call';
import { publicAxiosInstance } from '../../../core/api/rest-api/config/instances/v2';
import { errorAlert, successAlert } from '../../../core/utils/alert.util';
import { SignInContainer } from './SignInContainer';
import { Card } from './Card';


const validateEmail = (email : string) => {
    return String(email)
        .toLowerCase()
        .match(
            /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|.(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        );
};
interface RegisterPageProps {
    disableCustomTheme?: boolean;
}

const RegistersPage : FC<RegisterPageProps> = (props) => {
    // REDUX
    const dispatch = useDispatch();

    // STATES
    const [loading, setLoading] = useState(false);

    const [membername, setMembername] = useState('')
    const [password, setPassword] = useState('')
    const [email, setEmail] = useState('')
    const [YOB, setYOB] = useState(0)
    const [gender, setGender] = useState('male')
    const [isAdmin, setIsAdmin] = useState(false)

    const [membernameErrorMessage, setMembernameErrorMessage] = useState('')
    const [passwordErrorMessage, setPasswordErrorMessage] = useState('')
    const [emailErrorMessage, setEmailErrorMessage] = useState('')
    const [YOBErrorMessage, setYOBErrorMessage] = useState('')
    const [genderErrorMessage, setGenderErrorMessage] = useState('')


    const validateRegisterMember = () => {
        let isValid = true

        if (membername === '') {
            isValid = false
            setMembernameErrorMessage('Tên không được để trống')
        } else {
            setMembernameErrorMessage('')
        }

        if (password === '') {
            isValid = false
            setPasswordErrorMessage('Password không được để trống')
        } else {
            setPasswordErrorMessage('')
        }

        if (email === '' || !validateEmail(email)) {
            isValid = false
            setEmailErrorMessage('Email không hợp lệ')
        } else {
            setEmailErrorMessage('')
        }

        if (YOB <= 0) {
            isValid = false
            setYOBErrorMessage('YOB phải > 0 ')
        } else {
            setYOBErrorMessage('')
        }

        if (gender === '') {
            isValid = false
            setGenderErrorMessage('gender không được để trống')
        } else {
            setGenderErrorMessage('')
        }

        return isValid
    }

    const handleRegisterMember = async () => {
        if (validateRegisterMember()) {
            setLoading(true)
            const addMember = await callAxiosRestApi({
                instance: publicAxiosInstance,
                method: 'post',
                url: '/Member/register',
                data: {
                    membername: membername,
                    password: password,
                    email: email,
                    YOB: YOB,
                    gender: gender,
                    isAdmin: isAdmin
                }
            });
            if (addMember.success) {
                await successAlert(addMember.message.content + '. Please login now!');
                window.location.href = '/login'
            } else if (!addMember.isAppError) {
                errorAlert(addMember.message.content+ '. Please try again!');
            }
            setLoading(false)
        }
    }

    return (
        // <div {...props}>
        //     <CssBaseline enableColorScheme />
        //     <SignInContainer direction="column" justifyContent="space-between">
        //         <Card variant="outlined">
        //             <Typography
        //                 component="h1"
        //                 variant="h4"
        //                 sx={{ width: '100%', fontSize: 'clamp(2rem, 10vw, 2.15rem)' }}
        //             >
        //                 Register
        //             </Typography>
        //             <Box
        //                 sx={{
        //                     display: 'flex',
        //                     flexDirection: 'column',
        //                     width: '100%',
        //                     gap: 2,
        //                 }}
        //             >
        //                 <FormControl>
        //                     <FormLabel htmlFor="email">membername</FormLabel>
        //                     <TextField
        //                         error={membernameErrorMessage.length > 0}
        //                         helperText={membernameErrorMessage}
        //                         id="email"
        //                         type="text"
        //                         name="email"
        //                         placeholder="membername"
        //                         autoComplete="email"
        //                         autoFocus
        //                         required
        //                         fullWidth
        //                         variant="outlined"
        //                         color={membernameErrorMessage ? 'error' : 'primary'}
        //                         onChange={(e) => setMembername(e.target.value)}
        //                     />
        //                 </FormControl>
        //                 <FormControl>
        //                     <FormLabel htmlFor="password">password</FormLabel>
        //                     <TextField
        //                         error={passwordErrorMessage.length > 0}
        //                         helperText={passwordErrorMessage}
        //                         name="password"
        //                         placeholder="••••••"
        //                         type="password"
        //                         id="password"
        //                         autoComplete="current-password"
        //                         required
        //                         fullWidth
        //                         variant="outlined"
        //                         color={passwordErrorMessage ? 'error' : 'primary'}
        //                         onChange={(e) => setPassword(e.target.value)}
        //                     />
        //                 </FormControl>
        //                 <FormControl>
        //                     <FormLabel htmlFor="email">email</FormLabel>
        //                     <TextField
        //                         error={emailErrorMessage.length > 0}
        //                         helperText={emailErrorMessage}
        //                         id="email"
        //                         type="email"
        //                         name="email"
        //                         placeholder="your@email.com"
        //                         autoComplete="email"
        //                         required
        //                         fullWidth
        //                         variant="outlined"
        //                         color={emailErrorMessage ? 'error' : 'primary'}
        //                         onChange={(e) => setEmail(e.target.value)}
        //                     />
        //                 </FormControl>
        //                 <FormControl>
        //                     <FormLabel htmlFor="email">YOB</FormLabel>
        //                     <TextField
        //                         error={YOBErrorMessage.length > 0}
        //                         helperText={YOBErrorMessage}
        //                         id="email"
        //                         type="number"
        //                         name="email"
        //                         placeholder="year of birth"
        //                         autoComplete="email"
        //                         required
        //                         fullWidth
        //                         variant="outlined"
        //                         color={YOBErrorMessage ? 'error' : 'primary'}
        //                         onChange={(e) => setYOB(parseInt(e.target.value))}
        //                     />
        //                 </FormControl>
        //                 <FormControl>
        //                     <FormLabel htmlFor="email">gender</FormLabel>
        //                     <RadioGroup
        //                         aria-labelledby="demo-controlled-radio-buttons-group"
        //                         name="controlled-radio-buttons-group"
        //                         value={gender}
        //                         onChange={(e) => setGender(e.target.value)}
        //                     >
        //                         <FormControlLabel value="male" control={<Radio />} label="Male" />
        //                         <FormControlLabel value="female" control={<Radio />} label="Female" />

        //                     </RadioGroup>
        //                 </FormControl>
        //                 <Button
        //                     fullWidth
        //                     variant="contained"
        //                     onClick={handleRegisterMember}
        //                     loading={loading}
        //                 >
        //                     Sign up
        //                 </Button>
        //             </Box>
        //             <Divider>or</Divider>
        //             <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        //                 <Typography sx={{ textAlign: 'center' }}>
        //                     Already have an account?{' '}
        //                     <Link
        //                         href="/login"
        //                         variant="body2"
        //                         sx={{ alignSelf: 'center' }}
        //                     >
        //                         Log in
        //                     </Link>
        //                 </Typography>
        //             </Box>
        //         </Card>
        //     </SignInContainer>
        // </AppTheme>
        <div></div>
    );
}


export default RegistersPage;