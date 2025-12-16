import type { Config } from "tailwindcss";

const config = {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      fontFamily: {
        poppins: ["Poppins", "sans-serif"],
        fleur: ["Fleur De Leah", "cursive"],
      },
      colors: {
        "mystic-green": "#aae339",
      },
    },
  },
  plugins: [],
} satisfies Config;

export default config;
