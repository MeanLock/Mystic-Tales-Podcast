// ...existing code...
import { Text } from "@/src/components/ui/Text";
import { View } from "@/src/components/ui/View";
import FontAwesome from "@expo/vector-icons/FontAwesome";
import { FontAwesome5, FontAwesome6, MaterialIcons } from "@expo/vector-icons";
import { forwardRef, useRef, useState } from "react";

import {
  Alert,
  Image,
  KeyboardAvoidingView,
  Platform,
  StyleSheet,
  TextInput,
  TouchableOpacity,
  Text as RNText,
  View as RNView,
  Pressable,
  TouchableWithoutFeedback,
  Keyboard,
  ActivityIndicator,
} from "react-native";
import { useColorScheme } from "@/src/components/useColorScheme";
import { useRouter } from "expo-router";
import { tintColorDark, tintColorLight } from "@/src/constants/Colors";
import { useLoginMutation } from "@/src/core/services/auth/auth.service";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { setCredentials } from "@/src/features/auth/authSlice";
import { User } from "@/src/types/user";
import { getCapacitorDevice } from "@/src/core/utils/device";
import { showError, showSuccess } from "@/src/features/alert/alertSlice";

type IconInputProps = {
  value: string;
  onChangeText: (t: string) => void;
  placeholder?: string;
  secureTextEntry?: boolean;
  keyboardType?: "default" | "email-address" | "numeric";
  returnKeyType?: "next" | "done";
  onSubmitEditing?: () => void;
  leftIcon?: React.ReactNode;
  right?: React.ReactNode;
};

const IconInput = forwardRef<TextInput, IconInputProps>(
  (
    {
      value,
      onChangeText,
      placeholder,
      secureTextEntry,
      keyboardType,
      returnKeyType,
      onSubmitEditing,
      leftIcon,
      right,
    },
    ref
  ) => {
    const colorScheme = useColorScheme();
    return (
      <RNView
        style={[
          style.pill,
          {
            backgroundColor: colorScheme === "dark" ? "#0f0f10" : "#f4f4f4",
            borderColor: colorScheme === "dark" ? "#333" : "#ccc",
          },
        ]}
      >
        <RNView
          style={[style.leftIconWrap, { backgroundColor: "transparent" }]}
        >
          {leftIcon ?? <RNText style={style.iconText}>✉️</RNText>}
        </RNView>

        <TextInput
          ref={ref}
          value={value}
          onChangeText={onChangeText}
          placeholder={placeholder}
          placeholderTextColor="#9a9a9a"
          secureTextEntry={secureTextEntry}
          keyboardType={keyboardType}
          returnKeyType={returnKeyType}
          onSubmitEditing={onSubmitEditing}
          style={style.pillInput}
          underlineColorAndroid="transparent"
        />

        {right ? <RNView style={style.rightWrap}>{right}</RNView> : null}
      </RNView>
    );
  }
);

IconInput.displayName = "IconInput";

export default function Login() {
  // HOOKS
  const router = useRouter();
  const colorScheme = useColorScheme();
  const [login, { isLoading }] = useLoginMutation();
  const authState = useSelector((state: RootState) => state.auth);
  const dispatch = useDispatch();

  // ví dụ dùng state (thường dùng)
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);

  // ref để focus password input và ref để lưu giá trị mà không render lại
  const passwordRef = useRef<TextInput | null>(null);
  const emailValueRef = useRef<string>("");

  const onSubmit = async () => {
    const e = email || emailValueRef.current;
    if (!e) {
      Alert.alert("Lỗi", "Vui lòng nhập email");
      return;
    } else if (!password) {
      Alert.alert("Lỗi", "Vui lòng nhập password");
      return;
    } else {
      await handleLogin(e, password);
    }
  };

  const handleLogin = async (email: string, password: string) => {
    try {
      console.log("Logging in with:", email, password);

      // Lấy thông tin thiết bị
      const deviceInfo = await getCapacitorDevice();

      // Gọi API login
      const result = await login({
        ManualLoginInfo: {
          Email: email,
          Password: password,
        },
        DeviceInfo: {
          DeviceId: deviceInfo.DeviceId,
          Platform: deviceInfo.Platform,
          OSName: deviceInfo.OSName,
        },
      }).unwrap();

      if (result.isError) {
        if (result.isUnVerified) {
          dispatch(
            showError({
              message:
                "Your account is not verified. Please verify your email before logging in.",
              seconds: 10,
            })
          );
        } else {
          dispatch(
            showError({
              message: result.message || "Login failed. Please try again.",
              seconds: 10,
            })
          );
        }
        return;
      } else {
        // Navigate to home after successful login
        dispatch(showSuccess({ message: "Login successful!", seconds: 5 }));
        router.replace("/(tabs)/home");
      }
    } catch (error: any) {
      // Handle different error types
      let errorMessage = "Đăng nhập thất bại. Vui lòng thử lại.";

      if (error?.data?.error) {
        errorMessage = error.data.error;
      } else if (error?.error) {
        errorMessage = error.error;
      } else if (typeof error === "string") {
        errorMessage = error;
      }

      Alert.alert("Lỗi", errorMessage);
    }
  };

  return (
    <KeyboardAvoidingView
      behavior={Platform.OS === "ios" ? "padding" : undefined}
      style={{ flex: 1, paddingTop: 30, position: "relative" }}
    >
      <Pressable className="p-5" onPress={() => router.back()}>
        <MaterialIcons
          name="keyboard-backspace"
          size={40}
          color={colorScheme === "dark" ? "#fff" : "#929292"}
        />
      </Pressable>
      <TouchableWithoutFeedback onPress={Keyboard.dismiss} accessible={false}>
        <View variant="normal" className="p-5 gap-5">
          <View className="mb-5 py-10">
            <Text className="text-7xl font-bold">Hey,</Text>
            <Text className="text-7xl font-bold">Welcome</Text>
            <Text className="text-7xl font-bold">Back!</Text>
          </View>

          {/* Form với pill inputs */}
          <IconInput
            value={email}
            onChangeText={(t) => {
              setEmail(t);
              emailValueRef.current = t;
            }}
            placeholder="Email"
            keyboardType="email-address"
            returnKeyType="next"
            onSubmitEditing={() => passwordRef.current?.focus()}
            leftIcon={
              <FontAwesome
                name="envelope-o"
                size={18}
                color={colorScheme === "dark" ? "#fff" : "#929292"}
              />
            }
          />

          <IconInput
            ref={passwordRef}
            value={password}
            onChangeText={setPassword}
            placeholder="Password"
            secureTextEntry={!showPassword}
            returnKeyType="done"
            onSubmitEditing={onSubmit}
            leftIcon={
              <FontAwesome6
                name="lock"
                size={18}
                color={colorScheme === "dark" ? "#fff" : "#929292"}
              />
            }
            right={
              <Pressable
                onPress={() => setShowPassword((s) => !s)}
                style={style.showBtn}
              >
                {showPassword ? (
                  <FontAwesome5
                    solid={false}
                    name="eye-slash"
                    size={15}
                    color={colorScheme === "dark" ? "#fff" : "#929292"}
                  />
                ) : (
                  <FontAwesome5
                    solid={false}
                    name="eye"
                    size={15}
                    color={colorScheme === "dark" ? "#fff" : "#929292"}
                  />
                )}
              </Pressable>
            }
          />

          <TouchableOpacity
            style={[style.submit, isLoading && { opacity: 0.7 }]}
            onPress={onSubmit}
            disabled={isLoading}
          >
            {isLoading ? (
              <ActivityIndicator color="#000" />
            ) : (
              <Text style={style.submitText}>Login</Text>
            )}
          </TouchableOpacity>

          <View className="w-full items-center justify-center">
            <RNText className="text-gray-500">or continue with</RNText>
          </View>

          <TouchableOpacity style={style.goggleLogin} onPress={onSubmit}>
            <FontAwesome5 name="google" size={20} />
            <RNText className="font-bold text-xl">Google</RNText>
          </TouchableOpacity>
        </View>
      </TouchableWithoutFeedback>
      <View className="w-full absolute bottom-20 flex-row justify-center items-center">
        <RNText className="text-gray-500">Don't have an account? </RNText>
        <TouchableOpacity onPress={() => router.push("/(auth)/streamTest")}>
          <RNText
            style={{
              color: colorScheme === "dark" ? tintColorDark : tintColorLight,
            }}
            className="font-bold"
          >
            Test Stream
          </RNText>
        </TouchableOpacity>
      </View>
    </KeyboardAvoidingView>
  );
}

const style = StyleSheet.create({
  logo: {
    height: 150,
  },
  // pill input
  pill: {
    height: 60,
    flexDirection: "row",
    alignItems: "center",
    borderWidth: 1,
    borderRadius: 28,
    paddingHorizontal: 12,
    paddingVertical: 5,
    marginTop: 8,
  },
  leftIconWrap: {
    width: 40,
    height: 40,
    borderRadius: 20,
    alignItems: "center",
    justifyContent: "center",
    marginRight: 10,
  },
  iconText: {
    fontSize: 16,
  },
  pillInput: {
    flex: 1,
    color: "#fff",
    fontSize: 16,
    paddingVertical: 0,
  },
  rightWrap: {
    marginLeft: 8,
  },
  showBtn: {
    paddingHorizontal: 8,
    paddingVertical: 4,
    justifyContent: "center",
  },
  showBtnText: {
    color: "#6ea8fe",
    fontWeight: "600",
  },

  label: {
    color: "#cfcfcf",
    fontSize: 13,
    marginTop: 14,
    marginBottom: 6,
  },

  submit: {
    marginTop: 24,
    backgroundColor: "#fff",
    paddingVertical: 15,
    borderRadius: 50,
    alignItems: "center",
  },
  submitText: {
    color: "#000",
    fontWeight: "800",
  },
  goggleLogin: {
    flexDirection: "row",
    backgroundColor: "#fff",
    paddingVertical: 15,
    borderRadius: 50,
    alignItems: "center",
    justifyContent: "center",
    gap: 10,
  },
});
// ...existing code...
