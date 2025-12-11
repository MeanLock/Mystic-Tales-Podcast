# KEYWORD SEARCH - QUERY SCENARIO

## OVERVIEW

Keyword Search cho phép user tìm kiếm podcast content (channels, shows, episodes) bằng từ khóa. Kết quả được ranked bằng hybrid scoring: BM25 (text relevance) + Engagement metrics.

**Key Characteristics:**
- **Hybrid Scoring**: BM25 (65%) + Engagement (35%)
- **Vietnamese Support**: Remove diacritics cho hashtag matching
- **Multi-entity Search**: Channels, Shows, Episodes cùng lúc
- **Minimum Threshold**: Filter results với score ≥ 0.15
- **Cache Tracking**: Record search keywords để suggest
- **Anonymous-friendly**: Không phụ thuộc user authentication

---

## SEARCH FEATURES

### 1. KEYWORD SUGGESTIONS 🔍

**API:** `GetPodcastKeywordSearchSuggestionsAsync(string prefix, int limit = 10)`

**Logic:**
1. Normalize prefix: `lowercase + trim`
2. Load cache: `query:search_keyword:podcast_content:customer`
3. Match keywords where **ANY word starts with prefix**
   - Split keyword by spaces
   - Check if any word starts with prefix
4. Sort by SearchCount DESC
5. Take top N suggestions

**Example:**
```
prefix = "tech"
matches:
  - "tech talk" (SearchCount: 150) ✅
  - "talking tech" (SearchCount: 120) ✅
  - "technology news" (SearchCount: 100) ✅
  - "podcast tech" (SearchCount: 80) ✅
```

**Cache Key:** `query:search_keyword:podcast_content:customer`  
**Cache Structure:**
```json
{
  "KeywordList": [
    {
      "Keyword": "tech talk",
      "SearchCount": 150
    },
    {
      "Keyword": "technology news",
      "SearchCount": 100
    }
  ]
}
```

---

### 2. KEYWORD TRACKING 📊

**API:** `UpdatePodcastFeedContentKeywordSearchCacheAsync(string keyword)`

**Logic:**
1. Normalize keyword: `lowercase + trim`
2. Load existing cache
3. If keyword exists → Increment SearchCount
4. If keyword new → Add with SearchCount = 1
5. Save back to cache (NO expiration)

**Update Behavior:**
- Existing keyword: `SearchCount++`
- New keyword: `SearchCount = 1`
- Cache TTL: **None** (persistent)

---

### 3. MAIN SEARCH - FULL RESULTS 🎯

**API:** `GetPodcastFeedContentsByKeywordSearchAsync(string keyword)`

**Output:**
```typescript
PodcastContentKeywordSearchResultResponseDTO {
  TopSearchResults: List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO>; // Mixed Show+Episode, top 20
  ChannelList: List<ChannelListItemResponseDTO>; // All matching channels
  ShowList: List<ShowListItemResponseDTO>; // All matching shows
  EpisodeList: List<EpisodeListItemResponseDTO>; // All matching episodes
}
```

**Flow:**
1. Record keyword to cache (increment SearchCount)
2. Normalize keyword → searchTerms (split by spaces)
3. Load cache metrics (all-time only)
4. Query channels, shows, episodes in parallel
5. Calculate hybrid scores (BM25 + Engagement)
6. Filter by minScoreThreshold = 0.15
7. Normalize scores for mixed results
8. Build TopSearchResults (mixed Show+Episode, top 20)
9. Map all results to DTOs

---

### 4. QUERY SEARCH - LIMITED RESULTS 🎯

**API:** `GetPodcastFeedContentsByKeywordQueryAsync(string keyword, int limit)`

**Output:**
```typescript
List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO> // Mixed Show+Episode, limited
```

**Difference from Main Search:**
- NO keyword cache recording
- Only return TopSearchResults (mixed list)
- Limit results by parameter
- Lighter weight (for autocomplete/quick search)

---

## QUERY LOGIC

### Channel Query:

**Query Conditions:**
```sql
WHERE pc.DeletedAt == NULL
```

**Include Relationships:**
- PodcastChannelStatusTrackings
- PodcastChannelHashtags → Hashtag

**Status Filter:**
```csharp
Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Podcaster Validation:**
```csharp
account.DeactivatedAt == NULL
account.HasVerifiedPodcasterProfile == TRUE
```

**Keyword Matching:**
Match if ANY searchTerm found in:
- Channel.Name (lowercase)
- Channel.Description (lowercase)
- Podcaster.FullName (lowercase)
- Podcaster.PodcasterProfileName (lowercase)
- Hashtags.Name (lowercase, diacritics removed)

---

### Show Query:

**Query Conditions:**
```sql
WHERE ps.DeletedAt == NULL
```

**Include Relationships:**
- PodcastShowStatusTrackings
- PodcastChannel → PodcastChannelStatusTrackings
- PodcastShowHashtags → Hashtag

**Status Filter:**
```csharp
Latest PodcastShowStatusTracking.PodcastShowStatusId == Published
Channel conditions (if show has channel):
  - Channel.DeletedAt == NULL
  - Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Podcaster Validation:**
```csharp
account.DeactivatedAt == NULL
account.HasVerifiedPodcasterProfile == TRUE
```

**Keyword Matching:**
Match if ANY searchTerm found in:
- Show.Name (lowercase)
- Show.Description (lowercase)
- Podcaster.FullName (lowercase)
- Podcaster.PodcasterProfileName (lowercase)
- Channel.Name (lowercase, if show has channel)
- Hashtags.Name (lowercase, diacritics removed)

---

### Episode Query:

**Query Conditions:**
```sql
WHERE pe.DeletedAt == NULL
  AND pe.PodcastShow.DeletedAt == NULL
```

**Include Relationships:**
- PodcastEpisodeStatusTrackings
- PodcastShow → PodcastShowStatusTrackings
- PodcastShow → PodcastChannel → PodcastChannelStatusTrackings
- PodcastEpisodeSubscriptionType
- PodcastEpisodeHashtags → Hashtag

**Status Filter:**
```csharp
Latest PodcastEpisodeStatusTracking.PodcastEpisodeStatusId == Published
Latest PodcastShowStatusTracking.PodcastShowStatusId == Published
Channel conditions (if show has channel):
  - Channel.DeletedAt == NULL
  - Latest PodcastChannelStatusTracking.PodcastChannelStatusId == Published
```

**Podcaster Validation:**
```csharp
account.DeactivatedAt == NULL
account.HasVerifiedPodcasterProfile == TRUE
```

**Keyword Matching:**
Match if ANY searchTerm found in:
- Episode.Name (lowercase)
- Episode.Description (lowercase)
- Show.Name (lowercase)
- Podcaster.FullName (lowercase)
- Podcaster.PodcasterProfileName (lowercase)
- Hashtags.Name (lowercase, diacritics removed)

---

## SCORING SYSTEM

### Hybrid Score Formula:

```
finalScore = 0.65 × BM25Score + 0.35 × EngagementScore
```

**Minimum Threshold:** `finalScore ≥ 0.15`

---

### BM25 Score (Text Relevance):

**Parameters:**
- k1 = 1.5 (term frequency saturation)
- b = 0.75 (length normalization)

**Field Weights:**
- Name: 5.0
- Description: 2.0

**Formula:**
```
BM25Score = 5.0 × BM25(Name) + 2.0 × BM25(Description)

BM25(field) = Σ(IDF(term) × normalizedTF(term))

Where:
- IDF(term) = log((N - df + 0.5) / (df + 0.5) + 1)
  - N = Total documents
  - df = Document frequency (docs containing term)
  
- normalizedTF = (tf × (k1 + 1)) / (tf + k1 × (1 - b + b × (docLength / avgLength)))
  - tf = Term frequency in field
  - docLength = Field length
  - avgLength = Average field length
```

---

### Engagement Score (Popularity):

#### Channel Engagement:
```
engagementScore = 0.6 × (LC/MLC) + 0.4 × (TF/MTF)

Where:
- LC = ListenCount
- MLC = MaxListenCount (from cache)
- TF = TotalFavorite
- MTF = MaxTotalFavorite (from cache)
```

#### Show Engagement:
```
engagementScore = 0.3 × (TF/MTF) + 0.3 × (LC/MLC) + 0.2 × (RT/MRT) + 0.2 × RecencyBoost

Where:
- TF = TotalFollow
- MTF = MaxTotalFollow (from cache)
- LC = ListenCount
- MLC = MaxListenCount (from cache)
- RT = AverageRating × log(RatingCount + 1)
- MRT = MaxRatingTerm (from cache)
- RecencyBoost = see below
```

#### Episode Engagement:
```
engagementScore = 0.5 × (LC/MLC) + 0.3 × (TS/MTS) + 0.2 × RecencyBoost

Where:
- LC = ListenCount
- MLC = MaxListenCount (from cache)
- TS = TotalSave
- MTS = MaxTotalSave (from cache)
- RecencyBoost = see below
```

---

### Recency Boost:

**For Shows:**
```
RecencyBoost = 
  1.0  if publishedAt ≤ 7 days ago
  0.5  if publishedAt ≤ 30 days ago
  0.0  otherwise
```

**For Episodes:**
```
RecencyBoost = 
  1.0  if publishedAt ≤ 7 days ago
  0.5  if publishedAt ≤ 30 days ago
  0.0  otherwise
```

**Published Tracking:**
- Find latest `StatusTracking` where `StatusId == Published`
- Use `CreatedAt` of that tracking as `publishedAt`

---

## TOP SEARCH RESULTS (MIXED)

### Normalization Strategy:

**Purpose:** Enable fair comparison between Shows and Episodes

**Logic:**
1. Calculate maxShowScore = max(show.finalScore)
2. Calculate maxEpisodeScore = max(episode.finalScore)
3. Normalize each show: `normalizedScore = finalScore / maxShowScore`
4. Normalize each episode: `normalizedScore = finalScore / maxEpisodeScore`
5. Merge normalized shows + episodes into single list
6. Sort by normalizedScore DESC
7. Take top 20 items

**Output:**
```typescript
List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO> {
  Show: PodcastShowSnippetResponseDTO | null,
  Episode: PodcastEpisodeSnippetResponseDTO | null
}
```

**Example:**
```json
[
  { "Show": {...}, "Episode": null },
  { "Show": null, "Episode": {...} },
  { "Show": {...}, "Episode": null },
  { "Show": null, "Episode": {...} },
  ...
]
```

---

## VIETNAMESE SUPPORT

### Diacritic Removal:

**Function:** `RemoveVietnameseDiacritics(string text)`

**Logic:**
1. Normalize to FormD (separate base + diacritics)
2. Remove NonSpacingMark characters
3. Normalize to FormC
4. Replace đ → d, Đ → D

**Example:**
```
Input:  "Tiếng Việt"
Output: "Tieng Viet"

Input:  "công nghệ"
Output: "cong nghe"

Input:  "đồ ăn"
Output: "do an"
```

**Usage:**
- Applied to hashtag matching only
- User input kept original (case-insensitive matching)
- Hashtag.Name normalized during comparison

---

## CACHE DEPENDENCIES

### All-Time Max Values:

**Channel:**
- `query:metric:channel:all_time_max`
  - MaxListenCount
  - MaxTotalFavorite

**Show:**
- `query:metric:show:all_time_max`
  - MaxTotalFollow
  - MaxListenCount
  - MaxRatingTerm

**Episode:**
- `query:metric:episode:all_time_max`
  - MaxListenCount
  - MaxTotalSave

**Refresh:** Daily @ 00:00 (CronExpression: `0 0 0 * * *`)  
**TTL:** 24 hours (86400s)

### Keyword Cache:

**Cache Key:** `query:search_keyword:podcast_content:customer`  
**Refresh:** On-demand (every search updates)  
**TTL:** None (persistent)

---

## PERFORMANCE CONSIDERATIONS

### Index Requirements:

```sql
-- Channels
CREATE INDEX idx_channel_search ON PodcastChannel (DeletedAt);
CREATE INDEX idx_channel_hashtag ON PodcastChannelHashtag (PodcastChannelId, HashtagId);

-- Shows
CREATE INDEX idx_show_search ON PodcastShow (DeletedAt);
CREATE INDEX idx_show_hashtag ON PodcastShowHashtag (PodcastShowId, HashtagId);

-- Episodes
CREATE INDEX idx_episode_search ON PodcastEpisode (DeletedAt, PodcastShowId);
CREATE INDEX idx_episode_hashtag ON PodcastEpisodeHashtag (PodcastEpisodeId, HashtagId);

-- Hashtags
CREATE INDEX idx_hashtag_name ON Hashtag (Name);

-- Status Trackings
CREATE INDEX idx_channel_status ON PodcastChannelStatusTracking (PodcastChannelId, CreatedAt);
CREATE INDEX idx_show_status ON PodcastShowStatusTracking (PodcastShowId, CreatedAt);
CREATE INDEX idx_episode_status ON PodcastEpisodeStatusTracking (PodcastEpisodeId, CreatedAt);
```

### Query Optimization:

1. **Parallel Queries**: Channels, Shows, Episodes fetched concurrently
2. **Batch Podcaster Fetch**: All podcaster accounts loaded in one batch
3. **Pre-filter**: Apply status filters post-query (avoid complex SQL)
4. **Cache Max Values**: O(1) normalization with cached metrics

### Score Calculation:

- BM25: O(N × M) where N = docs, M = terms
- Engagement: O(N) with cached max values
- Total: O(N × M) acceptable for search workload

---

## ERROR HANDLING

### Cache Miss Scenarios:

**Keyword Cache Miss:**
- Impact: Suggestions return empty list
- Fallback: Initialize empty cache
- Log: Info level

**Metric Cache Miss:**
- Impact: Engagement score = 0 (only BM25 used)
- Fallback: Continue with BM25 only
- Log: Warning level

### Invalid Input:

**Empty Keyword:**
- Throw ArgumentException
- Return 400 Bad Request

**Special Characters:**
- Allow all characters (normalized to lowercase)
- SQL injection prevented by EF Core parameterization

### Data Validation:

**Missing Podcaster:**
- Filter out during mapping
- Log: Debug level

**Missing Status Tracking:**
- Filter out (treat as not published)
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
```

---

## EXAMPLE QUERIES

### Search "tech podcast":

**Input:**
```
keyword = "tech podcast"
searchTerms = ["tech", "podcast"]
```

**Matching Logic:**
- Show.Name contains "tech" OR "podcast" ✅
- Show.Description contains "tech" OR "podcast" ✅
- Hashtag.Name (normalized) contains "tech" OR "podcast" ✅

**Score Calculation:**
```
Show A:
  BM25Score = 5.0 × BM25(Name) + 2.0 × BM25(Desc) = 8.5
  EngagementScore = 0.3×0.8 + 0.3×0.9 + 0.2×0.7 + 0.2×1.0 = 0.85
  FinalScore = 0.65×8.5 + 0.35×0.85 = 5.82

Show B:
  BM25Score = 3.2
  EngagementScore = 0.65
  FinalScore = 0.65×3.2 + 0.35×0.65 = 2.31
```

**Filtering:**
- Show A: 5.82 ≥ 0.15 ✅ (included)
- Show B: 2.31 ≥ 0.15 ✅ (included)

**Normalization (for TopSearchResults):**
```
maxShowScore = 5.82
Show A normalized = 5.82 / 5.82 = 1.0
Show B normalized = 2.31 / 5.82 = 0.40

maxEpisodeScore = 4.5
Episode X normalized = 4.5 / 4.5 = 1.0
Episode Y normalized = 3.2 / 4.5 = 0.71

Mixed Results (sorted):
1. Show A (1.0)
2. Episode X (1.0)
3. Episode Y (0.71)
4. Show B (0.40)
```

---

## CHANGELOG

### Version 1.0 (Initial)
- Hybrid scoring: BM25 + Engagement
- Vietnamese diacritic support
- Multi-entity search (Channels, Shows, Episodes)
- Keyword suggestions with search count tracking
- Minimum score threshold filtering (0.15)
- Top mixed results (Show + Episode, normalized scores)
- Anonymous-friendly (no user authentication required)
