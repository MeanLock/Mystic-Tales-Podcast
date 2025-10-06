import { View, Button, Text } from "react-native";

export default function Login() {
  return (
    <View style={{ padding: 24 }}>
      <Text style={{ fontSize: 18, marginBottom: 12 }}>Login screen</Text>
      <Button title="Mock Login" />
    </View>
  );
}
