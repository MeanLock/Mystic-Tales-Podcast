# from hmac import new
# from pyexpat import model
# from fastapi import FastAPI
# from pydantic import BaseModel
# from sentence_transformers import SentenceTransformer
# import numpy as np
# from typing import List, Optional


# app = FastAPI()

# # Load model Tiếng Việt tối ưu cho semantic search
# Vietnamese_document_embedding_model = SentenceTransformer("./vietnamese-document-embedding",trust_remote_code=True)
# Vietnamese_document_embedding_model.max_seq_length = 2048

# class SummarizedFilterTagDTO(BaseModel):
#     FilterTagId: int
#     Summary: Optional[str] = None
    


    
# @app.post("/encode/filter-tags")
# def encode_filter_tags(data: List[SummarizedFilterTagDTO]):
#     """
#     Nhận vào list các object dạng {FilterTagId, Summary},
#     encode từng summary bằng Vietnamese_document_embedding_model,
#     trả về list mới dạng {FilterTagId, EmbeddingVector}
#     """
#     result = []
#     for item in data:
#         if item.Summary is not None and item.Summary != "":
#             vector = Vietnamese_document_embedding_model.encode([item.Summary])[0]
#             result.append({"FilterTagId": item.FilterTagId, "EmbeddingVector": vector.tolist()})
#         else:
#             result.append({"FilterTagId": item.FilterTagId, "EmbeddingVector": None})
#     return result

# class EmbeddingVectorFilterTagDTO(BaseModel):
#     FilterTagId: int
#     EmbeddingVector: Optional[List[float]] = None
    
# class CandidateEmbeddingVectorFilterTagsDTO(BaseModel):
#     CandidateId: int
#     EmbeddingVectorFilterTags: List[EmbeddingVectorFilterTagDTO] 
#     CandidateTagFilterAccuracyRate: Optional[float] = None

# class FilterTagSimilarityComparisonRequestDTO(BaseModel):
#     TargetEmbeddingVectorFilterTags: List[EmbeddingVectorFilterTagDTO]
#     CandidateEmbeddingVectorFilterTags: List[CandidateEmbeddingVectorFilterTagsDTO]
#     MinScore: float = 0.45  # Điểm tối thiểu để coi là tương đồng
#     MaxScore: float = 0.85  # Điểm tối đa để coi là tương đồng
#     TargetTagFilterAccuracyRate: Optional[float] = None
    
# class FilterTagSimilarityComparisonResultDTO(BaseModel):
#     CandidateId: int
#     SimilarityScore : float

# @app.post("/compare/filter-tag-similarity-by-target-accuracy")
# def filter_candidates_by_tag_similarity(req: FilterTagSimilarityComparisonRequestDTO):
#     min_score = req.MinScore
#     max_score = req.MaxScore
#     tag_filter_accuracy_rate = req.TargetTagFilterAccuracyRate

#     def calc_similarity(v1, v2):
#         if v1 is None or v2 is None:
#             return max_score
#         sim = np.dot(v1, v2) / (np.linalg.norm(v1) * np.linalg.norm(v2))
#         return sim

#     def convert_similarity_to_accuracy(similarity, min_score, max_score):
#         if similarity <= min_score:
#             return 0
#         elif similarity >= max_score:
#             return 1
#         else:
#             return round((similarity - min_score) / (max_score - min_score), 2)

#     results = []
#     target_tag_dict = {tag.FilterTagId: tag.EmbeddingVector for tag in req.TargetEmbeddingVectorFilterTags}
#     for candidate in req.CandidateEmbeddingVectorFilterTags:
#         candidate_id = candidate.CandidateId
#         acc_similarities = []
#         for tag in candidate.EmbeddingVectorFilterTags:
#             target_vec = target_tag_dict.get(tag.FilterTagId)
#             # if target_vec is None:
#             #     continue
#             sim = calc_similarity(target_vec, tag.EmbeddingVector)
#             acc_similarities.append(sim)
#         if not acc_similarities:
#             continue
#         avg_sim = np.mean(acc_similarities)
#         avg_accuracy = convert_similarity_to_accuracy(avg_sim, min_score, max_score)
#         if avg_accuracy >= (tag_filter_accuracy_rate / 100):
#             results.append(FilterTagSimilarityComparisonResultDTO(CandidateId=candidate_id, SimilarityScore=avg_sim))
#     return results


# @app.post("/compare/filter-tag-similarity-by-candidate-accuracy")
# def filter_candidates_by_tag_similarity_by_candidate_accuracy(req: FilterTagSimilarityComparisonRequestDTO):
#     min_score = req.MinScore
#     max_score = req.MaxScore

#     def calc_similarity(v1, v2):
#         if v1 is None or v2 is None:
#             return max_score
#         sim = np.dot(v1, v2) / (np.linalg.norm(v1) * np.linalg.norm(v2))
#         return sim

#     def convert_similarity_to_accuracy(similarity, min_score, max_score):
#         if similarity <= min_score:
#             return 0
#         elif similarity >= max_score:
#             return 1
#         else:
#             return round((similarity - min_score) / (max_score - min_score), 2)

#     results = []
#     target_tag_dict = {tag.FilterTagId: tag.EmbeddingVector for tag in req.TargetEmbeddingVectorFilterTags}
#     for candidate in req.CandidateEmbeddingVectorFilterTags:
#         candidate_id = candidate.CandidateId
#         candidate_accuracy_rate = candidate.CandidateTagFilterAccuracyRate
#         acc_similarities = []
#         for tag in candidate.EmbeddingVectorFilterTags:
#             target_vec = target_tag_dict.get(tag.FilterTagId)
#             # if target_vec is None:
#             #     continue
#             sim = calc_similarity(target_vec, tag.EmbeddingVector)
#             print (f"FilterTagId: {tag.FilterTagId}, surveyId: {candidate_id}, Similarity: {sim}")
#             acc_similarities.append(sim)
#         if not acc_similarities:
#             continue
#         avg_sim = np.mean(acc_similarities)
#         avg_accuracy = convert_similarity_to_accuracy(avg_sim, min_score, max_score)
#         if avg_accuracy >= (candidate_accuracy_rate / 100):
#             results.append(FilterTagSimilarityComparisonResultDTO(CandidateId=candidate_id, SimilarityScore=avg_sim))
#     return results



