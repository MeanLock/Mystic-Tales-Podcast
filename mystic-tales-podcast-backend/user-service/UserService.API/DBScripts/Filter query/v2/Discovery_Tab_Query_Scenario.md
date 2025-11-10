# DISCOVERY TAB - QUERY SCENARIO (UPDATED)

## 8 DISCOVERY SECTIONS

### 1. CONTINUE LISTENING 🎧

**Target:** ≤10 episodes

**Query Logic:**
```
Query: continue
Filter: isCompleted = 0 AND currentPosition < audioLength
Sort: lastListenAt DESC
Limit: 10 episodes
```

**Cache:** Realtime (no cache)

**Notes:** Không check duplicate (entity khác: episodes)

---

### 2. BASED ON YOUR TASTE 🎵

**Target:** 12 shows

**Part A (75%) - Top Categories:**
- Source: `query:metric:user_preferences:temporal_30d:{userId}.ListenedPodcastCategories`
- Target: 9 shows
- Fetch: 3 shows (random) × up to 4 categories = 12
- Logic: Lấy 2-4 categories user nghe nhiều nhất (30d), mỗi category random 3 shows → top 9

**Part B (25%) - Top Podcasters:**
- Source: `query:metric:user_preferences:temporal_30d:{userId}.ListenedPodcasters`
- Target: 3 shows
- Fetch: 2 shows (random, không trùng Part A) × up to 2 podcasters = 4
- Logic: Lấy top 2 podcasters user nghe nhiều nhất (30d), mỗi podcaster random 2 shows → top 3

**Part C (0%) - Fallback Categories:**
- Source: `query:metric:system_preferences:temporal_30d.ListenedPodcastCategories`
- Target: n (số lượng Part A thiếu)
- Fetch: 3 shows (random, không trùng A+B) × up to 4 categories = 12
- Logic: Trám vào Part A nếu thiếu

**Part D (0%) - Fallback Podcasters:**
- Source: `query:metric:system_preferences:temporal_30d.ListenedPodcasters`
- Target: n (số lượng Part B thiếu)
- Fetch: 2 shows (random, không trùng A+B+C) × up to 2 podcasters = 4
- Logic: Trám vào Part B nếu thiếu

**Cache Key:** `query:metric:user_preferences:temporal_30d:{userId}` & `query:metric:system_preferences:temporal_30d`
**Refresh:** Every 2 hours (CronExpression: `0 0 */2 * * *`)
**TTL:** 2 hours (7200s)

---

### 3. NEW RELEASES 🆕

**Target:** 10 shows

**Part A (50%) - From Interested Podcasters:**
- Source: `query:metric:user_preferences:temporal_30d:{userId}.ListenedPodcasters`
- Target: 5 shows
- Fetch: Top 7 shows × top 2 podcasters = 14 (published trong 2 days)
- Logic: Lấy top 2 podcasters user nghe nhiều (30d) → shows mới (2d) → sort publishedAt DESC → top 5

**Part B (50%) - System New:**
- Source: System-wide
- Target: 5 shows
- Fetch: 10 shows (không trùng Part A, published trong 2 days)
- Logic: Shows mới (2d) → sort publishedAt DESC → top 10 (lấy 5 + trám Part A)

**Cache Key:** `query:metric:user_preferences:temporal_30d:{userId}`
**Refresh:** Every 2 hours (phụ thuộc vào user preferences cache)
**TTL:** 2 hours (7200s)

---

### 4. HOT THIS WEEK 🔥

**Target:** 15 items (10 shows + 5 channels)

**Part A (80%) - Hot (7 days):**
- Target: 12 items (8 shows + 4 channels)
- Fetch: Top 8 hotShows + top 4 hotChannels
- Cache: Uses `query:metric:show:temporal_7d_max` & `query:metric:channel:temporal_7d_max`
- Formula: `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)` for shows
           `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)` for channels
- Logic: Sort hotScore DESC → top 8 shows + top 4 channels

**Part B (20%) - Popular (all-time):**
- Target: 3 items (2 shows + 1 channel)
- Fetch: Top 2 popularShows + top 1 popularChannel
- Cache: Uses `query:metric:show:all_time_max` & `query:metric:channel:all_time_max`
- Formula: `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)` for shows
           `popularScore = 0.6×(TLS/MTLS) + 0.4×(TF/MTF)` for channels
- Logic: Sort popularScore DESC → top 2 shows + top 1 channel

**Cache Keys:** 
- Temporal: `query:metric:show:temporal_7d_max`, `query:metric:channel:temporal_7d_max`
- AllTime: `query:metric:show:all_time_max`, `query:metric:channel:all_time_max`
**Refresh:** Every 12 hours for temporal (CronExpression: `0 0 */12 * * *`), Daily for all-time
**TTL:** 12 hours (43200s) for temporal, 24 hours (86400s) for all-time

---

### 5. TOP SUBCATEGORY 📂

**Target:** 10 shows

**SubCategory Selection:**
- Source: `query:metric:user_preferences:temporal_30d:{userId}.ListenedPodcastCategories[0].SubCategoryIds[0]`
- Fallback: `query:metric:system_preferences:temporal_30d.ListenedPodcastCategories[0].SubCategoryIds[0]`

**Part A (80%) - Personal:**
- Target: 8 shows
- Fetch: 8 shows in subcategory (không trùng BasedOnYourTaste/NewReleases/HotThisWeek)
- Formula: `personalShowScore = 0.6 × userEngagement + 0.4 × showQuality`
  - userEngagement = episodes_listened / total_episodes
  - showQuality = 0.5×(min(TF,100k)/100k) + 0.5×(avgRating/5)
- Filter: `personalShowScore != 0`
- Logic: Sort personalShowScore DESC → top 8

**Part B (20%) - Popular:**
- Target: 2 shows
- Fetch: (2 + n) popular shows in subcategory (không trùng A + sections trên)
- Cache: Uses `query:metric:show:all_time_max`
- Formula: `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)`
- Logic: Sort popularScore DESC → top 2 (+ trám Part A nếu thiếu)

**Cache Keys:** `query:metric:user_preferences:temporal_30d:{userId}`, `query:metric:system_preferences:temporal_30d`, `query:metric:show:all_time_max`
**Refresh:** Every 2 hours (user/system preferences), Daily (all-time metrics)
**TTL:** 2 hours (7200s) for preferences, 24 hours (86400s) for all-time

---

### 6. TALENTED ROOKIES ⭐

**Target:** 8 podcasters

**Single Part (100%):**
- Fetch: 8 rookies (verified trong 90 days, không trùng TopPodcasters)
- Cache: Uses `query:metric:podcaster:temporal_7d_max` & `query:metric:podcaster:all_time_max`
- Formula: `rookieScore = 0.4×(TLS/MTLS) + 0.4×(G/MG) + 0.2×(RT/MRT)`
  - TLS = Total ListenSession (7d)
  - G = TotalFollow / PodcasterAgeDay
  - RT = AverageRating × log(RatingCount + 1)
- Logic: Sort rookieScore DESC → top 8

**Cache Keys:** `query:metric:podcaster:temporal_7d_max`, `query:metric:podcaster:all_time_max`
**Refresh:** Every 12 hours for temporal, Daily for all-time
**TTL:** 12 hours (43200s) for temporal, 24 hours (86400s) for all-time

---

### 7. EXPLORE [RANDOM CATEGORY] 🎲

**Target:** 12 shows

**Category Selection:**
- Logic: Random 1 category KHÔNG thuộc top 2 của user
- Source: All categories EXCLUDE `query:metric:user_preferences:temporal_30d:{userId}.ListenedPodcastCategories[0,1]`

**Part A (30%) - Hot:**
- Target: 3 shows
- Fetch: 3 hot shows in category (không trùng BasedOnYourTaste/NewReleases/HotThisWeek/TopSubCategory)
- Cache: Uses `query:metric:show:temporal_7d_max`
- Formula: `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)` (7 days)
- Filter: `hotScore > 0`
- Logic: Sort hotScore DESC → top 3

**Part B (70%) - Popular:**
- Target: 9 shows
- Fetch: (9 + n) popular shows in category (không trùng A + sections trên)
- Cache: Uses `query:metric:show:all_time_max`
- Formula: `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)`
- Logic: Sort popularScore DESC → top 9 (+ trám Part A nếu thiếu)

**Cache Keys:** `query:metric:user_preferences:temporal_30d:{userId}`, `query:metric:show:temporal_7d_max`, `query:metric:show:all_time_max`
**Refresh:** Query realtime (random mỗi lần), cache metrics refresh theo schedule (Every 2h for preferences, Every 12h for temporal, Daily for all-time)

---

### 8. TOP PODCASTERS 👥

**Target:** 12 podcasters

**Part A (20%) - Hot (7 days):**
- Target: 4 podcasters
- Fetch: 4 hot podcasters
- Cache: Uses `query:metric:podcaster:temporal_7d_max`
- Formula: `hotScore = 0.5×(NLS/MNLS) + 0.3×(NF/MNF) + 0.15×(G/2MG) + 0.05×(Rating/5)`
- Filter: `hotScore > 0`
- Logic: Sort hotScore DESC → top 4

**Part B (80%) - Popular (all-time):**
- Target: 8 podcasters
- Fetch: 8 popular podcasters (không trùng Part A)
- Cache: Uses `query:metric:podcaster:all_time_max`
- Formula: `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.15×(RT/MRT) + 0.05×(Age/MaxAge)`
- Logic: Sort popularScore DESC → top 8

**Cache Keys:** `query:metric:podcaster:temporal_7d_max`, `query:metric:podcaster:all_time_max`
**Refresh:** Every 12 hours for temporal (CronExpression: `0 0 */12 * * *`), Daily for all-time (CronExpression: `0 0 0 * * *`)
**TTL:** 12 hours (43200s) for temporal, 24 hours (86400s) for all-time

---

## DEDUPLICATION STRATEGY

### Priority Order (Top-down):
1. ContinueListening (episodes - no dedup)
2. BasedOnYourTaste (shows)
3. NewReleases (shows)
4. HotThisWeek (shows + channels)
5. TopSubCategory (shows) - check duplicate with sections 2,3,4
6. TalentedRookies (podcasters - no dedup)
7. ExploreRandom (shows) - check duplicate with sections 2,3,4,5
8. TopPodcasters (podcasters - no dedup)

### Rules:
- Items đã xuất hiện ở section trên → skip ở section dưới
- Chỉ dedup trong cùng entity type (shows ≠ channels ≠ podcasters ≠ episodes)
- Part A > Part B trong mỗi section

---

## REFRESH SCHEDULE

| Section | Frequency | CronExpression | Reason |
|---------|-----------|----------------|--------|
| Continue | Realtime | N/A | User behavior instant |
| BasedOnYourTaste | Every 2 hours | `0 0 */2 * * *` | User preference cache refresh |
| NewReleases | Every 2 hours | `0 0 */2 * * *` | Depends on user preferences |
| HotThisWeek | Every 12 hours | `0 0 */12 * * *` | Temporal metrics refresh |
| TopSubCategory | Every 2 hours | `0 0 */2 * * *` | User/system preferences refresh |
| TalentedRookies | Every 12 hours | `0 0 */12 * * *` | Temporal + all-time metrics |
| ExploreRandom | Realtime | N/A | Random mỗi lần (metrics cached) |
| TopPodcasters | Every 12 hours | `0 0 */12 * * *` | Temporal metrics refresh |

**Cache Dependencies:**
- `query:metric:podcaster:all_time_max` - Daily @ 00:00 (TTL: 24h)
- `query:metric:show:all_time_max` - Daily @ 00:00 (TTL: 24h)
- `query:metric:channel:all_time_max` - Daily @ 00:00 (TTL: 24h)
- `query:metric:podcaster:temporal_7d_max` - Every 12h (TTL: 12h)
- `query:metric:show:temporal_7d_max` - Every 12h (TTL: 12h)
- `query:metric:channel:temporal_7d_max` - Every 12h (TTL: 12h)
- `query:metric:system_preferences:temporal_30d` - Every 2h (TTL: 2h)
- `query:metric:user_preferences:temporal_30d:{userId}` - Every 2h (TTL: 2h)

---

## SCORING FORMULAS

### Popular Score (All-time):
**Shows:**
```
popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)

Where (from query:metric:show:all_time_max):
- TF = TotalFollow
- MTF = MaxTotalFollow
- LC = ListenCount
- MLC = MaxListenCount
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm
```

**Channels:**
```
popularScore = 0.6×(TLS/MTLS) + 0.4×(TF/MTF)

Where (from query:metric:channel:all_time_max):
- TLS = TotalListenSession
- MTLS = MaxTotalListenSession
- TF = TotalFavorite
- MTF = MaxTotalFavorite
```

**Podcasters:**
```
popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.15×(RT/MRT) + 0.05×(Age/MaxAge)

Where (from query:metric:podcaster:all_time_max):
- TF = TotalFollow
- MTF = MaxTotalFollow
- LC = ListenCount
- MLC = MaxListenCount
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm
- Age = PodcasterAgeDay (days since verified)
- MaxAge = Longest podcaster age
```

### Hot Score (7 days):
**Shows:**
```
hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)

Where (from query:metric:show:temporal_7d_max):
- NLS = NewListenSession (last 7 days)
- MNLS = MaxNewListenSession
- NF = NewFollow (last 7 days)
- MNF = MaxNewFollow
```

**Channels:**
```
hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)

Where (from query:metric:channel:temporal_7d_max):
- NLS = NewListenSession (last 7 days)
- MNLS = MaxNewListenSession
- NF = NewFavorite (last 7 days)
- MNF = MaxNewFavorite
```

**Podcasters:**
```
hotScore = 0.5×(NLS/MNLS) + 0.3×(NF/MNF) + 0.15×(G/2MG) + 0.05×(Rating/5)

Where (from query:metric:podcaster:temporal_7d_max):
- NLS = NewListenSession (last 7 days)
- MNLS = MaxNewListenSession
- NF = NewFollow (last 7 days)
- MNF = MaxNewFollow
- G = Growth = NLS + NF
- MG = MaxGrowth
```

### Rookie Score:
```
rookieScore = 0.4×(TLS/MTLS) + 0.4×(G/MG) + 0.2×(RT/MRT)

Where:
- From query:metric:podcaster:temporal_7d_max:
  - TLS = Total ListenSession (7d)
  - MTLS = MaxTotalListenSession
  - G = Growth = TotalFollow / PodcasterAgeDay
  - MG = MaxGrowth
  
- From query:metric:podcaster:all_time_max:
  - RT = RatingTerm = AverageRating × log(RatingCount + 1)
  - MRT = MaxRatingTerm

Filter: Podcasters verified ≤ 90 days
```

### Personal Show Score:
```
personalShowScore = 0.6 × userEngagement + 0.4 × showQuality

userEngagement = episodes_listened / total_episodes_in_show
showQuality = 0.5×(min(totalFollow, 100000)/100000) + 0.5×(averageRating/5)
```

---

## TIME RANGES

```
minShortRangeUserBehaviorLookbackDayCount: 2 days
minMediumRangeUserBehaviorLookbackDayCount: 7 days
minLongRangeUserBehaviorLookbackDayCount: 30 days
minShortRangeContentBehaviorLookbackDayCount: 2 days
minMediumRangeContentBehaviorLookbackDayCount: 7 days
minLongRangeContentBehaviorLookbackDayCount: 30 days
minExtraLongRangeContentBehaviorLookbackDayCount: 90 days
```
