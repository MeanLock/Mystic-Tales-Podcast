// PlayerOrderModeToggle.tsx
import * as React from "react";
import "./TogglePlayOrderMode.css";

export type PlayerOrderMode = "Sequential" | "Random";

type PlayerOrderModeToggleProps = {
  mode: PlayerOrderMode;
  onTogglePlayerOrderMode: (nextMode: PlayerOrderMode) => void;
  classNameName?: string;
};

export const PlayerOrderModeToggle: React.FC<PlayerOrderModeToggleProps> = ({
  mode,
  onTogglePlayerOrderMode,
}) => {
  const isRandom = mode === "Random";

  const handleToggle = () => {
    const nextMode: PlayerOrderMode = isRandom ? "Sequential" : "Random";
    onTogglePlayerOrderMode(nextMode);
  };

  return (
    <div className="checkbox-wrapper-35">
      <input
        value="private"
        name="switch"
        id="switch"
        type="checkbox"
        className="switch"
        checked={isRandom}
        onChange={handleToggle}
      />
      <label htmlFor="switch">
        <span className="switch-x-text">Play Order Mode: </span>
        <span className="switch-x-toggletext">
          <span className="switch-x-unchecked">
            <span className="switch-x-hiddenlabel">Unchecked: </span>Sequential
          </span>
          <span className="switch-x-checked">
            <span className="switch-x-hiddenlabel">Checked: </span>Random
          </span>
        </span>
      </label>
    </div>
  );
};
