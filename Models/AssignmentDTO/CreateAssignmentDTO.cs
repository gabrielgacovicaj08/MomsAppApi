using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.AssignmentDTO
{
    public class CreateAssignmentDTO : IValidatableObject
    {
        [Required]
        public DateOnly work_date { get; set; }

        [Range(1, int.MaxValue)]
        public int employee_id { get; set; }

        [Range(1, int.MaxValue)]
        public int structure_id { get; set; }

        public TimeOnly? shift_start { get; set; }
        public TimeOnly? shift_end { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (shift_start.HasValue && shift_end.HasValue && shift_end <= shift_start)
            {
                yield return new ValidationResult(
                    "shift_end must be later than shift_start.",
                    new[] { nameof(shift_start), nameof(shift_end) });
            }
        }
    }
}
