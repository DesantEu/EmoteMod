using Microsoft.Xna.Framework.Input;
using Celeste.Mod.UI;
namespace Celeste.Mod.EmoteMod
{
    class EmoteBindings : OuiGenericMenu, OuiModOptions.ISubmenu
    {
        public override string MenuName => "Emotes";

        protected override void addOptionsToMenu(TextMenu menu)
        {
            //menu.Add(new TextMenu.Button("Emote 0").Pressed())
        }
    }
    [SettingName("settings_modname")]
    public class EmoteSettings : EverestModuleSettings
    {
        #region default control and emote settings
        [SettingName("settings_emote0")]
        public string emote0 { get; set; } = "spin";
        [SettingName("settings_emote0")]
        [DefaultButtonBinding(0, Keys.NumPad0)]
        public ButtonBinding button0 { get; set; }
        [SettingName("settings_emote1")]
        public string emote1 { get; set; } = "sleep";
        [SettingName("settings_emote1")]
        [DefaultButtonBinding(0, Keys.NumPad1)]
        public ButtonBinding button1 { get; set; }
        [SettingName("settings_emote2")]
        public string emote2 { get; set; } = "wakeup";
        [SettingName("settings_emote2")]
        [DefaultButtonBinding(0, Keys.NumPad2)]
        public ButtonBinding button2 { get; set; }
        [SettingName("settings_emote3")]
        public string emote3 { get; set; } = "laugh";
        [SettingName("settings_emote3")]
        [DefaultButtonBinding(0, Keys.NumPad3)]
        public ButtonBinding button3 { get; set; }
        [SettingName("settings_emote4")]
        public string emote4 { get; set; } = "idlea";
        [SettingName("settings_emote4")]
        [DefaultButtonBinding(0, Keys.NumPad4)]
        public ButtonBinding button4 { get; set; }
        [SettingName("settings_emote5")]
        public string emote5 { get; set; } = "idleb";
        [SettingName("settings_emote5")]
        [DefaultButtonBinding(0, Keys.NumPad5)]
        public ButtonBinding button5 { get; set; }
        [SettingName("settings_emote6")]
        public string emote6 { get; set; } = "idlec";
        [SettingName("settings_emote6")]
        [DefaultButtonBinding(0, Keys.NumPad6)]
        public ButtonBinding button6 { get; set; }
        [SettingName("settings_emote7")]
        public string emote7 { get; set; } = "tired";
        [SettingName("settings_emote7")]
        [DefaultButtonBinding(0, Keys.NumPad7)]
        public ButtonBinding button7 { get; set; }
        [SettingName("settings_emote8")]
        public string emote8 { get; set; } = "hug";
        [SettingName("settings_emote8")]
        [DefaultButtonBinding(0, Keys.NumPad8)]
        public ButtonBinding button8 { get; set; }
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


        // you saw nothing
        [SettingIgnore]
        public int Backpack { get; set; } = 0;
    }
}
