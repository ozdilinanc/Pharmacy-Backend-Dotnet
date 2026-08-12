namespace PharmacyProject.Application.DTOs.Pharmacy
{
    public class ManualMatchRequestDto
    {
        public int UnmatchedPharmacyId { get; set; }

        public int RealPharmacyId { get; set; }
    }
}