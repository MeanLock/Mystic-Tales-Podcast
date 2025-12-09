// src/components/GlobalAlert.tsx
import { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/src/store/store";
import { hideAlert } from "@/src/features/alert/alertSlice";
import { Modal, View, Text, TouchableOpacity, StyleSheet } from "react-native";

// Global action handlers registry
const actionHandlers = new Map<string, () => void>();

export const registerAlertAction = (actionId: string, handler: () => void) => {
  actionHandlers.set(actionId, handler);
};

export const unregisterAlertAction = (actionId: string) => {
  actionHandlers.delete(actionId);
};

export const GlobalAlert = () => {
  const dispatch = useDispatch();
  const {
    visible,
    title,
    description,
    type,
    isCloseable,
    isFunctional,
    functionalButtonText,
    autoCloseDuration,
    actionId,
  } = useSelector((state: RootState) => state.alert);

  useEffect(() => {
    if (!visible || !autoCloseDuration || autoCloseDuration <= 0) return;

    const timer = setTimeout(() => {
      dispatch(hideAlert());
    }, autoCloseDuration * 1000);

    return () => clearTimeout(timer);
  }, [visible, autoCloseDuration, dispatch]);

  const handleClose = () => {
    if (isCloseable) {
      dispatch(hideAlert());
    }
  };

  const handleFunctionalAction = () => {
    if (actionId && actionHandlers.has(actionId)) {
      const handler = actionHandlers.get(actionId);
      if (handler) {
        handler();
      }
    }
    dispatch(hideAlert());
  };

  if (!visible) return null;

  const getIcon = () => {
    switch (type) {
      case "success":
        return "✓";
      case "error":
        return "✕";
      case "warning":
        return "!";
      case "info":
        return "i";
      default:
        return "i";
    }
  };

  const getTextStyle = () => {
    switch (type) {
      case "success":
        return styles.titleSuccess;
      case "error":
        return styles.titleError;
      case "warning":
        return styles.titleWarning;
      case "info":
        return styles.titleInfo;
      default:
        return styles.titleInfo;
    }
  };

  const getIconStyle = () => {
    switch (type) {
      case "success":
        return styles.iconSuccess;
      case "error":
        return styles.iconError;
      case "warning":
        return styles.iconWarning;
      case "info":
        return styles.iconInfo;
      default:
        return styles.iconInfo;
    }
  };

  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      onRequestClose={handleClose}
    >
      <View style={styles.overlay}>
        <View style={styles.card}>
          <View className="flex flex-col items-center">
            <View className="w-full flex flex-row items-center justify-end">
              {isCloseable && (
                <TouchableOpacity
                  onPress={handleClose}
                  style={styles.closeBtn}
                  activeOpacity={0.7}
                >
                  <Text style={styles.closeText}>✕</Text>
                </TouchableOpacity>
              )}
            </View>
            <View style={[styles.iconBadge, getIconStyle()]}>
              <Text style={styles.icon}>{getIcon()}</Text>
            </View>

            <View className="w-full flex items-center justify-center py-2">
              <Text style={getTextStyle()} className="text-lg font-bold">
                {title}
              </Text>
            </View>
          </View>

          <View className="w-full flex items-center justify-center">
            <Text numberOfLines={1} style={styles.message}>
              {description}
            </Text>
          </View>

          {isFunctional && (
            <TouchableOpacity
              style={styles.functionalBtn}
              onPress={handleFunctionalAction}
              activeOpacity={0.8}
            >
              <Text style={styles.functionalBtnText}>
                {functionalButtonText || "OK"}
              </Text>
            </TouchableOpacity>
          )}
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0, 0, 0, 0.6)",
    justifyContent: "center",
    alignItems: "center",
    backdropFilter: "blur(4px)",
  },

  card: {
    width: "82%",
    borderRadius: 20,
    paddingVertical: 20,
    paddingHorizontal: 18,
    backgroundColor: "rgba(20, 20, 25, 0.85)",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.1)",
    shadowColor: "#000",
    shadowOpacity: 0.4,
    shadowOffset: { width: 0, height: 8 },
    shadowRadius: 16,
    elevation: 8,
  },

  headerRow: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 14,
    gap: 12,
  },

  iconBadge: {
    width: 60,
    height: 60,
    borderRadius: 12,
    justifyContent: "center",
    alignItems: "center",
    borderWidth: 1.5,
    flexShrink: 0,
  },

  iconSuccess: {
    backgroundColor: "rgba(16, 185, 129, 0.15)",
    borderColor: "rgba(16, 185, 129, 0.4)",
  },

  iconError: {
    backgroundColor: "rgba(239, 68, 68, 0.15)",
    borderColor: "rgba(239, 68, 68, 0.4)",
  },

  iconWarning: {
    backgroundColor: "rgba(245, 158, 11, 0.15)",
    borderColor: "rgba(245, 158, 11, 0.4)",
  },

  iconInfo: {
    backgroundColor: "rgba(59, 130, 246, 0.15)",
    borderColor: "rgba(59, 130, 246, 0.4)",
  },

  icon: {
    fontSize: 22,
    fontWeight: "700",
    color: "#fff",
  },

  titleContainer: {
    flex: 1,
  },

  title: {
    flex: 1,
    fontSize: 16,
    fontWeight: "700",
    color: "#fff",
    letterSpacing: 0.3,
  },

  closeBtn: {
    width: 36,
    height: 36,
    borderRadius: 10,
    backgroundColor: "rgba(255, 255, 255, 0.08)",
    justifyContent: "center",
    alignItems: "center",
    borderWidth: 1,
    borderColor: "rgba(255, 255, 255, 0.12)",
  },

  closeText: {
    fontSize: 16,
    color: "rgba(255, 255, 255, 0.6)",
    fontWeight: "600",
  },

  message: {
    fontSize: 14,
    color: "rgba(255, 255, 255, 0.75)",
    lineHeight: 20,
    marginBottom: 14,
    textAlign: "center",
  },

  functionalBtn: {
    backgroundColor: "rgba(173, 227, 57, 0.25)",
    paddingVertical: 12,
    paddingHorizontal: 16,
    borderRadius: 8,
    alignItems: "center",
    marginTop: 10,
    borderWidth: 1,
    borderColor: "rgba(173, 227, 57)",
  },

  functionalBtnText: {
    color: "#fff",
    fontSize: 14,
    fontWeight: "600",
    letterSpacing: 0.3,
  },

  hintText: {
    fontSize: 12,
    color: "rgba(255, 255, 255, 0.5)",
    marginTop: 8,
    textAlign: "center",
    marginLeft: 56,
  },
  titleSuccess: {
    color: "#10b981",
  },
  titleError: {
    color: "#ef4444",
  },
  titleWarning: {
    color: "#f59e0b",
  },
  titleInfo: {
    color: "#3b82f6",
  },
});
