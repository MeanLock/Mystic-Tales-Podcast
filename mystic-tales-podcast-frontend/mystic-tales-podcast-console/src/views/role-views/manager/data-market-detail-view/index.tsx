import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCardBody, CCol, CFormInput, CFormSelect, CRow, CSpinner, CPagination, CPaginationItem } from '@coreui/react';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import CustomerUpdate from '../../admin/customer-view/CustomerUpdate';
import { Divide, Eye, MagnifyingGlass, ArrowSquareOut } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { useParams } from 'react-router-dom';
import DataMarketDetailTabs from './DataMarketDetailTab';
import { MetricCard } from './MetricCard';
import CashFlowChart from './CashFlowChart';

ModuleRegistry.registerModules([AllCommunityModule]);

interface DataMarketDetailViewProps { }

interface DataMarketDetailViewContextProps {
  handleDataChange: () => void;
  surveyData: SurveyData | null

}


// Mock data for dashboard
const dashboardStats = {
  totalPurchase: '35,000,000đ',
  totalRevenue: '265,000,000đ',
  totalCommission: '300,000,000đ'
};
const chartDataSets = {
  purchase: [20, 35, 25, 45, 30, 55, 40, 65, 50, 70],
  revenue: [30, 25, 40, 35, 50, 30, 60, 45, 70, 55],
  commission: [25, 40, 20, 50, 35, 45, 25, 60, 40, 65],
}
const chartData = [
  { month: 'T1', income: 6, expense: 4 },
  { month: 'T2', income: 8, expense: 5 },
  { month: 'T3', income: 10, expense: 6 },
  { month: 'T4', income: 7, expense: 5 },
  { month: 'T5', income: 9, expense: 6 },
  { month: 'T6', income: 4, expense: 3 },
  { month: 'T7', income: 3, expense: 2 },
  { month: 'T8', income: 6, expense: 4 },
  { month: 'T9', income: 8, expense: 5 },
  { month: 'T10', income: 9, expense: 6 },
  { month: 'T11', income: 5, expense: 4 },
  { month: 'T12', income: 11, expense: 7 },
  { month: 'T13', income: 12, expense: 8 },
  { month: 'T14', income: 8, expense: 6 }
];

const profitData = [
  { rank: 1, amount: '15,000,000đ' },
  { rank: 2, amount: '10,000,000đ' },
  { rank: 3, amount: '10,000,000đ' }
];

const transactionHistory = [
  {
    id: '3f2b7e8c-1234-4e91-bc56-8bb8da29ad4f',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: '9ef23fba-41e2-4b3f-9fe2-2cb0f4ca2fe5',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: 'a8eff74-3345-4b4a-b3ab-1d5bf739f4ec',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: 'd99ff702-d118-4a2b-84f9-0eb0f3b024bb',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: 'fce83d8b-eb57-4a4e-8420-3fa728e91fcb',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: '6d99af5f-4b4a-470a-8c0f-2050e5c7d9b68',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: '0d43ed94-8922-4212-8eab-8ea2f2a1dcf3',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: 'b4cd8d96-744a-4ad1-872e-b40eafde44b6',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '10,000đ',
    versionId: 'svdt-0231-213123',
    time: '20/05/2025'
  },
  {
    id: '5e6c8425-5b41-4ea7-9c18-26ffef7c41d8',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '5,000đ',
    versionId: 'svdt-0w27-125w3',
    time: '20/05/2025'
  },
  {
    id: '8ae2b4c5-9a61-42ed-95a4-f90083ea3b12f',
    type: 'Trả tiền hoa hồng',
    contributorId: '3f2b7e8c-1234',
    amount: '5,000đ',
    versionId: 'svdt-0w27-125w3',
    time: '20/05/2025'
  }
];

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
  imageUrl: string;
}
const mockSurveyData: SurveyData =
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
  platformPercentage: 4,
  imageUrl: "https://samyangvietnam.com/wp-content/uploads/2024/01/can-nang-cua-meo-con-01.jpg"
}
export const DataMarketDetailViewContext = createContext<DataMarketDetailViewContextProps | null>(null);

const DataMarketDetailView: FC<DataMarketDetailViewProps> = () => {
  const { id } = useParams<{ id: string }>();
  const maxValue = Math.max(...chartData.map(d => Math.max(d.income, d.expense)));
  const [activeTab, setActiveTab] = useState('payment-history');
  const [surveyData, setSurveyData] = useState<SurveyData | null>(null);
  const handleDataChange = () => {
    if (id) {
      //console.log("Refreshing data for ID:", id)
      setSurveyData(mockSurveyData)
    }
  }
  useEffect(() => {
    if (id) {
      //console.log('Fetching data for ID:', id);
      setSurveyData(mockSurveyData);
    }
  }, [id]);

  return (
    <div className="data-market-detail">
      <h1 className="data-market-detail__title">Chi Tiết Bộ Data</h1>
      <div className="stats-grid">
        <div className="stats-grid__card">
          <div className="data-card d-flex gap-4">
            <div>
              <h3 className="data-card__title">{surveyData?.title}</h3>
              <p className="data-card__code">{surveyData?.code}</p>
              <img src={surveyData?.imageUrl} className="data-card__image" />
            </div>

            <div className='pt-2'>
              <div className="data-card__info">
                <span className="data-card__info-label" >Ngày đăng</span>
                <span className="data-card__info-value">{surveyData?.date}</span>
              </div>
              <div className="data-card__info">
                <span className="data-card__info-label"> Số lượng Versions</span>
                <span className="data-card__info-value">{surveyData?.versions}</span>
              </div>
              <div className="data-card__info">
                <span className="data-card__info-label">Tỉ lệ lợi nhuận</span>
                <span className="data-card__info-value">{surveyData?.commissionRate}%</span>
              </div>
              <div className="data-card__info" style={{ borderBottom: 'none' }}>
                <span className="data-card__info-label" >Trạng thái</span>
                <span className="data-card__info-value text-success" >{surveyData?.status}</span>
              </div>
            </div>
          </div>
        </div>

        <MetricCard
          icon="💰"
          title="Tổng Lợi Nhuận"
          value={dashboardStats.totalPurchase}
          color="green"
          chartData={chartDataSets.purchase}
        />

        <MetricCard
          icon="👥"
          title="Tổng Doanh Thu"
          value={dashboardStats.totalRevenue}
          color="blue"
          chartData={chartDataSets.revenue}
        />

        <MetricCard
          icon="💎"
          title="Tổng Hoa Hồng"
          value={dashboardStats.totalCommission}
          color="yellow"
          chartData={chartDataSets.commission}
        />
      </div>

      <div className="analysis-section">
        <CashFlowChart />

        <div className="profit-section">
          <h3 className="profit-section__title mb-3">Tỉ trọng lợi nhuận</h3>
          <div className="d-flex align-items-center gap-4 mb-3">
            <span className="profit-section__subtitle">Version</span>
            <span className="profit-section__subtitle">Lợi nhuận</span>
            <span className="profit-section__subtitle">Tỉ trọng</span>
          </div>
          <div>
            {profitData.map((item, index) => (
              <div key={index} className="d-flex align-items-center gap-3 mb-3">
                <span className="profit-section__list-rank">{item.rank}</span>
                <span className="profit-section__list-amount">{item.amount}</span>
                <div className="profit-section__list-bar">
                  <div
                    className="profit-section__list-bar-fill"
                    style={{ width: `${85 - index * 20}%` }}
                  ></div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>

      <DataMarketDetailViewContext.Provider value={{ handleDataChange, surveyData }}>
        <DataMarketDetailTabs />
      </DataMarketDetailViewContext.Provider>

      {/* Tab Navigation */}
      {/* <div className="tab-navigation">
        <button
          className={`tab-navigation__button ${activeTab === 'payment-history' ? 'tab-navigation__button--active' : ''}`}
          onClick={() => setActiveTab('payment-history')}
        >
          Quản Lý Lịch Sử Dòng Tiền
        </button>
        <button
          className={`tab-navigation__button ${activeTab === 'version-management' ? 'tab-navigation__button--active' : ''}`}
          onClick={() => setActiveTab('version-management')}
        >
          Quản Lý Version
        </button>
      </div> */}

      {/* Transaction History Table */}
      {/* {activeTab === 'payment-history' && (
        <div className="transaction-section">
          <h3 className="transaction-section__title">Lịch sử trả hoa hồng</h3>
          <div className="transaction-table">
            <div className="transaction-table__header">
              <div className="transaction-table__cell transaction-table__cell--header">Transaction ID</div>
              <div className="transaction-table__cell transaction-table__cell--header">Transaction Type</div>
              <div className="transaction-table__cell transaction-table__cell--header">To Contributor ID</div>
              <div className="transaction-table__cell transaction-table__cell--header">Amount</div>
              <div className="transaction-table__cell transaction-table__cell--header">Version ID</div>
              <div className="transaction-table__cell transaction-table__cell--header">Transaction Time</div>
            </div>
            {transactionHistory.map((transaction, index) => (
              <div key={index} className="transaction-table__row">
                <div className="transaction-table__cell">{transaction.id}</div>
                <div className="transaction-table__cell">{transaction.type}</div>
                <div className="transaction-table__cell">{transaction.contributorId}</div>
                <div className="transaction-table__cell transaction-table__cell--amount">{transaction.amount}</div>
                <div className="transaction-table__cell">{transaction.versionId}</div>
                <div className="transaction-table__cell">{transaction.time}</div>
              </div>
            ))}
          </div>
        </div>
      )} */}

      {/* {activeTab === 'version-management' && (
        <div className="version-section">
          <h3 className="version-section__title">Quản lý Version</h3>
          <p className="version-section__content">Version management content will be displayed here.</p>
        </div>
      )} */}
    </div>

  );
}

export default DataMarketDetailView;



