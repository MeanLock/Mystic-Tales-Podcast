# KEYWORD SEARCH - QUERY SCENARIO

## OVERVIEW

Keyword Search cho phép user tìm kiếm podcast content (channels, shows, episodes) bằng từ khóa. Kết quả được ranked bằng hybrid scoring: BM25 (text relevance) + Engagement metrics.

**Key Characteristics:**
- **Hybrid Scoring**: BM25 (65%) + Engagement (35%)
- **Vietnamese Support**: Remove diacritics cho hashtag matching
- **Multi-entity Search**: Channels, Shows, Episodes cùng lúc
- **4-Tier Adaptive Balancing**: TopSearchResults với quality tiers
- **Comprehensive Full Lists**: Return ALL matched items (no threshold filtering)
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
  TopSearchResults: List<PodcastShowOrEpisodeKeywordSearchedListItemResponseDTO>; // Mixed Show+Episode, top 20, 4-tier quality
  ChannelList: List<ChannelListItemResponseDTO>; // ALL matching channels (no threshold)
  ShowList: List<ShowListItemResponseDTO>; // ALL matching shows (no threshold)
  EpisodeList: List<EpisodeListItemResponseDTO>; // ALL matching episodes (no threshold)
}
```

**Flow:**
1. Record keyword to cache (increment SearchCount)
2. Normalize keyword → searchTerms (split by spaces)
3. Load cache metrics (all-time only)
4. Query channels, shows, episodes in parallel
5. Calculate hybrid scores (BM25 + Engagement) for ALL items
6. **PHASE 1**: Build TopSearchResults với 4-Tier Adaptive Balancing (20 items)
7. **PHASE 2**: Build Full Lists (ALL items, no filtering, sorted by finalScore)
8. Map all results to DTOs

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

## 4-TIER ADAPTIVE BALANCING

**Applies to:** TopSearchResults ONLY (20 items)

**Full Lists Strategy:** Return ALL matched items without filtering

### TIER 1 - STRICT QUALITY
```
Threshold: finalScore >= 0.15
Target: >= 20 items
Logic: IF found >= 20 items → Take top 20, DONE ✅
```

**When Triggered:**
- Popular keywords với nhiều quality content
- Most common scenario (70% searches)

**Example:**
```
Search "công nghệ" → 80 shows match
- 50 shows: finalScore >= 0.15 ✅
- TIER 1 triggers → Take top 20 from 50 shows
```

---

### TIER 2 - RELAXED QUALITY
```
Threshold: finalScore >= 0.08
Target: >= 20 items
Logic: IF found >= 20 items → Take top 20, DONE ✅
```

**When Triggered:**
- Medium popularity keywords
- Not enough TIER 1 quality content
- ~20% searches

**Example:**
```
Search "khởi nghiệp nông nghiệp" → 35 shows match
- 8 shows: finalScore >= 0.15 (not enough)
- 18 shows: finalScore >= 0.08 (enough) ✅
- TIER 2 triggers → Take top 20 from 18 shows
```

---

### TIER 3 - RELEVANCE-FOCUSED (Catch New Content)
```
Threshold: bm25Score >= 0.05
Target: >= 10 items (half of target)
Logic: IF found >= 10 items → Take top 20, DONE ✅
```

**When Triggered:**
- Rare/niche keywords
- Content mới (engagement = 0) nhưng keyword match tốt
- ~8% searches

**Purpose:** Enable new content discovery based on textual relevance

**Example:**
```
Search "podcast nuôi ong mật Đà Lạt" → 12 shows match
- 2 shows: finalScore >= 0.15 (not enough)
- 3 shows: finalScore >= 0.08 (not enough)
- 8 shows: bm25Score >= 0.05 (not enough for 10)
- 12 shows: bm25Score >= 0.03
- TIER 3 fails, go to TIER 4
```

---

### TIER 4 - FALLBACK (Never Empty)
```
Threshold: NONE (ALL matched items)
Target: ANY
Logic: Return ALL available items (sorted by finalScore), DONE ✅
```

**When Triggered:**
- Very rare/specific keywords
- Ensure non-empty results
- ~2% searches

**Example:**
```
Search "podcast về craft beer ở Đà Nẵng" → 3 shows match
- No tier has enough items
- TIER 4 triggers → Return all 3 shows
```

---

### TIER DECISION FLOW

```
Query entities → Calculate scores for ALL items
│
├─ PHASE 1: TopSearchResults (20 items)
│   │
│   ├─ Check TIER 1 (finalScore >= 0.15)
│   │   Found >= 20? YES → Take top 20 ✅ DONE
│   │              NO  → Continue to TIER 2
│   │
│   ├─ Check TIER 2 (finalScore >= 0.08)
│   │   Found >= 20? YES → Take top 20 ✅ DONE
│   │              NO  → Continue to TIER 3
│   │
│   ├─ Check TIER 3 (bm25Score >= 0.05)
│   │   Found >= 10? YES → Take top 20 ✅ DONE
│   │              NO  → Continue to TIER 4
│   │
│   └─ TIER 4 (ALL matched)
│       Return ALL available items ✅ DONE
│
└─ PHASE 2: Full Lists (ALL items)
    ├─ ChannelList: ALL matched channels sorted by finalScore
    ├─ ShowList: ALL matched shows sorted by finalScore
    └─ EpisodeList: ALL matched episodes sorted by finalScore
```

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

**Weight Rationale:**
- **BM25 (65%)**: Prioritize textual relevance
- **Engagement (35%)**: Secondary boost for popular content
- **Effect**: Content mới cần BM25 RẤT cao mới compete với content có engagement

**No Global Threshold for Full Lists:**
- TopSearchResults: Uses 4-tier thresholds
- Full Lists (ChannelList, ShowList, EpisodeList): No filtering, ALL items returned

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

**Purpose:** Enable fair comparison between Shows and Episodes for TopSearchResults

**Input:** Scored items from 4-Tier Adaptive Balancing
- topShowScores (filtered by tier)
- topEpisodeScores (filtered by tier)

**Logic:**
1. Calculate maxShowScore = max(topShowScores.finalScore)
2. Calculate maxEpisodeScore = max(topEpisodeScores.finalScore)
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

## FULL LISTS STRATEGY

**Applies to:** ChannelList, ShowList, EpisodeList

**Key Principle:** Comprehensive results - return ALL matched items

**Logic:**
1. Use ALL scored items (no tier filtering)
2. Sort by finalScore DESC
3. Map to DTOs

**Result Distribution:**
```
ShowList (100 items total):
  Items 1-50:  High finalScore (0.3 - 0.5)  ← Content nổi tiếng
  Items 51-80: Medium finalScore (0.1 - 0.3) ← Content trung bình
  Items 81-100: Low finalScore (0.01 - 0.1)  ← Content mới (0 engagement)
  
User có thể scroll xuống tìm content mới ✅
```

**Rationale:**
- TopSearchResults: Quality-focused (20 curated items)
- Full Lists: Discovery-focused (ALL available items)
- Content creators: Responsible for keyword strategy để compete

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
- 4-Tier Check: O(N) × 4 iterations (worst case)
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

## TIER MONITORING

### Recommended Metrics:

**Tier Distribution (Weekly):**
```
TIER 1 (strict):   70% searches ✅ Good quality availability
TIER 2 (relaxed):  20% searches ✅ Acceptable fallback
TIER 3 (BM25):     8% searches  ⚠️ Edge case (new content/niche)
TIER 4 (fallback): 2% searches  ⚠️ Very rare keywords

Warning: If TIER 3+4 > 15% → Review content quality or thresholds
```

**Logging Example:**
```
[AdaptiveBalancing-Show] ✅ TIER 1: 45 items >= 0.15
[AdaptiveBalancing-Show] ⚠️ TIER 2: 18 items >= 0.08
[AdaptiveBalancing-Show] 🔍 TIER 3: 12 items with BM25 >= 0.05
[AdaptiveBalancing-Show] 🆘 TIER 4: Returning all 5 items
```

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

### Example 1: Popular Keyword (TIER 1)

**Input:**
```
keyword = "tech podcast"
searchTerms = ["tech", "podcast"]
```

**Query Results:**
```
100 shows match keyword:
- 60 shows: finalScore >= 0.15 (TIER 1 quality)
- 25 shows: finalScore 0.08-0.15 (TIER 2 quality)
- 15 shows: finalScore < 0.08 (low quality/new content)
```

**TIER Decision:**
```
TIER 1 check: 60 items >= 20 ✅
Action: Take top 20 from 60 shows
Result: TopSearchResults = 20 high-quality items
```

**Full Lists:**
```json
{
  "TopSearchResults": [20 items],  // TIER 1 filtered
  "ShowList": [100 items],         // ALL shows, sorted by finalScore
  "EpisodeList": [80 items],       // ALL episodes, sorted by finalScore
  "ChannelList": [15 items]        // ALL channels, sorted by finalScore
}
```

**Score Examples:**
```
Show A (High Quality):
  BM25Score = 5.0×1.2 + 2.0×0.8 = 7.6
  EngagementScore = 0.3×0.8 + 0.3×0.9 + 0.2×0.7 + 0.2×1.0 = 0.85
  FinalScore = 0.65×7.6 + 0.35×0.85 = 5.24 ✅ TIER 1

Show B (Medium Quality):
  BM25Score = 3.2
  EngagementScore = 0.45
  FinalScore = 0.65×3.2 + 0.35×0.45 = 2.24 ✅ TIER 1

Show C (New Content):
  BM25Score = 2.8
  EngagementScore = 0.0 (no engagement yet)
  FinalScore = 0.65×2.8 + 0.35×0 = 1.82 ✅ TIER 1

Show D (Low Match):
  BM25Score = 0.5
  EngagementScore = 0.3
  FinalScore = 0.65×0.5 + 0.35×0.3 = 0.43
  → Not in TopSearchResults (only #67 in ShowList)
  → But still in ShowList ✅ (user can scroll down to find)
```

---

### Example 2: Niche Keyword (TIER 3)

**Input:**
```
keyword = "podcast nuôi ong mật Đà Lạt"
searchTerms = ["podcast", "nuoi", "ong", "mat", "da", "lat"]
```

**Query Results:**
```
12 shows match keyword:
- 2 shows: finalScore >= 0.15 (old content)
- 3 shows: finalScore >= 0.08 (medium)
- 5 shows: bm25Score >= 0.05, finalScore < 0.08 (new content, good keyword match)
- 2 shows: finalScore < 0.05 (poor match)
```

**TIER Decision:**
```
TIER 1 check: 2 items < 20 ❌
TIER 2 check: 5 items < 20 ❌
TIER 3 check: 10 items >= 10 ✅
Action: Take top 20 (but only 10 available) → Return all 10
Result: TopSearchResults = 10 items (mix TIER 1+2+3)
```

**Full Lists:**
```json
{
  "TopSearchResults": [10 items],  // TIER 3 filtered (BM25 >= 0.05)
  "ShowList": [12 items],          // ALL 12 shows
  "EpisodeList": [8 items],        // ALL episodes
  "ChannelList": [3 items]         // ALL channels
}
```

**New Content Example:**
```
Show E (New, Zero Engagement):
  BM25Score = 1.8 (keyword match tốt: "nuôi ong", "Đà Lạt" in name)
  EngagementScore = 0.0 (created 5 days ago, no listeners yet)
  FinalScore = 0.65×1.8 + 0.35×0 = 1.17
  → In TopSearchResults ✅ (TIER 1)
  → Position #3 in ShowList ✅
  
Show F (New, Low BM25):
  BM25Score = 0.4 (keyword match yếu: chỉ "podcast" in description)
  EngagementScore = 0.0
  FinalScore = 0.65×0.4 + 0.35×0 = 0.26
  → Not in TopSearchResults ❌ (below TIER 3)
  → Position #11 in ShowList ✅ (user có thể scroll tìm)
```

---

### Example 3: Very Rare Keyword (TIER 4)

**Input:**
```
keyword = "craft beer brewing Đà Nẵng"
searchTerms = ["craft", "beer", "brewing", "da", "nang"]
```

**Query Results:**
```
3 shows match keyword:
- 1 show: finalScore = 0.12 (below all tiers)
- 1 show: finalScore = 0.08 (TIER 2 threshold)
- 1 show: finalScore = 0.05 (very low)
```

**TIER Decision:**
```
TIER 1 check: 0 items ❌
TIER 2 check: 1 item < 20 ❌
TIER 3 check: 2 items < 10 ❌
TIER 4: Return ALL matched items
Result: TopSearchResults = 3 items (all available)
```

**Full Lists:**
```json
{
  "TopSearchResults": [3 items],   // TIER 4 (all matched)
  "ShowList": [3 items],           // Same as TopSearchResults
  "EpisodeList": [1 item],
  "ChannelList": [0 items]
}
```

**Rationale:** Better to return 3 relevant results than empty list ✅

---

## CHANGELOG

### Version 2.0 (Current)
- **Added**: 4-Tier Adaptive Balancing for TopSearchResults
  - TIER 1: finalScore >= 0.15 (strict quality)
  - TIER 2: finalScore >= 0.08 (relaxed quality)
  - TIER 3: bm25Score >= 0.05 (relevance-focused)
  - TIER 4: ALL matched items (fallback)
- **Changed**: Full Lists return ALL matched items (no threshold filtering)
- **Changed**: Content mới có thể xuất hiện ở cuối Full Lists (fair competition)
- **Added**: Tier monitoring và logging recommendations
- **Updated**: Example queries với tier scenarios

### Version 1.0 (Initial)
- Hybrid scoring: BM25 + Engagement
- Vietnamese diacritic support
- Multi-entity search (Channels, Shows, Episodes)
- Keyword suggestions with search count tracking
- Minimum score threshold filtering (0.15) - DEPRECATED in v2.0
- Top mixed results (Show + Episode, normalized scores)
- Anonymous-friendly (no user authentication required)
