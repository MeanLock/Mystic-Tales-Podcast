import { Button } from "@/components/ui/button";
import { pauseAudio, playAudio } from "@/redux/slices/mediaPlayerSlice/mediaPlayerSlice";
import { useDispatch } from "react-redux";

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
        FileUrl:
          "https://www.youtube.com/watch?v=plnfIj7dkJE&list=RDplnfIj7dkJE",
        LatestPosition: 0,
        PodcasterName: "The Weeknd",
        Show: {
          Id: 1,
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
        FileUrl:
          "https://www.youtube.com/watch?v=-MZ8guTxcFU&list=RD-MZ8guTxcFU",
        LatestPosition: 0,
        PodcasterName: "Lauv",
        Show: {
          Id: 2,
          Name: "First Love Album",
        },
      })
    );
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
    </div>
  );
};

export default TrendingPage;
