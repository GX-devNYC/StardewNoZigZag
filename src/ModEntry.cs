using HarmonyLib;
using NoZigZag.API;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;

namespace NoZigZag.src
{
	internal sealed class ModEntry : Mod
{
		public ModConfig Config;

		public override void Entry(IModHelper helper)
		{
			this.Config = this.Helper.ReadConfig<ModConfig>();
			helper.Events.GameLoop.GameLaunched += OnGameLaunched;
		}

		// when game is launched: setup the config menu
		public void OnGameLaunched(object? sender, GameLaunchedEventArgs e)
		{
			SetConfigMenu(sender, e);
		}

		public void HarmonyPatching()
		{
			Harmony harmony = new(ModManifest.UniqueID);

			harmony.Patch(
				original: AccessTools.Method(typeof(PathFindController), nameof(PathFindController.findPath)),
				prefix: new HarmonyMethod(typeof(HarmonyPathfinding), nameof(HarmonyPathfinding.findPath_prefix))
			);
		}

		// Generic Config Menu setup
		public void SetConfigMenu(object? sender, GameLaunchedEventArgs e)
		{
			// get Generic Mod Config Menu's API (if it's installed)
			var configMenu = this.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
			if (configMenu is null)
				return;

			// register mod
			configMenu.Register(
					mod: this.ModManifest,
					reset: () => this.Config = new ModConfig(),
					save: () => this.Helper.WriteConfig(this.Config)
			);

			// add some config options
			configMenu.AddBoolOption(
					mod: this.ModManifest,
					getValue: () => this.Config.ModEnabled,
					setValue: value => this.Config.ModEnabled = value,
					name: () => "Allow mod to disable NPC zigzagging",
					tooltip: () => "This is Stardew NoZigZag's entire function."
			);
		}
	}
}