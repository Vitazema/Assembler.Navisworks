using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Server.Assembler.Domain.Dto;
using Server.Assembler.Domain.Dto.DataObjects;
using Server.Assembler.Domain.Dto.JsonResponse;

namespace Bim360.Assembler.Services
{
  public class DocumentManagementService
  {
    private const string HubId = "b.1153ac7d-c1ab-4f9d-9837-26d6df6ec56a";
    private const string AccountId = "1153ac7d-c1ab-4f9d-9837-26d6df6ec56a";

    private const string HubsRequestStr = "project/v1/hubs/";
    private const string ProjectsRequestStr = "data/v1/projects/";
    private const string AccountsRequestStr = "hq/v1/accounts/";

    private const string VndHeader = "application/vnd.api+json";
    private const string JsonHeader = "application/json";
    private const string OctetStreamHeader = "application/octet-stream";

    private const string ProjectFiles = "Project Files";
    private const string Plans = "Plans";

    private string _accessToken = string.Empty;
    private HttpClient _client;

    /// <summary>
    ///   Загружает заданный файл в BIM360
    /// </summary>
    /// <param name="fileInfoDto">Содержит информацию о загружаемом документе</param>
    /// <param name="getAccessToken">Указывает, должен ли быть получен токен доступа</param>
    public async Task UploadFile(FileInfoDto fileInfoDto, bool getAccessToken = true)
    {
      if (!IsValidFileInfo(fileInfoDto))
        throw new Exception("Информация о файле некорректна error code: 400");

      if (getAccessToken)
        await InitHttpClient();

      var targetProject = await GetProjectFolder(fileInfoDto.TargetProjectName, $"{HubsRequestStr}{HubId}/projects");
      if (targetProject == null)
        throw new Exception("Проект с заданным именем не найден error code: 500");

      var targetFolder = await GetTargetFolder(targetProject.Id, fileInfoDto.TargetFolderName, fileInfoDto.MainFolder);
      if (targetFolder == null)
        throw new Exception("Папка с заданным именем не найдена error code: 500");

      var storage = await GetStorage(targetProject.Id, targetFolder.Id, fileInfoDto.FileName);
      if (storage == null)
        throw new Exception("Ошибка создания хранилища для файла error code: 500");

      if (!await TryToUpload(fileInfoDto, storage))
        throw new Exception("Ошибка в процессе загрузки файла error code: 500");

      if (!await TryToCreateFileVersion(fileInfoDto, targetProject, targetFolder, storage))
        throw new Exception("Ошибка в процессе создания версии файла error code: 500");
    }

    /// <summary>
    ///   Загружает список заданных файлов в BIM360
    /// </summary>
    /// <param name="fileInfoDtos">Коллекция объектов, содержащих информацию о загружаемых документах</param>
    public async Task UploadFiles(List<FileInfoDto> fileInfoDtos)
    {
      await InitHttpClient();

      var tasks = fileInfoDtos.ToDictionary(k => k, v => UploadFile(v, false));

      try
      {
        Task.WaitAll(tasks.Values.ToArray());
      }
      catch (AggregateException)
      {
        var errorMessage = string.Join(
          Environment.NewLine,
          tasks.Where(t => t.Value.Exception != null)
            .Select(t => $"Файл: {t.Key.FileName}, Ошибка: {t.Value.Exception.Message}"));

        throw new Exception($"Ошибка загрузки файлов.{Environment.NewLine}{errorMessage} error code: 500");
      }
    }

    /// <summary>
    ///   Создает папку в BIM360
    /// </summary>
    /// <param name="folderInfoDto">Информация о папке</param>
    /// <returns>Результат создания папки</returns>
    public async Task CreateFolder(FolderInfoDto folderInfoDto)
    {
      if (!IsValidFolderInfo(folderInfoDto))
        throw new Exception("Информация о папке некорректна error code: 400");

      await InitHttpClient();

      var targetProject = await GetProjectFolder(folderInfoDto.ProjectName, $"{HubsRequestStr}{HubId}/projects");
      if (targetProject == null)
        throw new Exception("Проект с заданным именем не найден error code: 500");

      var targetFolder =
        await GetTargetFolder(targetProject.Id, folderInfoDto.ParentFolderName, folderInfoDto.MainFolder);
      if (targetFolder == null)
        throw new Exception("Папка с заданным именем не найдена error code: 500");

      var requestUrl = $"{ProjectsRequestStr}{targetProject.Id}/{Constants.Commands}";
      var response = await PostJsonAsync(
        requestUrl,
        JsonBodyCreator.GetCreateFolder(folderInfoDto.FolderName, targetFolder.Id),
        VndHeader);

      if (!response.IsSuccessStatusCode)
        throw new Exception("Ошибка в процессе создания папки error code: 400");
    }

    /// <summary>
    ///   Создает проект в BIM360
    /// </summary>
    /// <param name="projectInfoDto">Информация о проекте</param>
    /// <returns>Результат создания проекта</returns>
    public async Task CreateProject(ProjectInfoDto projectInfoDto)
    {
      if (!IsValidProjectInfo(projectInfoDto))
        throw new Exception("Информация о проекте некорректна error code: 400");

      await InitHttpClient();

      var requestUrl = $"{AccountsRequestStr}{AccountId}/{Constants.Projects}";
      var response = await PostJsonAsync(
        requestUrl,
        JsonBodyCreator.GetProjectCreationRequest(projectInfoDto),
        JsonHeader);

      if (!response.IsSuccessStatusCode)
        throw new Exception("Ошибка в процессе создания проекта error code: 400");
    }

    /// <summary>
    ///   Получает токен доступа и инициализирует Http клиент
    /// </summary>
    private async Task InitHttpClient()
    {
      _accessToken = await new AuthService().GetAccessToken();
      _client = GetHttpClient();
    }

    /// <summary>
    ///   Проверяет информацию о файле
    /// </summary>
    /// <param name="fileInfoDto">Информация о файле</param>
    private bool IsValidFileInfo(FileInfoDto fileInfoDto)
    {
      return fileInfoDto != null &&
             !string.IsNullOrEmpty(fileInfoDto.Path) &&
             !string.IsNullOrEmpty(fileInfoDto.FileName) &&
             !string.IsNullOrEmpty(fileInfoDto.TargetProjectName) &&
             !string.IsNullOrEmpty(fileInfoDto.TargetFolderName) &&
             (fileInfoDto.MainFolder == MainFolder.File || fileInfoDto.MainFolder == MainFolder.Plan) &&
             fileInfoDto.Path.EndsWith(fileInfoDto.FileName);
    }

    /// <summary>
    ///   Проверяет информацию о папке
    /// </summary>
    /// <param name="folderInfoDto">Информация о папке</param>
    private bool IsValidFolderInfo(FolderInfoDto folderInfoDto)
    {
      return !string.IsNullOrEmpty(folderInfoDto.ProjectName) &&
             !string.IsNullOrEmpty(folderInfoDto.ParentFolderName) &&
             !string.IsNullOrEmpty(folderInfoDto.FolderName) &&
             (folderInfoDto.MainFolder == MainFolder.File || folderInfoDto.MainFolder == MainFolder.Plan);
    }

    /// <summary>
    ///   Проверяет информацию о проекте
    /// </summary>
    /// <param name="projectInfoDto">Информация о проекте</param>
    private bool IsValidProjectInfo(ProjectInfoDto projectInfoDto)
    {
      return true;
    }

    /// <summary>
    ///   Возвращает папку проекта с заданным именем
    /// </summary>
    /// <param name="projectFolderName">Имя папки проекта</param>
    /// <param name="projectsRequestStr">Строка запроса</param>
    /// <returns>Папка проекта</returns>
    private async Task<ItemDto> GetProjectFolder(string projectFolderName, string projectsRequestStr)
    {
      var projects = await GetItems(projectsRequestStr, ItemType.Project);
      return projects.FirstOrDefault(project => project.Name.Trim().ToUpper() == projectFolderName.Trim().ToUpper());
    }

    /// <summary>
    ///   Возвращает список элементов Data Management для заданного запроса
    /// </summary>
    /// <param name="request">Строка запроса</param>
    /// <param name="itemType">Тип искомых элементов</param>
    /// <returns>Список папок Data Management для заданного запроса</returns>
    private async Task<List<ItemDto>> GetItems(string request, ItemType itemType)
    {
      var response = await _client.GetAsync(request);
      // Shoud be Newtonsoft.Json because Text.Json cannot serealize
      var foldersResponse = JsonConvert.DeserializeObject<ItemsResponse>(await response.Content.ReadAsStringAsync());

      var items = itemType switch
      {
        ItemType.Folder => foldersResponse.Data.Where(item => item.Type == Constants.Folders),
        ItemType.Project => foldersResponse.Data.Where(item => item.Type == Constants.Projects),
        ItemType.Item => foldersResponse.Data.Where(item => item.Type == Constants.Items),
        _ => new List<IdentifiedDataExtended>()
      };

      return items.Select(item =>
        new ItemDto
        {
          Type = item.Type,
          Id = item.Id,
          Name = item.Attributes.Name ?? string.Empty,
          DisplayName = item.Attributes.DisplayName ?? string.Empty,
          Version = item.Attributes.Extension.Version
        }).ToList();
    }

    /// <summary>
    ///   Возвращает папку по заданному имени
    /// </summary>
    /// <param name="projectFolderId">Идентификатор папки проекта</param>
    /// <param name="targetFolderName">Имя целевой папки</param>
    /// <param name="mainFolder">Тип главной папки</param>
    /// <returns>Список папок дерева Data Management</returns>
    private async Task<ItemDto> GetTargetFolder(string projectFolderId, string targetFolderName, MainFolder mainFolder)
    {
      var topFoldersRequestStr = $"{HubsRequestStr}{HubId}/{Constants.Projects}/{projectFolderId}/topFolders";
      var topFolders = await GetItems(topFoldersRequestStr, ItemType.Folder);
      var topFolder = mainFolder switch
      {
        MainFolder.File =>
        topFolders.FirstOrDefault(folder => folder.Name == ProjectFiles) ?? new ItemDto(),
        MainFolder.Plan =>
        topFolders.FirstOrDefault(folder => folder.Name == Plans) ?? new ItemDto(),
        _ => null
      };

      if (topFolder == null)
        return null;

      if (topFolder.Name == targetFolderName)
        return topFolder;

      return await FindTargetFolder(projectFolderId, topFolder.Id, targetFolderName);
    }

    /// <summary>
    ///   Возвращает список папок Data Management для заданного запроса
    /// </summary>
    /// <param name="projectId">Идентификатор проекта</param>
    /// <param name="folderId">Идентификатор папки</param>
    /// <param name="folderName">Имя папки для поиска</param>
    private async Task<ItemDto> FindTargetFolder(
      string projectId,
      string folderId,
      string folderName)
    {
      var folders = await GetItems(
        $"{ProjectsRequestStr}{projectId}/{Constants.Folders}/{folderId}/contents",
        ItemType.Folder);
      if (!folders.Any())
        return null;

      var foundFolderDto = folders.FirstOrDefault(folder =>
        folder.Name.Trim().ToUpper() == folderName.Trim().ToUpper());
      if (foundFolderDto != null)
        return foundFolderDto;

      foreach (var folder in folders)
      {
        var targetFolderDto = await FindTargetFolder(projectId, folder.Id, folderName);
        if (targetFolderDto != null)
          return targetFolderDto;
      }

      return null;
    }

    /// <summary>
    ///   Возвращает клиент для доступа к API BIM360
    /// </summary>
    /// <returns>Клиент для доступа к API BIM360</returns>
    private HttpClient GetHttpClient()
    {
      var httpClient = new HttpClient
      {
        BaseAddress = new Uri(Constants.ApiUrl),
        Timeout = TimeSpan.FromMinutes(15),
        DefaultRequestHeaders = {Authorization = new AuthenticationHeaderValue("Bearer", _accessToken)}
      };

      return httpClient;
    }

    /// <summary>
    ///   Возвращает данные хранилища для загружаемого файла
    /// </summary>
    /// <param name="targetProjectId">Идентификатор папки проекта</param>
    /// <param name="targetFolderId">Идентификатор папки для загрузки</param>
    /// <param name="fileName">Имя файла</param>
    /// <returns>Данные хранилища для загружаемого файла</returns>
    private async Task<StorageDto> GetStorage(
      string targetProjectId,
      string targetFolderId,
      string fileName)
    {
      var requestUrl = $"{ProjectsRequestStr}{targetProjectId}/{Constants.Storage}";
      var response = await PostJsonAsync(
        requestUrl,
        JsonBodyCreator.GetCreateStorage(fileName, targetFolderId),
        VndHeader);

      // Shoud be Newtonsoft.Json because Text.Json cannot serealize
      var storageResponse = JsonConvert.DeserializeObject<CreationResponse>(await response.Content.ReadAsStringAsync());
      var storageId = storageResponse.Data.Id;
      var storageIdEndingSplit = storageId.Split(':').Last().Split('/');

      return new StorageDto
      {
        Type = storageResponse.Data.Type,
        Id = storageId,
        BucketKey = storageIdEndingSplit[0],
        Name = storageIdEndingSplit[1]
      };
    }

    /// <summary>
    ///   Отправляет POST запрос с JSON контентом
    /// </summary>
    /// <param name="requestUrl">Адрес запроса</param>
    /// <param name="jsonContent">JSON контент</param>
    /// <param name="header">заголовок запроса</param>
    /// <returns>Результат запроса</returns>
    private async Task<HttpResponseMessage> PostJsonAsync(
      string requestUrl,
      string jsonContent,
      string header)
    {
      var client = GetHttpClient();
      client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(header));
      var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
      {
        Content = new StringContent(jsonContent, Encoding.UTF8)
      };
      request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(header);

      var response = await client.SendAsync(request);

      return response;
    }

    /// <summary>
    ///   Выполняет загрузку выбранного файла
    /// </summary>
    /// <param name="fileInfoDto">Информация о файле</param>
    /// <param name="storage">Хранилище файла</param>
    /// <returns>Результат загрузки выбранного файла</returns>
    private async Task<bool> TryToUpload(FileInfoDto fileInfoDto, StorageDto storage)
    {
      try
      {
        var uploadRequest = $"oss/v2/buckets/{storage.BucketKey}/{Constants.Objects}/{storage.Name}";
        byte[] byteArray;
        if (!new Uri(fileInfoDto.Path).IsFile)
        {
          var httpHandler = new HttpClientHandler {Credentials = AuthService.GetNetworkCredential()};
          var client = new HttpClient(httpHandler);

          var fileResponse = await client.GetAsync(fileInfoDto.Path);
          byteArray = await fileResponse.Content.ReadAsByteArrayAsync();
        }
        else
        {
          byteArray = await File.ReadAllBytesAsync(fileInfoDto.Path);
        }

        var content = new ByteArrayContent(byteArray);
        content.Headers.ContentType = MediaTypeHeaderValue.Parse(OctetStreamHeader);

        var response = await _client.PutAsync(uploadRequest, content);

        return response.IsSuccessStatusCode;
      }
      catch (Exception e)
      {
        throw new Exception(
          $"Ошибка в процессе загрузки файла. {e.Message} {e.InnerException?.Message} error code: 400");
      }
    }

    /// <summary>
    ///   Выполняет создание версии файла
    /// </summary>
    /// <param name="fileInfo">Информация о файле</param>
    /// <param name="targetProject">Целевой проект</param>
    /// <param name="targetFolder">Целевая папка</param>
    /// <param name="storage">Данные хранилища</param>
    /// <returns>Результат создания версии файла</returns>
    private async Task<bool> TryToCreateFileVersion(
      FileInfoDto fileInfo,
      ItemDto targetProject,
      ItemDto targetFolder,
      StorageDto storage)
    {
      ItemDto previousVersion = null;
      var items = await GetItems(
        $"{ProjectsRequestStr}{targetProject.Id}/{Constants.Folders}/{targetFolder.Id}/contents",
        ItemType.Item);
      if (items.Any())
        previousVersion = items.FirstOrDefault(item => item.DisplayName == fileInfo.FileName);

      string requestUrl;
      HttpResponseMessage response;
      if (previousVersion == null)
      {
        requestUrl = $"{ProjectsRequestStr}{targetProject.Id}/{Constants.Items}";
        response = await PostJsonAsync(
          requestUrl,
          JsonBodyCreator.GetCreateFileVersion(fileInfo.FileName, targetFolder.Id, storage.Id),
          VndHeader);

        return response.IsSuccessStatusCode;
      }

      requestUrl = $"{ProjectsRequestStr}{targetProject.Id}/{Constants.Versions}";
      response = await PostJsonAsync(
        requestUrl,
        JsonBodyCreator.GetCreateAdditionalFileVersion(fileInfo.FileName, storage.Id, previousVersion.Id),
        VndHeader);

      return response.IsSuccessStatusCode;
    }
  }
}