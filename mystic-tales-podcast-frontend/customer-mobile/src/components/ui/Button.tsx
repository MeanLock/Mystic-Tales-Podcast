import { View } from "react-native";

interface Props {
  title: string;
}

const Button: React.FC<Props> = ({ title }) => {
  return (
    <View className="bg-[#AEE339] px-3 py-2 rounded-md text-black font-bold">
      {title}
    </View>
  );
};

export default Button;
