using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Server.Assembler.Api.Controllers;
using Server.Assembler.Domain.Entities;
using Server.Assembler.ModelExportService.Services;
using Xunit;

namespace Server.Assembler.Tests
{
  public class RsnFileTests
  {
    public ExportService exportService;
    public RsnController rsnController;
    public RsnFileTests()
    {
      exportService = new ExportService();
      rsnController = new RsnController(exportService);
    }

    [Fact]
    public void WrongRsnFilepathShouldThrowException()
    {
      var fileName = @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Praaaaaaaaaaaaa\0000-Navisctiles\tst\СССР.rvt";

      Assert.Throws<ArgumentException>(() => new RsnFileInfo(fileName).rsnFilePath);
    }

    [Fact]
    public void DiffrentCorrectCasesFilePathDontThrowException()
    {
      var fileNames = new List<string>
      {
        @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Praaaaaaaaaaaaaaaaaaaaaaaaaaa\0000-Navisctiles\tst\СССР.rvt",
        @"\\vpp-revittest01.main.picompany.ru\0000_Тестовые данные\Корпус 1.2\0000-Р-ЖД-1.2.rvt",
        @"\\\\vpp-revittest01.main.picompany.ru\\0000_Тестовые данные\\Корпус 1.2\\0000-Р-ЖД-1.2.rvt"
      };

      foreach (var fileName in fileNames)
      {
        var kek = new RsnFileInfo(fileName).rsnFilePath;
      }
    }

    [Fact]
    public void NotValidFilePaths()
    {
      var files = new List<string>()
      {
        @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Praaaaaaaaaaaaaaaaaaaaaaaaaaa\0000-Navisctiles\tst\СССР.rvt",
        @"\\vpp-revittest01.main.picompany.ru\0000_Тестовые данные\Корпус 1.2\0000-Р-ЖД-1.2",
        @"\\\\vpp-revittdfggain.picompany.ru\\0000_Тестовые данные\\Корпус 1.2\\0000-Р-ЖД-1.2.rvt"
      };

      foreach (var fileName in files)
      {
        var kek = new RsnFileInfo(fileName).rsnFilePath;
        Assert.Throws<Exception>(() => new RsnFileInfo(fileName).rsnFilePath);
      }
    }

    //[Fact]
    //public void ProjectFileDontCount()
    //{
    //  var fileName = @"\\vpp-revittest01.main.picompany.ru\D$\RS17\Prj\00-Navis\tst\СССР.rvt";

    //  var log = rsnController.ExportModelToRvt(fileName).Result as ObjectResult;
    //  Assert.Equal("Путь к файлу проекта не является правильным для RSN", log.Value);
    //}

    [Fact]
    public void ShouldNotFindServerNameException()
    {
      var fileName = @"\\vpp-rebkgui,kiu.main.picompany.ru\D$\RS17\Prj\00-Navis\tst\СССР.rvt";

      var log = rsnController.BatchExportModel(new ExportTask(){Files = new List<string> {fileName}}).Result as ObjectResult;
      Assert.Equal("Имя сервера не определено, либо сервер неизвестен. Файл должен находится на RSN", log.Value);
    }
  }
}