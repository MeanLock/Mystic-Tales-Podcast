import type { PodcastBookingTone } from "@/core/types/booking";

type TonePillProps = {
  bookingTone: PodcastBookingTone;
  onSelect: (tone: PodcastBookingTone) => void;
  isSelected?: boolean;
};

const themes = [
  // 1. Soft Lavender
  {
    bg: "bg-[linear-gradient(120deg,_rgba(129,140,248,0.18),_rgba(236,72,153,0.16),_rgba(248,250,252,0.2))]",
    text: "text-white",
  },

  // 2. Mint Breeze
  {
    bg: "bg-[linear-gradient(130deg,_rgba(45,212,191,0.16),_rgba(56,189,248,0.18),_rgba(224,242,254,0.25))]",
    text: "text-white",
  },

  // 3. Peach Sorbet
  {
    bg: "bg-[linear-gradient(135deg,_rgba(254,215,170,0.22),_rgba(253,186,116,0.2),_rgba(251,113,133,0.16))]",
    text: "text-white",
  },

  // 4. Cloudy Rose
  {
    bg: "bg-[linear-gradient(135deg,_rgba(248,250,252,0.3),_rgba(254,202,202,0.22),_rgba(244,114,182,0.18))]",
    text: "text-white",
  },

  // 5. Blue Mist
  {
    bg: "bg-[linear-gradient(125deg,_rgba(219,234,254,0.28),_rgba(129,140,248,0.18),_rgba(15,23,42,0.12))]",
    text: "text-white",
  },

  // 6. Matcha Cream
  {
    bg: "bg-[linear-gradient(140deg,_rgba(190,242,100,0.22),_rgba(74,222,128,0.18),_rgba(240,253,244,0.28))]",
    text: "text-white",
  },

  // 7. Blush & Sky
  {
    bg: "bg-[linear-gradient(130deg,_rgba(254,226,226,0.26),_rgba(191,219,254,0.24),_rgba(221,214,254,0.2))]",
    text: "text-white",
  },

  // 8. Subtle Noir (nhẹ nhàng cho nền tối)
  {
    bg: "bg-[linear-gradient(135deg,_rgba(15,23,42,0.65),_rgba(30,64,175,0.45),_rgba(15,23,42,0.6))]",
    text: "text-white",
  },
];

// Hash đơn giản từ string → số nguyên, để chọn theme
function hashStringToIndex(str: string, modulo: number): number {
  let hash = 0;
  for (let i = 0; i < str.length; i += 1) {
    hash = (hash << 5) - hash + str.charCodeAt(i);
    hash |= 0; // convert to 32-bit int
  }
  return Math.abs(hash) % modulo;
}

function getThemeForTone(bookingTone: PodcastBookingTone) {
  // tuỳ schema của bạn, có thể concat thêm Id, Type... cho đa dạng
  const key =
    (bookingTone.Name ?? "") +
    String((bookingTone as any).Id ?? "") +
    String(bookingTone.AvailablePodcasterCount ?? "");
  const index = hashStringToIndex(key, themes.length);
  return themes[index];
}

function TonePill({
  bookingTone,
  onSelect,
  isSelected = false,
}: TonePillProps) {
  const theme = getThemeForTone(bookingTone);

  return (
    <div
      onClick={() => onSelect(bookingTone)}
      className={`flex h-12.5 shadow-sm items-center rounded-full p-2 gap-5 ${
        theme.bg
      } cursor-pointer transition-all duration-500 hover:-translate-y-0.5 ${
        isSelected ? "ring-2 ring-mystic-green bg-white/10" : ""
      }`}
    >
      <div className="h-full aspect-square bg-white rounded-full flex items-center justify-center">
        {bookingTone.AvailablePodcasterCount < 100 ? (
          <p className="font-poppins font-bold text-black text-xs line-clamp-1">
            {bookingTone.AvailablePodcasterCount.toLocaleString()}
          </p>
        ) : (
          <p className="font-poppins font-bold text-black text-[8px] line-clamp-1">
            100+
          </p>
        )}
      </div>
      <p className={`text-xs font-bold text-center ${theme.text}`}>
        {bookingTone.Name}
      </p>
    </div>
  );
}

export default TonePill;
