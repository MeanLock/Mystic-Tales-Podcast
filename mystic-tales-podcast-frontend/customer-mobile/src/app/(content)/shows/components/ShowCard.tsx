import AutoResolvingImage from "@/src/components/autoResolveImage/AutoResolvingImage";
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import { Show} from "@/src/core/types/show.type";
import { useRouter } from "expo-router";
import { Image, Pressable, StyleSheet } from "react-native";

interface ShowCardProps {
  Id: number;
  Title: string;
  NewEpisodeCount: number;
  LastUpdated: string;
  ImageUrl: string;
}

const ShowCard = ({ show, width }: { show: Show; width: number }) => {
  const router = useRouter();

  function renderUpdate(count: number, time: string): string {
    // 1) Không có time
    if (!time) return "No Updates";

    const t = new Date(time);
    if (isNaN(t.getTime())) return ""; // time không hợp lệ

    const now = Date.now();
    const diffMs = now - t.getTime();
    if (diffMs < 0) return ""; // thời điểm ở tương lai -> bỏ qua
    const pad = (n: number) => n.toString().padStart(2, "0");
    const formatDate = (d: Date) =>
      `${pad(d.getDate())}/${pad(d.getMonth() + 1)}/${d.getFullYear()}`;
    const MS_PER_HOUR = 3600_000;
    const MS_PER_DAY = 24 * MS_PER_HOUR;

    let base = "";

    // 2) count === 0
    if (count === 0) {
      if (diffMs < MS_PER_DAY) {
        const hours = Math.max(1, Math.floor(diffMs / MS_PER_HOUR));
        base = `${hours} ${hours === 1 ? "hour" : "hours"} ago`;
      } else if (diffMs < 5 * MS_PER_DAY) {
        const days = Math.floor(diffMs / MS_PER_DAY);
        if (days >= 1) {
          base = `${days} ${days === 1 ? "day" : "days"} ago`;
        }
      }
      return base; // có thể là "" nếu ngoài phạm vi
    }

    // 3) count > 0: giống trên + "y new episodes"
    if (diffMs < MS_PER_DAY) {
      const hours = Math.max(1, Math.floor(diffMs / MS_PER_HOUR));
      base = `${hours} ${hours === 1 ? "hour" : "hours"} ago`;
    } else if (diffMs < 5 * MS_PER_DAY) {
      const days = Math.floor(diffMs / MS_PER_DAY);
      if (days >= 1) {
        base = `${days} ${days === 1 ? "day" : "days"} ago`;
      }
    } else {
      return (base = `Updated at ${formatDate(t)}`); // ngoài phạm vi quy định
    }

    const epLabel = `${count} new ${count === 1 ? "episode" : "episodes"}`;
    return base ? ` ${epLabel} • ${base}` : epLabel;
  }

  const handleNavigate = (id: string) => {
    router.push({
      pathname: "/(content)/shows/details/[id]", // <-- CHỈ RÕ GROUP
      params: { id },
    });
  };

  return (
    <Pressable
      onPress={() => handleNavigate(show.Id)}
      style={[style.card, , { width: width, height: width + 50 }]}
    >
      <View style={[style.imageContainer, { height: width }]}>
        <AutoResolvingImage FileKey={show.MainImageFileKey} type="PodcastPublicSource" style={{resizeMode: "cover", width: "100%", height: "100%", borderRadius: 8 }} />
      </View>
      <View style={style.informationsContainer}>
        <Text style={style.title} numberOfLines={1}>
          {show.Name}
        </Text>
        <Text numberOfLines={1} style={style.updateDescription}>
          {show.UploadFrequency || show.UpdatedAt}
        </Text>
      </View>
    </Pressable>
  );
};

export default ShowCard;

const style = StyleSheet.create({
  card: {
    flexDirection: "column",
    justifyContent: "space-between",
  },
  imageContainer: {
    width: "100%",
    borderRadius: 8,
  },
  image: {
    width: "100%",
    height: "100%",
    resizeMode: "cover",
    borderRadius: 8,
  },
  informationsContainer: {
    width: "100%",
    height: 50,
    justifyContent: "space-between",
    padding: 2,
  },
  title: {
    color: "#fff",
    fontWeight: 500,
  },
  updateDescription: {
    color: "#999999",
  },
});
