using Vintagestory.API.Client;

namespace VintageSymphony.Config;

#nullable disable
public class ConfigurationDialog : GuiDialog
{
	private const string LoadGameMusicToggleKey = "vscfg_gameMusicToggle";
	private const string LoadVintageSymphonyMusicToggleKey = "vscfg_modMusicToggle";

	private readonly Configuration configuration;
	private readonly ConfigurationLoader configurationLoader;

	public ConfigurationDialog(ICoreClientAPI api, Configuration configuration, ConfigurationLoader configurationLoader)
		: base(api)
	{
		this.configuration = configuration;
		this.configurationLoader = configurationLoader;
		SetupDialog();
	}

	private void SetupDialog()
	{
		// Auto-sized dialog at the center of the screen
		ElementBounds dialogBounds = ElementStdBounds.AutosizedMainDialog.WithAlignment(EnumDialogArea.CenterMiddle);

		// Just a simple 300x300 pixel box
		ElementBounds textBounds = ElementBounds.Fixed(0, 40, 400, 100);

		// Background boundaries. Again, just make it fit it's child elements, then add the text as a child element
		ElementBounds bgBounds = ElementBounds.Fill.WithFixedPadding(GuiStyle.ElementToDialogPadding);
		bgBounds.BothSizing = ElementSizing.FitToChildren;
		bgBounds.WithChildren(textBounds);

		const int width = 350;
		const int height = 30;
		var bounds1 = ElementBounds.Fixed(10, 25, width, height);
		var bounds2 = ElementBounds.Fixed(10, 50, width, height);
		// Lastly, create the dialog
		SingleComposer = capi.Gui.CreateCompo("myAwesomeDialog", dialogBounds)
			.AddShadedDialogBG(bgBounds)
			.AddDialogTitleBar("Vintage Symphony configuration", OnTitleBarCloseClicked);
		
		var boundsChk1 = ElementBounds.Fixed(10, 50, 10, height);
		var boundsChk1Label = ElementBounds.Fixed(50, boundsChk1.fixedY + 5, width, boundsChk1.fixedHeight);
		SingleComposer.AddSwitch(ToggleLoadVintageSymphonyMusic, boundsChk1, LoadVintageSymphonyMusicToggleKey)
			.AddStaticText("Load Vintage Symphony music *", CairoFont.WhiteSmallText(), EnumTextOrientation.Left, boundsChk1Label);
		
		var boundsChk2 = ElementBounds.Fixed(10, 90, 10, height);
		var boundsChk2Label = ElementBounds.Fixed(50, boundsChk2.fixedY + 5, width, boundsChk2.fixedHeight);
		SingleComposer.AddSwitch(ToggleLoadGameMusic, boundsChk2, LoadGameMusicToggleKey)
			.AddStaticText("Load Vintage Story music *", CairoFont.WhiteSmallText(), EnumTextOrientation.Left, boundsChk2Label);
		
		var boundsNote = ElementBounds.Fixed(10, 140, width, height);
		SingleComposer.AddStaticText("* requires game restart", CairoFont.WhiteDetailText(), EnumTextOrientation.Left, boundsNote);

		SingleComposer.Compose();
		
		var modMusicToggle = SingleComposer.GetSwitch(LoadVintageSymphonyMusicToggleKey);
		modMusicToggle.On = configuration.LoadVintageSymphonyMusic;
		
		var gameMusicToggle = SingleComposer.GetSwitch(LoadGameMusicToggleKey);
		gameMusicToggle.On = configuration.LoadGameMusic;
	}

	private void ToggleLoadGameMusic(bool state)
	{
		configuration.LoadGameMusic = state;
		configurationLoader.SaveConfiguration(configuration);
	}

	private void ToggleLoadVintageSymphonyMusic(bool state)
	{
		configuration.LoadVintageSymphonyMusic = state;
		configurationLoader.SaveConfiguration(configuration);
	}

	private void OnTitleBarCloseClicked()
	{
		TryClose();
	}

	public override string ToggleKeyCombinationCode => "vintage-symphony-config";
}