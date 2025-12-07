// src/components/GlobalAlert.tsx
import React, { useEffect } from "react";
import { useSelector, useDispatch } from "react-redux";
import type { RootState } from "@/src/store/store";
import { hideAlert } from "@/src/features/alert/alertSlice";
import { Modal, View, Text, TouchableOpacity, StyleSheet } from "react-native";

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
    onClickAction,
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
    if (onClickAction) {
      onClickAction();
    }
    dispatch(hideAlert());
  };

  if (!visible) return null;

  const getIcon = () => {
    switch (type) {
      case "success":
        return "✅";
      case "error":
        return "⚠️";
      case "warning":
        return "⚠️";
      case "info":
        return "ℹ️";
      default:
        return "ℹ️";
    }
  };

  const getCardStyle = () => {
    switch (type) {
      case "success":
        return styles.successCard;
      case "error":
        return styles.errorCard;
      case "warning":
        return styles.warningCard;
      case "info":
        return styles.infoCard;
      default:
        return styles.infoCard;
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
        <View style={[styles.card, getCardStyle()]}>
          <View style={styles.headerRow}>
            <Text style={styles.icon}>{getIcon()}</Text>
            <Text style={styles.title}>{title}</Text>

            {isCloseable && (
              <TouchableOpacity onPress={handleClose} style={styles.closeBtn}>
                <Text style={styles.closeText}>✕</Text>
              </TouchableOpacity>
            )}
          </View>

          <Text style={styles.message}>{description}</Text>

          {isFunctional && (
            <TouchableOpacity
              style={styles.functionalBtn}
              onPress={handleFunctionalAction}
            >
              <Text style={styles.functionalBtnText}>
                {functionalButtonText || "OK"}
              </Text>
            </TouchableOpacity>
          )}

          {!autoCloseDuration && isCloseable && (
            <Text style={styles.hintText}>Tap the ✕ button to close.</Text>
          )}
        </View>
      </View>
    </Modal>
  );
};

const styles = StyleSheet.create({
  overlay: {
    flex: 1,
    backgroundColor: "rgba(0,0,0,0.35)",
    justifyContent: "center",
    alignItems: "center",
  },
  card: {
    width: "80%",
    borderRadius: 16,
    paddingVertical: 16,
    paddingHorizontal: 14,
    backgroundColor: "#fff",
    shadowColor: "#000",
    shadowOpacity: 0.2,
    shadowOffset: { width: 0, height: 4 },
    shadowRadius: 10,
    elevation: 6,
  },
  successCard: {
    borderLeftWidth: 6,
    borderLeftColor: "#10b981",
  },
  errorCard: {
    borderLeftWidth: 6,
    borderLeftColor: "#ef4444",
  },
  warningCard: {
    borderLeftWidth: 6,
    borderLeftColor: "#f59e0b",
  },
  infoCard: {
    borderLeftWidth: 6,
    borderLeftColor: "#3b82f6",
  },
  headerRow: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
  },
  icon: {
    fontSize: 24,
    marginRight: 8,
  },
  title: {
    flex: 1,
    fontSize: 18,
    fontWeight: "700",
    color: "#1f2937",
  },
  closeBtn: {
    padding: 4,
  },
  closeText: {
    fontSize: 20,
    color: "#6b7280",
  },
  message: {
    fontSize: 14,
    color: "#4b5563",
    lineHeight: 20,
    marginBottom: 12,
  },
  functionalBtn: {
    backgroundColor: "#3b82f6",
    paddingVertical: 10,
    paddingHorizontal: 16,
    borderRadius: 8,
    alignItems: "center",
    marginTop: 8,
  },
  functionalBtnText: {
    color: "#fff",
    fontSize: 14,
    fontWeight: "600",
  },
  hintText: {
    fontSize: 12,
    color: "#9ca3af",
    marginTop: 6,
    textAlign: "center",
  },
});
