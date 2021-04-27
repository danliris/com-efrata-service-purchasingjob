﻿using AutoMapper;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.UnitPaymentOrderViewModel;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.UnitPaymentOrderDataUtils;
using Com.DanLiris.Service.Purchasing.Test.Helpers;
using Com.DanLiris.Service.Purchasing.WebApi.Controllers.v1.UnitPaymentCorrectionNoteControllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Com.DanLiris.Service.Purchasing.Test.Controllers.UnitPaymentCorrectionNoteControllerTests
{
    public class UnitPaymentCorrectionNoteGenerateDataControllerTest
    {
        private Mock<IServiceProvider> GetServiceProvider()
        {
            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider
                .Setup(x => x.GetService(typeof(IdentityService)))
                .Returns(new IdentityService() { Token = "Token", Username = "Test" });

            serviceProvider
                .Setup(x => x.GetService(typeof(IHttpClientService)))
                .Returns(new HttpClientTestService());

            return serviceProvider;
        }

        private UnitPaymentCorrectionNoteGenerateDataController GetController(Mock<IUnitPaymentPriceCorrectionNoteFacade> facadeM, Mock<IValidateService> validateM, Mock<IMapper> mapper)
        {
            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);

            var servicePMock = GetServiceProvider();
            if (validateM != null)
            {
                servicePMock
                    .Setup(x => x.GetService(typeof(IValidateService)))
                    .Returns(validateM.Object);
            }

            var identityService = new IdentityService
            {
                Username = "Unit Test"
            };

            UnitPaymentCorrectionNoteGenerateDataController controller = new UnitPaymentCorrectionNoteGenerateDataController(mapper.Object, facadeM.Object, identityService)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext()
                    {
                        User = user.Object
                    }
                }
            };
            controller.ControllerContext.HttpContext.Request.Headers["Authorization"] = "Bearer unittesttoken";
            controller.ControllerContext.HttpContext.Request.Path = new PathString("/v1/unit-test");
            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "7";

            return controller;
        }

        protected int GetStatusCode(IActionResult response)
        {
            return (int)response.GetType().GetProperty("StatusCode").GetValue(response, null);
        }

        [Fact]
        public void Should_Success_Get_Xls()
        {
            var mockFacade = new Mock<IUnitPaymentPriceCorrectionNoteFacade>();
            mockFacade.Setup(x => x.GenerateDataExcel(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Returns(new MemoryStream());

            var mockMapper = new Mock<IMapper>();

            var user = new Mock<ClaimsPrincipal>();
            var claims = new Claim[]
            {
                new Claim("username", "unittestusername")
            };
            user.Setup(u => u.Claims).Returns(claims);
            UnitPaymentCorrectionNoteGenerateDataController controller = GetController(mockFacade, null, mockMapper);
            controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext()
                {
                    User = user.Object
                }
            };

            controller.ControllerContext.HttpContext.Request.Headers["x-timezone-offset"] = "0";
            var response = controller.GetXls(It.IsAny<DateTime>(), It.IsAny<DateTime>());
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", response.GetType().GetProperty("ContentType").GetValue(response, null));

        }

        [Fact]
        public void Should_Error_Get_Xls()
        {
            var mockFacade = new Mock<IUnitPaymentPriceCorrectionNoteFacade>();
            mockFacade.Setup(x => x.GenerateDataExcel(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int>()))
                .Throws(new Exception());
            var mockMapper = new Mock<IMapper>();

            UnitPaymentCorrectionNoteGenerateDataController controller = GetController(mockFacade, null, mockMapper);
            var response = controller.GetXls(It.IsAny<DateTime>(), It.IsAny<DateTime>());
            Assert.Equal((int)HttpStatusCode.InternalServerError, (int)response.GetType().GetProperty("StatusCode").GetValue(response, null));
        }
    }
}
