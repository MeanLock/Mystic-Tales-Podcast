# TRENDING TAB - QUERY STRATEGY

## OVERVIEW

Trending Tab hiển thị nội dung trending toàn hệ thống với 14 sections: 8 Fixed Sections (podcasters, channels, shows, episodes) và 6 Dynamic Category Sections được interleaved. 

**Key Characteristics:**
- **NO cross-section deduplication** - Mỗi section hoàn toàn độc lập
- **Dynamic Categories**: Random stable 6 categories từ top 10 trending, refresh mỗi 6h (4 windows/day)
- **Interleaved Layout**: Fixed sections xen kẽ với Dynamic Category sections
- **Anonymous-friendly**: Tất cả sections không phụ thuộc vào user authentication

---

## DEDUPLICATION STRATEGY

### Priority Order:
**NO DEDUPLICATION BETWEEN SECTIONS** - Tất cả sections độc lập

### Rules:
- Không dedup giữa các section khác nhau
- Chỉ đảm bảo không duplicate trong cùng 1 section (implicit via distinct entity IDs)
- Dynamic Categories sử dụng stable random seed (6h window) để tránh thay đổi liên tục

---

## SECTION ORDER & LAYOUT

```
Section Order:
1. Popular Podcasters (10 podcasters)
2. Dynamic Category 1 (10 shows)
3. Hot Podcasters (8 podcasters)
4. Dynamic Category 2 (10 shows)
5. Popular Channels (8 channels)
6. Dynamic Category 3 (10 shows)
7. Hot Channels (8 channels)
8. Dynamic Category 4 (10 shows)
9. Popular Shows (12 shows)
10. Dynamic Category 5 (10 shows)
11. Hot Shows (10 shows)
12. Dynamic Category 6 (10 shows)
13. New Episodes (15 episodes)
14. Popular Episodes (15 episodes)

Pattern: Fixed → Dynamic → Fixed → Dynamic (interleaved)
```

---

## SECTION 1: POPULAR PODCASTERS 👥

**Target:** 10 podcasters

**Single Part (100%):**
- **Query:** `isVerified = true` (từ UserService)
- **Mapping Filter:** `deactivatedAt == null && hasVerifiedPodcasterProfile == true`
- **Formula:** `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.15×(RT/MRT) + 0.05×(Age/MaxAge)`
  - TF = TotalFollow (all-time)
  - LC = ListenCount (all-time)
  - RT = AverageRating × log(RatingCount + 1)
  - Age = Days since verified
  - M* = Max values from cache metric
- **Logic:**
  1. Query all verified podcasters từ UserService (`isVerified = true`)
  2. Filter bằng mapping: `deactivatedAt == null && hasVerifiedPodcasterProfile == true`
  3. Tính popularScore realtime cho từng podcaster với cache metrics
  4. Sort popularScore DESC → top 10 podcasters (lấy chính)

**Cache Key:** `query:metric:podcaster:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 2: HOT PODCASTERS THIS WEEK 🔥

**Target:** 8 podcasters

**Single Part (100%):**
- **Query:** `isVerified = true` (từ UserService)
- **Mapping Filter:** `deactivatedAt == null && hasVerifiedPodcasterProfile == true`
- **Formula:** `hotScore = 0.5×(NLS/MNLS) + 0.3×(NF/MNF) + 0.15×(G/2MG) + 0.05×(Rating/5)`
  - NLS = NewListenSession (last 7 days)
  - NF = NewFollow (last 7 days)
  - G = NLS + NF (Growth)
  - Rating = AverageRating
  - M* = Max values from cache metric
- **Logic:**
  1. Query all verified podcasters từ UserService (`isVerified = true`)
  2. Filter bằng mapping: `deactivatedAt == null && hasVerifiedPodcasterProfile == true`
  3. Query listen sessions (7 days) từ PodcastService:
     - `ls.CreatedAt >= NOW - 7 days`
     - `ls.IsContentRemoved == false`
     - `ls.PodcastEpisode.DeletedAt == null`
     - `ls.PodcastEpisode.PodcastShow.DeletedAt == null`
     - Filter by episode latest status = Published
  4. Query follows (7 days) từ UserService:
     - `AccountFollowedPodcaster.CreatedAt >= NOW - 7 days`
  5. Tính hotScore realtime cho từng podcaster
  6. Filter hotScore > 0
  7. Sort hotScore DESC → top 8 podcasters (lấy chính)

**Cache Key:** `query:metric:podcaster:temporal_7d_max`  
**Refresh:** Every 12 hours (CronExpression: `0 0 */12 * * *`)  
**TTL:** 12 hours (43200s)

---

## SECTION 3: POPULAR CHANNELS 📺

**Target:** 8 channels

**Single Part (100%):**
- **Query:** `deletedAt == null`
- **Status Filter:** Latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Formula:** `popularScore = 0.6×(LC/MLC) + 0.4×(TF/MTF)`
  - LC = ListenCount (all-time)
  - TF = TotalFavorite (all-time)
  - M* = Max values from cache metric
- **Logic:**
  1. Query all channels: `deletedAt == null`
  2. Include PodcastChannelStatusTrackings
  3. Filter by latest status = Published
  4. Tính popularScore realtime cho từng channel với cache metrics
  5. Sort popularScore DESC → top 8 channels (lấy chính)

**Cache Key:** `query:metric:channel:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 4: HOT CHANNELS THIS WEEK 🔥

**Target:** 8 channels

**Single Part (100%):**
- **Query:** `deletedAt == null`
- **Status Filter:** Latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Formula:** `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)`
  - NLS = NewListenSession (last 7 days)
  - NF = NewFavorite (last 7 days)
  - M* = Max values from cache metric
- **Logic:**
  1. Query all channels: `deletedAt == null`
  2. Include PodcastChannelStatusTrackings
  3. Filter by latest status = Published
  4. Query listen sessions (7 days) - từ episodes thuộc shows của channel:
     - `ls.CreatedAt >= NOW - 7 days`
     - `ls.IsContentRemoved == false`
     - `ls.PodcastEpisode.DeletedAt == null`
     - `ls.PodcastEpisode.PodcastShow.DeletedAt == null`
     - `ls.PodcastEpisode.PodcastShow.PodcastChannelId != null`
     - Filter by episode latest status = Published
  5. Query favorites (7 days) từ UserService:
     - `AccountFavoritedPodcastChannel.CreatedAt >= NOW - 7 days`
  6. Tính hotScore realtime cho từng channel
  7. Filter hotScore > 0
  8. Sort hotScore DESC → top 8 channels (lấy chính)

**Cache Key:** `query:metric:channel:temporal_7d_max`  
**Refresh:** Every 12 hours (CronExpression: `0 0 */12 * * *`)  
**TTL:** 12 hours (43200s)

---

## SECTION 5: POPULAR SHOWS 📻

**Target:** 12 shows

**Single Part (100%):**
- **Query:** `deletedAt == null`
- **Status Filter:** 
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
  - (Nếu show thuộc channel): `channel.DeletedAt == null` AND latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Formula:** `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)`
  - TF = TotalFollow (all-time)
  - LC = ListenCount (all-time)
  - RT = AverageRating × log(RatingCount + 1)
  - M* = Max values from cache metric
- **Logic:**
  1. Query all shows: `deletedAt == null`
  2. Include PodcastShowStatusTrackings, PodcastChannel, PodcastChannelStatusTrackings
  3. Filter by latest show status = Published
  4. Filter by channel conditions (nếu show có channel):
     - `channel.DeletedAt == null`
     - Latest channel status = Published
  5. Tính popularScore realtime cho từng show với cache metrics
  6. Sort popularScore DESC → top 12 shows (lấy chính)

**Cache Key:** `query:metric:show:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 6: HOT SHOWS THIS WEEK 🔥

**Target:** 10 shows

**Single Part (100%):**
- **Query:** `deletedAt == null`
- **Status Filter:**
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
  - (Nếu show thuộc channel): `channel.DeletedAt == null` AND latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Formula:** `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)`
  - NLS = NewListenSession (last 7 days)
  - NF = NewFollow (last 7 days)
  - M* = Max values from cache metric
- **Logic:**
  1. Query all shows: `deletedAt == null`
  2. Include PodcastShowStatusTrackings, PodcastChannel, PodcastChannelStatusTrackings
  3. Filter by latest show status = Published
  4. Filter by channel conditions (nếu show có channel)
  5. Query listen sessions (7 days):
     - `ls.CreatedAt >= NOW - 7 days`
     - `ls.IsContentRemoved == false`
     - `ls.PodcastEpisode.DeletedAt == null`
     - `ls.PodcastEpisode.PodcastShow.DeletedAt == null`
     - Filter by episode latest status = Published
  6. Query follows (7 days) từ UserService:
     - `AccountFollowedPodcastShow.CreatedAt >= NOW - 7 days`
  7. Tính hotScore realtime cho từng show
  8. Filter hotScore > 0
  9. Sort hotScore DESC → top 10 shows (lấy chính)

**Cache Key:** `query:metric:show:temporal_7d_max`  
**Refresh:** Every 12 hours (CronExpression: `0 0 */12 * * *`)  
**TTL:** 12 hours (43200s)

---

## SECTION 7: NEW EPISODES 🆕

**Target:** 15 episodes

**Part Main (100%) - Recent Published:**
- **Query:** `deletedAt == null AND show.deletedAt == null`
- **Status Filter:**
  - Latest `PodcastEpisodeStatusTracking.CreatedAt >= NOW - 2 days`
  - Latest `PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published`
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
  - (Nếu show thuộc channel): `channel.DeletedAt == null` AND latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Sort:** Published tracking `CreatedAt DESC`
- **Logic:**
  1. Query episodes: `deletedAt == null AND show.deletedAt == null`
  2. Include:
     - PodcastEpisodeStatusTrackings
     - PodcastShow → PodcastShowStatusTrackings
     - PodcastShow → PodcastChannel → PodcastChannelStatusTrackings
  3. Filter episodes có Published tracking trong 2 days
  4. Filter by episode latest status = Published
  5. Filter by show latest status = Published
  6. Filter by channel conditions (nếu show có channel)
  7. Sort by Published tracking CreatedAt DESC → top 15 episodes (lấy chính)

**Fallback (0%) - Extended Time Range:**
- **Condition:** Nếu Part Main < 15 episodes
- **Query:** Tương tự Part Main nhưng extend sang 7 days
- **Logic:** 
  1. Query với `PodcastEpisodeStatusTracking.CreatedAt >= NOW - 7 days`
  2. Filter tương tự Part Main
  3. Sort tương tự Part Main
  4. Lấy số còn thiếu để đủ 15

**Cache:** Không (query realtime)

---

## SECTION 8: POPULAR EPISODES 🔥

**Target:** 15 episodes

**Part A (70%) - Top All-Time:**
- **Query:** `deletedAt == null AND show.deletedAt == null AND listenCount > 0`
- **Status Filter:**
  - Latest `PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published`
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
  - (Nếu show thuộc channel): `channel.DeletedAt == null` AND latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Sort:** `listenCount DESC`
- **Logic:**
  1. Query episodes: `deletedAt == null AND show.deletedAt == null AND listenCount > 0`
  2. Include:
     - PodcastEpisodeStatusTrackings
     - PodcastShow → PodcastShowStatusTrackings
     - PodcastShow → PodcastChannel → PodcastChannelStatusTrackings
  3. Filter by episode latest status = Published
  4. Filter by show latest status = Published
  5. Filter by channel conditions (nếu show có channel)
  6. Sort listenCount DESC → top 10 episodes (lấy chính)

**Part B (30%) - Diversity Fallback:**
- **Query:** `deletedAt == null AND show.deletedAt == null AND listenCount > 0 AND showId NOT IN (Part A shows)`
- **Status Filter:** Tương tự Part A
- **Sort:** `listenCount DESC`
- **Logic:**
  1. Query episodes với điều kiện như Part A NHƯNG `showId NOT IN (Part A shows)`
  2. Filter tương tự Part A
  3. Sort listenCount DESC → top (5 + n) episodes
     - n = số lượng Part A thiếu (nếu Part A < 10)
     - Lấy 5 episodes (lấy chính) + n episodes trám vào Part A

**Cache:** Không (sử dụng denormalized listenCount column)

---

## SECTION 9-14: DYNAMIC CATEGORY [RANDOM CATEGORY] × 6 🎲

**Target:** 10 shows × 6 categories = 60 shows total

### Category Selection Logic:

**Source:** `query:metric:system_preferences:temporal_30d.ListenedPodcastCategories`

**Selection Strategy:**
- Lấy top 10 trending categories theo `ListenCount DESC`
- Random **stable** 6 categories từ top 10
- **Stability Window:** 6 hours (4 windows per day)
- **Seed Calculation:** `Math.Floor(CurrentHour / 6)`
  - Window 1: 00:00-05:59 (seed = 0)
  - Window 2: 06:00-11:59 (seed = 1)
  - Window 3: 12:00-17:59 (seed = 2)
  - Window 4: 18:00-23:59 (seed = 3)

**Reason for Stable Random:**
- Tránh categories thay đổi liên tục mỗi lần refresh
- User experience tốt hơn khi categories ổn định trong 6h
- Vẫn đảm bảo tính dynamic với 4 lần thay đổi/ngày

---

### Per Category Section Structure:

**Target per category:** 10 shows

#### Part A (40%) - Hot Shows (7 days):
- **Target:** 4 shows
- **Query:** `categoryId == selected AND deletedAt == null`
- **Status Filter:**
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
  - (Nếu show thuộc channel): `channel.DeletedAt == null` AND latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`
- **Formula:** `hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)` (7 days)
  - NLS = NewListenSession (last 7 days)
  - NF = NewFollow (last 7 days)
  - M* = Max values from cache metric
- **Logic:**
  1. Query shows trong selected category: `categoryId == selected AND deletedAt == null`
  2. Include PodcastShowStatusTrackings, PodcastChannel, PodcastChannelStatusTrackings
  3. Filter by show latest status = Published
  4. Filter by channel conditions (nếu show có channel)
  5. Query listen sessions (7 days) cho shows trong category
  6. Query follows (7 days) cho shows trong category
  7. Tính hotScore realtime cho từng show
  8. Filter hotScore > 0
  9. Sort hotScore DESC → top 4 shows (lấy chính)

#### Part B (60%) - Popular Shows (all-time):
- **Target:** 6 shows
- **Query:** `categoryId == selected AND deletedAt == null AND id NOT IN (Part A)`
- **Status Filter:** Tương tự Part A
- **Formula:** `popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)`
  - TF = TotalFollow (all-time)
  - LC = ListenCount (all-time)
  - RT = AverageRating × log(RatingCount + 1)
  - M* = Max values from cache metric
- **Logic:**
  1. Query shows trong selected category (KHÔNG trùng Part A)
  2. Filter tương tự Part A
  3. Tính popularScore realtime cho từng show với cache metrics
  4. Sort popularScore DESC → top (6 + n) shows
     - n = số lượng Part A thiếu (nếu Part A < 4)
     - Lấy 6 shows (lấy chính) + n shows trám vào Part A

**Cache Keys:**
- Category selection: `query:metric:system_preferences:temporal_30d`
- Hot scores: `query:metric:show:temporal_7d_max`
- Popular scores: `query:metric:show:all_time_max`

**Refresh:**
- System preferences: Every 2 hours (CronExpression: `0 0 */2 * * *`)
- Temporal metrics: Every 12 hours (CronExpression: `0 0 */12 * * *`)
- All-time metrics: Daily @ 00:00 (CronExpression: `0 0 0 * * *`)

**TTL:**
- System preferences: 2 hours (7200s)
- Temporal: 12 hours (43200s)
- All-time: 24 hours (86400s)

---

## SCORING FORMULAS

### Popular Score (All-time):

**Podcasters:**
```
popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.15×(RT/MRT) + 0.05×(Age/MaxAge)

Where:
- TF = TotalFollow (all-time)
- LC = ListenCount (all-time)
- RT = AverageRating × log(RatingCount + 1)
- Age = Days since verified
- MTF = MaxTotalFollow (from cache)
- MLC = MaxListenCount (from cache)
- MRT = MaxRatingTerm (from cache)
- MaxAge = MaxAge (from cache)
```

**Channels:**
```
popularScore = 0.6×(LC/MLC) + 0.4×(TF/MTF)

Where:
- LC = ListenCount (all-time)
- TF = TotalFavorite (all-time)
- MLC = MaxListenCount (from cache)
- MTF = MaxTotalFavorite (from cache)
```

**Shows:**
```
popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)

Where:
- TF = TotalFollow (all-time)
- LC = ListenCount (all-time)
- RT = AverageRating × log(RatingCount + 1)
- MTF = MaxTotalFollow (from cache)
- MLC = MaxListenCount (from cache)
- MRT = MaxRatingTerm (from cache)
```

---

### Hot Score (7 days):

**Podcasters:**
```
hotScore = 0.5×(NLS/MNLS) + 0.3×(NF/MNF) + 0.15×(G/2MG) + 0.05×(Rating/5)

Where:
- NLS = NewListenSession (last 7 days)
- NF = NewFollow (last 7 days)
- G = NLS + NF (Growth = tổng new activity)
- Rating = AverageRating (all-time)
- MNLS = MaxNewListenSession (from cache)
- MNF = MaxNewFollow (from cache)
- MG = MaxGrowth (from cache)

Note: G normalized by 2MG để balance với các metrics khác
```

**Channels:**
```
hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)

Where:
- NLS = NewListenSession (last 7 days) - từ episodes của shows thuộc channel
- NF = NewFavorite (last 7 days)
- MNLS = MaxNewListenSession (from cache)
- MNF = MaxNewFavorite (from cache)
```

**Shows:**
```
hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)

Where:
- NLS = NewListenSession (last 7 days)
- NF = NewFollow (last 7 days)
- MNLS = MaxNewListenSession (from cache)
- MNF = MaxNewFollow (from cache)
```

---

## REFRESH SCHEDULE

| Cache Key | Entity | Time Window | Frequency | CronExpression | TTL | Update Job Name |
|-----------|--------|-------------|-----------|----------------|-----|-----------------|
| `query:metric:podcaster:all_time_max` | Podcasters | All-time | Daily @ 00:00 | `0 0 0 * * *` | 24h (86400s) | PodcasterAllTimeMaxQueryMetricUpdateJob |
| `query:metric:podcaster:temporal_7d_max` | Podcasters | 7 days | Every 12h | `0 0 */12 * * *` | 12h (43200s) | PodcasterTemporal7dMaxQueryMetricUpdateJob |
| `query:metric:show:all_time_max` | Shows | All-time | Daily @ 00:00 | `0 0 0 * * *` | 24h (86400s) | ShowAllTimeMaxQueryMetricUpdateJob |
| `query:metric:show:temporal_7d_max` | Shows | 7 days | Every 12h | `0 0 */12 * * *` | 12h (43200s) | ShowTemporal7dMaxQueryMetricUpdateJob |
| `query:metric:channel:all_time_max` | Channels | All-time | Daily @ 00:00 | `0 0 0 * * *` | 24h (86400s) | ChannelAllTimeMaxQueryMetricUpdateJob |
| `query:metric:channel:temporal_7d_max` | Channels | 7 days | Every 12h | `0 0 */12 * * *` | 12h (43200s) | ChannelTemporal7dMaxQueryMetricUpdateJob |
| `query:metric:system_preferences:temporal_30d` | System | 30 days | Every 2h | `0 0 */2 * * *` | 2h (7200s) | SystemPreferencesTemporal30dQueryMetricUpdateJob |

**Cache Dependency Flow:**
1. **All-time metrics** → Updated daily @ 00:00 → Used for popular scores
2. **Temporal metrics** → Updated every 12h → Used for hot scores
3. **System preferences** → Updated every 2h → Used for category selection (Dynamic Categories)

**Cache Hit Strategy:**
- All scores calculated realtime using cached max values for normalization
- Cache miss → Return empty section (graceful degradation)
- Stale cache (beyond TTL) → Still usable but trigger background refresh

---

## VALIDATION CONDITIONS SUMMARY

### General Validation Rules:

**Podcasters:**
- Query condition: `isVerified = true` (từ UserService)
- Mapping filter: `deactivatedAt == null && hasVerifiedPodcasterProfile == true`

**Channels:**
- Query condition: `deletedAt == null`
- Status filter: Latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`

**Shows:**
- Query condition: `deletedAt == null`
- Status filter: Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
- Channel validation (nếu show thuộc channel):
  - `channel.DeletedAt == null`
  - Latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`

**Episodes:**
- Query condition: `deletedAt == null AND show.deletedAt == null`
- Status filter:
  - Latest `PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published`
  - Latest `PodcastShowStatusTracking.PodcastShowStatusId == Published`
- Channel validation (nếu show thuộc channel):
  - `channel.DeletedAt == null`
  - Latest `PodcastChannelStatusTracking.PodcastChannelStatusId == Published`

---

### Listen Session Validation (for Hot Scores):

**For all hot score calculations:**
```csharp
Listen Session Query Conditions:
- ls.CreatedAt >= NOW - 7 days
- ls.IsContentRemoved == false
- ls.PodcastEpisode.DeletedAt == null
- ls.PodcastEpisode.PodcastShow.DeletedAt == null
- (For channels): ls.PodcastEpisode.PodcastShow.PodcastChannelId != null

Post-query Filter:
- Episode latest status = Published
- (Additional filters per entity type)
```

---

### Status Tracking Logic:

**Lấy Latest Status:**
```csharp
var latestStatus = entity.StatusTrackings
    .OrderByDescending(t => t.CreatedAt)
    .FirstOrDefault()?.StatusId;
```

**Published Status Enum Values:**
- `PodcastShowStatusEnum.Published`
- `PodcastChannelStatusEnum.Published`
- `PodcastEpisodeStatusEnum.Published`

---

## PERFORMANCE CONSIDERATIONS

### Query Optimization:

1. **Index Requirements:**
   - `PodcastShow`: Index on (CategoryId, DeletedAt, [Status])
   - `PodcastChannel`: Index on (DeletedAt, [Status])
   - `PodcastEpisode`: Index on (ShowId, DeletedAt, ListenCount, [Status])
   - `PodcastEpisodeListenSession`: Index on (CreatedAt, IsContentRemoved)
   - `PodcasterProfile`: Index on (IsVerified, VerifiedAt)

2. **Cache Strategy:**
   - Max values cached → O(1) normalization
   - Avoid N+1 queries → Batch fetch entities
   - Mapping helpers pre-load related data

3. **Pagination Support:**
   - Not required for Trending Tab (fixed counts per section)
   - All sections return exact counts (no "load more")

4. **Concurrent Queries:**
   - Independent sections → Can be parallelized
   - Cache loading → Parallel Task.WhenAll
   - Score calculations → Sequential per section (acceptable latency)

---

## ERROR HANDLING & FALLBACKS

### Cache Miss Scenarios:

**Scenario 1: Temporal metric cache miss**
- Impact: Hot score sections return empty
- Fallback: Skip section (graceful degradation)
- Log: Warning level

**Scenario 2: All-time metric cache miss**
- Impact: Popular score sections return empty
- Fallback: Skip section (graceful degradation)
- Log: Warning level

**Scenario 3: System preferences cache miss**
- Impact: Dynamic Categories cannot select categories
- Fallback: Use default top 6 categories by ID
- Log: Warning level

### Query Timeout:

**Timeout Limits:**
- Per-section query: 10 seconds
- Total feed generation: 30 seconds

**Timeout Handling:**
- Section timeout → Return null for that section
- Continue processing other sections
- Log error with section name

### Data Validation:

**Invalid Data Scenarios:**
1. Show without podcaster → Filter out during mapping
2. Episode without show → Already filtered by query conditions
3. Negative metrics → Treat as 0 in score calculation
4. Null status tracking → Filter out during status check

---

## MONITORING & METRICS

### Key Metrics to Monitor:

1. **Performance Metrics:**
   - Average response time per section
   - Cache hit rate (all-time vs temporal)
   - Query execution time distribution

2. **Data Quality Metrics:**
   - % sections returning empty
   - Average items per section
   - Score distribution (detect anomalies)

3. **Business Metrics:**
   - Click-through rate per section
   - User engagement by section type
   - Dynamic category diversity

### Alert Thresholds:

- Cache hit rate < 95% → Alert (cache refresh issues)
- Section timeout rate > 5% → Alert (query performance)
- Average section items < 50% target → Warning (data quality)
- Response time > 2s → Warning (performance degradation)

---

## CHANGELOG

### Version 1.0 (Initial)
- NO cross-section deduplication
- 8 Fixed Sections + 6 Dynamic Categories
- Stable random category selection (6h windows)
- Hot scores (7 days) + Popular scores (all-time)
- Anonymous-friendly (no user authentication required)
