using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
namespace Celeste.Mod.EmoteMod
{
	//so this is for emotes category
	class EmoteBindings : OuiGenericMenu, OuiModOptions.ISubmenu // emote binds submenu class
	{
		public override string MenuName => "Emotes"; // text above things

		protected override void addOptionsToMenu(TextMenu menu) // submenu
		{
			menu.Add(new TextMenu.Button($"1. {EmoteModMain.Settings.emote0}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote0,
					v => EmoteModMain.Settings.emote0 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"2. {EmoteModMain.Settings.emote1}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote1,
					v => EmoteModMain.Settings.emote1 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"3. {EmoteModMain.Settings.emote2}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote2,
					v => EmoteModMain.Settings.emote2 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"4. {EmoteModMain.Settings.emote3}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote3,
					v => EmoteModMain.Settings.emote3 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"5. {EmoteModMain.Settings.emote4}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote4,
					v => EmoteModMain.Settings.emote4 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"6. {EmoteModMain.Settings.emote5}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote5,
					v => EmoteModMain.Settings.emote5 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"7. {EmoteModMain.Settings.emote6}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote6,
					v => EmoteModMain.Settings.emote6 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"8. {EmoteModMain.Settings.emote7}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote7,
					v => EmoteModMain.Settings.emote7 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"9. {EmoteModMain.Settings.emote8}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote8,
					v => EmoteModMain.Settings.emote8 = v,
					12,
					1
				);
			}));

			menu.Add(new TextMenu.Button($"10. {EmoteModMain.Settings.emote9}").Pressed(() => {
				Audio.Play(SFX.ui_main_savefile_rename_start);
				menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(
					EmoteModMain.Settings.emote9,
					v => EmoteModMain.Settings.emote9 = v,
					12,
					1
				);
			}));
		}
	}

	//this is what shows up in settings

	[SettingName("settings_modname")]
	public class EmoteSettings : EverestModuleSettings
	{
		#region default control and emote settings
		// emotes
		[SettingIgnore]
		[SettingName("settings_emote0")]
		public string emote0 { get; set; } = "spin";
		[SettingName("settings_emote0")]
		[DefaultButtonBinding(0, Keys.NumPad0)]
		public ButtonBinding button0 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote1")]
		public string emote1 { get; set; } = "sleep";
		[SettingName("settings_emote1")]
		[DefaultButtonBinding(0, Keys.NumPad1)]
		public ButtonBinding button1 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote2")]
		public string emote2 { get; set; } = "wakeup";
		[SettingName("settings_emote2")]
		[DefaultButtonBinding(0, Keys.NumPad2)]
		public ButtonBinding button2 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote3")]
		public string emote3 { get; set; } = "laugh";
		[SettingName("settings_emote3")]
		[DefaultButtonBinding(0, Keys.NumPad3)]
		public ButtonBinding button3 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote4")]
		public string emote4 { get; set; } = "idlea";
		[SettingName("settings_emote4")]
		[DefaultButtonBinding(0, Keys.NumPad4)]
		public ButtonBinding button4 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote5")]
		public string emote5 { get; set; } = "idleb";
		[SettingName("settings_emote5")]
		[DefaultButtonBinding(0, Keys.NumPad5)]
		public ButtonBinding button5 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote6")]
		public string emote6 { get; set; } = "idlec";
		[SettingName("settings_emote6")]
		[DefaultButtonBinding(0, Keys.NumPad6)]
		public ButtonBinding button6 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote7")]
		public string emote7 { get; set; } = "tired";
		[SettingName("settings_emote7")]
		[DefaultButtonBinding(0, Keys.NumPad7)]
		public ButtonBinding button7 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote8")]
		public string emote8 { get; set; } = "hug";
		[SettingName("settings_emote8")]
		[DefaultButtonBinding(0, Keys.NumPad8)]
		public ButtonBinding button8 { get; set; }
		[SettingIgnore]
		[SettingName("settings_emote9")]
		public string emote9 { get; set; } = "fallfast";
		[SettingName("settings_emote9")]
		[DefaultButtonBinding(0, Keys.NumPad9)]
		public ButtonBinding button9 { get; set; }
		#endregion  

		// gravity settings
		[SettingName("settings_gravityCancel_name")]
		[SettingSubText("settings_gravityCancel_desc")]
		public bool CancelGravity { get; set; } = true;


		// this is for speed
		[SettingInGame(true)]
		public int AnimationSpeed { get; set; } = 9;
		public void CreateAnimationSpeedEntry(TextMenu menu, bool inGame)
		{
			menu.Add(new TextMenu.Slider(Dialog.Clean("settings_speed_name"), EmoteModMain.speedFormatter, 0, EmoteModMain.speeds.Length - 1, EmoteModMain.Settings.AnimationSpeed)
				.Change(id => EmoteModMain.Settings.AnimationSpeed = id));
		}

		// new cool emotes category thing
		public string EmoteBindings { get; set; } = ""; // do not remove or it wont work
		public void CreateEmoteBindingsEntry(TextMenu menu, bool inGame) // create emotes submenu
		{
			if (!inGame)
			{
				menu.Add(new TextMenu.Button("Emotes Config")
					.Pressed(() => OuiGenericMenu.Goto<EmoteBindings>(overworld => overworld.Goto<OuiModOptions>(), new object[0])));
			}
		}

		// you saw nothing
		[SettingIgnore]
		public int Backpack { get; set; } = 0;
		public void CreateBackpackEntry(TextMenu menu, bool inGame)
		{
			menu.Add(new TextMenu.Slider(Dialog.Clean("settings_backpack_name"), EmoteModMain.backpackFormatter, 0, 2, EmoteModMain.Settings.Backpack)
				.Change(id => EmoteModMain.Settings.Backpack = id));
		}
	}
}
