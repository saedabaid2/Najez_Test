using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Almanea.BusinessLogic;
using Almanea.Models;
using AutoMapper;
using EntityFrameworkPaginate;

namespace Almanea.Data
{

    public class db_User : cls_Dispose
    {
        private AlmaneaDbEntities _context;

        public bool isEnglish = cls_Defaults.IsEnglish;

        public db_User()
        {
            _context = new AlmaneaDbEntities();
        }

        protected override void Dispose(bool disposing)
        {
            _context.Dispose();
        }

        public async Task<int> AddGroup(vm_GroupCompanies model)
        {
            tblUserGroupCompany inputs = Mapper.Map<vm_GroupCompanies, tblUserGroupCompany>(model);
            inputs.IsInternal = model.IsInternal;
            _context.tblUserGroupCompanies.Add(inputs);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(model.CompanyLogo))
            {
                (await _context.tblUserGroupCompanies.SingleOrDefaultAsync((tblUserGroupCompany x) => x.UserGroupId == inputs.UserGroupId)).CompanyLogo = inputs.UserGroupId + model.CompanyLogo;
                await _context.SaveChangesAsync();
            }
            return inputs.UserGroupId;
        }

        public async Task<int> EditGroup(vm_GroupCompanies model)
        {
            tblUserGroupCompany group = await _context.tblUserGroupCompanies.SingleOrDefaultAsync((tblUserGroupCompany x) => x.UserGroupId == model.UserGroupId);
            group.CompanyNameEN = model.CompanyNameEN;
            group.CompanyNameAR = model.CompanyNameAR;
            group.Email = model.Email;
            group.Fax = model.Fax;
            group.Telephone = model.Telephone;
            group.UserGroupTypeId = model.UserGroupTypeId;
            group.Status = model.Status;
            group.IsInternal = model.IsInternal;
            group.Contract = model.Contract;
            if (!string.IsNullOrEmpty(model.CompanyLogo))
            {
                group.CompanyLogo = model.UserGroupId + model.CompanyLogo;
            }
            await _context.SaveChangesAsync();
            return model.UserGroupId;
        }

        public Page<tblUserGroupCompany> GetGroups(int pageSize, int currentPage, Filters<tblUserGroupCompany> filters, Sorts<tblUserGroupCompany> sorts)
        {
            return _context.tblUserGroupCompanies.Paginate(currentPage, pageSize, sorts, filters);
        }

        public tblAdminUser GetSuperAdminId()
        {
            return _context.tblAdminUsers.Where((tblAdminUser u) => (int?)u.UserGroupTypeId == (int?)8).FirstOrDefault();
        }

        public List<int> GetSuperAdminSPId(int SuperAdminId)
        {
            return (from u in _context.tblAdminUsers
                    where u.AddedBy == SuperAdminId
                    select u into x
                    select x.UserId).ToList();
        }

        public Page<tblUserGroupCompany> GetSupplierAdminGroups(int pageSize, int currentPage, Filters<tblUserGroupCompany> filters, Sorts<tblUserGroupCompany> sorts, int UserId)
        {
            return _context.tblUserGroupCompanies.Paginate(currentPage, pageSize, sorts, filters);
        }

        public async Task<vm_GroupCompanies> GetGroupById(int Id)
        {
            return Mapper.Map<tblUserGroupCompany, vm_GroupCompanies>(await _context.tblUserGroupCompanies.FirstOrDefaultAsync((tblUserGroupCompany x) => x.UserGroupId == Id));
        }

        public async Task<string> GetGroupName(int Id)
        {
            tblUserGroupCompany group = await _context.tblUserGroupCompanies.FirstOrDefaultAsync((tblUserGroupCompany x) => x.UserGroupId == Id);
            if (isEnglish)
            {
                return group.CompanyNameEN;
            }
            return group.CompanyNameAR;
        }

        public List<SelectListItem> GetGroupByType(int Id, int groupid)
        {
            if (Id == 7 || groupid == 2)
            {
                return (from x in _context.tblUserGroupCompanies
                        where x.Status == true && x.UserGroupTypeId == groupid
                        select new SelectListItem
                        {
                            Value = x.UserGroupId.ToString(),
                            Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameEN)
                        }).ToList();
            }
            return (from x in _context.tblUserGroupCompanies
                    where x.Status == true && x.UserGroupTypeId == Id
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameEN)
                    }).ToList();
        }

        public List<SelectListItem> GetSupplierGroupByType(int Id, int UserId)
        {
            return (from x in _context.tblUserGroupCompanies
                    where x.Status == true && x.UserGroupTypeId == Id && x.AddedBy == UserId
                    select new SelectListItem
                    {
                        Value = x.UserGroupId.ToString(),
                        Text = ((isEnglish == true) ? x.CompanyNameEN : x.CompanyNameEN)
                    }).ToList();
        }

        public List<SelectListItem> GetAccountTypeSP(int Id)
        {
            List<SelectListItem> model = new List<SelectListItem>();
            if (Id == 1)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            if (Id == 10)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            return (from x in _context.tblAccountTypes
                    where (x.IsActive == (bool?)true && x.AccountTypeId == 11) || x.AccountTypeId == 10
                    select new SelectListItem
                    {
                        Value = x.AccountTypeId.ToString(),
                        Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                    }).ToList();
        }

        public List<SelectListItem> GetAccountTypeSPAdmin(int Id)
        {
            List<SelectListItem> model = new List<SelectListItem>();
            if (Id == 1)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id && x.TitleEN == "Admin"
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            return (from x in _context.tblAccountTypes
                    where (x.IsActive == (bool?)true && x.AccountTypeId == 11) || x.AccountTypeId == 10
                    select new SelectListItem
                    {
                        Value = x.AccountTypeId.ToString(),
                        Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                    }).ToList();
        }

        public List<SelectListItem> GetAccountTypeSupplier(int Id)
        {
            List<SelectListItem> model = new List<SelectListItem>();
            if (Id == 2)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            if (Id == 1)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            if (Id == 10)
            {
                return (from x in _context.tblAccountTypes
                        where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                        select new SelectListItem
                        {
                            Value = x.AccountTypeId.ToString(),
                            Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                        }).ToList();
            }
            return (from x in _context.tblAccountTypes
                    where (x.IsActive == (bool?)true && x.AccountTypeId == 11) || x.AccountTypeId == 10
                    select new SelectListItem
                    {
                        Value = x.AccountTypeId.ToString(),
                        Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                    }).ToList();
        }

        public List<SelectListItem> GetAccountType(int Id)
        {
            List<SelectListItem> model = new List<SelectListItem>();
            if (Id == 2)
            {
                model = cls_DropDowns.DDL_SupplierAccountTypes();
            }
            else if (Id == 1)
            {
                model = cls_DropDowns.DDL_ServiceProviderAccountTypes();
            }
            else if (Id == 10)
            {
                model = cls_DropDowns.DDL_UserAccountTypes();
            }
            else if (Id == 10)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.AccountTypeId == Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
            }
            else if (Id == 11)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.AccountTypeId == Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
            }
            else if (Id == 6)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
            }
            else if (Id == 3)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
                if (model.Count == 0)
                {
                    model = cls_DropDowns.DDL_AccountTypes(1);
                }
            }
            else if (Id == 7)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
                if (model.Count == 0)
                {
                    model = cls_DropDowns.DDL_AccountTypes(1);
                }
            }
            else if (Id == 9)
            {
                model = (from x in _context.tblAccountTypes
                         where x.IsActive == (bool?)true && x.GroupTypeId == (int?)Id
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
                if (model.Count == 0)
                {
                    model = cls_DropDowns.DDL_AccountTypes(1);
                }
            }
            else
            {
                model = (from x in _context.tblAccountTypes
                         where (x.IsActive == (bool?)true && x.AccountTypeId == 11) || x.AccountTypeId == 10
                         select new SelectListItem
                         {
                             Value = x.AccountTypeId.ToString(),
                             Text = ((isEnglish == true) ? x.TitleEN : x.TitleAR)
                         }).ToList();
            }
            return model;
        }

        public async Task<string> GetAccountName(int Id)
        {
            if (Id > 0)
            {
                tblAccountType group = await _context.tblAccountTypes.FirstOrDefaultAsync((tblAccountType x) => x.AccountTypeId == Id);
                if (isEnglish)
                {
                    return group.TitleEN;
                }
                return group.TitleAR;
            }
            return "";
        }

        public bool GroupEmailExists(string Email, int? Id)
        {
            tblUserGroupCompany userGroup = ((!Id.HasValue) ? _context.tblUserGroupCompanies.SingleOrDefault((tblUserGroupCompany x) => x.Email.Equals(Email)) : _context.tblUserGroupCompanies.SingleOrDefault((tblUserGroupCompany x) => x.Email.Equals(Email) && (int?)x.UserGroupId != Id));
            if (userGroup != null)
            {
                return true;
            }
            return false;
        }

        public Tuple<int, string> DeleteGroupImage(int Id)
        {
            tblUserGroupCompany userGroup = _context.tblUserGroupCompanies.SingleOrDefault((tblUserGroupCompany x) => x.UserGroupId == Id);
            string logo = userGroup.CompanyLogo;
            userGroup.CompanyLogo = null;
            _context.SaveChanges();
            return new Tuple<int, string>(1, logo);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (HMACSHA512 hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }
        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (HMACSHA512 hmac = new HMACSHA512(passwordSalt))
            {
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static tblAdminUser FirstUser()
        {
            vm_User user = new vm_User
            {
                FirstName = "Master",
                LastName = "Admin",
                Email = "admin@syanah.com",
                AccountTypeId = 1,
                UserGroupTypeId = 3,
                IqaamaNo = "",
                MobileNo = "00",
                ProfilePic = "",
                UserGroupId = null
            };
            tblAdminUser inputs = Mapper.Map<vm_User, tblAdminUser>(user);
            db_Settings objSettings = new db_Settings();
            inputs.EncryptedPassword = objSettings.EncryptString("12345678", useHashing: false);
            return inputs;
        }

        public async Task<int> AddUser(vm_User model)
        {
            tblAdminUser inputs = Mapper.Map<vm_User, tblAdminUser>(model);
            db_Settings objSettings = new db_Settings();
            objSettings.EncryptString(model.Password, useHashing: false);
            inputs.EncryptedPassword = objSettings.EncryptString(model.Password, useHashing: false);
            _context.tblAdminUsers.Add(inputs);
            await _context.SaveChangesAsync();
            if (model.AccountTypeId == 10)
            {
                db_Settings obj = new db_Settings();
                obj.AddCapacity(model.UserGroupId);
            }
            if (!string.IsNullOrEmpty(model.ProfilePic))
            {
                (await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == inputs.UserId)).ProfilePic = inputs.UserId + model.ProfilePic;
                await _context.SaveChangesAsync();
            }
            return inputs.UserId;
        }

        public tblAdminUser getactievsp(int? providerid)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == providerid).FirstOrDefault();
        }

        public async Task<int> EditUser(vm_User model)
        {
            tblAdminUser group = await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == model.UserId);
            string tempLogo = group.ProfilePic;
            group.FirstName = model.FirstName;
            group.LastName = model.LastName;
            group.Email = model.Email;
            group.MobileNo = model.MobileNo;
            group.IqaamaNo = model.IqaamaNo;
            group.AccountTypeId = model.AccountTypeId;
            group.UserGroupTypeId = model.UserGroupTypeId;
            group.LabourIsDriver = model.LabourIsDriver;
            if (model.UserGroupId > 0)
            {
                group.UserGroupId = model.UserGroupId;
            }
            group.Status = model.StatusId;
            if (!string.IsNullOrEmpty(model.ProfilePic))
            {
                group.ProfilePic = model.UserId + model.ProfilePic;
            }
            else
            {
                group.ProfilePic = tempLogo;
            }
            await _context.SaveChangesAsync();
            if (model.AccountTypeId == 10)
            {
                tblLaborInactive blockdatelabor = _context.tblLaborInactives.Where((tblLaborInactive x) => x.LabourId == (int?)model.UserId).FirstOrDefault();
                if (blockdatelabor == null)
                {
                    tblLaborInactive obj2 = new tblLaborInactive();
                    obj2.LabourId = model.UserId;
                    obj2.ProviderId = Convert.ToInt32(model.UserGroupId);
                    obj2.InactiveDates = model.LaborBlockDate;
                    obj2.UpdateDate = DateTime.UtcNow;
                    _context.tblLaborInactives.Add(obj2);
                    _context.SaveChanges();
                }
                else
                {
                    blockdatelabor.LabourId = model.UserId;
                    blockdatelabor.ProviderId = Convert.ToInt32(model.UserGroupId);
                    blockdatelabor.InactiveDates = model.LaborBlockDate;
                    blockdatelabor.UpdateDate = DateTime.UtcNow;
                    _context.Entry(blockdatelabor).State = EntityState.Modified;
                    _context.SaveChanges();
                }
            }
            if (model.AccountTypeId == 10)
            {
                db_Settings obj = new db_Settings();
                int? providerId = group.UserGroupId;
                obj.EditCapacity(providerId, model.StatusId);
            }
            return model.UserId;
        }

        public Page<tblAdminUser> GetUsers(int pageSize, int currentPage, Filters<tblAdminUser> filters, Sorts<tblAdminUser> sorts)
        {
            return _context.tblAdminUsers.Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<tblAdminUser> GetServiceProviderUsers(int pageSize, int currentPage, Filters<tblAdminUser> filters, Sorts<tblAdminUser> sorts, int UserGroupId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.UserGroupId == (int?)UserGroupId).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<Inventory_Master> GetInventory(int pageSize, int currentPage, Filters<Inventory_Master> filters, Sorts<Inventory_Master> sorts, int UserGroupId = 0, int LabourId = 0, DateTime? Date = null)
        {
            if (!Date.HasValue && LabourId != 0)
            {
                return (from x in _context.Inventory_Master
                        where x.LabourId == (int?)LabourId && x.UserGroupID == (int?)UserGroupId
                        orderby x.StartDate descending
                        select x).Paginate(currentPage, pageSize, sorts, filters);
            }
            if (Date != DateTime.MinValue && LabourId != 0)
            {
                return (from x in _context.Inventory_Master
                        where x.StartDate <= Date && x.EndDate >= Date && x.LabourId == (int?)LabourId && x.UserGroupID == (int?)UserGroupId
                        orderby x.StartDate descending
                        select x).Paginate(currentPage, pageSize, sorts, filters);
            }
            if (Date != DateTime.MinValue && Date.HasValue && LabourId == 0)
            {
                return (from x in _context.Inventory_Master
                        where x.StartDate <= Date && x.EndDate >= Date && x.UserGroupID == (int?)UserGroupId
                        orderby x.StartDate descending
                        select x).Paginate(currentPage, pageSize, sorts, filters);
            }
            return (from x in _context.Inventory_Master
                    where x.UserGroupID == (int?)UserGroupId
                    orderby x.StartDate descending
                    select x).Paginate(currentPage, pageSize, sorts, filters);
        }

        public Page<Inventoy_Details> GetInventoryDetails(int pageSize, int currentPage, Filters<Inventoy_Details> filters, Sorts<Inventoy_Details> sorts, int Id)
        {
            return (from x in _context.Inventoy_Details
                    where x.Master_Id == (int?)Id
                    orderby x.Id
                    select x).Paginate(currentPage, pageSize, sorts, filters);
        }

        public async Task<vm_User> GetUserById(int Id)
        {
            return Mapper.Map<tblAdminUser, vm_User>(await _context.tblAdminUsers.Include((tblAdminUser c) => c.tblLaborInactives).FirstOrDefaultAsync((tblAdminUser x) => x.UserId == Id));
        }

        public tblAdminUser GetUser(int Id)
        {
            return _context.tblAdminUsers.FirstOrDefault((tblAdminUser x) => x.UserId == Id);
        }

        public bool UserEmailExists(string Email, int? Id)
        {
            tblAdminUser userGroup = ((!Id.HasValue) ? _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.Email.Equals(Email)) : _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.Email.Equals(Email) && (int?)x.UserId != Id));
            if (userGroup != null)
            {
                return true;
            }
            return false;
        }

        public bool UserMobileNoExists(string MobileNo, int? Id)
        {
            tblAdminUser userGroup = ((!Id.HasValue) ? _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.MobileNo.Equals(MobileNo)) : _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.MobileNo.Equals(MobileNo) && (int?)x.UserId != Id));
            if (userGroup != null)
            {
                return true;
            }
            return false;
        }

        public bool CheckUserName(string Email)
        {
            tblAdminUser userGroup = _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.Email.Equals(Email) && x.Status == true);
            if (userGroup != null)
            {
                return true;
            }
            return false;
        }

        public async Task<bool> CheckPassword(string Email, string Password)
        {
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email) && x.Status == true);
            db_Settings objSettings = new db_Settings();
            if (Password == objSettings.DecryptString(user.EncryptedPassword, useHashing: false))
            {
                return true;
            }
            return false;
        }

        public async Task<tblAdminUser> Login(string Email, string Password)
        {
            db_Settings objSettings = new db_Settings();
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email) && x.Status == true);
            if (user == null)
            {
                return null;
            }
            if (Password != objSettings.DecryptString(user.EncryptedPassword, useHashing: false))
            {
                return null;
            }
            return user;
        }

        public async Task<tblAdminUser> GetCompanyImage(string Email)
        {
            new db_Settings();
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email) && x.Status == true);
            if (user == null)
            {
                return null;
            }
            return user;
        }

        public async Task<vm_StatusInfo> ForgotPassword(string Email)
        {
            vm_StatusInfo response = new vm_StatusInfo();
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email));
            if (user == null)
            {
                response.Status = "Failure";
                response.StatusCode = 404;
                response.Message = "User Not Found";
            }
            else
            {
                string password = cls_Defaults.GenerateCode(8);
                await UpdatePassword(user.UserId, password);
                if (user.Status)
                {
                    await cls_Sms.ForgotPassword(user.MobileNo, password);
                }
                response.Status = "Success";
                response.StatusCode = 200;
                response.Message = "Successfully Send OTP";
            }
            return response;
        }

        public async Task<vm_StatusInfo> ResetPassword(string Email, string PasswordOTP, string NewPassword)
        {
            db_Settings objSettings = new db_Settings();
            vm_StatusInfo response = new vm_StatusInfo();
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email) && x.Status == true);
            if (user == null)
            {
                response.Status = "Failure";
                response.StatusCode = 404;
                response.Message = "User Not Found";
            }
            else if (PasswordOTP != objSettings.DecryptString(user.EncryptedPassword, useHashing: false))
            {
                response.Status = "Failure";
                response.StatusCode = 400;
                response.Message = "Invalid OTP";
            }
            else
            {
                await UpdatePassword(user.UserId, NewPassword);
                response.Status = "Success";
                response.StatusCode = 200;
                response.Message = "Successfully Update Password";
            }
            return response;
        }

        public async Task<int> Forgot(string Email)
        {
            tblAdminUser user = await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.Email.Equals(Email));
            if (user == null)
            {
                return -1;
            }
            string password = cls_Defaults.GenerateCode(8);
            if (user.AccountTypeId == 1)
            {
                password = "12345678";
            }
            await UpdatePassword(user.UserId, password);
            if (user.Status)
            {
                await cls_Sms.ForgotPassword(user.MobileNo, password);
            }
            return 1;
        }

        public async Task<int> UpdatePassword(int Id, string Password)
        {
            tblAdminUser group = await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == Id);
            db_Settings objSettings = new db_Settings();
            group.EncryptedPassword = objSettings.EncryptString(Password, useHashing: false);
            await _context.SaveChangesAsync();
            return Id;
        }

        public async Task<int> UpdateUserLoginStatus(int Id)
        {
            tblAdminUser group = await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == Id);
            group.IsLogin = true;
            group.LastLoggedIn = DateTime.Now;
            await _context.SaveChangesAsync();
            return Id;
        }

        public async Task<int> UpdateUserLogoutStatus(int Id)
        {
            (await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == Id)).IsLogin = false;
            await _context.SaveChangesAsync();
            return Id;
        }

        public Tuple<int, string> DeleteUserImage(int Id)
        {
            tblAdminUser model = _context.tblAdminUsers.SingleOrDefault((tblAdminUser x) => x.UserId == Id);
            string logo = model.ProfilePic;
            model.ProfilePic = null;
            _context.SaveChanges();
            return new Tuple<int, string>(1, logo);
        }

        public async Task<List<tblAdminUser>> GetUserByTypeId(int Id)
        {
            return await _context.tblAdminUsers.Where((tblAdminUser x) => (int?)x.UserGroupTypeId == (int?)Id).ToListAsync();
        }

        public async Task<List<tblAdminUser>> GetServiceProvider(int?[] basic)
        {
            List<int> accounttypeid = new List<int> { 6, 7 };
            return await _context.tblAdminUsers.Where((tblAdminUser x) => accounttypeid.Contains(x.AccountTypeId) && basic.Contains(x.UserGroupId)).ToListAsync();
        }

        public string SaveOrderDiplay(int ordrid, int agentid)
        {
            int? provider = _context.tblAdminUsers.Where((tblAdminUser x) => x.UserId == agentid).FirstOrDefault().UserGroupId;
            OrderDisplay obj = new OrderDisplay();
            obj.AjentId = agentid;
            obj.IsMessasgeSent = true;
            obj.IsOrderReserved = false;
            obj.OrderId = ordrid;
            obj.ProviderId = provider;
            obj.ReservedBy = null;
            obj.Datetime = DateTime.UtcNow;
            _context.OrderDisplays.Add(obj);
            _context.SaveChanges();
            return "Saved success";
        }

        public List<int> CheckAssignLabours(int OrderId, DateTime InstallDate)
        {
            return (from x in _context.tblOrders
                    where (x.OrderId != OrderId) & (x.InstallDate == InstallDate)
                    select x.LabourId).ToList();
        }

        public object CheckDriveravailabilty(int UserId, DateTime InstallDate)
        {
            return _context.tblOrders.Where((tblOrder x) => (x.DriverId == (int?)UserId || x.LabourId == UserId) && x.InstallDate == InstallDate).FirstOrDefault();
        }

        public async Task<List<tblAdminUser>> GetDrivers(int UserGroupId)
        {
            return await _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 11).ToListAsync();
        }

        public List<tblAdminUser> GetAvailDrivers(int UserGroupId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 11).ToList();
        }

        public List<tblAdminUser> GetAvailLabourAndDrivers(int UserGroupId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.LabourIsDriver == true).ToList();
        }

        public List<tblAdminUser> GetServiceProvider(int UserGroupId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && (x.AccountTypeId == 7 || x.AccountTypeId == 6) && x.Status == true).ToList();
        }

        public async Task<List<tblAdminUser>> GetLaboursAndDrivers(int UserGroupId)
        {
            return await _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 10 && x.LabourIsDriver == true).ToListAsync();
        }

        public async Task<List<tblAdminUser>> GetLabours(int UserGroupId)
        {
            return await _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 10).Take(4).ToListAsync();
        }

        public async Task<List<tblAdminUser>> GetAssignLabours(int UserGroupId, DateTime InstallDate, int orderid)
        {
            tblProviderTimeSlot timeslotassigned = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)orderid).FirstOrDefault();
            var records = new[]
            {
            new
            {
                LabourId = ""
            }
        }.ToList();
            List<tblOrder> Labours = _context.tblOrders.Where((tblOrder x) => (x.ReservedProvider == UserGroupId) & (x.InstallDate == InstallDate)).ToList();
            List<int> list = new List<int>();
            foreach (tblOrder item in Labours)
            {
                tblProviderTimeSlot getassigntimeslot = _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.OrderId == (int?)item.OrderId).FirstOrDefault();
                if (getassigntimeslot != null && timeslotassigned != null && timeslotassigned.StartHour.HasValue && timeslotassigned.StartHour == getassigntimeslot.StartHour && timeslotassigned.EndHour == getassigntimeslot.EndHour)
                {
                    records.Add(new
                    {
                        LabourId = item.LabourId.ToString()
                    });
                    list.Add(item.LabourId);
                }
            }
            records.Skip(1);
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && !list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public List<Item> GetItems2()
        {
            return _context.Items.ToList();
        }

        public List<tblAdminUser> GetAavilableLabours(int OrderId = 0)
        {
            IEnumerable<int?> laboursCapacity = from c in _context.LabourCapacities.Where((LabourCapacity x) => x.OrderId == (int?)OrderId && x.CapcityPercentage == (decimal?)100m).ToList()
                                                select c.LabourId;
            List<int> list = new List<int>();
            foreach (int? item in laboursCapacity)
            {
                list.Add(Convert.ToInt32(item));
            }
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && !list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public List<tblAdminUser> GetAavilableLabours2(int OrderId = 0, int UsergroupId = 0)
        {
            IEnumerable<int?> laboursCapacity = from c in _context.LabourCapacities.Where((LabourCapacity x) => x.OrderId == (int?)OrderId && x.CapcityPercentage == (decimal?)100m).ToList()
                                                select c.LabourId;
            List<int> list = new List<int>();
            foreach (int? item in laboursCapacity)
            {
                list.Add(Convert.ToInt32(item));
            }
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && !list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true && x.UserGroupId == (int?)UsergroupId).ToList();
        }

        public List<tblAdminUser> GetAavilableLabours(int UserGroupId, DateTime InstallDate, int starthrs, int OrderId = 0)
        {
            IEnumerable<int?> timeslotassigned = from c in _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.StartHour == (int?)starthrs && x.InstallDate == InstallDate).ToList()
                                                 select c.LabourId;
            IEnumerable<int?> laboursCapacity = from c in _context.LabourCapacities.Where((LabourCapacity x) => x.OrderId == (int?)OrderId && x.InstallDate == InstallDate && x.CapcityPercentage == (decimal?)100m).ToList()
                                                select c.LabourId;
            List<int> list = new List<int>();
            foreach (int? item2 in timeslotassigned)
            {
                list.Add(Convert.ToInt32(item2));
            }
            foreach (int? item in laboursCapacity)
            {
                list.Add(Convert.ToInt32(item));
            }
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && !list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public List<tblAdminUser> GetAssignedLabours(int UserGroupId, DateTime InstallDate, int starthrs, int OrderId = 0)
        {
            IEnumerable<int?> OrdersAssigneds = from c in _context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)OrderId).ToList()
                                                select c.LabourId;
            List<int> list = new List<int>();
            foreach (int? item in OrdersAssigneds)
            {
                list.Add(Convert.ToInt32(item));
            }
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public List<tblAdminUser> GetAavilableLaboursBasedOnDate(int UserGroupId, DateTime InstallDate, int starthrs)
        {
            IEnumerable<int?> timeslotassigned = from c in _context.tblProviderTimeSlots.Where((tblProviderTimeSlot x) => x.InstallDate == InstallDate).ToList()
                                                 select c.LabourId;
            List<int> list = new List<int>();
            foreach (int? item in timeslotassigned)
            {
                list.Add(Convert.ToInt32(item));
            }
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && !list.Contains(x.UserId) && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public List<tblAdminUser> GetLaboursDopr(int UserGroupId)
        {
            return _context.tblAdminUsers.Where((tblAdminUser x) => x.Status == true && x.UserGroupId == (int?)UserGroupId && x.AccountTypeId == 10 && x.Status == true).ToList();
        }

        public async Task<tblOrder> GetOrderById(int Id)
        {
            return await _context.tblOrders.FirstOrDefaultAsync((tblOrder x) => x.OrderId == Id && x.Status != 11);
        }

        public async Task<List<OrdersAssigned>> GetAssignedOrderById(int Id)
        {
            return await _context.OrdersAssigneds.Where((OrdersAssigned x) => x.OrderId == (int?)Id).ToListAsync();
        }

        public async Task<tblAdminUser> GetUsertbl(int Id)
        {
            return await _context.tblAdminUsers.FirstOrDefaultAsync((tblAdminUser x) => x.UserId == Id);
        }

        public async Task<int> UpdateUserPassword(int Id, string CurrentPassword, string newPassword)
        {
            db_Settings objSettings = new db_Settings();
            tblAdminUser group = await _context.tblAdminUsers.SingleOrDefaultAsync((tblAdminUser x) => x.UserId == Id);
            if (group == null)
            {
                return 0;
            }
            if (CurrentPassword != objSettings.DecryptString(group.EncryptedPassword, useHashing: false))
            {
                return -1;
            }
            group.EncryptedPassword = objSettings.EncryptString(newPassword, useHashing: false);
            await _context.SaveChangesAsync();
            return Id;
        }

        public int GetUserIsActive(int Id)
        {
            tblAdminUser users = _context.tblAdminUsers.Where((tblAdminUser x) => x.UserId == Id && x.Status == true).FirstOrDefault();
            if (users != null)
            {
                return 1;
            }
            return 0;
        }
    }
}