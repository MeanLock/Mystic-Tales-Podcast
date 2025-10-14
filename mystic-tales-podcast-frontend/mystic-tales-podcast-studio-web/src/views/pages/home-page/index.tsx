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
import { StyledTypography, SyledCard, SyledCardContent } from '../../components/common/mui-ui/MuiUiComponents';


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


            <Box
                sx={{
                    display: 'flex',
                    flexDirection: { xs: 'column-reverse', md: 'row' },
                    width: '100%',
                    justifyContent: 'space-between',
                    alignItems: { xs: 'start', md: 'center' },
                    gap: 4,
                    overflow: 'auto',
                }}
            >
                <Box
                    sx={{
                        display: 'inline-flex',
                        flexDirection: 'row',
                        gap: 3,
                        overflow: 'auto',
                        paddingY: 3,
                    }}
                >
                    <Chip
                        onClick={() => handleBrandFilter(null)}
                        size="medium"
                        label="All categories"
                    />
                    {brands.map((brand : any ) => (
                        <Chip
                            onClick={() => handleBrandFilter(brand._id)}
                            size="medium"
                            label={brand.brandName}
                            variant={filter.brand?.toString() == brand._id.toString() ? 'filled' : 'outlined'}
                            color={filter.brand?.toString() == brand._id.toString() ? 'success' : 'default'}
                            sx={{
                                backgroundColor: 'transparent',
                                border: 'none',
                            }}
                        />
                    ))
                    }

                </Box>
                <Box
                    sx={{
                        display: { xs: 'none', sm: 'flex' },
                        flexDirection: 'row',
                        gap: 1,
                        width: { xs: '100%', md: 'fit-content' },
                        overflow: 'auto',
                    }}
                >

                    <IconButton size="small" aria-label="RSS feed" onClick={handlePerfumeNameFilter}>
                        <RssFeedRoundedIcon />
                    </IconButton>
                </Box>
            </Box>
            {/* {(filter.brand?.length > 0 || filter.perfumeName?.length > 0 ) ? "true" : "false"} */}
            {(!loading  && ((filter.brand && filter.brand?.length > 0) || (filter.perfumeName && filter.perfumeName?.length > 0) )) &&

                <Typography variant="h6" sx={{ textAlign: 'center', width: '100%' }}  >
                    {perfumes.length} item found
                </Typography>
            }
            <Grid container spacing={2} columns={12} paddingY={4}>

                {(!loading  && perfumes.length === 0) &&

                    <Typography variant="h6" sx={{ textAlign: 'center', width: '100%' }}  >
                        No item found
                    </Typography>
                }
                {!loading ? perfumes.map((perfume : any) => (
                    <Grid size={{ xs: 12, md: 4 }}>
                        <SyledCard
                            variant="outlined"
                            onFocus={() => handleFocus(2)}
                            onBlur={handleBlur}
                            tabIndex={0}
                            className={focusedCardIndex === 2 ? 'Mui-focused' : ''}
                            sx={{ height: '100%' }}
                            onClick={() => handleNavigate(`/perfumes/${perfume._id}`)}
                        >
                            <CardMedia
                                component="img"
                                alt="green iguana"
                                image={perfume.imageUrl}
                                sx={{
                                    height: { sm: 'auto', md: '50%' },
                                    aspectRatio: { sm: '16 / 9', md: '' },
                                }}
                            />
                            <SyledCardContent>
                                <Typography gutterBottom variant="caption" component="div">
                                    {perfume.brand.brandName}
                                </Typography>
                                <Typography gutterBottom variant="h6" component="div">
                                    {perfume.perfumeName}
                                </Typography>
                                <StyledTypography variant="body2" color="text.secondary" gutterBottom>
                                    Nồng độ: &nbsp;
                                    <Typography color="primary" variant="body2" component="span">
                                        {perfume.concentration}
                                    </Typography>
                                </StyledTypography>
                                <StyledTypography variant="body2" color="text.secondary" gutterBottom>
                                    Đối tượng sử dụng: {perfume.targetAudience}
                                </StyledTypography>
                                <StyledTypography variant="body2" color="text.secondary" gutterBottom>
                                    Giá: &nbsp;
                                    <Typography color="success" variant="body2" component="span">
                                        {perfume.price}$
                                    </Typography>
                                </StyledTypography>

                            </SyledCardContent>
                            {/* <Author authors={[{ name: 'AUTHOTR', avatar: '/static/images/avatar/2.jpg' }]} /> */}

                        </SyledCard>
                    </Grid>
                ))
                    :
                    <Box sx={{ display: 'flex', height: '100%', width: '100%', alignItems: 'center', justifyContent: 'center', paddingY: '100px' }}>
                        <CircularProgress color="inherit" />
                    </Box>
                }


            </Grid>

        </Box>



    );
}


export default HomePage;