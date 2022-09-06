using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Almanea;
using Almanea.BusinessLogic;
using Almanea.Models;
using Newtonsoft.Json;

namespace Almanea.Controllers
{
    [SiteAuthorize("Admin", "SuperAdmin","Supplier")]
    public class CategoriesController : BaseController
    {
        private AlmaneaDbEntities db = new AlmaneaDbEntities();

        // GET: Categories
        public ActionResult Index()
        {
            Category model = new Category();
            //var categories = db.Categories.Include(c => c.Category2);
            //ViewBag.Data= categories.ToList();
            return View(model);
        }
        public JsonResult GetCategoriesList()
        {

            var objResult = new vm_Result();
            var UserGroupId = Session[cls_Defaults.Session_UserGroupId];
            int userGroupTypeId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupTypeId]);
            int accountTypeId = Convert.ToInt32(Session[cls_Defaults.Session_AccountTypeId]);

            var categories = new List<Category>();
            if (userGroupTypeId == (int)enumGroupType.Supplier && accountTypeId == (int)enumSupplierAcct.Admin)
            {
                categories = db.Categories.Include(c => c.Category2).Where(c=> c.UserGroupId == (int)UserGroupId).ToList();
            }
            else
            {
                categories = db.Categories.Include(c => c.Category2).ToList();
            }
                
            var data = (from obj in categories select new { Name = obj.Name, NameEn = obj.NameEn, Description = obj.Description
            ,DescriptionEn=obj.DescriptionEn,Active=obj.Active,SortOrder=obj.SortOrder,Id=obj.Id
            });
            objResult.Data = data;
            objResult.Count = data.Count();
            return Json(
                 new
                 {
                     aaData = objResult.Data,
                     //sEcho = model.sEcho,
                     iTotalRecords = objResult.Count,
                     iTotalDisplayRecords = objResult.Count
                 }
                 , JsonRequestBehavior.AllowGet);
        }
        // GET: Categories/Details/5
        public ActionResult Details(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create
        //public ActionResult Create()
        //{
        //    ViewBag.ParentId = new SelectList(db.Categories, "Id", "Name");
        //    return View();
        //}

        // POST: Categories/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description,NameEn,DescriptionEn,ParentId,HasChild,Thumb,Picture,ThumbEn,PictureEn,Active,SortOrder")] Category category)
        {
            if (ModelState.IsValid)
            {
                var UserGroupId = Session[cls_Defaults.Session_UserGroupId];
                category.UserGroupId = Convert.ToInt32(UserGroupId);
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.ParentId = new SelectList(db.Categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            ViewBag.ParentId = new SelectList(db.Categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,NameEn,DescriptionEn,ParentId,HasChild,Thumb,Picture,ThumbEn,PictureEn,Active,SortOrder")] Category category)
        {
            if (ModelState.IsValid)
            {
                var userGroupId = Convert.ToInt32(Session[cls_Defaults.Session_UserGroupId]);
                category.UserGroupId = userGroupId;
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.ParentId = new SelectList(db.Categories, "Id", "Name", category.ParentId);
            return View(category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(short? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(short id)
        {
            Category category = db.Categories.Find(id);
            db.Categories.Remove(category);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
