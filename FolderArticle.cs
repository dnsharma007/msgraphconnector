using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using articlesFreshServiceConnector.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json.Serialization;

namespace articlesFreshServiceConnector.Data
{
    public class FolderArticle
    {
        public FolderArticle()
        {
            LastUpdated = DateTime.UtcNow;
        }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
        /*public List<Int64>? GroupFolderGroupIds { get; set; }
        public List<Int64>? FolderDepartmentIds { get; set; }
        public List<Int64>? GroupFolderRequesterGroupIds { get; set; }
        public List<Int64>? GroupFolderDepartmentIds { get; set; }
        public List<Int64>? GroupFolderWorkspaceIds { get; set; }
        public List<object>? Attachments { get; set; }*/
        [JsonPropertyName("Folder_Visibility")]
        public int? FolderVisibility { get; set; }
        [JsonPropertyName("id")]
        public Int64 Id { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime? CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime? UpdatedAt { get; set; }
        [JsonPropertyName("title")]
        public string? Title { get; set; }
        [JsonPropertyName("status")]
        public int? Status { get; set; }
        [JsonPropertyName("user_id")]
        public Int64? UserId { get; set; }
        [JsonPropertyName("source")]
        public string? Source { get; set; }
        [JsonPropertyName("source_name")]
        public string? SourceName { get; set; }
        [JsonPropertyName("approval_status")]
        public int? ApprovalStatus { get; set; }
        [JsonPropertyName("position")]
        public int? Position { get; set; }
        public SolutionFolder SolutionFolder { get; set; }
        [JsonPropertyName("folder_id")]
        public Int64 FolderId { get; set; }
        // public SolutionCategory SolutionCategory { get; set; }
        [JsonPropertyName("category_id")]
        public Int64 CategoryId { get; set; }
        [JsonPropertyName("thumbs_up")]
        public Int64? ThumbsUp { get; set; }
        [JsonPropertyName("thumbs_down")]
        public Int64? ThumbsDown { get; set; }
        [JsonPropertyName("modified_by")]
        public Int64? ModifiedBy { get; set; }
        [JsonPropertyName("modified_at")]
        public DateTime? ModifiedAt { get; set; }
        [JsonPropertyName("inserted_into_tickets")]
        public int? InsertedIntoTickets { get; set; }
        [JsonPropertyName("")]
        public string? Url { get; set; }
        [JsonPropertyName("workspace_id")]
        public Int64? WorkspaceId { get; set; }
        [JsonPropertyName("article_type")]
        public int? ArticleType { get; set; }
        [JsonPropertyName("views")]
        public Int64? Views { get; set; }
        [JsonPropertyName("description_text")]
        public string? DescriptionText { get; set; }
        [JsonPropertyName("keywords")]
        public string[]? Keywords { get; set; }
        public string? keyword { get; set; }
        [JsonPropertyName("review_date")]
        public DateTime? ReviewDate { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsDeleted { get; set; }

        public async Task<List<FolderArticle>> GetFolderArticleFromAPI(string apiUrl, string propName)
        {
            HttpClientWrapper httpClient = new HttpClientWrapper();

            try
            {
                Console.WriteLine("Getting Articles from API : " + apiUrl);
                var articles = new List<FolderArticle>();
                var articlesArray = await httpClient.GetAsync(apiUrl, propName);
                foreach (var articleJson in articlesArray)
                {
                    var article = JsonSerializer.Deserialize<FolderArticle>(articleJson.GetRawText());
                    articles.Add(article);
                    Console.WriteLine(article.Id);

                }

                return articles;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch data. Status code: {ex.Message}");
                return new List<FolderArticle>();
            }
        }
    }
}
