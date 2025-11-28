import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useParams } from 'react-router-dom';
import FilterSurveyDetailTabs from './FilterSurveyDetailTab';
import { FilterSurvey } from '../../../../core/types';
import { managerAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2/manager-axios-instance';
import SurveyTalkLoading from '../../../components/common/loading';
import { getFilterSurveyDetail } from '../../../../core/services/survey/survey-core/survey-core.service';
import { formatDate } from '../../../../core/utils/date.util';


ModuleRegistry.registerModules([AllCommunityModule]);

interface FilterSurveyDetailViewProps { }

interface FilterSurveyDetailViewContextProps {
    handleDataChange: () => void;
    survey: FilterSurvey | null

}
export const FilterSurveyDetailViewContext = createContext<FilterSurveyDetailViewContextProps | null>(null);

const FilterSurveyDetailView: FC<FilterSurveyDetailViewProps> = () => {
    const { id } = useParams<{ id: string }>();
    const [survey, setsurvey] = useState<FilterSurvey | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const handleDataChange = async () => {
        setIsLoading(true);
        const response = await getFilterSurveyDetail(managerAxiosInstance, (Number)(id));
        if (response.success && response.data) {
            setsurvey(response.data.Surveys);
        } else {
            console.error('API Error:', response.message);
        }
        setIsLoading(false);
    }

    useEffect(() => {
        handleDataChange()
    }, [id]);

    return (
    <div className="filter-survey-detail">
        {isLoading ? (
            <SurveyTalkLoading />
        ) : survey === null ? (
            <div className="text-center text-danger">
                Không tìm thấy khảo sát với ID: {id}
            </div>
        ) : (
            <>
                <h2 className="filter-survey-detail__title mb-5">Chi Tiết Khảo Sát Đầu Vào</h2>
                <div className="d-flex gap-5 mb-5 ">
                    <div className="filter-survey-detail__card">
                        <h5 className="mb-3 fw-bold">
                            {survey?.Title}
                        </h5>
                        <div className="d-flex gap-2 align-items-center">
                            <span className="filter-survey-detail__card-label">Số câu hỏi:</span>
                            <span className="filter-survey-detail__card-value">{survey?.QuestionCount} câu</span>
                        </div>

                        <div className="d-flex gap-2">
                            <span className="filter-survey-detail__card-label">Ngày publish:</span>
                            <span className="filter-survey-detail__card-value">{formatDate(survey.PublishedAt || '')}</span>
                        </div>

                        <div className="d-flex gap-2">
                            <span className="filter-survey-detail__card-label">Trạng thái:</span>
                            <span
                                className={`filter-survey-detail__card-value ${survey?.IsAvailable ? 'text-success' : 'text-danger'
                                    }`}
                            >
                                {survey?.IsAvailable ? 'Đang hoạt động' : 'Ngừng hoạt động'}
                            </span>
                        </div>

                        <div className="d-flex gap-2">
                            <span className="filter-survey-detail__card-label">XP Reward:</span>
                            <div className="flex items-center gap-1">
                                <span className="filter-survey-detail__card-value">{survey?.SurveyPrivateData?.MaxXp}</span>
                            </div>
                        </div>
                    </div>

                    {/* <div className="filter-survey-detail__card">
                        <h5>Tổng số lượng Takers</h5>
                        <h5 className='fw-bold'> 1,000 Takers</h5>

                        <div className="mt-3 text-xs text-gray-500 text-center">
                            Thống kê
                        </div>
                    </div> */}
                </div>
                <FilterSurveyDetailViewContext.Provider value={{ handleDataChange, survey }}>
                    <FilterSurveyDetailTabs />
                </FilterSurveyDetailViewContext.Provider>
            </>
        )}
    </div>

    );
}

export default FilterSurveyDetailView;



