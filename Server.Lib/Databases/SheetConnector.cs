using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;

namespace Server.Lib.Databases
{
  public class SheetConnector
  {
    static readonly string[] _scopes = { SheetsService.Scope.Spreadsheets };
    private readonly string _applicationName;
    private readonly string _spreadsheetId;

    private SheetsService _sheetService;
    private UserCredential _userCredential;
    private GoogleCredential _googleCredential;

    public SheetConnector(string applicationname, string spreadsheetId)
    {
      _applicationName = applicationname;
      _spreadsheetId = spreadsheetId;
    }

    public bool SetSheetApiConnection(Stream googleCredentialStream)
    {
      _googleCredential = GoogleCredential.FromStream(googleCredentialStream).CreateScoped(_scopes);
      _sheetService = new SheetsService(new BaseClientService.Initializer
      {
        HttpClientInitializer = _googleCredential,
        ApplicationName = _applicationName
      });
      return true;
    }

    /// <summary>
    ///   Check and connect to Google Spreadsheet API. It requires user interaction.
    /// </summary>
    /// <param name="userCredentialStream"></param>
    /// <returns>Return true if service is initialize and successfuly connected.</returns>
    public bool SetSheetUserConnection(Stream userCredentialStream)
    {
      try
      {
        // The file token.json stores the user's access and refresh tokens, and is created
        // automatically when the authorization flow completes for the first time.
        var token = "userToken.json";
        _userCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
          GoogleClientSecrets.Load(userCredentialStream).Secrets,
          _scopes,
          "user",
          CancellationToken.None,
          new FileDataStore(token, true)).Result;

        // Create Google Sheets Api service
        _sheetService = new SheetsService(new BaseClientService.Initializer
        {
          HttpClientInitializer = _userCredential,
          ApplicationName = _applicationName
        });
        return true;
      }
      catch (Exception exception)
      {
        Console.WriteLine(exception.Message);
        return false;
      }
    }

    /// <summary>
    ///   Read data from given sheet.
    /// </summary>
    /// <param name="sheetName">Spreadsheet tab name.</param>
    /// <param name="range">Define data range. Use Spreadsheet notation.</param>
    /// <param name="removeHeader">Remove first row. Remember to treat sheet value range accourdingly.</param>
    /// <param name="skipEmptyRows">Collapse column if values of all the cells in one row are empty.</param>
    /// <param name="includeGridData">Grab cell formatting.</param>
    /// <param name="dropIfIncompleteRowThreshold">Remove data if length is lesser than given value.</param>
    /// <param name="returnNullIfEmptyCell">If cell is empty the parameter define how to response such case.</param>
    /// <returns>Cell list in raw Google Spreadsheet format. If no any data then it will return Null.</returns>
    public IEnumerable<KeyValuePair<int, IList<CellData>>> ReadSheetData(string sheetName, string range, bool removeHeader = false,
      bool skipEmptyRows = false, bool includeGridData = true,
      int dropIfIncompleteRowThreshold = -1, bool returnNullIfEmptyCell = false)
    {
      var sheetRange = $"{sheetName}!{range}";
      var sheetRequest = _sheetService.Spreadsheets.Get(_spreadsheetId);
      sheetRequest.Ranges = sheetRange;
      //TODO: find out why .RowData not included if set to false
      sheetRequest.IncludeGridData = includeGridData;

      var sheetResponse = sheetRequest.Execute();

      var rowData = sheetResponse.Sheets[0].Data[0].RowData;

      // Skip execution if cell range is emppty
      if (rowData != null && rowData.Count > 0)
      {
        for (var i = 0; rowData.Count > i; i++)
        {
          if (removeHeader && i == 0)
            continue;

          var rowValues = rowData[i].Values;

          // Define what to do with all empty row
          if (rowValues == null)
          {
            if (skipEmptyRows)
              continue;

            if (!returnNullIfEmptyCell)
            {
              rowValues = new List<CellData> { new CellData { FormattedValue = string.Empty } };
            }
          }
          else
          {
            if (rowValues.Count != dropIfIncompleteRowThreshold &&
                dropIfIncompleteRowThreshold != -1)
              continue;
          }

          // Add result to output
          yield return new KeyValuePair<int, IList<CellData>>(i, rowValues);
        }
      }
    }

    public void WriteData(string sheetName, string range, List<IList<object>> data)
    {
      var sheetRange = $"{sheetName}!{range}";
      var valueRange = new ValueRange();

      valueRange.Values = data;

      var appendRequest = _sheetService.Spreadsheets.Values.Update(valueRange, _spreadsheetId, sheetRange);
      appendRequest.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.USERENTERED;
      var appendResponse = appendRequest.Execute();
    }

    public void DeleteData(string sheetName, string range)
    {
      var sheetRange = $"{sheetName}!{range}";
      var requestBody = new ClearValuesRequest();

      var deleteRequest = _sheetService.Spreadsheets.Values.Clear(requestBody, _spreadsheetId, sheetRange);
      var deleteResponse = deleteRequest.Execute();
    }



    // ------------- TEST ZONE ------------------

    //public List<IEnumerable<object>> ReadSheetValues(string sheetRange)
    //{
    //  var request = sheetService.Spreadsheets.Values.Get(_spreadsheetId, sheetRange);

    //  ValueRange response = request.Execute();

    //  return response.Values.Select(x => x.DefaultIfEmpty("")).ToList();
    //}
  }
}
