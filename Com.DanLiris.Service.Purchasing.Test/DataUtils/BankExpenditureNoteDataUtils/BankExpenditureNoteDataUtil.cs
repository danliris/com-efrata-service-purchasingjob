﻿using Com.DanLiris.Service.Purchasing.Lib.Facades.BankExpenditureNoteFacades;
using Com.DanLiris.Service.Purchasing.Lib.Models.BankExpenditureNoteModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.Expedition;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Test.DataUtils.ExpeditionDataUtil;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Test.DataUtils.BankExpenditureNoteDataUtils
{
    public class BankExpenditureNoteDataUtil
    {
        private readonly BankExpenditureNoteFacade Facade;
        private readonly PurchasingDocumentAcceptanceDataUtil pdaDataUtil;
        public BankExpenditureNoteDataUtil(BankExpenditureNoteFacade Facade, PurchasingDocumentAcceptanceDataUtil pdaDataUtil)
        {
            this.Facade = Facade;
            this.pdaDataUtil = pdaDataUtil;
        }

        public async Task<BankExpenditureNoteDetailModel> GetNewDetailSpinningData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteItemModel> Items = new List<BankExpenditureNoteItemModel>();
            foreach (var item in purchasingDocumentExpedition.Items)
            {
                BankExpenditureNoteItemModel Item = new BankExpenditureNoteItemModel
                {
                    Price = item.Price,
                    ProductCode = item.ProductCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitCode = item.UnitCode,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    Uom = item.Uom
                };

                Items.Add(Item);
            }

            return new BankExpenditureNoteDetailModel()
            {
                Id = 0,
                UnitPaymentOrderId = purchasingDocumentExpedition.Id,
                UnitPaymentOrderNo = purchasingDocumentExpedition.UnitPaymentOrderNo,
                DivisionCode = purchasingDocumentExpedition.DivisionCode,
                DivisionName = "SPINNING",
                Currency = purchasingDocumentExpedition.Currency,
                DueDate = purchasingDocumentExpedition.DueDate,
                InvoiceNo = purchasingDocumentExpedition.InvoiceNo,
                SupplierCode = purchasingDocumentExpedition.SupplierCode,
                SupplierName = purchasingDocumentExpedition.SupplierName,
                TotalPaid = purchasingDocumentExpedition.TotalPaid,
                UPODate = purchasingDocumentExpedition.UPODate,
                IncomeTax = purchasingDocumentExpedition.IncomeTax,
                Vat = purchasingDocumentExpedition.Vat,
                Items = Items
            };
        }

        public async Task<BankExpenditureNoteDetailModel> GetNewDetailWeavingData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteItemModel> Items = new List<BankExpenditureNoteItemModel>();
            foreach (var item in purchasingDocumentExpedition.Items)
            {
                BankExpenditureNoteItemModel Item = new BankExpenditureNoteItemModel
                {
                    Price = item.Price,
                    ProductCode = item.ProductCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitCode = item.UnitCode,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    Uom = item.Uom
                };

                Items.Add(Item);
            }

            return new BankExpenditureNoteDetailModel()
            {
                Id = 0,
                UnitPaymentOrderId = purchasingDocumentExpedition.Id,
                UnitPaymentOrderNo = purchasingDocumentExpedition.UnitPaymentOrderNo,
                DivisionCode = purchasingDocumentExpedition.DivisionCode,
                DivisionName = "WEAVING",
                Currency = purchasingDocumentExpedition.Currency,
                DueDate = purchasingDocumentExpedition.DueDate,
                InvoiceNo = purchasingDocumentExpedition.InvoiceNo,
                SupplierCode = purchasingDocumentExpedition.SupplierCode,
                SupplierName = purchasingDocumentExpedition.SupplierName,
                TotalPaid = purchasingDocumentExpedition.TotalPaid,
                UPODate = purchasingDocumentExpedition.UPODate,
                Vat = purchasingDocumentExpedition.Vat,
                Items = Items
            };
        }

        public async Task<BankExpenditureNoteDetailModel> GetNewDetailFinishingPrintingData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteItemModel> Items = new List<BankExpenditureNoteItemModel>();
            foreach (var item in purchasingDocumentExpedition.Items)
            {
                BankExpenditureNoteItemModel Item = new BankExpenditureNoteItemModel
                {
                    Price = item.Price,
                    ProductCode = item.ProductCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitCode = item.UnitCode,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    Uom = item.Uom
                };

                Items.Add(Item);
            }

            return new BankExpenditureNoteDetailModel()
            {
                Id = 0,
                UnitPaymentOrderId = purchasingDocumentExpedition.Id,
                UnitPaymentOrderNo = purchasingDocumentExpedition.UnitPaymentOrderNo,
                DivisionCode = purchasingDocumentExpedition.DivisionCode,
                DivisionName = "FINISHING & PRINTING",
                Currency = purchasingDocumentExpedition.Currency,
                DueDate = purchasingDocumentExpedition.DueDate,
                InvoiceNo = purchasingDocumentExpedition.InvoiceNo,
                SupplierCode = purchasingDocumentExpedition.SupplierCode,
                SupplierName = purchasingDocumentExpedition.SupplierName,
                TotalPaid = purchasingDocumentExpedition.TotalPaid,
                UPODate = purchasingDocumentExpedition.UPODate,
                Vat = purchasingDocumentExpedition.Vat,
                Items = Items
            };
        }

        public async Task<BankExpenditureNoteDetailModel> GetNewDetailGarmentData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteItemModel> Items = new List<BankExpenditureNoteItemModel>();
            foreach (var item in purchasingDocumentExpedition.Items)
            {
                BankExpenditureNoteItemModel Item = new BankExpenditureNoteItemModel
                {
                    Price = item.Price,
                    ProductCode = item.ProductCode,
                    ProductId = item.ProductId,
                    ProductName = item.ProductName,
                    Quantity = item.Quantity,
                    UnitCode = item.UnitCode,
                    UnitId = item.UnitId,
                    UnitName = item.UnitName,
                    Uom = item.Uom
                };

                Items.Add(Item);
            }

            return new BankExpenditureNoteDetailModel()
            {
                Id = 0,
                UnitPaymentOrderId = purchasingDocumentExpedition.Id,
                UnitPaymentOrderNo = purchasingDocumentExpedition.UnitPaymentOrderNo,
                DivisionCode = purchasingDocumentExpedition.DivisionCode,
                DivisionName = "GARMENT",
                Currency = purchasingDocumentExpedition.Currency,
                DueDate = purchasingDocumentExpedition.DueDate,
                InvoiceNo = purchasingDocumentExpedition.InvoiceNo,
                SupplierCode = purchasingDocumentExpedition.SupplierCode,
                SupplierName = purchasingDocumentExpedition.SupplierName,
                TotalPaid = purchasingDocumentExpedition.TotalPaid,
                UPODate = purchasingDocumentExpedition.UPODate,
                Vat = purchasingDocumentExpedition.Vat,
                Items = Items
            };
        }

        public async Task<BankExpenditureNoteModel> GetNewData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition1 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());
            PurchasingDocumentExpedition purchasingDocumentExpedition2 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteDetailModel> Details = new List<BankExpenditureNoteDetailModel>()
            {
                await GetNewDetailSpinningData(),
                await GetNewDetailWeavingData(),
                await GetNewDetailFinishingPrintingData(),
                await GetNewDetailGarmentData()
            };

            BankExpenditureNoteModel TestData = new BankExpenditureNoteModel()
            {
                BankAccountNumber = "100020003000",
                BankAccountCOA = "BankAccountCOA",
                BankAccountName = "BankAccountName",
                BankCode = "BankCode",
                BankId = 1,
                BankName = "BankName",
                BankCurrencyCode = "CurrencyCode",
                BankCurrencyId = 1,
                BankCurrencyRate = "1",
                GrandTotal = 120,
                BGCheckNumber = "BGNo",
                SupplierImport = false,
                CurrencyRate = 1,
                CurrencyId = 1,
                CurrencyCode = "Code",
                Details = Details,
            };

            return TestData;
        }

        public async Task<BankExpenditureNoteModel> GetNewDataVatZero()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition1 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());
            PurchasingDocumentExpedition purchasingDocumentExpedition2 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            var detail = await GetNewDetailSpinningData();
            detail.Vat = 0;
            List<BankExpenditureNoteDetailModel> Details = new List<BankExpenditureNoteDetailModel>()
            {
                detail
            };

            BankExpenditureNoteModel TestData = new BankExpenditureNoteModel()
            {
                BankAccountNumber = "100020003000",
                BankAccountCOA = "BankAccountCOA",
                BankAccountName = "BankAccountName",
                BankCode = "BankCode",
                BankId = 1,
                BankName = "BankName",
                BankCurrencyCode = "CurrencyCode",
                BankCurrencyId = 1,
                BankCurrencyRate = "1",
                GrandTotal = 120,
                BGCheckNumber = "BGNo",
                SupplierImport = false,
                CurrencyRate = 1,
                CurrencyId = 1,
                CurrencyCode = "Code",
                Details = Details,
            };

            return TestData;
        }

        public async Task<BankExpenditureNoteModel> GetNewDataIDR()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition1 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());
            PurchasingDocumentExpedition purchasingDocumentExpedition2 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteDetailModel> Details = new List<BankExpenditureNoteDetailModel>()
            {
                await GetNewDetailSpinningData(),
                await GetNewDetailWeavingData(),
                await GetNewDetailFinishingPrintingData(),
                await GetNewDetailGarmentData()
            };

            BankExpenditureNoteModel TestData = new BankExpenditureNoteModel()
            {
                BankAccountNumber = "100020003000",
                BankAccountCOA = "BankAccountCOA",
                BankAccountName = "BankAccountName",
                BankCode = "BankCode",
                BankId = 1,
                BankName = "BankName",
                BankCurrencyCode = "CurrencyCode",
                BankCurrencyId = 1,
                BankCurrencyRate = "1",
                GrandTotal = 120,
                BGCheckNumber = "BGNo",
                SupplierImport = false,
                CurrencyRate = 1,
                CurrencyId = 1,
                CurrencyCode = "IDR",
                Details = Details,
            };

            return TestData;
        }

        public async Task<BankExpenditureNoteModel> GetImportData()
        {
            PurchasingDocumentExpedition purchasingDocumentExpedition1 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());
            PurchasingDocumentExpedition purchasingDocumentExpedition2 = await Task.Run(() => this.pdaDataUtil.GetCashierTestData());

            List<BankExpenditureNoteDetailModel> Details = new List<BankExpenditureNoteDetailModel>()
            {
                await GetNewDetailSpinningData(),
                await GetNewDetailWeavingData(),
                await GetNewDetailFinishingPrintingData(),
                await GetNewDetailGarmentData()
            };

            BankExpenditureNoteModel TestData = new BankExpenditureNoteModel()
            {
                BankAccountNumber = "100020003000",
                BankAccountCOA = "BankAccountCOA",
                BankAccountName = "BankAccountName",
                BankCode = "BankCode",
                BankId = 1,
                BankName = "BankName",
                BankCurrencyCode = "CurrencyCode",
                BankCurrencyId = 1,
                BankCurrencyRate = "1",
                GrandTotal = 120,
                BGCheckNumber = "BGNo",
                SupplierImport = true,
                Details = Details,
            };

            return TestData;
        }

        public async Task<BankExpenditureNoteModel> GetTestData()
        {
            IdentityService identityService = new IdentityService()
            {
                Token = "Token",
                Username = "Unit Test"
            };
            BankExpenditureNoteModel model = await GetNewData();
            await Facade.Create(model, identityService);
            return await Facade.ReadById((int)model.Id);
        }
    }
}
