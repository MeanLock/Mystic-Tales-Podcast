import "./activityIndicatorStyles.css";

interface ActivityIndicatorProps {
  size?: number;
  color?: string;
  borderWidth?: number;
}

const ActivityIndicator = ({
  size = 15,
  color = "rgba(0, 0, 0, 1)",
  borderWidth = 2,
}: ActivityIndicatorProps) => {
  return (
    <div
      className="loaderActivityIndicator"
      style={{
        width: `${size}px`,
        height: `${size}px`,
        borderWidth: `${borderWidth}px`,
        borderColor: color,
        borderLeftColor: "transparent",
      }}
    ></div>
  );
};
export default ActivityIndicator;
