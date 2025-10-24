import { Card, CardContent } from "@/components/ui/card";
import { FaStar } from "react-icons/fa";

interface CardProps {
  card: {
    Id: string;
    Name: string;
    ImageUrl: string;
    Podcaster: {
      Id: number;
      FullName: string;
    };
    Rating: number;
    RatingCount: number;
  };
}

const ShowCardWithRating = ({ card }: CardProps) => {
  return (
    <Card className="bg-transparent border-none shadow-sm p-1 transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer">
      <CardContent className="flex flex-col aspect-square items-start justify-between text-card-foreground bg-transparent p-2 rounded-lg">
        <div className="w-full h-full aspect-square mb-2 relative">
          <img
            src={card.ImageUrl}
            className="w-full h-full aspect-square object-cover rounded-lg"
          />
          <div className="flex items-center gap-1 absolute right-2 bottom-2 bg-white/30 backdrop-blur-md px-2 py-1 rounded-md">
            <p className="font-poppins font-semibold text-white text-xs md:text-sm">
              {card.Rating}
            </p>
            <FaStar color="#fff" size={15} />
            <p className="text-[8px] md:text-xs font-light text-gray-200">
              ({card.RatingCount.toLocaleString()})
            </p>
          </div>
        </div>

        <div className="w-full overflow-ellipsis">
          <p className=" text-white text-sm font-medium mb-2 truncate">
            {card.Name}
          </p>
          <p className=" text-gray-300 text-xs font-medium">
            {card.Podcaster.FullName}
          </p>
        </div>
      </CardContent>
    </Card>
  );
};

export default ShowCardWithRating;
