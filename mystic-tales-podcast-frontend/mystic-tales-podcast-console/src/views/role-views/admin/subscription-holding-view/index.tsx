
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
import SubscriptionHoldingModal from "./SubscriptionHoldingModal"
import { formatDate } from "@/core/utils/date.util"
import { getSubscriptionTransactionList } from "@/core/services/subscription/subscription.service"

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

const state_creator = (table: any[], handleShowModal: (data: any) => void) => {


  const state = {
    columnDefs: [
      {
        headerName: "Id",
        flex: 0.4,
        field: "Id",
      },
      { headerName: "Name", field: "Name",  cellStyle: { overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' } },
      { headerName: "Show", field: "PodcastShowName",  cellStyle: { overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' } },
      { headerName: "Channel", field: "PodcastChannelName",  cellStyle: { overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' } },
      { headerName: "Current Version", field: "CurrentVersion", flex: 0.9 },
      { headerName: "Registration Count", field: "PodcastSubscriptionRegistrationList.length", flex: 0.9 },
      { 
        headerName: "Total Holding", 
        flex: 0.9,
        valueGetter: (params: any) => {
          const registrations = params.data.PodcastSubscriptionRegistrationList || [];
          return registrations.reduce((sum: number, reg: any) => sum + (reg.HoldingAmount || 0), 0).toLocaleString();
        }
      },
      { 
        headerName: "Total Profit", 
        flex: 0.9,
        valueGetter: (params: any) => {
          const registrations = params.data.PodcastSubscriptionRegistrationList || [];
          return registrations.reduce((sum: number, reg: any) => sum + (reg.ProfitAmount || 0), 0).toLocaleString();
        }
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
          return (
            <button
              onClick={() => handleShowModal(params.data)}
              style={{
                background: 'none',
                border: 'none',
                cursor: 'pointer',
                padding: '4px 8px',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                width: '100%',
                height: '100%'
              }}
            >
              <Eye size={27} color='var(--secondary-green)' />
            </button>
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
  const [state, setState] = useState<GridState>({
    columnDefs: [],
    rowData: []
  })
  const [isLoading, setIsLoading] = useState<boolean>(true);
  const [selectedSubscription, setSelectedSubscription] = useState<any>(null);
  const [showModal, setShowModal] = useState(false);

  const handleShowModal = (data: any) => {
    setSelectedSubscription(data.PodcastSubscriptionRegistrationList);
    setShowModal(true);
  };

  const handleCloseModal = () => {
    setShowModal(false);
    setSelectedSubscription(null);
  };


  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const res = await getSubscriptionTransactionList(adminAxiosInstance);
      console.log("Fetched transaction list:", res);
      if (res.success && res.data && res.data.PodcastSubscriptionList) {
        setState(state_creator(res.data.PodcastSubscriptionList || [], handleShowModal));
      } else {
        console.error('API Error:', res.message);
        setState(state_creator([], handleShowModal));
      }
    } catch (error) {
      console.error('Lỗi khi fetch sub holding list:', error);
      setState(state_creator([], handleShowModal));
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
        <h3 className="transaction__title mb-5">Subscription</h3>
        <CCol xs={12}>
          {isLoading ? (
            <div className="flex justify-center items-center h-100" >
              <Loading />
            </div>
          ) : (
            <div id="withdrawal-table" className="">
              <AgGridReact
                columnDefs={state?.columnDefs || []}
                rowData={state?.rowData || []}
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

      {showModal && selectedSubscription && (
        <SubscriptionHoldingModal
          transaction={selectedSubscription}
          onClose={handleCloseModal}
        />
      )}
    </SubscriptionHoldingViewContext.Provider>
  )
}

export default SubscriptionHoldingView
