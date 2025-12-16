import React, { createContext, FC, useEffect, useState } from 'react';
import { Button, TextField, InputAdornment, Chip, Box, Typography, Card, CardMedia, CardContent, Badge, Menu, MenuItem, FormControlLabel, Checkbox, Accordion, AccordionSummary, AccordionDetails, Divider, InputBase } from '@mui/material';
import { Search, Add, FilterList, PersonAdd, PlayArrow, ExpandMore } from '@mui/icons-material';
import './styles.scss';
import { EmptyComponent } from '@/views/components/common/empty';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import ShowCreate from './ShowCreate';
import Loading from '@/views/components/common/loading';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import { getShowList } from '@/core/services/show/show.service';
import { getCategories } from '@/core/services/misc/category.service';
import ShowCard from './components/ShowCard';


interface SelectedFilter {
    categoryId: number;
    categoryName: string;
    subcategoryId?: number;
    subcategoryName?: string;
}


interface MyShowPageProps { }
interface MyShowPageContextProps {
    handleDataChange: () => void;
    categoryList: any[];
}

export const MyShowPageContext = createContext<MyShowPageContextProps | null>(null);
const MyShowPage: FC<MyShowPageProps> = () => {
    const [ShowList, setShowList] = useState<any[]>([])
    const [categoryList, setCategoryList] = useState<any[]>([])
    const [searchQuery, setSearchQuery] = useState('')
    const [selectedFilters, setSelectedFilters] = useState<SelectedFilter[]>([])
    const [filterAnchorEl, setFilterAnchorEl] = useState<null | HTMLElement>(null)
    const [loading, setLoading] = useState(false);

    const fetchShowList = async () => {
        setLoading(true);
        try {
            const showList = await getShowList(loginRequiredAxiosInstance);
            console.log("Fetched show :", showList);
            if (showList.success) {
                setShowList(showList.data.ShowList);
            } else {
                console.error('API Error:', showList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch show list:', error);
        } finally {
            setLoading(false);
        }
    }

    const fetchCategory = async () => {
        setLoading(true);
        try {
            const categoryList = await getCategories(loginRequiredAxiosInstance);
            console.log("Fetched category list:", categoryList);
            if (categoryList.success) {
                setCategoryList(categoryList.data.PodcastCategoryList);
            } else {
                console.error('API Error:', categoryList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch category list:', error);
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchShowList()
        fetchCategory()
    }, [])

    const handleFilterRemove = (filterToRemove: SelectedFilter) => {
        setSelectedFilters(prev =>
            prev.filter(filter =>
                !(filter.categoryId === filterToRemove.categoryId &&
                    filter.subcategoryId === filterToRemove.subcategoryId)
            )
        )
    }

    const handleCategoryToggle = (categoryId: number, categoryName: string) => {
        const existingFilter = selectedFilters.find(f => f.categoryId === categoryId && !f.subcategoryId)

        if (existingFilter) {
            // Remove category and all its subcategories
            setSelectedFilters(prev => prev.filter(f => f.categoryId !== categoryId))
        } else {
            // Add category, remove all subcategories of this category
            setSelectedFilters(prev => [
                ...prev.filter(f => f.categoryId !== categoryId),
                { categoryId, categoryName }
            ])
        }
    }

    const handleSubcategoryToggle = (categoryId: number, categoryName: string, subcategoryId: number, subcategoryName: string) => {
        const existingFilter = selectedFilters.find(f =>
            f.categoryId === categoryId && f.subcategoryId === subcategoryId
        )

        if (existingFilter) {
            // Remove subcategory
            setSelectedFilters(prev =>
                prev.filter(f => !(f.categoryId === categoryId && f.subcategoryId === subcategoryId))
            )
        } else {
            // Add subcategory (and remove category-only filter if exists)
            setSelectedFilters(prev => {
                const filtered = prev.filter(f => !(f.categoryId === categoryId && !f.subcategoryId))
                return [...filtered, { categoryId, categoryName, subcategoryId, subcategoryName }]
            })
        }
    }

    const isCategorySelected = (categoryId: number) => {
        return selectedFilters.some(f => f.categoryId === categoryId && !f.subcategoryId)
    }

    const isSubcategorySelected = (categoryId: number, subcategoryId: number) => {
        return selectedFilters.some(f => f.categoryId === categoryId && f.subcategoryId === subcategoryId)
    }

    const clearAllFilters = () => {
        setSelectedFilters([])
        setFilterAnchorEl(null)
    }

    const filteredShows = ShowList.filter(show => {
        const matchesSearch = show.Name.toLowerCase().includes(searchQuery.toLowerCase())
        const matchesCategory = selectedFilters.length === 0 ||
            selectedFilters.some(filter => {
                if (filter.subcategoryId) return show.PodcastSubCategory.Id === filter.subcategoryId
                return show.PodcastCategory.Id === filter.categoryId
            })
        return matchesSearch && matchesCategory
    })

    // Group shows by channel
    const groupedShows = filteredShows.reduce((acc: any, show: any) => {
        const channelKey = show.PodcastChannel
            ? `channel_${show.PodcastChannel.Id}`
            : 'single_shows'

        if (!acc[channelKey]) {
            acc[channelKey] = {
                type: show.PodcastChannel ? 'channel' : 'single',
                channelInfo: show.PodcastChannel,
                shows: []
            }
        }

        acc[channelKey].shows.push(show)
        return acc
    }, {})
    const groupKeys = Object.keys(groupedShows);
    const orderedKeys = [
        ...groupKeys.filter(k => k === 'single_shows'),
        ...groupKeys.filter(k => k !== 'single_shows')
    ];
    const totalShowCount = filteredShows.length

    return (
        <MyShowPageContext.Provider value={{ handleDataChange: fetchShowList, categoryList: categoryList }}>
            <div className="my-show-page">
                {/* Header Section */}
                <div className="my-show-page__header">
                    <div className="my-show-page__header-top flex justify-between items-center mb-6">
                        <div className="my-show-page__search-section flex items-center gap-4 flex-1">
                            <Box className="my-show-page__search-container">
                                <Box className="my-show-page__search-icon">
                                    <Search />
                                </Box>
                                <InputBase
                                    placeholder="Search Show By Name"
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="my-show-page__search-input"
                                />
                            </Box>
                            <Button
                                className="my-show-page__category-filter "
                                variant="contained"
                                startIcon={<FilterList />}
                                onClick={(e) => setFilterAnchorEl(e.currentTarget)}
                            >
                                Category Filter: {selectedFilters.length}
                            </Button>

                            <Menu
                                anchorEl={filterAnchorEl}
                                open={Boolean(filterAnchorEl)}
                                onClose={() => setFilterAnchorEl(null)}
                                PaperProps={{
                                    sx: {
                                        backgroundColor: '#2a2a2a',
                                        color: 'white',
                                        maxWidth: '400px',
                                        minWidth: '350px',
                                        '& .MuiAccordion-root': {
                                            backgroundColor: 'transparent',
                                            boxShadow: 'none',
                                            '&:before': { display: 'none' }
                                        },
                                        '& .MuiAccordionSummary-root': {
                                            backgroundColor: '#333',
                                            borderRadius: '6px',
                                            margin: '4px 0',
                                            minHeight: 'auto',
                                            '&:hover': { backgroundColor: '#444' },
                                            '& .MuiAccordionSummary-content': {
                                                margin: '8px 0',
                                                alignItems: 'center'
                                            }
                                        },
                                        '& .MuiAccordionDetails-root': {
                                            backgroundColor: '#1a1a1a',
                                            padding: '8px 16px',
                                            borderRadius: '6px',
                                            marginBottom: '8px'
                                        }
                                    }
                                }}
                            >
                                <Box sx={{ maxHeight: '400px', overflowY: 'auto', padding: '8px' }}>
                                    {/* Clear All Button */}
                                    {selectedFilters.length > 0 && (
                                        <Box sx={{ textAlign: 'center', mb: 2 }}>
                                            <Button
                                                size="small"
                                                onClick={clearAllFilters}
                                                sx={{
                                                    color: '#ff6b6b',
                                                    textTransform: 'none',
                                                    '&:hover': { backgroundColor: 'rgba(255, 107, 107, 0.1)' }
                                                }}
                                            >
                                                Clear All ({selectedFilters.length})
                                            </Button>
                                            <Divider sx={{ mt: 1, borderColor: '#444' }} />
                                        </Box>
                                    )}

                                    {categoryList.map((category) => (
                                        <Accordion key={category.Id} disableGutters>
                                            <AccordionSummary
                                                expandIcon={<ExpandMore sx={{ color: 'var(--primary-green)' }} />}

                                            >
                                                <Checkbox
                                                    checked={isCategorySelected(category.Id)}
                                                    onClick={(e) => e.stopPropagation()} // ngăn event expand
                                                    onChange={() => handleCategoryToggle(category.Id, category.Name)}
                                                    sx={{
                                                        color: 'var(--primary-green)',
                                                        '&.Mui-checked': { color: 'var(--primary-green)' },

                                                    }}
                                                />
                                                <Typography
                                                    sx={{ color: 'white', fontWeight: 'bold' }}
                                                >
                                                    {category.Name}
                                                </Typography>
                                            </AccordionSummary>
                                            <AccordionDetails>
                                                {category.PodcastSubCategoryList.map((subcategory) => (
                                                    <FormControlLabel
                                                        key={subcategory.Id}
                                                        control={
                                                            <Checkbox
                                                                checked={isSubcategorySelected(category.Id, subcategory.Id)}
                                                                onChange={() =>
                                                                    handleSubcategoryToggle(
                                                                        category.Id,
                                                                        category.Name,
                                                                        subcategory.Id,
                                                                        subcategory.Name
                                                                    )
                                                                }
                                                                size="small"
                                                                sx={{
                                                                    color: 'var(--primary-green)',
                                                                    '&.Mui-checked': { color: 'var(--primary-green)' }
                                                                }}
                                                            />
                                                        }
                                                        label={
                                                            <Typography sx={{ color: '#ccc', fontSize: '0.9rem' }}>
                                                                {subcategory.Name}
                                                            </Typography>
                                                        }
                                                        sx={{
                                                            margin: 0,
                                                            marginLeft: '10px'
                                                        }}
                                                    />
                                                ))}
                                            </AccordionDetails>
                                        </Accordion>
                                    ))}
                                </Box>
                            </Menu>
                        </div>
                        <Modal_Button
                            className="my-show-page__new-show-btn"
                            content="New Show"
                            variant="contained"
                            size='lg'
                            startIcon={<Add />}
                        >
                            <ShowCreate  />
                        </Modal_Button>

                    </div>

                    {/* Filter Tags */}
                    <div className="my-show-page__filter-tags flex items-center gap-2 mb-6">
                        {selectedFilters.map((filter, index) => (
                            <Chip
                                key={`${filter.categoryId}-${filter.subcategoryId || 'category'}-${index}`}
                                label={
                                    filter.subcategoryName
                                        ? `${filter.categoryName} > ${filter.subcategoryName}`
                                        : filter.categoryName
                                }
                                onDelete={() => handleFilterRemove(filter)}
                                className="my-show-page__filter-tag"
                                size="small"
                                sx={{
                                    backgroundColor: filter.subcategoryName ? '#444' : '#2a2a2a',
                                    color: '#9ccc65',
                                    border: '1px solid #9ccc65',
                                    '& .MuiChip-deleteIcon': { color: '#9ccc65' },
                                    '& .MuiChip-label': {
                                        fontSize: '0.8rem',
                                        maxWidth: '200px',
                                        overflow: 'hidden',
                                        textOverflow: 'ellipsis'
                                    }
                                }}
                            />
                        ))}
                    </div>

                    {/* Results Header */}
                    <div className="my-show-page__results-header pt-3">
                        {/* <Typography variant="h5" className="my-show-page__results-count" >
                        {totalShowCount} <span className="text-white font-semibold ml-1">Show{totalShowCount !== 1 ? 's' : ''}</span>
                    </Typography>

                    <div className="my-show-page__sort-section flex items-center gap-2">
                        <Typography className='text-white font-semibold'>Sort By:</Typography>
                        <Button
                            className="my-show-page__sort-btn"
                            variant="outlined"
                        >
                            {sortBy} ↓
                        </Button>
                    </div> */}
                    </div>
                </div>

                {loading ?
                    (<div className="flex justify-center items-center h-100">
                        <Loading />
                    </div>) :
                    (<>
                        {totalShowCount > 0 ? (
                            <div className="my-show-page__content">
                                {orderedKeys.map((key) => {
                                    const group = groupedShows[key];
                                    return (
                                        <div key={key} className="my-show-page__group mb-5">
                                            {/* Group Header */}
                                            <Typography variant="h4" className="my-show-page__group-title mb-4">
                                                {group.type === 'single'
                                                    ? 'Single Shows'
                                                    : (
                                                        <>
                                                            Channel: <span style={{ fontFamily: 'inter', color: 'var(--primary-green)', marginLeft: '5px', fontWeight: 'bold' }}>{group.channelInfo.Name}</span>
                                                        </>
                                                    )
                                                }
                                            </Typography>

                                            <div className="my-show-page__shows-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-6">
                                                {group.shows.map((show: any) => (
                                                    <ShowCard key={show.Id} show={show} />
                                                ))}
                                            </div>
                                        </div>
                                    );
                                })}
                            </div>
                        ) : (
                            <div>
                                <EmptyComponent item="show" subtitle="Try adjusting your search terms or filters" />
                                <Modal_Button
                                    className="my-show-page__new-show-btn"
                                    content="Create New Show"
                                    variant="contained"
                                    size='lg'
                                    startIcon={<Add />}
                                >
                                    <ShowCreate />
                                </Modal_Button>

                            </div>
                        )}
                    </>)}
            </div>
        </MyShowPageContext.Provider>
    );
};

export default MyShowPage;