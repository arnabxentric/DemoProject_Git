using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace XenERP.Models
{
    public class IngredientFormulaModelView
    {
        public long ProductId { get; set; }
        public List<nutriItem> itemList { get; set; }
    }

    public class nutriItem
    {
        public long NutritionId { get; set; }
        public string NutritionName { get; set; }
        public decimal NutriValue { get; set; }

    }
}