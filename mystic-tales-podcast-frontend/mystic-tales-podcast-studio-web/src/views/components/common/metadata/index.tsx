import { cn } from "@/lib/utils";

interface MetadataFieldProps {
  label: string;
  value: string;
  className?: string;
}

export function MetadataField({ label, value, className }: MetadataFieldProps) {
  return (
    <div className={cn("flex flex-col gap-1 min-w-[100px]", className)}>
      <span className="text-[10px] uppercase tracking-wider text-muted-foreground font-medium">
        {label}
      </span>
      <span className="text-sm text-[#d9d9d9] font-medium">
        {value}
      </span>
    </div>
  );
}