import React, { useEffect } from "react";
import {
  Modal,
  View,
  Text,
  TouchableOpacity,
  StyleSheet,
} from "react-native";

type BaseAlertProps = {
  visible: boolean;
  message: string;
  seconds?: number; // thời gian auto-close (giây). Không truyền = không auto-close
  onClose: () => void;
};

type Variant = "success" | "error";

function BaseAlert({ visible, message, seconds, onClose, variant }: BaseAlertProps & { variant: Variant }) {
  useEffect(() => {
    if (!visible || !seconds || seconds <= 0) return;

    const timer = setTimeout(() => {
      onClose();
    }, seconds * 1000);

    return () => clearTimeout(timer);
  }, [visible, seconds, onClose]);

  const isSuccess = variant === "success";

  return (
    <Modal
      visible={visible}
      transparent
      animationType="fade"
      onRequestClose={onClose}
    >
      <View style={styles.overlay}>
        <View
          style={[
            styles.card,
            isSuccess ? styles.successCard : styles.errorCard,
          ]}
        >
          <View style={styles.headerRow}>
            <Text style={styles.icon}>
              {isSuccess ? "✅" : "⚠️"}
            </Text>
            <Text style={styles.title}>
              {isSuccess ? "Success" : "Error"}
            </Text>

            <TouchableOpacity onPress={onClose} style={styles.closeBtn}>
              <Text style={styles.closeText}>✕</Text>
            </TouchableOpacity>
          </View>

          <Text style={styles.message}>{message}</Text>

          {/* Nếu không có seconds thì gợi ý user bấm đóng */}
          {!seconds || seconds <= 0 ? (
            <Text style={styles.hintText}>
              Tap the ✕ button to close.
            </Text>
          ) : null}
        </View>
      </View>
    </Modal>
  );
}

export function SuccessAlert(props: BaseAlertProps) {
  return <BaseAlert {...props} variant="success" />;
}

export function ErrorAlert(props: BaseAlertProps) {
  return <BaseAlert {...props} variant="error" />;
}

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
    borderLeftWidth: 4,
    borderLeftColor: "#16a34a",
  },
  errorCard: {
    borderLeftWidth: 4,
    borderLeftColor: "#ef4444",
  },
  headerRow: {
    flexDirection: "row",
    alignItems: "center",
    marginBottom: 8,
  },
  icon: {
    fontSize: 20,
    marginRight: 6,
  },
  title: {
    fontSize: 16,
    fontWeight: "600",
    flex: 1,
  },
  closeBtn: {
    paddingHorizontal: 6,
    paddingVertical: 2,
  },
  closeText: {
    fontSize: 16,
    color: "#6b7280",
  },
  message: {
    fontSize: 14,
    color: "#374151",
    marginBottom: 4,
  },
  hintText: {
    fontSize: 12,
    color: "#9ca3af",
    marginTop: 4,
  },
});
