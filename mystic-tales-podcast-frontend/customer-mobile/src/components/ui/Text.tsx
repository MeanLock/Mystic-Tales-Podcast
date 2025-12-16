import React from "react";
import { Text as DefaultText, TextProps } from "react-native";
import { cn } from "@/src/utils/cn";
import { useColorScheme } from "../useColorScheme";

type AppTextProps = TextProps & {
  variant?: "title" | "body" | "caption";
  weight?: "regular" | "medium" | "bold";
  className?: string;
};

export function Text({
  variant = "body",
  weight = "regular",
  className,
  ...props
}: AppTextProps) {
  const colorScheme = useColorScheme();
  const color = colorScheme === "dark" ? "text-white" : "text-black";

  const variantClass = {
    title: "text-2xl",
    body: "text-base",
    caption: "text-sm opacity-80",
  }[variant];

  const weightClass = {
    regular: "font-normal",
    medium: "font-medium",
    bold: "font-bold",
  }[weight];

  return (
    <DefaultText
      className={cn(color, variantClass, weightClass, className)}
      {...props}
    />
  );
}
