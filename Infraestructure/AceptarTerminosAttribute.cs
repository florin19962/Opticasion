using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Opticasion.Infraestructure
{
    public class AceptarTerminosAttribute :Attribute, IModelValidator
    {
        public String ErrorMessage { get; set; } = "Debe ser un valor true";
        public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
        {
            if ((context.Model as Boolean?).Equals(false))
            {
                return new List<ModelValidationResult>() { new ModelValidationResult("", this.ErrorMessage) };
            }
            else
            {
                return Enumerable.Empty<ModelValidationResult>();
            }
        }
    }
}
