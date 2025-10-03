import { createContext, FC, useEffect, useMemo, useState } from 'react'
import './styles.scss'
import { AgGridReact } from 'ag-grid-react';
import { CButton, CButtonGroup, CCard, CCardBody, CCol, CRow, CSpinner } from '@coreui/react';
import { AllCommunityModule, ModuleRegistry } from 'ag-grid-community';
import CustomerUpdate from '../customer-view/CustomerUpdate';
import { Eye } from 'phosphor-react';
import Modal_Button from '../../../components/common/modal/ModalButton';

ModuleRegistry.registerModules([AllCommunityModule]);

interface LevelData {
    id: number;
    title: string;
    outerCircleColor: string;
    innerCircleColor: string;
    numberColor: string;
    surveysNeeded: number;
    userCount: number;
    penaltyPerDay: number;
}
const mockLevelData: LevelData[] = [
    {
        id: 1,
        title: "Level 1",
        outerCircleColor: "#4CAF50",
        innerCircleColor: "#81C784",
        numberColor: "#1B5E20",
        surveysNeeded: 10,
        userCount: 150,
        penaltyPerDay: 1
    },
    {
        id: 2,
        title: "Level 2",
        outerCircleColor: "#2196F3",
        innerCircleColor: "#64B5F6",
        numberColor: "#0D47A1",
        surveysNeeded: 20,
        userCount: 100,
        penaltyPerDay: 2
    },
    {
        id: 3,
        title: "Level 3",
        outerCircleColor: "#9C27B0",
        innerCircleColor: "#BA68C8",
        numberColor: "#4A148C",
        surveysNeeded: 30,
        userCount: 90,
        penaltyPerDay: 3
    },
    {
        id: 4,
        title: "Level 4",
        outerCircleColor: "#FF9800",
        innerCircleColor: "#FFB74D",
        numberColor: "#E65100",
        surveysNeeded: 40,
        userCount: 80,
        penaltyPerDay: 4
    },
    {
        id: 5,
        title: "Level 5",
        outerCircleColor: "#E91E63",
        innerCircleColor: "#F06292",
        numberColor: "#880E4F",
        surveysNeeded: 50,
        userCount: 70,
        penaltyPerDay: 5
    },
    {
        id: 6,
        title: "Level 6",
        outerCircleColor: "#00BCD4",
        innerCircleColor: "#4DD0E1",
        numberColor: "#006064",
        surveysNeeded: 60,
        userCount: 60,
        penaltyPerDay: 6
    },
    {
        id: 7,
        title: "Level 7",
        outerCircleColor: "#CDDC39",
        innerCircleColor: "#DCE775",
        numberColor: "#827717",
        surveysNeeded: 70,
        userCount: 50,
        penaltyPerDay: 7
    },
    {
        id: 8,
        title: "Level 8",
        outerCircleColor: "#FFC107",
        innerCircleColor: "#FFD54F",
        numberColor: "#FF6F00",
        surveysNeeded: 80,
        userCount: 40,
        penaltyPerDay: 8
    },
    {
        id: 9,
        title: "Level 9",
        outerCircleColor: "#795548",
        innerCircleColor: "#A1887F",
        numberColor: "#3E2723",
        surveysNeeded: 90,
        userCount: 30,
        penaltyPerDay: 9
    },
    {
        id: 10,
        title: "Level 10",
        outerCircleColor: "#607D8B",
        innerCircleColor: "#90A4AE",
        numberColor: "#263238",
        surveysNeeded: 100,
        userCount: 20,
        penaltyPerDay: 10
    }
];

interface LevelViewProps { }
interface LevelViewContextProps {
    handleDataChange: () => void;
}

interface LevelCardProps {
    data: LevelData;
}

export const CustomerViewContext = createContext<LevelViewContextProps | null>(null);
const LevelCard: FC<LevelCardProps> = ({ data }) => {
    return (
        <CCard className="level-card h-100">
            <CCardBody className="d-flex flex-column p-0">
                <div className="level-circle-container">
                    <div
                        className="outer-circle"
                        style={{ backgroundColor: data.outerCircleColor }}
                    >
                        <div
                            className="inner-circle"
                            style={{ backgroundColor: data.innerCircleColor }}
                        >
                            <span style={{ color: data.numberColor }}>{data.id}</span>
                        </div>
                    </div>
                    <h3 className="level-title mt-3">{data.title}</h3>
                </div>

                <div className="level-info mt-2 mb-3">
                    <div className="info-item">
                        <label>Surveys Needed:</label>
                        <span>{data.surveysNeeded}</span>
                    </div>
                    <div className="info-item">
                        <label>Số lượng User:</label>
                        <span>{data.userCount}</span>
                    </div>
                    <div className="info-item">
                        <label>Điểm trừ:</label>
                        <span>{data.penaltyPerDay} điểm / ngày</span>
                    </div>
                </div>
            </CCardBody>
        </CCard>
    );
};


const LevelView: FC<LevelViewProps> = () => {

    return (
        <div className="level-view">
            <CRow className="g-4 justify-content-center">
                {mockLevelData.map(level => (
                    <CCol key={level.id} className="level-column">
                        <LevelCard data={level} />
                    </CCol>
                ))}
            </CRow>
        </div>
    );

}

export default LevelView;