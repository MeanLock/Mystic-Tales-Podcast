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
} from "react-native";
import { useColorScheme } from "@/src/components/useColorScheme";
import { useRouter } from "expo-router";
import { tintColorDark, tintColorLight } from "@/src/constants/Colors";
import { useLoginMutation } from "@/src/services/auth/authApi";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "@/src/store/store";
import { setCredentials } from "@/src/features/auth/authSlice";

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
  const [login] = useLoginMutation();
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
    } else {
      // Sau khi có API thì bỏ vào đây
      // await handleLogin(e, password);
      handleLoginDump();
    }
  };

  const handleLogin = async (email: string, password: string) => {
    try {
      // Gọi API login ở đây
      console.log("Logging in with:", email, password);
      const result = await login({
        LoginInfo: { Email: email, Password: password },
      }).unwrap();
      console.log("Login successful:", result);
    } catch (error) {
      Alert.alert("Lỗi", "Đăng nhập thất bại. Vui lòng thử lại.");
    }
  };

  const handleLoginDump = () => {
    // Chức năng đăng nhập giả lập (bỏ qua API)
    console.log("Logging in with (dump):", email, password);
    dispatch(
      setCredentials({
        accessToken: "dummy_access_token",
        user: {
          Id: 1,
          Email: "customer1@example.com",
          Role: {
            Id: 2,
            Name: "Customer",
          },
          Fullname: "Hoàng Minh Lộc",
          Dob: "2004-10-03",
          Gender: "male",
          Address: "S5.01B Vinhomes Grand Park, Quận 9, TP. HCM",
          Phone: "0896893636",
          Balance: 100000,
          MainImageFileKey:
            "https://i.pinimg.com/736x/84/6c/b5/846cb5c8b99cb86fdd052c661d0af33f.jpg",
          IsVerified: true,
          GoogleId: null,
          PodcastListenSlot: 100,
          ViolationPoint: 0,
          ViolationLevel: 0,
          LastViolationPointChanged: "2025-10-07T04:53:50.673Z",
          LastViolationLevelChanged: "2025-10-07T04:53:50.673Z",
          LastPodcastListenSlotChanged: "2025-10-07T04:53:50.673Z",
          DeactivatedAt: null,
          CreatedAt: "2025-10-07T04:53:50.673Z",
          UpdatedAt: "2025-10-07T04:53:50.673Z",
          IsBeingPunish: false,
        },
      })
    );
    router.replace("/(tabs)/home");
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

          <TouchableOpacity style={style.submit} onPress={onSubmit}>
            <Text style={style.submitText}>Login</Text>
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
        <TouchableOpacity onPress={() => router.push("/(auth)/register")}>
          <RNText
            style={{
              color: colorScheme === "dark" ? tintColorDark : tintColorLight,
            }}
            className="font-bold"
          >
            Register
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
