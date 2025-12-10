import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import HtmlText from "@/src/components/renderHtml/HtmlText";
import { Channel } from "@/src/core/types/channel.type";
import { useRouter } from "expo-router";
import { Pressable, Text, View } from "react-native";

const ChannelCard = ({ channel }: { channel: Channel }) => {
  const router = useRouter();
  return (
    <Pressable
      onPress={() => router.push(`/(content)/channels/details/${channel.Id}`)}
      key={channel.Id}
      className="w-full flex flex-row items-center gap-3 p-2 border-b-[0.5px] border-b-[#333]"
    >
      <AutoResolvingImage
        FileKey={channel.MainImageFileKey}
        type="PodcastPublicSource"
        key={channel.Id}
        style={{ width: 80, height: 80, borderRadius: 100 }}
      />
      <View className="flex-1 overflow-hidden justify-between">
        <Text className="text-white font-bold" numberOfLines={1}>
          {channel.Name}
        </Text>

        <Text className="font-light text-[#D9D9D9]">
          {channel.ShowCount} shows
        </Text>
      </View>
    </Pressable>
  );
};
export default ChannelCard;
