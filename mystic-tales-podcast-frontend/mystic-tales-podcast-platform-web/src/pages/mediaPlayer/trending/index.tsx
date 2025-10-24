import { Button } from "@/components/ui/button";
import {
  addToQueue,
  pauseAudio,
  playAudio,
} from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useDispatch } from "react-redux";

const mockAudio = [
  {
    Id: "3",
    Name: "Feelings - Lauv",
    AudioLength: 184,
    EpisodeNumber: 2,
    FileUrl: "/audio/3.mp3",
    ImageUrl:
      "https://i.pinimg.com/736x/5e/36/fc/5e36fc8e37f897c30a030f6c4260d3c9.jpg",
    PodcasterName: "Lauv",
    Show: { Id: "3", Name: "How I'm Feelings" },
  },
  {
    Id: "4",
    Name: "Lauv & Troye Sivan - i'm so tired...",
    AudioLength: 163,
    EpisodeNumber: 3,
    FileUrl: "/audio/4.mp3",
    ImageUrl:
      "https://i.pinimg.com/736x/05/d8/34/05d8343b0c1ec2426216e81f3dc53891.jpg",
    PodcasterName: "Lauv",
    Show: { Id: "3", Name: "How I'm Feelings" },
  },
  {
    Id: "5",
    Name: "Lauv - Modern Loneliness",
    AudioLength: 247,
    EpisodeNumber: 4,
    FileUrl: "/audio/5.mp3",
    ImageUrl:
      "https://i.pinimg.com/736x/bd/bf/81/bdbf819478933ca6ba1a5b7ae6fc394e.jpg",
    PodcasterName: "Lauv",
    Show: { Id: "3", Name: "How I'm Feelings" },
  },
  {
    Id: "6",
    Name: "Mai Về / Dfoxie37 x Myhoa / 37 SOUND",
    AudioLength: 282,
    EpisodeNumber: 5,
    FileUrl: "/audio/6.mp3",
    ImageUrl:
      "https://i.pinimg.com/736x/bd/96/00/bd96009be6408259c976ae73da11d26e.jpg",
    PodcasterName: "Dfoxie37",
    Show: { Id: "4", Name: "Choa Nói Thật" },
  },
];
const TrendingPage = () => {
  const dispatch = useDispatch();

  const addCurrentAudio = () => {
    dispatch(
      playAudio({
        Id: "1",
        Name: "STAR BOY - The Weeknd",
        ImageUrl:
          "https://i.pinimg.com/1200x/5f/f4/83/5ff483cf28501863e5415cd1546951de.jpg",
        AudioLength: 230,
        EpisodeNumber: 1,
        FileUrl: "/audio/1.mp3",
        LatestPosition: 0,
        PodcasterName: "The Weeknd",
        Show: {
          Id: "1",
          Name: "Star Boy The Album",
        },
      })
    );
  };

  const addAnotherAudio = () => {
    dispatch(
      playAudio({
        Id: "2",
        Name: "Lauv ft. Julia Michaels - There's No Way",
        ImageUrl:
          "https://i.pinimg.com/1200x/19/08/af/1908af99490b06b08767873e480ee9a9.jpg",
        AudioLength: 180,
        EpisodeNumber: 1,
        FileUrl: "/audio/2.mp3",
        LatestPosition: 0,
        PodcasterName: "Lauv",
        Show: {
          Id: "2",
          Name: "First Love Album",
        },
      })
    );
  };

  const handleAddAudioToQueue = (id: string) => {
    const audioToAdd = mockAudio.find((a) => a.Id === id);
    if (audioToAdd) {
      dispatch(
        addToQueue({
          audio: audioToAdd,
          position: "to-last",
        })
      );
    }
  };

  return (
    <div className="flex flex-col gap-10">
      <p className="text-3xl text-white font-poppins font-bold">
        Test Media Player Page
      </p>
      {/* Play 1 Audio Ngay */}
      <Button onClick={() => addCurrentAudio()}>
        <p>Play An Audio</p>
      </Button>

      {/* Play 1 Audio Khác Khi Đang Có Current Audio */}
      <Button onClick={() => addAnotherAudio()}>
        <p>Play Another Audio</p>
      </Button>

      <div className="w-full flex items-center justify-around">
        <Button onClick={() => handleAddAudioToQueue("3")}>
          <p>Add To Queue 1</p>
        </Button>

        <Button onClick={() => handleAddAudioToQueue("4")}>
          <p>Add To Queue 2</p>
        </Button>

        <Button onClick={() => handleAddAudioToQueue("5")}>
          <p>Add To Queue 3</p>
        </Button>

        <Button onClick={() => handleAddAudioToQueue("6")}>
          <p>Add To Queue 4</p>
        </Button>
      </div>
    </div>
  );
};

export default TrendingPage;
