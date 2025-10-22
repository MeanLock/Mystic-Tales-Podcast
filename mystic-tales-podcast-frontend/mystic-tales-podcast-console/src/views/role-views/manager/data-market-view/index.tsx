import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { CButton, CButtonGroup, CCard, CCardBody, CCol, CFormInput, CFormSelect, CRow, CSpinner, CPagination, CPaginationItem } from '@coreui/react';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { Divide, Eye, MagnifyingGlass, ArrowSquareOut } from 'phosphor-react';
import { useNavigate } from 'react-router-dom';
import SurveyCardCharts from './SurveyCardCharts';

ModuleRegistry.registerModules([AllCommunityModule]);

interface DataMarketViewProps { }

interface DataMarketViewContextProps {
    handleDataChange: () => void;
}

interface SurveyData {
    id: string;
    title: string;
    code: string;
    date: string;
    time: string;
    status: string;
    versions: number;
    totalRules: number;
    totalPurchaseAmount: string;
    totalContributions: number;
    totalCommissionAmount: string;
    commissionRate: number;
    revenue: string;
    dataOwnerPercentage: number;
    dataMarketPercentage: number;
    platformPercentage: number;
}

export const DataMarketViewContext = createContext<DataMarketViewContextProps | null>(null);

// Mock data
const mockSurveyData: SurveyData[] = [
    {
        id: '1',
        title: 'Sức Khỏe Cơ Bản ',
        code: '#st-001-230-020',
        date: '10/01/2025',
        time: '19:00 PM',
        status: 'Not yet',
        versions: 3,
        totalRules: 1700,
        totalPurchaseAmount: '300,000,000đ',
        totalContributions: 2000000,
        totalCommissionAmount: '240,000,000đ',
        commissionRate: 75,
        revenue: '126,000,000đ',
        dataOwnerPercentage: 50,
        dataMarketPercentage: 15,
        platformPercentage: 4
    },
    {
        id: '2',
        title: 'Khảo Sát Thị Trường',
        code: '#st-001-230-021',
        date: '11/01/2025',
        time: '14:30 PM',
        status: 'Active',
        versions: 2,
        totalRules: 2100,
        totalPurchaseAmount: '450,000,000đ',
        totalContributions: 1800000,
        totalCommissionAmount: '320,000,000đ',
        commissionRate: 80,
        revenue: '180,000,000đ',
        dataOwnerPercentage: 45,
        dataMarketPercentage: 20,
        platformPercentage: 5
    },
    {
        id: '3',
        title: 'Nghiên Cứu Người Dùng',
        code: '#st-001-230-022',
        date: '12/01/2025',
        time: '10:15 AM',
        status: 'Completed',
        versions: 4,
        totalRules: 1500,
        totalPurchaseAmount: '200,000,000đ',
        totalContributions: 2500000,
        totalCommissionAmount: '150,000,000đ',
        commissionRate: 70,
        revenue: '95,000,000đ',
        dataOwnerPercentage: 55,
        dataMarketPercentage: 12,
        platformPercentage: 3
    },
    {
        id: '4',
        title: 'Phân Tích Hành Vi',
        code: '#st-001-230-023',
        date: '13/01/2025',
        time: '16:45 PM',
        status: 'Pending',
        versions: 1,
        totalRules: 1900,
        totalPurchaseAmount: '380,000,000đ',
        totalContributions: 2200000,
        totalCommissionAmount: '280,000,000đ',
        commissionRate: 85,
        revenue: '140,000,000đ',
        dataOwnerPercentage: 48,
        dataMarketPercentage: 18,
        platformPercentage: 6
    },
    {
        id: '5',
        title: 'Đánh Giá Sản Phẩm',
        code: '#st-001-230-024',
        date: '14/01/2025',
        time: '11:20 AM',
        status: 'Draft',
        versions: 3,
        totalRules: 1600,
        totalPurchaseAmount: '250,000,000đ',
        totalContributions: 1900000,
        totalCommissionAmount: '190,000,000đ',
        commissionRate: 72,
        revenue: '110,000,000đ',
        dataOwnerPercentage: 52,
        dataMarketPercentage: 14,
        platformPercentage: 4
    },
    {
        id: '6',
        title: 'Khảo Sát Dịch Vụ',
        code: '#st-001-230-025',
        date: '15/01/2025',
        time: '13:00 PM',
        status: 'Active',
        versions: 2,
        totalRules: 1800,
        totalPurchaseAmount: '320,000,000đ',
        totalContributions: 2100000,
        totalCommissionAmount: '230,000,000đ',
        commissionRate: 78,
        revenue: '135,000,000đ',
        dataOwnerPercentage: 49,
        dataMarketPercentage: 16,
        platformPercentage: 5
    }
];

const SurveyCard: FC<{ data: SurveyData, navigate: (path: string) => void }> = ({ data, navigate }) => {
    const getStatusColor = (status: string) => {
        switch (status.toLowerCase()) {
            case 'active': return 'success';
            case 'completed': return 'info';
            case 'pending': return 'warning';
            case 'draft': return 'secondary';
            default: return 'light';
        }
    };

    return (
        <CCard className="survey-card mb-4">
            <CCardBody className='d-flex flex-row ' style={{ padding: '35px 60px' }}>
                <div className="survey-card__info d-flex flex-column gap-4 pt-3 me-4">
                    <div className='d-flex flex-column '>
                        <h5 className="survey-card__title">{data.title}</h5>
                        <span className="survey-card__code">{data.code}</span>
                    </div>

                    <div className='d-flex flex-column gap-3 '>
                        <div className="survey-card__meta-item">
                            <span className="survey-card__meta-icon">🕐</span>
                            <span>{data.date} - {data.time}</span>
                        </div>
                        <div className="survey-card__meta-item">
                            <span className="survey-card__meta-icon">🕐</span>
                            <span>{data.status}</span>
                        </div>
                    </div>
                </div>



                <div className="survey-card__stats d-flex flex-row gap-5 ">
                    <div className="survey-card__stat-group">
                        <div className="survey-card__stat-item">
                            <span className="survey-card__stat-label">Số lượng versions:</span>
                            <span className="survey-card__stat-value">{data.versions} versions</span>
                        </div>
                        <div className="survey-card__stat-item">
                            <span className="survey-card__stat-label">Tổng lượt mua:</span>
                            <span className="survey-card__stat-value">{data.totalRules} lượt</span>
                        </div>
                        <div className="survey-card__stat-item">
                            <span className="survey-card__stat-label">Tổng tiền mua:</span>
                            <span className="survey-card__stat-value text-success">{data.totalPurchaseAmount}</span>
                        </div>
                        <div className="survey-card__stat-item">
                            <span className="survey-card__stat-label">Tổng lượt đóng góp:</span>
                            <span className="survey-card__stat-value">{data.totalContributions.toLocaleString()} lượt</span>
                        </div>
                        <div className="survey-card__stat-item">
                            <span className="survey-card__stat-label">Tổng tiền hoa hồng:</span>
                            <span className="survey-card__stat-value text-danger">{data.totalCommissionAmount}</span>
                        </div>
                    </div>


                    <div className="d-flex flex-column gap-4">
                        <div className="d-flex justify-content-between align-items-start ">
                            <div className="survey-card__rate-section">
                                <div className="survey-card__rate-item ">
                                    <span className="survey-card__rate-label">Tỷ lệ hoa hồng:</span>
                                    <span className="survey-card__rate-value">{data.commissionRate}%</span>
                                </div>
                                <div className="survey-card__rate-item">
                                    <span className="survey-card__rate-label">Doanh thu:</span>
                                    <span className="survey-card__rate-value">{data.revenue}</span>
                                </div>
                            </div>
                            <CButton variant="ghost" size="sm" className="survey-card__action-btn mt-n2"
                                onClick={() => {
                                    navigate('/data-market/detail/' + data.id);
                                }}>
                                <ArrowSquareOut size={30} />
                            </CButton>
                        </div>

                        <SurveyCardCharts />
                    </div>


                </div>

            </CCardBody>
        </CCard>
    );
};

const DataMarketView: FC<DataMarketViewProps> = () => {
    const [currentPage, setCurrentPage] = useState(1);
    const [searchTerm, setSearchTerm] = useState('');
    const [sortBy, setSortBy] = useState('');
    const navigate = useNavigate();

    const itemsPerPage = 3;

    // Filter and sort data
    const filteredData = useMemo(() => {
        let filtered = mockSurveyData.filter(item =>
            item.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
            item.code.toLowerCase().includes(searchTerm.toLowerCase())
        );

        if (sortBy === 'title') {
            filtered.sort((a, b) => a.title.localeCompare(b.title));
        } else if (sortBy === 'date') {
            filtered.sort((a, b) => new Date(a.date).getTime() - new Date(b.date).getTime());
        } else if (sortBy === 'revenue') {
            filtered.sort((a, b) => parseFloat(a.revenue.replace(/[^\d]/g, '')) - parseFloat(b.revenue.replace(/[^\d]/g, '')));
        }

        return filtered;
    }, [searchTerm, sortBy]);

    // Pagination
    const totalPages = Math.ceil(filteredData.length / itemsPerPage);
    const startIndex = (currentPage - 1) * itemsPerPage;
    const endIndex = startIndex + itemsPerPage;
    const currentItems = filteredData.slice(startIndex, endIndex);

    const handlePageChange = (page: number) => {
        setCurrentPage(page);
    };

    const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setSearchTerm(e.target.value);
        setCurrentPage(1); // Reset to first page when searching
    };

    const handleSortChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
        setSortBy(e.target.value);
        setCurrentPage(1); // Reset to first page when sorting
    };

    return (
        <div className="data-market">
            <div className="data-market__header mb-4">
                <div className='data-market__search-group'>
                    <div className="data-market__search-input-wrapper">
                        <MagnifyingGlass size={20} weight="light" className="search-icon" />
                        <CFormInput
                            type="search"
                            className="data-market__search-input"
                            placeholder="Tìm kiếm..."
                            aria-label="Search"
                            size="sm"
                            value={searchTerm}
                            onChange={handleSearchChange}
                        />
                    </div>
                    <CFormSelect
                        className="data-market__sort-select"
                        aria-label="Sort selection"
                        value={sortBy}
                        onChange={handleSortChange}
                    >
                        <option value="">Sắp xếp theo: Không</option>
                        <option value="title">Sắp xếp theo: Tên</option>
                        <option value="date">Sắp xếp theo: Ngày</option>
                        <option value="revenue">Sắp xếp theo: Doanh thu</option>
                    </CFormSelect>
                </div>

                <CButton className='data-market__add-button' href="#">
                    Tạo Dữ Liệu Mới
                </CButton>
            </div>

            <div className="data-market__content">
                <div className="data-market__results-info mb-3">
                    <span className="text-muted">
                        Hiển thị {startIndex + 1}-{Math.min(endIndex, filteredData.length)} của {filteredData.length} kết quả
                    </span>
                </div>

                <div className="data-market__cards">
                    {currentItems.map((item) => (
                        <SurveyCard key={item.id} data={item} navigate={navigate} />
                    ))}
                </div>

                {filteredData.length === 0 && (
                    <div className="text-center py-5">
                        <p className="text-muted">Không tìm thấy dữ liệu phù hợp</p>
                    </div>
                )}

                {totalPages > 1 && (
                    <div className="data-market__pagination d-flex justify-content-center mt-4">
                        <CPagination aria-label="Data pagination">
                            <CPaginationItem
                                disabled={currentPage === 1}
                                onClick={() => handlePageChange(currentPage - 1)}
                            >
                                Trước
                            </CPaginationItem>
                            {Array.from({ length: totalPages }, (_, i) => i + 1).map((page) => (
                                <CPaginationItem
                                    key={page}
                                    active={page === currentPage}
                                    onClick={() => handlePageChange(page)}
                                >
                                    {page}
                                </CPaginationItem>
                            ))}
                            <CPaginationItem
                                disabled={currentPage === totalPages}
                                onClick={() => handlePageChange(currentPage + 1)}
                            >
                                Sau
                            </CPaginationItem>
                        </CPagination>
                    </div>
                )}
            </div>
        </div>
    );
}

export default DataMarketView;