﻿using Com.DanLiris.Service.Purchasing.Lib.Enums;
using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Services;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.CacheManager.CacheData;
using Com.DanLiris.Service.Purchasing.Lib.Utilities.Currencies;
using Com.Moonlay.Models;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Remotion.Linq.Clauses.ResultOperators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.VBRequestPOExternal
{
    public class VBRequestPOExternalService : IVBRequestPOExternalService
    {
        private readonly PurchasingDbContext _dbContext;
        private readonly IdentityService _identityService;
        private readonly IMemoryCacheManager _cacheManager;
        private readonly ICurrencyProvider _currencyProvider;
        private readonly IServiceProvider _serviceProvider;
        private const string UserAgent = "service-purchasing";

        public VBRequestPOExternalService(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
        {
            _dbContext = dbContext;
            _identityService = serviceProvider.GetService<IdentityService>();
            _cacheManager = serviceProvider.GetService<IMemoryCacheManager>();
            _currencyProvider = serviceProvider.GetService<ICurrencyProvider>();

            _serviceProvider = serviceProvider;
        }

        private List<CategoryCOAResult> Categories => _cacheManager.Get(MemoryCacheConstant.Categories, entry => { return new List<CategoryCOAResult>(); });
        private List<IdCOAResult> Units => _cacheManager.Get(MemoryCacheConstant.Units, entry => { return new List<IdCOAResult>(); });
        private List<IdCOAResult> Divisions => _cacheManager.Get(MemoryCacheConstant.Divisions, entry => { return new List<IdCOAResult>(); });
        private List<IncomeTaxCOAResult> IncomeTaxes => _cacheManager.Get(MemoryCacheConstant.IncomeTaxes, entry => { return new List<IncomeTaxCOAResult>(); });

        public List<POExternalDto> ReadPOExternal(string keyword, string division, string currencyCode)
        {
            var result = new List<POExternalDto>();

            if (!string.IsNullOrWhiteSpace(division) && division.ToUpper() == "GARMENT")
            {
                var garmentQuery = _dbContext.GarmentExternalPurchaseOrders.Where(entity => entity.PaymentType == "CASH" && entity.IsPosted).Include(entity => entity.Items).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    garmentQuery = garmentQuery.Where(entity => entity.EPONo.Contains(keyword));
                }

                if (!string.IsNullOrWhiteSpace(currencyCode))
                {
                    garmentQuery = garmentQuery.Where(entity => entity.CurrencyCode == currencyCode);
                }

                var garmentQueryResult = garmentQuery.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                var epoIdAndPOIds = garmentQueryResult.SelectMany(element => element.Items).Select(element => new EPOIdAndPOId() { EPOId = element.GarmentEPOId, POId = element.POId }).ToList();
                var poIds = epoIdAndPOIds.Select(element => element.POId).ToList();
                var purchaseOrders = _dbContext.GarmentInternalPurchaseOrders.Where(entity => poIds.Contains(entity.Id)).ToList();
                //var internalPOs = _dbContext

                result = garmentQueryResult.Select(entity => new POExternalDto(entity, purchaseOrders)).ToList();
            }

            var query = _dbContext.ExternalPurchaseOrders.Where(entity => entity.POCashType == "VB" && entity.IsPosted).Include(entity => entity.Items).ThenInclude(entity => entity.Details).AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(entity => entity.EPONo.Contains(keyword));
            }

            if (!string.IsNullOrWhiteSpace(currencyCode))
            {
                query = query.Where(entity => entity.CurrencyCode == currencyCode);
            }

            var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

            //result = queryResult.Select(entity => new POExternalDto(entity)).ToList();
            result.AddRange(queryResult.Select(entity => new POExternalDto(entity)).ToList());

            return result;
        }

        public List<SPBDto> ReadSPB(string keyword, string division, List<long> epoIds, string currencyCode, string typePurchasing)
        {
            var result = new List<SPBDto>();

            if (!string.IsNullOrEmpty(division) && division.ToUpper() == "GARMENT")
            {
                if (epoIds.Count == 0)
                {
                    List<long> internNoteIds = (from garmentEPO in _dbContext.GarmentExternalPurchaseOrders.AsNoTracking()
                                                join garmentINDetail in _dbContext.GarmentInternNoteDetails.AsNoTracking()
                                                on garmentEPO.Id equals garmentINDetail.EPOId
                                                join garmentINItem in _dbContext.GarmentInternNoteItems.AsNoTracking()
                                                on garmentINDetail.GarmentItemINId equals garmentINItem.Id
                                                where garmentEPO.PaymentType == "CASH" && garmentEPO.IsPosted
                                                select garmentINItem.GarmentINId).ToList();


                    var garmentQuery = _dbContext.GarmentInternNotes.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => internNoteIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();
                    if (!string.IsNullOrWhiteSpace(keyword))
                        garmentQuery = garmentQuery.Where(entity => entity.INNo.Contains(keyword));

                    if (!string.IsNullOrWhiteSpace(currencyCode))
                        garmentQuery = garmentQuery.Where(entity => entity.CurrencyCode == currencyCode);

                    var garmentQueryResult = garmentQuery.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                    var invoiceIds = garmentQueryResult.SelectMany(element => element.Items).Select(element => element.InvoiceId).ToList();
                    var invoices = _dbContext.GarmentInvoices.Where(entity => invoiceIds.Contains(entity.Id)).ToList();

                    internNoteIds = garmentQueryResult.Select(element => element.Id).ToList();
                    var internNoteItems = _dbContext.GarmentInternNoteItems.AsNoTracking().Where(entity => internNoteIds.Contains(entity.GarmentINId)).ToList();
                    var internNoteItemIds = internNoteItems.Select(element => element.Id).ToList();
                    var internNoteDetails = _dbContext.GarmentInternNoteDetails.AsNoTracking().Where(entity => internNoteItemIds.Contains(entity.GarmentItemINId)).ToList();

                    result = garmentQueryResult.Select(element => new SPBDto(element, invoices, internNoteItems, internNoteDetails)).ToList();

                    epoIds = _dbContext.ExternalPurchaseOrders.AsNoTracking().Where(entity => entity.PaymentMethod == "CASH" && entity.POCashType == "VB" && entity.IsPosted).Select(entity => entity.Id).ToList();

                    var epoItemIds = _dbContext.ExternalPurchaseOrderItems.AsNoTracking().Where(entity => epoIds.Contains(entity.EPOId)).Select(entity => entity.Id).ToList();
                    var epoDetailIds = _dbContext.ExternalPurchaseOrderDetails.AsNoTracking().Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => entity.Id).ToList();
                    var spbItemIds = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => epoDetailIds.Contains(entity.EPODetailId)).Select(entity => entity.UPOItemId).ToList();
                    var spbIds = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbItemIds.Contains(entity.Id)).Select(entity => entity.UPOId).ToList();

                    var query = _dbContext.UnitPaymentOrders.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => spbIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();

                    if (!string.IsNullOrWhiteSpace(keyword))
                        query = query.Where(entity => entity.UPONo.Contains(keyword));

                    if (!string.IsNullOrWhiteSpace(currencyCode))
                        query = query.Where(entity => entity.CurrencyCode == currencyCode);

                    var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                    spbIds = queryResult.Select(element => element.Id).ToList();
                    var spbItems = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbIds.Contains(entity.UPOId)).ToList();
                    spbItemIds = spbItems.Select(element => element.Id).ToList();
                    var spbDetails = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => spbItemIds.Contains(entity.UPOItemId)).ToList();
                    var unitReceiptNoteItemIds = spbDetails.Select(element => element.URNItemId).ToList();
                    var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.AsNoTracking().Where(entity => unitReceiptNoteItemIds.Contains(entity.Id)).ToList();
                    var unitReceiptNoteIds = unitReceiptNoteItems.Select(entity => entity.URNId).ToList();
                    var unitReceiptNotes = _dbContext.UnitReceiptNotes.AsNoTracking().Where(entity => unitReceiptNoteIds.Contains(entity.Id)).ToList();

                    //result = queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList();
                    result.AddRange(queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList());
                }
                else
                {
                    if (!string.IsNullOrEmpty(typePurchasing) && typePurchasing.ToUpper() == "UMUM")
                    {
                        var epoItemIds = _dbContext.ExternalPurchaseOrderItems.AsNoTracking().Where(entity => epoIds.Contains(entity.EPOId)).Select(entity => entity.Id).ToList();
                        var epoDetailIds = _dbContext.ExternalPurchaseOrderDetails.AsNoTracking().Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => entity.Id).ToList();
                        var spbItemIds = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => epoDetailIds.Contains(entity.EPODetailId)).Select(entity => entity.UPOItemId).ToList();
                        var spbIds = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbItemIds.Contains(entity.Id)).Select(entity => entity.UPOId).ToList();

                        var query = _dbContext.UnitPaymentOrders.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => spbIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();

                        if (!string.IsNullOrWhiteSpace(keyword))
                            query = query.Where(entity => entity.UPONo.Contains(keyword));

                        if (!string.IsNullOrWhiteSpace(currencyCode))
                            query = query.Where(entity => entity.CurrencyCode == currencyCode);

                        var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                        spbIds = queryResult.Select(element => element.Id).ToList();
                        var spbItems = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbIds.Contains(entity.UPOId)).ToList();
                        spbItemIds = spbItems.Select(element => element.Id).ToList();
                        var spbDetails = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => spbItemIds.Contains(entity.UPOItemId)).ToList();
                        var unitReceiptNoteItemIds = spbDetails.Select(element => element.URNItemId).ToList();
                        var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.AsNoTracking().Where(entity => unitReceiptNoteItemIds.Contains(entity.Id)).ToList();
                        var unitReceiptNoteIds = unitReceiptNoteItems.Select(entity => entity.URNId).ToList();
                        var unitReceiptNotes = _dbContext.UnitReceiptNotes.AsNoTracking().Where(entity => unitReceiptNoteIds.Contains(entity.Id)).ToList();

                        //result = queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList();
                        result.AddRange(queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList());
                    }
                    else
                    {
                        List<long> internNoteIds = (from garmentINDetail in _dbContext.GarmentInternNoteDetails.AsNoTracking()
                                                    join garmentINItem in _dbContext.GarmentInternNoteItems.AsNoTracking()
                                                    on garmentINDetail.GarmentItemINId equals garmentINItem.Id
                                                    where epoIds.Contains(garmentINDetail.EPOId)
                                                    select garmentINItem.GarmentINId).ToList();


                        var garmentQuery = _dbContext.GarmentInternNotes.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => internNoteIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();
                        if (!string.IsNullOrWhiteSpace(keyword))
                            garmentQuery = garmentQuery.Where(entity => entity.INNo.Contains(keyword));

                        if (!string.IsNullOrWhiteSpace(currencyCode))
                            garmentQuery = garmentQuery.Where(entity => entity.CurrencyCode == currencyCode);

                        var garmentQueryResult = garmentQuery.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                        var invoiceIds = garmentQueryResult.SelectMany(element => element.Items).Select(element => element.InvoiceId).ToList();
                        var invoices = _dbContext.GarmentInvoices.Where(entity => invoiceIds.Contains(entity.Id)).ToList();

                        internNoteIds = garmentQueryResult.Select(element => element.Id).ToList();
                        var internNoteItems = _dbContext.GarmentInternNoteItems.AsNoTracking().Where(entity => internNoteIds.Contains(entity.GarmentINId)).ToList();
                        var internNoteItemIds = internNoteItems.Select(element => element.Id).ToList();
                        var internNoteDetails = _dbContext.GarmentInternNoteDetails.AsNoTracking().Where(entity => internNoteItemIds.Contains(entity.GarmentItemINId)).ToList();

                        result = garmentQueryResult.Select(element => new SPBDto(element, invoices, internNoteItems, internNoteDetails)).ToList();
                    }
                }
            }
            else
            {
                if (epoIds.Count <= 0)
                {
                    epoIds = _dbContext.ExternalPurchaseOrders.AsNoTracking().Where(entity => entity.PaymentMethod == "CASH" && entity.POCashType == "VB" && entity.IsPosted).Select(entity => entity.Id).ToList();
                }

                var epoItemIds = _dbContext.ExternalPurchaseOrderItems.AsNoTracking().Where(entity => epoIds.Contains(entity.EPOId)).Select(entity => entity.Id).ToList();
                var epoDetailIds = _dbContext.ExternalPurchaseOrderDetails.AsNoTracking().Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => entity.Id).ToList();
                var spbItemIds = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => epoDetailIds.Contains(entity.EPODetailId)).Select(entity => entity.UPOItemId).ToList();
                var spbIds = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbItemIds.Contains(entity.Id)).Select(entity => entity.UPOId).ToList();

                var query = _dbContext.UnitPaymentOrders.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => spbIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                    query = query.Where(entity => entity.UPONo.Contains(keyword));

                if (!string.IsNullOrWhiteSpace(currencyCode))
                    query = query.Where(entity => entity.CurrencyCode == currencyCode);

                var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

                spbIds = queryResult.Select(element => element.Id).ToList();
                var spbItems = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbIds.Contains(entity.UPOId)).ToList();
                spbItemIds = spbItems.Select(element => element.Id).ToList();
                var spbDetails = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => spbItemIds.Contains(entity.UPOItemId)).ToList();
                var unitReceiptNoteItemIds = spbDetails.Select(element => element.URNItemId).ToList();
                var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.AsNoTracking().Where(entity => unitReceiptNoteItemIds.Contains(entity.Id)).ToList();
                var unitReceiptNoteIds = unitReceiptNoteItems.Select(entity => entity.URNId).ToList();
                var unitReceiptNotes = _dbContext.UnitReceiptNotes.AsNoTracking().Where(entity => unitReceiptNoteIds.Contains(entity.Id)).ToList();

                //result = queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList();
                result.AddRange(queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList());
            }

            //if (!string.IsNullOrWhiteSpace(division) && division.ToUpper() == "GARMENT")
            //{
            //    //if (epoIds.Count <= 0)
            //    //{
            //    //    epoIds = _dbContext.GarmentExternalPurchaseOrders.AsNoTracking().Where(entity => entity.PaymentType == "CASH" && entity.IsPosted).Select(entity => entity.Id).ToList();
            //    //}

            //    //var internNoteItemIds = _dbContext.GarmentInternNoteDetails.AsNoTracking().Where(entity => epoIds.Contains(entity.EPOId)).Select(entity => entity.GarmentItemINId).ToList();
            //    //var internNoteItems = _dbContext.GarmentInternNoteItems.AsNoTracking().Where(entity => internNoteItemIds.Contains(entity.Id)).ToList();
            //    //var internNoteIds = internNoteItems.Select(entity => entity.GarmentINId).ToList();

            //    List<long> internNoteIds = new List<long>();

            //    if (epoIds.Count == 0)
            //    {
            //        internNoteIds = (from garmentEPO in _dbContext.GarmentExternalPurchaseOrders.AsNoTracking()
            //                         join garmentINDetail in _dbContext.GarmentInternNoteDetails.AsNoTracking()
            //                         on garmentEPO.Id equals garmentINDetail.EPOId
            //                         join garmentINItem in _dbContext.GarmentInternNoteItems.AsNoTracking()
            //                         on garmentINDetail.GarmentItemINId equals garmentINItem.Id
            //                         where garmentEPO.PaymentType == "CASH" && garmentEPO.IsPosted
            //                         select garmentINItem.GarmentINId).ToList();

            //    }
            //    else
            //    {
            //        internNoteIds = (from garmentINDetail in _dbContext.GarmentInternNoteDetails.AsNoTracking()
            //                         join garmentINItem in _dbContext.GarmentInternNoteItems.AsNoTracking()
            //                         on garmentINDetail.GarmentItemINId equals garmentINItem.Id
            //                         where epoIds.Contains(garmentINDetail.EPOId)
            //                         select garmentINItem.GarmentINId).ToList();
            //    }


            //    var garmentQuery = _dbContext.GarmentInternNotes.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => internNoteIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();
            //    if (!string.IsNullOrWhiteSpace(keyword))
            //        garmentQuery = garmentQuery.Where(entity => entity.INNo.Contains(keyword));

            //    if (!string.IsNullOrWhiteSpace(currencyCode))
            //        garmentQuery = garmentQuery.Where(entity => entity.CurrencyCode == currencyCode);

            //    var garmentQueryResult = garmentQuery.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

            //    var invoiceIds = garmentQueryResult.SelectMany(element => element.Items).Select(element => element.InvoiceId).ToList();
            //    var invoices = _dbContext.GarmentInvoices.Where(entity => invoiceIds.Contains(entity.Id)).ToList();

            //    internNoteIds = garmentQueryResult.Select(element => element.Id).ToList();
            //    var internNoteItems = _dbContext.GarmentInternNoteItems.AsNoTracking().Where(entity => internNoteIds.Contains(entity.GarmentINId)).ToList();
            //    var internNoteItemIds = internNoteItems.Select(element => element.Id).ToList();
            //    var internNoteDetails = _dbContext.GarmentInternNoteDetails.AsNoTracking().Where(entity => internNoteItemIds.Contains(entity.GarmentItemINId)).ToList();

            //    result = garmentQueryResult.Select(element => new SPBDto(element, invoices, internNoteItems, internNoteDetails)).ToList();

            //}

            //if (epoIds.Count <= 0)
            //{
            //    epoIds = _dbContext.ExternalPurchaseOrders.AsNoTracking().Where(entity => entity.PaymentMethod == "CASH" && entity.POCashType == "VB" && entity.IsPosted).Select(entity => entity.Id).ToList();
            //}

            //var epoItemIds = _dbContext.ExternalPurchaseOrderItems.AsNoTracking().Where(entity => epoIds.Contains(entity.EPOId)).Select(entity => entity.Id).ToList();
            //var epoDetailIds = _dbContext.ExternalPurchaseOrderDetails.AsNoTracking().Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => entity.Id).ToList();
            //var spbItemIds = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => epoDetailIds.Contains(entity.EPODetailId)).Select(entity => entity.UPOItemId).ToList();
            //var spbIds = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbItemIds.Contains(entity.Id)).Select(entity => entity.UPOId).ToList();

            //var query = _dbContext.UnitPaymentOrders.AsNoTracking().Include(entity => entity.Items).ThenInclude(entity => entity.Details).Where(entity => spbIds.Contains(entity.Id) && !entity.IsCreatedVB).AsQueryable();

            //if (!string.IsNullOrWhiteSpace(keyword))
            //    query = query.Where(entity => entity.UPONo.Contains(keyword));

            //if (!string.IsNullOrWhiteSpace(currencyCode))
            //    query = query.Where(entity => entity.CurrencyCode == currencyCode);

            //var queryResult = query.OrderByDescending(entity => entity.LastModifiedUtc).Take(10).ToList();

            //spbIds = queryResult.Select(element => element.Id).ToList();
            //var spbItems = _dbContext.UnitPaymentOrderItems.AsNoTracking().Where(entity => spbIds.Contains(entity.UPOId)).ToList();
            //spbItemIds = spbItems.Select(element => element.Id).ToList();
            //var spbDetails = _dbContext.UnitPaymentOrderDetails.AsNoTracking().Where(entity => spbItemIds.Contains(entity.UPOItemId)).ToList();
            //var unitReceiptNoteItemIds = spbDetails.Select(element => element.URNItemId).ToList();
            //var unitReceiptNoteItems = _dbContext.UnitReceiptNoteItems.AsNoTracking().Where(entity => unitReceiptNoteItemIds.Contains(entity.Id)).ToList();
            //var unitReceiptNoteIds = unitReceiptNoteItems.Select(entity => entity.URNId).ToList();
            //var unitReceiptNotes = _dbContext.UnitReceiptNotes.AsNoTracking().Where(entity => unitReceiptNoteIds.Contains(entity.Id)).ToList();

            ////result = queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList();
            //result.AddRange(queryResult.Select(element => new SPBDto(element, spbDetails, spbItems, unitReceiptNoteItems, unitReceiptNotes)).ToList());

            return result;
        }

        public int UpdateSPB(string division, int spbId)
        {
            if (division == "GARMENT")
            {
                var model = _dbContext.GarmentInternNotes.FirstOrDefault(entity => entity.Id == spbId);
                model.IsCreatedVB = !model.IsCreatedVB;

                EntityExtension.FlagForUpdate(model, _identityService.Username, UserAgent);
                _dbContext.GarmentInternNotes.Update(model);
            }
            else
            {
                var model = _dbContext.UnitPaymentOrders.FirstOrDefault(entity => entity.Id == spbId);
                model.IsCreatedVB = !model.IsCreatedVB;

                EntityExtension.FlagForUpdate(model, _identityService.Username, UserAgent);
                _dbContext.UnitPaymentOrders.Update(model);
            }

            return _dbContext.SaveChanges();
        }

        public async Task<int> AutoJournalVBRequest(VBFormDto form)
        {
            var unitPaymentOrderItemIds = _dbContext.UnitPaymentOrderItems.Where(entity => form.EPOIds.Contains(entity.UPOId)).Select(entity => entity.Id).ToList();
            var unitPaymentOrders = _dbContext.UnitPaymentOrders.Where(entity => form.EPOIds.Contains(entity.Id)).ToList();
            var epoDetailIds = _dbContext.UnitPaymentOrderDetails.Where(entity => unitPaymentOrderItemIds.Contains(entity.UPOItemId)).Select(entity => entity.EPODetailId).ToList();

            var externalPurchaseOrderItems = _dbContext.ExternalPurchaseOrderItems.Where(entity => form.EPOIds.Contains(entity.EPOId)).Select(entity => new { entity.Id, entity.EPOId, entity.PRId, entity.UnitId }).ToList();
            var epoItemIds = externalPurchaseOrderItems.Select(element => element.Id).ToList();
            var epoIds = externalPurchaseOrderItems.Select(element => element.EPOId).ToList();
            var externalPurchaseOrders = _dbContext.ExternalPurchaseOrders.Where(entity => epoIds.Contains(entity.Id)).Select(entity => new { entity.Id, entity.IncomeTaxId, entity.UseIncomeTax, entity.IncomeTaxName, entity.IncomeTaxRate, entity.CurrencyCode, entity.IncomeTaxBy, entity.SupplierIsImport }).ToList();
            var externalPurchaseOrderDetails = _dbContext.ExternalPurchaseOrderDetails.Where(entity => epoItemIds.Contains(entity.EPOItemId)).Select(entity => new { entity.Id, entity.EPOItemId, entity.DealQuantity, entity.PricePerDealUnit, entity.IncludePpn }).ToList();

            var purchaseRequestIds = externalPurchaseOrderItems.Select(element => element.PRId).ToList();
            var purchaseRequests = _dbContext.PurchaseRequests.Where(w => purchaseRequestIds.Contains(w.Id)).Select(s => new { s.Id, s.CategoryCode, s.CategoryId, s.UnitId, s.DivisionId }).ToList();

            var journalTransactionToPost = new JournalTransaction()
            {
                Date = form.Date,
                Description = "Auto Journal Clearance VB",
                ReferenceNo = form.DocumentNo,
                Status = "POSTED",
                Items = new List<JournalTransactionItem>()
            };

            var journalDebitItems = new List<JournalTransactionItem>();
            var journalCreditItems = new List<JournalTransactionItem>();

            foreach (var unitPaymentOrder in unitPaymentOrders)
            {
                //var externalPurchaseOrderItem = externalPurchaseOrderItems.FirstOrDefault(element => element.Id == externalPurchaseOrderDetail.EPOItemId);
                //var externalPurchaseOrder = externalPurchaseOrders.FirstOrDefault(element => element.Id == externalPurchaseOrderItem.EPOId);
                //var purchaseRequest = purchaseRequests.FirstOrDefault(element => element.Id == externalPurchaseOrderItem.PRId);
                var upo = form.UPOIds.FirstOrDefault(element => element.UPOId.GetValueOrDefault() == unitPaymentOrder.Id);

                int.TryParse(unitPaymentOrder.CategoryId, out var categoryId);
                var category = Categories.FirstOrDefault(f => f._id.Equals(categoryId));
                if (category == null)
                {
                    category = new CategoryCOAResult()
                    {
                        ImportDebtCOA = "9999.00",
                        LocalDebtCOA = "9999.00",
                        PurchasingCOA = "9999.00",
                        StockCOA = "9999.00"
                    };
                }
                else
                {
                    if (string.IsNullOrEmpty(category.ImportDebtCOA))
                    {
                        category.ImportDebtCOA = "9999.00";
                    }
                    if (string.IsNullOrEmpty(category.LocalDebtCOA))
                    {
                        category.LocalDebtCOA = "9999.00";
                    }
                    if (string.IsNullOrEmpty(category.PurchasingCOA))
                    {
                        category.PurchasingCOA = "9999.00";
                    }
                    if (string.IsNullOrEmpty(category.StockCOA))
                    {
                        category.StockCOA = "9999.00";
                    }
                }

                int.TryParse(unitPaymentOrder.DivisionId, out var divisionId);
                var division = Divisions.FirstOrDefault(f => f.Id.Equals(divisionId));
                if (division == null)
                {
                    division = new IdCOAResult()
                    {
                        COACode = "0"
                    };
                }
                else
                {
                    if (string.IsNullOrEmpty(division.COACode))
                    {
                        division.COACode = "0";
                    }
                }

                int.TryParse("0", out var unitId);
                var unit = Units.FirstOrDefault(f => f.Id.Equals(unitId));
                if (unit == null)
                {
                    unit = new IdCOAResult()
                    {
                        COACode = "00"
                    };
                }
                else
                {
                    if (string.IsNullOrEmpty(unit.COACode))
                    {
                        unit.COACode = "0";
                    }
                }

                int.TryParse(unitPaymentOrder.IncomeTaxId, out var incomeTaxId);
                var incomeTax = IncomeTaxes.FirstOrDefault(f => f.Id.Equals(incomeTaxId));

                if (incomeTax == null || string.IsNullOrWhiteSpace(incomeTax.COACodeCredit))
                {
                    incomeTax = new IncomeTaxCOAResult()
                    {
                        COACodeCredit = "9999.00"
                    };
                }

                //double.TryParse(unitPaymentOrder.IncomeTaxRate, out var incomeTaxRate);

                var currency = await _currencyProvider.GetCurrencyByCurrencyCode(unitPaymentOrder.CurrencyCode);
                var currencyRate = currency != null ? currency.Rate.GetValueOrDefault() : 1;

                //var basePrice = externalPurchaseOrderDetail.PricePerDealUnit * externalPurchaseOrderDetail.DealQuantity * currencyRate;
                //var totalPrice = basePrice;

                //if (!externalPurchaseOrderDetail.IncludePpn)
                //    totalPrice += basePrice * 0.1;

                //if (externalPurchaseOrder.UseIncomeTax && externalPurchaseOrder.IncomeTaxBy.ToUpper() == "SUPPLIER")
                //    totalPrice -= basePrice * (incomeTaxRate / 100);


                journalDebitItems.Add(new JournalTransactionItem()
                {
                    COA = new COA()
                    {
                        //Code = unitPaymentOrder.SupplierIsImport ? $"{category.ImportDebtCOA}.{division.COACode}.{unit.COACode}" : $"{category.LocalDebtCOA}.{division.COACode}.{unit.COACode}"
                        Code = $"{category.ImportDebtCOA}.{division.COACode}.{unit.COACode}"
                    },
                    Debit = (decimal)upo.Amount
                });



                journalCreditItems.Add(new JournalTransactionItem()
                {
                    COA = new COA()
                    {
                        Code = currency.Code.ToUpper() == "IDR" ? $"1011.00.{division.COACode}.{unit.COACode}" : $"1012.00.{division.COACode}.{unit.COACode}"
                    },
                    Credit = (decimal)upo.Amount
                });

            }

            journalDebitItems = journalDebitItems.GroupBy(grouping => grouping.COA.Code).Select(s => new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = s.Key
                },
                Debit = s.Sum(sum => Math.Round(sum.Debit.GetValueOrDefault(), 4)),
                Credit = 0
            }).ToList();
            journalTransactionToPost.Items.AddRange(journalDebitItems);

            journalCreditItems = journalCreditItems.GroupBy(grouping => grouping.COA.Code).Select(s => new JournalTransactionItem()
            {
                COA = new COA()
                {
                    Code = s.Key
                },
                Debit = 0,
                Credit = s.Sum(sum => Math.Round(sum.Credit.GetValueOrDefault(), 4))
            }).ToList();
            journalTransactionToPost.Items.AddRange(journalCreditItems);

            if (journalTransactionToPost.Items.Any(item => item.COA.Code.Split(".").FirstOrDefault().Equals("9999")))
                journalTransactionToPost.Status = "DRAFT";

            var journalTransactionUri = "journal-transactions";
            var httpClient = _serviceProvider.GetService<IHttpClientService>();
            var response = await httpClient.PostAsync($"{APIEndpoint.Finance}{journalTransactionUri}", new StringContent(JsonConvert.SerializeObject(journalTransactionToPost).ToString(), Encoding.UTF8, General.JsonMediaType));

            return (int)response.StatusCode;
        }
    }

    public class EPOIdAndPOId
    {
        public long EPOId { get; set; }
        public long POId { get; set; }
    }
}
