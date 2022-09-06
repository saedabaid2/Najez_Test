using System.ComponentModel.DataAnnotations;

namespace Almanea.Models
{

	public class vm_ServicesMapper
	{
		[Key]
		public string EncryptId { get; set; }

		public int ServiceId { get; set; }

		[Display(Name = "ServiceProvider", ResourceType = typeof(Translation))]
		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
		public int? ServiceProviderId { get; set; }

		public int? SupplierId { get; set; }

		[Required(ErrorMessageResourceName = "Required", ErrorMessageResourceType = typeof(Translation))]
		public string Estimated { get; set; }

		public bool isworking { get; set; }

		public string ServiceNameEN { get; set; }

		public virtual tblService tblService { get; set; }

		public bool InventoryRequired { get; set; }
	}
}