import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ColDef, ModuleRegistry } from 'ag-grid-community';
import { Account } from '../../../../core/types';
import { adminAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2';
import SurveyTalkLoading from '../../../components/common/loading';
import { formatDate } from '../../../../core/utils/date.util';
import { getPlatformFeedback } from '@/core/services/statistic/user-statistics/user-statistics.service';

ModuleRegistry.registerModules([AllCommunityModule]);

interface PlatformFeedbackProps { }
interface PlatformFeedbackContextProps {
}
interface GridState {
  columnDefs: ColDef[];
  rowData: Account[];
}

export const PlatformFeedbackContext = createContext<PlatformFeedbackContextProps | null>(null);

const state_creator = (table: Account[]) => {
  const state = {
    columnDefs: [
      { headerName: "AccountId", field: "AccountId", flex: 0.4 },
      { headerName: "Rating Score", field: "RatingScore", flex: 0.6 },
       {
        headerName: "Comment", field: "Comment",
      },

      {
        headerName: "Created At",
        valueGetter: (params: { data: Account }) => formatDate(params.data.CreatedAt),
         flex: 0.4
      },
    
    
    ],
    rowData: table

  }
  return state
}

const PlatformFeedback: FC<PlatformFeedbackProps> = () => {
  let [state, setState] = useState<GridState | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  const handleDataChange = async () => {
    setIsLoading(true);
    try {
      const feedbackList = await getPlatformFeedback(adminAxiosInstance);
      if (feedbackList.success) {
        setState(state_creator(feedbackList.data));
      } else {
        console.error('API Error:', feedbackList.message);
      }
    } catch (error) {
      console.error('Lỗi khi fetch phản hồi nền tảng:', error);
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
    <PlatformFeedbackContext.Provider value={{ handleDataChange: handleDataChange }}>
      <CRow>
        <CCol xs={12}>
          {isLoading ? (
            <SurveyTalkLoading />
          ) : (
            <div
              id="Manager-table"
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

    </PlatformFeedbackContext.Provider>
  )
}

export default PlatformFeedback;