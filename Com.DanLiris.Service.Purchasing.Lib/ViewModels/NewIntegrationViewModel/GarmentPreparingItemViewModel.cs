using Com.DanLiris.Service.Purchasing.Lib.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Com.DanLiris.Service.Purchasing.Lib.ViewModels.NewIntegrationViewModel
{
    class GarmentPreparingItemViewModel : BaseViewModel
    {
        public Guid Identity { get; set; }
        public long UENItemId { get; set; }
        public ProductViewModel Product { get; set; }
        public string DesignColor { get; set; }
        public double Quantity { get; set; }
        public double RemainingQuantity { get; set; }
        public UomViewModel Uom { get; set; }
        public decimal BasicPrice { get; set; }
        public string ROSource { get; set; }
        public string FabricType { get; set; }

    }
}
