
import { type FC, useContext, useEffect, useMemo, useState } from "react"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import type { ColDef } from "ag-grid-community"
import { CommunitySurveyDetailViewContext } from ".."
import { CButton, CCol, CRow, CSpinner } from "@coreui/react"

ModuleRegistry.registerModules([AllCommunityModule])

interface GridState {
    columnDefs: any[];
    rowData: any[];
}

const state_creator = (table: any[]) => {

    const state = {
        columnDefs: [
            { headerName: "Taken Result ID", field: "Id" },
            {
                headerName: "Taker ID",
                field: "takenHistory.Taker.Id",
                valueGetter: (params: any) => params.data.Taker.Id || "N/A",

            },
            {
                headerName: "Taker Name",
                field: "takenHistory.Taker.FullName",
                valueGetter: (params: any) => params.data.Taker.FullName || "N/A",

            },
            {
                headerName: "Total Reward",
                field: "MoneyEarned",
                valueFormatter: (params: any) =>
                    params.value != null ? `${params.value.toLocaleString("en-US")}đ` : "N/A",
                cellRenderer: (params: any) => {
                    const value = params.value != null ? `${params.value.toLocaleString("en-US")}đ` : "N/A"
                    return (
                        <span className="text-success">
                            {value}
                        </span>
                    )
                }
            },
            {
                headerName: "XP Reward",
                field: "XpEarned",
                cellRenderer: (params: any) => {
                    const value = params.value != null ? `${params.value} ` : "N/A"
                    return (
                        <span  className="text-info">
                            {value}
                        </span>
                    )
                }
            },
            {
                headerName: "Taken At",
                field: "CompletedAt",
                valueFormatter: (params: any) => {
                    if (params.value) {
                        return new Date(params.value).toLocaleDateString("vi-VN")
                    }
                    return "N/A"
                },
            },


        ],
        rowData: table

    }
    return state
}
const TakenHistoryTab: FC = () => {
    let [state, setState] = useState<GridState | null>(null);
    const context = useContext(CommunitySurveyDetailViewContext)
    const [loading, setLoading] = useState(false);

       const handleDataChange = async () => {
        if (!context?.surveyData) return;
        
        //console.log("TakenHistoryTab: Loading data...");
        setLoading(true);
        
        try {
            const takenResults = context.surveyData.SurveyTakenResults || [];
            setState(state_creator(takenResults));
        } catch (error) {
            console.error("Error loading taken history data:", error);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (context?.activeTab === "taken-history" && context?.surveyData) {
            handleDataChange();
        }
    }, [context?.activeTab, context?.surveyData]);

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
        <CRow>
            <CCol xs={12}>
                {state === null ? <CButton className="w-100" color="secondary" disabled>
                    <CSpinner as="span" size="sm" aria-hidden="true" />
                    Loading...
                </CButton> :
                    <div>
                        <AgGridReact
                            columnDefs={state.columnDefs}
                            rowData={state.rowData}
                            defaultColDef={defaultColDef}
                            rowHeight={70}
                            headerHeight={40}
                            pagination={true}
                            paginationPageSize={10}
                            paginationPageSizeSelector={[10, 20, 50, 100]}
                            domLayout='autoHeight'
                        />
                    </div>}
            </CCol>
        </CRow>
    )
}

export default TakenHistoryTab
