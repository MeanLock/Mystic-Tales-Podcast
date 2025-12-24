# CATEGORY BASE - QUERY SCENARIO

## OVERVIEW

Category Base Feed hiển thị toàn bộ nội dung của 1 category cụ thể: Channels, Shows (Hot + Popular), Episodes, và các Subcategory sections. 

**Key Characteristics:**
- **Category-scoped**: Tất cả content phải belong to target category
- **Fixed Sections**: 4 main sections (TopChannels, TopShows, HotShows, TopEpisodes)
- **Dynamic Subcategories**: N subcategory sections (depends on category)
- **Partial Deduplication**: Only SubcategorySections exclude TopShows + HotShows
- **Anonymous-friendly**: Không phụ thuộc user authentication

---

## SECTION ORDER & LAYOUT

```
Section Order:
1. Category Info (PodcastCategoryDTO)
2. Top Channels (16 channels)
3. Top Shows (24 shows)
4. Hot Shows (20 shows)
5. Top Episodes (30 episodes)
6. Subcategory Sections (N sections, 20 shows each)

Total Fixed: 90 items (16 channels + 24 shows + 20 shows + 30 episodes)
Total Dynamic: 20N shows (N = number of subcategories)
```

---

## DEDUPLICATION STRATEGY

### Deduplication Rules:

**Fixed Sections (1-5):**
- **NO deduplication** between TopChannels, TopShows, HotShows, TopEpisodes
- Each section is completely independent

**Subcategory Sections (6):**
- **Exclude shows** from TopShows + HotShows
- Shows trong SubcategorySections KHÔNG được trùng với TopShows + HotShows
- Logic:
  ```csharp
  var dedupShowIds = topShows.Concat(hotShows).Select(s => s.Id).ToHashSet();
  Query condition: !dedupShowIds.Contains(s.Id)
  ```

### Deduplication Priority:
1. TopShows (24 shows) - Collected for dedup
2. HotShows (20 shows) - Collected for dedup
3. SubcategorySections (20N shows) - Exclude dedupShowIds

---

## SECTION 1: CATEGORY INFO

**Output:**
```typescript
PodcastCategoryDTO {
  Id: number;
  Name: string;
  MainImageFileKey: string;
}
```

**Validation:**
- Category.Id must exist in database
- Throw Exception if category not found

---

## SECTION 2: TOP CHANNELS 📺

**Target:** 16 channels

**Query Logic:**

**Step 1: Query All Channels**
```sql
WHERE pc.DeletedAt == NULL
```

**Include:**
- PodcastChannelStatusTrackings
- PodcastShows → PodcastShowStatusTrackings
- PodcastShows → PodcastSubCategory

**Step 2: Filter Channels with Shows in Target Category**
```csharp
Channel conditions:
- Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published

Has at least 1 Published show in target category:
- Show.DeletedAt == NULL
- Latest PodcastShowStatusTracking.PodcastShowStatusId == Published
- Show.PodcastSubCategory.PodcastCategoryId == targetCategoryId
```

**Step 3: Calculate Popular Score**
```
popularScore = 0.6 × (LC/MLC) + 0.4 × (TF/MTF)

Where:
- LC = Channel.ListenCount
- MLC = MaxListenCount (from query:metric:channel:all_time_max)
- TF = Channel.TotalFavorite
- MTF = MaxTotalFavorite (from query:metric:channel:all_time_max)
```

**Step 4: Sort & Limit**
```csharp
Sort: popularScore DESC
Take: 16 channels
```

**Cache Key:** `query:metric:channel:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 3: TOP SHOWS 🎙️

**Target:** 24 shows

**Query Logic:**

**Step 1: Query All Shows**
```sql
WHERE ps.DeletedAt == NULL
```

**Include:**
- PodcastShowStatusTrackings
- PodcastSubCategory
- PodcastChannel → PodcastChannelStatusTrackings

**Step 2: Filter Shows in Target Category**
```csharp
Show conditions:
- Show.PodcastSubCategory.PodcastCategoryId == targetCategoryId
- Latest PodcastShowStatusTracking.PodcastShowStatusId == Published

Channel conditions (if show has channel):
- Channel.DeletedAt == NULL
- Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Step 3: Calculate Popular Score**
```
popularScore = 0.4 × (TF/MTF) + 0.4 × (LC/MLC) + 0.2 × (RT/MRT)

Where:
- TF = Show.TotalFollow
- MTF = MaxTotalFollow (from query:metric:show:all_time_max)
- LC = Show.ListenCount
- MLC = MaxListenCount (from query:metric:show:all_time_max)
- RT = Show.AverageRating × log(Show.RatingCount + 1)
- MRT = MaxRatingTerm (from query:metric:show:all_time_max)
```

**Step 4: Sort & Limit**
```csharp
Sort: popularScore DESC
Take: 24 shows
```

**Cache Key:** `query:metric:show:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 4: HOT SHOWS 🔥

**Target:** 20 shows

**Query Logic:**

**Step 1: Query All Shows**
```sql
WHERE ps.DeletedAt == NULL
```

**Include:**
- PodcastShowStatusTrackings
- PodcastSubCategory
- PodcastChannel → PodcastChannelStatusTrackings

**Step 2: Filter Shows in Target Category**
```csharp
Show conditions:
- Show.PodcastSubCategory.PodcastCategoryId == targetCategoryId
- Latest PodcastShowStatusTracking.PodcastShowStatusId == Published

Channel conditions (if show has channel):
- Channel.DeletedAt == NULL
- Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Step 3: Calculate Hot Score (7 days)**
```
hotScore = 0.6 × (NLS/MNLS) + 0.4 × (NF/MNF)

Where:
- NLS = NewListenSession (last 7 days)
  - Query PodcastEpisodeListenSession where:
    - CreatedAt >= NOW - 7 days
    - IsContentRemoved == false
    - Episode.DeletedAt == NULL
    - Episode.Show.DeletedAt == NULL
    - Episode.ShowId IN (category shows)
  - Filter by Episode Latest Status == Published
  - Group by ShowId, Count sessions

- NF = NewFollow (last 7 days)
  - Query AccountFollowedPodcastShow from UserService where:
    - CreatedAt >= NOW - 7 days
    - ShowId IN (category shows)
  - Group by ShowId, Count follows

- MNLS = MaxNewListenSession (from query:metric:show:temporal_7d_max)
- MNF = MaxNewFollow (from query:metric:show:temporal_7d_max)
```

**Step 4: Sort & Limit**
```csharp
Sort: hotScore DESC
Take: 20 shows

IMPORTANT: NO filter hotScore > 0
Reason: Guarantee exactly 20 results even if all scores are 0
```

**Cache Keys:** 
- `query:metric:show:temporal_7d_max`

**Refresh:** Every 12 hours @ 00:00, 12:00 (CronExpression: `0 0 */12 * * *`)  
**TTL:** 12 hours (43200s)

---

## SECTION 5: TOP EPISODES 🎧

**Target:** 30 episodes

**Query Logic:**

**Step 1: Query All Episodes**
```sql
WHERE pe.DeletedAt == NULL
```

**Include:**
- PodcastEpisodeStatusTrackings
- PodcastShow → PodcastShowStatusTrackings
- PodcastShow → PodcastSubCategory
- PodcastShow → PodcastChannel → PodcastChannelStatusTrackings

**Step 2: Filter Episodes in Target Category**
```csharp
Episode conditions:
- Episode.PodcastShow != NULL
- Episode.PodcastShow.PodcastSubCategory.PodcastCategoryId == targetCategoryId
- Latest PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published

Show conditions:
- Latest PodcastShowStatusTracking.PodcastShowStatusId == Published

Channel conditions (if show has channel):
- Channel.DeletedAt == NULL
- Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Step 3: Calculate Popular Score**
```
popularScore = 0.6 × (LC/MLC) + 0.4 × (TS/MTS)

Where:
- LC = Episode.ListenCount
- MLC = MaxListenCount (from query:metric:episode:all_time_max)
- TS = Episode.TotalSave
- MTS = MaxTotalSave (from query:metric:episode:all_time_max)
```

**Step 4: Sort & Limit**
```csharp
Sort: popularScore DESC
Take: 30 episodes
```

**Cache Key:** `query:metric:episode:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SECTION 6: SUBCATEGORY SECTIONS 📂

**Target:** 20 shows per subcategory × N subcategories

**Dynamic Count:** N = số lượng subcategories thuộc target category

**Query Logic:**

**Step 1: Query All Subcategories**
```sql
WHERE psc.PodcastCategoryId == targetCategoryId
```

**Notes:**
- Subcategory entity has NO DeletedAt field
- All subcategories của category sẽ được include

**Step 2: For Each Subcategory, Build Section**

**Per Subcategory Query:**
```sql
WHERE ps.DeletedAt == NULL
  AND ps.PodcastSubCategoryId == subcategory.Id
  AND ps.Id NOT IN (dedupShowIds)
```

**Include:**
- PodcastShowStatusTrackings
- PodcastChannel → PodcastChannelStatusTrackings

**Deduplication:**
```csharp
var dedupShowIds = topShows.Concat(hotShows).Select(s => s.Id).ToHashSet();
Query condition: !dedupShowIds.Contains(ps.Id)
```

**Filter Shows:**
```csharp
Show conditions:
- Latest PodcastShowStatusTracking.PodcastShowStatusId == Published

Channel conditions (if show has channel):
- Channel.DeletedAt == NULL
- Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Calculate Popular Score:**
```
popularScore = 0.4 × (TF/MTF) + 0.4 × (LC/MLC) + 0.2 × (RT/MRT)

Where:
- TF = Show.TotalFollow
- MTF = MaxTotalFollow (from query:metric:show:all_time_max)
- LC = Show.ListenCount
- MLC = MaxListenCount (from query:metric:show:all_time_max)
- RT = Show.AverageRating × log(Show.RatingCount + 1)
- MRT = MaxRatingTerm (from query:metric:show:all_time_max)
```

**Sort & Limit:**
```csharp
Sort: popularScore DESC
Take: 20 shows per subcategory
```

**Output Structure:**
```typescript
List<SubCategoryCategoryBasePodcastFeedSection> {
  PodcastSubCategory: PodcastSubCategoryDTO {
    Id: number;
    Name: string;
    PodcastCategoryId: number;
  };
  ShowList: List<ShowListItemResponseDTO>; // 20 shows
}
```

**Cache Key:** `query:metric:show:all_time_max`  
**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

---

## SCORING FORMULAS

### Popular Score (All-time):

**Channels:**
```
popularScore = 0.6 × (LC/MLC) + 0.4 × (TF/MTF)

Where (from query:metric:channel:all_time_max):
- LC = ListenCount
- MLC = MaxListenCount
- TF = TotalFavorite
- MTF = MaxTotalFavorite
```

**Shows:**
```
popularScore = 0.4 × (TF/MTF) + 0.4 × (LC/MLC) + 0.2 × (RT/MRT)

Where (from query:metric:show:all_time_max):
- TF = TotalFollow
- MTF = MaxTotalFollow
- LC = ListenCount
- MLC = MaxListenCount
- RT = RatingTerm = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm
```

**Episodes:**
```
popularScore = 0.6 × (LC/MLC) + 0.4 × (TS/MTS)

Where (from query:metric:episode:all_time_max):
- LC = ListenCount
- MLC = MaxListenCount
- TS = TotalSave
- MTS = MaxTotalSave
```

### Hot Score (7 days):

**Shows:**
```
hotScore = 0.6 × (NLS/MNLS) + 0.4 × (NF/MNF)

Where (from query:metric:show:temporal_7d_max):
- NLS = NewListenSession (last 7 days)
- MNLS = MaxNewListenSession
- NF = NewFollow (last 7 days)
- MNF = MaxNewFollow
```

---

## VALIDATION CONDITIONS

### General Status Validation:

**Channel:**
```csharp
Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Show:**
```csharp
Latest PodcastShowStatusTracking.PodcastShowStatusId == Published
```

**Episode:**
```csharp
Latest PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published
```

**Channel Validation (for Shows/Episodes):**
```csharp
If show.PodcastChannel != NULL:
  - Channel.DeletedAt == NULL
  - Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

### Category Validation:

**For Channels:**
```csharp
Has at least 1 show where:
  - Show.PodcastSubCategory.PodcastCategoryId == targetCategoryId
  - Show published status valid
```

**For Shows:**
```csharp
Show.PodcastSubCategory.PodcastCategoryId == targetCategoryId
```

**For Episodes:**
```csharp
Episode.PodcastShow.PodcastSubCategory.PodcastCategoryId == targetCategoryId
```

**For Subcategories:**
```csharp
Subcategory.PodcastCategoryId == targetCategoryId
```

---

## CACHE DEPENDENCIES

### All-Time Max Values:

**Channel:**
- `query:metric:channel:all_time_max`
  - MaxListenCount
  - MaxTotalFavorite
- **Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)
- **TTL:** 24 hours (86400s)

**Show:**
- `query:metric:show:all_time_max`
  - MaxTotalFollow
  - MaxListenCount
  - MaxRatingTerm
- **Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)
- **TTL:** 24 hours (86400s)

**Episode:**
- `query:metric:episode:all_time_max`
  - MaxListenCount
  - MaxTotalSave
- **Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)
- **TTL:** 24 hours (86400s)

### Temporal Max Values:

**Show (7 days):**
- `query:metric:show:temporal_7d_max`
  - MaxNewListenSession
  - MaxNewFollow
- **Refresh:** Every 12 hours @ 00:00, 12:00 (CronExpression: `0 0 */12 * * *`)
- **TTL:** 12 hours (43200s)

---

## PERFORMANCE CONSIDERATIONS

### Index Requirements:

```sql
-- Channels
CREATE INDEX idx_channel_category ON PodcastChannel (DeletedAt);
CREATE INDEX idx_channel_shows ON PodcastShow (PodcastChannelId, DeletedAt);

-- Shows
CREATE INDEX idx_show_subcategory ON PodcastShow (PodcastSubCategoryId, DeletedAt);
CREATE INDEX idx_show_category_search ON PodcastSubCategory (PodcastCategoryId);

-- Episodes
CREATE INDEX idx_episode_show ON PodcastEpisode (PodcastShowId, DeletedAt);

-- Status Trackings
CREATE INDEX idx_channel_status ON PodcastChannelStatusTracking (PodcastChannelId, CreatedAt);
CREATE INDEX idx_show_status ON PodcastShowStatusTracking (PodcastShowId, CreatedAt);
CREATE INDEX idx_episode_status ON PodcastEpisodeStatusTracking (PodcastEpisodeId, CreatedAt);

-- Listen Sessions (for Hot Score)
CREATE INDEX idx_listen_session_time ON PodcastEpisodeListenSession (CreatedAt, IsContentRemoved);
CREATE INDEX idx_listen_session_show ON PodcastEpisodeListenSession (PodcastShowId);
```

### Query Optimization:

1. **Batch Loading:**
   - All channels/shows/episodes loaded in one query
   - Status filtered post-query (avoid complex SQL joins)
   - Podcaster accounts batch fetched

2. **Cache Strategy:**
   - Max values cached → O(1) normalization
   - Reuse show:all_time_max cache across 3 sections (TopShows, HotShows, Subcategories)

3. **Deduplication:**
   - Collect dedupShowIds after TopShows + HotShows
   - Pass as HashSet to subcategory query (fast O(1) lookup)

### Score Calculation:

- Popular Score: O(N) with cached max values
- Hot Score: O(N + M) where M = listen sessions in last 7 days
- Total: Acceptable for category-scoped queries

---

## ERROR HANDLING

### Category Not Found:

**Scenario:** `podcastCategoryId` không tồn tại

**Handling:**
```csharp
throw new Exception($"Category with ID {podcastCategoryId} not found");
```

**Response:** 404 Not Found

### Cache Miss:

**All-time Metrics:**
- Impact: Popular score sections return empty
- Fallback: Skip section (graceful degradation)
- Log: Warning level

**Temporal Metrics:**
- Impact: Hot Scores section returns items sorted by score = 0
- Fallback: Still return 20 items (no filtering)
- Log: Warning level

### Empty Subcategories:

**Scenario:** Category không có subcategories

**Handling:**
- SubCategorySections = empty list
- Other sections proceed normally
- Log: Info level

### Insufficient Shows:

**Scenario:** Subcategory có < 20 shows sau deduplication

**Handling:**
- Return available shows (không cần đủ 20)
- Example: Có 12 shows → Return 12 shows
- Log: Debug level

---

## METRIC ABBREVIATIONS

```
LC    = ListenCount
MLC   = MaxListenCount
TF    = TotalFollow (Show) / TotalFavorite (Channel)
MTF   = MaxTotalFollow (Show) / MaxTotalFavorite (Channel)
TS    = TotalSave
MTS   = MaxTotalSave
RT    = RatingTerm = AverageRating × log(RatingCount + 1)
MRT   = MaxRatingTerm
NLS   = NewListenSession (7 days)
MNLS  = MaxNewListenSession
NF    = NewFollow (7 days)
MNF   = MaxNewFollow
```

---

## EXAMPLE FLOW

### Category: "Technology" (ID = 3)

**Step 1: Category Info**
```json
{
  "Id": 3,
  "Name": "Technology",
  "MainImageFileKey": "categories/tech.jpg"
}
```

**Step 2: Top Channels (16)**
```
Query: Channels with shows in Technology category
Filter: Channel Published + Has at least 1 Published show in category
Score: popularScore = 0.6×(LC/MLC) + 0.4×(TF/MTF)
Result: 16 channels sorted by popularScore DESC
```

**Step 3: Top Shows (24)**
```
Query: Shows in Technology category
Filter: Show Published + Channel valid
Score: popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)
Result: 24 shows sorted by popularScore DESC
Collect: dedupShowIds (24 IDs)
```

**Step 4: Hot Shows (20)**
```
Query: Shows in Technology category
Filter: Show Published + Channel valid
Score: hotScore = 0.6×(NLS/MNLS) + 0.4×(NF/MNF)
Result: 20 shows sorted by hotScore DESC (no filter score > 0)
Collect: dedupShowIds += 20 IDs (total 44 IDs)
```

**Step 5: Top Episodes (30)**
```
Query: Episodes where show in Technology category
Filter: Episode + Show + Channel all Published
Score: popularScore = 0.6×(LC/MLC) + 0.4×(TS/MTS)
Result: 30 episodes sorted by popularScore DESC
```

**Step 6: Subcategory Sections**
```
Query: Subcategories of Technology
Result: [
  "AI & Machine Learning" (ID=8),
  "Software Development" (ID=12),
  "Hardware & Gadgets" (ID=15)
]

For each subcategory:
  Query: Shows in subcategory WHERE Id NOT IN (dedupShowIds)
  Filter: Show Published + Channel valid
  Score: popularScore = 0.4×(TF/MTF) + 0.4×(LC/MLC) + 0.2×(RT/MRT)
  Result: 20 shows per subcategory

Total Subcategory Shows: 3 × 20 = 60 shows
```

**Total Output:**
- 1 Category Info
- 16 Top Channels
- 24 Top Shows
- 20 Hot Shows
- 30 Top Episodes
- 60 Subcategory Shows (3 sections × 20)
- **Grand Total: 151 items (1 + 16 + 24 + 20 + 30 + 60)**

---

## CHANGELOG

### Version 1.0 (Initial)
- Category-scoped feed with 5 fixed sections
- Dynamic subcategory sections (N subcategories)
- Partial deduplication (SubcategorySections exclude TopShows + HotShows)
- Hot shows guarantee 20 results (no score > 0 filter)
- Popular + Hot scoring with cached metrics
- Anonymous-friendly (no user authentication required)
