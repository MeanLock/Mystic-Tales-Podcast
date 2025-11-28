// src/components/MixxingText.tsx
import React from "react";
import { Text, StyleSheet, TextProps } from "react-native";
import { useColorScheme } from "../useColorScheme";
import { tintColorDark, tintColorLight } from "@/src/constants/Colors";

/**
 * Hiển thị đoạn text có phần được tô màu riêng
 * @param originalText - toàn bộ chuỗi hiển thị
 * @param coloredText - phần sẽ được tô màu
 */
interface MixxingTextProps extends TextProps {
  originalText: string;
  coloredText: string;
  style?: any;
}

export const MixxingText: React.FC<MixxingTextProps> = ({
  originalText,
  coloredText,
  style,
  ...props
}) => {
  const colorScheme = useColorScheme();
  const baseColor = "#fff";
  const highlightColor = tintColorDark;

  // Tách đoạn text thành 3 phần: trước, tô màu, sau
  const parts = originalText.split(coloredText);

  // Nếu không tìm thấy coloredText → trả nguyên
  if (parts.length === 1) {
    return (
      <Text style={[styles.text, { color: baseColor }, style]} {...props}>
        {originalText}
      </Text>
    );
  }

  return (
    <Text style={[styles.text, { color: baseColor }, style]} {...props}>
      {parts[0]}
      <Text style={[styles.highlight, { color: highlightColor }]}>
        {coloredText}
      </Text>
      {parts[1]}
    </Text>
  );
};

const styles = StyleSheet.create({
  text: {
    fontSize: 18,
    fontWeight: "500",
  },
  highlight: {
    fontWeight: "700",
  },
});

export default MixxingText;
