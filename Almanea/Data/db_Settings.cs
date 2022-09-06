using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;
using LinqKit;

namespace Almanea.Data
{

    public class db_Settings : cls_Dispose
    {
        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        private AlmaneaDbEntities _context;

        private bool isEnglish = cls_Defaults.IsEnglish;

        private db_User objUser = new db_User();

        public db_Settings()
        {
            _context = new AlmaneaDbEntities();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public async Task<vm_Settings> GetSetting()
        {
            List<tblSetting> setting = await _context.tblSettings.ToListAsync();
            new vm_Settings();
            try
            {
                return new vm_Settings
                {
                    Vat = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("vat")).KeyValue,
                    OrderDuration = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderduration")).KeyValue,
                    CompanyName = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyname")).KeyValue,
                    CompanyEmail = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyemail")).KeyValue,
                    CompanyPhone = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyphone")).KeyValue,
                    CompanyWebsite = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companywebsite")).KeyValue,
                    ContractPercent = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("contractpercent")).KeyValue,
                    OrderShow = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("ordershowduration")).KeyValue),
                    PreferInstallAsap = Convert.ToBoolean(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderinstallasap")).KeyValue),
                    MinOrdersPerDay = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("minordersperday")).KeyValue),
                    IsProoduction = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("isprooduction")).KeyValue),
                    Autodispatch = Convert.ToBoolean(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("autodispatch")).KeyValue),
                    MaxNumberOfUnit = Convert.ToInt16(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("maxnumberofunit")).KeyValue),
                    SessionTimeOut = Convert.ToInt16(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("sessiontimeout")).KeyValue)
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<vm_Settings> GetServicesBlockDate(List<int> Sl)
        {
            List<string> SP = (from x in _context.tblServices
                               where Sl.Contains(x.ServiceId)
                               select x.ServiceProviderId).ToList();
            List<int> ServiceProviderList = new List<int>();
            foreach (string item2 in SP)
            {
                string value = item2;
                string[] SPList = value.Split(',');
                string[] array = SPList;
                foreach (string SPid in array)
                {
                    ServiceProviderList.Add(Convert.ToInt32(SPid));
                }
            }
            List<tblSetting> setting = await _context.tblSettings.Where((tblSetting s) => ServiceProviderList.Contains(s.ProviderId) && s.KeyName == "BlockDate").ToListAsync();
            string blockdate = "";
            int total = 0;
            foreach (tblSetting item in setting)
            {
                total++;
                blockdate = ((setting.Count != total) ? ((total != 1) ? (blockdate + item.KeyValue + ",") : (blockdate + item.KeyValue + ",")) : (blockdate + item.KeyValue));
            }
            new vm_Settings();
            try
            {
                return new vm_Settings
                {
                    BlockDate = blockdate
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public SelectListItem GetServicesmap(int? spid, int? serviceid)
        {
            return (from x in _context.tblServiceMappers
                    where x.ServiceProviderId == spid && x.ServiceId == serviceid && x.Estimated != null
                    select new SelectListItem
                    {
                        Value = x.Estimated.ToString(),
                        Text = ((object)x.ServiceProviderId).ToString()
                    }).FirstOrDefault();
        }

        public tblServiceMapper GetServicesmap2(int? spid, int? serviceid)
        {
            return _context.tblServiceMappers.Where((tblServiceMapper x) => x.ServiceProviderId == spid && x.ServiceId == serviceid && x.Estimated != null).FirstOrDefault();
        }

        public tblTeamCapacityCalculation GetSPCapacitynotinadded1(int? spid, DateTime? installdetatime, int orderid)
        {
            tblTeamCapacityCalculation model = (from x in _context.tblTeamCapacityCalculations
                                                where x.ServiceProviderId == spid && x.InstallDate == installdetatime
                                                orderby x.Id descending
                                                select x).FirstOrDefault();
            int spestimat = 0;
            int qty = 0;
            int calcva = 0;
            int dailcapacity = 0;
            List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(spid));
            tblProviderWorkinHour wokrhrs = GetEorkinhHrsy(Convert.ToInt32(spid));
            if (labours.Count > 0 && labours != null && labours.Count > 0)
            {
                dailcapacity = Convert.ToInt32(labours.Count * wokrhrs.WorkingHours);
            }
            List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == orderid).ToList();
            foreach (tblOrderService item in OrderServicssse)
            {
                tblServiceMapper spassignedservice = GetServicesmap2(Convert.ToInt32(spid), item.ServiceId);
                spestimat = Convert.ToInt32(spassignedservice.Estimated);
                calcva += item.Quantity * spestimat / 60;
            }
            if (model != null)
            {
                decimal totaloccperct = default(decimal);
                int totaloccperctcapaictydaily = 0;
                int totaloccperctcapcitycurent = 0;
                int capacityconsumed = 0;
                totaloccperctcapcitycurent = Convert.ToInt32(model.CurrentCapacity);
                tblTeamCapacityCalculation capcal2 = new tblTeamCapacityCalculation();
                capcal2.Updatedate = DateTime.Now;
                capcal2.InstallDate = Convert.ToDateTime(installdetatime);
                capcal2.OrderId = orderid;
                capcal2.ServiceProviderId = Convert.ToInt32(spid);
                capcal2.DailyCapacity = dailcapacity;
                capcal2.ConsumedCapacity = calcva;
                capcal2.CurrentCapacity = totaloccperctcapcitycurent - calcva;
                double curavailabe2 = (double)capcal2.CurrentCapacity.Value / (double)capcal2.DailyCapacity.Value * 100.0;
                if (curavailabe2 < 0.0)
                {
                    curavailabe2 = 0.0;
                }
                capcal2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                _context.tblTeamCapacityCalculations.Add(capcal2);
                _context.SaveChanges();
            }
            else
            {
                tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                capcal.Updatedate = DateTime.Now;
                capcal.InstallDate = Convert.ToDateTime(installdetatime);
                capcal.OrderId = orderid;
                capcal.ServiceProviderId = Convert.ToInt32(spid);
                capcal.DailyCapacity = dailcapacity;
                capcal.ConsumedCapacity = calcva;
                capcal.CurrentCapacity = dailcapacity - calcva;
                double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                if (curavailabe < 0.0)
                {
                    curavailabe = 0.0;
                }
                capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
                _context.tblTeamCapacityCalculations.Add(capcal);
                _context.SaveChanges();
            }
            return model;
        }

        public tblTeamCapacityCalculation GetSPCapacitynotinadded(int? spid, DateTime? installdetatime, int orderid)
        {
            tblTeamCapacityCalculation model = (from x in _context.tblTeamCapacityCalculations
                                                where x.ServiceProviderId == spid && x.InstallDate == installdetatime
                                                orderby x.Id descending
                                                select x).FirstOrDefault();
            if (model == null)
            {
                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(spid));
                if (labours.Count > 0)
                {
                    tblSetting wokrhrs = GetEorkinhHrsysettings(Convert.ToInt32(spid));
                    if (labours != null && labours.Count > 0)
                    {
                        int dailcapacity = labours.Count * Convert.ToInt32(wokrhrs.KeyValue);
                        tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                        capcal.Updatedate = DateTime.Now;
                        capcal.InstallDate = installdetatime;
                        capcal.OrderId = orderid;
                        capcal.ServiceProviderId = spid;
                        capcal.DailyCapacity = dailcapacity;
                        capcal.CurrentCapacity = dailcapacity;
                        double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                        capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
                        _context.tblTeamCapacityCalculations.Add(capcal);
                        _context.SaveChanges();
                    }
                    return (from x in _context.tblTeamCapacityCalculations
                            where x.ServiceProviderId == spid
                            orderby x.Id descending
                            select x).FirstOrDefault();
                }
                return (from x in _context.tblTeamCapacityCalculations
                        where x.ServiceProviderId == spid && x.Updatedate == DateTime.UtcNow
                        orderby x.Id descending
                        select x).FirstOrDefault();
            }
            return model;
        }

        public tblTeamCapacityCalculation GetSPCapacity(int? spid)
        {
            return (from x in _context.tblTeamCapacityCalculations
                    where x.ServiceProviderId == spid && x.Updatedate == DateTime.UtcNow
                    orderby x.Id descending
                    select x).FirstOrDefault();
        }

        public tblProviderWorkinHour GetEorkinhHrsy(int? spid)
        {
            return _context.tblProviderWorkinHours.Where((tblProviderWorkinHour x) => x.ServiceProviderId == spid).FirstOrDefault();
        }

        public tblSetting GetEorkinhHrsysettings(int? spid)
        {
            return _context.tblSettings.Where((tblSetting x) => x.KeyName == "WorkingHours" && (int?)x.ProviderId == spid).FirstOrDefault();
        }

        public List<tblOrder> GetOrdersInstaldate(int? spid)
        {
            return _context.tblOrders.Where((tblOrder x) => (int?)x.ReservedProvider == spid && x.InstallDate > DateTime.UtcNow).ToList();
        }

        public int HasInventory(int? spid)
        {
            tblSetting model = _context.tblSettings.Where((tblSetting x) => x.KeyName == "TeamCapacityPercentage" && (int?)x.ProviderId == spid).FirstOrDefault();
            if (model != null)
            {
                if (!model.HasInventory.HasValue)
                {
                    return 0;
                }
                return model.HasInventory.Value;
            }
            return 0;
        }

        public tblSetting Getpercensetting(int? spid)
        {
            return _context.tblSettings.Where((tblSetting x) => x.KeyName == "TeamCapacityPercentage" && (int?)x.ProviderId == spid).FirstOrDefault();
        }

        public tblSetting GetBlockeddate(int? spid)
        {
            return _context.tblSettings.Where((tblSetting x) => x.KeyName == "BlockDate" && (int?)x.ProviderId == spid).FirstOrDefault();
        }

        public List<SelectListItem> GetProviderSetting(int? groupid)
        {
            return (from x in _context.tblProviderSettingMappers
                    where x.SupplierId == groupid
                    select new SelectListItem
                    {
                        Value = ((object)x.SupplierId).ToString(),
                        Text = ((object)x.ServiceProviderId).ToString()
                    }).ToList();
        }

        public List<int?> GetSpOrderDistributed(int? orderId)
        {
            return (from x in _context.OrderDisplays
                    where x.OrderId == orderId
                    select x into c
                    select c.ProviderId).Distinct().ToList();
        }

        public List<int?> GetProviderSettingID(int? groupid)
        {
            return (from x in _context.tblProviderSettingMappers
                    where x.SupplierId == groupid
                    select x into c
                    select c.ServiceProviderId).ToList();
        }

        public vm_ProviderSettings GetSPSetting()
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            tblProviderSettingMapper setting = _context.tblProviderSettingMappers.Where((tblProviderSettingMapper x) => x.SupplierId == (int?)UserGroupId).FirstOrDefault();
            vm_ProviderSettings model = new vm_ProviderSettings();
            try
            {
                if (setting != null)
                {
                    model.IsInternal = setting.IsInternal.HasValue && setting.IsInternal.Value;
                    return model;
                }
                return model;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public vm_Settings GetSettin1g()
        {
            List<tblSetting> setting = _context.tblSettings.ToList();
            vm_Settings model = new vm_Settings();
            try
            {
                return new vm_Settings
                {
                    Vat = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("vat")).KeyValue,
                    OrderDuration = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderduration")).KeyValue,
                    CompanyName = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyname")).KeyValue,
                    CompanyEmail = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyemail")).KeyValue,
                    CompanyPhone = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyphone")).KeyValue,
                    CompanyWebsite = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companywebsite")).KeyValue,
                    ContractPercent = setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("contractpercent")).KeyValue,
                    OrderShow = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("ordershowduration")).KeyValue),
                    PreferInstallAsap = Convert.ToBoolean(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderinstallasap")).KeyValue),
                    MinOrdersPerDay = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("minordersperday")).KeyValue),
                    IsProoduction = Convert.ToInt16(setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("isprooduction")).KeyValue),
                    BlockDate = setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("blockdate")).KeyValue,
                    Autodispatch = Convert.ToBoolean(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("autodispatch")).KeyValue),
                    MaxNumberOfUnit = Convert.ToInt16(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("maxnumberofunit")).KeyValue),
                    SessionTimeOut = Convert.ToInt16(setting.FirstOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("sessiontimeout")).KeyValue)
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string EncryptString(string toEncrypt, bool useHashing)
        {
            byte[] toEncryptArray = Encoding.UTF8.GetBytes(toEncrypt);
            byte[] initVectorBytes = Encoding.ASCII.GetBytes("$secure#");
            string key = "abcdefghijklmnop";
            byte[] keyArray;
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
            else
            {
                keyArray = Encoding.UTF8.GetBytes(key);
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.PKCS7;
            tdes.IV = initVectorBytes;
            ICryptoTransform cTransform = tdes.CreateEncryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        public string DecryptString(string EncryptOrderId, bool useHashing)
        {
            byte[] toEncryptArray = Convert.FromBase64String(EncryptOrderId);
            byte[] initVectorBytes = Encoding.ASCII.GetBytes("$secure#");
            string key = "abcdefghijklmnop";
            byte[] keyArray;
            if (useHashing)
            {
                MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();
                keyArray = hashmd5.ComputeHash(Encoding.UTF8.GetBytes(key));
            }
            else
            {
                keyArray = Encoding.UTF8.GetBytes(key);
            }
            TripleDESCryptoServiceProvider tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = keyArray;
            tdes.Mode = CipherMode.CBC;
            tdes.Padding = PaddingMode.PKCS7;
            tdes.IV = initVectorBytes;
            ICryptoTransform cTransform = tdes.CreateDecryptor();
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            return Encoding.UTF8.GetString(resultArray);
        }

        public async Task<int> AddProviderMapInternal(vm_ProviderSettings model, int?[] basic)
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            tblAdminUser addedby = _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 17).FirstOrDefault();
            if (addedby != null)
            {
                _context.tblProviderSettingMappers.RemoveRange(_context.tblProviderSettingMappers.Where((tblProviderSettingMapper x) => x.SupplierId == (int?)UserGroupId));
                _context.SaveChanges();
                List<tblUserGroupCompany> spintern = _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => x.AddedBy == addedby.UserId && x.IsInternal == true).ToList();
                if (spintern.Count > 0)
                {
                    foreach (tblUserGroupCompany item in spintern)
                    {
                        tblProviderSettingMapper inputs = new tblProviderSettingMapper
                        {
                            IsInternal = model.IsInternal,
                            ServiceProviderId = item.UserGroupId,
                            SupplierId = UserGroupId
                        };
                        _context.tblProviderSettingMappers.Add(inputs);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return 1;
        }

        public async Task<int> AddProviderMap(vm_ProviderSettings model, int?[] basic)
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            if (basic.Length != 0)
            {
                _context.tblProviderSettingMappers.RemoveRange(_context.tblProviderSettingMappers.Where((tblProviderSettingMapper x) => x.SupplierId == (int?)UserGroupId));
                _context.SaveChanges();
                for (int i = 0; i < basic.Length; i++)
                {
                    tblProviderSettingMapper inputs = new tblProviderSettingMapper
                    {
                        IsInternal = model.IsInternal,
                        ServiceProviderId = basic[i],
                        SupplierId = UserGroupId
                    };
                    _context.tblProviderSettingMappers.Add(inputs);
                }
            }
            if (UserGroupId > 0 || basic.Length != 0)
            {
                await _context.SaveChangesAsync();
            }
            return 1;
        }

        public async Task<int> AddEditTeamCapacityPercentage(int? TeamCapacityPercentage)
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            if (UserGroupId > 0 && TeamCapacityPercentage >= 0)
            {
                tblSetting Setting = await _context.tblSettings.SingleOrDefaultAsync((tblSetting x) => x.KeyName == UserGroupId.ToString());
                if (Setting == null)
                {
                    tblSetting inputs2 = new tblSetting
                    {
                        KeyName = UserGroupId.ToString(),
                        KeyValue = TeamCapacityPercentage.ToString()
                    };
                    _context.tblSettings.Add(inputs2);
                }
                else
                {
                    Setting.KeyName = UserGroupId.ToString();
                    Setting.KeyValue = TeamCapacityPercentage.ToString();
                }
            }
            if (UserGroupId > 0 && TeamCapacityPercentage >= 0)
            {
                await _context.SaveChangesAsync();
            }
            return 1;
        }

        public int EditCapacity(int? providerId, bool status)
        {
            List<tblTeamCapacityCalculation> model = db.tblTeamCapacityCalculations.Where((tblTeamCapacityCalculation x) => x.ServiceProviderId == providerId && x.InstallDate >= DateTime.UtcNow).ToList();
            tblSetting wokrhrs = GetEorkinhHrsysettings(providerId);
            int dailycap = Convert.ToInt32(wokrhrs.KeyValue);
            if (model.Count > 0)
            {
                foreach (tblTeamCapacityCalculation item in model)
                {
                    if (!status)
                    {
                        item.DailyCapacity = Convert.ToInt32(item.DailyCapacity) - dailycap;
                        item.CurrentCapacity = Convert.ToInt32(item.CurrentCapacity) - dailycap;
                        double curavailabe2 = (double)item.CurrentCapacity.Value / (double)item.DailyCapacity.Value * 100.0;
                        item.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                    }
                    if (status)
                    {
                        item.DailyCapacity = Convert.ToInt32(item.DailyCapacity) + dailycap;
                        item.CurrentCapacity = Convert.ToInt32(item.CurrentCapacity) + dailycap;
                        double curavailabe = (double)item.CurrentCapacity.Value / (double)item.DailyCapacity.Value * 100.0;
                        item.CapcityPercentage = Convert.ToDecimal(curavailabe);
                    }
                    item.ServiceProviderId = providerId;
                    item.Updatedate = DateTime.UtcNow;
                    db.Entry(item).State = EntityState.Modified;
                    db.SaveChanges();
                }
            }
            return 1;
        }

        public string GetLaborBlockDate(int? Id)
        {
            tblLaborInactive datess = _context.tblLaborInactives.Where((tblLaborInactive x) => x.LabourId == Id).FirstOrDefault();
            if (datess != null)
            {
                return datess.InactiveDates.ToString();
            }
            return "";
        }

        public int AddCapacity(int? providerId)
        {
            tblTeamCapacityCalculation tblTeamCapacity2 = (from x in db.tblTeamCapacityCalculations
                                                           where x.ServiceProviderId == providerId
                                                           orderby x.Id descending
                                                           select x).FirstOrDefault();
            tblSetting wokrhrs = GetEorkinhHrsysettings(providerId);
            int dailycap = Convert.ToInt32(wokrhrs.KeyValue);
            if (tblTeamCapacity2 != null)
            {
                tblTeamCapacity2.DailyCapacity = Convert.ToInt32(tblTeamCapacity2.DailyCapacity) + dailycap;
                tblTeamCapacity2.CurrentCapacity = Convert.ToInt32(tblTeamCapacity2.CurrentCapacity) + dailycap;
                tblTeamCapacity2.ServiceProviderId = providerId;
                tblTeamCapacity2.Updatedate = DateTime.Now;
                double curavailabe2 = (double)tblTeamCapacity2.CurrentCapacity.Value / (double)tblTeamCapacity2.DailyCapacity.Value * 100.0;
                tblTeamCapacity2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                db.Entry(tblTeamCapacity2).State = EntityState.Modified;
                db.SaveChanges();
            }
            else
            {
                tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                capcal.Updatedate = DateTime.Now;
                capcal.ServiceProviderId = providerId;
                capcal.DailyCapacity = dailycap;
                capcal.CurrentCapacity = dailycap;
                double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
                _context.tblTeamCapacityCalculations.Add(capcal);
                _context.SaveChanges();
            }
            return 1;
        }

        public async Task<int> EditSetting(vm_Settings model)
        {
            DbSet<tblSetting> settings = _context.tblSettings;
            tblSetting vat = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("vat"));
            vat.KeyValue = model.Vat;
            tblSetting orderduration = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderduration"));
            orderduration.KeyValue = model.OrderDuration;
            tblSetting companyname = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyname"));
            companyname.KeyValue = model.CompanyName;
            tblSetting companyphone = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyphone"));
            companyphone.KeyValue = model.CompanyPhone;
            tblSetting companyemail = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companyemail"));
            companyemail.KeyValue = model.CompanyEmail;
            tblSetting companywebsite = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("companywebsite"));
            companywebsite.KeyValue = model.CompanyWebsite;
            tblSetting OrderDuration = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("ordershowduration"));
            OrderDuration.KeyValue = model.OrderShow.ToString();
            tblSetting installAsap = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("orderinstallasap"));
            installAsap.KeyValue = model.PreferInstallAsap.ToString();
            tblSetting autodispatch = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("autodispatch"));
            autodispatch.KeyValue = model.Autodispatch.ToString();
            tblSetting sessiontimeout = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("sessiontimeout"));
            sessiontimeout.KeyValue = ((model.SessionTimeOut == 0) ? null : model.SessionTimeOut.ToString());
            tblSetting maxnumberofunit = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("minordersperday"));
            maxnumberofunit.KeyValue = ((model.MinOrdersPerDay == 0) ? null : model.MinOrdersPerDay.ToString());
            tblSetting isProduction = settings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("isprooduction"));
            isProduction.KeyValue = model.IsProoduction.ToString();
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> EditProviderSetting(vm_Settings model, int? TeamCapacityPercentage, int WorkingHours, bool HasInventory, bool notifyme)
        {
            int HasInventory_int = 0;
            int notifyme_int = 0;
            int ProviderId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            DbSet<tblSetting> settings = _context.tblSettings;
            if (HasInventory)
            {
                HasInventory_int = 1;
            }
            if (notifyme)
            {
                notifyme_int = 1;
            }
            tblSetting blockDate = settings.Where((tblSetting x) => x.ProviderId == ProviderId).SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("blockdate"));
            if (blockDate != null)
            {
                blockDate.KeyValue = ((!string.IsNullOrEmpty(model.BlockDate)) ? model.BlockDate.ToString() : null);
                _context.Entry(blockDate).State = EntityState.Modified;
            }
            else
            {
                tblSetting inputs3 = new tblSetting
                {
                    KeyName = "BlockDate",
                    KeyValue = ((!string.IsNullOrEmpty(model.BlockDate)) ? model.BlockDate.ToString() : null),
                    ProviderId = ProviderId
                };
                _context.tblSettings.Add(inputs3);
            }
            await _context.SaveChangesAsync();
            tblSetting Setting = settings.Where((tblSetting x) => x.ProviderId == ProviderId).SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("TeamCapacityPercentage"));
            if (Setting == null)
            {
                tblSetting inputs4 = new tblSetting
                {
                    KeyName = "TeamCapacityPercentage",
                    KeyValue = TeamCapacityPercentage.ToString(),
                    ProviderId = ProviderId,
                    HasInventory = HasInventory_int,
                    notify = notifyme_int
                };
                _context.tblSettings.Add(inputs4);
            }
            else
            {
                Setting.KeyValue = TeamCapacityPercentage.ToString();
                Setting.ProviderId = ProviderId;
                Setting.HasInventory = HasInventory_int;
                Setting.notify = notifyme_int;
                _context.Entry(Setting).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            tblSetting SettingWorkinHour = settings.Where((tblSetting x) => x.ProviderId == ProviderId).SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("WorkingHours"));
            if (SettingWorkinHour == null)
            {
                tblSetting inputs2 = new tblSetting
                {
                    KeyName = "WorkingHours",
                    KeyValue = WorkingHours.ToString(),
                    ProviderId = ProviderId
                };
                _context.tblSettings.Add(inputs2);
            }
            else
            {
                SettingWorkinHour.KeyValue = WorkingHours.ToString();
                SettingWorkinHour.ProviderId = ProviderId;
                _context.Entry(SettingWorkinHour).State = EntityState.Modified;
            }
            await _context.SaveChangesAsync();
            return 1;
        }

        public string GetSettingByKey(string Key)
        {
            List<tblSetting> setting = _context.tblSettings.ToList();
            return setting.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals(Key)).KeyValue;
        }

        public decimal Vat()
        {
            tblSetting setting = _context.tblSettings.SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("vat"));
            return Convert.ToDecimal(setting.KeyValue);
        }

        public int AddLocation(vm_Locations model)
        {
            tblLocation inputs = Mapper.Map<vm_Locations, tblLocation>(model);
            inputs.Direction = model.DirectionId;
            try
            {
                inputs.LocationId = _context.tblLocations.Max((tblLocation x) => x.LocationId) + 1;
                _context.tblLocations.Add(inputs);
                _context.SaveChanges();
            }
            catch (Exception)
            {
            }
            return inputs.LocationId;
        }

        public async Task<int> EditLocation(vm_Locations model)
        {
            tblLocation group = await _context.tblLocations.SingleOrDefaultAsync((tblLocation x) => x.LocationId == model.LocationId);
            group.LocationNameEN = model.LocationNameEN;
            group.LocationNameAR = model.LocationNameAR;
            group.Status = model.Status;
            group.Direction = model.DirectionId;
            group.UserGroupId = model.UserGroupId;
            await _context.SaveChangesAsync();
            return 1;
        }

        public Page<tblLocation> GetLocations(int pageSize, int currentPage, Sorts<tblLocation> sorts)
        {
            return _context.tblLocations.Paginate(currentPage, pageSize, sorts);
        }

        public Page<tblLocation> GetSupllierAdminLocations(int pageSize, int currentPage, Sorts<tblLocation> sorts, string UserGroupId)
        {
            return _context.tblLocations.Where((tblLocation l) => l.UserGroupId == UserGroupId).Paginate(currentPage, pageSize, sorts);
        }

        public List<SelectListItem> GetLocations()
        {
            return (from x in _context.tblLocations
                    where x.Status == true
                    select new SelectListItem
                    {
                        Value = x.LocationId.ToString(),
                        Text = ((isEnglish == true) ? x.LocationNameEN : x.LocationNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierAdminLocations(string UserGroupId)
        {
            return (from x in _context.tblLocations
                    where x.Status == true && x.UserGroupId == UserGroupId
                    select new SelectListItem
                    {
                        Value = x.LocationId.ToString(),
                        Text = ((isEnglish == true) ? x.LocationNameEN : x.LocationNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetServiceProviderLocations()
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int ActtypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
            tblAdminUser SupplierAdmin = new tblAdminUser();
            if (UserGroupTypeId == 1 && ActtypeId == 20)
            {
                int SupplierAdminId2 = GetSupplierOrProviderAdminId(UserId);
                SupplierAdmin = GetSupplierAdmin(SupplierAdminId2);
            }
            else if (UserGroupTypeId == 1)
            {
                int ServiceProviderId = GetSupplierOrProviderAdminId(UserId);
                int SupplierAdminId = GetSupplierOrProviderAdminId(ServiceProviderId);
                SupplierAdmin = GetSupplierAdmin(SupplierAdminId);
            }
            return (from x in _context.tblLocations
                    where x.Status == true && x.UserGroupId == ((object)SupplierAdmin.UserGroupId).ToString()
                    select new SelectListItem
                    {
                        Value = x.LocationId.ToString(),
                        Text = ((isEnglish == true) ? x.LocationNameEN : x.LocationNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierAdminAgent(int UserGroupId)
        {
            return (from x in _context.tblAdminUsers
                    where x.Status == true && x.UserGroupId == (int?)UserGroupId && (x.AccountTypeId == 15 || x.AccountTypeId == 14)
                    select new SelectListItem
                    {
                        Value = x.UserId.ToString(),
                        Text = string.Concat(x.FirstName + " ", x.LastName)
                    }).ToList();
        }

        public async Task<List<vm_Locations>> GetLocationsList()
        {
            List<vm_Locations> output = Mapper.Map<List<tblLocation>, List<vm_Locations>>(await _context.tblLocations.Where((tblLocation x) => x.Status == true).ToListAsync());
            foreach (vm_Locations item in output)
            {
                item.LocationNameEN = (isEnglish ? item.LocationNameEN : item.LocationNameAR);
            }
            return output;
        }

        public void InserOrUpdateServiceMapper(vm_Services entity)
        {
            List<int> serviceProviderId = new List<int>();
            List<int> supplierId = new List<int>();
            try
            {
                if (entity.ServiceProviderId != null)
                {
                    serviceProviderId = entity.ServiceProviderId.Split(',').Select(int.Parse).ToList();
                }
                if (entity.SupplierId != null)
                {
                    supplierId = entity.SupplierId.Split(',').Select(int.Parse).ToList();
                }
                tblServiceMapper sm = new tblServiceMapper
                {
                    ServiceId = entity.ServiceId
                };
                List<tblServiceMapper> collection = _context.tblServiceMappers.Where((tblServiceMapper k) => k.ServiceId == sm.ServiceId).ToList();
                foreach (tblServiceMapper item in collection)
                {
                    _context.tblServiceMappers.Remove(item);
                    _context.SaveChanges();
                }
                foreach (int ss in supplierId)
                {
                    sm.SupplierId = ss;
                    if (serviceProviderId.Count > 0)
                    {
                        foreach (int pp in serviceProviderId)
                        {
                            sm.ServiceProviderId = pp;
                            _context.Entry(sm).State = EntityState.Added;
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        _context.Entry(sm).State = EntityState.Added;
                        _context.SaveChanges();
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public async Task<int> AddService(vm_Services model)
        {
            tblService inputs = Mapper.Map<vm_Services, tblService>(model);
            _context.tblServices.Add(inputs);
            await _context.SaveChangesAsync();
            return inputs.ServiceId;
        }

        public async Task<int> EditService(vm_Services model)
        {
            (await _context.tblServices.SingleOrDefaultAsync((tblService x) => x.ServiceId == model.ServiceId)).Estimatetime = model.Estimatetime;
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> EditService2(vm_Services model)
        {
            tblService group = await _context.tblServices.SingleOrDefaultAsync((tblService x) => x.ServiceId == model.ServiceId);
            group.ServiceProviderId = model.ServiceProviderId;
            group.CategoryId = Convert.ToInt16(model.CategoryId);
            group.ServiceNameEN = model.ServiceNameEN;
            group.ServiceNameAR = model.ServiceNameAR;
            group.UnitPrice = model.UnitPrice;
            group.Status = model.Status;
            group.IsDisplay = model.IsDisplay;
            _context.Entry(group).State = EntityState.Modified;
            _context.SaveChanges();
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> EditServiceMapper(int? serviceId, string estimated, int? providerid, bool isworking, string[] items, string[] quantity, bool InventoryRequired)
        {
            int i = 0;
            tblServiceMapper group = await _context.tblServiceMappers.SingleOrDefaultAsync((tblServiceMapper x) => x.ServiceId == serviceId && x.ServiceProviderId == providerid);
            List<Service_Items> existServiceItems = _context.Service_Items.Where((Service_Items x) => x.ServiceId == serviceId).ToList();
            if (existServiceItems != null && existServiceItems.Count > 0)
            {
                _context.Service_Items.RemoveRange(existServiceItems);
            }
            if (items != null)
            {
                foreach (string item in items)
                {
                    Service_Items Service_Items = new Service_Items();
                    Service_Items.ItemId = int.Parse(item);
                    Service_Items.ServiceId = serviceId;
                    Service_Items.quantity = int.Parse(quantity[i].ToString());
                    _context.Service_Items.Add(Service_Items);
                    i++;
                }
                _context.SaveChanges();
            }
            group.Estimated = estimated;
            group.IsWorking = isworking;
            group.tblService.InventoryRequired = (InventoryRequired ? 1 : 0);
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> EditServicesAsSeen(List<int> serviceIds, int? providerid)
        {
            if (serviceIds.Count > 0)
            {
                foreach (int item in serviceIds)
                {
                    (await _context.tblServiceMappers.SingleOrDefaultAsync((tblServiceMapper x) => x.ServiceId == (int?)item && x.ServiceProviderId == providerid)).ServiceAcceptStatus = true;
                    await _context.SaveChangesAsync();
                }
            }
            return 1;
        }

        public Page<tblService> GetServices(int pageSize, int currentPage, Sorts<tblService> sorts, Filters<tblService> filters)
        {
            return _context.tblServices.Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblService> GetSupplierAdminServices(int pageSize, int currentPage, Sorts<tblService> sorts, Filters<tblService> filters, int UserGroupId)
        {
            return _context.tblServices.Where((tblService s) => s.SupplierId == UserGroupId.ToString()).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblService> GetServiceswithmap(int pageSize, int currentPage, Sorts<tblService> sorts, Filters<tblService> filters)
        {
            return _context.tblServices.Paginate(currentPage, pageSize, sorts, filters);
        }

        public async Task<tblService> GetServicesById(int serviceId)
        {
            return await _context.tblServices.FindAsync(serviceId);
        }

        public tblService GetServicesById2(int serviceId)
        {
            return _context.tblServices.Find(serviceId);
        }

        public async Task<List<vm_ServiceItem>> GetServiceItemById(int serviceId)
        {
            List<Service_Items> model = await (from x in _context.Service_Items.Include((Service_Items x) => x.tblService)
                                               where x.ServiceId == (int?)serviceId
                                               select x).ToListAsync();
            List<vm_ServiceItem> vm_ServiceItems = new List<vm_ServiceItem>();
            foreach (Service_Items ones in model)
            {
                vm_ServiceItem vm_ServiceItem = new vm_ServiceItem();
                vm_ServiceItem.ServiceId = ones.ServiceId.Value;
                vm_ServiceItem.ItemId = ones.ItemId.Value;
                vm_ServiceItem.quantity = ones.quantity.Value;
                vm_ServiceItems.Add(vm_ServiceItem);
            }
            return vm_ServiceItems;
        }

        public tblServiceMapper GetServicesMap(int? serviceid, int? providerid)
        {
            return (from x in _context.tblServiceMappers.Include((tblServiceMapper x) => x.tblService)
                    where x.ServiceId == serviceid && x.ServiceProviderId == providerid
                    select x).FirstOrDefault();
        }

        public bool RemoveOrderAssigned(int OrderId)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                IQueryable<OrdersAssigned> ordersAssigneds = context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)OrderId);
                if (ordersAssigneds != null)
                {
                    context.OrdersAssigneds.RemoveRange(ordersAssigneds);
                }
                context.SaveChanges();
            }
            return true;
        }

        public bool AddEditOrderAssigned(int LabourId, int Status, int OrderId, string isLeader, int ServiceId, string Quantity, int userGroupTypeId)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                int driverId = GetUser(LabourId);
                if (driverId == -1)
                {
                    List<tblAdminUser> drivers = objUser.GetAvailDrivers(userGroupTypeId);
                    driverId = drivers.FirstOrDefault().UserId;
                }
                tblOrderService orderService = context.tblOrderServices.FirstOrDefault((tblOrderService x) => x.OrderId == OrderId);
                OrdersAssigned orderAssigned = new OrdersAssigned();
                orderAssigned.LabourId = LabourId;
                orderAssigned.Status = Status;
                orderAssigned.OrderId = OrderId;
                orderAssigned.ServiceId = ServiceId;
                orderAssigned.Quantity = Quantity;
                orderAssigned.Isleader = isLeader;
                orderAssigned.Total = orderService.Quantity;
                orderAssigned.DriverId = driverId;
                orderAssigned.Created_At = DateTime.Now;
                context.OrdersAssigneds.Add(orderAssigned);
                context.SaveChanges();
            }
            return true;
        }

        public async Task<List<SelectListItem>> GetSPListsetting()
        {
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            int AccountTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            if (UserGroupTypeId == 2 && AccountTypeId == 14)
            {
                UserId = GetSupplierOrProviderAdminId(UserId);
            }
            _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserId && x.AccountTypeId == 17).FirstOrDefault();
            tblAdminUser SuperAdmin = objUser.GetSuperAdminId();
            List<SelectListItem> model = (from x in _context.tblUserGroupCompanies
                                          where x.Status == true && x.UserGroupTypeId == 1
                                          where x.AddedBy == UserId || x.AddedBy == SuperAdmin.UserId
                                          select new SelectListItem
                                          {
                                              Value = x.UserGroupId.ToString(),
                                              Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                                          }).ToList();
            foreach (SelectListItem item in model)
            {
                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(item.Value));
                if (labours.Count > 0)
                {
                    _ = labours.Count * 9;
                    item.Text = item.Text;
                }
            }
            return model;
        }

        public async Task<List<tblProviderSettingMapper>> GetSPListsettingAssigned()
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            return _context.tblProviderSettingMappers.Where((tblProviderSettingMapper x) => x.SupplierId == (int?)UserGroupId).ToList();
        }

        public async Task<List<SelectListItem>> GetSPList()
        {
            List<SelectListItem> model = (from x in _context.tblUserGroupCompanies
                                          where x.Status == true && x.UserGroupTypeId == 1
                                          select new SelectListItem
                                          {
                                              Value = x.UserGroupId.ToString(),
                                              Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                                          }).ToList();
            foreach (SelectListItem item in model)
            {
                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(item.Value));
                if (labours.Count > 0)
                {
                    item.Text = string.Concat(str2: (labours.Count * 9).ToString(), str0: item.Text, str1: "(", str3: "Hrs.)");
                }
            }
            return model;
        }

        public List<SelectListItem> GetServicesList()
        {
            List<tblService> asas = _context.tblServices.ToList();
            return (from x in _context.tblServices
                    where x.Status == true
                    select new SelectListItem
                    {
                        Value = x.ServiceId.ToString(),
                        Text = ((isEnglish == true) ? x.ServiceNameEN : x.ServiceNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierAdminServicesList(int UserGroupId)
        {
            List<tblService> asas = _context.tblServices.ToList();
            return (from x in _context.tblServices
                    where x.Status == true && x.SupplierId == UserGroupId.ToString()
                    select new SelectListItem
                    {
                        Value = x.ServiceId.ToString(),
                        Text = ((isEnglish == true) ? x.ServiceNameEN : x.ServiceNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetServices(int cat)
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            int AccountTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
            List<tblService> collection = (from x in _context.tblServices
                                           where x.Status == true
                                           where (int?)x.CategoryId == (int?)cat
                                           select x into k
                                           orderby k.ServiceId
                                           select k).ToList();
            return collection.Select((tblService x) => new SelectListItem
            {
                Value = x.ServiceId + "_" + x.UnitPrice,
                Text = (isEnglish ? x.ServiceNameEN : x.ServiceNameAR)
            }).ToList();
        }

        public List<SelectListItem> GetServices()
        {
            List<SelectListItem> selectListItem = new List<SelectListItem>();
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            int AccountTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
            List<tblService> collection = (from x in _context.tblServices
                                           where x.Status == true
                                           select x into k
                                           orderby k.ServiceId
                                           select k).ToList();
            if (UserGroupTypeId == 3)
            {
                return collection.Select((tblService x) => new SelectListItem
                {
                    Value = x.ServiceId + "_" + x.UnitPrice,
                    Text = (isEnglish ? x.ServiceNameEN : x.ServiceNameAR)
                }).ToList();
            }
            foreach (tblService item in collection)
            {
                bool contain = false;
                if (UserGroupTypeId == 1 && !string.IsNullOrEmpty(item.ServiceProviderId))
                {
                    List<int> ck2 = item.ServiceProviderId.Split(',').Select(int.Parse).ToList();
                    contain = ck2.Contains(UserGroupId);
                }
                if (UserGroupTypeId == 2 && !string.IsNullOrEmpty(item.SupplierId))
                {
                    List<int> ck = item.SupplierId.Split(',').Select(int.Parse).ToList();
                    contain = ck.Contains(UserGroupId);
                }
                if (contain)
                {
                    SelectListItem ListItem = new SelectListItem
                    {
                        Value = item.ServiceId + "_" + item.UnitPrice,
                        Text = (isEnglish ? item.ServiceNameEN : item.ServiceNameAR),
                        Selected = true
                    };
                    selectListItem.Add(ListItem);
                }
            }
            return selectListItem;
        }

        public async Task<int> AddAdditionalWork(vm_AdditionalWork model)
        {
            tblAdditionalWork inputs = new tblAdditionalWork
            {
                AdditionalWorkId = model.AdditionalWorkId,
                AdditionalWorkNameAR = model.AdditionalWorkNameAR,
                AdditionalWorkNameEN = model.AdditionalWorkNameEN,
                UserGroupId = model.UserGroupId,
                CategoryrId = model.CategoryId,
                Price = model.Price
            };
            _context.tblAdditionalWorks.Add(inputs);
            await _context.SaveChangesAsync();
            return inputs.AdditionalWorkId;
        }

        public async Task<int> EditAdditionalWork(vm_AdditionalWork model)
        {
            tblAdditionalWork group = await _context.tblAdditionalWorks.SingleOrDefaultAsync((tblAdditionalWork x) => x.AdditionalWorkId == model.AdditionalWorkId);
            group.AdditionalWorkNameEN = model.AdditionalWorkNameEN;
            group.AdditionalWorkNameAR = model.AdditionalWorkNameAR;
            group.CategoryrId = model.CategoryId;
            group.Price = model.Price;
            await _context.SaveChangesAsync();
            return 1;
        }

        public Page<tblAdditionalWork> GetAdditionalWork(int pageSize, int currentPage, Sorts<tblAdditionalWork> sorts)
        {
            return _context.tblAdditionalWorks.Paginate(currentPage, pageSize, sorts);
        }

        public Page<tblAdditionalWork> GetSupplierAdminAdditionalWork(int pageSize, int currentPage, Sorts<tblAdditionalWork> sorts, int UserGroupId)
        {
            return _context.tblAdditionalWorks.Where((tblAdditionalWork a) => a.UserGroupId == (int?)UserGroupId).Paginate(currentPage, pageSize, sorts);
        }

        public async Task<tblAdditionalWork> GetAdditionalWorkById(int serviceId)
        {
            return await _context.tblAdditionalWorks.FindAsync(serviceId);
        }

        public List<vm_OrderAdditionalWork> GetAdditionalWorkByOrderId(int orderid)
        {
            List<tblOrderAdditionalWork> model = _context.tblOrderAdditionalWorks.Where((tblOrderAdditionalWork x) => x.OrderId == orderid).ToList();
            List<vm_OrderAdditionalWork> additionalworklist = new List<vm_OrderAdditionalWork>();
            foreach (tblOrderAdditionalWork item in model)
            {
                additionalworklist.Add(new vm_OrderAdditionalWork
                {
                    AdditionalWorkNameEN = item.tblAdditionalWork.AdditionalWorkNameEN,
                    AdditionalWorkNameAR = item.tblAdditionalWork.AdditionalWorkNameAR,
                    Price = item.tblAdditionalWork.Price
                });
            }
            return additionalworklist;
        }

        public List<vm_AdditionalWork> DDLAdditionlWork()
        {
            return _context.tblAdditionalWorks.Select((tblAdditionalWork x) => new vm_AdditionalWork
            {
                AdditionalWorkId = x.AdditionalWorkId,
                AdditionalWorkNameAR = x.AdditionalWorkNameAR,
                AdditionalWorkNameEN = x.AdditionalWorkNameEN,
                Price = x.Price
            }).ToList();
        }

        public List<vm_AdditionalWork> DDLAdditionlWork2(int cat)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int ServiceProviderId = GetSupplierOrProviderAdminId(UserId);
            tblAdminUser SupplierAdmin = GetSupplierAdmin(ServiceProviderId);
            return (from x in _context.tblAdditionalWorks
                    where x.UserGroupId == SupplierAdmin.UserGroupId && (int?)x.CategoryrId == (int?)cat
                    select new vm_AdditionalWork
                    {
                        AdditionalWorkId = x.AdditionalWorkId,
                        AdditionalWorkNameAR = x.AdditionalWorkNameAR,
                        AdditionalWorkNameEN = x.AdditionalWorkNameEN,
                        Price = x.Price
                    }).ToList();
        }

        private static DateTime ParseDate(string s)
        {
            if (!DateTime.TryParse(s, out var result))
            {
                return DateTime.ParseExact(s, "yyyy-MM-ddT24:mm:ssK", CultureInfo.InvariantCulture).AddDays(1.0);
            }
            return result;
        }

        public async Task<int> NewOrder(vm_Order order, List<vm_OrderServices> services)
        {
            int SupplierId = 0;
            if (HttpContext.Current.Session != null && HttpContext.Current.Session["S_UserGroupId"] != null)
            {
                SupplierId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString());
            }
            else
            {
                SupplierId = order.UserGroupId;
            }
            _context.tblUserGroupCompanies.Find(SupplierId);
            if (await _context.tblOrders.Where((tblOrder x) => x.InvoiceNo == order.InvoiceNo && x.SupplierId == SupplierId).FirstOrDefaultAsync() != null)
            {
                return -2;
            }
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {

                    try
                    {
                        tblOrder inputs = Mapper.Map<vm_Order, tblOrder>(order);
                        inputs.LocationId = order.LocationId;
                        inputs.PreferDate = 2;
                        inputs.CustomerCode = cls_Defaults.GenerateCode(3);
                        inputs.SupplierId = SupplierId;
                        inputs.ServiceVat = order.Vat;
                        inputs.ServiceTotal = order.TotalAmount - order.Vat;
                        inputs.TotalAmount = order.TotalAmount;
                        inputs.Comments = order.Comments;
                        inputs.Status = 1;
                        context.tblOrders.Add(inputs);
                        tblResetOrder restore = Mapper.Map<tblOrder, tblResetOrder>(inputs);
                        await context.SaveChangesAsync();
                        int OrderId = inputs.OrderId;
                        if (OrderId > 0)
                        {
                            restore.OrderId = OrderId;
                            context.tblResetOrders.Add(restore);
                            await context.SaveChangesAsync();
                            int AddedService = 0;
                            foreach (vm_OrderServices item in services)
                            {
                                item.OrderId = OrderId;
                                await NewOrderService(item, context);
                                AddedService++;
                            }
                            if (OrderId > 0 && AddedService > 0)
                            {
                                dbTrnx.Commit();
                                order.OrderId = OrderId;
                                await AddNewHistory(order);
                                return OrderId;
                            }
                            dbTrnx.Rollback();
                        }
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<int> NewOrderService(vm_OrderServices item, AlmaneaDbEntities context)
        {
            tblOrderService objService = new tblOrderService
            {
                ServiceId = item.ServiceId,
                Quantity = item.Quantity,
                IsActive = true,
                OrderId = item.OrderId,
                Unit = item.Unit
            };
            context.tblOrderServices.Add(objService);
            await context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> NewOrderService(List<vm_OrderServices> model, string Comments, decimal Vat, decimal Total)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                int orderId = model[0].OrderId;
                tblOrder order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == orderId);
                if (order == null)
                {
                    return -1;
                }
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = orderId,
                            ActivityDate = DateTime.Now,
                            Status = 8,
                            Comments = Comments,
                            UserId = UserId,
                            LabourId = model[0].LabourId,
                            DriverId = model[0].DriverId
                        };
                        context.tblOrderHistories.Add(history);
                        await context.SaveChangesAsync();
                        order.Status = 8;
                        order.ServiceTotal += (decimal?)Total;
                        order.ServiceVat += (decimal?)Vat;
                        order.TotalAmount += Total + Vat;
                        await context.SaveChangesAsync();
                        foreach (vm_OrderServices item in model)
                        {
                            item.IsAdditional = 1;
                            await NewOrderService(item, context);
                        }
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public Inventory_Master GetInventoryMaster(int Id)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
                return context.Inventory_Master.FirstOrDefault((Inventory_Master x) => x.Id == Id);
        }

        public int GetLaboursNotified(int QuantityNotified, int? UserGroupId)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
                return context.Inventory_Master.Where((Inventory_Master x) => x.UserGroupID == UserGroupId).Count((Inventory_Master x) => x.AvalQuantity <= (int?)x.notifytxt || x.EndDate < DateTime.Now);
        }

        public Inventory_Master GetInventoryMasterLabour(int Id)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
                return context.Inventory_Master.FirstOrDefault((Inventory_Master x) => x.LabourId == (int?)Id);
        }

        public Inventory_Master GetInventoryMasterByDate(int LabourId, int ItemId)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
                return context.Inventory_Master.FirstOrDefault(x => x.LabourId == (int?)LabourId && x.ItemId == (int?)ItemId);
        }

        public int GetInventoryDetails(int Id)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                int InventoryDetails = -1;
                if (context.Inventoy_Details.FirstOrDefault((Inventoy_Details x) => x.Master_Id == (int?)Id) != null)
                {
                    InventoryDetails = context.Inventoy_Details.Where((Inventoy_Details x) => x.Master_Id == (int?)Id).Sum((Inventoy_Details x) => x.Quantity).Value;
                }
                return InventoryDetails;
            }
        }

        public int GetInventoryDetailsLabour(string Id)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                int InventoryDetails = 0;
                if (context.Inventoy_Details.FirstOrDefault((Inventoy_Details x) => x.LabourId == Id) == null)
                {
                    return 0;
                }
                return context.Inventoy_Details.Where((Inventoy_Details x) => x.LabourId == Id).Sum((Inventoy_Details x) => x.Quantity).Value;
            }
        }
        public async Task<int> InventoryDetailUpdate(vm_InventoryDetail item)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        int i = int.Parse(item.LabourId);
                        int ServiceId = int.Parse(item.ServiceId);

                        var InventoryMaster = context.Inventory_Master.Where((Inventory_Master x) => x.LabourId == (int?)i).ToList();

                        foreach (var oneInventoryMaster in InventoryMaster)
                        {
                            var Service_Items = context.Service_Items.FirstOrDefault(x => x.ItemId == oneInventoryMaster.ItemId && x.ServiceId == ServiceId);
                            if (Service_Items != null)
                                oneInventoryMaster.AvalQuantity = oneInventoryMaster.AvalQuantity - Service_Items.quantity;

                        }
                        var InventoryMaster2 = context.Inventory_Master.FirstOrDefault((Inventory_Master x) => x.LabourId == (int?)i);

                        if (InventoryMaster2 != null)
                        {

                            Inventoy_Details inventory = new Inventoy_Details
                            {
                                Master_Id = InventoryMaster2.Id,
                                ServiceId = item.ServiceId,
                                ItemId = InventoryMaster2.ItemId.ToString(),
                                LabourId = item.LabourId,
                                Quantity = item.Quantity,
                                UserGroupID = item.userGroupId,
                                OrderId = item.orderId
                            };

                            context.Inventoy_Details.Add(inventory);

                        }
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();

                    }
                    catch (Exception ex)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 1;
        }

        public async Task<int> InventoryUpdate(vm_InventoryMaster item2, int UserGroupID, int notifytxt = 0)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        int item = int.Parse(item2.ItemId);
                        int LabourId = int.Parse(item2.LabourId);

                        Inventory_Master inventory_Master = context.Inventory_Master.FirstOrDefault(x => x.ItemId == item && x.LabourId == LabourId);
                        if (inventory_Master == null)
                            inventory_Master = new Inventory_Master();
                        inventory_Master.ItemId = int.Parse(item2.ItemId);
                        inventory_Master.LabourId = int.Parse(item2.LabourId);
                        inventory_Master.StartDate = DateTime.Now;//update date
                        inventory_Master.EndDate = DateTime.Now;
                        inventory_Master.notifytxt = notifytxt;
                        inventory_Master.UserGroupID = UserGroupID;
                        if (inventory_Master != null)
                        {
                            if (inventory_Master.Quantity == null)
                                inventory_Master.Quantity = 0;
                            if (inventory_Master.AvalQuantity == null)
                                inventory_Master.AvalQuantity = 0;
                            inventory_Master.Quantity += item2.Quantity;
                            inventory_Master.AvalQuantity += item2.Quantity;//Â«‰ «·„⁄ „œ…
                        }
                        else
                        {
                            inventory_Master.Quantity = item2.Quantity;
                            inventory_Master.AvalQuantity = item2.Quantity;//Â«‰ «·„⁄ „œ…
                        }
                        if (inventory_Master.Id == 0)
                            context.Inventory_Master.Add(inventory_Master);
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                    }
                    catch (Exception ex)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 1;
        }

        public async Task<int> EditOrderService(vm_OrderServices item, AlmaneaDbEntities context)
        {
            tblOrderService group = await context.tblOrderServices.SingleOrDefaultAsync((tblOrderService x) => x.OrderServiceId == item.OrderServiceId);
            group.IsActive = true;
            group.Quantity = item.Quantity;
            group.Unit = item.Unit;
            await context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> EditOrderService(int Id, bool IsActive)
        {
            (await _context.tblOrderServices.SingleOrDefaultAsync((tblOrderService x) => x.OrderServiceId == Id)).IsActive = IsActive;
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> GetService(string name)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
                return (await context.tblServices.FirstOrDefaultAsync((tblService x) => x.ServiceNameEN == name))?.ServiceId ?? (-1);
        }

        public async Task<int> EditOrder(vm_Order order, List<vm_OrderServices> services)
        {
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[cls_Defaults.Session_UserGroupId] != null)
            {
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString());
            }
            else
            {
                _ = order.UserGroupId;
            }
            if (await _context.tblOrders.Where((tblOrder x) => x.InvoiceNo == order.InvoiceNo && x.OrderId != order.OrderId).FirstOrDefaultAsync() != null)
            {
                return -2;
            }
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder group = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == order.OrderId);
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        group.SellerName = order.SellerName;
                        group.SellerContact = order.SellerContact;
                        group.InvoiceNo = order.InvoiceNo;
                        group.CustomerName = order.CustomerName;
                        group.CustomerContact = order.CustomerContact;
                        group.AlternateMobile = order.AlternateMobile;
                        group.LocationId = order.LocationId;
                        group.PreferDate = 2;
                        group.PrefferTime = order.PrefferTime;
                        if (order.PreferDate == 2)
                        {
                            group.InstallDate = DateTime.ParseExact(order.InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }
                        group.ServiceVat = order.ServiceVat;
                        group.ServiceTotal = order.TotalAmount - order.ServiceVat;
                        group.TotalAmount = order.TotalAmount;
                        group.SmsInArabic = order.SmsInArabic;
                        await context.SaveChangesAsync();
                        foreach (tblOrderService s in await context.tblOrderServices.Where((tblOrderService x) => x.OrderId == order.OrderId).ToListAsync())
                        {
                            s.IsActive = false;
                        }
                        await context.SaveChangesAsync();
                        int AddedService = 0;
                        foreach (vm_OrderServices item in services)
                        {
                            item.OrderId = order.OrderId;
                            if (item.OrderServiceId > 0)
                            {
                                await EditOrderService(item, context);
                            }
                            else
                            {
                                await NewOrderService(item, context);
                            }
                            AddedService++;
                        }
                        if (AddedService > 0)
                        {
                            dbTrnx.Commit();
                            return 1;
                        }
                        dbTrnx.Rollback();
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<int> EditOrderAPI(vm_Order order, List<vm_OrderServices> services)
        {
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[cls_Defaults.Session_UserGroupId] != null)
            {
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString());
            }
            else
            {
                _ = order.UserGroupId;
            }
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder group = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == order.OrderId);
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {

                    try
                    {
                        group.SellerName = order.SellerName;
                        group.SellerContact = order.SellerContact;
                        group.InvoiceNo = order.InvoiceNo;
                        group.CustomerName = order.CustomerName;
                        group.CustomerContact = order.CustomerContact;
                        group.AlternateMobile = order.AlternateMobile;
                        group.LocationId = order.LocationId;
                        group.PreferDate = 2;
                        group.PrefferTime = order.PrefferTime;
                        if (order.PreferDate == 2)
                        {
                            group.InstallDate = DateTime.ParseExact(order.InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        }
                        group.ServiceVat = order.ServiceVat;
                        group.ServiceTotal = order.TotalAmount - order.ServiceVat;
                        group.TotalAmount = order.TotalAmount;
                        group.SmsInArabic = order.SmsInArabic;
                        await context.SaveChangesAsync();
                        foreach (tblOrderService s in await context.tblOrderServices.Where((tblOrderService x) => x.OrderId == order.OrderId).ToListAsync())
                        {
                            s.IsActive = false;
                        }
                        await context.SaveChangesAsync();
                        int AddedService = 0;
                        foreach (vm_OrderServices item in services)
                        {
                            item.OrderId = order.OrderId;
                            if (item.OrderServiceId > 0)
                            {
                                await EditOrderService(item, context);
                            }
                            else
                            {
                                await NewOrderService(item, context);
                            }
                            AddedService++;
                        }
                        if (AddedService > 0)
                        {
                            dbTrnx.Commit();
                            return 1;
                        }
                        dbTrnx.Rollback();
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public int GetAssignedStatus(int OrderId)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                int? ord = (from x in context.OrdersAssigneds
                            where x.OrderId == (int?)OrderId
                            orderby x.Status
                            select x.Status).FirstOrDefault();
                if (!ord.HasValue)
                {
                    return 0;
                }
                return ord.Value;
            }
        }

        public async Task<int> EditBulkOrderStatus(vm_OrderStatus model, int[] LabourId, int status)
        {
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId].ToString());
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder group = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                List<OrdersAssigned> assigned = await (from x in context.OrdersAssigneds
                                                       where x.OrderId == (int?)model.OrderId
                                                       orderby x.LabourId descending
                                                       select x).ToListAsync();
                try
                {
                    int i = 0;
                    foreach (OrdersAssigned item in assigned)
                    {
                        item.Status = 2;
                        for (int ii2 = 0; ii2 < LabourId.Length; ii2++)
                        {
                            if (item.LabourId == LabourId[ii2])
                            {
                                item.Status = model.Status;
                                if (item.Status == (int)OrderStatus.Job_in_Progress)
                                {
                                    var srv_items = context.Service_Items.Where(x => x.ServiceId == item.ServiceId).ToList();
                                    if (srv_items != null && srv_items.Count() > 0)
                                    {
                                        foreach (var OneItem in srv_items)
                                        {
                                            Inventory_Master inventory_Master = context.Inventory_Master.FirstOrDefault(x => x.ItemId == OneItem.ItemId && x.LabourId == item.LabourId);
                                            if (inventory_Master == null)
                                            {
                                                inventory_Master = new Inventory_Master();
                                                inventory_Master.ItemId = OneItem.ItemId;
                                                inventory_Master.LabourId = item.LabourId;
                                                inventory_Master.StartDate = DateTime.Now;//update date
                                                inventory_Master.EndDate = DateTime.Now;
                                                inventory_Master.notifytxt = null;
                                                inventory_Master.UserGroupID = UserGroupId;
                                                inventory_Master.Quantity = 0;
                                                inventory_Master.AvalQuantity = 0;
                                                context.Inventory_Master.Add(inventory_Master);
                                                await context.SaveChangesAsync();
                                            }
                                        }
                                    }
                                }
                                if (item.Status == (int)OrderStatus.Finish)
                                {
                                    if (HttpContext.Current.Session[cls_Defaults.Session_HasInventory] != null && HttpContext.Current.Session[cls_Defaults.Session_HasInventory].ToString() == "1")
                                    {

                                        vm_InventoryDetail vm_InventoryDetails = new vm_InventoryDetail();
                                        vm_InventoryDetails.ServiceId = item.ServiceId.Value.ToString();
                                        vm_InventoryDetails.orderId = item.OrderId.Value;
                                        vm_InventoryDetails.Quantity = int.Parse(item.Quantity);
                                        vm_InventoryDetails.userGroupId = UserGroupId;
                                        vm_InventoryDetails.LabourId = item.LabourId.Value.ToString();
                                        await InventoryDetailUpdate(vm_InventoryDetails);
                                    }
                                }
                                break;
                            }
                        }
                        i++;
                    }

                    //Inventory_Master inventory_Master = context.Inventory_Master.FirstOrDefault(x => x.ItemId == item && x.LabourId == LabourId);
                    //if (inventory_Master == null)
                    //    inventory_Master = new Inventory_Master();
                    //inventory_Master.ItemId = int.Parse(item2.ItemId);
                    //inventory_Master.LabourId = int.Parse(item2.LabourId);
                    //inventory_Master.StartDate = DateTime.Now;//update date
                    //inventory_Master.EndDate = DateTime.Now;
                    //inventory_Master.notifytxt = notifytxt;
                    //inventory_Master.UserGroupID = UserGroupID;
                    //if (inventory_Master != null)
                    //{
                    //    inventory_Master.Quantity += item2.Quantity;
                    //    inventory_Master.AvalQuantity += item2.Quantity;//Â«‰ «·„⁄ „œ…
                    //}
                    //else
                    //{
                    //    inventory_Master.Quantity = item2.Quantity;
                    //    inventory_Master.AvalQuantity = item2.Quantity;//Â«‰ «·„⁄ „œ…
                    //}
                    //if (inventory_Master == null)
                    //    context.Inventory_Master.Add(inventory_Master);
                    //await context.SaveChangesAsync();


                    group.DriverId = model.DriverId;
                    group.Status = model.Status;
                    int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
                    for (int ii = 0; ii < LabourId.Length; ii++)
                    {
                        if (LabourId[ii] != 0)
                        {
                            string labourName = GetLabourorDriveNamebyId(LabourId[ii]);
                            tblOrderHistory history = new tblOrderHistory
                            {
                                OrderId = model.OrderId,
                                ActivityDate = DateTime.Now,
                                Status = model.Status,
                                Comments = labourName,
                                UserId = UserId,
                                FileAttachment = model.FileName,
                                ServiceProviderId = UserGroupId,
                                LabourId = LabourId[ii]
                            };
                            context.tblOrderHistories.Add(history);
                        }
                    }
                    await context.SaveChangesAsync();
                    return 1;
                }
                catch (Exception)
                {
                }
            }
            return 0;
        }

        public async Task<int> EditOrderStatus(vm_Order order)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder group = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == order.OrderId);
                List<OrdersAssigned> assigned = await context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)order.OrderId).ToListAsync();
                tblProviderTimeSlot providerTimeSlot = context.tblProviderTimeSlots.FirstOrDefault((tblProviderTimeSlot x) => x.OrderId == (int?)order.OrderId);
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        int i = 0;
                        foreach (OrdersAssigned item in assigned)
                        {
                            item.Status = order.Status;
                            i++;
                        }
                        group.DriverId = order.DriverId;
                        group.Status = order.Status;
                        group.ReservedProvider = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
                        if (providerTimeSlot != null)
                        {
                            providerTimeSlot.LabourId = group.LabourId;
                        }
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        private IQueryable<tblOrder> SearchOrders(string Seller, string Customer, string InvoiceNo, string InstallDate, int? Location, int TypeId)
        {
            Expression<Func<tblOrder, bool>> predicate = PredicateBuilder.False<tblOrder>();
            if (!string.IsNullOrEmpty(Seller))
            {
                predicate = predicate.And((tblOrder p) => p.SellerName.Equals(Seller));
            }
            if (!string.IsNullOrEmpty(Customer))
            {
                predicate = predicate.And((tblOrder p) => p.CustomerName.Equals(Customer));
            }
            if (!string.IsNullOrEmpty(InvoiceNo))
            {
                predicate = predicate.And((tblOrder p) => p.InvoiceNo.Contains(InvoiceNo));
            }
            if (Location > 0)
            {
                predicate = predicate.And((tblOrder p) => (int?)p.LocationId == Location);
            }
            if (!string.IsNullOrEmpty(InstallDate))
            {
                predicate = predicate.And((tblOrder p) => p.InstallDate == DateTime.ParseExact(InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture));
            }
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            Expression<Func<tblOrder, bool>> outer = PredicateBuilder.True<tblOrder>();
            if (UserGroupTypeId == 2)
            {
                outer = outer.And((tblOrder x) => x.SupplierId == UserGroupId);
                if (TypeId == 1)
                {
                    outer = outer.And((tblOrder x) => x.Status != 9);
                    outer = outer.And((tblOrder x) => x.Status != 10);
                    outer = outer.And((tblOrder x) => x.Status != 11);
                    outer = outer.And((tblOrder x) => x.Status != 12);
                }
                else
                {
                    outer = outer.And((tblOrder x) => x.Status != 9);
                    outer = outer.And((tblOrder x) => x.Status != 10);
                    outer = outer.And((tblOrder x) => x.Status != 11);
                    outer = outer.And((tblOrder x) => x.Status != 12);
                }
            }
            outer = outer.And(predicate);
            return _context.tblOrders.AsExpandable().Where(outer);
        }

        public IEnumerable<tblOrder> PrintSearch(string Seller, string Customer, string InvoiceNo, string InstallDate, int? Location, int TypeId)
        {
            IQueryable<tblOrder> so = SearchOrders(Seller, Customer, InvoiceNo, InstallDate, Location, TypeId);
            return so.ToList();
        }

        public List<int> getServiceByProvide(int providerId)
        {
            List<int> collection = (from l in _context.tblServiceMappers
                                    where l.ServiceProviderId == (int?)providerId
                                    select l into p
                                    select p.ServiceId ?? 0 into l
                                    orderby l
                                    select l).Distinct().ToList();
            return collection.ToList();
        }

        public Page<tblOrder> GetOrdersSP(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters)
        {
            return _context.tblOrders.Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetOrders(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters)
        {
            return _context.tblOrders.Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetSupplierAdminOrders(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, int UserId)
        {
            List<int> SupplierAdminUsers = (from u in _context.tblAdminUsers
                                            where u.AddedBy == UserId
                                            select u.UserId).ToList();
            return _context.tblOrders.Where((tblOrder o) => SupplierAdminUsers.Contains(o.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetOrdersOnlySP(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, int ProviderId, int AgentId)
        {
            IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)ProviderId).ToList()
                                                select x.UserId;
            List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)ProviderId).ToList();
            IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                     select x.OrderId;
            return _context.tblOrders.Where((tblOrder o) => slectedSPorderlistid.Contains(o.OrderId)).Paginate(currentPage, pageSize, sorts, filters);
        }

        public List<tblOrder> GetCalenderOrdersOnlySP(int ProviderId)
        {
            IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)ProviderId).ToList()
                                                select x.UserId;
            List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)ProviderId).ToList();
            IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                     select x.OrderId;
            int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
            DateTime now5Min = DateTime.Now.AddMinutes(-mins);
            return _context.tblOrders.Where((tblOrder o) => slectedSPorderlistid.Contains(o.OrderId) && o.AddedDate <= now5Min).ToList();
        }

        public Page<tblOrder> GetCancelandRejectedOrders(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters)
        {
            return _context.tblOrders.Where((tblOrder a) => a.Status == 3 || a.Status == 12).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetOrderssupplier(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, int SupplierId, int accountype, int userid)
        {
            Page<tblOrder> model = null;
            try
            {
                if (accountype == 15)
                {
                    IQueryable<tblOrder> data = _context.tblOrders.Where((tblOrder x) => x.SupplierId == SupplierId && x.AddedBy == userid).AsQueryable();
                    return data.Paginate(currentPage, pageSize, sorts, filters);
                }
                return _context.tblOrders.Where((tblOrder x) => x.SupplierId == SupplierId).AsQueryable().Paginate(currentPage, pageSize, sorts, filters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Page<tblOrder> GetOrderssupplierAgent(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, int SupplierId, int agentid)
        {
            return _context.tblOrders.Where((tblOrder x) => x.SupplierId == SupplierId && x.AddedBy == agentid).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetSalesReport(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, List<int> supplierId = null)
        {
            if (supplierId.Count > 1)
            {
                return _context.tblOrders.Where((tblOrder j) => supplierId.Contains(j.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);
            }
            return _context.tblOrders.Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrder> GetSupplierAdminSalesReport(int pageSize, int currentPage, Sorts<tblOrder> sorts, Filters<tblOrder> filters, int UserGroupTypeId, int AccountTypeId, int UserId, List<int> supplierId = null)
        {
            List<int> SupplierUser = (from u in _context.tblAdminUsers
                                      where u.AddedBy == UserId
                                      select u.UserId).ToList();
            if (UserGroupTypeId == 2 && AccountTypeId == 17)
            {
                return _context.tblOrders.Where((tblOrder o) => SupplierUser.Contains(o.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);
            }
            if (supplierId.Count > 1)
            {
                return _context.tblOrders.Where((tblOrder j) => supplierId.Contains(j.AddedBy) && SupplierUser.Contains(j.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);
            }
            return _context.tblOrders.Paginate(currentPage, pageSize, sorts, filters);
        }

        public async Task<vm_Order> GetOrderById(int Id)
        {
            vm_Order output = Mapper.Map<tblOrder, vm_Order>(await _context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == Id));
            output.Location = (isEnglish ? output.LocationEN : output.LocationAR);
            return output;
        }

        public vm_Order GetOrderById2(int Id)
        {
            tblOrder model = _context.tblOrders.FirstOrDefault((tblOrder x) => x.OrderId == Id);
            vm_Order output = Mapper.Map<tblOrder, vm_Order>(model);
            output.Location = (isEnglish ? output.LocationEN : output.LocationAR);
            return output;
        }

        public tblOrder GetOrderTimeslot(int Id)
        {
            return _context.tblOrders.FirstOrDefault((tblOrder x) => x.OrderId == Id && x.Status != 11);
        }

        public tblProviderTimeSlot GetOrderProviderTimeSlot(int OrderId, DateTime InstallDate)
        {
            return _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)OrderId && x.InstallDate.Value.Day == InstallDate.Day && x.InstallDate.Value.Month == InstallDate.Month && x.InstallDate.Value.Year == InstallDate.Year).FirstOrDefault();
        }

        public async Task<tblOrder> GetOrder(int Id)
        {
            return await _context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == Id && x.Status != 11);
        }

        public List<int> GetLaboursAssigned(int Id)
        {
            List<int> i = new List<int>();
            List<int?> model = (from x in _context.OrdersAssigneds
                                where x.OrderId == (int?)Id && x.Status != (int?)11
                                select x.LabourId).ToList();
            foreach (int? item in model)
            {
                i.Add(item.Value);
            }
            return i;
        }

        public async Task<vm_jsOutput> VerifyOrderActive(int OrderId)
        {
            vm_jsOutput output = new vm_jsOutput();
            vm_Order order = await GetOrderById(OrderId);
            if (order != null)
            {
                byte status = order.Status;
                int reservedProvider = order.ReservedProvider;
                if (status >= 2 && reservedProvider != Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]))
                {
                    output.StatusId = -2;
                    output.Message = Translation.OrderAlreadyReserved;
                }
            }
            return output;
        }

        public async Task<vm_jsOutput> VerifyOrder(int OrderId)
        {
            vm_jsOutput output = new vm_jsOutput();
            vm_Order order = await GetOrderById(OrderId);
            if (order != null)
            {
                byte status = order.Status;
                _ = order.ReservedProvider;
                if (status >= 2)
                {
                    output.StatusId = 2;
                }
                else if (status >= 1)
                {
                    output.StatusId = 1;
                }
            }
            return output;
        }

        public async Task<List<vm_Items>> GetItems()
        {
            return Mapper.Map<List<Item>, List<vm_Items>>(await _context.Items.ToListAsync());
        }

        public async Task<List<vm_Items>> GetItems2(int UserGroupId)
        {
            return Mapper.Map<List<Item>, List<vm_Items>>(await _context.Items.Where((Item x) => x.UserGroupId == (int?)UserGroupId).ToListAsync());
        }

        public async Task<List<vm_OrderServices>> GetOrderServiceById(int Id)
        {
            List<vm_OrderServices> output = Mapper.Map<List<tblOrderService>, List<vm_OrderServices>>(await _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == Id && x.IsActive == true).ToListAsync());
            foreach (vm_OrderServices item in output)
            {
                item.ServiceName = (isEnglish ? item.ServiceNameEN : item.ServiceNameAR);
            }
            return output;
        }

        public int getorderquantity(int OrderId)
        {
            return _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == OrderId).Sum((tblOrderService x) => x.Quantity);
        }

        public int getorderquantitybyService(int OrderId, int ServiceId)
        {
            return _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == OrderId && x.ServiceId == ServiceId).Sum((tblOrderService x) => x.Quantity);
        }

        public int getorderquantitybyDate(DateTime date)
        {
            int count = 0;
            try
            {
                return _context.tblOrderServices.Where((tblOrderService x) => x.tblOrder.InstallDate == date).DefaultIfEmpty().Sum((tblOrderService x) => x.Quantity);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public int getorderquantitybyDate2(string date)
        {
            try
            {
                DateTime datew = DateTime.ParseExact(date, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                var q = (from pd in _context.tblOrders
                         join od in _context.tblOrderServices on pd.OrderId equals od.OrderId
                         where DbFunctions.TruncateTime(pd.InstallDate) == datew
                         orderby od.OrderId
                         select new { od.Quantity }).ToList();
                int sum = 0;
                return q.Sum(z => z.Quantity);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async Task<List<string>> GetmultipleComplaintype(int complainid, bool IsEnglish)
        {
            List<Sp_multipleComplaintype_Result> objResult = _context.Sp_multipleComplaintype(complainid).ToList();
            List<vm_MultipleComplains> clients = new List<vm_MultipleComplains>();
            new List<vm_MultipleComplains>();
            foreach (Sp_multipleComplaintype_Result item in objResult)
            {
                clients.Add(Mapper.Map<Sp_multipleComplaintype_Result, vm_MultipleComplains>(item));
            }
            foreach (vm_MultipleComplains item2 in clients)
            {
                item2.TitleEN = (IsEnglish ? item2.TitleEN : item2.TitleAR);
            }
            if (clients.Count > 0)
            {
                return clients.Select((vm_MultipleComplains l) => l.TitleEN).ToList();
            }
            return null;
        }

        public async Task<int> UpdateStatus(int Id, byte StatusId)
        {
            tblOrder group = await _context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == Id);
            group.Status = StatusId;
            await _context.SaveChangesAsync();
            if (group.Status == 11)
            {
                List<OrderDisplay> getDistributeoredr = _context.OrderDisplays.Where((OrderDisplay em) => em.OrderId == (int?)Id).ToList();
                if (getDistributeoredr.Count != 0)
                {
                    foreach (OrderDisplay obj in getDistributeoredr)
                    {
                        _context.OrderDisplays.Remove(obj);
                    }
                }
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
                List<int?> splist = GetSpOrderDistributed(group.OrderId);
                if (splist.Count > 0)
                {
                    foreach (int? itemspid in splist)
                    {
                        tblTeamCapacityCalculation tblTeamCapacity2 = (from x in _context.tblTeamCapacityCalculations
                                                                       where x.ServiceProviderId == itemspid && x.OrderId == (int?)@group.OrderId
                                                                       orderby x.Id descending
                                                                       select x).FirstOrDefault();
                        tblTeamCapacity2.CurrentCapacity += ((!tblTeamCapacity2.ConsumedCapacity.HasValue) ? new int?(0) : tblTeamCapacity2.ConsumedCapacity);
                        tblTeamCapacity2.ConsumedCapacity = 0;
                        tblTeamCapacity2.ServiceProviderId = itemspid;
                        tblTeamCapacity2.Updatedate = DateTime.Now;
                        double curavailabe = (double)tblTeamCapacity2.CurrentCapacity.Value / (double)tblTeamCapacity2.DailyCapacity.Value * 100.0;
                        if (curavailabe < 0.0)
                        {
                            curavailabe = 0.0;
                        }
                        tblTeamCapacity2.CapcityPercentage = Convert.ToDecimal(curavailabe);
                        _context.Entry(tblTeamCapacity2).State = EntityState.Modified;
                        _context.SaveChanges();
                    }
                }
            }
            return 1;
        }

        public async Task<int> ReserveOrderByAutodispatch(int OrderId)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            tblOrder order1 = await _context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == OrderId);
            int dirverId = 0;
            int labourId = 0;
            List<tblAdminUser> drivers = await objUser.GetDrivers(UserGroupId);
            List<tblAdminUser> laboursandDrivers = await objUser.GetLaboursAndDrivers(UserGroupId);
            if (laboursandDrivers.Count > 0)
            {
                foreach (tblAdminUser item4 in laboursandDrivers)
                {
                    object AssignLabours2 = objUser.CheckDriveravailabilty(item4.UserId, Convert.ToDateTime(order1.InstallDate));
                    if (AssignLabours2 == null)
                    {
                        dirverId = item4.UserId;
                        labourId = item4.UserId;
                        break;
                    }
                }
            }
            else
            {
                foreach (tblAdminUser item3 in drivers)
                {
                    object AssignLabours = objUser.CheckDriveravailabilty(item3.UserId, Convert.ToDateTime(order1.InstallDate));
                    if (AssignLabours == null)
                    {
                        dirverId = item3.UserId;
                        break;
                    }
                }
            }
            if (labourId == 0)
            {
                objUser.CheckAssignLabours(order1.OrderId, Convert.ToDateTime(order1.InstallDate));
                List<tblAdminUser> labours = await objUser.GetLabours(UserGroupId);
                if (labours.Count > 0)
                {
                    foreach (tblAdminUser item2 in labours)
                    {
                        object AssignLabours3 = objUser.CheckDriveravailabilty(item2.UserId, Convert.ToDateTime(order1.InstallDate));
                        if (AssignLabours3 == null)
                        {
                            labourId = item2.UserId;
                            break;
                        }
                    }
                }
            }
            if (order1 != null)
            {
                if (order1.Status == 11)
                {
                    return -2;
                }
                if (order1.Status == 2)
                {
                    return -3;
                }
                if (order1.IsOnEdit)
                {
                    return -4;
                }
                try
                {
                    tblOrderHistory history = new tblOrderHistory
                    {
                        OrderId = OrderId,
                        ActivityDate = DateTime.Now,
                        Status = 2,
                        Comments = "Reserved by Autodispatch",
                        UserId = UserId,
                        LabourId = labourId,
                        DriverId = dirverId
                    };
                    _context.tblOrderHistories.Add(history);
                    _context.SaveChanges();
                    order1.Status = 2;
                    if (order1.Status == 2)
                    {
                        order1.ReservedProvider = UserGroupId;
                        order1.ReservedBy = UserId;
                    }
                    _context.SaveChanges();
                    if (dirverId > 0)
                    {
                        order1.DriverId = dirverId;
                        order1.Status = 17;
                        tblOrderHistory history2 = new tblOrderHistory
                        {
                            OrderId = OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 17,
                            Comments = "Reserved by Autodispatch",
                            UserId = UserId,
                            DriverId = dirverId
                        };
                        _context.tblOrderHistories.Add(history2);
                        _context.SaveChanges();
                    }
                    if (labourId > 0)
                    {
                        order1.Status = 18;
                        order1.DriverId = labourId;
                        tblOrderHistory history3 = new tblOrderHistory
                        {
                            OrderId = OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 18,
                            Comments = "Reserved by Autodispatch",
                            UserId = UserId,
                            LabourId = labourId
                        };
                        _context.tblOrderHistories.Add(history3);
                        _context.SaveChanges();
                    }
                    List<OrderDisplay> displayorderupdate = _context.OrderDisplays.Where((OrderDisplay x) => x.OrderId == (int?)order1.OrderId).ToList();
                    foreach (OrderDisplay item in displayorderupdate)
                    {
                        item.ReservedBy = UserId;
                        item.IsOrderReserved = true;
                        _context.SaveChanges();
                    }
                    return 1;
                }
                catch (Exception)
                {
                }
                return 0;
            }
            return -1;
        }

        public async Task<int> ReserveOrder(vm_OrderStatus model)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                if (order == null)
                {
                    return -1;
                }
                if (order.Status == 11)
                {
                    return -2;
                }
                if (order.Status == 2)
                {
                    return -3;
                }
                if (order.IsOnEdit)
                {
                    return -4;
                }
                try
                {
                    tblOrderHistory history = new tblOrderHistory
                    {
                        OrderId = model.OrderId,
                        ActivityDate = DateTime.Now,
                        Status = model.Status,
                        Comments = model.Comments,
                        UserId = UserId,
                        LabourId = model.LabourId[0],
                        DriverId = model.DriverId
                    };
                    if (model.Status == 4 || model.Status == 15)
                    {
                        tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                        vm_OrderList output = Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                        history.CommentsReschedule = output.InstallDate.ToString();
                    }
                    context.tblOrderHistories.Add(history);
                    await context.SaveChangesAsync();
                    order.Status = model.Status;
                    if (model.Status == 2)
                    {
                        order.ReservedProvider = UserGroupId;
                        order.ReservedBy = UserId;
                    }
                    await context.SaveChangesAsync();
                    List<OrderDisplay> displayorderupdate = context.OrderDisplays.Where((OrderDisplay x) => x.OrderId == (int?)order.OrderId).ToList();
                    foreach (OrderDisplay item in displayorderupdate)
                    {
                        item.ReservedBy = UserId;
                        item.IsOrderReserved = true;
                        await context.SaveChangesAsync();
                    }
                    return 1;
                }
                catch (Exception)
                {
                }
            }
            return 0;
        }

        public async Task<int> UpdateHistory(vm_OrderStatus model)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder order;
                if (model.Status == 1)
                {
                    model.Status = 2;
                    order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == 0);
                }
                else if (model.Status == 3)
                {
                    order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == 0);
                }
                else
                {
                    switch (UserGroupTypeId)
                    {
                        case 3:
                            order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                            break;
                        case 8:
                            order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                            break;
                        case 2:
                            order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.SupplierId == UserGroupId);
                            break;
                        default:
                            order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == UserGroupId);
                            break;
                    }
                }
                if (order == null)
                {
                    return -1;
                }
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {

                    try
                    {
                        if (model.Status == 16)
                        {
                            UserId = model.DriverId;
                        }
                        if (model.Status == 17)
                        {
                            model.Comments = GetLabourorDriveNamebyId(model.DriverId);
                        }
                        else if (model.Status == 18)
                        {
                            model.Comments = GetLabourorDriveNamebyId(model.LabourId[0]);
                        }
                        string ddtime = "";
                        if (model.Status == 5)
                        {
                            tblProviderTimeSlot tssl2 = GetTimeslot(model.OrderId);
                            if (tssl2 != null)
                            {
                                string timeslot2 = tssl2.StartHour + ":00-" + tssl2.EndHour + ":00";
                                ddtime = tssl2.InstallDate.ToString().Substring(0, 10) + " " + timeslot2;
                            }
                        }
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = model.Status,
                            Comments = model.Comments,
                            UserId = UserId,
                            FileAttachment = model.FileName,
                            ServiceProviderId = UserGroupId,
                            CommentsReschedule = ddtime
                        };
                        if (model.Status == 18)
                        {
                            string[] labourIds = model.LabourId.Split(',');
                            string[] array = labourIds;
                            foreach (string item3 in array)
                            {
                                history = new tblOrderHistory
                                {
                                    OrderId = model.OrderId,
                                    ActivityDate = DateTime.Now,
                                    Status = model.Status,
                                    Comments = model.Comments,
                                    UserId = UserId,
                                    FileAttachment = model.FileName,
                                    ServiceProviderId = UserGroupId,
                                    LabourId = int.Parse(item3),
                                    DriverId = model.DriverId,
                                    CommentsReschedule = ddtime
                                };
                            }
                        }
                        if (model.Status == 4 || model.Status == 15)
                        {
                            tblProviderTimeSlot tssl = GetTimeslot(model.OrderId);
                            if (tssl != null)
                            {
                                string timeslot = tssl.StartHour + ":00-" + tssl.EndHour + ":00";
                                ddtime = model.InstallDate.ToString().Substring(0, 10) + " " + timeslot;
                            }
                            tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                            Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                            history.CommentsReschedule = ddtime;
                        }
                        context.tblOrderHistories.Add(history);
                        await context.SaveChangesAsync();
                        if (model.Status != 3)
                        {
                            order.Status = model.Status;
                        }
                        if (model.Status == (int)OrderStatus.Finish)
                        {
                            order.InvoiceImages = model.InvoiceImage;
                            List<OrdersAssigned> assigned = await context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)order.OrderId).ToListAsync();
                            int i = 0;
                            foreach (OrdersAssigned item2 in assigned)
                            {
                                item2.Status = order.Status;
                                i++;
                            }
                            if (HttpContext.Current.Session[cls_Defaults.Session_HasInventory] != null && HttpContext.Current.Session[cls_Defaults.Session_HasInventory].ToString() == "1")
                            {
                                foreach (OrdersAssigned item in assigned)
                                {
                                    vm_InventoryDetail vm_InventoryDetails = new vm_InventoryDetail();
                                    vm_InventoryDetails.ServiceId = item.ServiceId.Value.ToString();
                                    vm_InventoryDetails.orderId = item.OrderId.Value;
                                    vm_InventoryDetails.Quantity = int.Parse(item.Quantity);
                                    vm_InventoryDetails.userGroupId = UserGroupId;
                                    vm_InventoryDetails.LabourId = item.LabourId.Value.ToString();
                                    await InventoryDetailUpdate(vm_InventoryDetails);
                                }
                            }
                        }
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public int CanUpdateAttachment(vm_OrderStatus model)
        {
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                model.Status = 2;
                tblOrderHistory order = context.tblOrderHistories.FirstOrDefault((tblOrderHistory x) => x.OrderId == model.OrderId && x.Status == 9);
                if (order != null)
                {
                    return 1;
                }
            }
            return 0;
        }

        public async Task<int> UpdateAttachment(vm_OrderStatus model)
        {
            Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder order;
                if (model.Status != 1)
                {
                    order = ((model.Status == 3) ? (await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == 0)) : ((UserGroupTypeId != 3) ? (await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == UserGroupId)) : (await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId))));
                }
                else
                {
                    model.Status = 2;
                    order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId && x.ReservedProvider == 0);
                }
                if (order == null)
                {
                    return -1;
                }
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        tblOrderHistory orderHistory = await context.tblOrderHistories.SingleOrDefaultAsync((tblOrderHistory x) => x.OrderId == model.OrderId && x.Status == model.Status);
                        orderHistory.Comments = model.Comments;
                        orderHistory.FileAttachment = model.InvoiceImage;
                        if (model.Status == 4 || model.Status == 15)
                        {
                            tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                            vm_OrderList output = Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                            orderHistory.CommentsReschedule = output.InstallDate.ToString();
                        }
                        await context.SaveChangesAsync();
                        if (model.Status != 3)
                        {
                            order.Status = model.Status;
                        }
                        if (model.Status == 9)
                        {
                            order.InvoiceImages = model.InvoiceImage;
                        }
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        return 2;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<int> AddNewHistory(vm_Order model)
        {
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[cls_Defaults.Session_UserId] != null)
            {
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            }
            int UserId = model.UserId;
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[cls_Defaults.Session_UserGroupId] != null)
            {
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            }
            else
            {
                _ = model.UserGroupId;
            }
            if (HttpContext.Current.Session != null && HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId] != null)
            {
                Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            }
            else
            {
                _ = model.GroupTypeId;
            }
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 1,
                            Comments = model.Comments,
                            UserId = UserId,
                            DriverId = model.DriverId
                        };
                        if (model.Status == 4 || model.Status == 15)
                        {
                            tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                            Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                            history.CommentsReschedule = model.InstallDate;
                        }
                        context.tblOrderHistories.Add(history);
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<int> Reschedule(vm_OrderSchedule model)
        {
            bool multple_dates = model.InstallDate.IndexOf(',') > -1;
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            if (model.LabourId2 == null)
            {
                model.LabourId2 = new List<int>();
            }
            if (model.LabourId == null)
            {
                model.LabourId = new List<int>();
                foreach (int LabourId in model.LabourId2)
                {
                    model.LabourId.Add(LabourId);
                }
            }
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder order = await context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                IQueryable<OrdersAssigned> assigned = context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)model.OrderId);
                int i = 0;
                foreach (OrdersAssigned item5 in assigned)
                {
                    item5.LabourId = model.LabourId[i];
                    i++;
                }
                if (order == null)
                {
                    return -1;
                }
                if (multple_dates)
                {
                    model.InstallDate = model.InstallDate.Split(',')[0].ToString();
                }
                order.Status = 4;
                order.LabourId = model.LabourId[0];
                order.DriverId = model.DriverId;
                order.PreferDate = model.PreferDate;
                order.PrefferHr = model.PrefferHr;
                order.PrefferMeridian = model.PrefferMeridian;
                order.InstallDate = DateTime.ParseExact(model.InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                order.PrefferTime = 0;
                context.Entry(order).State = EntityState.Modified;
                await context.SaveChangesAsync();
                try
                {
                    string tssl = "";
                    if (model.TimeSlot != null && model.TimeSlot.Length > 0)
                    {
                        string[] items2 = model.TimeSlot.Split('-');
                        string a2 = items2[0];
                        string b2 = items2[1];
                        tssl = a2.ToString() + ":00-" + b2.ToString() + ":00";
                    }
                    foreach (int item7 in model.LabourId)
                    {
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 4,
                            Comments = model.Comments,
                            UserId = UserId,
                            LabourId = item7,
                            DriverId = model.DriverId
                        };
                        history.CommentsReschedule = model.InstallDate.ToString().Substring(0, 9) + " " + tssl;
                        context.tblOrderHistories.Add(history);
                    }
                    tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                    Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                    string driverName = GetLabourorDriveNamebyId(model.DriverId.Value);
                    foreach (int item8 in model.LabourId)
                    {
                        string labourName = GetLabourorDriveNamebyId(item8);
                        tblOrderHistory Ahistory = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 18,
                            Comments = labourName,
                            UserId = UserId,
                            LabourId = item8,
                            DriverId = model.DriverId
                        };
                        context.tblOrderHistories.Add(Ahistory);
                    }
                    tblOrderHistory AAhistory = new tblOrderHistory
                    {
                        OrderId = model.OrderId,
                        ActivityDate = DateTime.Now,
                        Status = 17,
                        Comments = driverName,
                        UserId = UserId,
                        DriverId = model.DriverId
                    };
                    context.tblOrderHistories.Add(AAhistory);
                    context.SaveChanges();
                    if (order.Status == 4 || order.Status == 15)
                    {
                        int userGroupTypeId2 = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
                        if (model.TimeSlot != null && model.TimeSlot.Length > 0)
                        {
                            string[] items = model.TimeSlot.Split('-');
                            string a = items[0];
                            string b = items[1];
                            tblProviderTimeSlot ordertimeslot = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)model.OrderId).FirstOrDefault();
                            if (ordertimeslot == null)
                            {
                                foreach (int item4 in model.LabourId)
                                {
                                    tblProviderTimeSlot tblobjtimeslot = new tblProviderTimeSlot();
                                    tblobjtimeslot.InstallDate = Convert.ToDateTime(model.InstallDate);
                                    if (multple_dates)
                                    {
                                        LabourCapacity labourCapacity3 = (from x in _context.LabourCapacities
                                                                          where x.LabourId == (int?)item4 && x.OrderId == (int?)model.OrderId && x.InstallDate == order.InstallDate
                                                                          orderby x.Id descending
                                                                          select x).FirstOrDefault();
                                        decimal? capcityPercentage = labourCapacity3.CapcityPercentage;
                                        decimal num = 100;
                                        if ((capcityPercentage.GetValueOrDefault() == num) & capcityPercentage.HasValue)
                                        {
                                            tblobjtimeslot.InstallDate = Convert.ToDateTime(model.InstallDate.Split(',')[1]);
                                        }
                                    }
                                    tblobjtimeslot.OrderId = model.OrderId;
                                    tblobjtimeslot.LabourId = item4;
                                    tblobjtimeslot.ServiceProviderId = userGroupTypeId2;
                                    tblobjtimeslot.StartHour = Convert.ToInt32(a);
                                    tblobjtimeslot.EndHour = Convert.ToInt32(b);
                                    tblobjtimeslot.TotalConsumedHour = Convert.ToInt32(b) - Convert.ToInt32(a);
                                    tblobjtimeslot.Updatedate = DateTime.UtcNow;
                                    _context.tblProviderTimeSlots.Add(tblobjtimeslot);
                                }
                                _context.SaveChanges();
                            }
                            else
                            {
                                foreach (int item3 in model.LabourId)
                                {
                                    ordertimeslot.InstallDate = Convert.ToDateTime(model.InstallDate);
                                    if (multple_dates)
                                    {
                                        LabourCapacity labourCapacity4 = (from x in _context.LabourCapacities
                                                                          where x.LabourId == (int?)item3 && x.OrderId == (int?)model.OrderId && x.InstallDate == order.InstallDate
                                                                          orderby x.Id descending
                                                                          select x).FirstOrDefault();
                                        decimal? capcityPercentage = labourCapacity4.CapcityPercentage;
                                        decimal num = 100;
                                        if ((capcityPercentage.GetValueOrDefault() == num) & capcityPercentage.HasValue)
                                        {
                                            ordertimeslot.InstallDate = Convert.ToDateTime(model.InstallDate.Split(',')[1]);
                                        }
                                    }
                                    ordertimeslot.OrderId = model.OrderId;
                                    ordertimeslot.ServiceProviderId = userGroupTypeId2;
                                    ordertimeslot.LabourId = item3;
                                    ordertimeslot.StartHour = Convert.ToInt32(a);
                                    ordertimeslot.EndHour = Convert.ToInt32(b);
                                    ordertimeslot.TotalConsumedHour = Convert.ToInt32(b) - Convert.ToInt32(a);
                                    ordertimeslot.Updatedate = DateTime.UtcNow;
                                    _context.Entry(ordertimeslot).State = EntityState.Modified;
                                }
                                _context.SaveChanges();
                            }
                        }
                        tblTeamCapacityCalculation tblTeamCapacity2 = (from x in _context.tblTeamCapacityCalculations
                                                                       where x.ServiceProviderId == (int?)userGroupTypeId2 && x.OrderId == (int?)model.OrderId
                                                                       orderby x.Id descending
                                                                       select x).FirstOrDefault();
                        tblTeamCapacity2.CurrentCapacity += ((!tblTeamCapacity2.ConsumedCapacity.HasValue) ? new int?(0) : tblTeamCapacity2.ConsumedCapacity);
                        tblTeamCapacity2.ConsumedCapacity = 0;
                        tblTeamCapacity2.ServiceProviderId = userGroupTypeId2;
                        tblTeamCapacity2.Updatedate = DateTime.Now;
                        double curavailabe = (double)tblTeamCapacity2.CurrentCapacity.Value / (double)tblTeamCapacity2.DailyCapacity.Value * 100.0;
                        if (curavailabe < 0.0)
                        {
                            curavailabe = 0.0;
                        }
                        tblTeamCapacity2.CapcityPercentage = 100m - Convert.ToDecimal(curavailabe);
                        _context.Entry(tblTeamCapacity2).State = EntityState.Modified;
                        _context.SaveChanges();
                        tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
                                                             where x.ServiceProviderId == (int?)userGroupTypeId2 && x.InstallDate == order.InstallDate
                                                             orderby x.Id descending
                                                             select x).FirstOrDefault();
                        int calcva = 0;
                        int dailcapacity = 0;
                        List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(userGroupTypeId2));
                        tblSetting wokrhrs = GetEorkinhHrsysettings(Convert.ToInt32(userGroupTypeId2));
                        if (labours.Count > 0 && labours != null && labours.Count > 0)
                        {
                            dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
                        }
                        List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == order.OrderId).ToList();
                        foreach (tblOrderService item6 in OrderServicssse)
                        {
                            tblServiceMapper spassignedservice = GetServicesmap2(Convert.ToInt32(userGroupTypeId2), item6.ServiceId);
                            int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                            calcva += item6.Quantity * spestimat / 60;
                        }
                        if (model2 != null)
                        {
                            int totaloccperctcapcitycurent = Convert.ToInt32(model2.CurrentCapacity);
                            tblTeamCapacityCalculation capcal2 = new tblTeamCapacityCalculation();
                            capcal2.Updatedate = DateTime.Now;
                            capcal2.InstallDate = Convert.ToDateTime(order.InstallDate);
                            capcal2.OrderId = order.OrderId;
                            capcal2.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                            capcal2.DailyCapacity = dailcapacity;
                            capcal2.ConsumedCapacity = calcva;
                            capcal2.CurrentCapacity = totaloccperctcapcitycurent - calcva;
                            double curavailabe3 = (double)capcal2.CurrentCapacity.Value / (double)capcal2.DailyCapacity.Value * 100.0;
                            if (curavailabe3 < 0.0)
                            {
                                curavailabe3 = 0.0;
                            }
                            capcal2.CapcityPercentage = Convert.ToDecimal(curavailabe3);
                            _context.tblTeamCapacityCalculations.Add(capcal2);
                            foreach (int item2 in model.LabourId)
                            {
                                LabourCapacity labourCapacity2 = (from x in _context.LabourCapacities
                                                                  where x.LabourId == (int?)item2 && x.OrderId == (int?)model.OrderId && x.InstallDate == order.InstallDate
                                                                  orderby x.Id descending
                                                                  select x).FirstOrDefault();
                                if (labourCapacity2 == null)
                                {
                                    labourCapacity2 = new LabourCapacity();
                                }
                                labourCapacity2.Updatedate = DateTime.Now;
                                labourCapacity2.LabourId = item2;
                                labourCapacity2.OrderId = order.OrderId;
                                labourCapacity2.DailyCapacity = Convert.ToInt32(wokrhrs.KeyValue);
                                labourCapacity2.CurrentCapacity = ((!labourCapacity2.CurrentCapacity.HasValue) ? new int?(Convert.ToInt32(wokrhrs.KeyValue) - calcva) : (labourCapacity2.CurrentCapacity + Convert.ToInt32(wokrhrs.KeyValue) - calcva));
                                if (labourCapacity2.CurrentCapacity >= labourCapacity2.DailyCapacity)
                                {
                                    labourCapacity2.CurrentCapacity = 0;
                                }
                                labourCapacity2.ConsumedCapacity = labourCapacity2.DailyCapacity - labourCapacity2.CurrentCapacity;
                                labourCapacity2.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                                labourCapacity2.InstallDate = Convert.ToDateTime(order.InstallDate);
                                double aval = (double)labourCapacity2.ConsumedCapacity.Value / (double)labourCapacity2.DailyCapacity.Value * 100.0;
                                if (aval < 0.0)
                                {
                                    aval = 0.0;
                                }
                                labourCapacity2.CapcityPercentage = Convert.ToDecimal(aval);
                                if (labourCapacity2.Id == 0)
                                {
                                    _context.LabourCapacities.Add(labourCapacity2);
                                }
                            }
                            _context.SaveChanges();
                        }
                        else
                        {
                            tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                            capcal.Updatedate = DateTime.Now;
                            capcal.InstallDate = Convert.ToDateTime(order.InstallDate);
                            capcal.OrderId = order.OrderId;
                            capcal.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                            capcal.DailyCapacity = dailcapacity;
                            capcal.ConsumedCapacity = calcva;
                            capcal.CurrentCapacity = dailcapacity - calcva;
                            double curavailabe4 = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                            if (curavailabe4 < 0.0)
                            {
                                curavailabe4 = 0.0;
                            }
                            capcal.CapcityPercentage = Convert.ToDecimal(curavailabe4);
                            _context.tblTeamCapacityCalculations.Add(capcal);
                            foreach (int item in model.LabourId)
                            {
                                LabourCapacity labourCapacity = (from x in _context.LabourCapacities
                                                                 where x.LabourId == (int?)item && x.OrderId == (int?)model.OrderId && x.InstallDate == order.InstallDate
                                                                 orderby x.Id descending
                                                                 select x).FirstOrDefault();
                                if (labourCapacity == null)
                                {
                                    labourCapacity = new LabourCapacity();
                                }
                                labourCapacity.Updatedate = DateTime.Now;
                                labourCapacity.LabourId = item;
                                labourCapacity.OrderId = order.OrderId;
                                labourCapacity.DailyCapacity = Convert.ToInt32(wokrhrs.KeyValue);
                                labourCapacity.ConsumedCapacity = calcva;
                                labourCapacity.CurrentCapacity = Convert.ToInt32(wokrhrs.KeyValue) - calcva;
                                labourCapacity.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                                labourCapacity.InstallDate = Convert.ToDateTime(order.InstallDate);
                                double curavailabe2 = (double)labourCapacity.ConsumedCapacity.Value / (double)labourCapacity.DailyCapacity.Value * 100.0;
                                if (curavailabe2 < 0.0)
                                {
                                    curavailabe2 = 0.0;
                                }
                                labourCapacity.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                                if (labourCapacity.Id == 0)
                                {
                                    _context.LabourCapacities.Add(labourCapacity);
                                }
                            }
                            _context.SaveChanges();
                        }
                    }
                    return 1;
                }
                catch (Exception)
                {
                }
            }
            return 0;
        }

        public static async Task managecapacity(int OrderId)
        {
            Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            db_User objUser = new db_User();
            db_Settings objSettings = new db_Settings();
            AlmaneaDbEntities _context = new AlmaneaDbEntities();
            vm_Order order = await objSettings.GetOrderById(OrderId);
            if (order.Status != 4)
            {
                return;
            }
            int calcva = 0;
            new List<int>();
            List<int?> splist = objSettings.GetSpOrderDistributed(OrderId);
            if (splist.Count <= 0)
            {
                return;
            }
            foreach (int? itemspid in splist)
            {
                int dailcapacity = 0;
                List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(itemspid));
                tblSetting wokrhrs = objSettings.GetEorkinhHrsysettings(Convert.ToInt32(itemspid));
                if (labours.Count > 0 && labours != null && labours.Count > 0)
                {
                    dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
                }
                List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == OrderId).ToList();
                foreach (tblOrderService item in OrderServicssse)
                {
                    tblServiceMapper spassignedservice = objSettings.GetServicesmap2(Convert.ToInt32(itemspid), item.ServiceId);
                    int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                    calcva += item.Quantity * spestimat / 60;
                }
                int idd = Convert.ToInt32(itemspid);
                DateTime orderdatatt = Convert.ToDateTime(order.InstallDate);
                tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
                                                     where x.ServiceProviderId == (int?)idd && x.InstallDate == orderdatatt && x.OrderId == (int?)OrderId
                                                     orderby x.Id descending
                                                     select x).FirstOrDefault();
                if (model2 != null)
                {
                    Convert.ToInt32(model2.CurrentCapacity);
                    model2.Updatedate = DateTime.Now;
                    model2.InstallDate = Convert.ToDateTime(order.InstallDate);
                    model2.OrderId = order.OrderId;
                    model2.ServiceProviderId = Convert.ToInt32(itemspid);
                    model2.DailyCapacity = dailcapacity;
                    model2.ConsumedCapacity = calcva;
                    model2.CurrentCapacity -= calcva;
                    double curavailabe2 = (double)model2.CurrentCapacity.Value / (double)model2.DailyCapacity.Value * 100.0;
                    if (curavailabe2 < 0.0)
                    {
                        curavailabe2 = 0.0;
                    }
                    model2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                    _context.Entry(model2).State = EntityState.Modified;
                    _context.SaveChanges();
                }
                else
                {
                    tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                    capcal.Updatedate = DateTime.Now;
                    capcal.InstallDate = Convert.ToDateTime(order.InstallDate);
                    capcal.OrderId = order.OrderId;
                    capcal.ServiceProviderId = Convert.ToInt32(itemspid);
                    capcal.DailyCapacity = dailcapacity;
                    capcal.ConsumedCapacity = calcva;
                    capcal.CurrentCapacity = dailcapacity - calcva;
                    double curavailabe = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                    if (curavailabe < 0.0)
                    {
                        curavailabe = 0.0;
                    }
                    capcal.CapcityPercentage = Convert.ToDecimal(curavailabe);
                    _context.tblTeamCapacityCalculations.Add(capcal);
                    _context.SaveChanges();
                }
                calcva = 0;
            }
        }

        public async Task<int> AppointmentReschedule(vm_OrderSchedule model)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder reschedule = context.tblOrders.Where((tblOrder k) => k.OrderId == model.OrderId).FirstOrDefault();
                Mapper.Map<tblOrder, vm_OrderList>(reschedule);
                tblOrder order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                if (order == null)
                {
                    return -1;
                }
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        string tssl = "";
                        if (model.TimeSlot != null && model.TimeSlot.Length > 0)
                        {
                            string[] items2 = model.TimeSlot.Split('-');
                            string a2 = items2[0];
                            string b2 = items2[1];
                            tssl = a2.ToString() + ":00-" + b2.ToString() + ":00";
                        }
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = 15,
                            Comments = model.Comments,
                            UserId = UserId,
                            CommentsReschedule = model.InstallDate.ToString().Substring(0, 10) + " " + tssl,
                            LabourId = model.LabourId[0],
                            DriverId = model.DriverId
                        };
                        context.tblOrderHistories.Add(history);
                        await context.SaveChangesAsync();
                        order.Status = 15;
                        order.PreferDate = model.PreferDate;
                        order.PrefferHr = model.PrefferHr;
                        order.PrefferMeridian = model.PrefferMeridian;
                        order.InstallDate = DateTime.ParseExact(model.InstallDate, "dd/MM/yyyy", CultureInfo.InvariantCulture);
                        order.PrefferTime = 0;
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        int userGroupTypeId2 = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
                        if (order.Status == 15)
                        {
                            if (model.TimeSlot != null && model.TimeSlot.Length > 0)
                            {
                                string[] items = model.TimeSlot.Split('-');
                                string a = items[0];
                                string b = items[1];
                                tblProviderTimeSlot ordertimeslot = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)model.OrderId).FirstOrDefault();
                                if (ordertimeslot == null)
                                {
                                    tblProviderTimeSlot tblobjtimeslot = new tblProviderTimeSlot();
                                    tblobjtimeslot.OrderId = model.OrderId;
                                    tblobjtimeslot.LabourId = model.LabourId[0];
                                    tblobjtimeslot.ServiceProviderId = userGroupTypeId2;
                                    tblobjtimeslot.InstallDate = Convert.ToDateTime(model.InstallDate);
                                    tblobjtimeslot.StartHour = Convert.ToInt32(a);
                                    tblobjtimeslot.EndHour = Convert.ToInt32(b);
                                    tblobjtimeslot.TotalConsumedHour = Convert.ToInt32(b) - Convert.ToInt32(a);
                                    tblobjtimeslot.Updatedate = DateTime.UtcNow;
                                    _context.tblProviderTimeSlots.Add(tblobjtimeslot);
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    ordertimeslot.OrderId = model.OrderId;
                                    ordertimeslot.ServiceProviderId = userGroupTypeId2;
                                    ordertimeslot.InstallDate = Convert.ToDateTime(model.InstallDate);
                                    ordertimeslot.StartHour = Convert.ToInt32(a);
                                    ordertimeslot.EndHour = Convert.ToInt32(b);
                                    ordertimeslot.TotalConsumedHour = Convert.ToInt32(b) - Convert.ToInt32(a);
                                    ordertimeslot.Updatedate = DateTime.UtcNow;
                                    _context.Entry(ordertimeslot).State = EntityState.Modified;
                                    _context.SaveChanges();
                                }
                            }
                            tblTeamCapacityCalculation tblTeamCapacity2 = (from x in _context.tblTeamCapacityCalculations
                                                                           where x.ServiceProviderId == (int?)userGroupTypeId2 && x.OrderId == (int?)model.OrderId
                                                                           orderby x.Id descending
                                                                           select x).FirstOrDefault();
                            tblTeamCapacity2.CurrentCapacity += ((!tblTeamCapacity2.ConsumedCapacity.HasValue) ? new int?(0) : tblTeamCapacity2.ConsumedCapacity);
                            tblTeamCapacity2.ConsumedCapacity = 0;
                            tblTeamCapacity2.ServiceProviderId = userGroupTypeId2;
                            tblTeamCapacity2.Updatedate = DateTime.Now;
                            double curavailabe = (double)tblTeamCapacity2.CurrentCapacity.Value / (double)tblTeamCapacity2.DailyCapacity.Value * 100.0;
                            if (curavailabe < 0.0)
                            {
                                curavailabe = 0.0;
                            }
                            tblTeamCapacity2.CapcityPercentage = Convert.ToDecimal(curavailabe);
                            _context.Entry(tblTeamCapacity2).State = EntityState.Modified;
                            _context.SaveChanges();
                            tblTeamCapacityCalculation model2 = (from x in _context.tblTeamCapacityCalculations
                                                                 where x.ServiceProviderId == (int?)userGroupTypeId2 && x.InstallDate == order.InstallDate
                                                                 orderby x.Id descending
                                                                 select x).FirstOrDefault();
                            int calcva = 0;
                            int dailcapacity = 0;
                            List<tblAdminUser> labours = objUser.GetLaboursDopr(Convert.ToInt32(userGroupTypeId2));
                            tblSetting wokrhrs = GetEorkinhHrsysettings(Convert.ToInt32(userGroupTypeId2));
                            if (labours.Count > 0 && labours != null && labours.Count > 0)
                            {
                                dailcapacity = Convert.ToInt32(labours.Count * Convert.ToInt32(wokrhrs.KeyValue));
                            }
                            List<tblOrderService> OrderServicssse = _context.tblOrderServices.Where((tblOrderService x) => x.OrderId == order.OrderId).ToList();
                            foreach (tblOrderService item in OrderServicssse)
                            {
                                tblServiceMapper spassignedservice = GetServicesmap2(Convert.ToInt32(userGroupTypeId2), item.ServiceId);
                                int spestimat = Convert.ToInt32(spassignedservice.Estimated);
                                calcva += item.Quantity * spestimat / 60;
                            }
                            if (model2 != null)
                            {
                                int totaloccperctcapcitycurent = Convert.ToInt32(model2.CurrentCapacity);
                                tblTeamCapacityCalculation capcal2 = new tblTeamCapacityCalculation();
                                capcal2.Updatedate = DateTime.Now;
                                capcal2.InstallDate = Convert.ToDateTime(order.InstallDate);
                                capcal2.OrderId = order.OrderId;
                                capcal2.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                                capcal2.DailyCapacity = dailcapacity;
                                capcal2.ConsumedCapacity = calcva;
                                capcal2.CurrentCapacity = totaloccperctcapcitycurent - calcva;
                                double curavailabe2 = (double)capcal2.CurrentCapacity.Value / (double)capcal2.DailyCapacity.Value * 100.0;
                                if (curavailabe2 < 0.0)
                                {
                                    curavailabe2 = 0.0;
                                }
                                capcal2.CapcityPercentage = Convert.ToDecimal(curavailabe2);
                                _context.tblTeamCapacityCalculations.Add(capcal2);
                                _context.SaveChanges();
                            }
                            else
                            {
                                tblTeamCapacityCalculation capcal = new tblTeamCapacityCalculation();
                                capcal.Updatedate = DateTime.Now;
                                capcal.InstallDate = Convert.ToDateTime(order.InstallDate);
                                capcal.OrderId = order.OrderId;
                                capcal.ServiceProviderId = Convert.ToInt32(userGroupTypeId2);
                                capcal.DailyCapacity = dailcapacity;
                                capcal.ConsumedCapacity = calcva;
                                capcal.CurrentCapacity = dailcapacity - calcva;
                                double curavailabe3 = (double)capcal.CurrentCapacity.Value / (double)capcal.DailyCapacity.Value * 100.0;
                                if (curavailabe3 < 0.0)
                                {
                                    curavailabe3 = 0.0;
                                }
                                capcal.CapcityPercentage = Convert.ToDecimal(curavailabe3);
                                _context.tblTeamCapacityCalculations.Add(capcal);
                                _context.SaveChanges();
                            }
                        }
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<int> FinishOrders(List<vm_AdditionalService> adds, vm_OrderStatus model, decimal Vat)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                tblOrder order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                if (order == null)
                {
                    return -1;
                }
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {

                    try
                    {
                        tblOrderHistory history = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = model.Status,
                            Comments = model.Comments,
                            UserId = UserId,
                            FileAttachment = model.InvoiceImage,
                            LabourId = model.LabourId[0],
                            DriverId = model.DriverId
                        };
                        context.tblOrderHistories.Add(history);
                        await context.SaveChangesAsync();
                        decimal Price = default(decimal);
                        foreach (vm_AdditionalService item in adds)
                        {
                            tblOrderAdditionalService addService = new tblOrderAdditionalService
                            {
                                OrderId = model.OrderId,
                                AdditionalWorkId = item.ServiceId,
                                Quantity = item.Quantity,
                                Price = item.Price,
                                SpareParts = item.SpareParts,
                                AddedOn = DateTime.Now,
                                UserId = UserId,
                                Status = true
                            };
                            context.tblOrderAdditionalServices.Add(addService);
                            Price += (decimal)addService.Quantity * addService.Price;
                            await context.SaveChangesAsync();
                        }
                        order.CustomerSignOff = model.CustomerSignOff;
                        order.InvoiceImages = model.InvoiceImage;
                        order.Status = model.Status;
                        decimal tempVat = Price / 1.15m * (Vat / 100m);
                        decimal totalSub = Price - tempVat;
                        order.AdditionalTotal = totalSub;
                        order.AdditionalVat = tempVat;
                        order.TotalAmount += totalSub + tempVat;
                        await context.SaveChangesAsync();
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public async Task<List<vm_AdditionalService>> GetAdditional(int OrderId)
        {
            return await (from x in _context.tblOrderAdditionalServices
                          where x.OrderId == OrderId
                          select x into s
                          select new vm_AdditionalService
                          {
                              ServiceId = s.AdditionalWorkId,
                              Id = s.AdditionalServiceId,
                              OrderId = s.OrderId,
                              Price = s.Price,
                              Quantity = s.Quantity,
                              ServiceName = s.tblAdditionalWork.AdditionalWorkNameEN,
                              SpareParts = s.SpareParts
                          }).ToListAsync();
        }

        public async Task<List<vm_AdditionalService>> GetAdditionalAR(int OrderId)
        {
            return await (from x in _context.tblOrderAdditionalServices
                          where x.OrderId == OrderId
                          select x into s
                          select new vm_AdditionalService
                          {
                              ServiceId = s.AdditionalWorkId,
                              Id = s.AdditionalServiceId,
                              OrderId = s.OrderId,
                              Price = s.Price,
                              Quantity = s.Quantity,
                              ServiceName = s.tblAdditionalWork.AdditionalWorkNameAR,
                              SpareParts = s.SpareParts
                          }).ToListAsync();
        }

        public async Task<decimal> GetAdditional(List<int> OrderId)
        {
            decimal total = default(decimal);
            foreach (tblOrderAdditionalService item in await _context.tblOrderAdditionalServices.Where((tblOrderAdditionalService x) => OrderId.Any((int y) => y == x.OrderId)).ToListAsync())
            {
                total += item.Price;
            }
            return total;
        }

        public async Task<List<vm_OrderHistoryList>> GetHistory(int OrderId)
        {
            return Mapper.Map<List<tblOrderHistory>, List<vm_OrderHistoryList>>(await (from x in _context.tblOrderHistories
                                                                                       where x.OrderId == OrderId
                                                                                       orderby x.ActivityDate
                                                                                       select x).ToListAsync());
        }

        public async Task<List<vm_OrderHistoryList>> GetHistoryforSp(int OrderId)
        {
            return Mapper.Map<List<tblOrderHistory>, List<vm_OrderHistoryList>>(await (from x in _context.tblOrderHistories
                                                                                       where x.OrderId == OrderId && x.Status != 3
                                                                                       orderby x.ActivityDate descending
                                                                                       select x).ToListAsync());
        }

        public async Task<string> GetOrderReservedBy(int OrderId)
        {
            tblOrderHistory model = await _context.tblOrderHistories.FirstOrDefaultAsync((tblOrderHistory x) => x.OrderId == OrderId && x.Status == 2);
            if (model != null)
            {
                return model.tblAdminUser.FirstName + " " + model.tblAdminUser.LastName;
            }
            return "";
        }

        public async Task<tblOrderHistory> GetOrderReservedHistory(int OrderId)
        {
            return await _context.tblOrderHistories.FirstOrDefaultAsync((tblOrderHistory x) => x.OrderId == OrderId && x.Status == 2);
        }

        public string GetReservedBy(int UserId)
        {
            string text = "";
            tblAdminUser model = _context.tblAdminUsers.FirstOrDefault((tblAdminUser x) => x.UserId == UserId);
            if (model != null)
            {
                return text = model.FirstName + " " + model.LastName;
            }
            return "";
        }

        public async Task<Dictionary<int, string>> ExpiredOrders(int duration)
        {
            Dictionary<int, string> output = new Dictionary<int, string>();
            foreach (tblOrder item in await _context.tblOrders.Where((tblOrder x) => x.Status == 1 && x.ReservedProvider == 0 && x.ReportAdmin != (bool?)true).ToListAsync())
            {
                if ((DateTime.Now - item.AddedDate).TotalMinutes > (double)duration)
                {
                    string OrderNo = item.OrderId.ToString();
                    output.Add(item.OrderId, OrderNo);
                }
            }
            return output;
        }

        public async Task UpdateReportAdmin(int Id)
        {
            new List<string>();
            (await _context.tblOrders.Where((tblOrder x) => x.OrderId == Id).FirstOrDefaultAsync()).ReportAdmin = true;
            _context.SaveChanges();
        }

        public async Task UpdateIsEdit(int OrderId, bool status)
        {
            (await _context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == OrderId)).IsOnEdit = status;
            await _context.SaveChangesAsync();
        }

        public async Task<int> ReleaseOrder(vm_OrderStatus model)
        {
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            using (AlmaneaDbEntities context = new AlmaneaDbEntities())
            {
                using (DbContextTransaction dbTrnx = context.Database.BeginTransaction())
                {
                    try
                    {
                        tblOrder order = await context.tblOrders.SingleOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
                        List<tblOrderHistory> history = await context.tblOrderHistories.Where((tblOrderHistory x) => x.OrderId == model.OrderId).ToListAsync();
                        List<tblProviderTimeSlot> timeslots = await context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)model.OrderId && x.InstallDate == order.InstallDate).ToListAsync();
                        List<tblOrderReleas> inputs = new List<tblOrderReleas>();
                        foreach (tblOrderHistory item in history)
                        {
                            inputs.Add(new tblOrderReleas
                            {
                                ActivityDate = item.ActivityDate,
                                Comments = item.Comments,
                                OrderId = model.OrderId,
                                Status = item.Status,
                                UserId = item.UserId,
                                FileAttachment = item.FileAttachment
                            });
                        }
                        inputs.Add(new tblOrderReleas
                        {
                            ActivityDate = DateTime.Now,
                            Comments = model.Comments,
                            OrderId = model.OrderId,
                            Status = 13,
                            UserId = UserId
                        });
                        context.tblOrderReleases.AddRange(inputs);
                        context.SaveChanges();
                        context.tblProviderTimeSlots.RemoveRange(timeslots);
                        context.SaveChanges();
                        tblResetOrder release = await context.tblResetOrders.FirstOrDefaultAsync((tblResetOrder x) => x.OrderId == model.OrderId);
                        order.Status = 20;
                        order.ReservedProvider = 0;
                        order.InstallDate = release.InstallDate;
                        order.PrefferTime = release.PrefferTime.Value;
                        order.PreferDate = release.PreferDate.Value;
                        order.PrefferHr = release.PrefferHr;
                        order.PrefferMeridian = release.PrefferMeridian;
                        tblOrderHistory history2 = new tblOrderHistory
                        {
                            OrderId = model.OrderId,
                            ActivityDate = DateTime.Now,
                            Status = model.Status,
                            Comments = model.Comments,
                            UserId = UserId,
                            FileAttachment = model.FileName
                        };
                        context.tblOrderHistories.Add(history2);
                        context.SaveChanges();
                        dbTrnx.Commit();
                        return 1;
                    }
                    catch (Exception)
                    {
                        dbTrnx.Rollback();
                    }
                }
            }
            return 0;
        }

        public tblSM GetSMS2(string Key)
        {
            return _context.tblSMS.SingleOrDefault((tblSM x) => x.KeyName.ToUpper().Equals(Key));
        }

        public async Task<tblSM> GetSMS(string Key)
        {
            return await _context.tblSMS.SingleOrDefaultAsync((tblSM x) => x.KeyName.ToUpper().Equals(Key));
        }

        public async Task<tblPushNotification> GetPushNotification(string Key)
        {
            return await _context.tblPushNotifications.SingleOrDefaultAsync((tblPushNotification x) => x.KeyName.ToUpper().Equals(Key));
        }

        public async Task<List<vm_Invoice_Services>> ReportServices(int Id, decimal Vat)
        {
            List<tblOrderService> services = await (from x in _context.tblOrderServices.Include((tblOrderService x) => x.tblOrder.tblOrderAdditionalWorks)
                                                    where x.OrderId == Id && x.IsActive == true
                                                    select x).ToListAsync();
            List<vm_Invoice_Services> model = new List<vm_Invoice_Services>();
            tblOrderService i;
            foreach (tblOrderService item2 in services)
            {
                i = item2;
                vm_Invoice_Services item = new vm_Invoice_Services
                {
                    Title = (isEnglish ? i.tblService.ServiceNameEN : i.tblService.ServiceNameAR),
                    UnitPrice = i.tblService.UnitPrice,
                    Quantity = i.Quantity
                };
                try
                {
                    List<vm_AdditionalService> add = (from x in _context.tblOrderAdditionalServices
                                                      where x.OrderId == i.OrderId
                                                      select x into s
                                                      select new vm_AdditionalService
                                                      {
                                                          ServiceId = s.AdditionalWorkId,
                                                          Id = s.AdditionalServiceId,
                                                          OrderId = s.OrderId,
                                                          Price = s.Price,
                                                          Quantity = s.Quantity,
                                                          ServiceName = s.tblAdditionalWork.AdditionalWorkNameEN,
                                                          SpareParts = s.SpareParts,
                                                          AdditionalWorkPrice = s.tblAdditionalWork.Price
                                                      }).ToList();
                    decimal sumOf = default(decimal);
                    foreach (vm_AdditionalService Add1 in add)
                    {
                        item.AdditionalWork = Add1.AdditionalWorkPrice;
                        sumOf += (decimal)Add1.Quantity * Add1.Price;
                    }
                    item.Total = (decimal)item.Quantity * item.UnitPrice;
                    item.Vat = item.Total * (Vat / 100m);
                    item.AdditonalAmount = decimal.Parse(sumOf.ToString("0.00"));
                    item.AdditionalVat = item.AdditonalAmount * (Vat / 100m);
                    model.Add(item);
                }
                catch (Exception)
                {
                }
            }
            return model;
        }

        public vm_MenuCount GetNewOrderCount(int UserGroupTypeId, int? UserGroupId, int AccountTypeId, int UserId)
        {
            try
            {
                List<int> GroupUsers = new List<int>();
                vm_MenuCount menu = new vm_MenuCount();
                int objResult = 0;
                if (UserGroupTypeId == 2 && AccountTypeId == 17)
                {
                    GroupUsers = (from u in _context.tblAdminUsers
                                  where u.AddedBy == UserId
                                  select u.UserId).ToList();
                    objResult = (from o in _context.tblOrders
                                 where o.Status == 1 && GroupUsers.Contains(o.AddedBy)
                                 select o.OrderId).Count();
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 20)
                {
                    IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == UserGroupId).ToList()
                                                        select x.UserId;
                    List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == UserGroupId).ToList();
                    IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                             select x.OrderId;
                    int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
                    DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                    objResult = (from o in _context.tblOrders
                                 where slectedSPorderlistid.Contains(o.OrderId) && o.AddedDate <= now5Min && o.Status == 1
                                 select o.OrderId).Count();
                }
                else if (UserGroupTypeId == 1 && AccountTypeId == 6)
                {
                    int ProviderAdminId = GetSupplierOrProviderAdminId(UserId);
                    int SupplierAdmin = GetSupplierOrProviderAdminId(ProviderAdminId);
                    GroupUsers = (from u in _context.tblAdminUsers
                                  where u.AddedBy == SupplierAdmin
                                  select u.UserId).ToList();
                    objResult = (from o in _context.tblOrders
                                 where o.Status == 1 && GroupUsers.Contains(o.AddedBy)
                                 select o.OrderId).Count();
                }
                else
                {
                    objResult = 0;
                }
                menu.AllOrders = objResult;
                return menu;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public vm_MenuCount GetNewServiceCount(int UserGroupTypeId, int? UserGroupId, int AccountTypeId)
        {
            try
            {
                List<int> GroupUsers = new List<int>();
                vm_MenuCount menu = new vm_MenuCount();
                int objResult = 0;
                objResult = (menu.NewService = ((UserGroupTypeId == 1 && AccountTypeId == 20) ? (from s in _context.tblServiceMappers
                                                                                                 where s.ServiceProviderId == UserGroupId && s.ServiceAcceptStatus == false
                                                                                                 select s.ServiceId).Count() : 0));
                return menu;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public vm_MenuCount GetNewComplainCount(int UserGroupTypeId, int? UserGroupId, int AccountTypeId, int UserId)
        {
            try
            {
                List<int> GroupUsers = new List<int>();
                vm_MenuCount menu = new vm_MenuCount();
                int objResult = 0;
                if (UserGroupTypeId == 1 && (AccountTypeId == 20 || AccountTypeId == 6))
                {
                    int SupplierAdminid = GetSupplierOrProviderAdminId(UserId);
                    List<int> UserList = (from u in _context.tblAdminUsers
                                          where u.AddedBy == SupplierAdminid
                                          select u.UserId).ToList();
                    objResult = (from o in _context.tblOrderComplains
                                 where UserList.Contains((int)o.AddedBy) && o.StatusId == 1 && (int?)o.tblOrder.ReservedProvider == UserGroupId
                                 select o.ComplainId).Count();
                }
                else
                {
                    objResult = 0;
                }
                menu.NewComplain = objResult;
                return menu;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public vm_MenuCount GetMenuCount(int TypeId, int? GroupId)
        {
            try
            {
                ObjectResult<Sp_OrderCount_Result> objResult = _context.Sp_OrderCount(TypeId, GroupId);
                List<vm_MenuCount> clients = new List<vm_MenuCount>();
                foreach (Sp_OrderCount_Result item in objResult)
                {
                    clients.Add(Mapper.Map<Sp_OrderCount_Result, vm_MenuCount>(item));
                }
                if (clients.Count > 0)
                {
                    return clients[0];
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public vm_SalesMenuCount GetMenuCountForSale(int TypeId, string GroupId, int? supplierId, DateTime fromdate, DateTime todate, int companyId)
        {
            int objResult = _context.Sp_OrderCountforSale(TypeId, GroupId, supplierId, fromdate, todate, companyId);
            List<vm_SalesMenuCount> clients = new List<vm_SalesMenuCount>();
            if (clients.Count > 0)
            {
                return clients[0];
            }
            return null;
        }

        public vm_Dashboard GetMenuCountForDashBoard()
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            dashboard.OpneComplain = (from o in _context.tblOrderComplains
                                      where o.StatusId == 8
                                      select o.ComplainId).Count();
            dashboard.CloseComplain = (from o in _context.tblOrderComplains
                                       where o.StatusId == 5
                                       select o.ComplainId).Count();
            dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                    where o.InstallDate.Value.Month == DateTime.Today.Month && o.Status == 9
                                                    select o.OrderId).Count();
            List<vm_SalesMenuCount> clients = new List<vm_SalesMenuCount>();
            return dashboard;
        }

        public vm_Dashboard GetComplaints(string StartDate, string EndDate)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.OpneComplain = (from o in _context.tblOrderComplains
                                          where o.StatusId == 2 || o.StatusId == 6
                                          where DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate)
                                          select o.ComplainId).Count();
                List<int> CloseComplainOrderIdList = (from o in _context.tblOrderComplains
                                                      where o.StatusId == 6 && DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate)
                                                      select o.ComplainId).ToList();
                dashboard.CloseComplain = CloseComplainOrderIdList.Count();
                if (CloseComplainOrderIdList.Count > 0)
                {
                    List<tblComplainHistory> TimeList2 = (from o in _context.tblComplainHistories
                                                          where CloseComplainOrderIdList.Contains((int)o.ComplainId)
                                                          orderby o.UpdateOn descending
                                                          select o).ToList();
                    tblComplainHistory FirstDate2 = TimeList2.OrderByDescending((tblComplainHistory o) => o.UpdateOn).First();
                    tblComplainHistory LastDate2 = TimeList2.OrderByDescending((tblComplainHistory o) => o.UpdateOn).Last();
                    DateTime date2 = FirstDate2.UpdateOn;
                    DateTime date4 = LastDate2.UpdateOn;
                    dashboard.AverageClosingTime = Convert.ToInt32(new TimeSpan((date2 - date4).Ticks / CloseComplainOrderIdList.Count()).TotalHours);
                }
                else
                {
                    dashboard.AverageClosingTime = 0;
                }
            }
            else
            {
                dashboard.OpneComplain = (from o in _context.tblOrderComplains
                                          where o.StatusId == 2 || o.StatusId == 6
                                          select o.ComplainId).Count();
                List<int> CloseComplain = (from o in _context.tblOrderComplains
                                           where o.StatusId == 6
                                           select o.ComplainId).ToList();
                dashboard.CloseComplain = CloseComplain.Count();
                List<tblComplainHistory> TimeList = (from o in _context.tblComplainHistories
                                                     where CloseComplain.Contains((int)o.ComplainId)
                                                     orderby o.UpdateOn descending
                                                     select o).ToList();
                tblComplainHistory FirstDate = TimeList.OrderByDescending((tblComplainHistory o) => o.UpdateOn).First();
                tblComplainHistory LastDate = TimeList.OrderByDescending((tblComplainHistory o) => o.UpdateOn).Last();
                DateTime date1 = FirstDate.UpdateOn;
                DateTime date3 = LastDate.UpdateOn;
                dashboard.AverageClosingTime = Convert.ToInt32(new TimeSpan((date1 - date3).Ticks / CloseComplain.Count()).TotalHours);
            }
            List<vm_SalesMenuCount> clients = new List<vm_SalesMenuCount>();
            return dashboard;
        }

        public vm_Dashboard GetTodayandTomorrowOrderInstallation()
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            dashboard.TotalTodayOrderInstallation = (from o in _context.tblOrders
                                                     where o.InstallDate == DateTime.Today
                                                     select o.OrderId).Count();
            List<int> TodayOrderIdList = (from o in _context.tblOrders
                                          where o.InstallDate == DateTime.Today
                                          select o.OrderId).ToList();
            if (TodayOrderIdList.Count > 0)
            {
                dashboard.TotalTodayOrderInstallationUnit = (from o in _context.tblOrderServices
                                                             where TodayOrderIdList.Contains(o.OrderId)
                                                             select o.Quantity).Sum();
            }
            else
            {
                dashboard.TotalTodayOrderInstallationUnit = 0;
            }
            DateTime tomorrow = DateTime.Today.AddDays(1.0);
            dashboard.TotalTomorrowOrderInstallation = (from o in _context.tblOrders
                                                        where o.InstallDate == tomorrow
                                                        select o.OrderId).Count();
            List<int> TomorrowOrderIdList = (from o in _context.tblOrders
                                             where o.InstallDate == tomorrow
                                             select o.OrderId).ToList();
            if (TomorrowOrderIdList.Count > 0)
            {
                dashboard.TotalTomorrowOrderInstallationUnit = (from o in _context.tblOrderServices
                                                                where TomorrowOrderIdList.Contains(o.OrderId)
                                                                select o.Quantity).Sum();
            }
            else
            {
                dashboard.TotalTomorrowOrderInstallationUnit = 0;
            }
            List<vm_SalesMenuCount> clients = new List<vm_SalesMenuCount>();
            return dashboard;
        }

        public vm_Dashboard GetCompleteOrderInstallationofMonth(string StartDate, string EndDate)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboarMonthOrderlist> MonthOrderlist = new List<vm_DashboarMonthOrderlist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                        where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.Status == 9
                                                        select o.OrderId).Count();
                var Orderlist2 = (from o in _context.tblOrders
                                  where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.Status == 9
                                  group o by DbFunctions.TruncateTime(o.InstallDate) into o
                                  select new
                                  {
                                      Value = o.Count(),
                                      Day = (DateTime)o.Key
                                  }).ToList();
                foreach (var item2 in Orderlist2)
                {
                    MonthOrderlist.Add(new vm_DashboarMonthOrderlist
                    {
                        Day = Convert.ToDateTime(item2.Day.ToShortDateString()).ToString("dd/MM/yyyy"),
                        Order = item2.Value
                    });
                }
                dashboard.MonthOrderlist = MonthOrderlist;
            }
            else
            {
                dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                        where o.InstallDate.Value.Month == DateTime.Today.Month && o.Status == 9
                                                        select o.OrderId).Count();
                var Orderlist = (from o in _context.tblOrders
                                 where o.InstallDate.Value.Month == DateTime.Today.Month && o.Status == 9
                                 group o by DbFunctions.TruncateTime(o.InstallDate) into o
                                 select new
                                 {
                                     Value = o.Count(),
                                     Day = (DateTime)o.Key
                                 }).ToList();
                foreach (var item in Orderlist)
                {
                    MonthOrderlist.Add(new vm_DashboarMonthOrderlist
                    {
                        Day = Convert.ToDateTime(item.Day.ToShortDateString()).ToString("dd/MM/yyyy"),
                        Order = item.Value
                    });
                }
                dashboard.MonthOrderlist = MonthOrderlist;
            }
            return dashboard;
        }

        public vm_Dashboard GetTotalServiceOrder(string StartDate, string EndDate)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboardOrderServicelist> OrderServicelist = new List<vm_DashboardOrderServicelist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalServiceOrder = (from o in _context.tblOrders
                                               where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date
                                               select o.OrderId).Count();
                List<int> OrderIdList = (from o in _context.tblOrders
                                         where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date
                                         select o.OrderId).ToList();
                var Orderlist2 = (from o in _context.tblOrderServices
                                  where OrderIdList.Contains(o.OrderId)
                                  group o by o.ServiceId into o
                                  orderby o.Count()
                                  select new
                                  {
                                      ServiceId = o.Key,
                                      TotalOrder = o.Count()
                                  }).ToList().Take(6);
                foreach (var item2 in Orderlist2)
                {
                    string ServiceName2 = (from s in _context.tblServices
                                           where s.ServiceId == item2.ServiceId
                                           select s.ServiceNameEN).FirstOrDefault();
                    if (string.IsNullOrEmpty(ServiceName2))
                    {
                        ServiceName2 = (from s in _context.tblServices
                                        where s.ServiceId == item2.ServiceId
                                        select s.ServiceNameAR).FirstOrDefault();
                    }
                    OrderServicelist.Add(new vm_DashboardOrderServicelist
                    {
                        ServiceId = Convert.ToInt32(item2.ServiceId),
                        ServiceName = ServiceName2,
                        TotalServiceOrders = item2.TotalOrder
                    });
                }
                dashboard.OrderServicelist = OrderServicelist;
            }
            else
            {
                dashboard.TotalServiceOrder = _context.tblOrders.Select((tblOrder o) => o.OrderId).Count();
                var Orderlist = (from o in _context.tblOrderServices
                                 group o by o.ServiceId into o
                                 orderby o.Count()
                                 select new
                                 {
                                     ServiceId = o.Key,
                                     TotalOrder = o.Count()
                                 }).ToList().Take(6);
                foreach (var item in Orderlist)
                {
                    string ServiceName = (from s in _context.tblServices
                                          where s.ServiceId == item.ServiceId
                                          select s.ServiceNameEN).FirstOrDefault();
                    if (string.IsNullOrEmpty(ServiceName))
                    {
                        ServiceName = (from s in _context.tblServices
                                       where s.ServiceId == item.ServiceId
                                       select s.ServiceNameAR).FirstOrDefault();
                    }
                    OrderServicelist.Add(new vm_DashboardOrderServicelist
                    {
                        ServiceId = Convert.ToInt32(item.ServiceId),
                        ServiceName = ServiceName,
                        TotalServiceOrders = item.TotalOrder
                    });
                }
                dashboard.OrderServicelist = OrderServicelist;
            }
            return dashboard;
        }

        public vm_Dashboard GetInstallationWorkersUtilizationList(string StartDate, string EndDate)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            IQueryable<tblAdminUser> getLaboursList = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.AccountTypeId == 10).AsQueryable();
            List<int> LaboursList = getLaboursList.Select((tblAdminUser x) => x.UserId).ToList();
            IQueryable<tblOrderService> tblOrderServices = _context.tblOrderServices.AsQueryable();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                List<tblOrder> tblOrders2 = _context.tblOrders.Where((tblOrder o) => o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date).ToList();
                List<vm_DashboardLabourlist> LaboursDataList2 = new List<vm_DashboardLabourlist>();
                {
                    foreach (tblAdminUser item2 in getLaboursList)
                    {
                        string LabourFirstName2 = item2.FirstName;
                        string LabourLastName2 = item2.FirstName;
                        int TodayOrders2 = (from o in tblOrders2
                                            where o.LabourId == item2.UserId
                                            select o.OrderId).Count();
                        int TodayOrdersTotalUnit2 = 0;
                        List<int> TodayOrderList2 = (from o in tblOrders2
                                                     where o.LabourId == item2.UserId
                                                     select o.OrderId).ToList();
                        TodayOrdersTotalUnit2 = ((TodayOrderList2.Count > 0) ? (from o in tblOrderServices
                                                                                where TodayOrderList2.Contains(o.OrderId)
                                                                                select o.Quantity).Sum() : 0);
                        int TotalOrders2 = (from o in tblOrders2
                                            where o.LabourId == item2.UserId
                                            select o.OrderId).Count();
                        int Last30DaysOrder2 = (from o in tblOrders2
                                                where o.LabourId == item2.UserId
                                                select o.OrderId).Count();
                        LaboursDataList2.Add(new vm_DashboardLabourlist
                        {
                            Labour = LabourFirstName2 + " " + LabourLastName2,
                            TodaysOrders = TodayOrders2,
                            TodayOrdersTotalUnit = TodayOrdersTotalUnit2,
                            TotalOrders = TotalOrders2,
                            Performance = (Last30DaysOrder2 / 30 * 100 / 5).ToString()
                        });
                        dashboard.LabourList = LaboursDataList2;
                    }
                    return dashboard;
                }
            }
            List<vm_DashboardLabourlist> LaboursDataList = new List<vm_DashboardLabourlist>();
            List<tblOrder> tblOrders = _context.tblOrders.Where((tblOrder o) => o.InstallDate == DateTime.Today).ToList();
            foreach (tblAdminUser item in getLaboursList)
            {
                string LabourFirstName = item.FirstName;
                string LabourLastName = item.LastName;
                int TodayOrders = (from o in tblOrders
                                   where o.LabourId == item.UserId
                                   select o.OrderId).Count();
                int TodayOrdersTotalUnit = 0;
                List<int> TodayOrderList = (from o in tblOrders
                                            where o.LabourId == item.UserId
                                            select o.OrderId).ToList();
                if (TodayOrderList.Count > 0)
                {
                    TodayOrdersTotalUnit = (from o in tblOrderServices
                                            where TodayOrderList.Contains(o.OrderId)
                                            select o.Quantity).Sum();
                }
                int TotalOrders = (from o in tblOrders
                                   where o.LabourId == item.UserId
                                   select o.OrderId).Count();
                DateTime Last30Days = DateTime.Now.AddDays(-30.0);
                int Last30DaysOrder = (from o in tblOrders
                                       where o.InstallDate >= Last30Days && o.LabourId == item.UserId
                                       select o.OrderId).Count();
                LaboursDataList.Add(new vm_DashboardLabourlist
                {
                    Labour = LabourFirstName + " " + LabourLastName,
                    TodaysOrders = TodayOrders,
                    TodayOrdersTotalUnit = TodayOrdersTotalUnit,
                    TotalOrders = TotalOrders,
                    Performance = (Last30DaysOrder / 30 * 100 / 5).ToString()
                });
                dashboard.LabourList = LaboursDataList;
            }
            return dashboard;
        }

        public IEnumerable<vm_page_details> GetAdminPage(int PageId, int FieldType, int AccountType)
        {
            int target = 0;
            List<Admin_Page_Details> objResult = _context.Admin_Page_Details.Where((Admin_Page_Details x) => x.Page_Id == (int?)PageId && x.FieldType == (int?)FieldType).ToList();
            List<vm_page_details> details = new List<vm_page_details>();
            foreach (Admin_Page_Details item in objResult)
            {
                vm_page_details onedetails = new vm_page_details();
                onedetails.Admin_Pages = item.Admin_Pages;
                onedetails.Page_Id = item.Page_Id;
                onedetails.Display = item.Display;
                onedetails.FieldName = item.FieldName;
                onedetails.FieldType = item.FieldType;
                onedetails.SP = item.SP;
                onedetails.Agent = item.Agent;
                onedetails.Supplier = item.Supplier;
                switch (AccountType)
                {
                    case 6:
                    case 7:
                    case 20:
                        onedetails.Target = item.SP;
                        break;
                    case 14:
                    case 17:
                        onedetails.Target = item.Supplier;
                        break;
                }
                details.Add(onedetails);
            }
            return details;
        }

        public IEnumerable<vm_page_details> GetAdminPage()
        {
            List<Admin_Page_Details> objResult = _context.Admin_Page_Details.Include((Admin_Page_Details x) => x.Admin_Pages).ToList();
            List<vm_page_details> details = new List<vm_page_details>();
            foreach (Admin_Page_Details item in objResult)
            {
                vm_page_details Onedetails = new vm_page_details();
                Onedetails.Admin_Pages = item.Admin_Pages;
                Onedetails.Page_Id = item.Page_Id;
                Onedetails.Display = item.Display;
                Onedetails.FieldName = item.FieldName;
                Onedetails.FieldType = item.FieldType;
                Onedetails.SP = item.SP;
                Onedetails.Agent = item.Agent;
                Onedetails.Supplier = item.Supplier;
                details.Add(Onedetails);
            }
            return details;
        }

        public IEnumerable<tblOrder> GetSalesReportPrint(int TypeId, int? GroupId, DateTime fromdate, DateTime todate)
        {
            return _context.tblOrders.Where((tblOrder x) => (int?)x.SupplierId == GroupId).ToList();
        }

        public int GetSupplierOrProviderAdminId(int UserId)
        {
            int SupplierAdminId = (from u in _context.tblAdminUsers
                                   where u.UserId == UserId
                                   select u.AddedBy).FirstOrDefault();
            return Convert.ToInt32(SupplierAdminId);
        }

        public vm_Dashboard GetAdminDashboardWidgetsData(string StartDate, string EndDate, int UserGroupId, int UserGroupTypeId)
        {
            List<int> ProviderGroupUsers = (from u in _context.tblAdminUsers
                                            where u.UserGroupId == (int?)UserGroupId && (int?)u.UserGroupTypeId == (int?)UserGroupTypeId
                                            select u.UserId).ToList();
            vm_Dashboard dashboard = new vm_Dashboard();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.OpneComplain = (from o in _context.tblOrderComplains
                                          where o.StatusId == 2 || o.StatusId == 6
                                          where DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate) && ProviderGroupUsers.Contains((int)o.AddedBy)
                                          select o.ComplainId).Count();
                List<int> CloseComplainOrderIdList = (from o in _context.tblOrderComplains
                                                      where o.StatusId == 6 && DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate) && ProviderGroupUsers.Contains((int)o.AddedBy)
                                                      select o.ComplainId).ToList();
                dashboard.CloseComplain = CloseComplainOrderIdList.Count();
                dashboard.MissingOrders = 0;
                dashboard.UnassignedOrders = (from o in _context.tblOrders
                                              where o.Status == 1 && ProviderGroupUsers.Contains(o.AddedBy) && DbFunctions.TruncateTime(o.AddedDate) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedDate) <= DbFunctions.TruncateTime(_endDate)
                                              select o.OrderId).Count();
                dashboard.TotalRevenue = (int)(from o in _context.tblOrders
                                               where o.Status == 10 && ProviderGroupUsers.Contains(o.AddedBy) && DbFunctions.TruncateTime(o.AddedDate) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedDate) <= DbFunctions.TruncateTime(_endDate)
                                               select o.TotalAmount).Sum();
            }
            return dashboard;
        }

        public vm_Dashboard GetAdminServiceDistribution(string StartDate, string EndDate, int UserGroupId, int UserGroupTypeId)
        {
            List<int> ProviderGroupUsers = (from u in _context.tblAdminUsers
                                            where u.UserGroupId == (int?)UserGroupId && (int?)u.UserGroupTypeId == (int?)UserGroupTypeId
                                            select u.UserId).ToList();
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboardOrderServicelist> OrderServicelist = new List<vm_DashboardOrderServicelist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalServiceOrder = (from o in _context.tblOrders
                                               where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date
                                               select o.OrderId).Count();
                List<int> OrderIdList = (from o in _context.tblOrders
                                         where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date
                                         select o.OrderId).ToList();
                var Orderlist = (from o in _context.tblOrderServices
                                 where OrderIdList.Contains(o.OrderId)
                                 group o by o.ServiceId into o
                                 orderby o.Count()
                                 select new
                                 {
                                     ServiceId = o.Key,
                                     TotalOrder = o.Count()
                                 }).ToList().Take(6);
                foreach (var item in Orderlist)
                {
                    string ServiceName = (from s in _context.tblServices
                                          where s.ServiceId == item.ServiceId
                                          select s.ServiceNameEN).FirstOrDefault();
                    if (string.IsNullOrEmpty(ServiceName))
                    {
                        ServiceName = (from s in _context.tblServices
                                       where s.ServiceId == item.ServiceId
                                       select s.ServiceNameAR).FirstOrDefault();
                    }
                    OrderServicelist.Add(new vm_DashboardOrderServicelist
                    {
                        ServiceId = Convert.ToInt32(item.ServiceId),
                        ServiceName = ServiceName,
                        TotalServiceOrders = item.TotalOrder
                    });
                }
                dashboard.OrderServicelist = OrderServicelist;
            }
            return dashboard;
        }

        public vm_Dashboard GetAdminCompleteOrders(string StartDate, string EndDate, int UserGroupId, int UserGroupTypeId)
        {
            List<int> ProviderGroupUsers = (from u in _context.tblAdminUsers
                                            where u.UserGroupId == (int?)UserGroupId && (int?)u.UserGroupTypeId == (int?)UserGroupTypeId
                                            select u.UserId).ToList();
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboarMonthOrderlist> MonthOrderlist = new List<vm_DashboarMonthOrderlist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                        where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.Status == 9
                                                        select o.OrderId).Count();
                var Orderlist = (from o in _context.tblOrders
                                 where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.Status == 9
                                 group o by DbFunctions.TruncateTime(o.InstallDate) into o
                                 select new
                                 {
                                     Value = o.Count(),
                                     Day = (DateTime)o.Key
                                 }).ToList();
                foreach (var item in Orderlist)
                {
                    MonthOrderlist.Add(new vm_DashboarMonthOrderlist
                    {
                        Day = Convert.ToDateTime(item.Day.ToShortDateString()).Day + " " + Convert.ToDateTime(item.Day.ToShortDateString()).ToString("MMM", CultureInfo.InvariantCulture),
                        Order = item.Value
                    });
                }
                dashboard.MonthOrderlist = MonthOrderlist;
            }
            return dashboard;
        }

        public vm_Dashboard GetAdminWorkersList(string StartDate, string EndDate, int UserGroupId)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            IQueryable<tblAdminUser> getLaboursList = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.AccountTypeId == 10).AsQueryable();
            List<int> LaboursList = getLaboursList.Select((tblAdminUser x) => x.UserId).ToList();
            IQueryable<tblOrderService> tblOrderServices = _context.tblOrderServices.AsQueryable();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                List<tblOrder> tblOrders = _context.tblOrders.Where((tblOrder o) => DbFunctions.TruncateTime(o.InstallDate) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.InstallDate) <= DbFunctions.TruncateTime(_endDate)).ToList();
                List<vm_DashboardLabourlist> LaboursDataList = new List<vm_DashboardLabourlist>();
                {
                    foreach (tblAdminUser item in getLaboursList)
                    {
                        string LabourFirstName = item.FirstName;
                        string LabourLastName = item.LastName;
                        int TodayOrders = (from o in _context.tblOrders
                                           where o.LabourId == item.UserId && DbFunctions.TruncateTime(o.InstallDate) == DbFunctions.TruncateTime(DateTime.Today)
                                           select o.OrderId).Count();
                        int TodayOrdersTotalUnit = 0;
                        List<int> TodayOrderList = (from o in _context.tblOrders
                                                    where o.LabourId == item.UserId && DbFunctions.TruncateTime(o.InstallDate) == DbFunctions.TruncateTime(DateTime.Today)
                                                    select o.OrderId).ToList();
                        TodayOrdersTotalUnit = ((TodayOrderList.Count > 0) ? (from o in tblOrderServices
                                                                              where TodayOrderList.Contains(o.OrderId)
                                                                              select o.Quantity).Sum() : 0);
                        int TotalOrders = (from o in tblOrders
                                           where o.LabourId == item.UserId
                                           select o.OrderId).Count();
                        LaboursDataList.Add(new vm_DashboardLabourlist
                        {
                            Labour = LabourFirstName + " " + LabourLastName,
                            TodaysOrders = TodayOrders,
                            TodayOrdersTotalUnit = TodayOrdersTotalUnit,
                            TotalOrders = TotalOrders,
                            Performance = (TotalOrders / 26 * 100 / 5).ToString()
                        });
                        dashboard.LabourList = LaboursDataList;
                    }
                    return dashboard;
                }
            }
            return dashboard;
        }

        public tblAdminUser GetSupplierAdmin(int UserId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser u) => u.UserId == UserId).FirstOrDefault();
        }

        public tblAdminUser GetSPFromSupplierAdmin(int UserId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser u) => u.AddedBy == UserId && u.AccountTypeId == 20).FirstOrDefault();
        }

        public tblAdminUser GetSupplierFromProviderId(int UserId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser u) => u.UserId == UserId).FirstOrDefault();
        }

        public vm_Dashboard GetSupplierDashboardWidgetsData(string StartDate, string EndDate, int UserId, int UserGroupId, int UserGroupTypeId, int AccountTypeId)
        {
            List<int> GroupUsers = new List<int>();
            if (UserGroupTypeId == 2 && AccountTypeId == 17)
            {
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == UserId
                              select u.UserId).ToList();
            }
            else if (UserGroupTypeId == 2 && AccountTypeId == 18)
            {
                int AdminId = GetSupplierOrProviderAdminId(UserId);
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == AdminId
                              select u.UserId).ToList();
            }
            GroupUsers.Add(UserId);
            vm_Dashboard dashboard = new vm_Dashboard();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.OpneComplain = (from o in _context.tblOrderComplains
                                          where o.StatusId != 6 && o.StatusId != 7 && DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate) && GroupUsers.Contains((int)o.AddedBy)
                                          select o.ComplainId).Count();
                List<int> CloseComplainOrderIdList = (from o in _context.tblOrderComplains
                                                      where o.StatusId == 6 || o.StatusId == 6
                                                      where DbFunctions.TruncateTime(o.AddedOn) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate) && GroupUsers.Contains((int)o.AddedBy)
                                                      select o.ComplainId).ToList();
                dashboard.CloseComplain = CloseComplainOrderIdList.Count();
                if (CloseComplainOrderIdList.Count > 0)
                {
                    List<tblComplainHistory> TimeList = (from o in _context.tblComplainHistories
                                                         where CloseComplainOrderIdList.Contains((int)o.ComplainId)
                                                         orderby o.UpdateOn descending
                                                         select o).ToList();
                    tblComplainHistory FirstDate = TimeList.OrderByDescending((tblComplainHistory o) => o.UpdateOn).First();
                    tblComplainHistory LastDate = TimeList.OrderBy((tblComplainHistory o) => o.UpdateOn).First();
                    DateTime date1 = FirstDate.UpdateOn;
                    DateTime date2 = LastDate.UpdateOn;
                    dashboard.AverageClosingTime = Convert.ToInt32(new TimeSpan((date1 - date2).Ticks / CloseComplainOrderIdList.Count()).TotalHours);
                }
                else
                {
                    dashboard.AverageClosingTime = 0;
                }
                List<int> TodayOrderIdList = (from o in _context.tblOrders
                                              where o.InstallDate == DateTime.Today && o.Status != 12 && o.Status != 3 && GroupUsers.Contains(o.AddedBy)
                                              select o.OrderId).ToList();
                dashboard.TotalTodayOrderInstallation = TodayOrderIdList.Count();
                if (TodayOrderIdList.Count > 0)
                {
                    dashboard.TotalTodayOrderInstallationUnit = (from o in _context.tblOrderServices
                                                                 where TodayOrderIdList.Contains(o.OrderId)
                                                                 select o.Quantity).Sum();
                }
                else
                {
                    dashboard.TotalTodayOrderInstallationUnit = 0;
                }
                DateTime tomorrow = DateTime.Today.AddDays(1.0);
                List<int> TomorrowOrderIdList = (from o in _context.tblOrders
                                                 where o.InstallDate == tomorrow && o.Status != 12 && o.Status != 3 && GroupUsers.Contains(o.AddedBy)
                                                 select o.OrderId).ToList();
                dashboard.TotalTomorrowOrderInstallation = TomorrowOrderIdList.Count();
                if (TomorrowOrderIdList.Count > 0)
                {
                    dashboard.TotalTomorrowOrderInstallationUnit = (from o in _context.tblOrderServices
                                                                    where TomorrowOrderIdList.Contains(o.OrderId)
                                                                    select o.Quantity).Sum();
                }
                else
                {
                    dashboard.TotalTomorrowOrderInstallationUnit = 0;
                }
            }
            return dashboard;
        }

        public vm_Dashboard GetSupplierServiceDistribution(string StartDate, string EndDate, int UserId, int UserGroupId, int UserGroupTypeId, int AccountTypeId)
        {
            List<int> GroupUsers = new List<int>();
            if (UserGroupTypeId == 2 && AccountTypeId == 17)
            {
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == UserId
                              select u.UserId).ToList();
            }
            else if (UserGroupTypeId == 2 && AccountTypeId == 18)
            {
                int AdminId = GetSupplierOrProviderAdminId(UserId);
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == AdminId
                              select u.UserId).ToList();
            }
            GroupUsers.Add(UserId);
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboardOrderServicelist> OrderServicelist = new List<vm_DashboardOrderServicelist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalServiceOrder = (from o in _context.tblOrders
                                               where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && GroupUsers.Contains(o.AddedBy)
                                               select o.OrderId).Count();
                List<int> OrderIdList = (from o in _context.tblOrders
                                         where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && GroupUsers.Contains(o.AddedBy)
                                         select o.OrderId).ToList();
                var Orderlist = (from o in _context.tblOrderServices
                                 where OrderIdList.Contains(o.OrderId)
                                 group o by o.ServiceId into o
                                 orderby o.Count()
                                 select new
                                 {
                                     ServiceId = o.Key,
                                     TotalOrder = o.Count()
                                 }).ToList().Take(6);
                foreach (var item in Orderlist)
                {
                    string ServiceName = (from s in _context.tblServices
                                          where s.ServiceId == item.ServiceId
                                          select s.ServiceNameEN).FirstOrDefault();
                    if (string.IsNullOrEmpty(ServiceName))
                    {
                        ServiceName = (from s in _context.tblServices
                                       where s.ServiceId == item.ServiceId
                                       select s.ServiceNameAR).FirstOrDefault();
                    }
                    OrderServicelist.Add(new vm_DashboardOrderServicelist
                    {
                        ServiceId = Convert.ToInt32(item.ServiceId),
                        ServiceName = ServiceName,
                        TotalServiceOrders = item.TotalOrder
                    });
                }
                dashboard.OrderServicelist = OrderServicelist;
            }
            return dashboard;
        }

        public vm_Dashboard GetSupplierCompleteOrders(string StartDate, string EndDate, int UserId, int UserGroupId, int UserGroupTypeId, int AccountTypeId)
        {
            List<int> GroupUsers = new List<int>();
            if (UserGroupTypeId == 2 && AccountTypeId == 17)
            {
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == UserId
                              select u.UserId).ToList();
            }
            else if (UserGroupTypeId == 2 && AccountTypeId == 18)
            {
                int AdminId = GetSupplierOrProviderAdminId(UserId);
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == AdminId
                              select u.UserId).ToList();
            }
            GroupUsers.Add(UserId);
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboarMonthOrderlist> MonthOrderlist = new List<vm_DashboarMonthOrderlist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                        where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && GroupUsers.Contains(o.AddedBy)
                                                        where o.Status == 9 || o.Status == 10
                                                        select o.OrderId).Count();
                var Orderlist = (from o in _context.tblOrders
                                 where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && GroupUsers.Contains(o.AddedBy)
                                 where o.Status == 9 || o.Status == 10
                                 group o by o.InstallDate into o
                                 select new
                                 {
                                     Value = o.Count(),
                                     Day = (DateTime)o.Key
                                 }).ToList();
                foreach (var item in Orderlist)
                {
                    MonthOrderlist.Add(new vm_DashboarMonthOrderlist
                    {
                        Day = Convert.ToDateTime(item.Day.ToShortDateString()).Day + " " + Convert.ToDateTime(item.Day.ToShortDateString()).ToString("MMM", CultureInfo.InvariantCulture),
                        Order = item.Value
                    });
                }
                dashboard.MonthOrderlist = MonthOrderlist;
            }
            return dashboard;
        }

        public vm_Dashboard GetSupplierWorkersUtilizationListt(string StartDate, string EndDate, int UserId, int UserGroupId, int UserGroupTypeId, int AccountTypeId)
        {
            List<int> GroupUsers = new List<int>();
            if (UserGroupTypeId == 2 && AccountTypeId == 17)
            {
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == UserId
                              select u.UserId).ToList();
            }
            else if (UserGroupTypeId == 2 && AccountTypeId == 18)
            {
                int AdminId = GetSupplierOrProviderAdminId(UserId);
                GroupUsers = (from u in _context.tblAdminUsers
                              where u.AddedBy == AdminId
                              select u.UserId).ToList();
            }
            GroupUsers.Add(UserId);
            vm_Dashboard dashboard = new vm_Dashboard();
            IQueryable<tblAdminUser> getLaboursList = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.AccountTypeId == 10 && GroupUsers.Contains(x.AddedBy)).AsQueryable();
            List<int> LaboursList = getLaboursList.Select((tblAdminUser x) => x.UserId).ToList();
            IQueryable<tblOrderService> tblOrderServices = _context.tblOrderServices.AsQueryable();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                List<tblOrder> tblOrders = _context.tblOrders.Where((tblOrder o) => DbFunctions.TruncateTime(o.InstallDate) >= DbFunctions.TruncateTime(_startDate) && DbFunctions.TruncateTime(o.InstallDate) <= DbFunctions.TruncateTime(_endDate) && GroupUsers.Contains(o.AddedBy)).ToList();
                List<vm_DashboardLabourlist> LaboursDataList = new List<vm_DashboardLabourlist>();
                {
                    foreach (tblAdminUser item in getLaboursList)
                    {
                        string LabourFirstName = item.FirstName;
                        string LabourLastName = item.LastName;
                        int TodayOrders = (from o in _context.OrdersAssigneds
                                           where o.LabourId == (int?)item.UserId && o.tblOrder.InstallDate == DbFunctions.TruncateTime(DateTime.Today)
                                           select o.tblOrder.OrderId).Count();
                        int TodayOrdersTotalUnit = 0;
                        int TotalOrders = (from o in _context.OrdersAssigneds
                                           where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                           select o.OrderId).Count();
                        List<int> TodayOrderList = (from o in _context.OrdersAssigneds
                                                    where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                                    select o.tblOrder.OrderId).ToList();
                        TodayOrdersTotalUnit = ((TotalOrders > 0) ? (from o in _context.OrdersAssigneds
                                                                     where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                                                     select o into x
                                                                     select x.Total).Sum().Value : 0);
                        string TotaWorkingHours = _context.tblSettings.Where((tblSetting x) => (int?)x.ProviderId == item.UserGroupId).SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("WorkingHours")).KeyValue;
                        int? sum = 0;
                        List<tblProviderTimeSlot> TotalCosumedHours = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot o) => o.LabourId == (int?)item.UserId && o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date).ToList();
                        sum = ((TotalCosumedHours.Count < 0) ? new int?(0) : TotalCosumedHours.Select((tblProviderTimeSlot o) => o.TotalConsumedHour).Sum());
                        db_Settings objSetting = new db_Settings();
                        tblAdminUser SupplierAdmin = objSetting.GetSupplierFromProviderId(item.AddedBy);
                        float Performance = (float)sum.Value / float.Parse(TotaWorkingHours);
                        LaboursDataList.Add(new vm_DashboardLabourlist
                        {
                            ServiceProvider = SupplierAdmin.FirstName + " " + SupplierAdmin.LastName,
                            Labour = LabourFirstName + " " + LabourLastName,
                            TodaysOrders = TodayOrders,
                            TodayOrdersTotalUnit = TodayOrdersTotalUnit,
                            TotalOrders = TotalOrders,
                            Performance = $"{Performance * 100f:0.00}"
                        });
                        dashboard.LabourList = LaboursDataList;
                    }
                    return dashboard;
                }
            }
            return dashboard;
        }

        public vm_Dashboard GetProviderDashboardWidgetsData(string StartDate, string EndDate, int UserGroupId, int UserId)
        {
            int SupplierAdminid = GetSupplierOrProviderAdminId(UserId);
            List<int> UserList = (from u in _context.tblAdminUsers
                                  where u.AddedBy == SupplierAdminid
                                  select u.UserId).ToList();
            vm_Dashboard dashboard = new vm_Dashboard();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                int ProviderId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
                dashboard.OpneComplain = (from o in _context.tblOrderComplains.Include((tblOrderComplain x) => x.tblOrder)
                                          where o.StatusId == 1 && o.tblOrder.ReservedProvider == ProviderId
                                          select o.ComplainId).Count();
                List<int> CloseComplainOrderIdList = (from o in _context.tblOrderComplains
                                                      where o.StatusId == 6 || o.StatusId == 7
                                                      where DbFunctions.TruncateTime(o.AddedOn) >= _startDate && DbFunctions.TruncateTime(o.AddedOn) <= DbFunctions.TruncateTime(_endDate) && UserList.Contains((int)o.AddedBy)
                                                      select o.ComplainId).ToList();
                dashboard.CloseComplain = CloseComplainOrderIdList.Count();
                IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId).ToList()
                                                    select x.UserId;
                List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)UserGroupId).ToList();
                IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => !x.ReservedBy.HasValue || getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                         select x.OrderId;
                IEnumerable<int?> UnassignedOrderslistid = from x in orderDisplays.Where((OrderDisplay x) => x.ReservedBy.HasValue && getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                           select x.OrderId;
                int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                List<int> UnassignedOrdersIdList = (from o in _context.tblOrders
                                                    where UnassignedOrderslistid.Contains(o.OrderId) && o.LabourId == 0 && o.Status != 11 && o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min
                                                    select o.OrderId).ToList();
                dashboard.UnassignedOrders = UnassignedOrdersIdList.Count();
                if (UnassignedOrdersIdList.Count > 0)
                {
                    dashboard.UnassignedOrdersUnit = (from o in _context.tblOrderServices
                                                      where UnassignedOrdersIdList.Contains(o.OrderId)
                                                      select o.Quantity).Sum();
                }
                else
                {
                    dashboard.UnassignedOrdersUnit = 0;
                }
                IEnumerable<int?> MissingOrders = from x in orderDisplays.Where((OrderDisplay x) => x.ReservedBy.HasValue && Convert.ToInt32(x.ReservedBy) != UserId).ToList()
                                                  select x.OrderId;
                List<int> MissingOrdersId = (from o in _context.tblOrders
                                             where MissingOrders.Contains(o.OrderId) && o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min
                                             select o.OrderId).ToList();
                dashboard.MissingOrders = MissingOrdersId.Count();
                if (MissingOrdersId.Count > 0)
                {
                    dashboard.MissingOrdersUnit = (from o in _context.tblOrderServices
                                                   where MissingOrdersId.Contains(o.OrderId)
                                                   select o.Quantity).Sum();
                }
                else
                {
                    dashboard.MissingOrdersUnit = 0;
                }
                List<int> TotalRevenueOrderList = (from o in _context.tblOrders
                                                   where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && slectedSPorderlistid.Contains(o.OrderId)
                                                   where o.Status == 9 || o.Status == 10
                                                   select o.OrderId).ToList();
                if (TotalRevenueOrderList.Count > 0)
                {
                    dashboard.TotalRevenue = (int)(from o in _context.tblOrders
                                                   where TotalRevenueOrderList.Contains(o.OrderId)
                                                   select o.TotalAmount).Sum();
                }
                else
                {
                    dashboard.TotalRevenue = 0;
                }
            }
            return dashboard;
        }

        public vm_Dashboard GetProviderServiceDistribution(string StartDate, string EndDate, int UserGroupId)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboardOrderServicelist> OrderServicelist = new List<vm_DashboardOrderServicelist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId).ToList()
                                                    select x.UserId;
                List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)UserGroupId).ToList();
                IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                         select x.OrderId;
                int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                dashboard.TotalServiceOrder = (from o in _context.tblOrders
                                               where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min && slectedSPorderlistid.Contains(o.OrderId)
                                               select o.OrderId).Count();
                List<int> OrderIdList = (from o in _context.tblOrders
                                         where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min && slectedSPorderlistid.Contains(o.OrderId)
                                         select o.OrderId).ToList();
                var Orderlist = (from o in _context.tblOrderServices
                                 where OrderIdList.Contains(o.OrderId)
                                 group o by o.ServiceId into o
                                 orderby o.Count()
                                 select new
                                 {
                                     ServiceId = o.Key,
                                     TotalOrder = o.Count()
                                 }).ToList().Take(6);
                foreach (var item in Orderlist)
                {
                    string ServiceName = (from s in _context.tblServices
                                          where s.ServiceId == item.ServiceId
                                          select s.ServiceNameEN).FirstOrDefault();
                    if (string.IsNullOrEmpty(ServiceName))
                    {
                        ServiceName = (from s in _context.tblServices
                                       where s.ServiceId == item.ServiceId
                                       select s.ServiceNameAR).FirstOrDefault();
                    }
                    OrderServicelist.Add(new vm_DashboardOrderServicelist
                    {
                        ServiceId = Convert.ToInt32(item.ServiceId),
                        ServiceName = ServiceName,
                        TotalServiceOrders = item.TotalOrder
                    });
                }
                dashboard.OrderServicelist = OrderServicelist;
            }
            return dashboard;
        }

        public vm_Dashboard GetProviderCompleteOrders(string StartDate, string EndDate, int UserGroupId)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            List<vm_DashboarMonthOrderlist> MonthOrderlist = new List<vm_DashboarMonthOrderlist>();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId).ToList()
                                                    select x.UserId;
                List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)UserGroupId).ToList();
                IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                         select x.OrderId;
                int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                dashboard.TotalCompletedOrderofMonth = (from o in _context.tblOrders
                                                        where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min && slectedSPorderlistid.Contains(o.OrderId)
                                                        where o.Status == 9 || o.Status == 10
                                                        select o.OrderId).Count();
                var Orderlist = (from o in _context.tblOrders
                                 where o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min && slectedSPorderlistid.Contains(o.OrderId)
                                 where o.Status == 9 || o.Status == 10
                                 group o by o.InstallDate into o
                                 select new
                                 {
                                     Value = o.Count(),
                                     Day = (DateTime)o.Key
                                 }).ToList();
                foreach (var item in Orderlist)
                {
                    MonthOrderlist.Add(new vm_DashboarMonthOrderlist
                    {
                        Day = Convert.ToDateTime(item.Day.ToShortDateString()).Day + " " + Convert.ToDateTime(item.Day.ToShortDateString()).ToString("MMM", CultureInfo.InvariantCulture),
                        Order = item.Value
                    });
                }
                dashboard.MonthOrderlist = MonthOrderlist;
            }
            return dashboard;
        }

        public vm_Dashboard GetProviderWorkersList(string StartDate, string EndDate, int UserGroupId)
        {
            vm_Dashboard dashboard = new vm_Dashboard();
            IQueryable<tblAdminUser> getLaboursList = _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.AccountTypeId == 10 && x.UserGroupId == (int?)UserGroupId).AsQueryable();
            List<int> LaboursList = getLaboursList.Select((tblAdminUser x) => x.UserId).ToList();
            IQueryable<tblOrderService> tblOrderServices = _context.tblOrderServices.AsQueryable();
            if (!string.IsNullOrEmpty(StartDate) && !string.IsNullOrEmpty(EndDate))
            {
                DateTime _startDate = Convert.ToDateTime(StartDate).Date;
                DateTime _endDate = Convert.ToDateTime(EndDate).Date;
                IEnumerable<int> getagentspovider = from x in _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId).ToList()
                                                    select x.UserId;
                List<OrderDisplay> orderDisplays = _context.OrderDisplays.Where((OrderDisplay x) => x.ProviderId == (int?)UserGroupId).ToList();
                IEnumerable<int?> slectedSPorderlistid = from x in orderDisplays.Where((OrderDisplay x) => getagentspovider.Contains(Convert.ToInt32(x.ReservedBy))).ToList()
                                                         select x.OrderId;
                int mins = Convert.ToInt32(GetSettingByKey("ordershowduration"));
                DateTime now5Min = DateTime.Now.AddMinutes(-mins);
                List<tblOrder> tblOrders = _context.tblOrders.Where((tblOrder o) => o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date && o.AddedDate <= now5Min && slectedSPorderlistid.Contains(o.OrderId)).ToList();
                List<vm_DashboardLabourlist> LaboursDataList = new List<vm_DashboardLabourlist>();
                {
                    foreach (tblAdminUser item in getLaboursList)
                    {
                        string LabourFirstName = item.FirstName;
                        string LabourLastName = item.LastName;
                        int TodayOrders = (from o in _context.OrdersAssigneds
                                           where o.LabourId == (int?)item.UserId && o.tblOrder.InstallDate == DbFunctions.TruncateTime(DateTime.Today) && slectedSPorderlistid.Contains(o.OrderId)
                                           select o.tblOrder.OrderId).Count();
                        int TodayOrdersTotalUnit = 0;
                        List<int> TodayOrderList = (from o in _context.OrdersAssigneds
                                                    where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                                    select o.tblOrder.OrderId).ToList();
                        int TotalOrders = (from o in _context.OrdersAssigneds
                                           where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                           select o.OrderId).Count();
                        TodayOrdersTotalUnit = ((TodayOrderList.Count > 0) ? (from o in _context.OrdersAssigneds
                                                                              where o.LabourId == (int?)item.UserId && DbFunctions.TruncateTime(o.tblOrder.InstallDate) >= DbFunctions.TruncateTime(_startDate.Date) && DbFunctions.TruncateTime(o.tblOrder.InstallDate) <= DbFunctions.TruncateTime(_endDate.Date)
                                                                              select o into x
                                                                              select x.Total).Sum().Value : 0);
                        int? sum = 0;
                        List<tblProviderTimeSlot> TotalCosumedHours = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot o) => o.LabourId == (int?)item.UserId && o.InstallDate >= _startDate.Date && o.InstallDate <= _endDate.Date).ToList();
                        sum = ((TotalCosumedHours.Count < 0) ? new int?(0) : TotalCosumedHours.Select((tblProviderTimeSlot o) => o.TotalConsumedHour).Sum());
                        string TotaWorkingHours = _context.tblSettings.Where((tblSetting x) => x.ProviderId == UserGroupId).SingleOrDefault((tblSetting x) => x.KeyName.ToLower().Equals("WorkingHours")).KeyValue;
                        float Performance = (float)sum.Value / float.Parse(TotaWorkingHours);
                        LaboursDataList.Add(new vm_DashboardLabourlist
                        {
                            Labour = LabourFirstName + " " + LabourLastName,
                            TodaysOrders = TodayOrders,
                            TodayOrdersTotalUnit = TodayOrdersTotalUnit,
                            TotalOrders = TotalOrders,
                            Performance = $"{Performance * 100f:0.00}"
                        });
                        dashboard.LabourList = LaboursDataList;
                    }
                    return dashboard;
                }
            }
            return dashboard;
        }

        public async Task<int> Add_OrderUserLink(int OrderId)
        {
            DateTime FinishOn = await (from x in _context.tblOrderHistories
                                       where x.OrderId == OrderId
                                       orderby x.ActivityDate descending
                                       select x.ActivityDate).FirstOrDefaultAsync();
            try
            {
                tblOrderUserLink inputs = new tblOrderUserLink
                {
                    OrderId = OrderId,
                    AddedOn = DateTime.Now,
                    IsActive = true,
                    ExpireOn = FinishOn.AddDays(30.0)
                };
                _context.tblOrderUserLinks.Add(inputs);
                await _context.SaveChangesAsync();
                return inputs.Id;
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public int Add_OrderUserLink2(int OrderId)
        {
            try
            {
                tblOrderUserLink inputs = new tblOrderUserLink
                {
                    OrderId = OrderId,
                    AddedOn = DateTime.Now,
                    IsActive = true,
                    ExpireOn = DateTime.Now.AddDays(30.0)
                };
                _context.tblOrderUserLinks.Add(inputs);
                _context.SaveChanges();
                return inputs.Id;
            }
            catch (Exception)
            {
            }
            return 0;
        }

        public async Task<tblOrderUserLink> Get_OrderUserLink(int Id)
        {
            return await _context.tblOrderUserLinks.Where((tblOrderUserLink x) => x.Id == Id).FirstOrDefaultAsync();
        }

        public Item GetItemById(int Id)
        {
            return _context.Items.FirstOrDefault((Item x) => x.Id == Id);
        }

        public Page<Item> GetItem(int pageSize, int currentPage, Sorts<Item> sorts, Filters<Item> filters, int userGroupId)
        {
            return _context.Items.Where((Item x) => x.UserGroupId == (int?)userGroupId).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblOrderComplain> GetComplains(int pageSize, int currentPage,
         Sorts<tblOrderComplain> sorts, Filters<tblOrderComplain> filters, int flag, int SupplierId = 0)
        {
            Page<tblOrderComplain> model;
            if (flag == 1)
                model = _context.tblOrderComplains.Where(x => x.StatusId != (int)enumComplainStatus.ResolveBySp && x.StatusId != (int)enumComplainStatus.Reject).Paginate(currentPage, pageSize, sorts, filters);
            else if (flag == 0)
                model = _context.tblOrderComplains.Where(x => x.StatusId == (int)enumComplainStatus.ResolveBySp || x.StatusId == (int)enumComplainStatus.Reject).Paginate(currentPage, pageSize, sorts, filters);
            else
                model = _context.tblOrderComplains.Paginate(currentPage, pageSize, sorts, filters);

            return model;
        }
        public Page<tblOrderComplain> GetSupplierAdminComplains(int pageSize, int currentPage,
                Sorts<tblOrderComplain> sorts, Filters<tblOrderComplain> filters, int flag, int UserId)
        {
            List<int> SupplierAdminUsers = _context.tblAdminUsers.Where(u => u.AddedBy == UserId).Select(u => u.UserId).ToList();

            Page<tblOrderComplain> model;
            if (flag == 1)
                model = _context.tblOrderComplains.Where(x => x.StatusId != (int)enumComplainStatus.ResolveBySp && x.StatusId != (int)enumComplainStatus.Reject && SupplierAdminUsers.Contains((int)x.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);
            else if (flag == 0)
                model = _context.tblOrderComplains.Where(x => SupplierAdminUsers.Contains((int)x.AddedBy)).Where(x => x.StatusId == (int)enumComplainStatus.ResolveBySp || x.StatusId == (int)enumComplainStatus.Reject).Paginate(currentPage, pageSize, sorts, filters);
            else
                model = _context.tblOrderComplains.Where(x => SupplierAdminUsers.Contains((int)x.AddedBy)).Paginate(currentPage, pageSize, sorts, filters);

            return model;
        }


        public Page<tblOrderComplain> GetComplainsforSuppliers(int pageSize, int currentPage, Sorts<tblOrderComplain> sorts, Filters<tblOrderComplain> filters, int SupplierId, int accountype, int userid)
        {
            if (accountype == 15)
            {
                return _context.tblOrderComplains.Where((tblOrderComplain x) => x.tblOrder.SupplierId == SupplierId && x.tblOrder.AddedBy == userid).Paginate(currentPage, pageSize, sorts, filters);
            }
            return _context.tblOrderComplains.Where((tblOrderComplain x) => x.tblOrder.SupplierId == SupplierId).Paginate(currentPage, pageSize, sorts, filters);
        }

        public async Task<vm_ComplainResponse> GetComplainById(int Id)
        {
            vm_ComplainResponse output = Mapper.Map<tblOrderComplain, vm_ComplainResponse>(await _context.tblOrderComplains.Where((tblOrderComplain x) => x.ComplainId == Id).FirstOrDefaultAsync());
            if (output != null)
            {
                Task<List<string>> list = GetmultipleComplaintype(int.Parse(output.ComplainId), isEnglish);
                if (list.Result != null)
                {
                    output.Category = string.Join(", ", list.Result);
                }
                else
                {
                    output.Category = (isEnglish ? output.CategoryEN : output.CategoryAR);
                }
            }
            return output;
        }

        public async Task<tblOrderComplain> GetComplainDetail(int Id)
        {
            return await _context.tblOrderComplains.Where((tblOrderComplain x) => x.ComplainId == Id).FirstOrDefaultAsync();
        }

        public async Task<int> AddComplain(vm_Complain model)
        {
            tblOrder data = await _context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == model.OrderId);
            tblOrderComplain obj = new tblOrderComplain
            {
                OrderId = model.OrderId,
                Comments = model.Complain,
                StatusId = (byte)((model.ComplainBy != 3) ? 1u : 2u),
                AddedOn = DateTime.Now,
                Subject = model.Subject,
                ComplainBy = model.ComplainBy,
                AddedBy = ((Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]) == 0) ? data.AddedBy : Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId])),
                ComplainTypeId = model.CategoryId
            };
            _context.tblOrderComplains.Add(obj);
            await _context.SaveChangesAsync();
            tblComplainHistory history = new tblComplainHistory
            {
                ComplainId = obj.ComplainId,
                Comments = model.Complain,
                StatusId = (byte)1,
                UpdateOn = DateTime.Now,
                UpdatedBy = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId])
            };
            _context.tblComplainHistories.Add(history);
            await _context.SaveChangesAsync();
            return obj.ComplainId;
        }

        public async Task<int> AddMultipleComplain(vm_Complain model)
        {
            tblMultipleComplain obj = new tblMultipleComplain
            {
                Complainid = model.ComplainId,
                ComplainTypeId = model.CategoryId
            };
            _context.tblMultipleComplains.Add(obj);
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> DeleteItem(int Id)
        {
            Item model = GetItemById(Id);
            _context.Items.Remove(model);
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> AddItem(vm_Item model, int userGroupId)
        {
            Item item = new Item();
            item.name = model.name;
            item.UserGroupId = userGroupId;
            item.name_en = model.name_en;
            _context.Items.Add(item);
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> UpdateComplain(vm_ComplainResponse model)
        {
            if (model.StatusId > 0)
            {
                tblOrderComplain data = await _context.tblOrderComplains.FirstOrDefaultAsync((tblOrderComplain x) => x.ComplainId == model.Id);
                data.Response = model.Response;
                data.StatusId = model.StatusId;
                data.UpdateOn = DateTime.Now;
                await _context.SaveChangesAsync();
                tblComplainHistory history2 = new tblComplainHistory
                {
                    ComplainId = model.Id,
                    Comments = model.Response,
                    StatusId = model.StatusId,
                    UpdateOn = DateTime.Now,
                    UpdatedBy = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId])
                };
                _context.tblComplainHistories.Add(history2);
                await _context.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(model.Response2))
            {
                tblComplainHistory history = new tblComplainHistory
                {
                    ComplainId = model.Id,
                    Comments = model.Response2,
                    StatusId = model.StatusId,
                    UpdateOn = DateTime.Now,
                    UpdatedBy = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId])
                };
                _context.tblComplainHistories.Add(history);
                await _context.SaveChangesAsync();
            }
            return 1;
        }

        public async Task<vm_Email> getEmailById(int Id)
        {
            tblEmail model = await _context.tblEmails.Where((tblEmail x) => x.Id == Id).FirstOrDefaultAsync();
            return new vm_Email
            {
                Id = model.Id,
                KeyName = model.KeyName,
                SubjectEN = model.SubjectEN,
                SubjectAR = model.SubjectAR,
                EmailTextEN = model.EmailTextEN,
                EmailTextAR = model.EmailTextAR
            };
        }

        public async Task<int> EditEmail(vm_Email model)
        {
            tblEmail email = await _context.tblEmails.Where((tblEmail x) => x.Id == model.Id).FirstOrDefaultAsync();
            email.KeyName = model.KeyName;
            email.SubjectEN = model.SubjectEN;
            email.SubjectAR = model.SubjectAR;
            email.EmailTextEN = model.EmailTextEN;
            email.EmailTextAR = model.EmailTextAR;
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<List<vm_SMS>> GetSMS()
        {
            return Mapper.Map<List<tblSM>, List<vm_SMS>>(await _context.tblSMS.ToListAsync());
        }

        public async Task<int> EditSMS(vm_SMS model)
        {
            tblSM email = await _context.tblSMS.Where((tblSM x) => x.Id == model.Id).FirstOrDefaultAsync();
            email.SMSTextEN = model.SMSTextEN;
            email.SMSTextAR = model.SMSTextAR;
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<List<vm_PushNotification>> GetPushNotification()
        {
            return Mapper.Map<List<tblPushNotification>, List<vm_PushNotification>>(await _context.tblPushNotifications.ToListAsync());
        }

        public async Task<int> EditPushNotification(vm_PushNotification model)
        {
            tblPushNotification notification = await _context.tblPushNotifications.Where((tblPushNotification x) => x.Id == model.Id).FirstOrDefaultAsync();
            notification.PushNotificationTextEN = model.PushNotificationTextEN;
            notification.PushNotificationTextAR = model.PushNotificationTextAR;
            await _context.SaveChangesAsync();
            return 1;
        }

        public async Task<int> AddPushNotification(vm_PushNotification model)
        {
            tblPushNotification obj = new tblPushNotification
            {
                KeyName = model.KeyName,
                PushNotificationTextEN = model.PushNotificationTextEN,
                PushNotificationTextAR = model.PushNotificationTextAR
            };
            _context.tblPushNotifications.Add(obj);
            await _context.SaveChangesAsync();
            return obj.Id;
        }

        public async Task<List<vm_ComplainType>> GetComplainType()
        {
            return Mapper.Map<List<tblComplainType>, List<vm_ComplainType>>(await _context.tblComplainTypes.ToListAsync());
        }

        public async Task<List<vm_ComplainType>> GetComplainType(int usergroupid)
        {
            return Mapper.Map<List<tblComplainType>, List<vm_ComplainType>>(await _context.tblComplainTypes.Where((tblComplainType x) => (int?)x.UserGroupTypeId == (int?)usergroupid).ToListAsync());
        }

        public async Task<List<vm_ComplainType>> GetOrderServiceComplainType(int OrderId)
        {
            return Mapper.Map<List<tblComplainType>, List<vm_ComplainType>>(await _context.tblComplainTypes.ToListAsync());
        }

        public async Task<List<vm_ComplainType>> GetSupplierComplainType(int ProviderId)
        {
            return Mapper.Map<List<tblComplainType>, List<vm_ComplainType>>(await _context.tblComplainTypes.Where((tblComplainType c) => (int?)c.UserGroupTypeId == (int?)ProviderId).ToListAsync());
        }

        public List<short> GetOrderCategoryList(int OrderId)
        {
            List<int> ServiceList = (from o in _context.tblOrderServices
                                     where o.OrderId == OrderId
                                     select o.ServiceId).ToList();
            List<short?> ServiceCategoryList = (from s in _context.tblServices
                                                where ServiceList.Contains(s.ServiceId)
                                                select s into o
                                                select o.CategoryId).ToList();
            return (from c in _context.Categories
                    where ServiceCategoryList.Contains(c.Id)
                    select c.Id).ToList();
        }

        public List<SelectListItem> GetComplainList(List<short> CategoryId)
        {
            return (from x in _context.tblComplainTypes
                    where x.IsActive == (bool?)true && CategoryId.Contains((short)x.ComplainCategoryId)
                    select new SelectListItem
                    {
                        Value = x.ComplainTypeId.ToString(),
                        Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                    }).ToList();
        }

        public List<SelectListItem> GetComplainList(List<short> CategoryId, int Usergroupid = 0)
        {
            return (from x in _context.tblComplainTypes
                    where x.IsActive == (bool?)true && (int?)x.UserGroupTypeId == (int?)Usergroupid && CategoryId.Contains((short)x.ComplainCategoryId)
                    select new SelectListItem
                    {
                        Value = x.ComplainTypeId.ToString(),
                        Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                    }).ToList();
        }

        public List<SelectListItem> DDLComplainType()
        {
            return _context.tblComplainTypes.Select((tblComplainType x) => new SelectListItem
            {
                Value = x.ComplainTypeId.ToString(),
                Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
            }).ToList();
        }

        public List<SelectListItem> DDCategory(int usergroupid)
        {
            return (from x in _context.Categories
                    where x.UserGroupId == (int?)usergroupid
                    select new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = ((isEnglish == true) ? x.NameEn : x.Name)
                    }).ToList();
        }

        public async Task<int> AddEditComplainType(vm_ComplainType model)
        {
            int ProviderId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            if (model.ComplainTypeId > 0)
            {
                tblComplainType input2 = await _context.tblComplainTypes.Where((tblComplainType x) => x.ComplainTypeId == model.ComplainTypeId).FirstOrDefaultAsync();
                input2.TitleEN = model.TitleEN;
                input2.TitleAR = model.TitleAR;
                if (ProviderId > 0)
                {
                    input2.UserGroupTypeId = Convert.ToByte(ProviderId);
                }
                else
                {
                    input2.UserGroupTypeId = model.UserGroupTypeId;
                }
                input2.IsActive = model.IsActive;
                input2.ComplainCategoryId = model.ComplainCategoryId;
                await _context.SaveChangesAsync();
            }
            else
            {
                tblComplainType input = Mapper.Map<vm_ComplainType, tblComplainType>(model);
                if (ProviderId > 0)
                {
                    input.UserGroupTypeId = Convert.ToByte(ProviderId);
                }
                else
                {
                    input.UserGroupTypeId = model.UserGroupTypeId;
                }
                input.IsActive = true;
                _context.tblComplainTypes.Add(input);
                await _context.SaveChangesAsync();
            }
            return 1;
        }

        public Page<tblComplainHistory> GetComplainHistory(int pageSize, int currentPage, Sorts<tblComplainHistory> sorts, Filters<tblComplainHistory> filters)
        {
            return _context.tblComplainHistories.Paginate(currentPage, pageSize, sorts, filters);
        }

        public List<SelectListItem> DDLCompanies()
        {
            return (from o in _context.tblUserGroupCompanies
                    where o.UserGroupTypeId == 1
                    select o into x
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> DDLCompanies(int addedBy)
        {
            tblAdminUser SuperAdmin = objUser.GetSuperAdminId();
            return (from o in _context.tblUserGroupCompanies
                    where o.UserGroupTypeId == 1
                    where o.AddedBy == addedBy || o.AddedBy == SuperAdmin.UserId
                    select o into x
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> DDLSupllierCompanies(int UserId)
        {
            return (from o in _context.tblUserGroupCompanies
                    where o.UserGroupTypeId == 1 && o.AddedBy == UserId
                    select o into x
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> DDLAjentUsers(int id)
        {
            List<SelectListItem> output = new List<SelectListItem>();
            if (id == 0)
            {
                id = 2;
                return (from x in _context.tblAdminUsers
                        where x.AccountTypeId == 1 && (int?)x.UserGroupTypeId == (int?)id
                        select new SelectListItem
                        {
                            Value = x.UserId.ToString(),
                            Text = string.Concat(x.FirstName.ToString() + " ", x.LastName.ToString())
                        }).ToList();
            }
            return (from x in _context.tblAdminUsers
                    where x.AccountTypeId == 1 && x.UserGroupId == (int?)id
                    select new SelectListItem
                    {
                        Value = x.UserId.ToString(),
                        Text = string.Concat(x.FirstName.ToString() + " ", x.LastName.ToString())
                    }).ToList();
        }

        public string SelectUserGroup(int? UserGroupTypesId)
        {
            int grouptype = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            if (grouptype == 8)
            {
                return "Master Company";
            }
            tblUserGroupCompany output = _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => (int?)x.UserGroupId == UserGroupTypesId).FirstOrDefault();
            return output.CompanyNameEN;
        }

        public string GetLabourorDriveNamebyId(int id)
        {
            tblAdminUser output = _context.tblAdminUsers.Where((tblAdminUser x) => x.UserId == id).FirstOrDefault();
            return output.FirstName + " " + output.LastName;
        }

        public int GetUser(int id)
        {
            tblAdminUser output = _context.tblAdminUsers.Where((tblAdminUser x) => x.UserId == id).FirstOrDefault();
            if (output.LabourIsDriver)
            {
                return output.UserId;
            }
            return -1;
        }

        public int GetUserGroupTypeByCompanyId(int id)
        {
            return _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => x.UserGroupId == id).FirstOrDefault().UserGroupTypeId;
        }

        public tblUserGroupCompany GetAllSupplier(int id)
        {
            return _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => x.UserGroupId == id && x.UserGroupTypeId == 2).FirstOrDefault();
        }

        public List<vm_Direction> GetDirection()
        {
            List<vm_Direction> output = new List<vm_Direction>();
            if (isEnglish)
            {
                return _context.tblDirections.Select((tblDirection k) => new vm_Direction
                {
                    Id = k.Id,
                    DirectionName = k.DirectionName
                }).ToList();
            }
            return _context.tblDirections.Select((tblDirection k) => new vm_Direction
            {
                Id = k.Id,
                DirectionName = k.DirectionNameAr
            }).ToList();
        }

        public vm_Direction GetDirection(int Id)
        {
            vm_Direction output = new vm_Direction();
            if (isEnglish)
            {
                return (from k in _context.tblDirections
                        where k.Id == (long)Id
                        select new vm_Direction
                        {
                            Id = k.Id,
                            DirectionName = k.DirectionName
                        }).FirstOrDefault();
            }
            return (from k in _context.tblDirections
                    where k.Id == (long)Id
                    select new vm_Direction
                    {
                        Id = k.Id,
                        DirectionName = k.DirectionNameAr
                    }).FirstOrDefault();
        }

        public vm_Locations GetLocation(int Id)
        {
            vm_Locations output = new vm_Locations();
            if (isEnglish)
            {
                return (from k in _context.tblLocations
                        where k.LocationId == Id
                        select new vm_Locations
                        {
                            LocationId = k.LocationId,
                            LocationNameEN = k.LocationNameEN
                        }).FirstOrDefault();
            }
            return (from k in _context.tblLocations
                    where k.LocationId == Id
                    select new vm_Locations
                    {
                        LocationId = k.LocationId,
                        LocationNameEN = k.LocationNameAR
                    }).FirstOrDefault();
        }

        public List<tblUnit> GetUnit()
        {
            return _context.tblUnits.ToList();
        }

        public tblUserGroupCompany GetAllProvider(int id)
        {
            return _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => x.UserGroupId == id && x.UserGroupTypeId == 1).FirstOrDefault();
        }

        public List<tblUserGroupCompany> GetUserGroupCompanies()
        {
            return _context.tblUserGroupCompanies.ToList();
        }

        public List<SelectListItem> GetSupplierList()
        {
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 2
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplier(int UserGroupId)
        {
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 2 && x.UserGroupId == UserGroupId
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierAdminSupplierList(int UserId)
        {
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 2 && x.AddedBy == UserId
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierList(int serviceid)
        {
            List<SelectListItem> output = new List<SelectListItem>();
            List<int> split = new List<int>();
            tblService dd = _context.tblServices.Find(serviceid);
            if (dd.SupplierId != null)
            {
                split = dd.SupplierId.Split(',').Select(int.Parse).ToList();
            }
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 2
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR),
                        Selected = ((split.Count > 0) ? (split.Contains(x.UserGroupId) ? true : false) : false)
                    }).ToList();
        }

        public List<tblAdminUser> GetUserByGroupId(int Id)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)Id).ToList();
        }

        public List<SelectListItem> GetCategoryList()
        {
            return (from x in _context.Categories
                    where x.Active == true
                    select new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = ((isEnglish == true) ? x.NameEn : x.Name)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierAdminCategoryList(int UserGroupId)
        {
            return (from x in _context.Categories
                    where x.Active == true && x.UserGroupId == (int?)UserGroupId
                    select new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = ((isEnglish == true) ? x.NameEn : x.Name)
                    }).ToList();
        }

        public List<SelectListItem> GetOrderServiceCategoryList(int UserGroupId, List<int?> OrderServiceCategory)
        {
            return (from x in _context.Categories
                    where x.Active == true && x.UserGroupId == (int?)UserGroupId && OrderServiceCategory.Contains(x.Id)
                    select new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = ((isEnglish == true) ? x.NameEn : x.Name)
                    }).ToList();
        }

        public string GetCategoryList(int categoryid)
        {
            List<SelectListItem> output = new List<SelectListItem>();
            List<int> split = new List<int>();
            Category dd = _context.Categories.Find(categoryid);
            if (dd == null)
            {
                dd = _context.Categories.FirstOrDefault();
            }
            string cat = (isEnglish ? dd.NameEn : dd.Name);
            return cat.ToString();
        }

        public List<SelectListItem> GetCategoryList1(int categoryid)
        {
            return (from x in _context.Categories
                    where x.Id == categoryid
                    select new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = ((isEnglish == true) ? x.NameEn : x.Name)
                    }).ToList();
        }

        public string GetEstimatedtime(int serviceid, int GroupTyid)
        {
            tblServiceMapper output = _context.tblServiceMappers.Where((tblServiceMapper x) => x.ServiceId == (int?)serviceid && x.ServiceProviderId == (int?)GroupTyid).FirstOrDefault();
            return output.Estimated;
        }

        public tblProviderTimeSlot GetTimeslot(int orderid)
        {
            tblProviderTimeSlot ProviderTimeSlot = _context.tblProviderTimeSlots.FirstOrDefault((tblProviderTimeSlot x) => x.OrderId == (int?)orderid);
            if (ProviderTimeSlot != null)
            {
                return ProviderTimeSlot;
            }
            return null;
        }

        public bool GetIsworking(int serviceid, int GroupTyid)
        {
            tblServiceMapper output = _context.tblServiceMappers.Where((tblServiceMapper x) => x.ServiceId == (int?)serviceid && x.ServiceProviderId == (int?)GroupTyid).FirstOrDefault();
            return output.IsWorking;
        }

        public List<SelectListItem> GetProviderList()
        {
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 1
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierProviderList(int UserId)
        {
            tblAdminUser SuperAdmin = objUser.GetSuperAdminId();
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 1
                    where x.AddedBy == UserId || x.AddedBy == SuperAdmin.UserId
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR)
                    }).ToList();
        }

        public List<SelectListItem> GetProviderList(int serviceid)
        {
            int UserGroupTypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupTypeId]);
            int UserId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserId]);
            int UserGroupId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_UserGroupId]);
            int ActtypeId = Convert.ToInt32(HttpContext.Current.Session[cls_Defaults.Session_AccountTypeId]);
            List<SelectListItem> output = new List<SelectListItem>();
            List<int> split = new List<int>();
            tblService dd = _context.tblServices.Find(serviceid);
            if (dd.ServiceProviderId != null)
            {
                split = dd.ServiceProviderId.Split(',').Select(int.Parse).ToList();
            }
            tblAdminUser SuperAdmin = objUser.GetSuperAdminId();
            if (UserGroupTypeId == 2 && ActtypeId == 17)
            {
                return (from x in _context.tblUserGroupCompanies
                        where x.UserGroupTypeId == 1
                        where x.AddedBy == UserId || x.AddedBy == SuperAdmin.UserId
                        select new SelectListItem
                        {
                            Value = x.UserGroupId.ToString(),
                            Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR),
                            Selected = ((split.Count > 0) ? (split.Contains(x.UserGroupId) ? true : false) : false)
                        }).ToList();
            }
            return (from x in _context.tblUserGroupCompanies
                    where x.UserGroupTypeId == 1
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameAR),
                        Selected = ((split.Count > 0) ? (split.Contains(x.UserGroupId) ? true : false) : false)
                    }).ToList();
        }

        public string GetProviderOrSupplierByServiceId(int serviceid, int UserGroupTypeId)
        {
            string output = " -- ";
            List<int> ids = new List<int>();
            try
            {
                tblService dd = _context.tblServices.Find(serviceid);
                if (dd.ServiceProviderId != null || dd.SupplierId != null)
                {
                    if (dd.ServiceProviderId != null && UserGroupTypeId == 1)
                    {
                        ids = dd.ServiceProviderId.Split(',').Select(int.Parse).ToList();
                    }
                    if (dd.SupplierId != null && UserGroupTypeId == 2)
                    {
                        ids = dd.SupplierId.Split(',').Select(int.Parse).ToList();
                    }
                    List<tblUserGroupCompany> data = _context.tblUserGroupCompanies.Where((tblUserGroupCompany x) => x.UserGroupTypeId == UserGroupTypeId && ids.Contains(x.UserGroupId)).ToList();
                    if (data.Count > 0)
                    {
                        output = (isEnglish ? data.Select((tblUserGroupCompany k) => k.CompanyNameEN).Aggregate((string a, string b) => a + "," + b) : data.Select((tblUserGroupCompany k) => k.CompanyNameAR).Aggregate((string a, string b) => a + "," + b));
                        return output;
                    }
                    return output;
                }
                return output;
            }
            catch (Exception)
            {
                return output;
            }
        }

        public Page<tblTeamCapacityCalculation> GetTeamCapacity(int pageSize, int currentPage, Sorts<tblTeamCapacityCalculation> sorts, Filters<tblTeamCapacityCalculation> filters, List<int?> SpList)
        {
            return (from x in _context.tblTeamCapacityCalculations
                    orderby x.Updatedate descending
                    select x into o
                    where SpList.Contains(o.ServiceProviderId)
                    select o).Paginate(currentPage, pageSize, sorts, filters);
        }
    }
}