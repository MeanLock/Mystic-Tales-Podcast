
import { type FC, useContext, useEffect, useMemo, useState } from "react"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import { DataMarketDetailViewContext } from ".."
import type { ColDef } from "ag-grid-community"

ModuleRegistry.registerModules([AllCommunityModule])

interface PaymentHistoryData {
    id: string
    type: string
    contributorId: string
    amount: string
    versionId: string
    time: string
}
// Mock data
    const mockTransactionData: PaymentHistoryData[] = [
        {
            id: "3f2b7e8c-1234-4e91-bc56-8bb8da29ad4f",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1234",
            amount: "10,000đ",
            versionId: "svdt-0231-213123",
            time: "20/05/2025",
        },
        {
            id: "9ef23fba-41e2-4b3f-9fe2-2cb0f4ca2fe5",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1234",
            amount: "15,000đ",
            versionId: "svdt-0231-213124",
            time: "21/05/2025",
        },
        {
            id: "a8eff74-3345-4b4a-b3ab-1d5bf739f4ec",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1235",
            amount: "8,000đ",
            versionId: "svdt-0231-213125",
            time: "22/05/2025",
        },
    ]

    const mockCommissionData: PaymentHistoryData[] = [
        {
            id: "3f2b7e8c-1234-4e91-bc56-8bb8da29ad4f",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1234",
            amount: "10,000đ",
            versionId: "svdt-0231-213123",
            time: "20/05/2025",
        },
        {
            id: "9ef23fba-41e2-4b3f-9fe2-2cb0f4ca2fe5",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1234",
            amount: "15,000đ",
            versionId: "svdt-0231-213124",
            time: "21/05/2025",
        },
        {
            id: "a8eff74-3345-4b4a-b3ab-1d5bf739f4ec",
            type: "Trả tiền hoa hồng",
            contributorId: "3f2b7e8c-1235",
            amount: "8,000đ",
            versionId: "svdt-0231-213125",
            time: "22/05/2025",
        },
    ]

const PaymentHistoryTab: FC = () => {
    const [transactionData, setTransactionData] = useState<PaymentHistoryData[]>([])
    const [commissionData, setCommissionData] = useState<PaymentHistoryData[]>([])
    const [loading, setLoading] = useState(false)
    const context = useContext(DataMarketDetailViewContext)

    
    const defaultColDef: ColDef = useMemo(() => ({
        flex: 1,
        filter: true,
        resizable: true,
        sortable: true,
    }), [])

    const transactionColumnDefs: ColDef[] = useMemo(() => [
        { headerName: "Transaction ID", field: "id", flex: 1.5 },
        { headerName: "Transaction Type", field: "type" },
        { headerName: "To Contributor ID", field: "contributorId" },
        {
            headerName: "Số tiền",
            field: "amount",
            cellClass: "text-success",
        },
        { headerName: "Version ID", field: "versionId" },
        { headerName: "Thời gian", field: "time" },
    ], [])

    const commissionColumnDefs: ColDef[] = useMemo(() => [
        { headerName: "Transaction ID", field: "id", flex: 1.5 },
        { headerName: "Transaction Type", field: "type" },
        { headerName: "To Contributor ID", field: "contributorId" },
        {
            headerName: "Số tiền",
            field: "amount",
            cellClass: "text-danger",
        },
        { headerName: "Version ID", field: "versionId" },
        { headerName: "Thời gian", field: "time" },
    ], [])


    const loadAllData = async () => {
        setLoading(true)
        try {
            // Simulate API call
            await new Promise((resolve) => setTimeout(resolve, 100))
            setTransactionData(mockTransactionData)
            setCommissionData(mockCommissionData)

        } catch (error) {
            console.error("Error loading data:", error)
        } finally {
            setLoading(false)
        }
    }

    useEffect(() => {
        loadAllData()
        //console.log("check")
    }, [])

    return (
        <div className="payment-history-tab">
            {loading ? (
                <div className="payment-history-tab__loading">
                    <div className="spinner"></div>
                    <span>Đang tải dữ liệu...</span>
                </div>
            ) : (
                <div className="payment-history-tab__content">
                    <div className="payment-history-tab__section">
                        <h4 className="payment-history-tab__section-title">Lịch sử trả hoa hồng</h4>
                        <div className="ag-theme-alpine" style={{ width: "100%", height: "350px", marginBottom: "35px" }}>
                            <AgGridReact
                                columnDefs={commissionColumnDefs}
                                rowData={commissionData}
                                defaultColDef={defaultColDef}
                                rowHeight={55}
                                headerHeight={40}
                                pagination
                                paginationPageSize={10}
                                domLayout="normal"
                            />
                        </div>
                    </div>

                    <div className="payment-history-tab__section">
                        <h4 className="payment-history-tab__section-title">Lịch sử nhận tiền từ mua Data</h4>
                        <div className="ag-theme-alpine" style={{ width: "100%", height: "350px" }}>
                            <AgGridReact
                                columnDefs={transactionColumnDefs}
                                rowData={transactionData}
                                defaultColDef={defaultColDef}
                                rowHeight={55}
                                headerHeight={40}
                                pagination
                                paginationPageSize={10}
                                domLayout="normal"
                            />
                        </div>
                    </div>
                </div>
            )}
        </div>
    )
}

export default PaymentHistoryTab
