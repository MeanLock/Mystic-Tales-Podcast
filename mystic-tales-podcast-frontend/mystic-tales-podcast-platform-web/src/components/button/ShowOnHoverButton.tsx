import React, { useRef, useLayoutEffect, useState } from "react";
import type { IconType } from "react-icons";
import "./ShowOnHoverButton.css";

type ShowOnHoverButtonProps = {
  text: string;
  Icon: IconType;
  bgColor?: string;
  onClick?: () => void;
};

const ShowOnHoverButton: React.FC<ShowOnHoverButtonProps> = ({
  text,
  Icon,
  bgColor = "rgb(255, 65, 65)",
  onClick,
}) => {
  const textRef = useRef<HTMLDivElement>(null);
  const [textWidth, setTextWidth] = useState(0);

  useLayoutEffect(() => {
    if (textRef.current) {
      setTextWidth(textRef.current.scrollWidth);
    }
  }, [text]);

  return (
    <button
      onClick={onClick}
      className="show-on-hover-btn shadow-2xl"
      style={{
        ["--btn-bg" as any]: bgColor,
        ["--text-width" as any]: `${textWidth}px`,
      }}
    >
      <div className="show-on-hover-btn__icon">
        <Icon />
      </div>

      <div ref={textRef} className="show-on-hover-btn__text">
        {text}
      </div>
    </button>
  );
};

export default ShowOnHoverButton;
