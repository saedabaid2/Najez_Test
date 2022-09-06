using System;

namespace Almanea.Models
{

	public class vm_InventoryMaster
	{
		public int Id { get; set; }

		public string LabourId { get; set; }

		public DateTime StartDate { get; set; }

		public DateTime EndDate { get; set; }

		public string ItemId { get; set; }

		public int Quantity { get; set; }

		public int AvalQuantity { get; set; }

        public int notifyme { get; set; }
		public int notifytxt { get; set; }

	}
}