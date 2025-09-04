using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Web.Mvc;

namespace XenERP.Models
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotEqualToAttribute : ValidationAttribute, IClientValidatable
    {
        //public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        //{
        //    var rule = new ModelClientValidationRule
        //    {
        //        ErrorMessage = ErrorMessage,
        //        ValidationType = "notequalto"
        //    };
        //    rule.ValidationParameters.Add("other", OtherProperty);
        //   // rule.ValidationParameters["other"] = "#" + OtherProperty; // CSS Selector (won't work if applied to nested properties on a viewmodel)
        //    yield return rule;
        //}
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, name, OtherProperty);
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ErrorMessage = FormatErrorMessage(metadata.DisplayName),
                ValidationType = "notequalto"
            };
            rule.ValidationParameters.Add("other", OtherProperty);
            
            yield return rule;
        }
        public string OtherProperty { get; private set; }

        public NotEqualToAttribute(string otherProperty)
        {
            OtherProperty = otherProperty;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            //var property = validationContext.ObjectType.GetProperty(OtherProperty);
            //if (property == null)
            //{
            //    return new ValidationResult(
            //        string.Format(
            //            CultureInfo.CurrentCulture,
            //            "{0} is unknown property",
            //            OtherProperty
            //        ), new[] { validationContext.MemberName }
            //    );
            //}
            //var otherValue = property.GetValue(validationContext.ObjectInstance, null);
            //if (value == otherValue)
            //{
            //    return new ValidationResult(
            //        FormatErrorMessage(validationContext.DisplayName),
            //        new[] { validationContext.MemberName });
            //}
            //return ValidationResult.Success;
            decimal thisValue;
            decimal otherValue;
            PropertyInfo propertyInfo;
            try
            {
                thisValue = Convert.ToDecimal(value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Property Type is not numeric", ex);
            }
            try
            {
                propertyInfo = validationContext.ObjectType.GetProperty(OtherProperty);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Other property not found", ex);
            }
            try
            {
                otherValue = Convert.ToDecimal(propertyInfo.GetValue(validationContext.ObjectInstance, null));
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Other property type is not numeric", ex);
            }
            if(thisValue == otherValue)
            {
                return new ValidationResult(
                    FormatErrorMessage(validationContext.DisplayName),
                    new[] { validationContext.MemberName });
            }
            return ValidationResult.Success;
        }
    }
}
