import type { PodcasterReviewAPI } from "@/core/types/podcaster";
import { BsFillPeopleFill } from "react-icons/bs";
import { FaStar } from "react-icons/fa";

const RatingChart = ({ rating }: { rating: PodcasterReviewAPI[] }) => {
  const getAverageRating = () => {
    const ratingCount = rating.length;
    if (ratingCount === 0) {
      return 5;
    } else {
      const ratingSum = rating.reduce(
        (sumOfRating, r) => sumOfRating + r.Rating,
        0
      );
      const avgRating = ratingSum / ratingCount;
      return avgRating.toFixed(1);
    }
  };

  const getRatingDistribution = () => {
    const distribution = [0, 0, 0, 0, 0]; // [1-star, 2-star, 3-star, 4-star, 5-star]

    rating.forEach((r) => {
      if (r.Rating >= 1 && r.Rating <= 5) {
        distribution[r.Rating - 1]++;
      }
    });

    return distribution.reverse(); // Reverse để 5 sao ở đầu
  };

  const getPercentage = (count: number) => {
    if (rating.length === 0) return 0;
    return (count / rating.length) * 100;
  };

  const distribution = getRatingDistribution();

  return (
    <div className="w-full grid grid-cols-1 md:grid-cols-12 gap-10">
      <div className="w-full py-7 md:col-span-6 lg:col-span-2 flex flex-col items-center gap-1 text-white">
        <p className="text-6xl md:text-7xl lg:text-8xl m-0 p-0 leading-none font-extrabold">
          {getAverageRating()}
          <span className="text-xl md:text-2xl">/5</span>
        </p>
        <div className="flex items-center text-[#D9D9D9] text-base md:text-lg gap-1">
          (<BsFillPeopleFill />
          <p>{rating.length}</p>)
        </div>
      </div>
      <div className="w-full h-full md:col-span-6 lg:col-span-10 flex gap-6 md:gap-8 text-white py-7">
        <div className="w-auto flex flex-col h-full justify-between text-sm md:text-base">
          {[5, 4, 3, 2, 1].map((stars, index) => (
            <div key={stars} className="flex items-center h-[1.5em] gap-1">
              <div className="flex items-center justify-start min-w-10 md:min-w-12.5">
                <p className="text-[#D9D9D9]">({distribution[index]})</p>
              </div>
              <div className="flex items-center gap-0.5 text-white">
                {Array.from({ length: stars }).map((_, i) => (
                  <FaStar key={i} className="w-3 h-3 md:w-4 md:h-4" />
                ))}
              </div>
            </div>
          ))}
        </div>

        <div className="flex-1 flex flex-col h-full justify-between">
          {distribution.map((count, index) => (
            <div key={index} className="flex items-center h-[1.5em]">
              <div className="w-full h-1 md:h-2 bg-gray-300/20 rounded-full overflow-hidden relative">
                <div
                  className="absolute left-0 top-0 h-full bg-white rounded-full transition-all duration-500"
                  style={{ width: `${getPercentage(count)}%` }}
                ></div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default RatingChart;
