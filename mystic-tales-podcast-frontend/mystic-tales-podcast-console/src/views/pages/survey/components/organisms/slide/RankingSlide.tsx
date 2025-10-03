/* eslint-disable @typescript-eslint/no-explicit-any */
import { useDispatch, useSelector } from "react-redux";
import { handleUpdateRaking } from "@/app/appSlice";
import type { RootState } from "@/app/store";
import { useAppSelector } from "@/app/hooks";
import { HiddenCheck } from "../../molecules/hiddenCheck/HiddenCheck";

interface Props {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    data: any;
}

const RankingSlide = ({ data }: Props) => {
    const dispatch = useDispatch();
    const config = useAppSelector((state) => state.appSlice.infoSurvey);
    const isValid = useAppSelector((state) => state.appSlice.isValid);

    const questionId = data?.ValueJson?.QuestionContent?.Id;

    const ranking: { SurveyOptionId: string; RankIndex: number }[] =
        useSelector(
            (state: RootState) =>
                (
                    state.appSlice.surveyData?.SurveyResponses.find(
                        (r: any) => r.ValueJson.QuestionContent.Id === questionId
                    )?.ValueJson.QuestionResponse as any
                )?.Ranking || []
        );

    const options = data?.ValueJson?.QuestionContent?.Options || [];

    const handleRank = (id: string, content: string) => {
        // if (isValid) return;
        const idx = ranking.findIndex((r) => r.SurveyOptionId === id);
        let newRanking;
        if (idx > -1) {
            newRanking = ranking.filter((r) => r.SurveyOptionId !== id);
        } else {
            newRanking = [
                ...ranking,
                {
                    SurveyOptionId: id,
                    RankIndex: ranking.length + 1,
                    Content: content,
                },
            ];
        }
        newRanking = newRanking.map((r, i) => ({
            SurveyOptionId: r.SurveyOptionId,
            RankIndex: i + 1,
        }));
        dispatch(
            handleUpdateRaking({
                idChoose: questionId,
                ranking: newRanking,
            })
        );
    };

    const selectedMap = Object.fromEntries(
        (ranking || []).map((r) => [r.SurveyOptionId, r.RankIndex])
    );

    return (
        <div className="flex flex-col gap-4 w-[90%] max-w-5xl mx-auto mt-6">
        <div className="relative"> {/* Add wrapper div for options */}
            {isValid && (
                <div 
                    className="absolute inset-0 bg-gray-200/50 z-10 rounded-lg"
                    style={{ cursor: 'not-allowed' }}
                />
            )}
            {options.map((op: any) => (
                <div className="flex justify-center items-center gap-5 w-[100%] mb-4">
                    {op?.MainImageUrl && !op.MainImageUrl.includes("unknown.jpg") ? (
                        <img
                            alt="image"
                            src={op?.MainImageUrl}
                            className="w-[100px] object-contain"
                        />
                    ) : null}

                    <button
                        key={op.Id}
                        onClick={() => handleRank(op.Id, op.Content)}
                        className={`text-left px-5 py-2 rounded transition-all duration-150 font-medium text-lg flex items-center flex-1
                        ${
                            selectedMap[op.Id]
                                ? "text-white"
                                : "bg-transparent text-white border border-white"
                        }
                    `}
                        style={{
                            background: selectedMap[op.Id]
                                ? config?.ConfigJson?.ButtonBackgroundColor ||
                                  "#24738a"
                                : "transparent",
                            color: selectedMap[op.Id]
                                ? config?.ConfigJson?.ButtonContentColor ||
                                  "#ffffff"
                                : config?.ConfigJson?.ContentColor || "#000",
                        }}
                    >
                        {selectedMap[op.Id] && (
                            <span className="mr-3 font-bold">
                                #{selectedMap[op.Id]}
                            </span>
                        )}
                        {op.Content}
                    </button>
                </div>
            ))}
        </div>
        <HiddenCheck id={data?.ValueJson.QuestionContent.QuestionTypeId} />
    </div>
    );
};

export default RankingSlide;
