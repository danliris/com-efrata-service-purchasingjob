using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel
{
    class GarmentPreparingViewModel : BaseViewModel
    {
        public Guid Identity { get; set; }
        public bool IsPreparing { get; set; }
        public DateTimeOffset ExpenditureDate { get; set; }
        public DateTimeOffset ProcessDate { get; set; }
        public bool IsCuttingIn { get; set; }
        public long UENId { get; set; }
        public string UENNo { get; set; }
        public UnitViewModel Unit { get; set; }
        public string RONo { get; set; }
        public string Article { get; set; }
        public BuyerViewModel Buyer { get; set; }
        public virtual List<GarmentPreparingItemViewModel> Items { get; set; }

    }
}
