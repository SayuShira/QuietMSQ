using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.Config;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace Quiet_MSQ;

public class QuietMsq : IDalamudPlugin
{
    public string Name => "Quiet MSQ";

    private DalamudPluginInterface PluginInterface { get; init; }
    private GameConfig GameConfig { get; }
    private ClientState ClientState { get; }
    private Condition PlayerState { get; }

    private bool _previousState;


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
        // Castrum = 1043 // Praetoritum = 1044 // Porta Decumana = 1048
        if (zone is not (1043 or 1044 or 1048)) return;

        // Get the previous state as we transition into a cutscene (otherwise will be overwritten)
        if (inCutscene) GameConfig.System.TryGet("IsSndMaster", out _previousState);

        PluginLog.Debug($"The state of master volume will be returned to {_previousState}");
        PluginLog.Debug($"Setting master volume to {inCutscene || _previousState}");

        GameConfig.System.Set("IsSndMaster", inCutscene || _previousState);


        // Candidates: 9u, 14u, 15u, 18u, 20u (Sell sound), 33u (photo), 34u, 35u (chat open), 36u (nice and clear), 37u (whisper)
        // 38u (nice and clear), 42u (jungle drum), 43u (nice and clear), 60u (enemy target sound), 62 (delayed, clear)
        // 79u (nice and clear)
        // Be careful: 19u, 21u, 74(?`this stops after a bit)

        // Play a sound if the cutscene is over
        if (!inCutscene) UIModule.PlaySound(79u, 0, 0, 0);
    }

    public void Dispose()
    {
        PlayerState.ConditionChange -= OnConditionChange;
    }
}
