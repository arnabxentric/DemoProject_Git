using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using XenERP.Models;

namespace XenERP.Controllers
{
    public class UploadController : Controller
    {
        //
        // GET: /Upload/

        public ActionResult UploadPurchaseDoc(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }


        public ActionResult ReadPurchaseDoc(FormCollection formCollection)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        var uploadList = new List<UploadModelView>();
                        if (Request != null)
                        {
                            int userId = Convert.ToInt32(Session["userid"]);
                            int fYear = Convert.ToInt32(Session["fid"]);
                            var culture = "es-AR";
                            string dateFormat = "dd/MM/yyyy";
                            //int refCount = 1;


                            HttpPostedFileBase file = Request.Files["UploadedFile"];
                            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                            {
                                string fileName = file.FileName;
                                string fileContentType = file.ContentType;
                                byte[] fileBytes = new byte[file.ContentLength];
                                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                                using (var package = new ExcelPackage(file.InputStream))
                                {

                                    var currentSheet = package.Workbook.Worksheets;
                                    var workSheet = currentSheet.First();
                                    var noOfCol = workSheet.Dimension.End.Column;
                                    var noOfRow = workSheet.Dimension.End.Row;
                                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        var uploadData = new UploadModelView();
                                        uploadData.Date = Convert.ToDateTime(workSheet.Cells[rowIterator, 1].Value);
                                        uploadData.ReffInvoiceNo = Convert.ToString(workSheet.Cells[rowIterator, 2].Value);
                                        uploadData.Quantity = Convert.ToDecimal(workSheet.Cells[rowIterator, 3].Value);
                                        uploadData.Rate = Convert.ToDecimal(workSheet.Cells[rowIterator, 4].Value);
                                        uploadData.TotalAmount = Convert.ToDecimal(workSheet.Cells[rowIterator, 5].Value);
                                        uploadData.ProductId = Convert.ToInt32(workSheet.Cells[rowIterator, 6].Value);
                                        uploadData.SupplierId = Convert.ToInt32(workSheet.Cells[rowIterator, 7].Value);
                                        uploadList.Add(uploadData);
                                    }
                                }
                            }

                            foreach (var item in uploadList)
                            {
                                //var poDetails = db.PurchaseOrders.Where(r => r.NO == item.PurchaseOrder).FirstOrDefault();
                                //if (poDetails != null)
                                //{
                                //    int countpo = 1;
                                //    if (db.PurchaseReceipts.Where(p => p.CompanyId == 1 && p.BranchId == 6 && p.FinancialYearId == 10).Count() != 0)
                                //    {
                                //        countpo = (int)db.PurchaseReceipts.Where(p => p.CompanyId == 1 && p.BranchId == 6 && p.FinancialYearId == 10).Max(p => p.InvoiceNo) + 1;
                                //    }
                                //    PurchaseReceipt receipt = new PurchaseReceipt();
                                //    receipt.CreatedFrom = "Purchase Order";
                                //    receipt.ReferenceNo = poDetails.Id;
                                //    receipt.OrderNo = poDetails.NO;
                                //    receipt.SupplierId = poDetails.SupplierId;
                                //    receipt.Date = item.Date;
                                //    receipt.DueDate = item.Date;
                                //    receipt.ReceiptDate = item.Date;
                                //    receipt.CurrencyId = 84;
                                //    receipt.TransactionCurrency = 84;
                                //    receipt.Currencyrate = 1;
                                //    receipt.FinancialYearId = poDetails.FinancialYearId;
                                //    //receipt.LID = poDetails.LID;
                                //    //receipt.IsCashDiscount = poDetails.IsCashDiscount;
                                //    receipt.PaymentTermId = poDetails.PaymentTermId;
                                //    //receipt.CashDiscountAmount = poDetails.CashDiscountAmount;
                                //    receipt.TotalAmount = item.TotalAmount;
                                //    receipt.GrandTotal = item.TotalAmount;
                                //    receipt.BCGrandTotal = item.TotalAmount;
                                //    receipt.NO = "PC/24-25/" + countpo;
                                //    receipt.WarehouseId = poDetails.WarehouseId;
                                //    receipt.DeliveryName = poDetails.DeliveryName;
                                //    receipt.StreetPoBox = poDetails.StreetPoBox;
                                //    receipt.Suburb = poDetails.Suburb;
                                //    receipt.City = poDetails.City;
                                //    receipt.StateRegion = poDetails.StateRegion;
                                //    receipt.Country = poDetails.Country;
                                //    receipt.PostalCode = poDetails.PostalCode;
                                //    receipt.TagNo = Convert.ToString(countpo);
                                //    receipt.InvoiceNo = countpo;
                                //    receipt.CreatedBy = 1;
                                //    receipt.CreatedOn = DateTime.Now;
                                //    receipt.Status = "Invoiced";
                                //    receipt.UserId = 1;
                                //    receipt.CompanyId = 1;
                                //    receipt.BranchId = 6;
                                //    db.PurchaseReceipts.Add(receipt);
                                //    db.SaveChanges();

                                //PurchaseReceiptDetail detail = new PurchaseReceiptDetail();
                                //detail.PurchaseReceiptId = receipt.Id;
                                //detail.ItemId = 286;
                                //detail.Quantity = item.Quantity;
                                //detail.UnitId = 1;
                                //detail.UnitIdSecondary = 2;
                                //detail.SecUnitId = 2;
                                //detail.UnitFormula = 1;
                                //detail.SecUnitFormula = 1;
                                //detail.Price = item.Rate;
                                //detail.AccountId = 12;
                                //detail.CurrencyRate = 1;
                                //detail.TotalAmount = item.TotalAmount;
                                //db.PurchaseReceiptDetails.Add(detail);
                                //db.SaveChanges();

                                int countpo1 = 1;
                                if (db.PurchaseInvoices.Where(p => p.CompanyId == 1 && p.FinancialYearId == 11).Count() != 0)
                                {
                                    countpo1 = (int)db.PurchaseInvoices.Where(p => p.CompanyId == 1 && p.FinancialYearId == 11).Max(p => p.InvoiceNo) + 1;
                                }
                                PurchaseInvoice invoice = new PurchaseInvoice();
                                //invoice.OrderNo = receipt.NO + "(" + receipt.OrderNo + ")";
                                //invoice.ReceiptIds = Convert.ToString(receipt.Id);
                                //invoice.PurchaseOrderNo = receipt.OrderNo;
                                //invoice.PurchaseOrderId = poDetails.Id;
                                invoice.SupplierId = item.SupplierId;
                                invoice.Reference = item.ReffInvoiceNo;
                                invoice.Date = item.Date;
                                invoice.DueDate = item.Date;
                                invoice.InvoiceDate = item.Date;
                                invoice.DespatchDate = item.Date;
                                invoice.CurrencyId = 84;
                                invoice.TransactionCurrency = 84;
                                invoice.Currencyrate = 1;
                                invoice.FinancialYearId = 11;

                                //LID Need--------------
                                invoice.LID = 881;


                                //invoice.IsCashDiscount = false;
                                invoice.PaymentTermId = 2;
                                //invoice.CashDiscountAmount = 0;
                                invoice.TaxProduct = 0;
                                invoice.TaxOther = 0;
                                invoice.SubTotal = item.TotalAmount;
                                invoice.RoundOff = 0;
                                invoice.TotalAddAmount = 0;
                                invoice.TotalDeductAmount = 0;
                                invoice.GrandTotal = item.TotalAmount;
                                invoice.BCGrandTotal = item.TotalAmount;
                                invoice.NO = "PC/25-26/" + countpo1;
                                invoice.ReferenceInvoice = item.ReffInvoiceNo;
                                invoice.WarehouseId = 1;
                                invoice.DeliveryName = "Rupen Raha";
                                invoice.StreetPoBox = "22 R N Mukherjee Road";
                                invoice.City = "Kolkata";
                                invoice.StateRegion = "West Bengal";
                                invoice.PostalCode = "700001";
                                invoice.Country = "India";
                                invoice.InvoiceNo = countpo1;
                                invoice.CreatedBy = 1;
                                invoice.CreatedOn = DateTime.Now;
                                invoice.Status = "Saved";
                                invoice.UserId = 1;
                                invoice.CompanyId = 1;
                                invoice.BranchId = 0;
                                db.PurchaseInvoices.Add(invoice);
                                db.SaveChanges();

                                PurchaseInvoiceDetail invoDetail = new PurchaseInvoiceDetail();
                                invoDetail.PurchaseInvoiceId = invoice.Id;
                                invoDetail.ItemId = Convert.ToInt32(item.ProductId);
                                invoDetail.Quantity = item.Quantity;
                                invoDetail.UnitId = 2;
                                invoDetail.UnitIdSecondary = 1;
                                invoDetail.SecUnitId = 1;
                                invoDetail.SecUnitFormula = 50;
                                invoDetail.UnitFormula = item.Quantity / 50;
                                invoDetail.Price = item.Rate;
                                invoDetail.AccountId = 12;
                                invoDetail.CurrencyRate = 1;
                                invoDetail.TaxId = 1;
                                invoDetail.TaxPercent = 0;
                                invoDetail.TaxAmount = 0;
                                invoDetail.TotalAmount = item.TotalAmount;
                                db.PurchaseInvoiceDetails.Add(invoDetail);
                                db.SaveChanges();


                                PurchaseTax pTax = new PurchaseTax();
                                pTax.PurchaseInvoiceId = invoice.Id;
                                pTax.ItemId = Convert.ToInt32(item.ProductId);
                                pTax.TaxId = 1;
                                pTax.Amount = 0;
                                pTax.CurrencyRate = 1;
                                db.PurchaseTaxes.Add(pTax);
                                db.SaveChanges();

                                //SupplierProductPrice spp = new SupplierProductPrice();
                                //spp.SupplierId = 183;
                                //spp.ProductId = 619;
                                //spp.UnitFormula = item.Quantity;
                                //spp.PurchasePrice = item.Rate;
                                //spp.UserId = 1;
                                //spp.CompanyId = 1;
                                //spp.BranchId = 0;
                                //db.SupplierProductPrices.Add(spp);
                                //db.SaveChanges();


                                //var findSupplierProductPrice = db.SupplierProductPrices.Find(183, 619, item.Quantity);
                                //if (findSupplierProductPrice == null)
                                //{
                                //    var supplierProductPrice = new SupplierProductPrice();
                                //    supplierProductPrice.SupplierId = 183;
                                //    supplierProductPrice.ProductId = 619;
                                //    supplierProductPrice.PurchasePrice = item.Quantity;
                                //    supplierProductPrice.UnitFormula = item.Rate;
                                //    supplierProductPrice.UserId = 1;
                                //    supplierProductPrice.CompanyId =1;
                                //    supplierProductPrice.BranchId = 0;
                                //    db.SupplierProductPrices.Add(supplierProductPrice);
                                //    db.SaveChanges();
                                //}
                                //else
                                //{
                                //    findSupplierProductPrice.PurchasePrice = item.Quantity;
                                //    db.SaveChanges();
                                //}

                                //refCount++;

                            }
                        }
                        scope.Complete();
                    }

                    catch (DbEntityValidationException e)
                    {
                        scope.Dispose();
                        //throw;
                        //foreach (var eve in e.EntityValidationErrors)
                        //{

                        //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        //    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        //    foreach (var ve in eve.ValidationErrors)
                        //    {
                        //        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                        //    }
                        //}
                        //throw;
                        //ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }

            }
            return View("UploadPurchaseDoc");
        }


        public ActionResult UploadSalesDoc(string Msg, string Err)
        {
            if (Msg != null)
            {
                ViewBag.Message = Msg;
            }
            else
            {
                ViewBag.Error = Err;
            }
            return View();
        }


        public ActionResult ReadSalesDoc(FormCollection formCollection)
        {
            using (InventoryEntities db = new InventoryEntities())
            {
                using (TransactionScope scope = new TransactionScope())
                {
                    try
                    {
                        var uploadList = new List<UploadModelView>();
                        if (Request != null)
                        {
                            int userId = Convert.ToInt32(Session["userid"]);
                            int fYear = Convert.ToInt32(Session["fid"]);
                            var culture = "es-AR";
                            string dateFormat = "dd/MM/yyyy";
                            //int refCount = 1;


                            HttpPostedFileBase file = Request.Files["UploadedFile"];
                            if ((file != null) && (file.ContentLength > 0) && !string.IsNullOrEmpty(file.FileName))
                            {
                                string fileName = file.FileName;
                                string fileContentType = file.ContentType;
                                byte[] fileBytes = new byte[file.ContentLength];
                                var data = file.InputStream.Read(fileBytes, 0, Convert.ToInt32(file.ContentLength));
                                using (var package = new ExcelPackage(file.InputStream))
                                {

                                    var currentSheet = package.Workbook.Worksheets;
                                    var workSheet = currentSheet.First();
                                    var noOfCol = workSheet.Dimension.End.Column;
                                    var noOfRow = workSheet.Dimension.End.Row;
                                    for (int rowIterator = 2; rowIterator <= noOfRow; rowIterator++)
                                    {
                                        var uploadData = new UploadModelView();
                                        uploadData.Date = Convert.ToDateTime(workSheet.Cells[rowIterator, 1].Value);
                                        uploadData.Quantity = Convert.ToDecimal(workSheet.Cells[rowIterator, 2].Value);
                                        uploadData.Rate = Convert.ToDecimal(workSheet.Cells[rowIterator, 3].Value);
                                        uploadData.TotalAmount = Convert.ToDecimal(workSheet.Cells[rowIterator, 4].Value);
                                        uploadData.CustomerId = Convert.ToInt32(workSheet.Cells[rowIterator, 5].Value);
                                        uploadData.ProductId = Convert.ToInt32(workSheet.Cells[rowIterator, 6].Value);
                                        uploadList.Add(uploadData);
                                    }
                                }
                            }

                            foreach (var item in uploadList)
                            {
                                //int countpo = 1;
                                //if (db.SalesDeliveries.Where(p => p.CompanyId == 1 && p.BranchId == 6 && p.FinancialYearId == 10).Count() != 0)
                                //{
                                //    //&& p.BranchId == Branchid 
                                //    countpo = (int)db.SalesDeliveries.Where(p => p.CompanyId == 1 && p.BranchId == 6 && p.FinancialYearId == 10).Max(p => p.InvoiceNo) + 1;
                                //}
                                //SalesDelivery delivery = new SalesDelivery();
                                //delivery.CustomerId = 1002;
                                //delivery.Date = item.Date;
                                //delivery.DueDate = item.Date;
                                //delivery.ReceiptDate = item.Date;
                                //delivery.DespatchDate = item.Date;
                                //delivery.CurrencyId = 84;
                                //delivery.TransactionCurrency = 84;
                                //delivery.Currencyrate = 1;
                                //delivery.FinancialYearId = 10;
                                //delivery.DisApplicable = false;
                                //delivery.FD1Applicable = false;
                                //delivery.FD2Applicable=false;
                                //delivery.FD3Applicable=false;  
                                //delivery.FD4Applicable=false;
                                //delivery.Dis = 0;
                                //delivery.FD1 = 0;
                                //delivery.FD2= 0;
                                //delivery.FD3= 0;
                                //delivery.FD4= 0;
                                //delivery.DisAmount = 0;
                                //delivery.FD1Amount= 0;
                                //delivery.FD2Amount= 0;
                                //delivery.FD3Amount= 0;
                                // delivery.FD4Amount= 0;
                                //delivery.InvoiceNo = countpo;
                                //delivery.TaxTotal = 0;
                                //delivery.TotalAmount = item.TotalAmount;
                                //delivery.GrandTotal = item.TotalAmount;
                                //delivery.BCGrandTotal = item.TotalAmount;
                                //delivery.NO = "SD/24-25/" + countpo;
                                //delivery.WarehouseId = 11;
                                //delivery.CreatedBy = 1;
                                //delivery.CreatedOn = DateTime.Now;
                                //delivery.Status = "Invoiced";
                                ////delivery.Mode = 1;
                                //delivery.UserId = 1;
                                //delivery.CompanyId = 1;
                                //delivery.BranchId = 6;
                                //db.SalesDeliveries.Add(delivery);
                                //db.SaveChanges();

                                //SalesDeliveryDetail detail = new SalesDeliveryDetail();
                                //detail.SalesDeliveryId = delivery.Id;
                                //detail.ItemId = 286;
                                //detail.Quantity = item.Quantity;
                                //detail.UnitId = 1;
                                //detail.UnitIdSecondary = 1;
                                //detail.SecUnitId = 2;
                                //detail.UnitFormula = 1;
                                //detail.SecUnitFormula = 0;
                                //detail.Price = item.Rate;
                                //detail.AccountId = 12;
                                //detail.CurrencyRate = 1;
                                //detail.TotalAmount = item.TotalAmount;
                                //db.SalesDeliveryDetails.Add(detail);
                                //db.SaveChanges();

                                int countpo1 = 1;
                                if (db.SalesInvoices.Where(p => p.CompanyId == 1 && p.BranchId == 0 && p.FinancialYearId == 11 && p.Mode == 1 && p.NO.Contains("HISS/RM/25-26/")).Count() != 0)
                                {
                                    countpo1 = (int)db.SalesInvoices.Where(p => p.CompanyId == 1 && p.BranchId == 0 && p.FinancialYearId == 11 && p.Mode == 1 && p.NO.Contains("HISS/RM/25-26/")).Max(p => p.InvoiceNo) + 1;
                                }
                                SalesInvoice invoice = new SalesInvoice();
                                //invoice.CreatedFrom = "Delivery Note";
                                //invoice.ReferenceNo = delivery.Id;
                                //invoice.OrderNo = delivery.NO;
                                invoice.CustomerId = item.CustomerId;
                                invoice.Date = item.Date;
                                invoice.DueDate = item.Date;
                                invoice.InvoiceDate = item.Date;
                                invoice.DespatchDate = item.Date;
                                invoice.CurrencyId = 84;
                                invoice.TransactionCurrency = 84;
                                invoice.Currencyrate = 1;
                                invoice.FinancialYearId = 11;
                                invoice.CompoundedDis = false;
                                invoice.DisApplicable = false;
                                invoice.FD1Applicable = false;
                                invoice.FD2Applicable = false;
                                invoice.FD3Applicable = false;
                                invoice.FD4Applicable = false;
                                //invoice.FurComBillCr=false;
                                //invoice.DisPerBagApplicable=false; ;
                                //invoice.FurDisPerBagApplicable=false; 
                              
                                invoice.Dis = 0;
                                invoice.FD1 = 0;
                                invoice.FD2 = 0;
                                invoice.FD3 = 0;
                                invoice.FD4 = 0;
                                invoice.DisAmount = 0;
                                invoice.FD1Amount = 0;
                                invoice.FD2Amount = 0;
                                invoice.FD3Amount = 0;
                                invoice.FD4Amount = 0;
                                invoice.SubTotal = item.TotalAmount;
                                invoice.TaxProduct = 0;
                                invoice.TaxOther = 0;
                                invoice.TotalAddAmount = 0;
                                invoice.TotalDeductAmount = 0;
                                invoice.RoundOff = 0;
                                invoice.GrandTotal = item.TotalAmount;
                                invoice.BCGrandTotal = item.TotalAmount;
                                invoice.NO = "HISS/RM/25-26/" + countpo1;
                                invoice.WarehouseId = 1;
                                invoice.Mode = 1;
                                invoice.InvoiceNo = countpo1;
                                invoice.CreatedBy = 1;
                                invoice.CreatedOn = DateTime.Now;
                                invoice.Status = "Saved";
                                invoice.UserId = 1;
                                invoice.CompanyId = 1;
                                invoice.BranchId = 0;
                                invoice.LID = 11497;
                                db.SalesInvoices.Add(invoice);
                                db.SaveChanges();

                                SalesInvoiceDetail invoDetail = new SalesInvoiceDetail();
                                invoDetail.SalesInvoiceId = invoice.Id;
                                invoDetail.ItemId =Convert.ToInt32(item.ProductId);
                                invoDetail.Quantity = item.Quantity;
                                invoDetail.UnitId = 2;
                                invoDetail.UnitIdSecondary = 1;
                                invoDetail.SecUnitId = 1;
                                invoDetail.SecUnitFormula = 50;
                                invoDetail.UnitFormula = item.Quantity / 50;
                                invoDetail.Price = item.Rate;
                                invoDetail.Discount = 0;
                                invoDetail.DiscountAmount = 0;
                                invoDetail.CustDiscountAmount = 0;

                                invoDetail.AccountId = 12;
                                invoDetail.CurrencyRate = 1;
                                invoDetail.TaxId = 1;
                                invoDetail.TaxPercent = 0;
                                invoDetail.TaxAmount = 0;
                                invoDetail.TotalAmount = item.TotalAmount;
                                db.SalesInvoiceDetails.Add(invoDetail);
                                db.SaveChanges();

                                //refCount++;

                            }
                        }
                        scope.Complete();
                    }

                    catch (DbEntityValidationException e)
                    {
                        scope.Dispose();
                        //throw;
                        //foreach (var eve in e.EntityValidationErrors)
                        //{

                        //    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        //    eve.Entry.Entity.GetType().Name, eve.Entry.State);
                        //    foreach (var ve in eve.ValidationErrors)
                        //    {
                        //        Response.Write("- Property:" + ve.PropertyName + ", Error: " + ve.ErrorMessage);

                        //    }
                        //}
                        //throw;
                        //ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
                    }
                }

            }
            return View("UploadSalesDoc");
        }


    }
}
