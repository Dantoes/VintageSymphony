using Vintagestory.API.Client;

namespace VintageSymphony.UpdateNotifier;

#nullable disable
public class AvailableUpdateOverlay : HudElement
{
	private GuiComposer debugTextComposer;
	private GuiElementDynamicText textElement;
	private string text = "Vintage Symphony update available";

	public AvailableUpdateOverlay(ICoreClientAPI api)
		: base(api)
	{
		SetupDialog();
	}

	private void SetupDialog()
	{
		debugTextComposer = capi.Gui
			.CreateCompo("updateScreenText",
				ElementBounds.Percentual(EnumDialogArea.LeftTop, 1, 0.3).WithFixedAlignmentOffset(5.0, 5.0))
			.AddDynamicText(text, CairoFont.WhiteSmallishText(),
				ElementBounds.Fill, "updateScreenTextElem").OnlyDynamic()
			.Compose();
		textElement = debugTextComposer.GetDynamicText("updateScreenTextElem");
	}

	public override void OnFinalizeFrame(float dt)
	{
		debugTextComposer.PostRender(dt);
	}

	public override void OnRenderGUI(float deltaTime)
	{
		debugTextComposer.Render(deltaTime);
	}

	public void SetVersion(Version currentVersion, Version newVersion)
	{
		text = $"Vintage Symphony update available: {newVersion} (current: {currentVersion})";
		textElement.SetNewTextAsync(text);
	}
}