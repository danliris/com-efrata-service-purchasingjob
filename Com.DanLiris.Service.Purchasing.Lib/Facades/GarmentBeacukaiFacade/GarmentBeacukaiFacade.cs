﻿using Com.DanLiris.Service.Purchasing.Lib.Helpers;
using Com.DanLiris.Service.Purchasing.Lib.Interfaces;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentBeacukaiModel;
using Com.DanLiris.Service.Purchasing.Lib.Models.GarmentDeliveryOrderModel;
using Com.DanLiris.Service.Purchasing.Lib.ViewModels.GarmentBeacukaiViewModel;
using Com.Moonlay.Models;
using Com.Moonlay.NetCore.Lib;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Com.DanLiris.Service.Purchasing.Lib.Facades.GarmentBeacukaiFacade
{
	public class GarmentBeacukaiFacade : IGarmentBeacukaiFacade
	{
		private readonly PurchasingDbContext dbContext;
		private readonly DbSet<GarmentBeacukai> dbSet;
		public readonly IServiceProvider serviceProvider;
		private readonly DbSet<GarmentDeliveryOrder> dbSetDeliveryOrder;
		private string USER_AGENT = "Facade";
		public GarmentBeacukaiFacade(PurchasingDbContext dbContext, IServiceProvider serviceProvider)
		{
			this.dbContext = dbContext;
			this.dbSet = dbContext.Set<GarmentBeacukai>();
			this.dbSetDeliveryOrder = dbContext.Set<GarmentDeliveryOrder>();
			this.serviceProvider = serviceProvider;
		}

		public Tuple<List<GarmentBeacukai>, int, Dictionary<string, string>> Read(int Page = 1, int Size = 25, string Order = "{}", string Keyword = null, string Filter = "{}")
		{
			IQueryable<GarmentBeacukai> Query = this.dbSet.Include(m => m.Items);

			List<string> searchAttributes = new List<string>()
			{
				"beacukaiNo", "suppliername","customsType","items.garmentdono"
			};

			Query = QueryHelper<GarmentBeacukai>.ConfigureSearch(Query, searchAttributes, Keyword);

			Dictionary<string, string> FilterDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Filter);
			Query = QueryHelper<GarmentBeacukai>.ConfigureFilter(Query, FilterDictionary);

			Dictionary<string, string> OrderDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(Order);
			Query = QueryHelper<GarmentBeacukai>.ConfigureOrder(Query, OrderDictionary);

			Pageable<GarmentBeacukai> pageable = new Pageable<GarmentBeacukai>(Query, Page - 1, Size);
			List<GarmentBeacukai> Data = pageable.Data.ToList();
			int TotalData = pageable.TotalCount;

			return Tuple.Create(Data, TotalData, OrderDictionary);
		}

		public GarmentBeacukai ReadById(int id)
		{
			var model = dbSet.Where(m => m.Id == id)
			 .Include(m => m.Items)
			 .FirstOrDefault();
			return model;
		}

		public string GenerateBillNo()
		{
			string BillNo = null;
			GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
												  orderby data.BillNo descending
												  select data).FirstOrDefault();
			string year = DateTimeOffset.Now.Year.ToString().Substring(2, 2);
			string month = DateTimeOffset.Now.Month.ToString("D2");
			string hour = (DateTimeOffset.Now.Hour+7).ToString("D2");
			string day = DateTimeOffset.Now.Day.ToString("D2");
			string minute = DateTimeOffset.Now.Minute.ToString("D2");
			string second = DateTimeOffset.Now.Second.ToString("D2");
			string formatDate = year + month + day + hour + minute + second;
			int counterId = 0;
			if (deliveryOrder.BillNo != null)
			{
				BillNo = deliveryOrder.BillNo;
				string months = BillNo.Substring(4, 2);
				string number = BillNo.Substring(14);
				if (months == DateTimeOffset.Now.Month.ToString("D2"))
				{
					counterId = Convert.ToInt32(number) + 1;
				}
				else
				{
					counterId = 1;
				}
			}
			else
			{
				counterId = 1;

			}
			BillNo = "BP" + formatDate + counterId.ToString("D6");
			return BillNo;

		}

		public (string format, int counterId) GeneratePaymentBillNo()
		{
			string PaymentBill = null;
			GarmentDeliveryOrder deliveryOrder = (from data in dbSetDeliveryOrder
												  orderby data.PaymentBill descending
												  select data).FirstOrDefault();
			string year = DateTimeOffset.Now.Year.ToString().Substring(2, 2);
			string month = DateTimeOffset.Now.Month.ToString("D2");
			string day = DateTimeOffset.Now.Day.ToString("D2");
			string formatDate = year + month + day;
			int counterId = 0;
			if (deliveryOrder.BillNo != null)
			{
				PaymentBill = deliveryOrder.PaymentBill;
				string date = PaymentBill.Substring(2, 6);
				string number = PaymentBill.Substring(8);
				if (date == formatDate)
				{
					counterId = Convert.ToInt32(number) + 1;
				}
				else
				{
					counterId = 1;
				}
			}
			else
			{
				counterId = 1;
			}
			//PaymentBill = "BB" + formatDate + counterId.ToString("D3");

			return (string.Concat("BB", formatDate), counterId);

		}
		public async Task<int> Create(GarmentBeacukai model, string username, int clientTimeZoneOffset = 7)
		{
			int Created = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{

					EntityExtension.FlagForCreate(model, username, USER_AGENT);

                    var lastPaymentBill = GeneratePaymentBillNo();

					foreach (GarmentBeacukaiItem item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
															.ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
						{
							
							if (model.BillNo == "" | model.BillNo == null)
							{
								deliveryOrder.BillNo = GenerateBillNo();

							}
							else
							{
								deliveryOrder.BillNo = model.BillNo;
							}
                            deliveryOrder.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                            //deliveryOrder.CustomsId = model.Id;
                            double qty = 0;
							foreach (var deliveryOrderItem in deliveryOrder.Items)
							{
								foreach (var detail in deliveryOrderItem.Details)
								{
									qty += detail.DOQuantity;
								}
							}
							item.TotalAmount = Convert.ToDecimal(deliveryOrder.TotalAmount);
							item.TotalQty = qty;
							EntityExtension.FlagForCreate(item, username, USER_AGENT);
						}
					}

					this.dbSet.Add(model);
					Created = await dbContext.SaveChangesAsync();
					transaction.Commit();
					foreach (GarmentBeacukaiItem item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
															.ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
						{
							deliveryOrder.CustomsId = model.Id;
						}
					}
					Created = await dbContext.SaveChangesAsync();
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Created;
		}
		public int Delete(int id, string username)
		{
			int Deleted = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
					var model = this.dbSet
						.Include(d => d.Items)
						.SingleOrDefault(pr => pr.Id == id && !pr.IsDeleted);

					EntityExtension.FlagForDelete(model, username, USER_AGENT);

					foreach (var item in model.Items)
					{
						GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.FirstOrDefault(s => s.Id == item.GarmentDOId);
						if (deliveryOrder != null)
						{
							deliveryOrder.BillNo = null;
							deliveryOrder.PaymentBill = null;
							deliveryOrder.CustomsId = 0;
							EntityExtension.FlagForDelete(item, username, USER_AGENT);
						}

					}

					Deleted = dbContext.SaveChanges();
					transaction.Commit();
				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Deleted;
		}

		public HashSet<long> GetGarmentBeacukaiId(long id)
		{
			return new HashSet<long>(dbContext.GarmentBeacukaiItems.Where(d => d.GarmentBeacukai.Id == id).Select(d => d.Id));
		}

		public async Task<int> Update(int id, GarmentBeacukaiViewModel vm, GarmentBeacukai model, string user, int clientTimeZoneOffset = 7)
		{
			int Updated = 0;

			using (var transaction = this.dbContext.Database.BeginTransaction())
			{
				try
				{
                    GarmentBeacukai modelBC = dbSet.AsNoTracking().Include(a=>a.Items).FirstOrDefault(s => s.Id == model.Id);

                    EntityExtension.FlagForUpdate(model, user, USER_AGENT);

                    var lastPaymentBill = GeneratePaymentBillNo();

                    foreach(GarmentBeacukaiItem item in model.Items)
                    {
                        GarmentBeacukaiItem oldItem= modelBC.Items.FirstOrDefault(s => s.Id.Equals(item.Id));
                        GarmentBeacukaiItemViewModel itemVM= vm.items.FirstOrDefault(s => s.deliveryOrder.Id.Equals(item.GarmentDOId));
                        if (itemVM.selected)
                        {
                            if (oldItem == null)
                            {
                                GarmentDeliveryOrder deliveryOrder = dbSetDeliveryOrder.Include(m => m.Items)
                                                            .ThenInclude(i => i.Details).FirstOrDefault(s => s.Id == item.GarmentDOId);
                                if (deliveryOrder != null)
                                {

                                    if (model.BillNo == "" | model.BillNo == null)
                                    {
                                        deliveryOrder.BillNo = GenerateBillNo();

                                    }
                                    else
                                    {
                                        deliveryOrder.BillNo = model.BillNo;
                                    }
                                    deliveryOrder.PaymentBill = string.Concat(lastPaymentBill.format, (lastPaymentBill.counterId++).ToString("D3"));
                                    //deliveryOrder.CustomsId = model.Id;
                                    double qty = 0;
                                    foreach (var deliveryOrderItem in deliveryOrder.Items)
                                    {
                                        foreach (var detail in deliveryOrderItem.Details)
                                        {
                                            qty += detail.DOQuantity;
                                        }
                                    }
                                    item.TotalAmount = Convert.ToDecimal(deliveryOrder.TotalAmount);
                                    item.TotalQty = qty;
                                    EntityExtension.FlagForCreate(item, user, USER_AGENT);

                                    deliveryOrder.CustomsId = model.Id;
                                }
                            }
                            else if (oldItem != null)
                            {
                                item.TotalAmount=oldItem.TotalAmount;
                                item.TotalQty = oldItem.TotalQty;
                                EntityExtension.FlagForUpdate(item, user, USER_AGENT);
                            }


                        }
                        else
                        {
                            EntityExtension.FlagForDelete(item, user, USER_AGENT);
                            GarmentDeliveryOrder deleteDO = dbContext.GarmentDeliveryOrders.FirstOrDefault(s => s.Id == item.GarmentDOId);
                            deleteDO.BillNo = null;
                            deleteDO.PaymentBill = null;
                            deleteDO.CustomsId = 0;
                        }
                    }


                    this.dbSet.Update(model);
					Updated = await dbContext.SaveChangesAsync();
					transaction.Commit();

				}
				catch (Exception e)
				{
					transaction.Rollback();
					throw new Exception(e.Message);
				}
			}

			return Updated;

		}
	}
}