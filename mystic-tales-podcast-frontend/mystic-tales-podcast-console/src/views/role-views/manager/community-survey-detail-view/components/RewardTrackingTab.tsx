
import { type FC, useContext, useEffect, useMemo, useState } from "react"
import { AgGridReact } from "ag-grid-react"
import { AllCommunityModule, ModuleRegistry } from "ag-grid-community"
import type { ColDef } from "ag-grid-community"
import { CommunitySurveyDetailViewContext } from ".."
import { CButton, CCol, CRow, CSpinner } from "@coreui/react"
import SurveyTalkLoading from "../../../../components/common/loading"

ModuleRegistry.registerModules([AllCommunityModule])

interface GridState {
    columnDefs: any[];
    rowData: any[];
}

const state_creator = (table: any[]) => {

    const state = {
        columnDefs: [
            { headerName: "Reward TRacking ID", field: "Id" },
            {
                headerName: "Change Date",
                field: "CreatedAt",
                valueFormatter: (params: any) => {
                    if (params.value) {
                        return new Date(params.value).toLocaleDateString("vi-VN")
                    }
                    return "N/A"
                },
            },
            {
                headerName: "Point Change To",
                field: "RewardPrice",
                valueFormatter: (params: any) =>
                    params.value != null ? `${params.value.toLocaleString("en-US")}` : "N/A",
            },
            {
                headerName: "Change Amount",
                field: "RewardPrice",
                cellRenderer: (params: any) => {
                    const baseReward = params.context?.baseReward ?? 0;
                    const value = params.value != null ? params.value - baseReward : 0;
                    const formatted = value.toLocaleString("en-US");
                    const colorClass = value > 0 ? "text-success" : value < 0 ? "text-danger" : "";
                    const sign = value > 0 ? "+" : "";
                    return (
                    <span className={colorClass}>{sign}{formatted}</span>);
                }
            },
            {
                headerName: "XP Change To",
                field: "RewardXp",
            },
{
                headerName: "Change Amount",
                field: "RewardXp",
                cellRenderer: (params: any) => {
                    const maxXP = params.context?.maxXP ?? 0;
                    const value = params.value != null ? params.value - maxXP : 0;
                    const formatted = value.toLocaleString("en-US");
                    const colorClass = value > 0 ? "text-success" : value < 0 ? "text-danger" : "";
                    const sign = value > 0 ? "+" : "";
                    return (
                    <span className={colorClass}>{sign}{formatted}</span>);
                }
            },


        ],
        rowData: table

    }
    return state
}
const RewardTrackingTab: FC = () => {
    const [state, setState] = useState<GridState | null>(null);
    const [loading, setLoading] = useState(false);
    const context = useContext(CommunitySurveyDetailViewContext);

    const handleDataChange = async () => {
        if (!context?.surveyData) return;
        
        //console.log("RewardTrackingTab: Loading data...");
        setLoading(true);
        
        try {
            const rewardTracking = context.surveyData.SurveyRewardTrackings || [];
            setState(state_creator(rewardTracking));
        } catch (error) {
            console.error("Error loading reward tracking data:", error);
        } finally {
            setLoading(false);
        }
    };

    // Effect để load data khi tab được hiển thị hoặc data thay đổi
    useEffect(() => {
        if (context?.activeTab === "reward-tracking" && context?.surveyData) {
            handleDataChange();
        }
    }, [context?.activeTab, context?.surveyData]);

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
    }, []);

    if (loading) {
        return (
           <SurveyTalkLoading />
        );
    }

    return (
         <CRow>
            <CCol xs={12}>
                {state === null ? (
                    <div className="text-center">No data available</div>
                ) : (
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
                            context={{ 
                                baseReward: context?.surveyData?.TakerBaseRewardPrice ?? 0, 
                                maxXP: context?.surveyData?.SurveyPrivateData?.MaxXp ?? 0 
                            }}
                        />
                    </div>
                )}
            </CCol>
        </CRow>
    )
}

export default RewardTrackingTab
