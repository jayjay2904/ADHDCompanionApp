using ADHDCompanionApp.Models.Entities;
using ADHDCompanionApp.Services.Interfaces;

namespace ADHDCompanionApp.Services.Implementations;

public class TruthBombService : ITruthBombService
{
    private readonly List<TruthBomb> _truthBombs = new()
    {
        new TruthBomb { Text = "Progress over perfection." },
        new TruthBomb { Text = "You don’t need motivation. You need a tiny first step." },
        new TruthBomb { Text = "Done is better than perfect." },
        new TruthBomb { Text = "Low energy doesn’t mean no value." },
        new TruthBomb { Text = "You’re not broken. Your brain just works differently." }
    };

    public Task<TruthBomb> GetTruthBombAsync()
    {
        var random = new Random();
        var bomb = _truthBombs[random.Next(_truthBombs.Count)];

        return Task.FromResult(bomb);
    }
}