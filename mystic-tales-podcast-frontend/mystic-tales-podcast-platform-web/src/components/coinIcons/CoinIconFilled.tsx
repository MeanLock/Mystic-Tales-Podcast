import { type SVGProps } from "react";

interface IconProps extends SVGProps<SVGSVGElement> {
  size?: number;
}

const MTPCoinFilled = ({ size = 24, ...props }: IconProps) => (
  <svg
    width={size}
    height={size}
    viewBox="0 0 24 24"
    fill="currentColor"
    {...props}
  >
    <circle cx="12" cy="12" r="9" />
    <path
      fill="#000"
      d="M9 10a3 3 0 0 1 6 0v1c0 .8-.4 1.5-1 2v1H10v-1c-.6-.5-1-1.2-1-2z"
    />
    <circle cx="10.5" cy="11" r="0.6" fill="#000" />
    <circle cx="13.5" cy="11" r="0.6" fill="#000" />
    <rect x="11" y="14" width="2" height="0.8" fill="#000" />
  </svg>
);

export default MTPCoinFilled;
