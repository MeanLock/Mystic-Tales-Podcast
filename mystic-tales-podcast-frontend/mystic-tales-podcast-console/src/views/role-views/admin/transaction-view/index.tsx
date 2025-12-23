
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
import { formatDate } from "@/core/utils/date.util"
import Modal_Button from "@/views/components/common/modal/ModalButton"
import TransactionModal from "./TransactionModal"

ModuleRegistry.registerModules([AllCommunityModule])


type TransactionViewProps = {}

interface TransactionViewContextProps {
  handleDataChange: () => void
}

interface GridState {
  columnDefs: any[]
  rowData: any[]
}

export const TransactionViewContext = createContext<TransactionViewContextProps | null>(null)

const state_creator = (table: any[]) => {


  const state = {
    columnDefs: [
       {
                headerName: "No.",
                flex: 0.4,
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1; // Hiển thị số thứ tự từ 1
                },
                cellClass: '',
                sortable: false,
                filter: false
            },
      { headerName: "Customer", field: "Account.FullName", flex: 0.9 },
      { headerName: "Email", field: "Account.Email", flex: 0.9 },
      {
        headerName: "Amount",
        field: "Amount",
        flex: 0.8,
      },

      {
        headerName: "Reject Reason",
        field: "RejectReason",
        flex: 1,
        cellRenderer: (params: any) => {
          return params.data.RejectReason ? params.data.RejectReason : "---"
        },
      },
      {
        headerName: "Status",
        field: "IsRejected",
        flex: 0.8,
        cellClass: "d-flex align-items-center",
           valueGetter: (params: { data: any }) => {
                  if (params.data.IsRejected === null) return "Pending";
                  if (params.data.IsRejected) return "Rejected";
                  return "Success";
                },
        cellRenderer: (params: any) => {
          const status = {
            title: params.data.IsRejected === null ? "Pending" : params.data.IsRejected ? "Rejected" : "Success",
            color: params.data.IsRejected === null ? "#ffb300" : params.data.IsRejected ? "#ef5350" : "var(--secondary-green)",
            bg: params.data.IsRejected === null ? "rgba(255, 179, 0, 0.07)" : params.data.IsRejected ? "rgba(251, 222, 227, 0.2)" : "rgba(173, 227, 57, 0.06)",
          }
          return (
           <CCard
              style={{ width: '100px', color: `${status.color}`, background: `${status.bg}`, border: `2px solid ${status.color}` }}
              className={`text-center fw-bold rounded-pill px-1 `}
            >
              {status.title}
            </CCard>
          )
        },
      },
      {
             headerName: "Option",
             cellClass: 'd-flex justify-content-center py-0',
             cellRenderer: (params: { data: any }) => {
               const Modal_props = {
                 updateForm: <TransactionModal transaction={params.data} onClose={() => { }} />,
                 button: <Eye size={27} color='var(--secondary-green)' />,
                 update_button_color: 'white'
               }
               return (
     
                 <CButtonGroup style={{ width: '100%', height: "100%" }} role="group" aria-label="Basic mixed styles example">
                   <Modal_Button
                     disabled={false}
                     content={Modal_props.button}
                     color={Modal_props.update_button_color} 
                     size= "lg"
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



const TransactionView: FC<TransactionViewProps> = () => {
  const [state, setState] = useState<GridState | null>(null)
  const [withdrawalData, setWithdrawalData] = useState<any[]>([]);
   const [isLoading, setIsLoading] = useState<boolean>(true);


const handleDataChange = async () => {
     setIsLoading(true);
     try {
       const res = await getTransactionList(adminAxiosInstance);
       console.log("Fetched transaction list:", res);
       if (res.success) {
         setState(state_creator(res.data.AccountBalanceWithdrawalRequestList || []));
       } else {
         console.error('API Error:', res.message);
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
      cellClass: "d-flex align-items-center",
      editable: false,
    }
  }, [])

  return (
    <TransactionViewContext.Provider
      value={{
        handleDataChange: handleDataChange,
      }}
    >
      <CRow>
        <h3 className="transaction__title mb-5">Balance Withdrawal</h3>
        <CCol xs={12}>
          {isLoading ? (
            <div className="flex justify-center items-center h-100" >
              <Loading/>
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
  
    </TransactionViewContext.Provider>
  )
}

export default TransactionView
