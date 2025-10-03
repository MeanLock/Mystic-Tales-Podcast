import "./styles.scss";
import ButtonAddAnswer from "../buttons/ButtonAddAnswer";
import type { OptionType, QuestionType } from "@/core/types/tools";
import { useCallback, useEffect } from "react";

type Props = {
  isDisable?: boolean;
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
const optionDefault = {
  Id:"",
  Order: 1,
  Content: "",
};
const ShortSentences = ({
  question,
  handleUpdateQuestion,
  isDisable,
}: Props) => {
  const handleAddOption = useCallback(() => {
    handleUpdateQuestion("Options", [
      ...question.Options,
      { ...optionDefault, Order: question.Options.length + 1 },
    ]);
  }, [handleUpdateQuestion, question.Options]);
  //console.log(isDisable);
  useEffect(() => {
    if (!question?.Options?.length) {
      handleUpdateQuestion("Options", [optionDefault]);
    }
  }, [handleUpdateQuestion, question]);
  return (
    <div className="short-sentences-container">
      {question?.Options?.length
        ? question.Options.map((_, index) => {
            return (
              <input
                key={index}
                type="text"
                disabled
                placeholder="Vui lòng nhập tại đây"
                className="text-input"
              />
            );
          })
        : null}
      {isDisable ? null : <ButtonAddAnswer onClick={() => handleAddOption()} />}
    </div>
  );
};

export default ShortSentences;
