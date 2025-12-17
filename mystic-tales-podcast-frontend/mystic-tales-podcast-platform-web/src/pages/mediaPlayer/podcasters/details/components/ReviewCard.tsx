import type { PodcasterReviewAPI } from "@/core/types/podcaster";
import { FaStar } from "react-icons/fa";

const ReviewCard = ({ review }: { review: PodcasterReviewAPI }) => {
  return (
    <div className="w-full bg-white/10 backdrop-blur-md shadow-md aspect-video flex flex-col rounded-md p-2">
      <div className="w-full flex items-start gap-1">
        <div className="flex flex-col ml-1">
          <p className="text-lg font-semibold text-white line-clamp-1">
            {review.Title === "" ? review.Title : "I love this Podcaster!"}
          </p>
          <p className="flex items-center gap-0.5 text-yellow-400 mt-2">
            {Array.from({ length: review.Rating }).map((_, index) => (
              <FaStar key={index} />
            ))}
          </p>
        </div>
      </div>
      <div className="w-full pt-5 px-1">
        {review.Content ? (
          <p>{review.Content}</p>
        ) : (
          <p className="line-clamp-4 text-white font-medium">
            Tôi thực sự khâm phục tài năng của Podcaster này, anh ta có giọng
            nói trời phú có thể khiến tôi xúc động. Mỗi tập podcast đều mang
            đến những câu chuyện sâu sắc và ý nghĩa, giúp tôi thư giãn sau những
            giờ làm việc căng thẳng. Tôi rất mong chờ các tập tiếp theo từ anh
            ấy!
          </p>
        )}
      </div>
      <div className="flex-1 flex items-end justify-end mb-2 mr-2">
        <p className="text-xs font-light italic text-[#d9d9d9]">
          {review.Account.FullName} - {review.UpdatedAt}
        </p>
      </div>
    </div>
  );
};

export default ReviewCard;
