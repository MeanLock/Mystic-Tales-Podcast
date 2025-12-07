// HtmlText.tsx
import React, { useMemo } from "react";
import {
  View,
  StyleSheet,
  useWindowDimensions,
  StyleProp,
  ViewStyle,
  TextStyle,
} from "react-native";
import RenderHtml, {
  defaultSystemFonts,
  MixedStyleDeclaration,
} from "react-native-render-html";

type HtmlTextProps = {
  html: string;
  maxWidth?: number;
  numberOfLines?: number;
  color?: string;
  fontSize?: number;
  lineHeight?: number;
  style?: StyleProp<ViewStyle>;
  textStyle?: StyleProp<TextStyle>;
};

const HtmlText: React.FC<HtmlTextProps> = ({
  html,
  maxWidth,
  numberOfLines,
  color = "#111827",
  fontSize = 14,
  lineHeight,
  style,
  textStyle,
}) => {
  const { width: screenWidth } = useWindowDimensions();

  const contentWidth = useMemo(() => {
    if (typeof maxWidth === "number") {
      return Math.min(maxWidth, screenWidth - 32);
    }
    return screenWidth - 32;
  }, [maxWidth, screenWidth]);

  const calculatedLineHeight = lineHeight ?? fontSize * 1.4;

  const baseStyle: TextStyle = {
    color,
    fontSize,
    lineHeight: calculatedLineHeight,
  };

  // 👉 Gộp baseStyle + textStyle và cast sang MixedStyleDeclaration
  const mergedBaseStyle = useMemo(
    () =>
      StyleSheet.flatten([baseStyle, textStyle || {}]) as MixedStyleDeclaration,
    [baseStyle, textStyle]
  );

  // Calculate max height based on numberOfLines
  const maxHeight = numberOfLines
    ? numberOfLines * calculatedLineHeight
    : undefined;

  return (
    <View
      style={[
        styles.container,
        style,
        { maxWidth: contentWidth, maxHeight, overflow: "hidden" },
      ]}
    >
      <RenderHtml
        contentWidth={contentWidth}
        source={{ html }}
        baseStyle={mergedBaseStyle} // 👈 giờ đúng type MixedStyleDeclaration
        defaultTextProps={{
          numberOfLines,
          ellipsizeMode: "tail",
        }}
        tagsStyles={{
          p: {
            marginTop: 0,
            marginBottom: 2,
          },
          h1: {
            fontSize: fontSize + 6,
            fontWeight: "700",
            marginBottom: 4,
          },
          h2: {
            fontSize: fontSize + 4,
            fontWeight: "600",
            marginBottom: 4,
          },
          li: {
            marginBottom: 2,
          },
          a: {
            color: color,
            textDecorationLine: "none",
          },
          strong: {
            fontWeight: "400",
          },
          b: {
            fontWeight: "400",
          },
          em: {
            fontStyle: "normal",
          },
          i: {
            fontStyle: "normal",
          },
          u: {
            textDecorationLine: "none",
          },
        }}
        systemFonts={[...defaultSystemFonts, "System"]}
        renderersProps={{
          a: {
            onPress: () => {}, // Disable link press
          },
        }}
      />
    </View>
  );
};

export default HtmlText;

const styles = StyleSheet.create({
  container: {},
});
