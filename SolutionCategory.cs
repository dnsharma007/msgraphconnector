using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using articlesFreshServiceConnector.Data;

namespace articlesFreshServiceConnector.Data
{
    public class SolutionCategory
    {
        public SolutionCategory() 
        { 
            SolutionFolders = new List<SolutionFolder>();
        }
        [JsonPropertyName("Id")]
        public long Id { get; set; }
        [JsonPropertyName("Created_At")]
        public DateTime CreatedAt { get; set; }
        [JsonPropertyName("Updated_At")]
        public DateTime UpdatedAt { get; set; }
        [JsonPropertyName("Name")]
        public string Name { get; set; }
        [JsonPropertyName("Description")]
        public string Description { get; set; }
        [JsonPropertyName("Default_Category")]
        public bool DefaultCategory { get; set; }
        [JsonPropertyName("Position")]
        public int Position { get; set; }
        [JsonPropertyName("Workspace_Id")]
        public int WorkspaceId { get; set; }

        public List<SolutionFolder>? SolutionFolders { get; set; }
               
    }
}
