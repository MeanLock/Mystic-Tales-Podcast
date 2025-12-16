import HtmlText from "@/src/components/renderHtml/HtmlText";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";

const ShowDescription = ({ description }: { description: string }) => {
  return (
    <View className="gap-3">
      <Text className="text-[30px] font-bold text-white">Description</Text>
      <HtmlText html={description} color="white" />
    </View>
  );
};
export default ShowDescription;
