import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import { useParams } from 'react-router-dom';
import { CSpinner } from '@coreui/react';
import { InfoCard } from './InfoCard';
import { XPTrackingCard } from './XPTrackingCard';
import { BonusTrackingCard } from './BonusTrackingCard';
import CommunitySurveyDetailTabs from './CommunitySurveyDetailTab';
import { getCommunitySurveyDetail } from '../../../../core/services/survey/survey-core/survey-core.service';
import { managerAxiosInstance } from '../../../../core/api/rest-api/config/instances/v2/manager-axios-instance';
import { CommunitySurvey } from '../../../../core/types';
import SurveyTalkLoading from '../../../components/common/loading';


ModuleRegistry.registerModules([AllCommunityModule]);

interface CommunitySurveyDetailViewProps { }

interface CommunitySurveyDetailViewContextProps {
    handleDataChange: () => void;
    surveyData: CommunitySurvey | null;
    activeTab: string;
    onTabChange: (tabKey: string) => void;
}


export const CommunitySurveyDetailViewContext = createContext<CommunitySurveyDetailViewContextProps | null>(null);

const CommunitySurveyDetailView: FC<CommunitySurveyDetailViewProps> = () => {
    const { id } = useParams<{ id: string }>();
    const [surveyData, setSurveyData] = useState<CommunitySurvey | null>(null);
    const [loading, setLoading] = useState(true)
        const [activeTab, setActiveTab] = useState("preview-questions");

    const handleDataChange = async () => {
        if (id) {
            //console.log("Refreshing data for ID:", id)
            setLoading(true)
            try {
                const response = await getCommunitySurveyDetail(managerAxiosInstance, Number(id));
                if (response.success && response.data) {
                    setSurveyData(response.data.Survey);
                } else {
                    console.error('API Error:', response.message);
                }
            } catch (error) {
                console.error("Error fetching survey data:", error)
            } finally {
                setLoading(false)
            }
        }
    }
        const handleTabChange = async (tabKey: string) => {
        //console.log("Tab changed to:", tabKey);
        setActiveTab(tabKey);
        await handleDataChange();
    };
    useEffect(() => {
        handleDataChange();
    }, [id]);

    if (loading) {
        return (
            <div className="loading-spinner">
                < SurveyTalkLoading />
            </div>
        )
    }
     if (!surveyData) {
        return <div>No survey data found</div>;
    }
    return (
        <CommunitySurveyDetailViewContext.Provider value={{ handleDataChange, surveyData, activeTab, onTabChange: handleTabChange }}>

            <div className="community-survey-detail">
                <div className="d-flex gap-4 mb-5 ">
                    <div className="community-survey-detail__card">
                        <InfoCard />
                    </div>

                    <div className="community-survey-detail__card">
                        <h5 className="fw-bold mb-3">XP Tracking</h5>
                        <XPTrackingCard />
                    </div>
                    <div className="community-survey-detail__card">
                        <h5 className="fw-bold mb-3">Bonus Tracking</h5>
                        <BonusTrackingCard />
                    </div>
                </div>
                <CommunitySurveyDetailTabs />
            </div>
        </CommunitySurveyDetailViewContext.Provider>

    );
}

export default CommunitySurveyDetailView;



