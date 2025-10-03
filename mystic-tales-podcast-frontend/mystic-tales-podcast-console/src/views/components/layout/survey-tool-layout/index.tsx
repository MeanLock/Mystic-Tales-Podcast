import "./styles.scss";
import { SurveyLayoutContent } from "./SurveyLayoutContent";
import { SurveyLayoutHeader } from "./SurveyLayoutHeader";


const SurveyLayout = () => {
  return (
    <div>
      <SurveyLayoutHeader />
      <SurveyLayoutContent />
    </div>
  );
};

export default SurveyLayout;
