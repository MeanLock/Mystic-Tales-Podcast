import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';
import { Title } from 'chart.js';
import { DMCAAccusation } from '@/core/types';
import { useNavigate } from 'react-router-dom';

export const mockList: any = {
  DMCAAccusationList: [
    {
      Id: 1,
      PodcastShow: {
        Id: "9b8f7e65-1234-4cde-8abc-9876543210ab",
        Name: "The Dark Whispers",
      },
      PodcastEpisode: {
        Id: "7a6b5c4d-5678-43ef-9a12-6543210fedcb",
        Title: "Echoes in the Forest",
      },
      AssignedStaff: {
        Id: 101,
        FullName: "Nguyen Van Thinh",
        Email: "thinh.nguyen@example.com",
      },
      LastLawsuitCheckingAlertAt: "2025-10-05T15:30:00.000Z",
      CreatedAt: "2025-09-28T09:45:00.000Z",
      UpdatedAt: "2025-10-05T15:30:00.000Z",
    },
    {
      Id: 2,
      PodcastShow: {
        Id: "5e4d3c2b-9012-4fed-a345-876543210abc",
        Name: "Midnight Chronicles",
      },
      PodcastEpisode: {
        Id: "4d3c2b1a-2345-4abc-b678-76543210abcd",
        Title: "The Haunting of Willow Creek",
      },
      AssignedStaff: {
        Id: 102,
        FullName: "Tran Thi Mai",
        Email: "mai.tran@example.com",
      },
      LastLawsuitCheckingAlertAt: "2025-10-04T11:00:00.000Z",
      CreatedAt: "2025-09-25T08:20:00.000Z",
      UpdatedAt: "2025-10-04T11:00:00.000Z",
    },
    {
      Id: 3,
      PodcastShow: {
        Id: "3c2b1a09-8765-4def-b901-6543210abcdef",
        Name: "Mystic Realms",
      },
      PodcastEpisode: {
        Id: "2b1a0987-3456-4cba-c234-543210abcdef",
        Title: "Shadows of the Deep Sea",
      },
      AssignedStaff: null,
      LastLawsuitCheckingAlertAt: null,
      CreatedAt: "2025-09-22T10:15:00.000Z",
      UpdatedAt: "2025-10-02T16:10:00.000Z",
    },
  ],
};

ModuleRegistry.registerModules([AllCommunityModule]);

interface DMCAAccusationViewProps { }
interface DMCAAccusationViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: DMCAAccusation[];
}

export const DMCAAccusationViewContext = createContext<DMCAAccusationViewContextProps | null>(null);

const state_creator = (table: DMCAAccusation[], navigate : (path: string) => void) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.3 },
      { headerName: "Podcast Show", field: "PodcastShow.Name" },
      { headerName: "Podcast Episode", field: "PodcastEpisode.Title" },
      {
        headerName: "Lawsuit Checking",
        flex: 0.6,
        valueGetter: (params: { data: DMCAAccusation }) => formatDate(params.data.LastLawsuitCheckingAlertAt),
      },
      {
        headerName: "Created At",
        flex: 0.5,
        valueGetter: (params: { data: DMCAAccusation }) => formatDate(params.data.CreatedAt),
      },
      {
        headerName: "Actions",
        cellClass: "d-flex justify-content-center py-0",
        flex: 0.5,
        cellRenderer: (params: any) => {
          const DMCAAccusationId  = params.data.Id
          return (
            <div className="d-flex gap-2 align-items-center h-100">
              <CButton
                onClick={() => navigate("/staff/dmca-accusation/detail/" + DMCAAccusationId )}
              >
                 <Eye size={27} color='var(--secondary-green)'  />
              </CButton>
            </div>
          )
        },
      },
    ],
    rowData: table

  }
  return state
}

const DMCAAccusationView: FC<DMCAAccusationViewProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const navigate = useNavigate();
  // const handleDataChange = async () => {
  //   setIsLoading(true);
  //   try {
  //     const accountList = await getDMCAAccusationAccounts(adminAxiosInstance);
  //     if (accountList.success) {
  //       setState(state_creator(accountList.data.Accounts));
  //     } else {
  //       console.error('API Error:', accountList.message);
  //     }
  //   } catch (error) {
  //     console.error('Lỗi khi fetch DMCAAccusation accounts:', error);
  //   } finally {
  //     setIsLoading(false);
  //   }
  // }
  const handleDataChange = async () => {
    setIsLoading(false);
    setState(state_creator(mockList.DMCAAccusationList, navigate));

  }
  useEffect(() => {
    handleDataChange()
  }, [])

  const defaultColDef = useMemo(() => {
    return {
      flex: 1,
      filter: true,
      autoHeight: true,
      resizable: true,
      wrapText: true,
      cellClass: 'd-flex align-items-center',
      editable: false
    };
  }, [])
  return (
    <DMCAAccusationViewContext.Provider value={{ handleDataChange: handleDataChange }}>
            <h2 className="mb-4 fw-bold" style={{ color: 'var(--primary-grey)', borderBottom: '2px solid var(--primary-grey)', paddingBottom: '0.7rem' }}>DMCA Accusation</h2>
      <CRow >
        <CCol xs={12}>
          {isLoading ? (
            <SurveyTalkLoading />
          ) : (
            <div
              id="DMCAAccusation-table"
            >
              <AgGridReact
                columnDefs={state?.columnDefs}
                rowData={state?.rowData}
                defaultColDef={defaultColDef}
                rowHeight={70}
                headerHeight={40}
                pagination={true}
                paginationPageSize={10}
                paginationPageSizeSelector={[10, 20, 50, 100]}
                domLayout='autoHeight'
              />
            </div>)}
        </CCol>
      </CRow>

    </DMCAAccusationViewContext.Provider>
  )
}

export default DMCAAccusationView;