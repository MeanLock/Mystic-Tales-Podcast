import type React from "react"
import {
    Chart as ChartJS,
    CategoryScale,
    LinearScale,
    PointElement,
    LineElement,
    Title,
    Tooltip,
    Legend,
} from "chart.js"
import { Line } from "react-chartjs-2"
import { CommunitySurveyDetailViewContext } from "."
import { useContext, useState } from "react"
import { updateMaxXpCommunitySurvey } from "../../../../core/services/survey/survey-core/survey-core.service"
import { managerAxiosInstance } from "../../../../core/api/rest-api/config/instances/v2/manager-axios-instance"
import { Modal, Button, Form } from "react-bootstrap";
import EditIcon from '@mui/icons-material/Edit';
import { toastFormat } from "../../../components/common/toast/toast"
import { toast } from "react-toastify"
ChartJS.register(CategoryScale, LinearScale, PointElement, LineElement, Title, Tooltip, Legend)

interface XPTrackingCardProps { }

export const XPTrackingCard: React.FC<XPTrackingCardProps> = () => {
    const context = useContext(CommunitySurveyDetailViewContext);
    const data = context?.surveyData?.SurveyRewardTrackings || [];
    const maxXP = context?.surveyData?.SurveyPrivateData?.MaxXp || 0;
    const surveyId = context?.surveyData?.Id || 0;

    const [showModal, setShowModal] = useState(false);
    const [newMaxXp, setNewMaxXp] = useState<number>(maxXP);
    const [isUpdating, setIsUpdating] = useState(false);

    const handleOpenModal = () => {
        setNewMaxXp(maxXP);
        setShowModal(true);
    };

    const handleCloseModal = () => {
        setShowModal(false);
    };

    const handleUpdateMaxXp = async () => {
        setIsUpdating(true);
        try {
            const response = await updateMaxXpCommunitySurvey(managerAxiosInstance, surveyId, newMaxXp);
            if (response.success) {
                toast.success("Cập nhật Max XP thành công");
                context?.handleDataChange(); 
                setShowModal(false);
            } else {
               toast.error("Lỗi khi cập nhật Max XP");
            }
        } catch (error) {
            console.error("Error updating Max XP:", error);
        } finally {
            setIsUpdating(false);
        }
    };

    const latestTracking = data.reduce((latest, item) => {
        return !latest || new Date(item.CreatedAt) > new Date(latest.CreatedAt) ? item : latest;
    }, null as typeof data[0] | null);

    const currentXp = latestTracking?.RewardXp || 0;
    const chartData = {
        labels: data.map((item) => new Date(item.CreatedAt).toLocaleDateString("vi-VN")),
        datasets: [
            {
                label: "XP Tracking",
                data: data.map((item) => item.RewardXp),
                borderColor: "rgb(75, 192, 192)",
                backgroundColor: "rgba(75, 192, 192, 0.2)",
                tension: 0.1,
            },
        ],
    }

    const options = {
        responsive: true,
        plugins: {
            legend: {
                display: false,
            },
            title: {
                display: false,
            },
        },
        scales: {
            y: {
                beginAtZero: true,
                max: Math.max(maxXP, Math.max(...data.map((item) => item.RewardXp))),
            },
        },
    }

    return (
        <div className="xp-tracking-chart">
            <div className="mb-3">
                <div className="d-flex justify-content-between align-items-center mb-2">
                    <span className="text-muted small">Max XP:</span>
                    <div className="xp-input-box">
                        <span className="fw-bold">{maxXP}</span>
                        <EditIcon
                            className="ms-2 text-secondary"
                            style={{ cursor: "pointer", fontSize: "1.2rem" }}
                            onClick={handleOpenModal}
                        />
                    </div>
                </div>
                <div className="d-flex justify-content-between align-items-center mb-3">
                    <span className="text-muted small">Current XP Reward:</span>
                    <span className="fw-bold text-primary">{currentXp}</span>
                </div>
            </div>
            <div style={{ height: "180px" }}>
                <Line data={chartData} options={options} />
            </div>
            <Modal show={showModal} onHide={handleCloseModal} centered>
                <Modal.Header closeButton>
                    <Modal.Title>Cập nhật Max XP</Modal.Title>
                </Modal.Header>
                <Modal.Body>
                    <Form.Group>
                        <Form.Label>Nhập giá trị Max XP mới:</Form.Label>
                        <Form.Control
                            type="number"
                            value={newMaxXp}
                            onChange={(e) => setNewMaxXp(Number(e.target.value))}
                            min={0}
                        />
                    </Form.Group>
                </Modal.Body>
                <Modal.Footer>
                    <Button variant="secondary" onClick={handleCloseModal}>
                        Hủy
                    </Button>
                    <Button
                        variant="primary"
                        onClick={handleUpdateMaxXp}
                        disabled={isUpdating}
                    >
                        {isUpdating ? "Đang cập nhật..." : "Cập nhật"}
                    </Button>
                </Modal.Footer>
            </Modal>
        </div>
    )
}
