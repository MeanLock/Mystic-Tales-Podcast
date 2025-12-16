import "./styles.css";
const PlayingWave = () => {
  return (
    <div className="w-full h-full aspect-square flex items-center justify-center">
      <div className="flex items-center justify-center w-[16px] h-[16px]">
        <div className="loading-bar"></div>
        <div className="loading-bar"></div>
        <div className="loading-bar"></div>
        <div className="loading-bar"></div>
      </div>
    </div>
  );
};
export default PlayingWave;
