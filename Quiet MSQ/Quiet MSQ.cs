using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;

namespace Quiet_MSQ;

public class QuietMsq : IDalamudPlugin
{
    public string Name => "Quiet MSQ";

    private DalamudPluginInterface PluginInterface { get; init; }
    private GameConfig GameConfig { get; }
    private ClientState ClientState { get; }
    private Condition PlayerState { get; }

    public QuietMsq(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] GameConfig gameConfig,
        [RequiredVersion("1.0")] ClientState clientState,
        [RequiredVersion("1.0")] Condition playerState)
    {
        PluginInterface = pluginInterface;
        GameConfig = gameConfig;
        ClientState = clientState;
        PlayerState = playerState;

        PlayerState.ConditionChange += OnConditionChange;
    }

    private void OnConditionChange(ConditionFlag flag, bool inCutscene)
    {
        if (flag is not (ConditionFlag.WatchingCutscene or ConditionFlag.OccupiedInCutSceneEvent
            or ConditionFlag.WatchingCutscene78)) return;
        PluginLog.Debug(inCutscene ? $"Cutscene is playing." : $"Cutscene is over.");

        var zone = ClientState.TerritoryType;
        PluginLog.Debug($"Am I in a MSQ Roulette Zone? {zone.ToString()}");
        // Castrum = 1043 // Praetoritum = 1044 // Porta Decumana = 1052
        if (zone is not (1043 or 1044 or 1052)) return;

        GameConfig.System.TryGet("IsSndMaster", out bool previousState);
        GameConfig.System.Set("IsSndMaster", !inCutscene && previousState);
    }

    public void Dispose()
    {
        // nothing to dispose
    }
}
