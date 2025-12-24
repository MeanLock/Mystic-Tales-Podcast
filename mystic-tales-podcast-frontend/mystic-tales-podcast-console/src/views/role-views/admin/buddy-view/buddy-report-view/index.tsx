import { createContext, type FC, useEffect, useMemo, useState } from "react"
import "./styles.scss"
import { AgGridReact } from "ag-grid-react"
import { CButton, CCard, CCol, CFormInput, CRow, CSpinner } from "@coreui/react"
import { AllCommunityModule, ColDef, ModuleRegistry } from "ag-grid-community"
import { formatDate } from "../../../../../core/utils/date.util"
import { getBuddyReports } from "@/core/services/report/BuddyReport.service"
import { adminAxiosInstance } from "@/core/api/rest-api/config/instances/v2"
import Loading from "@/views/components/common/loading"

ModuleRegistry.registerModules([AllCommunityModule])

interface BuddyReportViewProps { }
interface BuddyReportViewContextProps {
    handleDataChange: () => void
}
interface GridState {
    columnDefs: ColDef[];
    rowData: any[];
}
export const BuddyReportViewContext = createContext<BuddyReportViewContextProps | null>(null)

const state_creator = (table: any[]) => {
    const state = {
        columnDefs: [
            {
                headerName: "No.",
                flex: 0.2,
                valueGetter: (params: any) => {
                    return params.node.rowIndex + 1; // Hiển thị số thứ tự từ 1
                },
                cellClass: '',
                sortable: false,
                filter: false
            },
            { headerName: "Content", field: "Content", flex: 0.8 },
            { headerName: "Reported By", field: "Account.FullName", flex: 0.8 },
            { headerName: "Podcast Buddy ", field: "PodcastBuddy.FullName", flex: 0.8 },
            { headerName: "Podcast Show Report Type ", field: "PodcastBuddyReportType.Name", flex: 0.8 },
            {
                headerName: "Created At",
                field: "CreatedAt",
                flex: 0.5,
                valueGetter: (params: any) => formatDate(params.data.CreatedAt),
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
                valueGetter: (params: { data: any }) => {
                    if (params.data.ResolvedAt !== null && params.data.ResolvedAt !== "") return "Resolved";
                    return "Unresolved";
                },
                cellRenderer: (params: { data: any }) => {
                    let status = {
                        title: '',
                        color: '',
                        bg: ''
                    };
                    if (params.data.ResolvedAt !== null && params.data.ResolvedAt !== "") {
                        status = {
                            title: 'Resolved',
                            color: 'var(--secondary-green)',
                            bg: 'rgba(173, 227, 57, 0.06)'
                        };

                    } else {
                        status = {
                            title: 'Unresolved',
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

        ],
        rowData: table

    }
    return state
}



const BuddyReportView: FC<BuddyReportViewProps> = () => {
    let [state, setState] = useState<GridState | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const handleDataChange = async () => {
        setIsLoading(true);
        try {
            const reportList = await getBuddyReports(adminAxiosInstance);
            console.log("Fetched buddy reports:", reportList);
            if (reportList.success) {
                setState(state_creator(reportList.data.BuddyReportList));
            } else {
                console.error('API Error:', reportList.message);
            }
        } catch (error) {
            console.error('Lỗi khi fetch buddy reports:', error);
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
        <BuddyReportViewContext.Provider value={{ handleDataChange }}>
            <div className="d-flex justify-content-between align-items-center mb-4">
                <h4 className="fw-semibold" style={{ color: "var(--primary-grey)" }}>Buddy Reports</h4>
            </div>
            <CRow >
                <CCol xs={12}>
                    {isLoading ? (
                        <div className="flex justify-content-center align-items-center h-150" >
                            <Loading />
                        </div>
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


        </BuddyReportViewContext.Provider>
    )
}

export default BuddyReportView
