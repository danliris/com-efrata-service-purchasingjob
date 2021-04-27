﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.Report;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitReceiptNote;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.Report;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.Report
{
    public class ImportPurchasingBookReportTest
    {
        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public async Task Should_Success_GetLocalPurchasingBookReport()
        {
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new LocalPurchasingBookReportViewModel());

            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.Equal((int)HttpStatusCode.OK, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Failed_GetLocalPurchasingBookReport_WithException()
        {
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GetReport(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception());

            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Failed_GetLocalPurchasingBookReportXls()
        {
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ReturnsAsync(new MemoryStream());

            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.NotNull(response);
        }

        [Fact]
        public async Task Should_Failed_GetLocalPurchasingBookReportXls_WithException()
        {
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).ThrowsAsync(new Exception());

            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.Get(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

        [Fact]
        public async Task Should_Success_GetXls()
        {
            //Setup
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).ReturnsAsync(new MemoryStream());

            //Act
            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.GetXls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            //Assert
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.GetType().GetProperty("ContentType").GetValue(response,null));
            Assert.Equal("Laporan Buku Pembelian Impor 01-01-0001_01-01-0001.xlsx", response.GetType().GetProperty("FileDownloadName").GetValue(response, null));
        }

        [Fact]
        public async Task Should_Throws_InternalServerError_GetXls()
        {
            //Setup
            var mockFacade = new Mock<IImportPurchasingBookReportFacade>();
            mockFacade.Setup(facade => facade.GenerateExcel(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>())).Throws(new Exception());

            //Act
            var controller = new ImportPurchasingBookReportController(mockFacade.Object);
            var response = await controller.GetXls(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, GetStatusCode(response));
        }

    }
}
