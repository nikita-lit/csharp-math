using System.ComponentModel.DataAnnotations;

namespace csharp_math.Models
{
    public class FunctionInputModel : IValidatableObject
    {
        public int? Id { get; set; }

        public bool RestrictDomain { get; set; }

        [Required(ErrorMessage = "Sisesta funktsiooni valem.")]
        [StringLength(200, ErrorMessage = "Valem võib olla kuni 200 märki pikk.")]
        [Display(Name = "Funktsiooni valem")]
        public string Formula { get; set; } = string.Empty;

        [Range(-1000, 1000, ErrorMessage = "Vahemiku otspunkt peab olema lõigus [-1000; 1000].")]
        [Display(Name = "Vahemiku algus")]
        public double RangeFrom { get; set; } = -5;

        [Range(-1000, 1000, ErrorMessage = "Vahemiku otspunkt peab olema lõigus [-1000; 1000].")]
        [Display(Name = "Vahemiku lõpp")]
        public double RangeTo { get; set; } = 5;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var error = Services.FunctionAnalyzer.ValidateRange(RangeFrom, RangeTo);
            if (error != null)
            {
                yield return new ValidationResult(error, [nameof(RangeTo)]);
            }
        }
    }
}
