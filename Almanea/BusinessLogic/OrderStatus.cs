namespace Almanea.BusinessLogic { 

public enum OrderStatus
{
	NewOrder = 1,
	Reserved = 2,
	Rejected = 3,
	ReSchedule = 4,
	AppointmentConfirmed = 5,
	ReceivedFromWarehouse = 14,
	AppointmentReschedule = 15,
	PartialDelivery = 16,
	Job_in_Progress = 6,
	HoldOn = 7,
	ChangeService = 8,
	Finish = 9,
	Complete = 10,
	Delete = 11,
	Cancel = 12,
	Release = 13,
	AssignDriver = 17,
	AssignLabour = 18,
	Postponed = 20
}
}