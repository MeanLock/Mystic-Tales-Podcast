
import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import {
  CButton,
  CButtonGroup,
  CCard,
  CCol,
  CRow,
  CSpinner,
  CModal,
  CModalHeader,
  CModalTitle,
  CModalBody,
  CModalFooter,
} from "@coreui/react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import { Eye } from "phosphor-react"
import { getTransactionList } from "@/core/services/transaction/transaction.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Loading from "@/views/components/common/loading"
import { getBookingHoldingList } from "@/core/services/booking/booking.service"
import { getSubscriptionHoldingList } from "@/core/services/subscription/subscription.service"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import SubscriptionHoldingModal from "./SubscriptionHoldingModal"
import { formatDate } from "@/core/utils/date.util"

ModuleRegistry.registerModules([AllCommunityModule])


type SubscriptionHoldingViewProps = {}

interface SubscriptionHoldingViewContextProps {
  handleDataChange: () => void
}

interface GridState {
  columnDefs: any[]
  rowData: any[]
}

export const SubscriptionHoldingViewContext = createContext<SubscriptionHoldingViewContextProps | null>(null)

const state_creator = (table: any[]) => {


  const state = {
    columnDefs: [
      {
        headerName: "Id",
        flex: 0.4,
        field: "Id",
      },
      { headerName: "Name", field: "Name", flex: 0.9 },
      { headerName: "Show", field: "PodcastShowName", flex: 0.9 },
      { headerName: "Channel", field: "PodcastChannelName", flex: 0.9 },
      { headerName: "Current Version", field: "CurrentVersion", flex: 0.9 },
       { headerName: "Registration Count", field: "PodcastSubscriptionRegistrationList.length", flex: 0.9 },
      {
        headerName: "Created At",
        field: "CreatedAt",
        cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
        valueGetter: (params: any) => formatDate(params.data.CreatedAt),
        comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
          const dateA = new Date(nodeA.data.CreatedAt).getTime();
          const dateB = new Date(nodeB.data.CreatedAt).getTime();
          return dateA - dateB;
        },
      },
      {
        headerName: "Recently Updated",
        field: "UpdatedAt",
        cellStyle: { display: 'flex', alignItems: 'center', fontSize: '0.75rem', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' },
        valueGetter: (params: any) => formatDate(params.data.UpdatedAt),
        comparator: (valueA: string, valueB: string, nodeA: any, nodeB: any) => {
          const dateA = new Date(nodeA.data.UpdatedAt).getTime();
          const dateB = new Date(nodeB.data.UpdatedAt).getTime();
          return dateA - dateB;
        },
      },
      {
        headerName: "",
        cellClass: 'd-flex justify-content-center py-0',
        cellRenderer: (params: { data: any }) => {
          const Modal_props = {
            updateForm: <SubscriptionHoldingModal transaction={params.data.PodcastSubscriptionRegistrationList} onClose={() => { }} />,
            button: <Eye size={27} color='var(--secondary-green)' />,
            update_button_color: 'white'
          }
          return (

            <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
              <Modal_Button
                disabled={false}
                content={Modal_props.button}
                color={Modal_props.update_button_color}
                size="xl"
              >
                {Modal_props.updateForm}
              </Modal_Button>
            </CButtonGroup>
          )

        },
        flex: 0.5,
      }
    ],
    rowData: table,
  }
  return state
}



const SubscriptionHoldingView: FC<SubscriptionHoldingViewProps> = () => {
  const [state, setState] = useState<GridState | null>(null)
  const [isLoading, setIsLoading] = useState<boolean>(true);


  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const res = await getSubscriptionHoldingList(adminAxiosInstance);
      console.log("Fetched transaction list:", res);
      if (res.success) {
        setState(state_creator(res.data.PodcastSubscriptionList || []));
      } else {
        console.error('API Error:', res.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch sub holding list:', error);
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
      cellClass: "d-flex align-items-center",
      editable: false,
    }
  }, [])

  return (
    <SubscriptionHoldingViewContext.Provider
      value={{
        handleDataChange: handleDataChange,
      }}
    >
      <CRow>
        <h3 className="transaction__title mb-5">Subscription Holding</h3>
        <CCol xs={12}>
          {isLoading ? (
            <div className="flex justify-center items-center h-100" >
              <Loading />
            </div>
          ) : (
            <div id="withdrawal-table" className="">
              <AgGridReact
                columnDefs={state.columnDefs || []}
                rowData={state.rowData}
                defaultColDef={defaultColDef}
                rowHeight={70}
                headerHeight={40}
                pagination={true}
                paginationPageSize={10}
                paginationPageSizeSelector={[10, 20, 50, 100]}
                domLayout="autoHeight"
              />
            </div>
          )}
        </CCol>
      </CRow>

    </SubscriptionHoldingViewContext.Provider>
  )
}

export default SubscriptionHoldingView
