using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Data.Entity.Infrastructure;

namespace XenERP.Controllers
{
    public class GroupController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        //
        // GET: /Group/

        public ActionResult Index()
        {
            int companyid = Convert.ToInt32(Session["companyid"]);
            return View(db.Groups.Where(d=>d.CompanyId==companyid).ToList());
        }

        //
        // GET: /Group/Details/5

        public ActionResult Details(int id = 0)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        //
        // GET: /Group/Create

        public ActionResult Create(string Msg, string Err, string id)
        {
            int groupid = Convert.ToInt32(id);
            long companyid = Convert.ToInt32(Session["companyid"]);
            int userid = Convert.ToInt32(Session["userid"]);

            long Branchid = Convert.ToInt64(Session["BranchId"]);

            List<Group> userList = new List<Group>();
            userList = db.Groups.Where(g => g.CompanyId == companyid).ToList();
            var presidents = userList.
                             Where(x => x.ParentGroupId == null).ToList();
            foreach (var president in presidents)
            {
                SetChildren(president, userList);
            }
            ViewBag.President = presidents;
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            Group group = new Group();

            if (id != null)
            {
                int rateid = Convert.ToInt32(id);

                var groups = db.Groups.Where(d => d.Id == groupid).FirstOrDefault();
                ViewBag.count = 0;
                group.Code = groups.Code;
                group.Name = groups.Name;
                group.ParentGroupId = groups.ParentGroupId;
                try
                {
                    string parentname = db.Groups.FirstOrDefault(d => d.Id == groups.ParentGroupId).Name;
                    ViewBag.parent = parentname;
                }
                catch { }
                ViewBag.count = 0;
             
            }
            else
            {

                group.Name = null;
                group.ParentGroupId = 0;
                
            }


            ViewBag.DdlGroups = db.Groups.Where(d => d.BranchId == Branchid && d.CompanyId == companyid).OrderBy(d=>d.Name);

         

            ViewBag.Groups = db.Groups.Where(d=>d.CompanyId==companyid).ToList();
            ViewBag.Message = Msg;
            return View(group);
        }


        private void SetChildren(Group model, List<Group> userList)
        {
            var childs = userList.
                            Where(x => x.ParentGroupId == model.Id).ToList();
            if (childs.Count > 0)
            {
                foreach (var child in childs)
                {
                    SetChildren(child, userList);
                    model.Group1.Add(child);
                }
            }
        }
        //
        // POST: /Group/Create

        [HttpPost]
        public ActionResult Create(Group group,FormCollection collection)
        {
            //if (ModelState.IsValid)
            //{
                int Createdby = Convert.ToInt32(Session["Createdid"]);
              
                ViewBag.Groups = db.Groups.ToList();

                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                int userid = Convert.ToInt32(Session["userid"]);

                try
                {
              
                    if (group.Id == 0)
                    {

                      
                        if (group.ParentGroupId>0)
                        {
                            var pc = Convert.ToString(collection["parentcode"]).Trim();
                            var pn = Convert.ToString(collection["parentname"]).Trim();
                            group.Code =pc  + "-" + group.Code.Trim();
                            group.Name =pn + " " + group.Name.Trim();

                            ViewBag.IsParent = "Child";
                            ViewBag.ParentCode = collection["parentcode"];
                            ViewBag.ParentName = collection["parentname"];
                        }
                        else
                        {
                            ViewBag.IsParent = "Parent";
                            group.ParentGroupId = null;
                            group.Code = group.Code.Trim();
                            group.Name = group.Name.Trim();
                            
                        }

                       
                        var checkcodeDuplicate = db.Groups.Any(c => c.Code == group.Code && c.CompanyId == companyid);
                        if (checkcodeDuplicate)
                        {
                            //ViewBag.ErrorType = "Code";
                            List<Group> userList = new List<Group>();
                            userList = db.Groups.Where(g => g.CompanyId == companyid).ToList();
                            var presidents = userList.
                                             Where(x => x.ParentGroupId == null).ToList();
                            foreach (var president in presidents)
                            {
                                SetChildren(president, userList);
                            }
                            ViewBag.President = presidents;
                            ViewBag.Message = "Group Code already exists...";
                            return View(group);
                        }

                        var checkDuplicate = db.Groups.Any(c => c.Name == group.Name && c.CompanyId == companyid);
                        if (checkDuplicate)
                        {
                            List<Group> userList = new List<Group>();
                            userList = db.Groups.Where(g => g.CompanyId == companyid).ToList();
                            var presidents = userList.
                                             Where(x => x.ParentGroupId == null).ToList();
                            foreach (var president in presidents)
                            {
                                SetChildren(president, userList);
                            }
                            ViewBag.President = presidents;
                           ViewBag.Message = "Group Name already exists...";
                           return View(group);
                            
                        }

                       
                       
                            group.CreatedOn = DateTime.Now;
                            group.CreatedBy = Createdby;
                           
                            group.BranchId = Branchid;
                            group.CompanyId = companyid;
                            group.UserId = userid;
                            
                            db.Groups.Add(group);
                            db.SaveChanges();
                            
                            return RedirectToAction("Create", new { Msg = "Data Saved Successfully." });
                        
                    }
                    else
                    {


                        var checkDuplicate = db.Groups.Where(c => c.Id != group.Id).Any(c => c.Name == group.Name && c.CompanyId == companyid);
                        if (checkDuplicate)
                        {
                           
                            return RedirectToAction("Create", new { Err = "Group Name already exists.." });
                        }
                        else
                        {
                            var catedit = db.Groups.Find(group.Id);
                            group.CreatedOn = DateTime.Now;
                            group.CreatedBy = Createdby;
                            group.Name = "0";
                            group.BranchId = Branchid;
                            group.CompanyId = companyid;
                            group.UserId = userid;
                            catedit.Id = group.Id;
                            catedit.Name = group.Name;
                            catedit.Code = group.Code;

                            catedit.ParentGroupId = group.ParentGroupId;


                            catedit.ModifiedBy = Createdby;
                            catedit.ModifiedOn = DateTime.Now;
                            db.SaveChanges();
                            return RedirectToAction("Create", new { Msg = "Data Updated Successfully..",id="" });

                        }
                    }
                }

                catch (DbEntityValidationException e) //--------Form Validation Error Throw--------//
                {
                    foreach (var eve in e.EntityValidationErrors)
                    {
                        Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        foreach (var ve in eve.ValidationErrors)
                        {
                            Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                        }
                    }
                    throw;
                    ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    return RedirectToAction("Create", new { Err = "Please Try Again !...." });

                }
                catch (DbUpdateException ex) //--------Databse Error Throw--------//
                {
                    UpdateException updateException = (UpdateException)ex.InnerException;
                    SqlException sqlException = (SqlException)updateException.InnerException;

                    foreach (SqlError error in sqlException.Errors)
                    {
                        Response.Write("- Property:" + error.Number + ", Error: " + error.Message);

                    }
                    return RedirectToAction("Create", new { Err = "Please Try Again !...." });
                }
                catch (DataException)
                {
                    //Log the error (add a variable name after DataException)
                    ViewBag.Error = "Error:Data  not Saved Successfully.......";
                    return RedirectToAction("Create", new { Err = "Please Try Again !...." });

                }
                catch
                {

                   
                    return RedirectToAction("Create", new { Err = "Insertion Failed.Try Again.." });

                }
            //}
            //var errors = ModelState.Select(x => x.Value.Errors).Where(y => y.Count > 0).ToList();
           // return View(group);
        }

       
        public JsonResult GetParentgroup(string id)
        {
            try
            {
                int parentid = Convert.ToInt32(id);
                var parentgroup = db.Groups.Where(d => d.Id == parentid).Select(d => new { Id = d.Id, Code = d.Code, Name = d.Name, ParentGroupId=d.ParentGroupId }).FirstOrDefault();

                return Json(parentgroup, JsonRequestBehavior.AllowGet);
            }
            catch {
                
                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult GetParentgroupEdit(string id)
        {
            try
            {
                int parentid = Convert.ToInt32(id);
                var parentgroup = db.Groups.Where(d => d.Id == parentid).Select(d => new { Id = d.Id, Code = d.Code, Name = d.Name, ParentGroupId = d.ParentGroupId,UserId= d.UserId,CompanyId= d.CompanyId,BranchId= d.BranchId, CreatedBy=d.CreatedBy, CreatedOn = d.CreatedOn }).FirstOrDefault();

                return Json(parentgroup, JsonRequestBehavior.AllowGet);
            }
            catch
            {

                return Json("", JsonRequestBehavior.AllowGet);
            }
        }

        // GET: /Group/Edit/5

        public ActionResult Edit(string Msg)
        {
            //Group group = db.Groups.Find(id);
            //if (group == null)
            //{
            //    return HttpNotFound();
            //}
            int companyid = Convert.ToInt32(Session["companyid"]);
            List<Group> userList = new List<Group>();
            userList = db.Groups.Where(g => g.CompanyId == companyid).ToList();
            var presidents = userList.
                             Where(x => x.ParentGroupId == null).ToList();
            foreach (var president in presidents)
            {
                SetChildren(president, userList);
            }
            ViewBag.President = presidents;
            ViewBag.Message = Msg;
            return View();
        }

        //
        // POST: /Group/Edit/5

        [HttpPost]
        public ActionResult Edit(Group group)
        {
            //if (ModelState.IsValid)
            //{
                int Createdby = Convert.ToInt32(Session["Createdid"]);
                group.ModifiedBy = Createdby;
                group.ModifiedOn = DateTime.Today;
                db.Entry(group).State = EntityState.Modified;
                db.SaveChanges();
                

                return RedirectToAction("Edit", new { Msg = "Data Updated Successfully.." });
            //}
            //return View(group);
        }

        //
        // GET: /Group/Delete/5

        public ActionResult Delete(int id = 0)
        {
            Group group = db.Groups.Find(id);
            if (group == null)
            {
                return HttpNotFound();
            }
            return View(group);
        }

        //
        // POST: /Group/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            Group group = db.Groups.Find(id);
            db.Groups.Remove(group);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}