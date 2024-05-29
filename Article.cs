// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

// <ArticleSnippet>
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.Graph.Models.ExternalConnectors;

namespace articlesFreshServiceConnector.Data
{
    public class Article
    {
        [JsonPropertyName("attachments@odata.type")]
        private const string AttachmentsODataType = "Collection(String)";

        [Key]
        public string? articleId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        // public double title { get; set; }
        public string? userId { get; set; }
        public List<string>? Attachments { get; set; }
        public string? folderId { get; set; }
        public string? folderName { get; set; }
        public string? categoryId { get; set; }
        public string? categoryName { get; set; }
        public string? url { get; set; }

        public Properties AsExternalItemProperties()
        {
            _ = Title ?? throw new MemberAccessException("Title cannot be null");
            //_ = Description ?? throw new MemberAccessException("Description cannot be null");
            //_ = Attachments ?? throw new MemberAccessException("Attachments cannot be null");
            _ = folderName ?? throw new MemberAccessException("folderName cannot be null");
            _ = categoryName ?? throw new MemberAccessException("categoryName cannot be null");

            var properties = new Properties
            {
                AdditionalData = new Dictionary<string, object>
            {
                { "articleId", articleId },
                { "title", Title },
                { "description", Description },
              //  { "title", title },
                { "userId", userId },
               { "attachments@odata.type", "Collection(String)" },
                { "attachments", Attachments },
                { "folderId" , folderId},
    { "folderName", folderName},
    { "categoryId", categoryId},
    { "categoryName", categoryName},
                    {"url",url }
}
            };

            return properties;
        }
    }
}
// </ArticlePartSnippet>
