import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import CustomerUpdate from './CustomerUpdate';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';
import { Account } from '../../../../core/types';
import { getCustomerAccounts } from '../../../../core/services/account/account.service';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import AvatarInput from '../../../components/common/avatar';
import { formatDate } from '../../../../core/utils/date.util';

ModuleRegistry.registerModules([AllCommunityModule]);

interface CustomerViewProps { }
interface CustomerViewContextProps {
  handleDataChange: () => void;
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Account[];
}

export const CustomerViewContext = createContext<CustomerViewContextProps | null>(null);

const state_creator = (table: Account[]) => {
  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.4 },
      {
        headerName: "Avatar", flex: 0.5,
        cellRenderer: (params: { data: Account }) => {
          return (
            <AvatarInput size={50} src={params.data.MainImageUrl ?? ''} />)
        },
      },
      { headerName: "Họ và Tên", field: "FullName" },
      { headerName: "Số Coin Sở Hữu", field: "Balance" },
      {
        headerName: "Level Hiện Tại",
        valueGetter: (params: { data: Account }) => `Level ${params.data.Level} - ${params.data.Xp} / 100`,
      },
      { headerName: "Tổng lượng giao dịch", field: "ProgressionSurveyCount" },
         {
        headerName: "Ngày Tham Gia",
        valueGetter: (params: { data: Account }) => formatDate(params.data.CreatedAt),
      },
      {
        headerName: "Trạng thái",
        cellClass: 'd-flex align-items-center',
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
            updateForm: <CustomerUpdate account={params.data} onClose={() => { }} />,
            title: 'Update Customer [ID: #' + params.data.Id + ']',
            button: <Eye size={27} color="purple" weight="duotone" />,
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

const CustomerView: FC<CustomerViewProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const accountList = await getCustomerAccounts(adminAxiosInstance);
      if (accountList.success) {
        setState(state_creator(accountList.data.Accounts));
      } else {
        console.error('API Error:', accountList.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch customer accounts:', error);
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
    <CustomerViewContext.Provider value={{ handleDataChange: handleDataChange }}>
      <CRow>
        <CCol xs={12}>
          {isLoading ? (
            <SurveyTalkLoading />
          ) : (
            <div
              id="customer-table"
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

    </CustomerViewContext.Provider>
  )
}

export default CustomerView;