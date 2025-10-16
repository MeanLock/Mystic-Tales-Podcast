import { useEffect, useState, type FC } from 'react';
import './styles.scss'
import Typography from '@mui/material/Typography';
import { Box, Paper, TableBody, Table as muiTable, TableContainer, TableRow, TableCell, TextField, IconButton, Card, CardMedia, CardContent, AvatarGroup, Avatar, CircularProgress, Grid } from '@mui/material';
import './styles.scss'
import RssFeedRoundedIcon from '@mui/icons-material/RssFeedRounded';
import Chip from '@mui/material/Chip';
import { Link, useNavigate } from 'react-router-dom';
import { callAxiosRestApi } from '../../../core/api/rest-api/main/api-call';
import { publicAxiosInstance } from '../../../core/api/rest-api/config/instances/v2';


interface HomePageProps {}

const HomePage : FC<HomePageProps>=() => {
    // HOOKS
    const navigate = useNavigate();

    // STATES
    const [loading, setLoading] = useState(true)
    const [keyword, setKeyword] = useState('')
    const [filter, setFilter] = useState<{ brand: string | null, perfumeName: string | null }>({
        brand: null,
        perfumeName: null,
    })
    const [perfumes, setPerfumes] = useState([])
    const [brands, setBrands] = useState([])

    useEffect(() => {
        setAttributes(filter);
    }, [])

    useEffect(() => {
        setAttributes(filter);
    }, [filter])

    const setAttributes = async (filter: {
        brand: string | null,
        perfumeName: string | null,
    }) => {
        setLoading(false)
        // const rep= await axios.get('https://e734-2a09-bac5-d46f-16c8-00-245-59.ngrok-free.app/api/services/service-types',{headers:{
        //     "ngrok-skip-browser-warning": "69420"
        // }})
        // console.log(rep.data)

        const params = new URLSearchParams();
        if (filter.brand) params.append('brand', filter.brand);
        if (filter.perfumeName) params.append('perfumeName', filter.perfumeName);

        const brands = await callAxiosRestApi({
            instance: publicAxiosInstance,
            method: 'get',
            url: '/Perfume/brands',
        });

        setBrands(brands.data.brands)

        const perfumes = await callAxiosRestApi({
            instance: publicAxiosInstance,
            method: 'get',
            url: '/Perfume/perfumes' + (params.toString() ? '?' + params.toString() : ''),
        });

        if (perfumes.success) {
            setPerfumes(perfumes.data.perfumes)
        } else {
            setPerfumes([])
        }

        setLoading(false)
    }


    const handleBrandFilter = (brand: string | null) => {
        setFilter({ ...filter, brand: brand })
    }

    const handlePerfumeNameFilter = () => {
        // console.log('keyword:', keyword)
        setFilter({ ...filter, perfumeName: keyword })
    }

    const handleSearchInput = (keyword: string) => {
        // console.log('keyword:', keyword)
        setKeyword(keyword)
    }

    const handleNavigate = (url: string) => {
        navigate(url)
    }


    const [focusedCardIndex, setFocusedCardIndex] = useState<number | null>(
        null,
    );
    const handleFocus = (index: number) => {
        setFocusedCardIndex(index);
    };

    const handleBlur = () => {
        setFocusedCardIndex(null);
    };

    return (


        <Box className="PerfumesPage" sx={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
            <div>
                <Typography variant="h1" gutterBottom>
                    Perfumes
                </Typography>
                <Typography>Stay in the loop with the latest about our perfumes</Typography>
            </div>
        </Box>



    );
}


export default HomePage;