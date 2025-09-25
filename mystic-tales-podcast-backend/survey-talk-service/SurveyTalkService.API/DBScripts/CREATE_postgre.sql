-- Table: public.SurveyTagFilter

DROP TABLE IF EXISTS public."SurveyTagFilter";
DROP TABLE IF EXISTS public."SurveyTakenResultTagFilter";
DROP TABLE IF EXISTS public."TakerTagFilter";

CREATE TABLE IF NOT EXISTS public."SurveyEmbeddingVectorTagFilter"
(
    "surveyId" integer NOT NULL,
    "filterTagId" integer NOT NULL,
    "embeddingVector" vector(768),
    CONSTRAINT "SurveyTagFilter_pkey" PRIMARY KEY ("surveyId", "filterTagId")
)

CREATE TABLE IF NOT EXISTS public."SurveyEmbeddingVectorTakenResultTagFilter"
(
    "surveyTakenResultId" integer NOT NULL,
    "additionalFilterTagId" integer NOT NULL,
    "embeddingVector" vector(768),
    CONSTRAINT "SurveyTakenResultTagFilter_pkey" PRIMARY KEY ("surveyTakenResultId", "additionalFilterTagId")
)

CREATE TABLE IF NOT EXISTS public."TakerEmbeddingVectorTagFilter"
(
    "takerId" integer NOT NULL,
    "filterTagId" integer NOT NULL,
    "embeddingVector" vector(768),
    CONSTRAINT "TakerTagFilter_pkey" PRIMARY KEY ("takerId", "filterTagId")
)

select * from "TakerEmbeddingVectorTagFilter" where "TakerEmbeddingVectorTagFilter"."takerId" = 1
select * from "TakerEmbeddingVectorTagFilter" where "TakerEmbeddingVectorTagFilter"."takerId" = 10
select * from "SurveyEmbeddingVectorTagFilter"
delete from "TakerEmbeddingVectorTagFilter" where "TakerEmbeddingVectorTagFilter"."takerId" = 6

INSERT INTO "TakerEmbeddingVectorTagFilter" ("takerId", "filterTagId", "embeddingVector")
VALUES (1, 2, '[1473223,0.032559 .... 1473223,0.032559]')
INSERT INTO "SurveyEmbeddingVectorTakenResultTagFilter" ("surveyTakenResultId", "additionalFilterTagId", "embeddingVector")
VALUES (1, 2, '[1473223,0.032559 .... 1473223,0.032559]')
INSERT INTO "SurveyEmbeddingVectorTagFilter" ("surveyId", "filterTagId", "embeddingVector")
VALUES (1, 2, '[1473223,0.032559 .... 1473223,0.032559]')