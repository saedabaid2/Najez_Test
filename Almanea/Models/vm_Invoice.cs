using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Almanea.Models
{
    public class vm_Invoice
    {
        public string Company_Title { get; set; }
        public string Company_Phone { get; set; }
        public string Company_Email { get; set; }
        public string Company_Website { get; set; }

        public string InvoiceDate { get; set; }
        public string DueDate { get; set; }
        public string InvoiceNo { get; set; }
        public string UserGroup { get; set; }

        public List<vm_Invoice_OrderDate> Orders { get; set; }

        public string PeriodTime { get; set; }
        public int TotalOrders { get; set; }


        public decimal ServiceAmount { get; set; }
        public decimal AdditonalAmount { get; set; }

        public decimal Total { get; set; }
        public decimal Vat { get; set; }
        public decimal SubTotal { get; set; }

        public decimal PayToSp { get; set; }
        public decimal Due { get; set; }
        public decimal Additional { get; set; }
    }

    public class vm_Invoice_OrderDate
    {
        public string Date { get; set; }
        public List<vm_Invoice_Orders> Orders { get; set; }
    }


    public class vm_Invoice_Orders
    {
        public string OrderNo { get; set; }
        public decimal AdditionalWorkPrice { get; set; }
        public string InvoiceNo { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNo { get; set; }

        public string Area { get; set; }
        public string OrderDate { get; set; }
        public string InstallDate { get; set; }
        public decimal SubTotal { get; set; }
        public decimal AdditonalAmount { get; set; }
        public decimal AdditionalVat { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }

        public string ServicesName { get; set; }

        public string Services_Title { get; set; }
        public int Services_Quantity { get; set; }
        public decimal Services_UnitPrice { get; set; }
        public decimal Services_Vat { get; set; }
        public decimal Services_Total { get; set; }
        public List<vm_Invoice_Services> Services { get; set; }
    }

    public class vm_Invoice_Orders_Print
    {
        public string OrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public string InstallDate { get; set; }
        public string ServicesTitle { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class vm_Invoice_Orders_Service_Print
    {
        public string OrderNo { get; set; }
        public string InvoiceNo { get; set; }
        public string InstallDate { get; set; }
        public string ServicesTitle { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Amount { get; set; }
        public decimal Vat { get; set; }
        public decimal AdditonalAmount { get; set; }
        public decimal AdditionalVat { get; set; }
        public decimal SubTotal { get; set; }
    }

    public class vm_Invoice_Services
    {
        public string Title { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Vat { get; set; }
        public decimal Total { get; set; }
        public decimal AdditonalAmount { get; set; }
        public decimal AdditionalVat { get; set; }
        public decimal AdditionalWork { get; set; }
    }


    public class vm_MenuCount
    {

        public int AllOrders { get; set; }
        public int Completed { get; set; }
        public int Archieve { get; set; }
        public int Cancel { get; set; }
        public int Finish { get; set; }

        public int NewComplain { get; set; }
        public int Resolved { get; set; }

        public int NewService { get; set; }


    }
    public class vm_MenuCount2
    {

        public int AllOrders { get; set; }
        public int Completed { get; set; }
        public int Archieve { get; set; }
        public int Cancel { get; set; }
        public int Finish { get; set; }

        public int Complain { get; set; }
        public int Resolved { get; set; }
        public int Quantity { get; set; }
        public int Pending { get; set; }
        public int Reserved { get; set; }



    }
    public class vm_SalesMenuCount
    {

        public int AllOrders { get; set; }
        public int Completed { get; set; }
        public int Archieve { get; set; }
        public int Cancel { get; set; }

        public int Quantity { get; set; }
        public int Pending { get; set; }
        public int Reserved { get; set; }

        public int UnitOfInstallation { get; set; }

        public int COMPLAINBYCUSTOMER { get; set; }
        public int COMPLAINBYSUPPLIER { get; set; }

        public int NUMBEROFUNITCANCELLED { get; set; }
        public int NUMBEROFUNITCOMPLETED { get; set; }

        public int RESPONSERATE { get; set; }
        public int RESPONSERATEBYAPPCONFIRMED { get; set; }
    }

    public class vm_Dashboard
    {
        public int OpneComplain { get; set; }
        public int CloseComplain { get; set; }
        public int AverageClosingTime { get; set; }

        public int MissingOrders { get; set; }
        public int MissingOrdersUnit { get; set; }
        public int UnassignedOrders { get; set; }
        public int UnassignedOrdersUnit { get; set; }
        public int TotalRevenue { get; set; }


        public int TotalTodayOrderInstallation { get; set; }
        public int TotalTodayOrderInstallationUnit { get; set; }

        public int TotalTomorrowOrderInstallation { get; set; }
        public int TotalTomorrowOrderInstallationUnit { get; set; }

        public int TotalCompletedOrderofMonth { get; set; }
        public int TotalServiceOrder { get; set; }

        public List<vm_DashboardLabourlist> LabourList { get; set; }
        public List<vm_DashboarMonthOrderlist> MonthOrderlist { get; set; }
        public List<vm_DashboardOrderServicelist> OrderServicelist { get; set; }

    }
    public class vm_DashboardLabourlist
    {
        public string ServiceProvider { get; set; }
        public string Labour { get; set; }
        public int TodaysOrders { get; set; }
        public int TodayOrdersTotalUnit { get; set; }

        public int TotalOrders { get; set; }
        public string Performance { get; set; }

    }
    public class vm_DashboarMonthOrderlist
    {
        public string Day { get; set; }
        public int Order { get; set; }
    }
    public class vm_DashboardOrderServicelist
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; }
        public int TotalServiceOrders { get; set; }

    }

    public class vm_DayOrderCount
    {


        public int daycount { get; set; }
        public int nextDaycount { get; set; }



    }
    public class vm_MultipleComplains
    {

        // public string Title { get; set; }
        public int ComplainTypeId { get; set; }
        public string TitleAR { get; set; }
        public string TitleEN { get; set; }

        public int Complainid { get; set; }
    }
}