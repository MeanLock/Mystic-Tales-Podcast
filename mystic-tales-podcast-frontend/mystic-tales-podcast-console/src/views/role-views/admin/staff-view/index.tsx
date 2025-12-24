import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account } from '../../../../core/types';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';
import StaffUpdate from './StaffUpdate';
import StaffRegister from './StaffRegister';
import { getStaffAccounts } from '@/core/services/account/account.service';
import Loading from '../../../components/common/loading';


ModuleRegistry.registerModules([AllCommunityModule]);

interface StaffViewProps { }
interface StaffViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Account[];
}

export const StaffViewContext = createContext<StaffViewContextProps | null>(null);

const state_creator = (table: Account[]) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.35 },
      {
        headerName: "Avatar", flex: 0.5,
        cellRenderer: (params: { data: Account }) => {
          return (
            <AvatarInput size={50} fileKey={params.data.MainImageFileKey} />)
        },
      },
      { headerName: "Full Name", field: "FullName" },
      { headerName: "Email", field: "Email" },
      { headerName: "Gender", field: "Gender", flex: 0.6 },
      {
        headerName: "Phone", field: "Phone", flex: 0.7
      },
      {
        headerName: "Created At",
        flex: 0.7,
        valueGetter: (params: { data: Account }) => formatDate(params.data.CreatedAt),
        comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
          const dateA = new Date(nodeA.data.CreatedAt).getTime();
          const dateB = new Date(nodeB.data.CreatedAt).getTime();
          return dateA - dateB;
        },
      },
      {
        headerName: "Status",
        cellClass: 'd-flex align-items-center',
        flex: 0.7,
        valueGetter: (params: { data: Account }) => {
          if (params.data.DeactivatedAt !== null) return "Deactivated";
          if (params.data.IsVerified) return "Verified";
          return "Unverified";
        },
        cellRenderer: (params: { data: Account }) => {
          let status = {
            title: '',
            color: '',
            bg: ''
          };
          if (params.data.DeactivatedAt !== null) {
            status = {
              title: 'Deactivated',
              color: '#ef5350',
              bg: 'rgba(251, 222, 227, 0.2)',
            };
          } else if (params.data.IsVerified) {
            status = {
              title: 'Verified',
             color: 'var(--secondary-green)',
              bg: 'rgba(173, 227, 57, 0.06)'
            };
          } else {
            status = {
              title: 'Unverified',
               color: '#ffb300',
              bg: 'rgba(255, 179, 0, 0.07)'
            };
          }
          return (
            <CCard
              style={{ width: '100px', color: `${status.color}`, background: `${status.bg}`, border: `2px solid ${status.color}` }}
              className={`text-center fw-bold rounded-pill px-1 `}
            >
              {status.title}
            </CCard>
          );
        },
      },
      {
        headerName: "Option",
        cellClass: 'd-flex justify-content-center py-0',
        cellRenderer: (params: { data: Account }) => {
          const Modal_props = {
            updateForm: <StaffUpdate account={params.data} onClose={() => { }} />,
            title: 'Staff [ID: #' + params.data.Id + ']',
            button: <Eye size={27} color='var(--secondary-green)' />,
            update_button_color: 'white'
          }
          return (

            <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
              <Modal_Button
                disabled={false}
                title={Modal_props.title}
                content={Modal_props.button}
                color={Modal_props.update_button_color} >
                {Modal_props.updateForm}
              </Modal_Button>
            </CButtonGroup>
          )

        },
        flex: 0.5,
      }
    ],
    rowData: table

  }
  return state
}

const StaffView: FC<StaffViewProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const accountList = await getStaffAccounts(adminAxiosInstance);
      console.log("Fetched staff accounts:", accountList);
      if (accountList.success) {
        setState(state_creator(accountList.data.StaffList));
      } else {
        console.error('API Error:', accountList.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch staff accounts:', error);
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
    <StaffViewContext.Provider value={{ handleDataChange: handleDataChange }}>
      <div className="staff-view__header">
        <h2 className="staff-view__title">Staff Management</h2>
        <div>
          <Modal_Button
            disabled={false}
            title=""
            content={<span className="staff-view__add-btn ">Add Staff</span>}
            color="white"
          >
            <StaffRegister onClose={() => { }} />
          </Modal_Button>
        </div>
      </div>
      <CRow>
        <CCol xs={12}>
          {isLoading ? (
            <div className="flex justify-content-center align-items-center h-150" >
              <Loading />
            </div>
          ) : (
            <div
              id="Staff-table"
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

    </StaffViewContext.Provider>
  )
}

export default StaffView;