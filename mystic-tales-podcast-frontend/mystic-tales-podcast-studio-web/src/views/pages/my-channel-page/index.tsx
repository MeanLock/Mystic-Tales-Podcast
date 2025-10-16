import React, { FC, useEffect, useState } from 'react';
import { Button, TextField, InputAdornment, Chip, Box, Typography, Card, CardMedia, CardContent, Badge, Menu, MenuItem, FormControlLabel, Checkbox, Accordion, AccordionSummary, AccordionDetails, Divider, InputBase } from '@mui/material';
import { Search, Add, FilterList, Favorite, PlayArrow, ExpandMore } from '@mui/icons-material';
import './styles.scss';

// Mock categories with subcategories for filter
export const mockCategoriesWithSub = [
    {
        id: 1,
        name: "Spirituality",
        subcategories: [
            { id: 3, name: "Mindfulness" },
            { id: 11, name: "Meditation" },
            { id: 12, name: "Buddhism" }
        ]
    },
    {
        id: 2,
        name: "Technology",
        subcategories: [
            { id: 4, name: "AI & Data Science" },
            { id: 13, name: "Software Development" },
            { id: 14, name: "Cybersecurity" }
        ]
    },
    {
        id: 3,
        name: "Health & Wellness",
        subcategories: [
            { id: 5, name: "Mental Health" },
            { id: 15, name: "Physical Fitness" },
            { id: 16, name: "Nutrition" }
        ]
    },
    {
        id: 4,
        name: "Mythology",
        subcategories: [
            { id: 6, name: "Norse Mythology" },
            { id: 17, name: "Greek Mythology" },
            { id: 18, name: "Asian Mythology" }
        ]
    },
    {
        id: 5,
        name: "Science",
        subcategories: [
            { id: 7, name: "Astronomy" },
            { id: 19, name: "Physics" },
            { id: 20, name: "Biology" }
        ]
    },
    {
        id: 6,
        name: "Horror",
        subcategories: [
            { id: 8, name: "Dark Mythology" },
            { id: 21, name: "Urban Legends" },
            { id: 22, name: "Paranormal" }
        ]
    },
    {
        id: 7,
        name: "Military",
        subcategories: [
            { id: 9, name: "Defense Technology" },
            { id: 23, name: "Military History" },
            { id: 24, name: "Strategy & Tactics" }
        ]
    }
];

interface SelectedFilter {
    categoryId: number;
    categoryName: string;
    subcategoryId?: number;
    subcategoryName?: string;
}

export const mockData: any = {
    ChannelList: [
        {
            Id: 1,
            Name: "Thần thoại Bắc Âu",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 5320,
            TotalShow: 5,
            PodcastCategory: {
                Id: 1,
                Name: "Spirituality",
            },
            PodcastSubCategory: {
                Id: 3,
                Name: "Mindfulness",
                PodcastCategoryId: 1,
            },
            CreatedAt: "2025-09-10T08:45:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 2,
            Name: "Tech Deep Dive",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 5320,
            TotalShow: 5,
            PodcastCategory: {
                Id: 2,
                Name: "Technology",
            },
            PodcastSubCategory: {
                Id: 4,
                Name: "AI & Data Science",
                PodcastCategoryId: 2,
            },
            CreatedAt: "2025-07-22T10:12:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 3,
            Name: "Healthy Mind",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 2180,
            TotalShow: 3,
            PodcastCategory: {
                Id: 3,
                Name: "Health & Wellness",
            },
            PodcastSubCategory: {
                Id: 5,
                Name: "Mental Health",
                PodcastCategoryId: 3,
            },
            CreatedAt: "2025-08-05T06:40:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 2,
                Name: "Unpublished",
            },
        },
        {
            Id: 4,
            Name: "Thần Thoại Bắc Âu",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 8760,
            TotalShow: 15,
            PodcastCategory: {
                Id: 4,
                Name: "Mythology",
            },
            PodcastSubCategory: {
                Id: 6,
                Name: "Norse Mythology",
                PodcastCategoryId: 4,
            },
            CreatedAt: "2025-06-15T06:40:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 5,
            Name: "Khoa Học & Vũ Trụ",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 12450,
            TotalShow: 22,
            PodcastCategory: {
                Id: 5,
                Name: "Science",
            },
            PodcastSubCategory: {
                Id: 7,
                Name: "Astronomy",
                PodcastCategoryId: 5,
            },
            CreatedAt: "2025-05-20T06:40:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 6,
            Name: "Ác Thần Xung Quanh Thế Giới",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 6890,
            TotalShow: 8,
            PodcastCategory: {
                Id: 6,
                Name: "Horror",
            },
            PodcastSubCategory: {
                Id: 8,
                Name: "Dark Mythology",
                PodcastCategoryId: 6,
            },
            CreatedAt: "2025-07-10T06:40:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 7,
            Name: "Khí Sư Quốc Phòng",
            MainImageFileKey: "main_mystic_talks.png",
            TotalFavorite: 3420,
            TotalShow: 6,
            PodcastCategory: {
                Id: 7,
                Name: "Military",
            },
            PodcastSubCategory: {
                Id: 9,
                Name: "Defense Technology",
                PodcastCategoryId: 7,
            },
            CreatedAt: "2025-09-01T06:40:31.391Z",
            UpdatedAt: "2025-10-16T06:40:31.391Z",
            CurrentStatus: {
                Id: 3,
                Name: "Draft",
            },
        },
    ],
};

interface MyChannelPageProps { }

const MyChannelPage: FC<MyChannelPageProps> = () => {
    const [ChannelList, setChannelList] = useState<any[]>([])
    const [searchQuery, setSearchQuery] = useState('')
    const [selectedFilters, setSelectedFilters] = useState<SelectedFilter[]>([])
    const [sortBy, setSortBy] = useState('Release Date')
    const [filterAnchorEl, setFilterAnchorEl] = useState<null | HTMLElement>(null)

    const fetchChannelList = async () => {
        setChannelList(mockData.ChannelList)
    }

    useEffect(() => {
        fetchChannelList()
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

        // Filter by category/subcategory if any filters are selected
        const matchesCategory = selectedFilters.length === 0 ||
            selectedFilters.some(filter => {
                // If filter has subcategory, match exact subcategory
                if (filter.subcategoryId) {
                    return channel.PodcastSubCategory.Id === filter.subcategoryId
                }
                // If filter only has category, match any subcategory in that category
                return channel.PodcastCategory.Id === filter.categoryId
            })

        return matchesSearch && matchesCategory
    })

    return (
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

                                {mockCategoriesWithSub.map((category) => (
                                    <Accordion key={category.id} disableGutters>
                                        <AccordionSummary
                                            expandIcon={<ExpandMore sx={{ color: 'var(--primary-green)' }} />}

                                        >
                                            <Checkbox
                                                checked={isCategorySelected(category.id)}
                                                onClick={(e) => e.stopPropagation()} // ngăn event expand
                                                onChange={() => handleCategoryToggle(category.id, category.name)}
                                                sx={{
                                                    color: 'var(--primary-green)',
                                                    '&.Mui-checked': { color: 'var(--primary-green)' },

                                                }}
                                            />
                                            <Typography
                                                sx={{ color: 'white', fontWeight: 'bold' }}
                                            >
                                                {category.name}
                                            </Typography>
                                        </AccordionSummary>
                                        <AccordionDetails>
                                            {category.subcategories.map((subcategory) => (
                                                <FormControlLabel
                                                    key={subcategory.id}
                                                    control={
                                                        <Checkbox
                                                            checked={isSubcategorySelected(category.id, subcategory.id)}
                                                            onChange={() =>
                                                                handleSubcategoryToggle(
                                                                    category.id,
                                                                    category.name,
                                                                    subcategory.id,
                                                                    subcategory.name
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
                                                            {subcategory.name}
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

                    <Button
                        className="my-channel-page__new-channel-btn"
                        variant="contained"
                        startIcon={<Add />}

                    >
                        New Channel
                    </Button>
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
                <div className="my-channel-page__results-header pt-3">
                    <Typography variant="h5" className="my-channel-page__results-count" >
                        {filteredChannels.length} <span className="text-white font-semibold ml-1">Channels</span>
                    </Typography>

                    <div className="my-channel-page__sort-section flex items-center gap-2">
                        <Typography className='text-white font-semibold'>Sort By:</Typography>
                        <Button
                            className="my-channel-page__sort-btn"
                            variant="outlined"
                        >
                            {sortBy} ↓
                        </Button>
                    </div>
                </div>
            </div>

            {/* Channels Grid */}
            {filteredChannels.length > 0 ? (
                <div className="my-channel-page__channels-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 ">
                    {filteredChannels.map((channel) => (
                        <Card key={channel.Id} className="my-channel-page__channel-card">
                            <div className="my-channel-page__channel-image-container relative">
                                <CardMedia
                                    component="img"
                                    height="200"
                                    image={`https://picsum.photos/300/300?random=${channel.Id}`}
                                    alt={channel.Name}
                                    className="my-channel-page__channel-image"
                                />

                                {/* Overlay Stats */}
                                <div className="my-channel-page__channel-stats absolute top-2 left-2 flex flex-col gap-1">
                                    <div className="my-channel-page__channel-stat text-xs ">
                                        <PlayArrow />
                                        {channel.TotalShow} Shows
                                    </div>
                                    <div className="my-channel-page__channel-stat text-xs ">
                                        <Favorite />
                                        {channel.TotalFavorite.toLocaleString()}
                                    </div>
                                </div>
                            </div>

                            <CardContent className="my-channel-page__channel-info ">
                                <Typography
                                    variant="h6"
                                    className="my-channel-page__channel-title "
                                >
                                    {channel.Name}
                                </Typography>

                                <div className="my-channel-page__channel-status">
                                    <Chip
                                        label={channel.CurrentStatus.Name}
                                        size="small"
                                        className={`my-channel-page__status-chip my-channel-page__status-chip--${channel.CurrentStatus.Name.toLowerCase()}`}
                                    />
                                </div>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            ) : (
                <div className="my-channel-page__empty-state">
                    <Search className="my-channel-page__empty-state-icon" />
                    <Typography variant="h5" className="my-channel-page__empty-state-title">
                        No channels found
                    </Typography>
                    <Typography className="my-channel-page__empty-state-description">
                        Try adjusting your search terms or filters
                    </Typography>
                    <Button
                        variant="contained"
                        startIcon={<Add />}
                        sx={{
                            backgroundColor: '#9ccc65',
                            color: '#000',
                            fontWeight: 'bold',
                            borderRadius: '8px',
                            textTransform: 'none',
                            '&:hover': { backgroundColor: '#8bc34a' }
                        }}
                    >
                        Create New Channel
                    </Button>
                </div>
            )}
        </div>
    );
};

export default MyChannelPage;