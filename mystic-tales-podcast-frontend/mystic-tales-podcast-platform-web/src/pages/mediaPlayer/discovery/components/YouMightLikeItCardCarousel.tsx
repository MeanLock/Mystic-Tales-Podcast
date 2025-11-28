import { Card, CardContent } from "@/components/ui/card";
import type { ShowUI } from "@/core/types/show";

const YouMightLikeItCard = ({ card }: { card: ShowUI }) => {
  return (
    <Card className="bg-transparent border-none shadow-sm p-1 transition-all duration-300 ease-out hover:shadow-lg hover:-translate-y-1 cursor-pointer">
      <CardContent className="flex aspect-square items-center text-card-foreground bg-transparent justify-center p-2 rounded-lg">
        <img
          src={card.ImageUrl}
          className="w-full h-full aspect-square object-cover rounded-lg"
        />
      </CardContent>
    </Card>
  );
};

export default YouMightLikeItCard;
