// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

// <ProgramSnippet>
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Models.ExternalConnectors;
using Microsoft.Graph.Models.ODataErrors;
using articlesFreshServiceConnector;
using articlesFreshServiceConnector.Data;
using articlesFreshServiceConnector.Graph;
using System.Text.Json;

Console.WriteLine("Articles Fresh Service Connector\n");

var settings = Settings.LoadSettings();

IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

// Initialize Graph
InitializeGraph(settings);

ExternalConnection? currentConnection = null;
int choice = -1;

while (choice != 0)
{
    Console.WriteLine($"Current connection: {(currentConnection == null ? "NONE" : currentConnection.Name)}\n");
    Console.WriteLine("Please choose one of the following options:");
    Console.WriteLine("0. Exit");
    Console.WriteLine("1. Create a connection");
    Console.WriteLine("2. Select an existing connection");
    Console.WriteLine("3. Delete current connection");
    Console.WriteLine("4. Register schema for current connection");
    Console.WriteLine("5. View schema for current connection");
    Console.WriteLine("6. Get items From Fresh Service");
    Console.WriteLine("7. Push updated items to current connection");
    Console.WriteLine("8. Push ALL items to current connection");
    Console.Write("Selection: ");

    try
    {
        choice = int.Parse(Console.ReadLine() ?? string.Empty);
    }
    catch (FormatException)
    {
        // Set to invalid value
        choice = -1;
    }

    switch (choice)
    {
        case 0:
            // Exit the program
            Console.WriteLine("Goodbye...");
            break;
        case 1:
            currentConnection = await CreateConnectionAsync();
            break;
        case 2:
            currentConnection = await SelectExistingConnectionAsync();
            break;
        case 3:
            await DeleteCurrentConnectionAsync(currentConnection);
            currentConnection = null;
            break;
        case 4:
            await RegisterSchemaAsync();
            break;
        case 5:
            await GetSchemaAsync();
            break;
        case 6:
            await SaveSolutionCategoryFromAPI();
            break;
        case 7:
            await UpdateItemsFromDatabaseAsync(true, settings.TenantId);
            break;
        case 8:
            await UpdateItemsFromDatabaseAsync(false, settings.TenantId);
            break;
        default:
            Console.WriteLine("Invalid choice! Please try again.");
            break;
    }
}

static string? PromptForInput(string prompt, bool valueRequired)
{
    string? response;

    do
    {
        Console.WriteLine($"{prompt}:");
        response = Console.ReadLine();
        if (valueRequired && string.IsNullOrEmpty(response))
        {
            Console.WriteLine("You must provide a value");
        }
    } while (valueRequired && string.IsNullOrEmpty(response));

    return response;
}

static DateTime GetLastUploadTime()
{
    if (File.Exists("lastuploadtime.bin"))
    {
        return DateTime.Parse(
            File.ReadAllText("lastuploadtime.bin")).ToUniversalTime();
    }

    return DateTime.MinValue;
}
static void SaveLastUploadTime(DateTime uploadTime)
{
    File.WriteAllText("lastuploadtime.bin", uploadTime.ToString("u"));
}

static void SaveLastDownloadTime(DateTime downloadTime)
{
    File.WriteAllText("lastdownloadtime.bin", downloadTime.ToString("u"));
}

static DateTime GetLastDownloadTime()
{
    if (File.Exists("lastdownloadtime.bin"))
    {
        return DateTime.Parse(
            File.ReadAllText("lastdownloadtime.bin")).ToUniversalTime();
    }

    return DateTime.MinValue;
}



// </ProgramSnippet>

// <InitializeGraphSnippet>
void InitializeGraph(Settings settings)
{
    try
    {
        GraphHelper.Initialize(settings);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing Graph: {ex.Message}");
    }
}
// </InitializeGraphSnippet>

// <CreateConnectionSnippet>
async Task<ExternalConnection?> CreateConnectionAsync()
{
    var connectionId = PromptForInput(
        "Enter a unique ID for the new connection (3-32 characters)", true) ?? "ConnectionId";
    var connectionName = PromptForInput(
        "Enter a name for the new connection", true) ?? "ConnectionName";
    var connectionDescription = PromptForInput(
        "Enter a description for the new connection", false);

    try
    {
        // Create the connection
        var connection = await GraphHelper.CreateConnectionAsync(
            connectionId, connectionName, connectionDescription);
        Console.WriteLine($"New connection created - Name: {connection?.Name}, Id: {connection?.Id}");
        return connection;
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error creating connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        return null;
    }
}
// </CreateConnectionSnippet>

// <GetConnectionsSnippet>
async Task<ExternalConnection?> SelectExistingConnectionAsync()
{
    // TODO
    Console.WriteLine("Getting existing connections...");
    try
    {
        var response = await GraphHelper.GetExistingConnectionsAsync();
        var connections = response?.Value ?? new List<ExternalConnection>();
        if (connections.Count <= 0)
        {
            Console.WriteLine("No connections exist. Please create a new connection");
            return null;
        }

        // Display connections
        Console.WriteLine("Choose one of the following connections:");
        var menuNumber = 1;
        foreach (var connection in connections)
        {
            Console.WriteLine($"{menuNumber++}. {connection.Name}");
        }

        ExternalConnection? selection = null;

        do
        {
            try
            {
                Console.Write("Selection: ");
                var choice = int.Parse(Console.ReadLine() ?? string.Empty);
                if (choice > 0 && choice <= connections.Count)
                {
                    selection = connections[choice - 1];
                }
                else
                {
                    Console.WriteLine("Invalid choice.");
                }
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid choice.");
            }
        } while (selection == null);

        return selection;
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error getting connections: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        return null;
    }
}
// </GetConnectionsSnippet>

// <DeleteConnectionSnippet>
async Task DeleteCurrentConnectionAsync(ExternalConnection? connection)
{
    if (connection == null)
    {
        Console.WriteLine(
            "No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    try
    {
        await GraphHelper.DeleteConnectionAsync(connection.Id);
        Console.WriteLine($"{connection.Name} deleted successfully.");
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error deleting connection: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}
// </DeleteConnectionSnippet>

// <RegisterSchemaSnippet>
async Task RegisterSchemaAsync()
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    Console.WriteLine("Registering schema, this may take a moment...");

    try
    {
        // Create the schema
        var schema = new Schema
        {
            BaseType = "microsoft.graph.externalItem",
            Properties = new List<Property>
           {
                new Property { Name = "articleId", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true },
                new Property { Name = "title", Type = PropertyType.String, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false, Labels = new List<Label?>() { Label.Title }},
                new Property { Name = "description", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "userId", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true },
                new Property { Name = "attachments", Type = PropertyType.StringCollection, IsQueryable = true, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "folderId", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true },
                new Property { Name = "folderName", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "categoryId", Type = PropertyType.String, IsQueryable = true, IsSearchable = false, IsRetrievable = true, IsRefinable = true },
                new Property { Name = "categoryName", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false },
                new Property { Name = "url", Type = PropertyType.String, IsQueryable = false, IsSearchable = true, IsRetrievable = true, IsRefinable = false }
             },
        };

        await GraphHelper.RegisterSchemaAsync(currentConnection.Id, schema);
        Console.WriteLine("Schema registered successfully");
    }
    catch (ServiceException serviceException)
    {
        Console.WriteLine($"Error registering schema: {serviceException.ResponseStatusCode} {serviceException.Message}");
    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error registering schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}
// </RegisterSchemaSnippet>

// <GetSchemaSnippet>
async Task GetSchemaAsync()
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    try
    {
        var schema = await GraphHelper.GetSchemaAsync(currentConnection.Id);
        Console.WriteLine(JsonSerializer.Serialize(schema));

    }
    catch (ODataError odataError)
    {
        Console.WriteLine($"Error getting schema: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
    }
}
// </GetSchemaSnippet>

// <UpdateItemsFromDatabaseSnippet>
async Task UpdateItemsFromDatabaseAsync(bool uploadModifiedOnly, string? tenantId)
{
    if (currentConnection == null)
    {
        Console.WriteLine("No connection selected. Please create a new connection or select an existing connection.");
        return;
    }

    _ = tenantId ?? throw new ArgumentException("tenantId is null");

    List<Article>? articlesToUpload = null;
    //List<FolderArticle>? articlesToDelete = null;

    var newUploadTime = DateTime.UtcNow;
    var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
    optionsBuilder.UseSqlite("Data Source=Articles.db");

    var context = new ArticleDbContext(optionsBuilder.Options);
    context.EnsureDatabase();
    var lastUploadTime = GetLastUploadTime();
    var lastDownloadTime = GetLastDownloadTime();

    List<FolderArticle>? articlesToDelete = context.FolderArticles
            .Where(p=> p.IsDeleted || p.LastUpdated < lastDownloadTime)
            .ToList();
    
    if (uploadModifiedOnly)
    {        
        Console.WriteLine($"Uploading changes since last upload at {lastUploadTime.ToLocalTime()}");

        var query = from article in context.FolderArticles
                    join folder in context.SolutionFolder
                    on article.FolderId equals folder.Id
                    join category in context.SolutionCategory
                    on folder.CategoryId equals category.Id
                    where article.Status==2 & !article.IsDeleted
                    & article.UpdatedAt > lastUploadTime 
                    orderby article.Id
                    select new Article
                    {
                        articleId = article.Id.ToString(),
                        Title = article.Title,
                        Description = article.DescriptionText,
                        userId = article.UserId.ToString(),
                        folderId = article.FolderId.ToString(),
                        folderName = folder.Name,
                        categoryId = article.CategoryId.ToString(),
                        categoryName = category.Name                       
                    };
        articlesToUpload = query.ToList();
    }
    else
    {
        var query = from article in context.FolderArticles
                    join folder in context.SolutionFolder
                    on article.FolderId equals folder.Id
                    join category in context.SolutionCategory
                    on folder.CategoryId equals category.Id
                    where article.Status == 2 & !article.IsDeleted
                    orderby article.Id
                    select new Article
                    {
                        articleId = article.Id.ToString(),
                        Title = article.Title,
                        Description = article.DescriptionText.Substring(1,250),
                        userId = article.UserId.ToString(),
                        folderId = article.FolderId.ToString(),
                        folderName = folder.Name,
                        categoryId = article.CategoryId.ToString(),
                        categoryName = category.Name,
                        url= string.IsNullOrEmpty(article.Url) ? "https://skillsoft.freshservice.com/a/solutions/articles/" + article.Id.ToString() : article.Url
                    };
        articlesToUpload = query.ToList();
    }   
    
    Console.WriteLine($"Processing {articlesToUpload.Count} add/updates, {articlesToDelete.Count} deletes.");
    var success = true;

    int i = 1;
    foreach (var article in articlesToUpload)
    {
        var newItem = new ExternalItem
        {
            Id = article.articleId.ToString(),
            Content = new ExternalItemContent
            {
                Type = ExternalItemContentType.Text,
                Value = article.Title
            },
            Acl = new List<Acl>
            {
                new Acl
                {
                    AccessType = AccessType.Grant,
                    Type = AclType.Everyone,
                    Value = tenantId,
                }
            },
            Properties = article.AsExternalItemProperties(),
        };

        try
        {
            Console.Write($"{i.ToString()}. Uploading article number {article.articleId}...");
            await GraphHelper.AddOrUpdateItemAsync(currentConnection.Id, newItem);
            Console.WriteLine("DONE");
            i++;
        }
        catch (ODataError odataError)
        {
            // success = false;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }
   

    foreach (var article in articlesToDelete)
    {
        try
        {
            Console.Write($"Deleting article number {article.Id}...");
            await GraphHelper.DeleteItemAsync(currentConnection.Id, article.Id.ToString());
            Console.WriteLine("DONE");
        }
        catch (ODataError odataError)
        {
            success = false;
            Console.WriteLine("FAILED");
            Console.WriteLine($"Error: {odataError.ResponseStatusCode}: {odataError.Error?.Code} {odataError.Error?.Message}");
        }
    }

    // If no errors, update our last upload time
    if (success)
    {
        SaveLastUploadTime(newUploadTime);
    }
}
async Task SaveSolutionCategoryFromAPI()
{
    var newDownloadTime = DateTime.UtcNow;
    
    var lastDownloadTime = GetLastDownloadTime();

    HttpClientWrapper httpClient = new HttpClientWrapper();

    Console.WriteLine("Getting Categories from API");
    Console.WriteLine();
    var solutionCategory = new SolutionCategory();
    JsonElement.ArrayEnumerator solutionCategoryfromAPI = await httpClient.GetAsync(configuration["GetCategoryAPIUrl"], "categories");

    var success = true;
    var optionsBuilder = new DbContextOptionsBuilder<ArticleDbContext>();
    optionsBuilder.UseSqlite("Data Source=Articles.db");

    var articlesDb = new ArticleDbContext(optionsBuilder.Options);
    articlesDb.EnsureDatabase();

    Console.WriteLine("Saving Categories to Database");
    Console.WriteLine();
    foreach (var categoryJson in solutionCategoryfromAPI)
    {
        var category = new SolutionCategory
        {
            Id = categoryJson.GetProperty("id").GetInt64(),
            CreatedAt = categoryJson.GetProperty("created_at").GetDateTime(),
            UpdatedAt = categoryJson.GetProperty("updated_at").GetDateTime(),
            Name = Convert.ToString(categoryJson.GetProperty("name")),
            Description = Convert.ToString(categoryJson.GetProperty("description")),
            DefaultCategory = categoryJson.GetProperty("default_category").GetBoolean(),
            Position = categoryJson.GetProperty("position").GetInt32(),
            WorkspaceId = categoryJson.GetProperty("workspace_id").GetInt32(),
            SolutionFolders = await GetSolutionFolder(categoryJson.GetProperty("id").GetInt64().ToString())

        };

        try
        {            
               // var existingCategory = articlesDb.SolutionCategory.FindAsync(category.Id);
               if (!articlesDb.SolutionCategory.Any(x=> x.Id==category.Id))
                {
                    articlesDb.SolutionCategory.Add(category);
                }
                else
                {
                    articlesDb.SolutionCategory.Update(category);
                }

                await articlesDb.SaveChangesAsync();            
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            success = false;
            break;
        }

    }
    if (success)
    {
        SaveLastDownloadTime(newDownloadTime);
    }
    Console.WriteLine("Categories saved to Database");
    Console.WriteLine();
}
async Task<List<SolutionFolder>> GetSolutionFolder(string categoryId)
{

    var SolutionFolder = new SolutionFolder();
    var solutionFolderfromAPI = await SolutionFolder.GetFoldersFromAPI(configuration["GetFolderAPIUrl"] + categoryId, "folders");
    foreach (SolutionFolder folder in solutionFolderfromAPI)
    {
        FolderArticle folderArticle = new FolderArticle();
        var articles = await folderArticle.GetFolderArticleFromAPI(configuration["GetArticleAPIUrl"] + folder.Id, "articles");
        if (articles.Count > 0)
            folder.FolderArticles = articles;
        //folder.FolderArticles.
    }

    return solutionFolderfromAPI;
    
}
// </UpdateItemsFromDatabaseSnippet>
