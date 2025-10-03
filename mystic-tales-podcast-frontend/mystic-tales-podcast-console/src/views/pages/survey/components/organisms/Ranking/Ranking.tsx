import { useEffect } from "react";
/* eslint-disable @typescript-eslint/no-unused-vars */
import type {
  OptionType,
  QuestionType,
  SurveyType,
} from "@/core/types/tools";
/* eslint-enable @typescript-eslint/no-unused-vars */
import ButtonAddAnswer from "../../molecules/buttons/ButtonAddAnswer";
import "./styles.scss";
import { answerDefault } from "../../../constants/questions";
import Answer from "../../molecules/answer/Answer";
import { generateUUID } from "@/core/utils/uuid.util";
import { toast } from "react-toastify";

type Props = {
  formData: SurveyType;
  question: QuestionType;
  handleUpdateQuestion: (
    key: keyof QuestionType,
    value:
      | string
      | number
      | boolean
      | OptionType[]
      | Record<string, string | number>
  ) => void;
};

const Ranking = ({ question, handleUpdateQuestion, formData }: Props) => {
  useEffect(() => {
    if (!question?.Options?.length) {
      handleUpdateQuestion("Options", [{ ...answerDefault,Id: generateUUID(), Order: 1 }]);
    }
  }, [question, handleUpdateQuestion]);

  const handleUpdateOption = (updatedOption: OptionType) => {
    const updatedOptions = (question?.Options || []).map((option) =>
      option.Order === updatedOption.Order ? updatedOption : option
    );
    handleUpdateQuestion("Options", updatedOptions);
  };

const handleDeleteOption = (orderToDelete: number) => {
  // Get the option being deleted
  const optionToDelete = question.Options.find(
    (option: any) => option.Order === orderToDelete
  );

  if (!optionToDelete) return;

  const affectedQuestions = new Set<number>(); // Track affected questions

  const allQuestions = formData.Questions || [];
  
  allQuestions.forEach((q: any) => {
    if (q.ConfigJson?.JumpLogics?.length > 0) {
      q.ConfigJson.JumpLogics = q.ConfigJson.JumpLogics.filter((jumpLogic: any) => {
        const hasConditionReference = jumpLogic.Conditions.some(
          (condition: any) => condition.OptionId === optionToDelete.Id
        );

        if (hasConditionReference) {
          affectedQuestions.add(q.Order);
        }

        return !hasConditionReference;
      });

      if (q.ConfigJson.JumpLogics.length === 0) {
        delete q.ConfigJson.JumpLogics;
      }
    }
  });

  // Update the options list
  const updatedOptions = question.Options.filter(
    (option: any) => option.Order !== orderToDelete
  ).map((option: any, index: number) => ({
    ...option,
    Order: index + 1,
  }));

  handleUpdateQuestion("Options", updatedOptions);

  // Show warning if any jump logic was affected
  if (affectedQuestions.size > 0) {
    const affectedQuestionsList = Array.from(affectedQuestions).sort((a, b) => a - b);
    toast.warning(`Một trong số các khối điều kiện ở các câu ${affectedQuestionsList.join(', ')} đã bị xóa, vui lòng kiểm tra lại`);
  }
};

  const handleAddAnswer = () => {
    const newOrder = question?.Options?.length
      ? Math.max(...question.Options.map((o) => o.Order)) + 1
      : 1;
    const newOption = { ...answerDefault, Id: generateUUID(),Order: newOrder };
    const updatedOptions = [...(question?.Options || []), newOption];
    handleUpdateQuestion("Options", updatedOptions);
  };

  return (
    <div className="ranking flex flex-col gap-2">
      {question?.Options?.length
        ? question.Options.map((item) => {
            return (
              <Answer
                data={item}
                key={item.Order}
                handleUpdateOption={handleUpdateOption}
                handleDeleteOption={handleDeleteOption}
                isDisableClose={false}
                formData={formData}
              />
            );
          })
        : null}
      <ButtonAddAnswer onClick={handleAddAnswer} />
    </div>
  );
};

export default Ranking;
