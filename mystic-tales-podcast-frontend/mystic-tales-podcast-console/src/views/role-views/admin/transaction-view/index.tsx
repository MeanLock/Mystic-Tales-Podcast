
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
import { getBalanceDepositHistory } from "../../../../core/services/payment/account-payment.service"
import { managerAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"

ModuleRegistry.registerModules([AllCommunityModule])


type TransactionViewProps = {}

interface TransactionViewContextProps {
  handleDataChange: () => void
  updateTransactionStatus: (id: string, status: number) => void;
}

interface GridState {
  columnDefs: any[]
  rowData: any[]
}

interface QRModalProps {
  visible: boolean
  onClose: () => void
  transaction: any
  onConfirm: () => void
  onCancel: () => void
}
interface TransactionStatus {
  Id: number;
  Name: string;
}

interface TransactionType {
  Id: number;
  Name: string;
}

interface Account {
  Id: number;
  FullName: string;
  Email: string;
  Phone: string;
  MainImageUrl: string;
}

interface DepositHistory {
  Id: number;
  Amount: number;
  CreatedAt: string;
  Account: Account;
  TransactionStatus: TransactionStatus;
  TransactionType: TransactionType;
}
export const TransactionViewContext = createContext<TransactionViewContextProps | null>(null)

const QRModal: FC<QRModalProps> = ({ visible, onClose, transaction, onConfirm, onCancel }) => {
  if (!transaction) return null

  const qrUrl = `https://img.vietqr.io/image/${transaction.BankId}-${transaction.AccountNo}-compact.png?amount=${transaction.Amount}&addInfo=Survey Talk Chuyen Tien&accountName=${transaction.AccountName}`
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount)
  }

  return (
    <CModal visible={visible} onClose={onClose} size="lg">
      <CModalHeader>
        <CModalTitle>Chi tiết yêu cầu rút tiền</CModalTitle>
      </CModalHeader>
      <CModalBody className="text-center">
        <div className="mb-4">
          <h5>Thông tin khách hàng</h5>
          <p>
            <strong>Tên:</strong> {transaction.CustomerName}
          </p>
          <p>
            <strong>Số tiền rút:</strong> {formatCurrency(transaction.Amount)}
          </p>
          <p>
            <strong>Ngân hàng:</strong> {transaction.BankName}
          </p>
          <p>
            <strong>Số tài khoản:</strong> {transaction.AccountNo}
          </p>
          <p>
            <strong>Tên tài khoản:</strong> {transaction.AccountName}
          </p>
        </div>

        <div className="mb-4 flex flex-column align-items-center">
          <h6>Mã QR chuyển khoản</h6>
          <img
            src={qrUrl || "/placeholder.svg"}
            alt="QR Code"
            style={{ maxWidth: "300px", width: "100%" }}
            onError={(e) => {
              e.currentTarget.src = "/placeholder.svg?height=300&width=300"
            }}
          />
        </div>
      </CModalBody>
      <CModalFooter>
        <CButton color="danger" onClick={onCancel} style={{ color: 'white' }}>
          Hủy
        </CButton>
        <CButton color="success" onClick={onConfirm} style={{ color: 'white' }}>
          Xác nhận chuyển thành công
        </CButton>
      </CModalFooter>
    </CModal>
  )
}

const state_creator = (table: any[], updateStatus: (id: string, status: number) => void) => {
  const formatCurrency = (amount: number) => {
    return new Intl.NumberFormat("vi-VN", {
      style: "currency",
      currency: "VND",
    }).format(amount)
  }

  const formatDate = (dateString: string | null) => {
    if (!dateString) return "Chưa xử lý"
    return new Date(dateString).toLocaleString("vi-VN")
  }

  const state = {
    columnDefs: [
      { headerName: "ID", field: "Id", flex: 0.3 },
      { headerName: "Họ và Tên", field: "CustomerName", flex: 0.9 },
      {
        headerName: "Số tiền rút",
        field: "Amount",
        flex: 0.8,
        cellRenderer: (params: any) => {
          return formatCurrency(params.data.Amount)
        },
      },

      {
        headerName: "Thời gian chuyển",
        field: "TransferDate",
        flex: 1,
        cellRenderer: (params: any) => {
          return formatDate(params.data.TransferDate)
        },
      },
      {
        headerName: "Status",
        field: "StatusId",
        flex: 0.8,
        cellClass: "d-flex align-items-center",
        cellRenderer: (params: any) => {
          const status = {
            title: params.data.StatusId === 1 ? "Chờ xử lý" : "Thành công",
            color: params.data.StatusId === 1 ? "warning" : "success",
            bg: "light",
          }
          return (
            <CCard
              textColor={status.color}
              style={{ width: "120px" }}
              className={`text-center fw-bold rounded-pill px-2 border-2 border-${status.color} bg-${status.bg}`}
            >
              {status.title}
            </CCard>
          )
        },
      },
      {
        headerName: "Option",
        cellClass: "d-flex justify-content-center py-0",
        cellRenderer: (params: any) => {
          return (
            <CButtonGroup style={{ width: "100%", height: "100%" }} role="group">
              <EyeButton transaction={params.data} updateStatus={updateStatus} />
            </CButtonGroup>
          )
        },
        flex: 0.5,
      },
    ],
    rowData: table,
  }
  return state
}

const EyeButton: FC<{ transaction: any; updateStatus: (id: string, status: number) => void }> = ({
  transaction,
  updateStatus,
}) => {
  const [modalVisible, setModalVisible] = useState(false)

  const handleConfirm = () => {
    updateStatus(transaction.Id.toString(), 2);
    setModalVisible(false)
  }

  const handleCancel = () => {
    setModalVisible(false)
  }

  return (
    <>
      <CButton
        color="white"
        onClick={() => setModalVisible(true)}
        style={{ border: "none", background: "transparent" }}
      >
        <Eye size={30} color="purple" weight="duotone" />
      </CButton>

      <QRModal
        visible={modalVisible}
        onClose={() => setModalVisible(false)}
        transaction={transaction}
        onConfirm={handleConfirm}
        onCancel={handleCancel}
      />
    </>
  )
}

const TransactionView: FC<TransactionViewProps> = () => {
  const [state, setState] = useState<GridState | null>(null)
  const [withdrawalData, setWithdrawalData] = useState<any[]>([]);
  

  const updateTransactionStatus = async (id: string, status: number) => {

    try {
      const apiUrl = `https://6857de4821f5d3463e566b36.mockapi.io/withdrawalRequests/${id}`;
      // Prepare updated data
      const updatedTransaction = {
        ...withdrawalData.find((item: any) => item.Id === id),
        StatusId: status,
        TransferDate: status === 2 ? Math.floor(Date.now() / 1000) : null, // Convert to Unix timestamp
      };

      // Update API
      const response = await fetch(apiUrl, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(updatedTransaction),
      });

      if (!response.ok) {
        throw new Error('Failed to update transaction');
      }

      // Update local state after successful API update
      const updatedData = withdrawalData.map((item: any) => {
        if (item.Id === id) {
          return updatedTransaction;
        }
        return item;
      });

      handleDataChange();
    } catch (error) {
      console.error('Error updating transaction:', error);
      // You might want to add error handling here (e.g., show toast notification)
    }
  };
  const handleDataChange = async () => {
    const apiUrl = "https://6857de4821f5d3463e566b36.mockapi.io/withdrawalRequests";
    const res = await fetch(apiUrl);
    const data = await res.json();
    setWithdrawalData(data);
    setState(state_creator(data, updateTransactionStatus)) // Use data directly instead of withdrawalData
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
        updateTransactionStatus: updateTransactionStatus,
      }}
    >
      <CRow>
        <h3 className="transaction__title mb-5">Balance Withdrawal</h3>
        <CCol xs={12}>
          {state === null ? (
            <CButton className="w-100" color="secondary" disabled>
              <CSpinner as="span" size="sm" aria-hidden="true" />
              Loading...
            </CButton>
          ) : (
            <div id="withdrawal-table" className="">
              <AgGridReact
                columnDefs={state.columnDefs}
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
