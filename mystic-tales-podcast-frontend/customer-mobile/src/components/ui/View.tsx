// src/components/View.tsx
import React from "react";
import {
  View as DefaultView,
  ViewProps,
  StyleSheet,
  Platform,
  ViewStyle,
} from "react-native";
import { BlurView } from "expo-blur";
import { useColorScheme } from "../useColorScheme"; // hoặc "@/src/components/useColorScheme"
import { cn } from "@/src/utils/cn";

type CustomViewProps = ViewProps & {
  variant?: "transparent" | "glassmophorism" | "glassmorphism" | "normal";
  className?: string;

  /** Options cho glass */
  radius?: number; // bo góc
  padding?: number;
  blurIntensity?: number; // 0..100 → càng cao càng “nhòe”
  overlayOpacity?: number; // độ đục lớp phủ trắng
  borderOpacity?: number; // độ đậm viền
  gradientOverlay?: boolean; // thêm gradient sáng nhẹ ở mép trên
};

export function View({
  variant = "transparent",
  className,
  radius = 0,
  padding = 16,
  blurIntensity = 80,
  overlayOpacity, // nếu không truyền sẽ auto theo theme
  borderOpacity, // nếu không truyền sẽ auto theo theme
  gradientOverlay = true,
  style,
  children,
  ...props
}: CustomViewProps) {
  const colorScheme = useColorScheme();
  const isDark = colorScheme === "dark";
  const isGlass = variant === "glassmophorism" || variant === "glassmorphism";

  // Preset theo theme cho cảm giác “glass” đúng chất
  const _overlayOpacity = overlayOpacity ?? (isDark ? 0.1 : 0.18); // dark mờ nhẹ, light đậm hơn chút
  const _borderOpacity = borderOpacity ?? (isDark ? 0.25 : 0.12); // viền sáng ở dark, viền tối nhẹ ở light

  if (variant === "transparent") {
    return (
      <DefaultView
        {...props}
        className={cn("bg-transparent", className)}
        style={style}
      >
        {children}
      </DefaultView>
    );
  }

  if (isGlass) {
    // Container để clip blur theo bo góc
    return (
      <DefaultView
        {...props}
        className={cn(className)}
        style={[
          style as ViewStyle,
          {
            borderRadius: radius,
            padding: padding,
            overflow: "hidden",
            // thêm shadow nhẹ để glass nổi khối
            ...(Platform.OS === "ios"
              ? {
                  shadowColor: "#000",
                  shadowOpacity: 0.18,
                  shadowRadius: 12,
                  shadowOffset: { width: 0, height: 8 },
                }
              : { elevation: 6 }),
          },
        ]}
      >
        {/* BLUR nền phía sau */}
        <BlurView
          intensity={blurIntensity} // 👈 tăng/giảm để chỉnh độ nhòe
          tint={isDark ? "dark" : "light"}
          style={StyleSheet.absoluteFill}
        />

        {/* LỚP PHỦ trắng (frost) */}
        <DefaultView
          pointerEvents="none"
          style={[
            StyleSheet.absoluteFill,
            {
              backgroundColor: `rgba(255,255,255,${_overlayOpacity})`,
              // viền highlight để giống kính
              borderColor: isDark
                ? `rgba(255,255,255,${_borderOpacity})`
                : `rgba(0,0,0,${_borderOpacity})`,
              borderWidth: StyleSheet.hairlineWidth,
            },
          ]}
        />

        {/* (tuỳ chọn) GRADIENT highlight ở đỉnh kính */}
        {gradientOverlay && (
          <DefaultView
            pointerEvents="none"
            style={[
              StyleSheet.absoluteFill,
              {
                backgroundColor: isDark
                  ? "rgba(255,255,255,0.06)"
                  : "rgba(255,255,255,0.08)",
                opacity: 0.0,
              },
            ]}
          />
        )}

        {/* CONTENT */}
        {children}
      </DefaultView>
    );
  }

  // NORMAL: dùng className/style là chính
  return (
    <DefaultView {...props} className={cn(className)} style={style}>
      {children}
    </DefaultView>
  );
}
