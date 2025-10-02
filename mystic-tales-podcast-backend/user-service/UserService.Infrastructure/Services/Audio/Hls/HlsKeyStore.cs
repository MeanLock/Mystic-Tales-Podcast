using System.Collections.Concurrent;

namespace AudioEQUploader.Controllers;

public static class HlsKeyStore
{
    private record KeyRecord(string SessionId, string KeyPath, DateTime CreatedUtc);
    private static readonly ConcurrentDictionary<string, KeyRecord> _keys = new();

    public static string Add(string sessionId, string keyPath)
    {
        var id = Guid.NewGuid().ToString("N");
        _keys[id] = new KeyRecord(sessionId, keyPath, DateTime.UtcNow);
        return id;
    }

    public static bool TryGet(string id, out string keyPath)
    {
        keyPath = string.Empty;
        if (_keys.TryGetValue(id, out var rec))
        {
            // OPTIONAL: expire after some time
            if ((DateTime.UtcNow - rec.CreatedUtc) > TimeSpan.FromHours(6))
            {
                _keys.TryRemove(id, out _);
                return false;
            }
            keyPath = rec.KeyPath;
            return true;
        }
        return false;
    }
}
