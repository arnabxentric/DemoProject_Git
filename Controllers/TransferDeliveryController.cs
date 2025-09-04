using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;
using System.Globalization;
using System.Transactions;
using Rotativa;

namespace XenERP.Controllers
{
    public class TransferDeliveryController : Controller
    {
        private InventoryEntities db = new InventoryEntities();
        private MasterClasses mc = new MasterClasses();
        private TransactionClasses tc = new TransactionClasses();
        //
        // GET: /TransferDelivery/
        public ActionResult Index()
        {
            long companyid = Convert.ToInt32(Session["companyid"]);
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            var branches = db.BranchMasters.Where(d => d.CompanyId == companyid).Select(d => new Taxname { Id = d.Id, Name = d.Name }).ToList();
            ViewBag.branches = branches;
            ViewBag.Branchid = Branchid;
            if (Branchid == 0)
                return View(db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.FinancialYearId == Fyid ).OrderBy(p => p.TransferDeliveryDate).ToList());
            else
                return View(db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.FromBranchId == Branchid && p.FinancialYearId == Fyid).OrderBy(p => p.TransferDeliveryDate).ToList());


        }

        [SessionExpire]
        public ActionResult Create(long? id = 0, string from = null)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();

            ViewBag.Branch = Branchid;
            //   System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var warehouses = db.Warehouses.Where(d => (d.Companyid == companyid)).ToList();
            ViewBag.ddlWarehouses = warehouses;
            if(Branchid == 0)
                ViewBag.ddlWarehouses1 = warehouses;
            else
                ViewBag.ddlWarehouses1 = warehouses.Where(d=>d.Branchid == Branchid).ToList();
            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            if (id == 0)
            {
                TransferDeliveryModelView pomv = new TransferDeliveryModelView();
               // pomv.TransferOrderDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
               // pomv.ExpectedDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.TransferDeliveryDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
               // pomv.TransferReceiveDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));


                return View(pomv);
            }
            else
            {
                if (from == null)
                {
                    TransferDeliveryReceive po = db.TransferDeliveryReceives.Find(id);
                    if (po == null)
                    {
                        return HttpNotFound();
                    }
                    TransferDeliveryModelView pomv = new TransferDeliveryModelView();
                    List<TransferDeliveryDetailModelView> podmvList = new List<TransferDeliveryDetailModelView>();

                    pomv.Id = po.Id;
                //    pomv.TransferOrderId = po.TransferOrderId;
                 //   pomv.TransferOrderNo = po.TransferOrderNo;
                    pomv.TransferDeliveryNo = po.TransferDeliveryNo;
                    pomv.SerialNo = po.SerialNo;
                    //if (po.TransferOrderDate != null)
                    //    pomv.TransferOrderDate = po.TransferOrderDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    //pomv.ExpectedDate = po.ExpectedDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.TransferDeliveryDate = po.TransferDeliveryDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                    pomv.TransferReceiveDate = null;
                    pomv.FromBranchId = po.FromBranchId;
                    pomv.ToBranchId = po.ToBranchId;
                    pomv.FromWareHouseId = po.FromWareHouseId;
                    pomv.ToWareHouseId = po.ToWareHouseId;
                    pomv.LorryNo = po.LorryNo;
                    pomv.Transporter = po.Transporter;
                    pomv.TransporterContact = po.TransporterContact;
                    pomv.TagNo = po.TagNo;
                    pomv.Comments = po.Comments;
                    pomv.Status = po.Status;
                    pomv.FinancialYearId = po.FinancialYearId;
                    pomv.UserId = po.UserId;
                    pomv.BranchId = po.BranchId;
                    pomv.CompanyId = po.CompanyId;
                    pomv.Status = po.Status;
                    pomv.CreatedBy = po.CreatedBy;
                    pomv.CreatedOn = po.CreatedOn;
                    pomv.ModifiedBy = po.ModifiedBy;
                    pomv.ModifiedOn = po.ModifiedOn;

                    var podlist = db.TransferDeliveryReceiveDetails.Where(p => p.TransferDeliveryReceiveId == po.Id).ToList();
                    foreach (var pod in podlist)
                    {
                        var podmv = new TransferDeliveryDetailModelView();
                        podmv.TransferDeliveryReceiveId = pod.TransferDeliveryReceiveId;
                        podmv.ItemId = pod.ItemId;
                        podmv.ItemCode = pod.Product.Code;
                        podmv.ItemName = pod.Product.Name;
                        podmv.BarCode = pod.BarCode;
                        podmv.Description = pod.Description;
                     //   podmv.OrderQuantity = pod.OrderQuantity;
                        podmv.TransferQuantity = pod.TransferQuantity;
                       // podmv.ReceiveQuantity = pod.ReceiveQuantity;
                        podmv.UnitId = pod.UnitId;
                        podmv.UnitName = pod.UOM.Code;
                        podmv.UnitIdSecondary = pod.UnitIdSecondary;
                        podmv.UnitSecondaryName = pod.UOM1.Code;
                        podmv.SecUnitId = pod.SecUnitId;
                        if (podmv.SecUnitId != null)
                            podmv.SecUnitName = pod.UOM2.Code;
                        else
                            podmv.SecUnitName = null;
                        podmv.UnitFormula = pod.UnitFormula;
                        podmv.SecUnitFormula = pod.SecUnitFormula;
                        podmvList.Add(podmv);
                    }
                    ViewBag.podmvList = podmvList;
                    return View(pomv);
                }
                // else
                // {
                //     TransferOrder po = db.TransferOrders.Find(id);
                //     if (po == null)
                //     {
                //         return HttpNotFound();
                //     }
                //     TransferDeliveryModelView pomv = new TransferDeliveryModelView();
                //     List<TransferDeliveryDetailModelView> podmvList = new List<TransferDeliveryDetailModelView>();

                //     pomv.Id = po.Id;
                ////     pomv.TransferOrderId = po.Id;
                //   //  pomv.TransferOrderNo = po.TransferOrderNo;
                // //    pomv.TransferDeliveryNo = po.TransferDeliveryNo;
                //     pomv.SerialNo = po.SerialNo;
                //  //   if (po.TransferOrderDate != null)
                //     //pomv.TransferOrderDate = po.TransferOrderDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                //     //pomv.ExpectedDate = po.ExpectedDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                //     pomv.TransferDeliveryDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                //     pomv.TransferReceiveDate = null;
                //     pomv.FromBranchId = po.FromBranchId;
                //     pomv.ToBranchId = po.ToBranchId;
                //     pomv.FromWareHouseId = po.FromWareHouseId;
                //     pomv.ToWareHouseId = po.ToWareHouseId;
                // //    pomv.LorryNo = po.LorryNo;
                //  //   pomv.TagNo = po.TagNo;
                //     pomv.Comments = po.Comments;
                //     pomv.Status = po.Status;
                //     pomv.FinancialYearId = po.FinancialYearId;
                //     pomv.UserId = po.UserId;
                //     pomv.BranchId = po.BranchId;
                //     pomv.CompanyId = po.CompanyId;
                //     pomv.Status = po.Status;


                //     var podlist = db.TransferOrderDetails.Where(p => p.TransferOrderId == po.Id).ToList();
                //     foreach (var pod in podlist)
                //     {
                //         var podmv = new TransferDeliveryDetailModelView();
                //   //      podmv.TransferDeliveryReceiveId = pod.TransferDeliveryReceiveId;
                //         podmv.ItemId = pod.ItemId;
                //         podmv.ItemCode = pod.Product.Code;
                //         podmv.ItemName = pod.Product.Name;
                //         podmv.BarCode = pod.BarCode;
                //         podmv.Description = pod.Description;
                //  //       podmv.OrderQuantity = pod.Quantity;
                //         podmv.TransferQuantity = 0;
                //  //       podmv.ReceiveQuantity =0;
                //         podmv.UnitId = pod.UnitId;
                //         podmv.UnitName = pod.UOM.Code;
                //         podmv.UnitIdSecondary = pod.UnitIdSecondary;
                //         podmv.UnitSecondaryName = pod.UOM1.Code;
                //         podmv.SecUnitId = pod.SecUnitId;
                //         if (podmv.SecUnitId != null)
                //             podmv.SecUnitName = pod.UOM2.Code;
                //         else
                //             podmv.SecUnitName = null;
                //         podmv.UnitFormula = pod.UnitFormula;
                //         podmv.SecUnitFormula = pod.SecUnitFormula;
                //         podmvList.Add(podmv);
                //     }
                //     ViewBag.podmvList = podmvList;
                //     return View(pomv);
                // }
                return View();
            }
           
        }

        //
        // POST: /PurchaseOrder/Create
        [SessionExpire]
        [HttpPost]
        public ActionResult Create(FormCollection poCollection)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt32(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            int Fyid = Convert.ToInt32(Session["fid"]);
            string dateFormat = Session["DateFormat"].ToString();
            long Createdby = Convert.ToInt32(Session["Createdid"]);
            var culture = Session["DateCulture"].ToString();
            //  System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var basecurrency = db.Companies.Where(c => c.Id == companyid).Select(c => new { CurrencyId = c.CurrencyId, CurrencyCode = c.Currency.ISO_4217 }).FirstOrDefault();
            string currencyval = string.Empty;


            ViewBag.Branch = Branchid;

            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            var warehouses = db.Warehouses.Where(d => (d.Companyid == companyid)).ToList();
            ViewBag.ddlWarehouses = warehouses;
            if (Branchid == 0)
                ViewBag.ddlWarehouses1 = warehouses;
            else
                ViewBag.ddlWarehouses1 = warehouses.Where(d => d.Branchid == Branchid).ToList();
            TransferDeliveryModelView pomv = new TransferDeliveryModelView();
            List<TransferDeliveryDetailModelView> podmvList = new List<TransferDeliveryDetailModelView>();

            pomv.Id = Convert.ToInt64(poCollection["Id"]);
            //pomv.TransferOrderId = Convert.ToInt64(poCollection["TransferOrderId"]);
            //pomv.TransferOrderNo = poCollection["TransferOrderNo"];
            pomv.TransferDeliveryNo = poCollection["TransferDeliveryNo"];
            pomv.Comments = poCollection["Comments"];
            //pomv.TransferOrderDate = Convert.ToString(poCollection["TransferOrderDate"]);
            //pomv.ExpectedDate = Convert.ToString(poCollection["ExpectedDate"]);
            pomv.TransferDeliveryDate = Convert.ToString(poCollection["TransferDeliveryDate"]);
         //   pomv.TransferReceiveDate = Convert.ToString(poCollection["TransferReceiveDate"]);
            pomv.FromWareHouseId = Convert.ToInt64(poCollection["FromWareHouseId"]);
            pomv.ToWareHouseId = Convert.ToInt64(poCollection["ToWareHouseId"]);
            pomv.LorryNo = poCollection["LorryNo"];
            pomv.Transporter = poCollection["Transporter"];
            pomv.TransporterContact = poCollection["TransporterContact"];
            pomv.TagNo = poCollection["TagNo"]; 
            pomv.Status = poCollection["Status"];

            var count = poCollection["producthide"].Split(',').Length;
            for (int i = 0; i < count; i++)
            {
                TransferDeliveryDetailModelView podmv = new TransferDeliveryDetailModelView();
                podmvList.Add(podmv);
            }
            foreach (var key in poCollection.AllKeys)
            {

                string[] value = poCollection[key].Split(',');
                var lt = value.Length;
                switch (key)
                {
                    case "producthide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].ItemCode = value[i];
                        }
                        break;

                    case "product":
                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].ItemId = Convert.ToInt32(value[i]);
                        }
                        break;

                    case "productcodehide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].ItemName = value[i];
                        }
                        break;

                    case "barcodehide":
                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].BarCode = value[i];
                        }
                        break;

                    case "orderquantityhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].OrderQuantity = Convert.ToDecimal(value[i]);
                        }
                        break;

                    case "transferquantityhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].TransferQuantity = Convert.ToDecimal(value[i]);
                        }
                        break;

                    case "receivequantityhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].ReceiveQuantity = Convert.ToDecimal(value[i]);
                        }
                        break;

                    case "uom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitIdSecondary = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "uomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].UnitSecondaryName = value[i];
                        }
                        break;
                    case "baseuom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "baseuomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].UnitName = value[i];
                        }
                        break;
                    case "secuom":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].SecUnitId = Convert.ToInt32(value[i]);
                        }
                        break;
                    case "secuomhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].SecUnitName = value[i];
                        }
                        break;
                    case "ofhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].UnitFormula = Convert.ToDecimal(value[i]);
                        }
                        break;
                    case "secofhide":

                        for (int i = 0; i < lt; i++)
                        {
                            if (!(value[i] == "" || value[i] == "null"))
                                podmvList[i].SecUnitFormula = Convert.ToDecimal(value[i]);
                        }
                        break;

                    case "descriptionhide":

                        for (int i = 0; i < lt; i++)
                        {

                            podmvList[i].Description = value[i];
                        }
                        break;

                }
            }
            //DateTime? transferorderDate = null;
            //if (pomv.TransferOrderDate != null)
            //    transferorderDate = DateTime.ParseExact(pomv.TransferOrderDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            //var expectedDate = DateTime.ParseExact(pomv.ExpectedDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));
            var transferDate = DateTime.ParseExact(pomv.TransferDeliveryDate, dateFormat, CultureInfo.CreateSpecificCulture(culture));

            pomv.FromBranchId = warehouses.Where(d => d.Id == pomv.FromWareHouseId).Select(d => d.Branchid).FirstOrDefault();
            pomv.ToBranchId = warehouses.Where(d => d.Id == pomv.ToWareHouseId).Select(d => d.Branchid).FirstOrDefault();
            //purchase order details model view validation
            foreach (var podmv in podmvList)
            {
                if (podmv.ItemId != 0 && podmv.TransferQuantity > 0)
                {
                    //do nothing
                }
                else
                {
                    if (!(podmv.ItemId == 0 && podmv.TransferQuantity == 0))
                    {
                      
                        ViewBag.podmvList = podmvList;

                        return View(pomv);
                    }

                }
            }


            // define our transaction scope
            var scope = new System.Transactions.TransactionScope(
                // a new transaction will always be created
                TransactionScopeOption.RequiresNew,
                // we will allow volatile data to be read during transaction
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted
                }
            );
            try
            {
                // use the scope we just defined
                using (scope)
                {
                    // create a new db context
                    using (var ctx = new InventoryEntities())
                    {
                        //Check Insert or Update
                        if (pomv.Id == 0)
                        {
                            //int countpo = db.PurchaseOrders.Where(p => p.CompanyId == companyid && p.BranchId == Branchid).Count();
                            //pomv.NO = tc.GenerateCode("PO", countpo);

                            var fyear = db.FinancialYearMasters.FirstOrDefault(f => f.fYearID == Fyid).Year;
                            var fs = fyear.Substring(2, 2);
                            var es = fyear.Substring(7, 2);
                            fyear = fs + "-" + es;
                            int countpo = 1;
                            int receivecount = 1;
                            string getBranchCode = "HO";
                            
                            pomv.TagNo = "TR-1";
                            //&& p.BranchId == Branchid
                            if (db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.BranchId == pomv.FromBranchId && p.FinancialYearId == Fyid).Count() != 0)
                            {
                                countpo = (int)db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.BranchId == pomv.FromBranchId && p.FinancialYearId == Fyid).Max(p => p.SerialNo) + 1;
                            }
                            if(pomv.FromBranchId !=0)
                                getBranchCode = db.BranchMasters.Where(p => p.CompanyId == companyid && p.Id == pomv.FromBranchId).Select(p => p.Code).FirstOrDefault();

                            pomv.TransferDeliveryNo ="TRN/" + getBranchCode + "/" + fyear + "/" + countpo;


                            if (db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.BranchId == pomv.ToBranchId ).Count() != 0)
                            {
                                var BranchCode = db.TransferDeliveryReceives.Where(p => p.CompanyId == companyid && p.BranchId == pomv.ToBranchId).OrderByDescending(p => p.Id).Select(p => p.TagNo).FirstOrDefault();
                                receivecount = Convert.ToInt32(BranchCode.Substring(3)) + 1;
                                
                            }

                            pomv.TagNo = "TR-"  + receivecount;

                            //Insert into purchaseorder table
                            TransferDeliveryReceive po = new TransferDeliveryReceive();
                            po.TransferOrderId = pomv.TransferOrderId;
                            po.TransferOrderNo = pomv.TransferOrderNo;
                            po.TransferDeliveryNo = pomv.TransferDeliveryNo;
                            po.SerialNo = countpo;
                          //  po.TransferOrderDate = null; ;
                         //   po.ExpectedDate = expectedDate;
                            po.TransferDeliveryDate = transferDate;
                            
                            po.FromBranchId = pomv.FromBranchId;
                            po.ToBranchId = pomv.ToBranchId;
                            po.FromWareHouseId = pomv.FromWareHouseId;
                            po.ToWareHouseId = pomv.ToWareHouseId;
                            po.Comments = pomv.Comments;
                            po.TagNo = pomv.TagNo;
                            po.LorryNo = pomv.LorryNo;
                            po.Transporter = pomv.Transporter;
                            po.TransporterContact = pomv.TransporterContact;
                            po.FinancialYearId = Fyid;
                            po.CreatedBy = Createdby;
                            po.CreatedOn = DateTime.Now;
                            po.UserId = userid;
                            po.BranchId = Branchid;
                            po.CompanyId = companyid;
                            po.Status = "Transferred";

                            db.TransferDeliveryReceives.Add(po);

                            db.SaveChanges();
                            pomv.Status = po.Status;
                            var po1 = db.TransferDeliveryReceives.Find(po.Id);
                            pomv.Id = po.Id;


                            //if(pomv.TransferOrderId !=0)
                            //{
                            //    var findTO = db.TransferOrders.Find(pomv.TransferOrderId);
                            //    findTO.Status = "Transferred";
                            //}

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.TransferQuantity > 0)
                                {


                                    TransferDeliveryReceiveDetail pod = new TransferDeliveryReceiveDetail();
                                    pod.TransferDeliveryReceiveId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = "";
                                    pod.Description = podmv.Description;
                                    pod.OrderQuantity = podmv.OrderQuantity;
                                    pod.TransferQuantity = podmv.TransferQuantity;
                               //     pod.ReceiveQuantity = podmv.ReceiveQuantity;
                                    pod.UnitId = podmv.UnitId;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;
                                    db.TransferDeliveryReceiveDetails.Add(pod);
                                }
                            }

                        }
                        else
                        {
                            //Update Purchase Request

                            var po = db.TransferDeliveryReceives.Find(pomv.Id);

                      //      po.ExpectedDate = expectedDate;
                            po.TransferDeliveryDate = transferDate;

                            po.FromBranchId = pomv.FromBranchId;
                            po.ToBranchId = pomv.ToBranchId;
                            po.FromWareHouseId = pomv.FromWareHouseId;
                            po.ToWareHouseId = pomv.ToWareHouseId;
                            po.Comments = pomv.Comments;
                            po.LorryNo = pomv.LorryNo;
                            po.Transporter = pomv.Transporter;
                            po.TransporterContact = pomv.TransporterContact;
                            po.ModifiedBy = Createdby;
                            po.ModifiedOn = DateTime.Now;

                            var podOldRecords = db.TransferDeliveryReceiveDetails.Where(p => p.TransferDeliveryReceiveId == po.Id).ToList();
                            foreach (var podOld in podOldRecords)
                            {
                                db.TransferDeliveryReceiveDetails.Remove(podOld);
                            }

                            foreach (var podmv in podmvList)
                            {
                                if (podmv.ItemId != 0 && podmv.TransferQuantity > 0)
                                {


                                    TransferDeliveryReceiveDetail pod = new TransferDeliveryReceiveDetail();
                                    pod.TransferDeliveryReceiveId = po.Id;
                                    pod.ItemId = podmv.ItemId;
                                    pod.BarCode = "";
                                    pod.Description = podmv.Description;
                                    pod.OrderQuantity = podmv.OrderQuantity;
                                    pod.TransferQuantity = podmv.TransferQuantity;
                              //      pod.ReceiveQuantity = podmv.ReceiveQuantity;
                                    pod.UnitId = podmv.UnitId;
                                    pod.UnitIdSecondary = podmv.UnitIdSecondary;
                                    pod.SecUnitId = podmv.SecUnitId;
                                    pod.UnitFormula = podmv.UnitFormula;
                                    pod.SecUnitFormula = podmv.SecUnitFormula;
                                    db.TransferDeliveryReceiveDetails.Add(pod);
                                }
                            }

                        }

                    }
                    db.SaveChanges();
                    scope.Complete();
                }

            }

            catch (System.Data.Entity.Validation.DbEntityValidationException e)
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

                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
               
                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (DataException dex)
            {
                //Log the error (add a variable name after DataException)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                
                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }
            catch (Exception exp)
            {
                
                ViewBag.podmvList = podmvList;

                ViewBag.Message = "Error";
                return View(pomv);
            }

            

            ViewBag.podmvList = podmvList;
            ViewBag.Message = "You have successfully saved Transfer Delivery " + pomv.TransferDeliveryNo;

            return View(pomv);
        }

        [SessionExpire]
        public ActionResult Display(long? id = 0)
        {
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);
            var dateFormat = Session["DateFormat"].ToString();
            var culture = Session["DateCulture"].ToString();

            ViewBag.Branch = Branchid;
            //   System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo(culture);
            var warehouses = db.Warehouses.Where(d => (d.Companyid == companyid)).ToList();
            ViewBag.ddlWarehouses = warehouses;


            ViewBag.unit = db.UOMs.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.category = db.Categories.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();
            ViewBag.group = db.Groups.Where(d => d.UserId == userid && d.CompanyId == companyid).ToList();

            if (id == 0)
            {
                TransferDeliveryModelView pomv = new TransferDeliveryModelView();
                pomv.TransferOrderDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ExpectedDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.TransferDeliveryDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.TransferReceiveDate = DateTime.Today.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));


                return View(pomv);
            }
            else
            {
                TransferDeliveryReceive po = db.TransferDeliveryReceives.Find(id);
                if (po == null)
                {
                    return HttpNotFound();
                }
                TransferDeliveryModelView pomv = new TransferDeliveryModelView();
                List<TransferDeliveryDetailModelView> podmvList = new List<TransferDeliveryDetailModelView>();

                pomv.Id = po.Id;
                pomv.TransferOrderNo = po.TransferOrderNo;
                pomv.TransferDeliveryNo = po.TransferDeliveryNo;
                pomv.SerialNo = po.SerialNo;
                if (po.TransferOrderDate != null)
                    pomv.TransferOrderDate = po.TransferOrderDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.ExpectedDate = po.ExpectedDate.Value.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.TransferDeliveryDate = po.TransferDeliveryDate.ToString(dateFormat, CultureInfo.CreateSpecificCulture(culture));
                pomv.TransferReceiveDate = null;
                pomv.FromBranchId = po.FromBranchId;
                pomv.ToBranchId = po.ToBranchId;
                pomv.FromWareHouseId = po.FromWareHouseId;
                pomv.ToWareHouseId = po.ToWareHouseId;
                pomv.LorryNo = po.LorryNo;
                pomv.TagNo = po.TagNo;
                pomv.Comments = po.Comments;
                pomv.Status = po.Status;
                pomv.FinancialYearId = po.FinancialYearId;
                pomv.UserId = po.UserId;
                pomv.BranchId = po.BranchId;
                pomv.CompanyId = po.CompanyId;
                pomv.Status = po.Status;
                pomv.CreatedBy = po.CreatedBy;
                pomv.CreatedOn = po.CreatedOn;
                pomv.ModifiedBy = po.ModifiedBy;
                pomv.ModifiedOn = po.ModifiedOn;

                var podlist = db.TransferDeliveryReceiveDetails.Where(p => p.TransferDeliveryReceiveId == po.Id).ToList();
                foreach (var pod in podlist)
                {
                    var podmv = new TransferDeliveryDetailModelView();
                    podmv.TransferDeliveryReceiveId = pod.TransferDeliveryReceiveId;
                    podmv.ItemId = pod.ItemId;
                    podmv.ItemCode = pod.Product.Code;
                    podmv.ItemName = pod.Product.Name;
                    podmv.BarCode = pod.BarCode;
                    podmv.Description = pod.Description;
                    podmv.OrderQuantity = pod.OrderQuantity;
                    podmv.TransferQuantity = pod.TransferQuantity;
                    podmv.ReceiveQuantity = pod.ReceiveQuantity;
                    podmv.UnitId = pod.UnitId;
                    podmv.UnitName = pod.UOM.Code;
                    podmv.UnitIdSecondary = pod.UnitIdSecondary;
                    podmv.UnitSecondaryName = pod.UOM1.Code;
                    podmv.SecUnitId = pod.SecUnitId;
                    if (podmv.SecUnitId != null)
                        podmv.SecUnitName = pod.UOM2.Code;
                    else
                        podmv.SecUnitName = null;
                    podmv.UnitFormula = pod.UnitFormula;
                    podmv.SecUnitFormula = pod.SecUnitFormula;
                    podmvList.Add(podmv);
                }
                ViewBag.podmvList = podmvList;
                return View(pomv);
            }
        }

        public ActionResult CreateTransferChallanPDF(long? id, long? Branchid, long? companyid, long? userid)
        {

            var culture = Convert.ToString(Session["DateCulture"]);
            var dateFormat = Convert.ToString(Session["DateFormat"]);
            TransferDeliveryModelView pomv = new TransferDeliveryModelView();
            List<TransferDeliveryDetailModelView> podmvList = new List<TransferDeliveryDetailModelView>();

           
            ViewBag.CompanyName = db.Companies.Select(d => d.Name).FirstOrDefault();
            TransferDeliveryReceive po = db.TransferDeliveryReceives.Find(id);
            if (po == null)
            {
                return HttpNotFound();
            }

           
            //var customer = db.Customers.Where(r => r.Id == po.CustomerId).Select(s => s.Code).FirstOrDefault();
            //ViewBag.customer = customer;
            var company = db.Companies.Where(c => c.Id == po.CompanyId).FirstOrDefault();
            // var companyname = db.Companies.Where(c => c.Id == comids).FirstOrDefault();
            ViewBag.company = company;

            ViewBag.companyName = company.Name;
            ViewBag.address = company.Address;
            var frombranch = db.BranchMasters.Where(c => c.Id == po.FromBranchId).FirstOrDefault();
            ViewBag.fromBranch = frombranch;

            var tobranch = db.BranchMasters.Where(c => c.Id == po.ToBranchId).FirstOrDefault();
            ViewBag.toBranch = tobranch;


            var podlist = db.TransferDeliveryReceiveDetails.Where(p => p.TransferDeliveryReceiveId == po.Id).ToList();
            foreach (var pod in podlist)
            {
                var podmv = new TransferDeliveryDetailModelView();
                podmv.TransferDeliveryReceiveId = pod.TransferDeliveryReceiveId;
                podmv.ItemId = pod.ItemId;
                podmv.ItemName = pod.Product.Description;
                podmv.Description = pod.Description;
                podmv.TransferQuantity = pod.TransferQuantity;
                
                podmv.UnitId = pod.UnitId;
                podmv.UnitName = pod.UOM.Description;
               
                podmv.UnitIdSecondary = pod.UnitIdSecondary;
                podmv.UnitSecondaryName = pod.UOM1.Code;
                podmv.SecUnitId = pod.SecUnitId;
                if (podmv.SecUnitId != null)
                    podmv.SecUnitName = pod.UOM2.Code;
                else
                    podmv.SecUnitName = null;
                podmv.UnitFormula = pod.UnitFormula;
                podmv.SecUnitFormula = pod.SecUnitFormula;
                
                podmvList.Add(podmv);
            }

            ViewBag.podmvList = podmvList;

            return View(po);

        }



        public ActionResult PrintTransferDeliveryPDF(long? id)
        {
            //return RedirectToAction("CreteSalesInvoicePDF", new { id = id });
            long Branchid = Convert.ToInt64(Session["BranchId"]);
            long companyid = Convert.ToInt64(Session["companyid"]);
            long userid = Convert.ToInt32(Session["userid"]);

            return new ActionAsPdf("CreateTransferChallanPDF", new { id = id, Branchid = Branchid, companyid = companyid, userid = userid }) { FileName = "TransferChallanPrint.pdf" };
        }

    }
}
