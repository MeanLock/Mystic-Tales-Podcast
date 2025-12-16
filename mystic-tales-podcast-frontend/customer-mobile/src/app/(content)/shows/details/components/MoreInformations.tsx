import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { EpisodeFromShow } from "@/src/core/types/episode.type";
import { EvilIcons } from "@expo/vector-icons";
import { StyleSheet } from "react-native";
import { Pressable } from "react-native";

interface MoreInformationsProps {
  Podcaster: {
    Id: number;
    FullName: string;
    Email: string;
    MainImageFileKey: string;
  };
  ReleaseDate: string;
  Copyright: string;
  PodcastChannel: {
    Id: string;
    Name: string;
    MainImageFileKey: string;
  } | null;
  ShowEpisodeList: EpisodeFromShow[];
  PodcastCategory: {
    Id: number;
    Name: string;
  };
  PodcastSubCategory: {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
}

const InformationItem = ({
  title,
  value,
  isLink,
}: {
  title: string;
  value: string;
  isLink: boolean;
}) => {
  return (
    <View style={styles.itemContainer}>
      <Text className="text-[#D9D9D9]">{title}</Text>
      {isLink ? (
        <Pressable className="flex-row items-center gap-1">
          <Text className="text-[#AEE339]">{value}</Text>
          <EvilIcons name="external-link" color="#AEE339" size={24} />
        </Pressable>
      ) : (
        <Text numberOfLines={1} className="text-white max-w-[60%]">
          {value}
        </Text>
      )}
    </View>
  );
};

const MoreInformations = ({
  Podcaster,
  ReleaseDate,
  Copyright,
  PodcastChannel,
  ShowEpisodeList,
  PodcastCategory,
  PodcastSubCategory,
}: MoreInformationsProps) => {
  const getActiveTimeString = (
    releaseDateString: string,
    episodes: EpisodeFromShow[]
  ) => {
    // Parse show release date and get the start year
    const releaseDate = new Date(releaseDateString);
    const startYear = releaseDate.getFullYear();

    // If no episodes, return only the start year
    if (!episodes || episodes.length === 0) {
      return `${startYear} - Present`;
    }

    // Find the latest episode by release date
    const latestEpisode = episodes.reduce((latest, current) => {
      const latestDate = new Date(latest.ReleaseDate);
      const currentDate = new Date(current.ReleaseDate);
      return currentDate > latestDate ? current : latest;
    }, episodes[0]);

    // Get the latest episode's year
    const latestDate = new Date(latestEpisode.ReleaseDate);
    const endYear = latestDate.getFullYear();

    // Calculate the current year
    const currentYear = new Date().getFullYear();

    // If the latest episode is from this year, show as "Present"
    if (endYear === currentYear) {
      return `${startYear} - Present`;
    }

    // Otherwise, show the year range
    return `${startYear} - ${endYear}`;
  };

  const checkExplicitContent = (episodes: EpisodeFromShow[]): boolean => {
    // If there are no episodes, return false
    if (!episodes || episodes.length === 0) {
      return false;
    }

    // Use some() to check if any episode has explicit content
    // This is more efficient than filter() as it stops as soon as it finds one match
    return episodes.some((episode) => episode.ExplicitContent === true);
  };

  return (
    <View>
      <Text className="text-[30px] font-bold text-white">
        Show Informations
      </Text>

      <View className="mt-5 w-full">
        <InformationItem
          title="Creator"
          value={Podcaster.FullName}
          isLink={true}
        />
        <InformationItem
          title="Active Time"
          value={getActiveTimeString(ReleaseDate, ShowEpisodeList)}
          isLink={false}
        />
        <InformationItem
          title="Episodes"
          value={ShowEpisodeList.length.toString()}
          isLink={false}
        />
        <InformationItem
          title="Classify"
          value={
            checkExplicitContent(ShowEpisodeList)
              ? "Explicit Content"
              : "Safe Content"
          }
          isLink={false}
        />
        <InformationItem title="Copyright" value={Copyright} isLink={false} />
        <InformationItem
          title="Category"
          value={PodcastCategory.Name}
          isLink={true}
        />
        <InformationItem
          title="Sub Category"
          value={PodcastSubCategory.Name}
          isLink={true}
        />
        {PodcastChannel && (
          <InformationItem
            title="Channel"
            value={PodcastChannel.Name}
            isLink={true}
          />
        )}
      </View>
    </View>
  );
};

export default MoreInformations;

const styles = StyleSheet.create({
  itemContainer: {
    width: "100%",
    flexDirection: "row",
    justifyContent: "space-between",
    alignItems: "center",
    borderBottomWidth: 0.3,
    borderBottomColor: "#514F4F",
    paddingVertical: 10,
  },
});
