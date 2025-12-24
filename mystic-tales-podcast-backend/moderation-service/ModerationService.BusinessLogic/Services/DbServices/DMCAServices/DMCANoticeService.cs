using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModerationService.BusinessLogic.DTOs.Cache;
using ModerationService.BusinessLogic.DTOs.DMCANotice.Details;
using ModerationService.BusinessLogic.DTOs.DMCANotice.ListItems;
using ModerationService.BusinessLogic.DTOs.LawsuitProof.Details;
using ModerationService.BusinessLogic.DTOs.LawsuitProof.ListItems;
using ModerationService.BusinessLogic.Helpers.DateHelpers;
using ModerationService.BusinessLogic.Services.CrossServiceServices.QueryServices;
using ModerationService.BusinessLogic.Services.DbServices.MiscServices;
using ModerationService.BusinessLogic.Services.DbServices.ReportServices;
using ModerationService.BusinessLogic.Services.MessagingServices.interfaces;
using ModerationService.DataAccess.Data;
using ModerationService.DataAccess.Entities.SqlServer;
using ModerationService.DataAccess.Repositories.interfaces;
using ModerationService.Infrastructure.Services.Kafka;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModerationService.BusinessLogic.Services.DbServices.DMCAServices
{
    public class DMCANoticeService
    {
        private readonly IGenericRepository<Dmcanotice> _dmcaNoticeGenericRepository;
        private readonly IGenericRepository<DmcanoticeAttachFile> _dmcaNoticeAttachFileGenericRepository;
        private readonly AccountCachingService _accountCachingService;

        private readonly ILogger<DMCANoticeService> _logger;
        private readonly KafkaProducerService _kafkaProducerService;
        private readonly IMessagingService _messagingService;
        private readonly HttpServiceQueryClient _httpServiceQueryClient;
        private readonly AppDbContext _appDbContext;

        private readonly DateHelper _dateHelper;
        public DMCANoticeService(
            IGenericRepository<Dmcanotice> dmcaNoticeGenericRepository,
            IGenericRepository<DmcanoticeAttachFile> dmcaNoticeAttachFileGenericRepository,
            AccountCachingService accountCachingService,
            ILogger<DMCANoticeService> logger,
            KafkaProducerService kafkaProducerService,
            IMessagingService messagingService,
            HttpServiceQueryClient httpServiceQueryClient,
            AppDbContext appDbContext,
            DateHelper dateHelper
            )
        {
            _dmcaNoticeGenericRepository = dmcaNoticeGenericRepository;
            _dmcaNoticeAttachFileGenericRepository = dmcaNoticeAttachFileGenericRepository;
            _accountCachingService = accountCachingService;
            _logger = logger;
            _kafkaProducerService = kafkaProducerService;
            _messagingService = messagingService;
            _httpServiceQueryClient = httpServiceQueryClient;
            _appDbContext = appDbContext;
            _dateHelper = dateHelper;
        }
        public async Task<Dmcanotice> CreateDMCANoticeAsync(Dmcanotice dmcaNotice)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _dmcaNoticeGenericRepository.CreateAsync(dmcaNotice);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Created DMCA Notice with ID: {DmcaNoticeId}", result.Id);
                    return result;
                } catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError($"Something went wrong. Error: {ex.StackTrace}");
                    return null;
                }
            //}
        }
        public async Task<Dmcanotice> UpdateDMCANoticeAsync(Dmcanotice dmcaNotice)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _dmcaNoticeGenericRepository.UpdateAsync(dmcaNotice.Id, dmcaNotice);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Updated DMCA Notice with ID: {DmcaNoticeId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError($"Something went wrong. Error: {ex.StackTrace}");
                    return null;
                }
            //}
        }
        public async Task<DmcanoticeAttachFile> CreateDMCANoticeAttachFile(DmcanoticeAttachFile dmcanoticeAttachFile)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _dmcaNoticeAttachFileGenericRepository.CreateAsync(dmcanoticeAttachFile);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Created DMCA Notice Attach File with ID: {DmcaNoticeAttachFileId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError($"Something went wrong. Error: {ex.StackTrace}");
                    return null;
                }
            //}
        }
        public async Task<DmcanoticeAttachFile> UpdateDMCANoticeAttachFile(DmcanoticeAttachFile dmcanoticeAttachFile)
        {
            //using (var transaction = await _appDbContext.Database.BeginTransactionAsync())
            //{
                try
                {
                    var result = await _dmcaNoticeAttachFileGenericRepository.UpdateAsync(dmcanoticeAttachFile.Id, dmcanoticeAttachFile);
                    //await transaction.CommitAsync();
                    _logger.LogInformation("Updated DMCA Notice Attach File with ID: {DmcaNoticeAttachFileId}", result.Id);
                    return result;
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    _logger.LogError($"Something went wrong. Error: {ex.StackTrace}");
                    return null;
                }
            //}
        }
        public async Task<DMCANoticeDetailResponseDTO> GetDMCANoticeByDMCAAccusationId(int dmcaAccusationId)
        {
            var dmcaNotice = _dmcaNoticeGenericRepository.FindAll()
                .Where(lp => lp.DmcaAccusationId == dmcaAccusationId)
                .FirstOrDefault();
            var dmcaNoticeAttachFile = await _dmcaNoticeAttachFileGenericRepository.FindAll()
                .Where(lpaf => lpaf.DmcaNoticeId.Equals(dmcaNotice.Id))
                .Select(lpaf => new DMCANoticeAttachFileListItemResponseDTO
                {
                    Id = lpaf.Id,
                    AttachFileKey = lpaf.AttachFileKey,
                    CreatedAt = lpaf.CreatedAt,
                })
                .ToListAsync();
            AccountStatusCache? staffAccount = null;
            if (dmcaNotice.ValidatedBy != null)
            {
                staffAccount = await _accountCachingService.GetAccountStatusCacheById(dmcaNotice.ValidatedBy.Value);
            }
            return new DMCANoticeDetailResponseDTO
            {
                Id = dmcaNotice.Id,
                IsValid = dmcaNotice.IsValid,
                InValidReason = dmcaNotice.InvalidReason,
                ValidatedBy = staffAccount != null ? staffAccount.FullName : null,
                ValidatedAt = dmcaNotice.ValidatedAt,
                DMCAAccusationId = dmcaNotice.DmcaAccusationId,
                CreatedAt = dmcaNotice.CreatedAt,
                UpdatedAt = dmcaNotice.UpdatedAt,
                DMCANoticeAttachFileList = dmcaNoticeAttachFile
            };
        }
    }
}
