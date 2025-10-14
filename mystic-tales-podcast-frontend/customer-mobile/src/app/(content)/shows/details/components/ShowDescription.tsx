import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";

const ShowDescription = ({ description }: { description: string }) => {
  return (
    <View className="gap-3">
      <Text className="text-[30px] font-bold text-white">Description</Text>
      <Text className="text-justify">{description}</Text>
    </View>
  );
};
export default ShowDescription;
