import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account, Podcaster, PodcasterProfile } from '../../../../core/types';
import { getPodcasterAccounts } from '../../../../core/services/account/account.service';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';
import PodcasterDetailTab from './PodcasterDetailTab';
import Loading from '../../../components/common/loading';


ModuleRegistry.registerModules([AllCommunityModule]);

interface PodcasterViewProps { }
interface PodcasterViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Podcaster[];
}

export const PodcasterViewContext = createContext<PodcasterViewContextProps | null>(null);

const state_creator = (table: Podcaster[]) => {

  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.4 },
      {
        headerName: "Avatar", flex: 0.5,
        cellRenderer: (params: { data: Podcaster }) => {
          return (
            <AvatarInput size={50} fileKey={params.data.MainImageFileKey} />)
        },
      },
      { headerName: "Podcaster Name", field: "PodcasterProfile.Name" },
      { headerName: "Full Name", field: "FullName" },
      { headerName: "Email", field: "Email" },
      { headerName: "Gender", field: "Gender", flex: 0.6 },
      {
        headerName: "Status",
        cellClass: 'd-flex align-items-center',
        flex: 0.7,
        cellRenderer: (params: { data: Podcaster }) => {
          let status = {
            title: '',
            color: '',
          };
          if (params.data.DeactivatedAt !== null) {
            status = {
              title: 'Deactivated',
              color: 'danger',
            };
          } else if (params.data.PodcasterProfile.IsVerified) {
            status = {
              title: 'Verified',
              color: 'success',
            };
          } else {
            status = {
              title: 'Pending',
              color: 'warning',
            };
          }
          return (
            <CCard
              textColor={`${status.color}`}
              style={{ width: '100px' }}
              className={`text-center fw-bold rounded-pill px-1 border-2 border-${status.color} bg-light`}
            >
              {status.title}
            </CCard>
          );
        },
      },
      {
        headerName: "Option",
        cellClass: 'd-flex justify-content-center py-0',
        cellRenderer: (params: { data: Podcaster }) => {
          const Modal_props = {
            updateForm: <PodcasterDetailTab account={params.data} onClose={() => { }} />,
            title: 'Podcaster [ID: #' + params.data.Id + ']',
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

const PodcasterView: FC<PodcasterViewProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState<boolean>(true);

  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const accountList = await getPodcasterAccounts(adminAxiosInstance);
      console.log("Fetched podcaster accounts:", accountList);
      if (accountList.success) {
        setState(state_creator(accountList.data.PodcasterList));
      } else {
        console.error('API Error:', accountList.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch podcaster accounts:', error);
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
    <PodcasterViewContext.Provider value={{ handleDataChange: handleDataChange }}>
      <CRow className="container-test">
        <CCol xs={12}>
          {isLoading ? (
             <div className="flex justify-content-center align-items-center h-150" >
              <Loading />
            </div>
          ) : (
            <div
              id="podcaster-table"
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

    </PodcasterViewContext.Provider>
  )
}

export default PodcasterView;