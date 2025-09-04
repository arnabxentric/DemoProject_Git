using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XenERP.Models.Repository
{
    
    public class TaxRepository
    {
        InventoryEntities db = new InventoryEntities();

       

        public long GetLedgerInsertId(LedgerMaster led)
        {
            if (led.LID == 0)
            {
                db.LedgerMasters.Add(led);
                db.SaveChanges();
            }
            return led.LID;
        }


        public long Getcountrycodeid(User user)
        {
            if (user.Id == 0)
            {
                db.Users.Add(user);
                db.SaveChanges();
            }
            return user.Id;
        }

        public Currency GetCurency(int id)
        {

            return db.Currencies.FirstOrDefault(d => d.CurrencyId == id);
        
        }
        public UOM GetUnit(int id)
        {

            return db.UOMs.FirstOrDefault(d => d.Id == id);

        }


        public Group Getgroup(int id)
        {

            return db.Groups.FirstOrDefault(d => d.Id == id);

        }


        public Category Getcategory(int id)
        {

            return db.Categories.FirstOrDefault(d => d.Id == id);

        }


        public Tax GetTax(int id)
        {

            return db.Taxes.FirstOrDefault(d => d.TaxId == id);

        }


        public Warehouse GetWarehouse(int id)
        {

            return db.Warehouses.FirstOrDefault(d => d.Id == id);

        }


        public Country GetCountry(int id)
        {

            return db.Countries.FirstOrDefault(d => d.CountryId == id);

        }
        public Company Getcompany(int id)
        {

            return db.Companies.FirstOrDefault(d => d.Id == id);

        }





        public GroupMaster GetGroupMaster(int id)
        {

            return db.GroupMasters.Where(d => d.groupID == id).FirstOrDefault();

        }
        public string GetLedgerMaster(long id)
        {

            return db.LedgerMasters.FirstOrDefault(d => d.LID == id).ledgerName;

        }
        public IQueryable<menuuser> GetGroupName(int type, long companyid, long branchid)
        {

            var selectedrecords = from d in db.GroupMasters
                                  where d.GrouptypeId == type && ((d.CompanyId == 0 && d.BranchId == 0) || (d.CompanyId == companyid && d.BranchId == branchid))

                                  select new menuuser
                                  {
                                      Id = d.groupID,
                                      Name = d.groupName

                                  };
            return selectedrecords;


        }
       
        public OpeningBalance GetOpeningbalance(int id)
        {

            return db.OpeningBalances.Where(d => d.ledgerID == id).FirstOrDefault();

        }
        public GroupMaster GetGroupName(int id)
        {

            return db.GroupMasters.Where(d => d.groupID == id).FirstOrDefault();

        }



        public GroupMaster GetGroupType(int id)
        {

            return db.GroupMasters.Where(d => d.groupID == id).FirstOrDefault();

        }

        public IQueryable<menuuser> GetSubgrpName(int type, long companyid, long branchid)
        {

            var selectedrecords = from d in db.GroupMasters
                                  where d.ParentGroupId == type && (d.CompanyId == 0 || d.BranchId == 0 || d.CompanyId == companyid || d.BranchId == branchid)

                                  select new menuuser
                                  {
                                      Id =  d.groupID,
                                      Name = d.groupName

                                  };
            return selectedrecords;


        }
        public IQueryable<menuuser> GetLedName(int type, long companyid, long branchid)
        {

            var selectedrecords = from d in db.LedgerMasters
                                  where d.groupID == type && (d.CompanyId == companyid && d.BranchId == branchid)

                                  select new menuuser
                                  {
                                      Id2 = d.LID,
                                      Name = d.ledgerName

                                  };
            return selectedrecords;
        }

        //--------------------28-07---------------//
        public DateCultureFormat Getdateformat(int id)
        {

            return db.DateCultureFormats.Where(d => d.Id == id).FirstOrDefault();

        }

        public Role GetRole(int id)
        {

            return db.Roles.Where(d => d.Id == id).FirstOrDefault();

        }

        //-------------16072014--------Naresh---//

        public long InsertTax(Tax tax)
        {
            if (tax.TaxId == 0)
            {
                db.Taxes.Add(tax);
                db.SaveChanges();
            }
            return tax.TaxId;
        }


        public long Insertcompanyid(Company company)
        {
            if (company.Id == 0)
            {
                db.Companies.Add(company);
                db.SaveChanges();
            }
            return company.Id;
        }

        public long InsertBranchid(BranchMaster branch)
        {
            if (branch.Id == 0)
            {
                db.BranchMasters.Add(branch);
                db.SaveChanges();
            }
            return branch.Id;
        }

        #region //----Naresh---------120814//



        public Company Getcompanydateformat(int id)
        {

            return db.Companies.Where(d => d.Id == id).FirstOrDefault();

        }


        public IEnumerable<menuuser> GetMenuheader(int userid, int compid)
        {

            var menu = from submenu in db.MenuMasters
                       join access in db.MenuaccessUsers
                       on submenu.Name equals access.Name
                       where access.AssignedUserId == userid && access.CompanyId == compid

                       select new menuuser
                       {

                           Id = submenu.Id,
                           Name = submenu.Name,
                           AccessMenuHeaderId = submenu.AccessMenuHeaderId

                       };

            return menu;
        }

        public int Insertfinancial(FinancialYearMaster finan)
        {
            if (finan.fYearID == 0)
            {
                db.FinancialYearMasters.Add(finan);
                db.SaveChanges();
            }
            return finan.fYearID;
        }

        //--------19082014---------------//

        public IQueryable<menuuser> GetUsersName(int type, long companyid, long UserId)
        {

            var selectedrecords = from d in db.Users
                                  where d.RoleId == type && (d.CompanyId == companyid && d.UserId == UserId)

                                  select new menuuser
                                  {
                                      Id = d.Id,
                                      Name = d.FirstName + d.LastName + "(" + d.UserName + ")"

                                  };
            return selectedrecords;


        }
        public IQueryable<menuuser> GetWarehouse(int type, long companyid, long UserId, long branch)
        {

            var selectedrecords = from d in db.Warehouses
                                  where d.Id != type && d.Companyid == companyid && d.Userid == UserId && d.Branchid == branch

                                  select new menuuser
                                  {
                                      Id2 = d.Id,
                                      Name = d.Name

                                  };
            return selectedrecords;


        }


        public BranchMaster GetBranch(int id)
        {

            return db.BranchMasters.Where(d => d.Id == id).FirstOrDefault();

        }



        public Tax Gettaxrate(long? taxid)
        {

            return db.Taxes.Where(d => d.TaxId == taxid).FirstOrDefault();

        }

        #endregion

    }
}