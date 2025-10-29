import React, { FC, useEffect, useState } from 'react';
import { Button, TextField, InputAdornment, Chip, Box, Typography, Card, CardMedia, CardContent, Badge, Menu, MenuItem, FormControlLabel, Checkbox, Accordion, AccordionSummary, AccordionDetails, Divider, InputBase } from '@mui/material';
import { Search, Add, FilterList,PersonAdd, PlayArrow, ExpandMore } from '@mui/icons-material';
import './styles.scss';
import { EmptyComponent } from '@/views/components/common/empty';


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
    ShowList: [
        {
            Id: 1,
            Name: "Cuộc đối Kjosrax",
            MainImageFileKey: "/images/show1.jpg",
            TotalFollow: 1240,
            EpisodeCount: 15,
            PodcastCategory: {
                Id: 1,
                Name: "Spirituality",
            },
            PodcastSubCategory: {
                Id: 11,
                Name: "Mindfulness",
                PodcastCategoryId: 1,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "Thần Thoại Bắc Âu",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 2,
            Name: "Soul Games: Tự Game ...",
            MainImageFileKey: "/images/show2.jpg",
            TotalFollow: 987,
            EpisodeCount: 15,
            PodcastCategory: {
                Id: 4,
                Name: "Mythology",
            },
            PodcastSubCategory: {
                Id: 17,
                Name: "Greek Mythology",
                PodcastCategoryId: 4,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f6",
                Name: "Thần Thoại Bắc ",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 2,
                Name: "Ready to Release",
            },
        },
        {
            Id: 3,
            Name: "Thế nào là cuốn hút ?",
            MainImageFileKey: "/images/show3.jpg",
            TotalFollow: 1532,
            EpisodeCount: 1,
            PodcastCategory: {
                Id: 4,
                Name: "Mythology",
            },
            PodcastSubCategory: {
                Id: 18,
                Name: "Asian Mythology",
                PodcastCategoryId: 4,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "Thần Thoại Bắc Âu",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 2,
                Name: "Draft",
            },
        },
        {
            Id: 4,
            Name: "Chuyện Thằng Carlos",
            MainImageFileKey: "/images/show4.jpg",
            TotalFollow: 823,
            EpisodeCount: 1,
            PodcastCategory: {
                Id: 6,
                Name: "Horror",
            },
            PodcastSubCategory: {
                Id: 21,
                Name: "Urban Legends",
                PodcastCategoryId: 6,
            },
            PodcastChannel: null,
            CurrentStatus: {
                Id: 3,
                Name: "Taken Down",
            },
        },
        {
            Id: 5,
            Name: "Thần Vũ thời cổ đại học",
            MainImageFileKey: "/images/show5.jpg",
            TotalFollow: 456,
            EpisodeCount: 15,
            PodcastCategory: {
                Id: 4,
                Name: "Mythology",
            },
            PodcastSubCategory: {
                Id: 17,
                Name: "Greek Mythology",
                PodcastCategoryId: 4,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "Thần Thoại Bắc Âu",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 1,
                Name: "Removed",
            },
        },
        {
            Id: 6,
            Name: "Lời thì thầm từ Loki hư",
            MainImageFileKey: "/images/show6.jpg",
            TotalFollow: 678,
            EpisodeCount: 15,
            PodcastCategory: {
                Id: 4,
                Name: "Mythology",
            },
            PodcastSubCategory: {
                Id: 6,
                Name: "Norse Mythology",
                PodcastCategoryId: 4,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "Thần Thoại Bắc Âu",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
        {
            Id: 7,
            Name: "Grim Vestige of Satan",
            MainImageFileKey: "/images/show7.jpg",
            TotalFollow: 234,
            EpisodeCount: 15,
            PodcastCategory: {
                Id: 6,
                Name: "Horror",
            },
            PodcastSubCategory: {
                Id: 22,
                Name: "Paranormal",
                PodcastCategoryId: 6,
            },
            PodcastChannel: {
                Id: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
                Name: "Thần Thoại Bắc Âu",
                MainImageFileKey: "/images/channel1.jpg",
            },
            CurrentStatus: {
                Id: 1,
                Name: "Published",
            },
        },
    ],
};

interface MyShowPageProps { }

const MyShowPage: FC<MyShowPageProps> = () => {
    const [ShowList, setShowList] = useState<any[]>([])
    const [searchQuery, setSearchQuery] = useState('')
    const [selectedFilters, setSelectedFilters] = useState<SelectedFilter[]>([])
    const [sortBy, setSortBy] = useState('Release Date')
    const [filterAnchorEl, setFilterAnchorEl] = useState<null | HTMLElement>(null)

    const fetchShowList = async () => {
        setShowList(mockData.ShowList)
    }

    useEffect(() => {
        fetchShowList()
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

        // Filter by category/subcategory if any filters are selected
        const matchesCategory = selectedFilters.length === 0 ||
            selectedFilters.some(filter => {
                // If filter has subcategory, match exact subcategory
                if (filter.subcategoryId) {
                    return show.PodcastSubCategory.Id === filter.subcategoryId
                }
                // If filter only has category, match any subcategory in that category
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
                                placeholder="Search Channel By Name"
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
                        className="my-show-page__new-show-btn"
                        variant="contained"
                        startIcon={<Add />}
                    >
                        New Show
                    </Button>
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
                    <Typography variant="h5" className="my-show-page__results-count" >
                        {/* {totalShowCount} <span className="text-white font-semibold ml-1">Show{totalShowCount !== 1 ? 's' : ''}</span> */}
                    </Typography>

                    <div className="my-show-page__sort-section flex items-center gap-2">
                        <Typography className='text-white font-semibold'>Sort By:</Typography>
                        <Button
                            className="my-show-page__sort-btn"
                            variant="outlined"
                        >
                            {sortBy} ↓
                        </Button>
                    </div>
                </div>
            </div>

            {/* Shows Grid by Groups */}
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

                                {/* Shows Grid */}
                                <div className="my-show-page__shows-grid grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-5 gap-6">
                                    {group.shows.map((show: any) => (
                                        <Card
                                            key={show.Id}
                                            className="my-show-page__show-card"
                                            onClick={() => {
                                                window.open(`/show/${show.Id}/overview`);
                                            }}
                                        >
                                            <div className="my-show-page__show-image-container relative">
                                                <CardMedia
                                                    component="img"
                                                    image={`https://picsum.photos/300/300?random=${show.Id}`}
                                                    alt={show.Name}
                                                    className="my-show-page__show-image"
                                                />

                                                {/* Overlay Stats */}
                                                <div className="my-show-page__show-stats absolute top-2 left-2 flex flex-col gap-1">
                                                    <div className="my-show-page__show-stat text-xs">
                                                        <PlayArrow />
                                                        {show.EpisodeCount} Episodes
                                                    </div>
                                                    <div className="my-show-page__show-stat-follow text-xs">
                                                        <PersonAdd />
                                                        {show.TotalFollow.toLocaleString()} 
                                                    </div>
                                                </div>
                                            </div>

                                            <CardContent className="my-show-page__show-info">
                                                <Typography
                                                    variant="h6"
                                                    className="my-show-page__show-title"
                                                >
                                                    {show.Name}
                                                </Typography>

                                                <div className="my-show-page__show-status">
                                                    <Chip
                                                        label={show.CurrentStatus.Name}
                                                        size="small"
                                                        className={`my-show-page__status-chip my-show-page__status-chip--${show.CurrentStatus.Name.toLowerCase().replace(/\s+/g, '-')}`}
                                                    />
                                                </div>
                                            </CardContent>
                                        </Card>
                                    ))}
                                </div>
                            </div>
                        );
                    })}
                </div>
            ) : (
                <div>
                    <EmptyComponent item="show" subtitle="Try adjusting your search terms or filters" />
                    <Button
                        variant="contained"
                        startIcon={<Add />}
                        sx={{
                            backgroundColor: 'var(--primary-green)',
                            color: '#000',
                            fontWeight: 'bold',
                            borderRadius: '8px',
                            textTransform: 'none',
                            '&:hover': { backgroundColor: '#8bc34a' }
                        }}
                    >
                        Create New Show
                    </Button>
                </div>
            )}
        </div>
    );
};

export default MyShowPage;