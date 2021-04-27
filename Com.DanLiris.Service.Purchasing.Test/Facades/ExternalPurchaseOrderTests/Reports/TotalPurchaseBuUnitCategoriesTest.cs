﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade;
using Com.DanLiris.Service.Purchasing.Lib.Facades.ExternalPurchaseOrderFacade.Reports;
using Com.DanLiris.Service.Purchasing.Lib.Models.ExternalPurchaseOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExternalPurchaseOrderDataUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;


namespace Com.DanLiris.Service.Purchasing.Test.Facades.ExternalPurchaseOrderTests.Reports
{
	[Collection("ServiceProviderFixture Collection")]

	public  class TotalPurchaseBuUnitCategoriesTest
    {
		private IServiceProvider ServiceProvider { get; set; }
		public TotalPurchaseBuUnitCategoriesTest(ServiceProviderFixture fixture)
		{
			ServiceProvider = fixture.ServiceProvider;

			IdentityService identityService = (IdentityService)ServiceProvider.GetService(typeof(IdentityService));
			identityService.Username = "Unit Test";
		}
		private ExternalPurchaseOrderDataUtil DataUtil
		{
			get { return (ExternalPurchaseOrderDataUtil)ServiceProvider.GetService(typeof(ExternalPurchaseOrderDataUtil)); }
		}

		private TotalPurchaseFacade Facade
		{
			get { return (TotalPurchaseFacade)ServiceProvider.GetService(typeof(TotalPurchaseFacade)); }
		}
		private ExternalPurchaseOrderFacade FacadeEPO
		{
			get { return (ExternalPurchaseOrderFacade)ServiceProvider.GetService(typeof(ExternalPurchaseOrderFacade)); }
		}
		[Fact]
		public async Task Should_Success_Get_Report_Total_Purchase_By_Categories_Data_Null_Parameter()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await DataUtil.GetNewData("unit-test");
			await FacadeEPO.Create(externalPurchaseOrder, "unit-test", 7);
			var Response = Facade.GetTotalPurchaseByUnitCategoriesReport(null, null, null, null, null, 7);
			Assert.NotEqual(1, 0);
		}
		[Fact]
		public async Task Should_Success_Get_Report_Total_Purchase_By_Unit_Categories_Data_Excel_Null_Parameter()
		{
			ExternalPurchaseOrder externalPurchaseOrder = await DataUtil.GetNewData("unit-test");
			await FacadeEPO.Create(externalPurchaseOrder, "unit-test", 7);
			var Response = Facade.GenerateExcelTotalPurchaseByUnitCategories(null,null,null,null, null, 7);
			Assert.IsType<System.IO.MemoryStream>(Response);
		}
		[Fact]
		public void Should_Success_Get_Report_Total_Purchase_By_Unit_Categories_Null_Data_Excel()
		{
			DateTime DateFrom = new DateTime(2018, 1, 1);
			DateTime DateTo = new DateTime(2018, 1, 1);
			var Response = Facade.GenerateExcelTotalPurchaseByUnitCategories(null, null, null, DateFrom, DateTo, 7);
			Assert.IsType<System.IO.MemoryStream>(Response);
		}
	}
}
