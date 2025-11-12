# CACHE STRATEGY SUMMARY (UPDATED)

## I. SYSTEM-WIDE CACHE

### 1.1. ALL-TIME MAX VALUES
**Refresh:** Daily at 00:00

**Podcaster:**
```json
{
  "PodcasterAllTimeMaxQueryMetric": {
    "MaxTotalFollow (MTF) - all-time": 100000,
    "MaxListenCount (MLC) - all-time": 5000000,
    "MaxRatingTerm (MRT) - all-time": 4.5,
    "MaxAge (MaxAge) - all-time": 1825
  }
}
```

**Show:**
```json
{
  "ShowAllTimeMaxQueryMetric": {
    "MaxTotalFollow (MTF) - all-time": 80000,
    "MaxListenCount (MLC) - all-time": 3000000,
    "MaxRatingTerm (MRT) - all-time": 4.2
  }
}
```

**Channel:**
```json
{
  "PodcasterAllTimeMaxQueryMetric": {
    "MaxTotalListenSession (MTLS) - all-time": 4500000,
    "MaxTotalFavorite (MTFa) - all-time": 50000
  }
}
```

---

### 1.2. TEMPORAL MAX VALUES (7 days)
**Refresh:** Every 12h at 00:00, 12:00

**Podcaster:**
```json
{
  "PodcasterTemporal7dMaxQueryMetric": {
    "MaxNewListenSession (MNLS) - 7d": 50000,
    "MaxNewFollow (MNF) - 7d": 10000,
    "MaxGrowth (MG) - 7d": 137.5,
    "MaxTotalListenSession (MTLS) - 7d": 45000
  }
}
```
**MIN RECORD REQUIRED:** 5 recorded podcasters

**Show:**
```json
{
  "ShowTemporal7dMaxQueryMetric": {
    "MaxNewListenSession (MNLS) - 7d": 40000,
    "MaxNewFollow (MNF) - 7d": 8000
  }
}
```
**MIN RECORD REQUIRED:** 5 recorded shows

**Channel:**
```json
{
  "ChannelTemporal7dMaxQueryMetric": {
    "MaxNewListenSession (MNLS) - 7d": 35000,
    "MaxNewFavorite (MNFa) - 7d": 5000
  }
}
```
**MIN RECORD REQUIRED:** 5 recorded channels

---

### 1.3. SYSTEM PREFERENCES (30 days)
**Refresh:** Daily at 00:00

```json
{
  "SystemPreferencesTemporal30dQueryMetric": {
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
          },
          {
            "PodcastSubCategoryId": 5,
            "ListenCount": 43000
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
    "last_updated": "2025-11-08T00:00:00Z"
  }
}
```
**MIN RECORD REQUIRED:** 4 categories, 2 podcasters

---

## II. USER-SPECIFIC CACHE

### 2.1. USER PREFERENCES (30 days)
**Refresh:** Daily at 00:00
**TTL:** 7 days

```json
{
  "UserPreferencesTemporal30dQueryMetric": {
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
      },
      {
        "PodcastCategoryId": 3,
        "PodcastSubCategoryIds": [
          {
            "PodcastSubCategoryId": 8,
            "ListenCount": 320
          }
        ],
        "ListenCount": 320
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
    "last_updated": "2025-11-08T00:00:00Z"
  }
}
```
**MIN RECORD REQUIRED:** 4 categories, 2 podcasters

---

## III. CACHE KEY NAMING CONVENTION

### System-Wide:
```
PodcasterAllTimeMaxQueryMetric
PodcasterTemporal7dMaxQueryMetric
ShowAllTimeMaxQueryMetric
ShowTemporal7dMaxQueryMetric
PodcasterAllTimeMaxQueryMetric
ChannelTemporal7dMaxQueryMetric
SystemPreferencesTemporal30dQueryMetric
```

### User-Specific:
```
UserPreferencesTemporal30dQueryMetric
```

---

## IV. BACKGROUND JOB SCHEDULE

```
00:00 Daily:
  - Update all-time max values (all entities)
  - Update temporal max values (all entities)
  - Update SystemPreferencesTemporal30dQueryMetric
  - Update UserPreferencesTemporal30dQueryMetric (all active users)
  - Cleanup expired user preferences (TTL expired)

12:00 Daily:
  - Update temporal max values (all entities)
```

---

## V. CACHE STORAGE (Redis)

**Hash Structure:**
```
├─ PodcasterAllTimeMaxQueryMetric (Hash)
├─ PodcasterTemporal7dMaxQueryMetric (Hash)
├─ ShowAllTimeMaxQueryMetric (Hash)
├─ ShowTemporal7dMaxQueryMetric (Hash)
├─ PodcasterAllTimeMaxQueryMetric (Hash)
├─ ChannelTemporal7dMaxQueryMetric (Hash)
├─ SystemPreferencesTemporal30dQueryMetric (JSON String)
└─ UserPreferencesTemporal30dQueryMetric (JSON String, TTL: 7d)
```

---

## VI. MIN RECORD REQUIRED BEHAVIOR

**All-time Max Values:**
- No minimum required
- Always return value (even if 0)

**Temporal Max Values (7d):**
- MIN: 5 recorded entities
- If < 5: Return empty (null)
- Reason: Prevent skewed data với sample size nhỏ

**System Preferences:**
- MIN: 4 categories, 2 podcasters
- If < min: Return empty (null)
- Reason: Ensure quality fallback data

**User Preferences:**
- MIN: 4 categories, 2 podcasters
- If < min: Return empty (null)
- Reason: Require sufficient listening history

---

## VII. TIME RANGE CONSTANTS

```
minShortRangeUserBehaviorLookbackDayCount: 2 days
minMediumRangeUserBehaviorLookbackDayCount: 7 days
minLongRangeUserBehaviorLookbackDayCount: 30 days
minShortRangeContentBehaviorLookbackDayCount: 2 days
minMediumRangeContentBehaviorLookbackDayCount: 7 days
minLongRangeContentBehaviorLookbackDayCount: 30 days
minExtraLongRangeContentBehaviorLookbackDayCount: 90 days
```

---

## VIII. METRIC ABBREVIATIONS

```
TF    = Total Follow
MTF   = Max Total Follow
LC    = Listen Count
MLC   = Max Listen Count
RT    = Rating Term
MRT   = Max Rating Term
NLS   = New Listen Session
MNLS  = Max New Listen Session
NF    = New Follow
MNF   = Max New Follow
NFa   = New Favorite
MNFa  = Max New Favorite
G     = Growth
MG    = Max Growth
TLS   = Total Listen Session
MTLS  = Max Total Listen Session
TFa   = Total Favorite
MTFa  = Max Total Favorite
```
