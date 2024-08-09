using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace VintageSymphony;

public abstract class BaseModSystem : ModSystem
{
    protected ICoreClientAPI? clientApi;
    protected bool IsGameStarted { get; private set; }

    public override void StartClientSide(ICoreClientAPI api)
    {
        clientApi = api;
        clientApi.Event.PlayerJoin += EventOnPlayerJoin;
    }

    public override void Dispose()
    {
        if (clientApi?.Event != null)
        {
            clientApi.Event.PlayerJoin -= EventOnPlayerJoin;
        }

        base.Dispose();
    }

    private void EventOnPlayerJoin(IClientPlayer player)
    {
        if (player == clientApi!.World.Player)
        {
            clientApi.Event.PlayerJoin -= EventOnPlayerJoin;
            IsGameStarted = true;
            OnGameStarted();
        }
    }

    protected virtual void OnGameStarted()
    {
    }
}