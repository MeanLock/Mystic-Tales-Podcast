# CACHE STRATEGY - DTO DEFINITIONS

## I. ALL-TIME MAX VALUES (Refresh: Daily 00:00)

### PodcasterAllTimeMaxQueryMetric
```typescript
class PodcasterAllTimeMaxQueryMetric {
  MaxTotalFollow: number;              // MTF
  MaxListenCount: number;              // MLC
  MaxRatingTerm: number;               // MRT
  MaxAge: number;                      // MaxAge (days)
  LastUpdated: DateTime;
}
```

### ShowAllTimeMaxQueryMetric
```typescript
class ShowAllTimeMaxQueryMetric {
  MaxTotalFollow: number;              // MTF
  MaxListenCount: number;              // MLC
  MaxRatingTerm: number;               // MRT
  LastUpdated: DateTime;
}
```

### ChannelAllTimeMaxQueryMetric
```typescript
class ChannelAllTimeMaxQueryMetric {
  MaxTotalListenSession: number;       // MTLS
  MaxTotalFavorite: number;            // MTF
  LastUpdated: DateTime;
}
```

---

## II. TEMPORAL MAX VALUES (Refresh: Every 12h at 00:00, 12:00)

### PodcasterTemporal7dMaxQueryMetric
```typescript
class PodcasterTemporal7dMaxQueryMetric {
  MaxNewListenSession: number;         // MNLS
  MaxNewFollow: number;                // MNF
  MaxGrowth: number;                   // MG
  MaxTotalListenSession: number;       // MTLS
  LastUpdated: DateTime;
  
  // MIN RECORD REQUIRED: 5 podcasters
  // If < 5: Return null
}
```

### ShowTemporal7dMaxQueryMetric
```typescript
class ShowTemporal7dMaxQueryMetric {
  MaxNewListenSession: number;         // MNLS
  MaxNewFollow: number;                // MNF
  LastUpdated: DateTime;
  
  // MIN RECORD REQUIRED: 5 shows
  // If < 5: Return null
}
```

### ChannelTemporal7dMaxQueryMetric
```typescript
class ChannelTemporal7dMaxQueryMetric {
  MaxNewListenSession: number;         // MNLS
  MaxNewFavorite: number;              // MNF
  LastUpdated: DateTime;
  
  // MIN RECORD REQUIRED: 5 channels
  // If < 5: Return null
}
```

---

## III. SYSTEM PREFERENCES (Refresh: Every 2 hour)

### SystemPreferencesTemporal30dQueryMetric
```typescript
class SystemPreferencesTemporal30dQueryMetric {
  ListenedPodcastCategories: ListenedPodcastCategory[];
  ListenedPodcasters: ListenedPodcaster[];
  LastUpdated: DateTime;
  
  // MIN RECORD REQUIRED: 4 categories, 2 podcasters
  // If < min: Return null
}

class ListenedPodcastCategory {
  PodcastCategoryId: number;
  PodcastSubCategoryIds: PodcastSubCategoryListenCount[];
  ListenCount: number;
}

class PodcastSubCategoryListenCount {
  PodcastSubCategoryId: number;
  ListenCount: number;
}

class ListenedPodcaster {
  PodcasterId: number;
  ListenCount: number;
}
```

---

## IV. USER PREFERENCES (Refresh: Every 2 hour, TTL: 2 hours)

### UserPreferencesTemporal30dQueryMetric
```typescript
class UserPreferencesTemporal30dQueryMetric {
  UserId: number;
  ListenedPodcastCategories: ListenedPodcastCategory[];
  ListenedPodcasters: ListenedPodcaster[];
  LastUpdated: DateTime;
  
  // MIN RECORD REQUIRED: 4 categories, 2 podcasters
  // If < min: Return null
}

class ListenedPodcastCategory {
  PodcastCategoryId: number;
  PodcastSubCategoryIds: PodcastSubCategoryListenCount[];
  ListenCount: number;
}

class PodcastSubCategoryListenCount {
  PodcastSubCategoryId: number;
  ListenCount: number;
}

class ListenedPodcaster {
  PodcasterId: number;
  ListenCount: number;
}
```

---

## V. CACHE KEY NAMING

### Redis Keys:
```
PodcasterAllTimeMaxQueryMetric
PodcasterTemporal7dMaxQueryMetric
ShowAllTimeMaxQueryMetric
ShowTemporal7dMaxQueryMetric
ChannelAllTimeMaxQueryMetric
ChannelTemporal7dMaxQueryMetric
SystemPreferencesTemporal30dQueryMetric
UserPreferencesTemporal30dQueryMetric
```

---

## VI. BACKGROUND JOB SCHEDULE

```
00:00 Daily:
  - Update PodcasterAllTimeMaxQueryMetric
  - Update ShowAllTimeMaxQueryMetric
  - Update ChannelAllTimeMaxQueryMetric
  - Update PodcasterTemporal7dMaxQueryMetric
  - Update ShowTemporal7dMaxQueryMetric
  - Update ChannelTemporal7dMaxQueryMetric


12:00 Daily:
  - Update PodcasterTemporal7dMaxQueryMetric
  - Update ShowTemporal7dMaxQueryMetric
  - Update ChannelTemporal7dMaxQueryMetric

Every 2 hours:
  - Update SystemPreferencesTemporal30dQueryMetric
  - Update UserPreferencesTemporal30dQueryMetric (active users in last 2h)
  - Cleanup expired UserPreferencesTemporal30dQueryMetric (TTL = 2 hours)
```

---

## VII. STORAGE FORMAT (Redis)

**All-Time Max Values:** Hash
```redis
HSET PodcasterAllTimeMaxQueryMetric MaxTotalFollow 100000
HSET PodcasterAllTimeMaxQueryMetric MaxListenCount 5000000
HSET PodcasterAllTimeMaxQueryMetric MaxRatingTerm 4.5
HSET PodcasterAllTimeMaxQueryMetric MaxAge 1825
HSET PodcasterAllTimeMaxQueryMetric LastUpdated "2025-11-08T00:00:00Z"
```

**Temporal Max Values:** Hash
```redis
HSET PodcasterTemporal7dMaxQueryMetric MaxNewListenSession 50000
HSET PodcasterTemporal7dMaxQueryMetric MaxNewFollow 10000
HSET PodcasterTemporal7dMaxQueryMetric MaxGrowth 137.5
HSET PodcasterTemporal7dMaxQueryMetric MaxTotalListenSession 45000
HSET PodcasterTemporal7dMaxQueryMetric LastUpdated "2025-11-08T00:00:00Z"
```

**Preferences:** JSON String
```redis
SET SystemPreferencesTemporal30dQueryMetric '{"ListenedPodcastCategories":[...],"ListenedPodcasters":[...],"LastUpdated":"2025-11-08T00:00:00Z"}'

SETEX UserPreferences_123_30d 7200 '{"UserId":123,"ListenedPodcastCategories":[...],"ListenedPodcasters":[...],"LastUpdated":"2025-11-08T00:00:00Z"}'
```

---

## VIII. TIME RANGE CONSTANTS

```typescript
const TimeRangeConfig = {
  MinShortRangeUserBehaviorLookbackDayCount: 2,
  MinMediumRangeUserBehaviorLookbackDayCount: 7,
  MinLongRangeUserBehaviorLookbackDayCount: 30,
  MinShortRangeContentBehaviorLookbackDayCount: 2,
  MinMediumRangeContentBehaviorLookbackDayCount: 7,
  MinLongRangeContentBehaviorLookbackDayCount: 30,
  MinExtraLongRangeContentBehaviorLookbackDayCount: 90
};
```

---

## IX. METRIC ABBREVIATIONS REFERENCE

```
MTF    = MaxTotalFollow
MLC    = MaxListenCount
MRT    = MaxRatingTerm
MNLS   = MaxNewListenSession
MNF    = MaxNewFollow
MNF   = MaxNewFavorite (Channel specific)
MG     = MaxGrowth
MTLS   = MaxTotalListenSession
MTF   = MaxTotalFavorite (Channel specific)
TF     = TotalFollow
LC     = ListenCount
RT     = RatingTerm
NLS    = NewListenSession
NF     = NewFollow
NF   = NewFavorite (Channel specific)
G      = Growth
TLS    = TotalListenSession
TF    = TotalFavorite (Channel specific)
```

---

## X. EXAMPLE CACHE DATA

### PodcasterAllTimeMaxQueryMetric Example:
```json
{
  "MaxTotalFollow": 100000,
  "MaxListenCount": 5000000,
  "MaxRatingTerm": 4.5,
  "MaxAge": 1825,
  "LastUpdated": "2025-11-08T00:00:00Z"
}
```

### SystemPreferencesTemporal30dQueryMetric Example:
```json
{
  "ListenedPodcastCategories": [
    {
      "PodcastCategoryId": 3,
      "PodcastSubCategoryIds": [
        {
          "PodcastSubCategoryId": 8,
          "ListenCount": 65000
        },
        {
          "PodcastSubCategoryId": 12,
          "ListenCount": 60000
        }
      ],
      "ListenCount": 125000
    },
    {
      "PodcastCategoryId": 1,
      "PodcastSubCategoryIds": [
        {
          "PodcastSubCategoryId": 2,
          "ListenCount": 55000
        }
      ],
      "ListenCount": 98000
    }
  ],
  "ListenedPodcasters": [
    {
      "PodcasterId": 123,
      "ListenCount": 45000
    },
    {
      "PodcasterId": 456,
      "ListenCount": 38000
    }
  ],
  "LastUpdated": "2025-11-08T00:00:00Z"
}
```

### UserPreferencesTemporal30dQueryMetric Example:
```json
{
  "UserId": 123,
  "ListenedPodcastCategories": [
    {
      "PodcastCategoryId": 1,
      "PodcastSubCategoryIds": [
        {
          "PodcastSubCategoryId": 2,
          "ListenCount": 280
        },
        {
          "PodcastSubCategoryId": 5,
          "ListenCount": 170
        }
      ],
      "ListenCount": 450
    }
  ],
  "ListenedPodcasters": [
    {
      "PodcasterId": 456,
      "ListenCount": 120
    },
    {
      "PodcasterId": 789,
      "ListenCount": 95
    }
  ],
  "LastUpdated": "2025-11-08T00:00:00Z"
}
```
