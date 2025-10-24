import { useState } from "react";
import { useLoginMutation } from "@/core/services/auth/auth.service";

const TestAPILoginPage = () => {
  const [email, setEmail] = useState("haohnse181525@fpt.edu.vn");
  const [password, setPassword] = useState("123");
  const [login, { isLoading, error, data }] = useLoginMutation();

  const handleLogin = async () => {
    try {
      const result = await login({
        ManualLoginInfo: {
          Email: email,
          Password: password,
        },
      }).unwrap();

      console.log("✅ Login Success:", result);
    } catch (err) {
      console.error("❌ Login Error:", err);
    }
  };

  return (
    <div className="min-h-screen bg-gray-900 text-white p-8">
      <div className="max-w-2xl mx-auto">
        <h1 className="text-3xl font-bold mb-8">Test API Login</h1>

        <div className="bg-gray-800 p-6 rounded-lg space-y-4">
          <div>
            <label className="block text-sm font-medium mb-2">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-mystic-green"
              placeholder="Enter email"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-2">Password</label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-4 py-2 bg-gray-700 border border-gray-600 rounded-lg focus:outline-none focus:ring-2 focus:ring-mystic-green"
              placeholder="Enter password"
            />
          </div>

          <button
            onClick={handleLogin}
            disabled={isLoading}
            className="w-full px-6 py-3 bg-mystic-green text-black font-semibold rounded-lg hover:bg-mystic-green/90 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isLoading ? "Loading..." : "Test Login"}
          </button>
        </div>

        {/* Loading State */}
        {isLoading && (
          <div className="mt-6 bg-blue-900/50 border border-blue-500 p-4 rounded-lg">
            <p className="text-blue-300">
              ⏳ Đang gọi API và polling saga result...
            </p>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="mt-6 bg-red-900/50 border border-red-500 p-4 rounded-lg">
            <h3 className="text-lg font-semibold text-red-300 mb-2">
              ❌ Error
            </h3>
            <pre className="text-sm text-red-200 overflow-auto">
              {JSON.stringify(error, null, 2)}
            </pre>
          </div>
        )}

        {/* Success State */}
        {data && (
          <div className="mt-6 bg-green-900/50 border border-green-500 p-4 rounded-lg">
            <h3 className="text-lg font-semibold text-green-300 mb-2">
              ✅ Saga Result Success
            </h3>
            <pre className="text-sm text-green-200 overflow-auto">
              {JSON.stringify(data, null, 2)}
            </pre>
          </div>
        )}

        {/* Instructions */}
        <div className="mt-8 bg-gray-800 p-6 rounded-lg">
          <h3 className="text-lg font-semibold mb-3">📝 Hướng dẫn:</h3>
          <ul className="list-disc list-inside space-y-2 text-gray-300">
            <li>Nhập email và password</li>
            <li>Click "Test Login" để gọi API</li>
            <li>Saga sẽ được kickoff và polling tự động</li>
            <li>Kết quả từ saga-result sẽ hiển thị ở dưới</li>
            <li>Check console để xem log chi tiết</li>
          </ul>
        </div>
      </div>
    </div>
  );
};

export default TestAPILoginPage;
