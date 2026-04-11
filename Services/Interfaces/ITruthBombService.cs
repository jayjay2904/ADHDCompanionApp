using ADHDCompanionApp.Models.Entities;

namespace ADHDCompanionApp.Services.Interfaces;

public interface ITruthBombService
{
    Task<TruthBomb> GetTruthBombAsync();
}