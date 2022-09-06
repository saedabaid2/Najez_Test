using System;
using System.Collections.Generic;

using System.Web.Http;

using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using Almanea.BusinessLogic;
using Almanea.Data;
using Almanea.Models;
using AutoMapper;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.ExtendedProperties;
namespace Almanea.Controllers
{
    public class orderController : ApiController
    {
        private db_Settings objSettings = new db_Settings();
        private AlmaneaDbEntities db = new AlmaneaDbEntities();
        private db_User objUser = new db_User();
        private bool isEnglish = cls_Defaults.IsEnglish;
        // GET api/<controller>


        //[HttpPost]
        public async Task<vm_jsOutput> Post([FromBody] vm_Order data)
        {
            var output = new vm_jsOutput();
            try
            {
                var serializer = new JavaScriptSerializer();
                //var services = data.Services.ToString();
                //var services = serializer.Deserialize<List<vm_OrderServices>>(strServices);

                var model = data;

                if (string.IsNullOrEmpty(model.InstallDate))
                {
                    if (model.PreferDate == 2)
                    {
                        output.Message = Translation.ReqInstallDate;
                    }
                    else
                    {
                        output.StatusId = 424;
                        output.Message = "error";
                    }
                    return output;
                }
                foreach (var item in data.Service)
                {
                    var servicename = item.ServiceNameAR;
                    if (string.IsNullOrEmpty(servicename))
                        servicename = item.ServiceNameEN;
                    var serviceId = await objSettings.GetService(servicename);
                    item.ServiceId = serviceId;
                }
                if (model.OrderId > 0)
                {
                    var StatusId = await objSettings.EditOrderAPI(model, model.Service);

                    output.StatusId = StatusId;
                    if (StatusId == -2)
                        output.Message = Translation.OrderInvoiceExist;
                    else
                        output.Message = Translation.success_EditOrder;

                }
                else
                {
                    var orderId = await objSettings.NewOrder(model, model.Service);
                    if (orderId > 0)
                    {
                        var OrderNo = (orderId).ToString();
                        db_Settings objSettings = new db_Settings();
                        var setting = await objSettings.GetSetting();
                        var IsProoduction = Convert.ToInt32(setting.IsProoduction);
                        if (IsProoduction == 1)
                        {
                            await cls_Sms.NewOrderAPI(data.UserGroupId,data.GroupTypeId, orderId, OrderNo, null, model.Service);

                        }
                        else
                            await cls_Sms.NewOrderAPI(data.UserGroupId, data.GroupTypeId, orderId, OrderNo, null, model.Service);
                        output.StatusId = orderId;
                        output.Message = Translation.success_NewOrder;
                    }
                    else if (orderId == -2)
                    {
                        output.StatusId = orderId;
                        output.Message = Translation.OrderInvoiceExist;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return output;

        }
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        //public void Post([FromBody] string value)
        //{
        //}

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}