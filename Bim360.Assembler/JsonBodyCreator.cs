using System;
using Newtonsoft.Json;
using Server.Assembler.Domain.Dto;

namespace Bim360.Assembler
{
  /// <summary>
  ///   Создает JSON тело запроса
  /// </summary>
  public static class JsonBodyCreator
  {
    private const string FolderTypeStr = "autodesk.bim360:Folder";
    private const string FileTypeStr = "autodesk.bim360:File";
    private const string CreateFolderTypeStr = "autodesk.core:CreateFolder";

    private const string V1 = "1.0";

    /// <summary>
    ///   Возвращает JSON строку запроса на создание хранилища
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="targetFolderId">Идентификатор целевой папки</param>
    /// <returns>JSON строка запроса на создание хранилища</returns>
    public static string GetCreateStorage(string fileName, string targetFolderId)
    {
      var storageData = new
      {
        jsonapi = new {version = V1},
        data = new
        {
          type = Constants.Objects,
          attributes = new
          {
            name = fileName
          },
          relationships = new
          {
            target = new
            {
              data = new
              {
                type = Constants.Folders,
                id = targetFolderId
              }
            }
          }
        }
      };

      return JsonConvert.SerializeObject(storageData);
    }

    /// <summary>
    ///   Возвращает JSON строку запроса на создание версии файла
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="targetFolderId">Идентификатор целевой папки</param>
    /// <param name="storageId">Идентификатор хранилища</param>
    /// <returns>JSON строка запроса на создание версии файла</returns>
    public static string GetCreateFileVersion(
      string fileName,
      string targetFolderId,
      string storageId)
    {
      var fileVersionData = new
      {
        jsonapi = new {version = V1},
        data = new
        {
          type = Constants.Items,
          attributes = new
          {
            displayName = fileName,
            extension = new
            {
              type = $"{Constants.Items}:{FileTypeStr}",
              version = V1
            }
          },
          relationships = new
          {
            tip = new
            {
              data = new
              {
                type = Constants.Versions,
                id = "1"
              }
            },
            parent = new
            {
              data = new
              {
                type = Constants.Folders,
                id = targetFolderId
              }
            }
          }
        },
        included = new[]
        {
          new
          {
            type = Constants.Versions,
            id = "1",
            attributes = new
            {
              name = fileName,
              extension = new
              {
                type = $"{Constants.Versions}:{FileTypeStr}",
                version = V1
              }
            },
            relationships = new
            {
              storage = new
              {
                data = new
                {
                  type = Constants.Objects,
                  id = storageId
                }
              }
            }
          }
        }
      };

      return JsonConvert.SerializeObject(fileVersionData);
    }

    /// <summary>
    ///   Возвращает JSON строку запроса на создание дополнительной версии файла
    /// </summary>
    /// <param name="fileName">Имя файла</param>
    /// <param name="storageId">Идентификатор хранилища</param>
    /// <param name="additionalVersionId">Идентификатор дополнительной версии</param>
    /// <returns>JSON строка запроса на создание дополнительной версии файла</returns>
    public static string GetCreateAdditionalFileVersion(
      string fileName,
      string storageId,
      string additionalVersionId)
    {
      var fileVersionData = new
      {
        jsonapi = new {version = V1},
        data = new
        {
          type = Constants.Versions,
          attributes = new
          {
            name = fileName,
            extension = new
            {
              type = $"{Constants.Versions}:{FileTypeStr}",
              version = V1
            }
          },
          relationships = new
          {
            item = new
            {
              data = new
              {
                type = Constants.Items,
                id = additionalVersionId
              }
            },
            storage = new
            {
              data = new
              {
                type = Constants.Objects,
                id = storageId
              }
            }
          }
        }
      };

      return JsonConvert.SerializeObject(fileVersionData);
    }

    /// <summary>
    ///   Возвращает JSON строку запроса на создание папки
    /// </summary>
    /// <param name="folderName">Имя папки</param>
    /// <param name="parentFolderId">Идентификатор родительской папки</param>
    /// <returns>JSON строка запроса на создание папки</returns>
    public static string GetCreateFolder(string folderName, string parentFolderId)
    {
      var folderId = Guid.NewGuid().ToString();
      var folderData = new
      {
        jsonapi = new {version = V1},
        data = new
        {
          type = Constants.Commands,
          attributes = new
          {
            extension = new
            {
              type = $"{Constants.Commands}:{CreateFolderTypeStr}",
              version = $"{V1}.0"
            }
          },
          relationships = new
          {
            resources = new
            {
              data = new[]
              {
                new
                {
                  type = Constants.Folders,
                  id = folderId
                }
              }
            }
          }
        },
        included = new[]
        {
          new
          {
            type = Constants.Folders,
            id = folderId,
            attributes = new
            {
              name = folderName,
              extension = new
              {
                type = $"{Constants.Folders}:{FolderTypeStr}",
                version = V1
              }
            },
            relationships = new
            {
              parent = new
              {
                data = new
                {
                  type = Constants.Folders,
                  id = parentFolderId
                }
              }
            }
          }
        }
      };

      return JsonConvert.SerializeObject(folderData);
    }

    /// <summary>
    ///   Возвращает JSON строку запроса на создание проекта
    /// </summary>
    /// <param name="projectInfoDto">Информация о проекте</param>
    /// <returns>JSON строка запроса на создание проекта</returns>
    public static string GetProjectCreationRequest(ProjectInfoDto projectInfoDto)
    {
      var projectData = new
      {
        name = projectInfoDto.Name,
        service_types = "field",
        start_date = "2015-05-02",
        end_date = "2016-04-03",
        project_type = "office",
        value = 3000,
        currency = "USD",
        job_number = "0219-01",
        address_line_1 = Guid.NewGuid().ToString(),
        address_line_2 = Guid.NewGuid().ToString(),
        city = "New York",
        state_or_province = "New York",
        postal_code = "10011",
        country = "United States",
        business_unit_id = Guid.NewGuid().ToString(),
        timezone = "America/New_York",
        language = "en",
        construction_type = "Renovation",
        contract_type = "Design-Bid"
      };

      return JsonConvert.SerializeObject(projectData);
    }
  }
}