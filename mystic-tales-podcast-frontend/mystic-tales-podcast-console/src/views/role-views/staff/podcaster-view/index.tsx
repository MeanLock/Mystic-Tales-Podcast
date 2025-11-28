import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account, Podcaster, PodcasterList } from '../../../../core/types';
import { getCustomerAccounts } from '../../../../core/services/account/account.service';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';
import PodcasterDetailTab from './PodcasterDetailTab';

export const mockPodcasterList: any = {
  PodcasterList: [
    {
      Podcaster: {
        Id: 1,
        Email: "user@example.com",
        Role: {
          Id: 1,
          Name: "Podcaster",
        },
        Fullname: "John Doe",
        Dob: "1995-10-09",
        Gender: "Male",
        Address: "123 Main Street, HCMC",
        Phone: "0123456789",
        Balance: 50000,
        MainImageFileKey: "main_image_key_123",
        IsVerified: true,
        GoogleId: "google_12345",
        PodcastListenSlot: 10,
        ViolationPoint: 0,
        ViolationLevel: 0,
        LastViolationPointChanged: "2025-10-09T08:33:37.903Z",
        LastViolationLevelChanged: "2025-10-09T08:33:37.903Z",
        LastPodcastListenSlotChanged: "2025-10-09T08:33:37.903Z",
        DeactivatedAt: "2025-10-09T08:33:37.903Z",
        CreatedAt: "2025-10-09T08:33:37.903Z",
        UpdatedAt: "2025-10-09T08:33:37.903Z",
        IsBeingPunish: false,
        PodcasterProfile: {
          AccountId: 1,
          Description: "Chuyên gia chia sẻ podcast về phát triển bản thân.",
          AverageRating: 4.8,
          RatingCount: 128,
          CommitmentDocumentFileKey: "https://www.antennahouse.com/hubfs/xsl-fo-sample/pdf/basic-link-1.pdf",
          BuddyAudioFileKey: "buddy_audio_key_123",
          OwnedBookingStorageSize: 1024,
          UsedBookingStorageSize: 256,
          IsVerified: true,
          CreatedAt: "2025-10-09T08:33:37.903Z",
          UpdatedAt: "2025-10-09T08:33:37.903Z",
        },
      },
    },
    {
      Podcaster: {
        Id: 2,
        Email: "guest@example.com",
        Role: {
          Id: 2,
          Name: "Guest",
        },
        Fullname: "Jane Smith",
        Dob: "1998-03-12",
        Gender: "Female",
        Address: "456 District 1, HCMC",
        Phone: "0987654321",
        Balance: 120000,
        MainImageFileKey: "main_image_key_456",
        IsVerified: true,
        GoogleId: "google_67890",
        PodcastListenSlot: 15,
        ViolationPoint: 2,
        ViolationLevel: 1,
        LastViolationPointChanged: "2025-10-09T08:33:37.903Z",
        LastViolationLevelChanged: "2025-10-09T08:33:37.903Z",
        LastPodcastListenSlotChanged: "2025-10-09T08:33:37.903Z",
        DeactivatedAt: "2025-10-09T08:33:37.903Z",
        CreatedAt: "2025-10-09T08:33:37.903Z",
        UpdatedAt: "2025-10-09T08:33:37.903Z",
        IsBeingPunish: true,
        PodcasterProfile: {
          AccountId: 2,
          Description: "Podcaster về công nghệ và sáng tạo nội dung.",
          AverageRating: 4.5,
          RatingCount: 89,
          CommitmentDocumentFileKey: "https://www.antennahouse.com/hubfs/xsl-fo-sample/pdf/basic-link-1.pdf",
          BuddyAudioFileKey: "buddy_audio_key_456",
          OwnedBookingStorageSize: 2048,
          UsedBookingStorageSize: 512,
          IsVerified: false,
          CreatedAt: "2025-10-09T08:33:37.903Z",
          UpdatedAt: "2025-10-09T08:33:37.903Z",
        },
      },
    },
    {
      Podcaster: {
        Id: 3,
        Email: "guest@example.com",
        Role: {
          Id: 2,
          Name: "Guest",
        },
        Fullname: "Jane Smith",
        Dob: "1998-03-12",
        Gender: "Female",
        Address: "456 District 1, HCMC",
        Phone: "0987654321",
        Balance: 120000,
        MainImageFileKey: "main_image_key_456",
        IsVerified: true,
        GoogleId: "google_67890",
        PodcastListenSlot: 15,
        ViolationPoint: 2,
        ViolationLevel: 1,
        LastViolationPointChanged: "2025-10-09T08:33:37.903Z",
        LastViolationLevelChanged: "2025-10-09T08:33:37.903Z",
        LastPodcastListenSlotChanged: "2025-10-09T08:33:37.903Z",
        DeactivatedAt: "2025-10-09T08:33:37.903Z",
        CreatedAt: "2025-10-09T08:33:37.903Z",
        UpdatedAt: "2025-10-09T08:33:37.903Z",
        IsBeingPunish: true,
        PodcasterProfile: {
          AccountId: 3,
          CommitmentDocumentFileKey: "commitment_file_456",
          BuddyAudioFileKey: "buddy_audio_key_456",
          IsVerified: null,
          CreatedAt: "2025-10-09T08:33:37.903Z",
          UpdatedAt: "2025-10-09T08:33:37.903Z",
        },
      },
    },

  ],
};

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

const state_creator = (table: { Podcaster: Podcaster }[]) => {
  const podcasterRows = table.map(item => {
    const p: any = item.Podcaster as any
    const { PodcasterProfile, ...accountFields } = p
    return {
      ...p,
      Account: { ...accountFields },
      PodcasterProfile,
    } as unknown as Podcaster
  });

  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.4 },
      { headerName: "Fullname", field: "Fullname" },
      { headerName: "Email", field: "Email" },
      { headerName: "Gender", field: "Gender", flex: 0.6 },
      { headerName: "Phone", field: "Phone", flex: 0.7 },
      {
        headerName: "Created At",
        flex: 0.7,
        valueGetter: (params: { data: Podcaster }) => formatDate(params.data.PodcasterProfile.CreatedAt),
      },
      {
        headerName: "Status",
        field: "StatusValue", // dùng cho sort
        cellClass: 'd-flex align-items-center',
        flex: 0.7,
        sortable: true,
        valueGetter: (params: { data: Podcaster }) => {
          if (params.data.PodcasterProfile.IsVerified === false) return "Rejected";
          if (params.data.PodcasterProfile.IsVerified === true) return "Verified";
          if (params.data.PodcasterProfile.IsVerified === null) return "Pending";
          return "Unverified";
        },
        cellRenderer: (params: { data: Podcaster }) => {
          let status = { title: '', color: '' };
          if (params.data.PodcasterProfile.IsVerified === false) {
            status = { title: 'Rejected', color: 'danger' };
          } else if (params.data.PodcasterProfile.IsVerified) {
            status = { title: 'Verified', color: 'success' };
          } else if (params.data.PodcasterProfile.IsVerified === null) {
            status = { title: 'Pending', color: 'warning' };
          } else {
            status = { title: 'Unverified', color: 'warning' };
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
            updateForm: <PodcasterDetailTab podcaster={params.data} onClose={() => { }} />,
            title: 'Podcaster ',
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
    rowData: podcasterRows
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
    setState(state_creator(mockPodcasterList.PodcasterList));

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
      <h2 className="mb-4 fw-bold" style={{ color: 'var(--primary-grey)', borderBottom: '2px solid var(--primary-grey)', paddingBottom: '0.7rem' }}>Podcaster Request</h2>
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