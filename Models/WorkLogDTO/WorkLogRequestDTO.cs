using System.ComponentModel.DataAnnotations;

namespace MomsAppApi.Models.WorkLogDTO
{
    public class WorkLogRequestDTO : IValidatableObject
    {
        private static readonly TimeSpan MaxAllowedShiftDuration = TimeSpan.FromHours(16);
        private static readonly TimeSpan FutureClockSkewAllowance = TimeSpan.FromMinutes(10);

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "assignment_id must be a positive integer.")]
        public int assignment_id { get; set; }

        [Required]
        public DateTime started_at { get; set; }

        [Required]
        public DateTime ended_at { get; set; }

        [MaxLength(2000)]
        public string notes { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var startedUtc = started_at.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(started_at, DateTimeKind.Utc)
                : started_at.ToUniversalTime();

            var endedUtc = ended_at.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(ended_at, DateTimeKind.Utc)
                : ended_at.ToUniversalTime();

            if (endedUtc <= startedUtc)
            {
                yield return new ValidationResult(
                    "ended_at must be later than started_at.",
                    [nameof(ended_at), nameof(started_at)]);
            }

            var duration = endedUtc - startedUtc;
            if (duration > MaxAllowedShiftDuration)
            {
                yield return new ValidationResult(
                    $"Shift duration cannot exceed {MaxAllowedShiftDuration.TotalHours:0} hours.",
                    [nameof(ended_at), nameof(started_at)]);
            }

            var nowUtc = DateTime.UtcNow;
            if (startedUtc > nowUtc.Add(FutureClockSkewAllowance) || endedUtc > nowUtc.Add(FutureClockSkewAllowance))
            {
                yield return new ValidationResult(
                    "started_at and ended_at cannot be in the future.",
                    [nameof(started_at), nameof(ended_at)]);
            }
        }
    }
}
