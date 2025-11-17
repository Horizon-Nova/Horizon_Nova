using HNB.IntelligentSystems.QdrantEmbedding.Models;
using HNB.IntelligentSystems.QdrantEmbedding.Module;
using Microsoft.AspNetCore.Mvc;

namespace HNB.IntelligentSystems.QdrantEmbedding.Api;

/// <summary>
/// QdrantEmbedding API Controller
/// 提供向量資料庫操作的 API 端點
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class QdrantEmbeddingApiController(QdrantEmbeddingModule module) : ControllerBase
{
    /// <summary>
    /// 檢查 Collection 是否存在
    /// GET /api/QdrantEmbeddingApi/CollectionExists?collectionName=xxx
    /// </summary>
    [HttpGet("CollectionExists")]
    public async Task<IActionResult> CollectionExists([FromQuery] string? collectionName)
    {
        try
        {
            var exists = await module.CollectionExists(collectionName);
            return Ok(new { success = true, exists, collectionName = collectionName ?? "default" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 建立 Collection
    /// POST /api/QdrantEmbeddingApi/CreateCollection
    /// </summary>
    [HttpPost("CreateCollection")]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CollectionName))
            return BadRequest(new { success = false, error = "Collection 名稱不能為空" });

        if (request.VectorSize <= 0)
            return BadRequest(new { success = false, error = "向量維度必須大於 0" });

        try
        {
            var result = await module.CreateCollection(request.CollectionName, request.VectorSize, request.DistanceMetric);
            if (result)
            {
                return Ok(new QdrantResponse { Success = true, Message = $"Collection '{request.CollectionName}' 建立成功" });
            }
            return BadRequest(new QdrantResponse { Success = false, Error = "Collection 建立失敗" });
        }
        catch (Exception ex)
        {
            return BadRequest(new QdrantResponse { Success = false, Error = ex.Message });
        }
    }

    /// <summary>
    /// 列出所有 Collection
    /// GET /api/QdrantEmbeddingApi/ListCollections
    /// </summary>
    [HttpGet("ListCollections")]
    public async Task<IActionResult> ListCollections()
    {
        try
        {
            var collections = await module.ListCollections();
            return Ok(new { success = true, collections });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 刪除 Collection
    /// DELETE /api/QdrantEmbeddingApi/DeleteCollection?collectionName=xxx
    /// </summary>
    [HttpDelete("DeleteCollection")]
    public async Task<IActionResult> DeleteCollection([FromQuery] string collectionName)
    {
        if (string.IsNullOrWhiteSpace(collectionName))
            return BadRequest(new { success = false, error = "Collection 名稱不能為空" });

        try
        {
            var result = await module.DeleteCollection(collectionName);
            if (result)
            {
                return Ok(new QdrantResponse { Success = true, Message = $"Collection '{collectionName}' 刪除成功" });
            }
            return BadRequest(new QdrantResponse { Success = false, Error = "Collection 刪除失敗" });
        }
        catch (Exception ex)
        {
            return BadRequest(new QdrantResponse { Success = false, Error = ex.Message });
        }
    }

    /// <summary>
    /// 插入或更新向量點
    /// POST /api/QdrantEmbeddingApi/UpsertVectors
    /// </summary>
    [HttpPost("UpsertVectors")]
    public async Task<IActionResult> UpsertVectors([FromBody] UpsertVectorsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CollectionName))
            return BadRequest(new { success = false, error = "Collection 名稱不能為空" });

        if (request.Points == null || request.Points.Count == 0)
            return BadRequest(new { success = false, error = "向量點列表不能為空" });

        try
        {
            var result = await module.UpsertVectors(request.CollectionName, request.Points);
            if (result)
            {
                return Ok(new QdrantResponse { Success = true, Message = $"成功插入 {request.Points.Count} 個向量點" });
            }
            return BadRequest(new QdrantResponse { Success = false, Error = "向量點插入失敗" });
        }
        catch (Exception ex)
        {
            return BadRequest(new QdrantResponse { Success = false, Error = ex.Message });
        }
    }

    /// <summary>
    /// 搜尋相似向量
    /// POST /api/QdrantEmbeddingApi/Search
    /// </summary>
    [HttpPost("Search")]
    public async Task<IActionResult> Search([FromBody] SearchRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CollectionName))
            return BadRequest(new { success = false, error = "Collection 名稱不能為空" });

        if (request.QueryVector == null || request.QueryVector.Count == 0)
            return BadRequest(new { success = false, error = "查詢向量不能為空" });

        try
        {
            var results = await module.Search(
                request.CollectionName,
                request.QueryVector,
                request.Limit,
                request.ScoreThreshold,
                request.Filter);

            return Ok(new SearchResponse
            {
                Success = true,
                Results = results
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new SearchResponse
            {
                Success = false,
                Error = ex.Message
            });
        }
    }

    /// <summary>
    /// 刪除所有向量點
    /// DELETE /api/QdrantEmbeddingApi/DeleteAllPoints?collectionName=xxx
    /// </summary>
    [HttpDelete("DeleteAllPoints")]
    public async Task<IActionResult> DeleteAllPoints([FromQuery] string? collectionName)
    {
        try
        {
            var result = await module.DeleteAllPoints(collectionName);
            if (result)
            {
                return Ok(new QdrantResponse { Success = true, Message = "所有向量點已刪除" });
            }
            return BadRequest(new QdrantResponse { Success = false, Error = "刪除向量點失敗" });
        }
        catch (Exception ex)
        {
            return BadRequest(new QdrantResponse { Success = false, Error = ex.Message });
        }
    }
}

