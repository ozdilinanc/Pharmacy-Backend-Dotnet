using PharmacyProject.Application.DTOs.Pharmacy;
using PharmacyProject.Application.Interfaces.Repositories;
using PharmacyProject.Application.Interfaces.Services;
using PharmacyProject.Core.Entities;

namespace PharmacyProject.Application.Services
{
    public class AdminManualMatchService : IAdminManualMatchService
    {
        private readonly IUnmatchedPharmacyRepository _unmatchedPharmacyRepository;
        private readonly IPharmacyRepository _pharmacyRepository;
        private readonly IPharmacyInsuranceRepository _pharmacyInsuranceRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AdminManualMatchService(
            IUnmatchedPharmacyRepository unmatchedPharmacyRepository,
            IPharmacyRepository pharmacyRepository,
            IPharmacyInsuranceRepository pharmacyInsuranceRepository,
            IUnitOfWork unitOfWork)
        {
            _unmatchedPharmacyRepository = unmatchedPharmacyRepository;
            _pharmacyRepository = pharmacyRepository;
            _pharmacyInsuranceRepository = pharmacyInsuranceRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<UnmatchedPharmacy>> GetUnmatchedPharmaciesAsync()
        {
            // Yalnızca henüz çözümlenmemiş olanları (IsResolved == false) getir
            return await _unmatchedPharmacyRepository.FindAsync(u => !u.IsResolved);
        }

        public async Task MatchPharmacyAsync(ManualMatchRequestDto matchRequestDto)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(matchRequestDto.UnmatchedPharmacyId);
            if (unmatched == null || unmatched.IsResolved)
            {
                throw new Exception("Karantina kaydı bulunamadı veya zaten çözümlenmiş.");
            }

            var realPharmacy = await _pharmacyRepository.GetByIdAsync(matchRequestDto.RealPharmacyId); // User defined RealPharmacyId originally!
            if (realPharmacy == null)
            {
                throw new Exception("Hedef eczane veritabanında bulunamadı.");
            }

            // Sigortası varsa eczane ile bağla
            if (unmatched.SourceInsurance.HasValue)
            {
                // Mevcut bir ilişki var mı kontrol et
                var existingRelation = await _pharmacyInsuranceRepository.FindAsync(pi => 
                    pi.PharmacyId == realPharmacy.Id && 
                    pi.InsuranceCompanyId == (int)unmatched.SourceInsurance.Value);

                if (!existingRelation.Any())
                {
                    await _pharmacyInsuranceRepository.AddAsync(new PharmacyInsurance
                    {
                        PharmacyId = realPharmacy.Id,
                        InsuranceCompanyId = (int)unmatched.SourceInsurance.Value
                    });
                }
            }

            // Unmatched kaydını çözümlendi olarak işaretle
            unmatched.IsResolved = true;
            unmatched.MatchedPharmacyId = realPharmacy.Id;
            unmatched.UpdatedAt = DateTime.UtcNow;

            _unmatchedPharmacyRepository.Update(unmatched);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteUnmatchedPharmacyAsync(int unmatchedPharmacyId)
        {
            var unmatched = await _unmatchedPharmacyRepository.GetByIdAsync(unmatchedPharmacyId);
            if (unmatched != null)
            {
                await _unmatchedPharmacyRepository.DeleteByIdAsync(unmatchedPharmacyId);
                await _unitOfWork.SaveChangesAsync();
            }
        }
    }
}
