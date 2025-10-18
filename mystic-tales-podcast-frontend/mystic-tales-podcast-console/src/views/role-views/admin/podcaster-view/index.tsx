import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account, PodcasterList } from '../../../../core/types';
import { getCustomerAccounts } from '../../../../core/services/account/account.service';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';
import PodcasterDetailTab from './PodcasterDetailTab';

export const mockPodcastersList: PodcasterList = {
  PodcasterList: [
    {
      Id: 1,
      Email: "user1@example.com",
      Role: { Id: 1, Name: "Customer" },
      Fullname: "Nguyen Van A",
      Dob: "1995-05-20",
      Gender: "Male",
      Address: "123 Main Street, Hanoi",
      Phone: "0123456789",
      Balance: 100000,
      MainImageFileKey: "https://picsum.photos/200/200?1",
      IsVerified: true,
      GoogleId: "google-1111",
      VerifyCode: "ABC123",
      PodcastListenSlot: 10,
      ViolationPoint: 2,
      ViolationLevel: 1,
      LastViolationPointChanged: "2025-10-04T07:54:49.276Z",
      LastViolationLevelChanged: "2025-10-04T07:54:49.276Z",
      LastPodcastListenSlotChanged: "2025-10-04T07:54:49.276Z",
      DeactivatedAt: "2025-10-04T07:54:49.276Z",
      CreatedAt: "2025-10-04T07:54:49.276Z",
      UpdatedAt: "2025-10-04T07:54:49.276Z",
      IsBeingPunish: true,
    },
    {
      Id: 2,
      Email: "user2@example.com",
      Role: { Id: 2, Name: "Admin" },
      Fullname: "Tran Thi B",
      Dob: "1998-12-15",
      Gender: "Female",
      Address: "456 Nguyen Trai, HCMC",
      Phone: "0987654321",
      Balance: 250000,
      MainImageFileKey: "https://picsum.photos/200/200?2",
      IsVerified: false,
      GoogleId: "google-2222",
      VerifyCode: "XYZ456",
      PodcastListenSlot: 15,
      ViolationPoint: 0,
      ViolationLevel: 0,
      LastViolationPointChanged: "2025-10-04T07:54:49.276Z",
      LastViolationLevelChanged: "2025-10-04T07:54:49.276Z",
      LastPodcastListenSlotChanged: "2025-10-04T07:54:49.276Z",
      DeactivatedAt: null,
      CreatedAt: "2025-10-04T07:54:49.276Z",
      UpdatedAt: "2025-10-04T07:54:49.276Z",
      IsBeingPunish: false,
    },
  ]
};
ModuleRegistry.registerModules([AllCommunityModule]);

interface PodcasterViewProps { }
interface PodcasterViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Account[];
}

export const PodcasterViewContext = createContext<PodcasterViewContextProps | null>(null);

const state_creator = (table: Account[]) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.4 },
      {
        headerName: "Avatar", flex: 0.5,
        cellRenderer: (params: { data: Account }) => {
          return (
            <AvatarInput size={50} src={params.data.MainImageFileKey ?? ''} />)
        },
      },
      { headerName: "Fullname", field: "Fullname" },
      { headerName: "Email", field: "Email" },
      { headerName: "Gender", field: "Gender", flex: 0.6 },
      {
        headerName: "Phone", field: "Phone", flex: 0.7
      },
      {
        headerName: "Created At",
        flex: 0.7,
        valueGetter: (params: { data: Account }) => formatDate(params.data.CreatedAt),
      },
      {
        headerName: "Status",
        cellClass: 'd-flex align-items-center',
        flex: 0.7,
        cellRenderer: (params: { data: Account }) => {
          let status = {
            title: '',
            color: '',
          };
          if (params.data.DeactivatedAt !== null) {
            status = {
              title: 'Deactivated',
              color: 'danger',
            };
          } else if (params.data.IsVerified) {
            status = {
              title: 'Verified',
              color: 'success',
            };
          } else {
            status = {
              title: 'Unverified',
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
        cellRenderer: (params: { data: Account }) => {
          const Modal_props = {
            updateForm: <PodcasterDetailTab account={params.data} onClose={() => { }} />,
            title: 'Podcaster [ID: #' + params.data.Id + ']',
            button:  <Eye size={27} color='var(--secondary-green)'  />,
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

  // const handleDataChange = async () => {
  //   setIsLoading(true);
  //   try {
  //     const accountList = await getCustomerAccounts(adminAxiosInstance);
  //     if (accountList.success) {
  //       setState(state_creator(accountList.data.Accounts));
  //     } else {
  //       console.error('API Error:', accountList.message);
  //     }
  //   } catch (error) {
  //     console.error('Lỗi khi fetch customer accounts:', error);
  //   } finally {
  //     setIsLoading(false);
  //   }
  // }
  const handleDataChange = async () => {
    setIsLoading(false);
    setState(state_creator(mockPodcastersList.PodcasterList));

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
            <SurveyTalkLoading />
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