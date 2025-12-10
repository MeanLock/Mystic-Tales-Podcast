import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { useRouter } from "expo-router";
import { Pressable, Text, View } from "react-native";

interface Props {
  show: {
    Id: string;
    Name: string;
    Description: string;
    MainImageFileKey: string;
    ReleaseDate: string;
    IsReleased: boolean;
  };
}

const ShowCard = ({ show }: Props) => {
  const router = useRouter();
  return (
    <Pressable
      onPress={() => router.push(`/(content)/shows/details/${show.Id}`)}
      className="w-full flex flex-row items-center gap-3 p-2 border-b-[0.5px] border-b-[#333]"
    >
      <AutoResolvingImage
        FileKey={show.MainImageFileKey}
        type="PodcastPublicSource"
        style={{ width: 80, height: 80, borderRadius: 8 }}
      />
      <View className="flex-1 overflow-hidden justify-between">
        <Text className="text-white font-bold" numberOfLines={1}>
          {show.Name}
        </Text>
        <HtmlText
          html={show.Description}
          numberOfLines={2}
          fontSize={14}
          color="#D9D9D9"
        />
      </View>
    </Pressable>
  );
};
export default ShowCard;
