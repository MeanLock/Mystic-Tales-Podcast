interface CustomToastProps {
  closeToast?: () => void;
  toastProps?: any;
  title: string;
  description: string;
  autoCloseDuration?: number;
  type: "success" | "error";
}

const CustomToast = ({
  closeToast,
  title,
  description,
  type,
}: CustomToastProps) => {
  return (
    <div
      className="relative w-full min-h-[100px] rounded-lg overflow-hidden p-4 bg-cover bg-center bg-no-repeat"
      style={{
        backgroundImage: "url(/images/toast/ToastBG.png)",
      }}
    >
      {/* Overlay để text dễ đọc hơn nếu cần */}
      <div className="absolute inset-0 bg-black/30 -z-10" />

      <div className="relative z-10 text-white">
        <h3 className="font-bold text-lg mb-2">
          {type === "success" ? "✓ " : "✗ "}
          {title}
        </h3>
        <p className="text-sm opacity-90">{description}</p>

        {closeToast && (
          <button
            onClick={closeToast}
            className="absolute top-2 right-2 text-white/70 hover:text-white transition-colors"
          >
            ✕
          </button>
        )}
      </div>
    </div>
  );
};

export default CustomToast;
