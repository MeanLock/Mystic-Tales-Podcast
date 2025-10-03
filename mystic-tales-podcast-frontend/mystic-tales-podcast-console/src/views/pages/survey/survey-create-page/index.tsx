
import "./styles.scss";

import React from "react";
import SurveyNew from "../pages/SurveyNew";

interface Props {}

const SurveyCreatePage: React.FC<Props> = ({}) => {
  return (
    <div className="w-full flex flex-col">
      <SurveyNew />
    </div>
  );
};

export default SurveyCreatePage;
