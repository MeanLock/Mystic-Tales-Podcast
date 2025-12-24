import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye, Plus } from 'phosphor-react';
import { DMCAAccusation } from '@/core/types';
import { useNavigate } from 'react-router-dom';
import Loading from '../../../components/common/loading';
import { getDMCAList } from '@/core/services/dmca/dmca.service';
import Modal_Button from '@/views/components/common/modal/ModalButton';
import { staffAxiosInstance } from '@/core/api/rest-api/config/instances/v2/staff-axios-instance';
import { formatDate } from '@/core/utils/date.util';

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

const show_state = (table: DMCAAccusation[], navigate: (path: string) => void) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.3 },
      { headerName: "Accuser", field: "AccuserFullName" },
      { headerName: "Accuser Email", field: "AccuserEmail" },
      { headerName: "Podcast Show", field: "PodcastShow.Name" },
       {
              headerName: "Created At",
              flex: 0.5,
              valueGetter: (params: { data: DMCAAccusation }) => formatDate(params.data.CreatedAt),
               comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                const dateA = new Date(nodeA.data.CreatedAt).getTime();
                const dateB = new Date(nodeB.data.CreatedAt).getTime();
                return dateA - dateB;
              },
            },
      {
        headerName: "Status",
        cellClass: 'd-flex align-items-center justify-content-center',
        cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
        flex: 1.1,
        valueGetter: (params: any) => {
          return params.data?.CurrentStatus?.Name?.trim() || '';
        },
        cellRenderer: (params: any) => {
          const status = params.data?.CurrentStatus?.Name?.trim() || '';
          let color = '#888';
          let bg = 'transparent';
          switch (status) {
            case 'Pending DMCA Notice Review':
              color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.07)'; // vàng cam tươi
              break;
            case 'Valid Counter Notice':
            case 'Valid Lawsuit Proof':
            case 'Podcaster Lawsuit Win':
            case 'Accuser Lawsuit Win':
            case 'Valid DMCA Notice':
              color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)';
              break;
            case 'Invalid DMCA Notice':
            case 'Invalid Counter Notice':
            case 'Invalid Lawsuit Proof':
            case 'Dismissed':
              color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)'; // đỏ dịu mắt
              break;
            case 'Unresolved Dismissed':
            case 'Direct Resolve Dismissed':
              color = '#ef5350';
              bg = 'rgba(239, 83, 80, 0.15)';
              break;
            default:
              color = '#9e9e9e'; bg = 'rgba(158,158,158,0.15)'; // xám trung tính sáng
          }

          return (
            <span
              style={{
                display: 'inline-block',
                minWidth: 100,
                padding: '0 10px',
                borderRadius: 50,
                fontWeight: 700,
                fontSize: '0.75rem',
                color,
                background: bg,
                textAlign: 'center',
                border: `2px solid ${color}`,
              }}
            >
              {status}
            </span>
          );
        },
      },
      {
        headerName: "Actions",
        cellClass: "d-flex justify-content-center py-0",
        flex: 0.5,
        cellRenderer: (params: any) => {
          const DMCAAccusationId = params.data.Id
          return (
            <div className="d-flex gap-2 align-items-center h-100">
              <CButton
                onClick={() => navigate("/staff/dmca-accusation/" + DMCAAccusationId + "/" + "show")}
              >
                <Eye size={27} color='var(--secondary-green)' />
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
const episode_state = (table: DMCAAccusation[], navigate: (path: string) => void) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.3 },
      { headerName: "Accuser", field: "AccuserFullName" },
      { headerName: "Accuser Email", field: "AccuserEmail" },
      { headerName: "Podcast Episode", field: "PodcastEpisode.Name" },
      {
              headerName: "Created At",
              flex: 0.5,
              valueGetter: (params: { data: DMCAAccusation }) => formatDate(params.data.CreatedAt),
               comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
                const dateA = new Date(nodeA.data.CreatedAt).getTime();
                const dateB = new Date(nodeB.data.CreatedAt).getTime();
                return dateA - dateB;
              },
            },
      {
        headerName: "Status",
        cellClass: 'd-flex align-items-center justify-content-center',
        cellStyle: { display: 'flex', alignItems: 'center', justifyContent: 'center', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
        flex: 1.1,
        valueGetter: (params: any) => {
          return params.data?.CurrentStatus?.Name?.trim() || '';
        },
        cellRenderer: (params: any) => {
          const status = params.data?.CurrentStatus?.Name?.trim() || '';
          let color = '#888';
          let bg = 'transparent';
          switch (status) {
            case 'Pending DMCA Notice Review':
              color = '#ffb300'; bg = 'rgba(255, 179, 0, 0.07)'; // vàng cam tươi
              break;
            case 'Valid Counter Notice':
            case 'Valid Lawsuit Proof':
            case 'Podcaster Lawsuit Win':
            case 'Accuser Lawsuit Win':
            case 'Valid DMCA Notice':
              color = 'var(--secondary-green)'; bg = 'rgba(173, 227, 57, 0.06)';
              break;
            case 'Invalid DMCA Notice':
            case 'Invalid Counter Notice':
            case 'Invalid Lawsuit Proof':
            case 'Dismissed':
              color = '#ef5350'; bg = 'rgba(251, 222, 227, 0.2)'; // đỏ dịu mắt
              break;
            case 'Unresolved Dismissed':
            case 'Direct Resolve Dismissed':
              color = '#ef5350';
              bg = 'rgba(239, 83, 80, 0.15)';
              break;
            default:
              color = '#9e9e9e'; bg = 'rgba(158,158,158,0.15)'; // xám trung tính sáng
          }

          return (
            <span
              style={{
                display: 'inline-block',
                minWidth: 100,
                padding: '0 10px',
                borderRadius: 50,
                fontWeight: 700,
                fontSize: '0.75rem',
                color,
                background: bg,
                textAlign: 'center',
                border: `2px solid ${color}`,
              }}
            >
              {status}
            </span>
          );
        },
      },
      {
        headerName: "Actions",
        cellClass: "d-flex justify-content-center py-0",
        flex: 0.5,
        cellRenderer: (params: any) => {
          const DMCAAccusationId = params.data.Id
          return (
            <div className="d-flex gap-2 align-items-center h-100">
              <CButton
                onClick={() => navigate("/staff/dmca-accusation/" + DMCAAccusationId + "/" + "episode")}
              >
                <Eye size={27} color='var(--secondary-green)' />
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
  let [show, setShow] = useState<GridState | null>(null);
  let [episode, setEpisode] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const navigate = useNavigate();

  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const response = await getDMCAList(staffAxiosInstance);
      if (response.success) {
        const data = response.data.DMCAAccusationList;
        const showList = data.filter((item: DMCAAccusation) => !item.PodcastEpisode);
        const episodeList = data.filter((item: DMCAAccusation) => !item.PodcastShow);
        setShow(show_state(showList, navigate));
        setEpisode(episode_state(episodeList, navigate));
      } else {
        console.error('API Error:', response.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch DMCAAccusation accounts:', error);
    } finally {
      setIsLoading(false);
    }
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
      <CRow >
        <CCol xs={12}>
          {isLoading ? (
            <div className="flex justify-content-center align-items-center h-150" >
              <Loading />
            </div>
          ) : (
            <div>
              <div className="flex justify-between items-center mb-4">
                <p className="pl-3 text-2xl font-bold text-black mb-0">Show Assigned</p>

              </div>
              <div
                id="DMCAAccusation-table"
              >
                <AgGridReact
                  columnDefs={show?.columnDefs}
                  rowData={show?.rowData}
                  defaultColDef={defaultColDef}
                  rowHeight={70}
                  headerHeight={40}
                  pagination={true}
                  paginationPageSize={5}
                  paginationPageSizeSelector={[10, 20, 50, 100]}
                  domLayout='autoHeight'
                />
              </div>
              <div className="flex justify-between items-center mb-4 mt-5">
                <p className="pl-3 text-2xl font-bold text-black mb-0">Episode Assigned</p>
              </div>
              <div
                id="DMCAAccusation-table"
              >
                <AgGridReact
                  columnDefs={episode?.columnDefs}
                  rowData={episode?.rowData}
                  defaultColDef={defaultColDef}
                  rowHeight={70}
                  headerHeight={40}
                  pagination={true}
                  paginationPageSize={5}
                  paginationPageSizeSelector={[10, 20, 50, 100]}
                  domLayout='autoHeight'
                />
              </div>
            </div>
          )}
        </CCol>
      </CRow>

    </DMCAAccusationViewContext.Provider>
  )
}

export default DMCAAccusationView;