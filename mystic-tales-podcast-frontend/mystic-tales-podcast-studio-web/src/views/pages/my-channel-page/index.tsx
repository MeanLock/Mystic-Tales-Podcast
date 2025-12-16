import React, { createContext, FC, useEffect, useState } from 'react';
import { Button, TextField, InputAdornment, Chip, Box, Typography, Card, CardMedia, CardContent, Badge, Menu, MenuItem, FormControlLabel, Checkbox, Accordion, AccordionSummary, AccordionDetails, Divider, InputBase } from '@mui/material';
import { Search, Add, FilterList, Favorite, PlayArrow, ExpandMore } from '@mui/icons-material';
import './styles.scss';
import { EmptyComponent } from '@/views/components/common/empty';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import ChannelCreate from './ChannelCreate';
import { getChannelList } from '@/core/services/channel/channel.service';
import { loginRequiredAxiosInstance } from '@/core/api/rest-api/config/instances/v2';
import Loading from '@/views/components/common/loading';
import ChannelCard from './components/ChannelCard';
import { getCategories } from '@/core/services/misc/category.service';


interface SelectedFilter {
    categoryId: number;
    categoryName: string;
    subcategoryId?: number;
    subcategoryName?: string;
}


interface MyChannelPageProps { }
interface MyChannelPageContextProps {
    handleDataChange: () => void;
    categoryList: any[];
}

export const MyChannelPageContext = createContext<MyChannelPageContextProps | null>(null);

const MyChannelPage: FC<MyChannelPageProps> = () => {
    const [ChannelList, setChannelList] = useState<any[]>([])
    const [categoryList, setCategoryList] = useState<any[]>([])
    const [searchQuery, setSearchQuery] = useState('')
    const [selectedFilters, setSelectedFilters] = useState<SelectedFilter[]>([])
    const [filterAnchorEl, setFilterAnchorEl] = useState<null | HTMLElement>(null)
    const [loading, setLoading] = useState(false);
    const [sortDirection, setSortDirection] = useState<'desc' | 'asc'>('desc')

    const fetchChannelList = async () => {
        setLoading(true);
        try {
            const channelList = await getChannelList(loginRequiredAxiosInstance);
            console.log("Fetched channel list:", channelList);
            if (channelList.success) {
                setChannelList(channelList.data.ChannelList);
            } else {
                console.error('API Error:', channelList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch channel list:', error);
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
        fetchChannelList()
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

    const filteredChannels = ChannelList.filter(channel => {
        const matchesSearch = channel.Name.toLowerCase().includes(searchQuery.toLowerCase())
        const matchesCategory = selectedFilters.length === 0 ||
            selectedFilters.some(filter => {
                if (filter.subcategoryId) return channel.PodcastSubCategory.Id === filter.subcategoryId
                return channel.PodcastCategory.Id === filter.categoryId
            })
        return matchesSearch && matchesCategory
    })
    const displayedChannels = React.useMemo(() => {
        const parse = (v: any) => {
            const s = v ?? ''
            const t = Date.parse(s)
            return Number.isFinite(t) ? t : 0
        }
        return [...filteredChannels].sort((a, b) => {
            const aTime = parse(a.CreatedAt)
            const bTime = parse(b.CreatedAt)
            return sortDirection === 'desc' ? bTime - aTime : aTime - bTime
        })
    }, [filteredChannels, sortDirection])
    return (
        <MyChannelPageContext.Provider value={{ handleDataChange: fetchChannelList, categoryList: categoryList }}>
            <div className="my-channel-page">
                {/* Header Section */}
                <div className="my-channel-page__header">
                    <div className="my-channel-page__header-top flex justify-between items-center mb-6">
                        <div className="my-channel-page__search-section flex items-center gap-4 flex-1">
                            <Box className="my-channel-page__search-container">
                                <Box className="my-channel-page__search-icon">
                                    <Search />
                                </Box>
                                <InputBase
                                    placeholder="Search Channel By Name"
                                    value={searchQuery}
                                    onChange={(e) => setSearchQuery(e.target.value)}
                                    className="my-channel-page__search-input"
                                />
                            </Box>
                            <Button
                                className="my-channel-page__category-filter "
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
                                                        key={subcategory.id}
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
                            className="my-channel-page__new-channel-btn"
                            content="New Channel"
                            color="primary"
                            variant="contained"
                            size='lg'
                            startIcon={<Add />}
                        >
                            <ChannelCreate />
                        </Modal_Button>
                    </div>

                    {/* Filter Tags */}
                    <div className="my-channel-page__filter-tags flex items-center gap-2 mb-6">
                        {selectedFilters.map((filter, index) => (
                            <Chip
                                key={`${filter.categoryId}-${filter.subcategoryId || 'category'}-${index}`}
                                label={
                                    filter.subcategoryName
                                        ? `${filter.categoryName} > ${filter.subcategoryName}`
                                        : filter.categoryName
                                }
                                onDelete={() => handleFilterRemove(filter)}
                                className="my-channel-page__filter-tag"
                                size="small"
                                sx={{
                                    backgroundColor: filter.subcategoryName ? '#444' : '#2a2a2a',
                                    color: 'var(--primary-green)',
                                    border: '1px solid var(--primary-green)',
                                    boxShadow: '1px 1px 5px rgba(12, 254, 4, 0.18)',
                                    '& .MuiChip-deleteIcon': { color: 'var(--primary-green)' },
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
                    <div className="my-channel-page__results-header pt-3">
                        <Typography variant="h5" className="my-channel-page__results-count" >
                            {filteredChannels.length} <span className="text-white font-semibold ml-1">Channels</span>
                        </Typography>

                        <div className="my-channel-page__sort-section flex items-center gap-2">
                            <Typography className='text-white font-semibold'>Sort By:</Typography>
                            <Button
                                className="my-channel-page__sort-btn"
                                variant="outlined"
                                onClick={() => setSortDirection(d => (d === 'desc' ? 'asc' : 'desc'))}
                            >
                                <span>Created At</span> {` ${sortDirection === 'desc' ? ' ↓' : ' ↑'}`}
                            </Button>
                        </div>
                    </div>
                </div>
                {loading ?
                    (<div className="flex justify-center items-center h-100">
                        <Loading />
                    </div>) :
                    (<>
                        {displayedChannels.length > 0 ? (
                            <div className="my-channel-page__channels-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 ">
                                {displayedChannels.map((channel) => (
                                    <ChannelCard key={channel.Id} channel={channel} />
                                ))}
                            </div>
                        ) : (
                            <div >
                                <EmptyComponent item="channel" subtitle="Try adjusting your search terms or filters" />

                                <Modal_Button
                                    className="my-channel-page__new-channel-btn"
                                    content="Create New Channel"
                                    color="primary"
                                    variant="contained"
                                    size='lg'
                                    startIcon={<Add />}
                                >
                                    <ChannelCreate />
                                </Modal_Button>
                            </div>
                        )}
                    </>)}
            </div>
        </MyChannelPageContext.Provider>
    );
};

export default MyChannelPage;