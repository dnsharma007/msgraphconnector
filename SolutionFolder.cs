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
using Microsoft.Graph.Models;
using System.Linq.Expressions;

namespace articlesFreshServiceConnector.Data
{
   
    public class SolutionFolder
    {
        public SolutionFolder()
        {
            FolderArticles = new List<FolderArticle>();
        }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }
        [JsonPropertyName("id")]
        public Int64 Id { get; set; }
        [JsonPropertyName("created_at")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("updated_at")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        public SolutionCategory SolutionCategory { get; set; }
        [JsonPropertyName("category_id")]
        public Int64? CategoryId { get; set; }
        [JsonPropertyName("position")]
        public int? Position { get; set; }
        [JsonPropertyName("visibility")]
        public int? Visibility { get; set; }
        [JsonPropertyName("approval_settings")]
        public Json? ApprovalSettings { get; set; } // Define ApprovalSettings class
        [JsonPropertyName("workspace_id")]
        public int? WorkspaceId { get; set; }
        [JsonPropertyName("default_folder")]
        public bool? DefaultFolder { get; set; }
        [JsonPropertyName("parent_id")]
        public Int64? ParentId { get; set; }
        [JsonPropertyName("has_sub_folders")]
        public bool? HasSubfolders { get; set; }
        /*
        [JsonPropertyName("Manage_By_Group_Ids")]
        public Json? ManageByGroupIds { get; set; }*/
        public List<FolderArticle>? FolderArticles { get; set; }

       
        public async Task<List<SolutionFolder>> GetFoldersFromAPI(string apiUrl, string propName)
        {
            HttpClientWrapper httpClient = new HttpClientWrapper();

            try
            {
                Console.WriteLine("Getting Folders from API : " + apiUrl);
                var folders = new List<SolutionFolder>();
                var foldersArray = await httpClient.GetAsync(apiUrl, propName);
                foreach (var folderJson in foldersArray)
                {
                    var folder = JsonSerializer.Deserialize<SolutionFolder>(folderJson.GetRawText());
                    folders.Add(folder);
                    Console.WriteLine(folder.Id);
                }

                return folders;
            }
                catch(Exception ex)
                {
                    Console.WriteLine($"Failed to fetch data. Erroee: {ex.Message}");
                    return new List<SolutionFolder>();
                }
            
        }

    }
   
    public class ApprovalSettings
    {   
        public string ApproverIds { get; set; }
        public int ApprovalType { get; set; }
    }
}
