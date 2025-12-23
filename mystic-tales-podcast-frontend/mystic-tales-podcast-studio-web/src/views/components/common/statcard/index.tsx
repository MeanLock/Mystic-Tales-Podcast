import { cn } from "@/lib/utils";
import './styles.scss';
interface StatCardProps {
  icon: React.ReactNode;
  label: string;
  value: string | number;
  className?: string;
  iconColor?: "primary" | "info" | "warning";
}

const iconColorClasses = {
  primary: "text-primary ",
  info: "text-muted-foreground ",
  warning: "text-yellow-400 ",
};

export function StatCard({ icon, label, value, className, iconColor = "primary" }: StatCardProps) {
  return (
    <div
      className={cn(
        "stat-card rounded-2xl p-5 flex justify-center items-center gap-6",
        className
      )}
    >
      <div className={cn("text-4xl", iconColorClasses[iconColor])}>
        {icon}
      </div>
      <div className="flex flex-col gap-1">
        <span className="text-[11px] uppercase tracking-widest text-muted-foreground font-semibold">
          {label}
        </span>
        <span className="text-2xl font-bold text-primary tabular-nums">
          {value}
        </span>
      </div>
    </div>
  );
}
