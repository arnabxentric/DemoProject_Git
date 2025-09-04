using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Transactions;

namespace XenERP.Controllers
{
    public class DocumentController : Controller
    {
        //
        // GET: /Document/

        public ActionResult CreateDocunt()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateDocunt(Document model)
        {
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        model.CompanyId = companyId;
                        model.BranchId = branchId;
                        model.UserId = userId;
                        model.CreatedOn = DateTime.Now;
                        db.Documents.Add(model);
                        db.SaveChanges();
                        scope.Complete();
                        return RedirectToAction("ShowAllDocumentList", new { Msg = "Document Created Successfully....." });
                    }
                    catch (Exception ex)
                    {
                        scope.Dispose();
                        return RedirectToAction("ShowAllDocumentList", new { Msg = "Document Creation Failes....." });
                        throw ex;
                    }
                }
            }

        }

        [HttpGet]
        public ActionResult ShowAllDocumentList(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            using (InventoryEntities db = new InventoryEntities())
            {
                int userid = Convert.ToInt32(Session["userid"]);
                int companyid = Convert.ToInt32(Session["companyid"]);
                long Branchid = Convert.ToInt64(Session["BranchId"]);
                var document = db.Documents.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
                return View(document);
            }
        }

        [HttpGet]
        public ActionResult EditDocument(int id)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                var result = db.Documents.Find(id);
                return View(result);
            }
        }

        [HttpPost]
        public ActionResult EditDocument(Document model)
        {
            string Createdby = Convert.ToString(Session["Createdid"]);
            int userId = Convert.ToInt32(Session["userid"]);
            int branchId = Convert.ToInt32(Session["BranchId"]);
            int companyId = Convert.ToInt32(Session["companyid"]);
            using (InventoryEntities db = new InventoryEntities())
            {
                try
                {
                    var result = db.Documents.Where(d => d.DocId == model.DocId).FirstOrDefault();
                    model.CompanyId = companyId;
                    model.UserId = userId;
                    model.BranchId = branchId;
                    model.ModifiedOn = DateTime.Now;
                    db.Entry(result).CurrentValues.SetValues(model);
                    db.SaveChanges();

                    return RedirectToAction("ShowAllDocumentList", new { Msg = "Data Updated Successfully...." });
                }
                catch
                {
                    return RedirectToAction("ShowAllDocumentList", new { Err = "Data  not saved successfully...." });
                }
            }
        }

    }
}
