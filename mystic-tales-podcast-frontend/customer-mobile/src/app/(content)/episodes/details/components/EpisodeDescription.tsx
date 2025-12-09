import HtmlText from "@/src/components/renderHtml/HtmlText";
import { View } from "@/src/components/ui/View";
import { Text } from "react-native";

const EpisodeDescription = ({ description }: { description: string }) => {
  return (
    <View className="w-full p-4 mb-6 gap-2">
      <Text className="text-white text-2xl font-bold">Description</Text>
      <HtmlText html={description} color="white" />
    </View>
  );
};

export default EpisodeDescription;
