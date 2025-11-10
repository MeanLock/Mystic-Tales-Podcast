# DISCOVERY TAB - KỊCH BẢN TRUY VẤN CHI TIẾT

## MỤC LỤC
1. [Tổng Quan Hệ Thống](#tổng-quan-hệ-thống)
2. [8 Hạng Mục Discovery](#8-hạng-mục-discovery)
3. [Cache Architecture](#cache-architecture)
4. [Refresh Strategy](#refresh-strategy)
5. [Công Thức Tính Điểm](#công-thức-tính-điểm)
6. [Cold-Start Strategy](#cold-start-strategy)

---

## TỔNG QUAN HỆ THỐNG

### Nguyên Tắc Thiết Kế

**1. Progressive Personalization**
- Từ 100% personalized (Continue Listening) → 70-80% mix → 50% mix → random exploration
- Tránh filter bubble bằng cách expose user với content mới

**2. Exploration-Exploitation Balance**
- 70-80% exploitation: Recommend theo taste đã biết
- 20-30% exploration: Introduce novelty & serendipity

**3. Performance First**
- Cache aggressive với system-wide max values
- Refresh intelligent theo tốc độ thay đổi của data
- Trade-off: Performance > Perfect accuracy

**4. Cold-Start Friendly**
- Mỗi hạng mục có fallback logic cho user mới
- Không bao giờ show empty sections

---

## 8 HẠNG MỤC DISCOVERY

### 1. CONTINUE LISTENING 🎧

**Mục đích:** Giúp user tiếp tục episodes đang nghe dở

**Query Logic:**
```
Query Type: continue
Source: PodcastEpisodeListenSession
Filter: 
  - isCompleted = 0 
  - lastListenDurationSeconds < audioLength
Sort: lastListenAt DESC (gần nhất lên đầu)
Limit: 5-10 episodes
```

**Cache Requirements:**
- ❌ Không cần system cache
- ✅ Realtime query từ user's listen history

**Display Condition:**
- Chỉ hiển thị khi user có episodes chưa nghe xong
- Nếu empty → skip section này

**Refresh Frequency:** Realtime (mỗi lần user vào Discovery tab)

**Implementation Notes:**
```sql
SELECT * FROM PodcastEpisodeListenSession
WHERE accountId = {userId}
  AND isCompleted = 0
  AND lastListenDurationSeconds < (
    SELECT audioLength FROM PodcastEpisode 
    WHERE id = podcastEpisodeId
  )
ORDER BY createdAt DESC
LIMIT 10;
```

---

### 2. BASED ON YOUR TASTE 🎵

**Mục đích:** Personalized recommendations với chút discovery

**Query Logic:**
```
Mix Ratio: 70% personalized + 30% exploration
Total Output: 12 shows

Part A (70% - 8-9 shows):
  Query Type: baseOnListened
  Steps:
    1. Lấy 2-4 categories user nghe nhiều nhất trong 30 ngày
    2. Với mỗi category, lấy 3 shows
    3. Sort shows theo popularScore trong category
  
  Formula cho mỗi show:
    popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)
    
    Where:
    - TF = TotalFollow của show
    - MTF = MaxTotalFollow (cached)
    - LC = ListenCount của show
    - MLC = MaxListenCount (cached)
    - RT = AverageRating × log(RatingCount + 1)
    - MRT = MaxRatingTerm (cached)

Part B (30% - 3-4 shows):
  Query Type: popular (toàn hệ thống)
  Steps:
    1. Calculate popularScore cho all shows
    2. Random pick 3-4 từ top 50 popular shows
    3. Exclude shows đã có trong Part A
  
Final Step:
  Shuffle(Part A + Part B) → return 12 shows
```

**Cache Requirements:**
- ✅ `user_listening_preferences.top_categories_30d`
  - Structure: `[{categoryId, subCategoryIds[], listen_count}]`
  - Lookback: 30 days
- ✅ System cache: MTF, MLC, MRT (all-time max values)

**Fallback (Cold-Start):**
```
IF user has no listening history:
  → 100% popular shows (top 12)
  → Use pure popularScore ranking
```

**Refresh Frequency:** Daily
- User preferences cache: Update 1x/day at 00:00
- System max cache: Update 1x/day at 00:00

**Implementation Notes:**
```javascript
// Pseudocode
async function getBasedOnTaste(userId) {
  const userPrefs = await getUserPreferences(userId);
  
  if (!userPrefs || userPrefs.top_categories_30d.length === 0) {
    // Cold-start fallback
    return await getPopularShows(12);
  }
  
  // Part A: 70% personalized
  const personalizedShows = [];
  const topCategories = userPrefs.top_categories_30d.slice(0, 4);
  
  for (const category of topCategories) {
    const shows = await getShowsByCategory(category.categoryId, 3);
    personalizedShows.push(...shows);
  }
  
  // Part B: 30% exploration
  const popularShows = await getPopularShows(50);
  const explorationShows = randomPick(
    popularShows.filter(s => !personalizedShows.includes(s)), 
    3
  );
  
  // Shuffle and return
  return shuffle([...personalizedShows, ...explorationShows]).slice(0, 12);
}
```

---

### 3. NEW RELEASES 🆕

**Mục đích:** Shows mới từ podcasters user quan tâm + discovery

**Query Logic:**
```
Mix Ratio: 50% personalized + 50% discovery
Total Output: 10 shows

Part A (50% - 5 shows):
  Query Type: fromInterestedPodcasters
  Steps:
    1. Lấy 2 podcasters user nghe nhiều nhất trong 30 ngày
       (từ cache: top_podcasters_30d)
    2. Lấy shows mới published (trong 2 ngày) từ 2 podcasters đó
    3. Filter: 
       - status = Published
       - publishedAt >= NOW() - 2 days
    4. Sort: releaseDate DESC
    5. Limit: 5 shows

Part B (50% - 5 shows):
  Query Type: new (toàn hệ thống)
  Steps:
    1. Lấy shows published trong 2 ngày gần nhất (toàn hệ thống)
    2. Filter:
       - status = Published
       - publishedAt >= NOW() - 2 days
       - NOT IN (Part A shows)
    3. Sort: releaseDate DESC
    4. Limit: 5 shows
  
Final Step:
  Interleave(Part A, Part B) → return 10 shows
  Pattern: [A1, B1, A2, B2, A3, B3, A4, B4, A5, B5]
```

**Cache Requirements:**
- ✅ `user_listening_preferences.top_podcasters_30d`
  - Structure: `[{podcasterId, listen_count}]`
  - Lookback: 30 days
- ❌ Không cần system max cache (query based on time)

**Fallback (Cold-Start):**
```
IF user has no listening history:
  → 100% new shows (top 10 mới nhất)
  → Pure time-based sorting
```

**Refresh Frequency:** Every 6 hours
- Vì content được publish liên tục
- Cache user podcaster preferences: Daily

**Time Range Configuration:**
```
minShortRangeContentBehaviorLookbackDayCount: 2 days
```

**Implementation Notes:**
```sql
-- Part A: From interested podcasters
SELECT ps.* 
FROM PodcastShow ps
WHERE ps.podcasterId IN (
  SELECT podcasterId 
  FROM user_top_podcasters 
  WHERE userId = {userId}
  LIMIT 2
)
  AND ps.status = 'Published'
  AND ps.publishedAt >= NOW() - INTERVAL 2 DAY
ORDER BY ps.releaseDate DESC
LIMIT 5;

-- Part B: System-wide new
SELECT ps.* 
FROM PodcastShow ps
WHERE ps.status = 'Published'
  AND ps.publishedAt >= NOW() - INTERVAL 2 DAY
  AND ps.id NOT IN ({Part A show ids})
ORDER BY ps.releaseDate DESC
LIMIT 5;
```

---

### 4. HOT THIS WEEK 🔥

**Mục đích:** Trending content trong 7 ngày gần đây

**Query Logic:**
```
Mix Ratio: 60% trending + 40% popular
Total Output: 15 items (shows + channels)

Part A (60% - 9 items):
  Query Type: hotRecently
  Entities: Mix of shows (5) + channels (4)
  
  For Shows:
    Formula: hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)
    Where:
    - NLS = New ListenSession trong 7 ngày của show này
    - MNLS = MaxNewListenSession trong 7 ngày (cached)
    - NF = New Follow trong 7 ngày của show này
    - MNF = MaxNewFollow trong 7 ngày (cached)
  
  For Channels:
    Formula: hotScore = 0.6×(NLS/MNLS) + 0.4×(NFa/MNFa)
    Where:
    - NLS = New ListenSession trong 7 ngày của channel này
    - MNLS = MaxNewListenSession trong 7 ngày (cached)
    - NFa = New Favorite trong 7 ngày của channel này
    - MNFa = MaxNewFavorite trong 7 ngày (cached)
  
  Steps:
    1. Calculate hotScore for all shows
    2. Calculate hotScore for all channels
    3. Sort both DESC
    4. Pick top 5 shows + top 4 channels

Part B (40% - 6 items):
  Query Type: popular
  Entities: Mix of shows (4) + channels (2)
  
  For Shows:
    Formula: popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)
    (All-time metrics)
  
  For Channels:
    Formula: popularScore = 0.6×(TLS/MTLS) + 0.4×(TFa/MTFa)
    (All-time metrics)
  
  Steps:
    1. Calculate popularScore for all shows
    2. Calculate popularScore for all channels
    3. Sort both DESC
    4. Pick top 4 shows + top 2 channels
  
Final Step:
  Shuffle(Part A + Part B) → return 15 items
```

**Cache Requirements:**

**Temporal Cache (7 days):**
- ✅ Shows: MNLS (MaxNewListenSession), MNF (MaxNewFollow)
- ✅ Channels: MNLS (MaxNewListenSession), MNFa (MaxNewFavorite)

**All-time Cache:**
- ✅ Shows: MTF (MaxTotalFollow), MLC (MaxListenCount), MRT (MaxRatingTerm)
- ✅ Channels: MTLS (MaxTotalListenSession), MTFa (MaxTotalFavorite)

**Fallback:** 
- Không cần (popular luôn có data)
- Nếu temporal data thiếu → fall back 100% popular

**Refresh Frequency:** Daily
- Temporal cache (7d): Update 1x/day at 00:00
- All-time cache: Update 1x/day at 00:00

**Time Range Configuration:**
```
minMediumRangeContentBehaviorLookbackDayCount: 7 days
```

**Implementation Notes:**
```javascript
// Calculate temporal metrics (background job)
async function calculateNewListenSession7d(showId) {
  const count = await db.query(`
    SELECT COUNT(DISTINCT id) as session_count
    FROM PodcastEpisodeListenSession
    WHERE podcastEpisodeId IN (
      SELECT id FROM PodcastEpisode WHERE podcastShowId = ${showId}
    )
    AND createdAt >= NOW() - INTERVAL 7 DAY
  `);
  return count;
}

async function calculateNewFollow7d(showId) {
  const count = await db.query(`
    SELECT COUNT(*) as follow_count
    FROM AccountFollowedPodcastShow
    WHERE podcastShowId = ${showId}
    AND createdAt >= NOW() - INTERVAL 7 DAY
  `);
  return count;
}
```

---

### 5. [TOP SUBCATEGORY FOR YOU] 📂

**Mục đích:** Deep dive vào subcategory user thích nhất

**Dynamic Section Title:**
```
Pattern: "{SubCategory Name} For You"
Examples:
  - "Serial Killers For You"
  - "Tech Startups For You"
  - "Horror Stories For You"

Logic:
  - Get user's top subcategory from cache
  - Use subcategory name from database
```

**Query Logic:**
```
Mix Ratio: 80% personalized + 20% exploration
Total Output: 12 shows

Step 1: Determine Target SubCategory
  Source: user_listening_preferences.top_categories_30d
  Logic:
    1. Get top category (highest listen_count)
    2. Get top subcategory within that category
    3. Store: targetSubCategoryId

Step 2: Part A (80% - 9-10 shows)
  Query Type: subCategoryId={targetSubCategoryId}
  Personalization Logic:
    1. Get all shows in this subcategory
    2. Calculate personalScore for each show:
       
       <!--Cũ personalScore = 0.6 × userEngagement + 0.4 × showQuality  -->
      personalScore = showQuality / userEngagement 
       
       Where:
       - userEngagement = episodes_listened_count / total_episodes (bỏ qua các show không có episode nào được được publish) (userEngagement min = 0,000000000001 để tránh trường hợp =0 và min phải thật nhỏ để tránh bị tình huống người dùng mới chỉ nghe 1 episode trong 1 show có total 1 tỉ tập đang publish)
       - showQuality = (totalFollow/10000) × 0.5 + (averageRating/5) × 0.5
       
       (Normalize to 0-1 range)
    
    3. Sort by personalScore DESC
    4. Return top 10

Step 3: Part B (20% - 2-3 shows)
  Query Type: subCategoryId={targetSubCategoryId} + popular
  Logic:
    1. Get shows in subcategory
    2. Calculate popularScore (all-time)
    3. Random pick 2-3 from top 20
    4. Exclude shows already in Part A
  
Final Step:
  Interleave(Part A, Part B) → return 12 shows
```

**Cache Requirements:**
- ✅ `user_listening_preferences.top_categories_30d`
  - Must include subcategory breakdown
- ✅ User's episode listen history per show
  - Track: episodes listened in each show
- ✅ System cache: MTF, MLC (for fallback scoring)

**Fallback (Cold-Start):**
```
IF user has no category preference:
  1. Pick trending/popular category (system-wide)
     → Query most listened category in last 7 days
  2. Pick top subcategory in that category
  3. Run query with 100% popular shows in that subcategory
```

**Refresh Frequency:** Every 2 days
- Balance between freshness and stability
- User có time để explore content

**Time Range Configuration:**
```
minLongRangeUserBehaviorLookbackDayCount: 30 days
```

**Implementation Notes:**
```javascript
// Get user's episode engagement in subcategory
async function getUserEngagementScore(userId, showId) {
  const result = await db.query(`
    SELECT 
      COUNT(DISTINCT pels.id) as episodes_listened,
      (SELECT COUNT(*) FROM PodcastEpisode WHERE podcastShowId = ${showId}) as total_episodes
    FROM PodcastEpisodeListenSession pels
    JOIN PodcastEpisode pe ON pels.podcastEpisodeId = pe.id
    WHERE pels.accountId = ${userId}
      AND pe.podcastShowId = ${showId}
  `);
  
  return result.episodes_listened / result.total_episodes;
}

// Calculate combined score
function calculatePersonalScore(userEngagement, totalFollow, averageRating) {
  const engagementScore = userEngagement;
  const qualityScore = 
    (Math.min(totalFollow, 100000) / 100000) * 0.5 + 
    (averageRating / 5) * 0.5;
  
  return 0.6 * engagementScore + 0.4 * qualityScore;
}
```

---

### 6. TALENTED ROOKIES ⭐

**Mục đích:** Discover new podcasters (verified trong 90 ngày)

**Query Logic:**
```
Query Type: talentedRookie
Total Output: 8 podcasters

Step 1: Filter Eligible Podcasters
  Criteria:
    - verifiedAt >= NOW() - 90 days
    - isVerified = true
    - deactivatedAt = null
    - violationLevel = 0

Step 2: Calculate Rookie Score
  Formula:
    rookieScore = 0.4×(TLS/MTLS) + 0.4×(G/MG) + 0.2×(RT/MRT)
  
  Where:
    TLS = Total ListenSession trong 7 ngày (temporal)
    MTLS = MaxTotalListenSession trong 7 ngày (cached, temporal)
    
    G = TotalFollow / PodcasterAgeDay (growth rate)
    PodcasterAgeDay = days since verifiedAt
    MG = MaxGrowth (cached, temporal 7 days)
    
    RT = AverageRating × log(RatingCount + 1) (rating term)
    MRT = MaxRatingTerm (cached, all-time)

Step 3: Ranking
  Sort: rookieScore DESC
  Limit: 8 podcasters

Step 4: For Each Podcaster
  Fetch:
    - Profile info (name, avatar, description)
    - Top show (by popularScore)
    - Stats: TotalFollow, AverageRating
```

**Cache Requirements:**

**Temporal Cache (7 days):**
- ✅ MTLS (MaxTotalListenSession)
- ✅ MG (MaxGrowth rate)

**All-time Cache:**
- ✅ MRT (MaxRatingTerm)

**Fallback:**
```
IF no rookies found (very rare):
  → Hide this section
  OR
  → Show "New Podcasters" with recent verified (no score calculation)
```

**Refresh Frequency:** Every 3 days
- Give users time to explore and listen
- Rookies don't change that fast

**Time Range Configuration:**
```
minMediumRangeContentBehaviorLookbackDayCount: 7 days (for TLS)
minExtraLongRangeContentBehaviorLookbackDayCount: 90 days (for filter)
```

**Implementation Notes:**
```sql
-- Find talented rookie podcasters
SELECT 
  pp.*,
  -- Calculate components
  (SELECT COUNT(*) FROM PodcastEpisodeListenSession pels
   JOIN PodcastEpisode pe ON pels.podcastEpisodeId = pe.id
   JOIN PodcastShow ps ON pe.podcastShowId = ps.id
   WHERE ps.podcasterId = pp.accountId
     AND pels.createdAt >= NOW() - INTERVAL 7 DAY
  ) as tls,
  
  pp.totalFollow / DATEDIFF(NOW(), pp.verifiedAt) as growth_rate,
  
  pp.averageRating * LOG(pp.ratingCount + 1) as rating_term

FROM PodcasterProfile pp
WHERE pp.verifiedAt >= NOW() - INTERVAL 90 DAY
  AND pp.isVerified = true
  AND pp.deactivatedAt IS NULL
  AND pp.violationLevel = 0
HAVING tls > 0  -- Must have some activity
ORDER BY 
  -- Calculate rookieScore (need to fetch cached max values)
  (tls / {MTLS}) * 0.4 + 
  (growth_rate / {MG}) * 0.4 + 
  (rating_term / {MRT}) * 0.2 DESC
LIMIT 8;
```

---

### 7. EXPLORE [RANDOM CATEGORY] 🎲

**Mục đích:** Escape filter bubble với category hoàn toàn mới

**Dynamic Section Title:**
```
Pattern: "Explore {Category Name}"
Examples:
  - "Explore Horror"
  - "Explore Business"
  - "Explore Comedy"

Logic:
  - Pick random category EXCLUDING user's top 2 categories
  - Keep same category for 3 days (stored in user session)
```

**Query Logic:**
```
Mix Ratio: 70% popular + 30% trending
Total Output: 10 shows

Step 1: Select Random Category
  Logic:
    1. Get user's top 2 categories from cache
    2. Get all categories in system
    3. Exclude user's top 2
    4. Random select with weighted probability:
       
       weight = log(category_show_count + 1) × category_popularity
       
       → Popular categories có higher weight (better content quality)
    
    5. Store selected categoryId in user session (3 days TTL)

Step 2: Part A (70% - 7 shows)
  Query Type: categoryId={selectedCategory} + popular
  Formula:
    popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)
  
  Steps:
    1. Filter shows in selected category
    2. Calculate popularScore for each
    3. Sort DESC
    4. Return top 7

Step 3: Part B (30% - 3 shows)
  Query Type: categoryId={selectedCategory} + hotRecently
  Formula:
    hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)
  
  Steps:
    1. Filter shows in selected category
    2. Calculate hotScore (7 days) for each
    3. Sort DESC
    4. Pick top 3
    5. Exclude any overlap with Part A
  
Final Step:
  Interleave(Part A, Part B) → return 10 shows
```

**Cache Requirements:**
- ✅ `user_listening_preferences.top_categories_30d` (for exclusion)
- ✅ System cache all-time: MTF, MLC, MRT
- ✅ System cache temporal (7d): MNLS, MNF
- ✅ `user_exploration_category_3d` (track current random category)

**Category Selection Strategy:**
```javascript
async function selectRandomCategory(userId, userTopCategories) {
  // Get all categories with show counts
  const allCategories = await db.query(`
    SELECT 
      pc.id,
      pc.name,
      COUNT(ps.id) as show_count,
      SUM(ps.listenCount) as total_listens
    FROM PodcastCategory pc
    LEFT JOIN PodcastShow ps ON ps.podcastCategoryId = pc.id
    WHERE pc.id NOT IN (${userTopCategories.join(',')})
    GROUP BY pc.id
    HAVING show_count >= 5  -- Minimum 5 shows
  `);
  
  // Calculate weights
  const weighted = allCategories.map(cat => ({
    ...cat,
    weight: Math.log(cat.show_count + 1) * Math.log(cat.total_listens + 1)
  }));
  
  // Weighted random selection
  return weightedRandom(weighted);
}
```

**Fallback:**
```
IF user is new (no top categories):
  → Skip exclusion logic
  → Random from top 5 most popular categories
```

**Refresh Frequency:** Every 3 days
- Same category stays for 3 days
- Gives user time to explore thoroughly
- Prevents confusion from too frequent changes

**Time Range Configuration:**
```
minMediumRangeContentBehaviorLookbackDayCount: 7 days (for hot score)
```

**Implementation Notes:**
```javascript
// Check if need to refresh random category
async function getExploreCategory(userId) {
  const cached = await redis.get(`explore_category:${userId}`);
  
  if (cached && cached.expiry > Date.now()) {
    return cached.categoryId;
  }
  
  // Select new random category
  const userPrefs = await getUserPreferences(userId);
  const topCategories = userPrefs?.top_categories_30d?.map(c => c.categoryId) || [];
  
  const newCategory = await selectRandomCategory(userId, topCategories.slice(0, 2));
  
  // Cache for 3 days
  await redis.setex(
    `explore_category:${userId}`,
    3 * 24 * 60 * 60,
    JSON.stringify({
      categoryId: newCategory.id,
      categoryName: newCategory.name,
      expiry: Date.now() + (3 * 24 * 60 * 60 * 1000)
    })
  );
  
  return newCategory.id;
}
```

---

### 8. TOP PODCASTERS 👥

**Mục đích:** Showcase top creators của platform

**Query Logic:**
```
Mix Ratio: 60% all-time legends + 40% rising stars
Total Output: 12 podcasters

Part A (60% - 7-8 podcasters):
  Query Type: popular (podcasters)
  Formula:
    popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.15×(RT/MRT) + 0.05×(Age/MaxAge)
  
  Where:
    TF = TotalFollow của podcaster
    MTF = MaxTotalFollow (cached, all-time)
    
    LC = ListenCount của podcaster (all shows)
    MLC = MaxListenCount (cached, all-time)
    
    RT = AverageRating × log(RatingCount + 1)
    MRT = MaxRatingTerm (cached, all-time)
    
    Age = Days since verifiedAt
    MaxAge = Longest verified podcaster age (cached, all-time)
  
  Steps:
    1. Filter: isVerified = true, deactivatedAt = null, violationLevel = 0
    2. Calculate popularScore for all
    3. Sort DESC
    4. Return top 7-8

Part B (40% - 4-5 podcasters):
  Query Type: hotRecently (podcasters)
  Formula:
    hotScore = 0.5×(NLS/MNLS) + 0.3×(NF/MNF) + 0.15×(G/2MG) + 0.05×(Rating/5)
  
  Where:
    NLS = New ListenSession trong 7 ngày
    MNLS = MaxNewListenSession 7d (cached, temporal)
    
    NF = New Follow trong 7 ngày
    MNF = MaxNewFollow 7d (cached, temporal)
    
    G = Growth = NLS + NF (combined growth)
    MG = MaxGrowth 7d (cached, temporal)
    
    Rating = AverageRating (normalized to 0-1)
  
  Steps:
    1. Same filter as Part A
    2. Calculate hotScore for all
    3. Sort DESC
    4. Pick top 4-5
    5. Exclude any overlap with Part A
  
Final Step:
  Interleave(Part A, Part B) → return 12 podcasters
  Pattern: [A,A,B,A,A,B,A,A,B,A,B,B]
```

**Cache Requirements:**

**All-time Cache:**
- ✅ MTF (MaxTotalFollow)
- ✅ MLC (MaxListenCount)
- ✅ MRT (MaxRatingTerm)
- ✅ MaxAge (Max podcaster age in days)

**Temporal Cache (7 days):**
- ✅ MNLS (MaxNewListenSession)
- ✅ MNF (MaxNewFollow)
- ✅ MG (MaxGrowth = max of NLS+NF)

**Display Format:**
```
Podcaster Card:
  ├─ Avatar (large, circular)
  ├─ Name + Verified badge
  ├─ Top Show
  │   ├─ Show cover image
  │   ├─ Show title
  │   └─ Brief description (1 line)
  ├─ Stats
  │   ├─ TotalFollow (formatted: 1.2K, 45K, etc.)
  │   └─ AverageRating (stars: ⭐⭐⭐⭐⭐)
  └─ "View Profile" button
```

**Fallback:** Không cần (popular podcasters luôn có)

**Refresh Frequency:** Weekly
- Popular creators thay đổi rất chậm
- All-time metrics stable
- Update every Monday 00:00

**Time Range Configuration:**
```
minMediumRangeUserBehaviorLookbackDayCount: 7 days (for hot score)
```

**Implementation Notes:**
```sql
-- Part A: Popular Podcasters (All-time Legends)
SELECT 
  pp.*,
  pp.totalFollow / {MTF} * 0.4 +
  pp.listenCount / {MLC} * 0.4 +
  (pp.averageRating * LOG(pp.ratingCount + 1)) / {MRT} * 0.15 +
  DATEDIFF(NOW(), pp.verifiedAt) / {MaxAge} * 0.05 as popularScore
FROM PodcasterProfile pp
WHERE pp.isVerified = true
  AND pp.deactivatedAt IS NULL
  AND pp.violationLevel = 0
ORDER BY popularScore DESC
LIMIT 8;

-- Part B: Hot Podcasters (Rising Stars in 7 days)
WITH RecentActivity AS (
  SELECT 
    ps.podcasterId,
    COUNT(DISTINCT pels.id) as new_sessions,
    (SELECT COUNT(*) FROM AccountFollowedPodcaster afp
     WHERE afp.podcasterId = ps.podcasterId
       AND afp.createdAt >= NOW() - INTERVAL 7 DAY
    ) as new_follows
  FROM PodcastShow ps
  JOIN PodcastEpisode pe ON pe.podcastShowId = ps.id
  JOIN PodcastEpisodeListenSession pels ON pels.podcastEpisodeId = pe.id
  WHERE pels.createdAt >= NOW() - INTERVAL 7 DAY
  GROUP BY ps.podcasterId
)
SELECT 
  pp.*,
  ra.new_sessions / {MNLS} * 0.5 +
  ra.new_follows / {MNF} * 0.3 +
  (ra.new_sessions + ra.new_follows) / (2 * {MG}) * 0.15 +
  pp.averageRating / 5 * 0.05 as hotScore
FROM PodcasterProfile pp
JOIN RecentActivity ra ON ra.podcasterId = pp.accountId
WHERE pp.isVerified = true
  AND pp.deactivatedAt IS NULL
  AND pp.violationLevel = 0
  AND pp.accountId NOT IN ({Part A podcaster IDs})
ORDER BY hotScore DESC
LIMIT 5;
```

---

## CACHE ARCHITECTURE

### System-Wide Cache (Background Job)

**Structure:**
```json
{
  "podcast_max_values": {
    "all_time": {
      "podcaster": {
        "MTF": 100000,
        "MLC": 5000000,
        "MRT": 4.5,
        "MaxAge": 1825
      },
      "show": {
        "MTF": 80000,
        "MLC": 3000000,
        "MRT": 4.2
      },
      "channel": {
        "MTLS": 4500000,
        "MTFa": 50000
      }
    },
    "temporal_7d": {
      "podcaster": {
        "MNLS": 50000,
        "MNF": 10000,
        "MG": 137.5,
        "MTLS": 45000
      },
      "show": {
        "MNLS": 40000,
        "MNF": 8000
      },
      "channel": {
        "MNLS": 35000,
        "MNFa": 5000
      }
    },
    "last_updated": "2025-11-08T00:00:00Z",
    "next_update": "2025-11-09T00:00:00Z"
  }
}
```

**Background Job Schedule:**
```
All-time Max Values:
  - Frequency: Daily at 00:00
  - Job: calculate_max_all_time_values()
  - Duration: ~10-15 minutes
  - Priority: Medium

Temporal Max Values (7 days):
  - Frequency: Every 12 hours (00:00, 12:00)
  - Job: calculate_max_temporal_values()
  - Duration: ~5-10 minutes
  - Priority: High (affects trending)
```

**Calculation Logic:**
```sql
-- Example: Calculate MTF (MaxTotalFollow for Podcasters)
UPDATE podcast_cache
SET value = (
  SELECT MAX(totalFollow) 
  FROM PodcasterProfile
  WHERE isVerified = true
    AND deactivatedAt IS NULL
    AND violationLevel = 0
)
WHERE key = 'podcaster.MTF'
  AND scope = 'all_time';

-- Example: Calculate MNLS (MaxNewListenSession 7d for Shows)
UPDATE podcast_cache
SET value = (
  SELECT MAX(session_count)
  FROM (
    SELECT 
      ps.id as show_id,
      COUNT(DISTINCT pels.id) as session_count
    FROM PodcastShow ps
    JOIN PodcastEpisode pe ON pe.podcastShowId = ps.id
    JOIN PodcastEpisodeListenSession pels ON pels.podcastEpisodeId = pe.id
    WHERE pels.createdAt >= NOW() - INTERVAL 7 DAY
    GROUP BY ps.id
  ) AS show_sessions
)
WHERE key = 'show.MNLS'
  AND scope = 'temporal_7d';
```

### User-Specific Cache

**Structure:**
```json
{
  "user_123_preferences": {
    "top_categories_30d": [
      {
        "categoryId": 1,
        "categoryName": "True Crime",
        "subCategoryIds": [2, 5],
        "subCategoryNames": ["Serial Killers", "White Collar Crime"],
        "listen_count": 450,
        "percentage": 35.5
      },
      {
        "categoryId": 3,
        "categoryName": "Horror",
        "subCategoryIds": [8],
        "subCategoryNames": ["Urban Legends"],
        "listen_count": 320,
        "percentage": 25.2
      }
    ],
    "top_podcasters_30d": [
      {
        "podcasterId": 456,
        "podcasterName": "John Doe",
        "listen_count": 120,
        "percentage": 15.8
      },
      {
        "podcasterId": 789,
        "podcasterName": "Jane Smith",
        "listen_count": 95,
        "percentage": 12.5
      }
    ],
    "total_listen_count_30d": 1267,
    "last_updated": "2025-11-08T00:00:00Z",
    "lookback_days": 30
  }
}
```

**User Cache Update Schedule:**
```
Frequency: Daily at 00:00
Trigger: Background job OR on-demand when user activity spike
TTL: 7 days (auto-expire if user inactive)
```

**Calculation Logic:**
```sql
-- Calculate user's top categories in last 30 days
INSERT INTO user_preferences_cache (userId, key, value, updated_at)
SELECT 
  pels.accountId as userId,
  'top_categories_30d' as key,
  JSON_ARRAYAGG(
    JSON_OBJECT(
      'categoryId', pc.id,
      'categoryName', pc.name,
      'subCategoryIds', sub_cats,
      'listen_count', category_listens,
      'percentage', (category_listens / total_listens * 100)
    )
    ORDER BY category_listens DESC
    LIMIT 4
  ) as value,
  NOW() as updated_at
FROM PodcastEpisodeListenSession pels
JOIN PodcastEpisode pe ON pels.podcastEpisodeId = pe.id
JOIN PodcastShow ps ON pe.podcastShowId = ps.id
JOIN PodcastCategory pc ON ps.podcastCategoryId = pc.id
WHERE pels.createdAt >= NOW() - INTERVAL 30 DAY
GROUP BY pels.accountId
ON DUPLICATE KEY UPDATE 
  value = VALUES(value),
  updated_at = VALUES(updated_at);
```

### Session Cache (Temporary)

**Purpose:** Store temporary exploration state

**Structure:**
```json
{
  "user_123_exploration": {
    "random_category": {
      "categoryId": 5,
      "categoryName": "Comedy",
      "selected_at": "2025-11-08T10:30:00Z",
      "expires_at": "2025-11-11T10:30:00Z"
    }
  }
}
```

**TTL:** 3 days
**Storage:** Redis (fast access)

---

## REFRESH STRATEGY

### Overview Table

| Section | Refresh Frequency | Reason | Cache Dependency |
|---------|------------------|---------|------------------|
| 1. Continue Listening | Realtime | User behavior changes instantly | User listen session |
| 2. Based on Your Taste | Daily (00:00) | User taste evolves slowly | User prefs + System max |
| 3. New Releases | Every 6h | New content published frequently | User prefs + Time |
| 4. Hot This Week | Daily (00:00) | Trending shifts daily | System temporal 7d |
| 5. Top SubCategory | Every 2 days | Balance stability & freshness | User prefs + System max |
| 6. Talented Rookies | Every 3 days | Give time to explore | System temporal 7d |
| 7. Explore Random | Every 3 days | Serendipity needs time | Session cache |
| 8. Top Podcasters | Weekly (Mon 00:00) | Popular changes very slowly | System all-time |

### Refresh Implementation

**1. Real-time Sections:**
```javascript
// No caching, query on-demand
async function getContinueListening(userId) {
  return await db.query(`
    SELECT * FROM PodcastEpisodeListenSession
    WHERE accountId = ${userId}
      AND isCompleted = 0
    ORDER BY createdAt DESC
    LIMIT 10
  `);
}
```

**2. Daily Refresh Sections:**
```javascript
// Cache with 24h TTL
async function getBasedOnTaste(userId) {
  const cacheKey = `discovery:taste:${userId}`;
  let cached = await redis.get(cacheKey);
  
  if (cached) {
    return JSON.parse(cached);
  }
  
  // Calculate fresh data
  const result = await calculateBasedOnTaste(userId);
  
  // Cache for 24 hours
  await redis.setex(cacheKey, 24 * 60 * 60, JSON.stringify(result));
  
  return result;
}
```

**3. Multi-day Refresh Sections:**
```javascript
// Cache with custom TTL
async function getExploreRandom(userId) {
  const cacheKey = `discovery:explore:${userId}`;
  let cached = await redis.get(cacheKey);
  
  if (cached) {
    return JSON.parse(cached);
  }
  
  // Calculate fresh data
  const result = await calculateExploreRandom(userId);
  
  // Cache for 3 days
  await redis.setex(cacheKey, 3 * 24 * 60 * 60, JSON.stringify(result));
  
  return result;
}
```

### Cache Invalidation Rules

**Force Refresh Triggers:**

1. **User performs significant action:**
   - Subscribes to new show
   - Listens to 5+ episodes in new category
   - Follows new podcaster
   → Invalidate user preference cache

2. **System detects anomaly:**
   - Viral content spike
   - New podcaster with abnormal growth
   → Invalidate temporal max cache

3. **Manual admin action:**
   - Featured content promotion
   - Category restructuring
   → Invalidate specific section cache

**Implementation:**
```javascript
// Event listener for cache invalidation
eventBus.on('user.subscribed', async (event) => {
  const { userId, showId } = event;
  
  // Invalidate affected caches
  await redis.del(`discovery:taste:${userId}`);
  await redis.del(`discovery:subcategory:${userId}`);
  await redis.del(`user_preferences:${userId}`);
  
  // Trigger background recalculation
  await queue.add('recalculate_user_prefs', { userId });
});
```

---

## CÔNG THỨC TÍNH ĐIỂM

### Popular Score (All-time)

**For Shows:**
```
popularScore = 0.4 × (TF/MTF) + 0.4 × (LC/MLC) + 0.2 × (RT/MRT)

Where:
- TF = TotalFollow của show
- MTF = MaxTotalFollow (system max)
- LC = ListenCount của show (all-time)
- MLC = MaxListenCount (system max)
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm (system max)

Range: 0 to 1
Higher = More popular all-time
```

**For Channels:**
```
popularScore = 0.6 × (TLS/MTLS) + 0.4 × (TFa/MTFa)

Where:
- TLS = Total ListenSession all-time của channel
- MTLS = MaxTotalListenSession (system max)
- TFa = TotalFavorite của channel
- MTFa = MaxTotalFavorite (system max)

Range: 0 to 1
Higher = More popular all-time
```

**For Podcasters:**
```
popularScore = 0.4 × (TF/MTF) + 0.4 × (LC/MLC) + 0.15 × (RT/MRT) + 0.05 × (Age/MaxAge)

Where:
- TF = TotalFollow của podcaster
- MTF = MaxTotalFollow (system max)
- LC = ListenCount của podcaster (sum of all shows)
- MLC = MaxListenCount (system max)
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm (system max)
- Age = Days since verifiedAt
- MaxAge = Max podcaster age (system max)

Range: 0 to 1
Higher = More established and popular
```

### Hot Score (Temporal - 7 days)

**For Shows:**
```
hotScore = 0.6 × (NLS/MNLS) + 0.4 × (NF/MNF)

Where:
- NLS = New ListenSession trong 7 ngày gần đây
- MNLS = MaxNewListenSession 7d (system max)
- NF = New Follow trong 7 ngày gần đây
- MNF = MaxNewFollow 7d (system max)

Range: 0 to 1
Higher = More trending recently
```

**For Channels:**
```
hotScore = 0.6 × (NLS/MNLS) + 0.4 × (NFa/MNFa)

Where:
- NLS = New ListenSession trong 7 ngày
- MNLS = MaxNewListenSession 7d (system max)
- NFa = New Favorite trong 7 ngày
- MNFa = MaxNewFavorite 7d (system max)

Range: 0 to 1
Higher = More trending recently
```

**For Podcasters:**
```
hotScore = 0.5 × (NLS/MNLS) + 0.3 × (NF/MNF) + 0.15 × (G/2MG) + 0.05 × (Rating/5)

Where:
- NLS = New ListenSession trong 7 ngày
- MNLS = MaxNewListenSession 7d (system max)
- NF = New Follow trong 7 ngày
- MNF = MaxNewFollow 7d (system max)
- G = Growth = NLS + NF (combined momentum)
- MG = MaxGrowth 7d (system max)
- Rating = AverageRating (current)

Range: 0 to ~1
Higher = More momentum recently
```

### Rookie Score (For Talented Rookies)

```
rookieScore = 0.4 × (TLS/MTLS) + 0.4 × (G/MG) + 0.2 × (RT/MRT)

Where:
- TLS = Total ListenSession trong 7 ngày
- MTLS = MaxTotalListenSession 7d (system max)
- G = Growth rate = TotalFollow / PodcasterAgeDay
- MG = MaxGrowth rate (system max)
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm (system max)

Constraints:
- Only for podcasters verified within last 90 days
- Must have PodcasterAgeDay > 0

Range: 0 to 1
Higher = Faster growing rookie with quality
```

### Personal Score (For Category Personalization)

```
personalScore = 0.6 × userEngagement + 0.4 × showQuality

Where:
- userEngagement = episodes_listened_count / total_episodes_in_show
  (Normalized to 0-1)
  
- showQuality = 0.5 × (min(totalFollow, 100000) / 100000) + 
                0.5 × (averageRating / 5)
  (Normalized to 0-1, cap follow at 100K to prevent extreme outliers)

Range: 0 to 1
Higher = Better fit for user's taste
```

### Formula Notes

**Why log(RatingCount + 1)?**
- Prevents zero division
- Diminishing returns: 10→20 ratings matters more than 1000→1010
- Rewards shows with many ratings but not disproportionately

**Why normalize with max values?**
- Brings all metrics to 0-1 scale
- Prevents one metric from dominating
- Allows weighted combination

**Why different weights?**
- Reflect relative importance
- Popular: Follow + Listen matter most (0.4 each)
- Hot: Recent activity matters most (0.6)
- Rookie: Balance growth with activity

---

## COLD-START STRATEGY

### Problem Definition

**Cold-Start Users:** Users with insufficient listening history
- No episodes listened
- < 5 episodes listened
- No category preference established

**Impact:** Cannot calculate personalized scores

### Solution by Section

**1. Continue Listening**
```
Problem: No listen history
Solution: Hide section entirely
Fallback: None (N/A for new users)
```

**2. Based on Your Taste**
```
Problem: No category preference
Solution: 100% popular shows
Fallback Logic:
  - Query: popular (global)
  - Sort: popularScore DESC
  - Limit: 12 shows
Note: No personalization until 5+ episodes listened
```

**3. New Releases**
```
Problem: No interested podcasters
Solution: 100% system-wide new
Fallback Logic:
  - Query: new (global, last 2 days)
  - Sort: releaseDate DESC
  - Limit: 10 shows
Note: Still provides value (discovery of new content)
```

**4. Hot This Week**
```
Problem: None (works without user history)
Solution: No fallback needed
Logic: Always show trending content
```

**5. Top SubCategory**
```
Problem: No category preference
Solution: Use trending category
Fallback Logic:
  - Pick: Most listened category in last 7 days (system-wide)
  - Query: subCategoryId of top subcategory in that category
  - Mix: 70% popular + 30% hot in that subcategory
Note: Exposes user to popular content in trending category
```

**6. Talented Rookies**
```
Problem: None (works without user history)
Solution: No fallback needed
Logic: Always show talented new podcasters
```

**7. Explore Random**
```
Problem: Cannot exclude user's categories (none yet)
Solution: Pure random selection
Fallback Logic:
  - Pick: Random from top 5 popular categories
  - Query: Mix popular + hot in that category
  - Keep: Same for 3 days
Note: Helps establish initial preferences
```

**8. Top Podcasters**
```
Problem: None (works without user history)
Solution: No fallback needed
Logic: Always show platform's top creators
```

### Progressive Personalization

**As user listens to more content:**

```
Stage 1: 0 episodes
└─ All sections use fallback (popular/trending content)

Stage 2: 1-4 episodes
├─ Section 1: May appear (if episode not finished)
├─ Section 2: Still using fallback (need more data)
├─ Section 3: Still using fallback
├─ Section 5: Still using fallback
└─ Others: Working normally

Stage 3: 5-10 episodes
├─ Section 1: Working
├─ Section 2: Start personalization (weak signal)
│   ├─ 50% based on initial preference
│   └─ 50% popular
├─ Section 3: Start personalization
│   └─ May have 1 interested podcaster
├─ Section 5: Start personalization
│   └─ Initial category preference emerging
└─ Others: Working normally

Stage 4: 10+ episodes
└─ All sections fully personalized
```

### Engagement Triggers

**Track user progression:**
```javascript
const userStage = {
  cold: 0,           // 0 episodes
  warming: 1,        // 1-4 episodes
  warm: 2,           // 5-10 episodes
  engaged: 3         // 10+ episodes
};

function getUserStage(listenCount) {
  if (listenCount === 0) return userStage.cold;
  if (listenCount < 5) return userStage.warming;
  if (listenCount < 10) return userStage.warm;
  return userStage.engaged;
}
```

**Adaptive UI:**
```javascript
// Show helpful hints for cold-start users
if (stage === userStage.cold) {
  showOnboardingBanner(
    "Listen to 5+ episodes to get personalized recommendations!"
  );
}

// Highlight exploration sections for new users
if (stage < userStage.warm) {
  highlightSections([
    'Hot This Week',
    'Talented Rookies',
    'Explore Random'
  ]);
}
```

### Analytics Tracking

**Monitor cold-start effectiveness:**
```javascript
// Track which sections drive initial engagement
analytics.track('discovery_section_click', {
  userId: userId,
  sectionName: 'New Releases',
  userStage: getUserStage(listenCount),
  isFallback: true,
  position: 3
});

// Track progression through stages
analytics.track('user_stage_progression', {
  userId: userId,
  fromStage: 'warming',
  toStage: 'warm',
  episodesListened: 5,
  daysSinceSignup: 3
});
```

---

## IMPLEMENTATION CHECKLIST

### Backend Requirements

**1. Database Indexes:**
```sql
-- Critical indexes for performance
CREATE INDEX idx_listen_session_account_created 
  ON PodcastEpisodeListenSession(accountId, createdAt);

CREATE INDEX idx_listen_session_created_category 
  ON PodcastEpisodeListenSession(createdAt, podcastCategoryId);

CREATE INDEX idx_show_category_status 
  ON PodcastShow(podcastCategoryId, status, publishedAt);

CREATE INDEX idx_podcaster_verified_status 
  ON PodcasterProfile(verifiedAt, isVerified, deactivatedAt);

CREATE INDEX idx_follow_created 
  ON AccountFollowedPodcastShow(createdAt);
  
CREATE INDEX idx_favorite_created 
  ON AccountFavoritedPodcastChannel(createdAt);
```

**2. Cache Tables:**
```sql
-- System-wide max values cache
CREATE TABLE podcast_cache (
  id INT AUTO_INCREMENT PRIMARY KEY,
  entity_type ENUM('podcaster', 'show', 'channel') NOT NULL,
  metric_name VARCHAR(50) NOT NULL,
  scope ENUM('all_time', 'temporal_7d') NOT NULL,
  value DECIMAL(18,2) NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  INDEX idx_cache_lookup (entity_type, metric_name, scope)
);

-- User preferences cache
CREATE TABLE user_preferences_cache (
  userId INT NOT NULL,
  cache_key VARCHAR(100) NOT NULL,
  cache_value JSON NOT NULL,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  expires_at TIMESTAMP NULL,
  PRIMARY KEY (userId, cache_key),
  INDEX idx_expiry (expires_at)
);
```

**3. Background Jobs:**
```javascript
// Job 1: Update system max cache (daily)
scheduler.schedule('0 0 * * *', async () => {
  await updateAllTimeMaxValues();
  await updateTemporalMaxValues();
});

// Job 2: Update temporal cache (every 12h)
scheduler.schedule('0 0,12 * * *', async () => {
  await updateTemporalMaxValues();
});

// Job 3: Update user preferences (daily)
scheduler.schedule('0 1 * * *', async () => {
  await updateUserPreferences();
});

// Job 4: Cleanup expired cache (daily)
scheduler.schedule('0 2 * * *', async () => {
  await cleanupExpiredCache();
});
```

**4. API Endpoints:**
```
GET /api/discovery/sections
  → Returns all 8 sections for a user
  → Handles fallbacks for cold-start

GET /api/discovery/section/{sectionId}
  → Returns specific section
  → Supports pagination

POST /api/discovery/refresh
  → Forces refresh of user's discovery cache
  → Use after significant user actions
```

### Frontend Requirements

**1. UI Components:**
- Section header with refresh button
- Show card with lazy loading
- Podcaster card with hover effects
- Empty state for cold-start
- Loading skeletons

**2. Infinite Scroll:**
```javascript
// Load more items when scrolling near bottom
const observer = new IntersectionObserver((entries) => {
  if (entries[0].isIntersecting) {
    loadMoreItems(sectionId);
  }
});
```

**3. Cache Management:**
```javascript
// Client-side cache with TTL
const clientCache = new Map();

async function getDiscoverySection(sectionId) {
  const cached = clientCache.get(sectionId);
  
  if (cached && Date.now() < cached.expiry) {
    return cached.data;
  }
  
  const data = await api.get(`/discovery/section/${sectionId}`);
  
  clientCache.set(sectionId, {
    data,
    expiry: Date.now() + getSectionTTL(sectionId)
  });
  
  return data;
}
```

### Monitoring & Alerts

**1. Performance Metrics:**
```
- Cache hit rate (target: >90%)
- API response time (target: <200ms)
- Background job duration (target: <15min)
- Database query time (target: <50ms)
```

**2. Business Metrics:**
```
- Click-through rate per section
- User engagement by stage
- Section effectiveness (clicks → listens)
- Personalization coverage (% using personalized vs fallback)
```

**3. Alerts:**
```javascript
// Alert if cache miss rate too high
if (cacheMissRate > 0.2) {
  alert('Discovery cache miss rate above 20%');
}

// Alert if background job fails
if (jobStatus === 'failed') {
  alert('Discovery cache update job failed');
}
```

---

## APPENDIX

### Time Range Constants
```
minShortRangeUserBehaviorLookbackDayCount: 2 days
minMediumRangeUserBehaviorLookbackDayCount: 7 days
minLongRangeUserBehaviorLookbackDayCount: 30 days

minShortRangeContentBehaviorLookbackDayCount: 2 days
minMediumRangeContentBehaviorLookbackDayCount: 7 days
minLongRangeContentBehaviorLookbackDayCount: 30 days
minExtraLongRangeContentBehaviorLookbackDayCount: 90 days
```

### Metric Abbreviations Reference
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

### Version History
```
v1.0 - 2025-11-08: Initial design
- 8 discovery sections defined
- Cache architecture established
- Formulas finalized
```

---

**END OF DOCUMENT**
