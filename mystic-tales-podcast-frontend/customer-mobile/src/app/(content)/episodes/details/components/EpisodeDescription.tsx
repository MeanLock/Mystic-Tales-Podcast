import HtmlText from "@/src/components/renderHtml/HtmlText";
import { View } from "@/src/components/ui/View";

const EpisodeDescription = ({ description }: { description: string }) => {
  return (
    <View className="w-full p-4 mb-6">
      <HtmlText html={description} color="white" />
    </View>
  );
};

export default EpisodeDescription;
