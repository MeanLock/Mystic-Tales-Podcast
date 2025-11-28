export type OptionType = {
  Id: string ;
  Content: string;
  Order: number;
  MainImageBase64?: string; // base64 image data
};

export type SlideType = {
  Min: number;
  Max: number;
  Step: number;
  Unit: string;
};

export type JumpLogicsType = {
  Conditions: {
    QuestionId: string;
    Conjunction: string;
    Operator: string;
    CompareValue: number;
  }[];
  TargetQuestionId: string;
};
export type QuestionType = {
  Id?: string ;
  MainImageBase64?: string;
  QuestionTypeId: number;
  Content: string;
  Description: string;
  TimeLimit: number;
  IsVoice: boolean;
  Order: number;
  ConfigJson: Record<string, string | number | SlideType[] | JumpLogicsType[]>;
  Options: OptionType[];
};

export type SurveyType = {
  Id: number;
  RequesterId: number;
  Title: string;
  Description: string;
  // IsPause: boolean;
  MarketSurveyVersionStatusId: number | null;
  SurveyTypeId: number;
  SurveyTopicId: number;
  SurveySpecificTopicId: number;
  SurveyStatusId: number;
  SecurityModeId: number;
  MainImageBase64?: string;
  BackgroundImageBase64?: string;
  IsSuccess?: boolean;
  ConfigJson: {
    Background: string;
    IsUseBackgroundImageBase64?: boolean;
    BackgroundGradient1Color: string;
    BackgroundGradient2Color: string;
    TitleColor: string;
    ContentColor: string;
    ButtonBackgroundColor: string;
    ButtonContentColor: string;
    Password: string | null;
    Brightness: number;
    IsResizableIframeEnabled?: boolean;
    DefaultBackgroundImageId: number;
    SkipStartPage: boolean;
  };
  Questions: QuestionType[];
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
} & any;

export interface PageProps {
  isDisable: boolean;
  formData: SurveyType;
  setFormData: React.Dispatch<React.SetStateAction<SurveyType>>;
  handleTabClick: (tabValue: number) => void;
  isTrigger?: boolean;
  defaultBackgroundThemes: any;
  securityModes: any;
  topics: any;
  specificTopics: any;
}
