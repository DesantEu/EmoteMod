using System;
using Monocle;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteModModule : EverestModule
    {

        // emotemod settigns
        public override Type SettingsType => typeof(EmoteModModuleSettings);
        public static EmoteModModuleSettings Settings => (EmoteModModuleSettings)Instance._Settings;
        public override Type SessionType => typeof(EmoteModModuleSession);
        public static EmoteModModuleSession Session => (EmoteModModuleSession)Instance._Session;

        public static CelesteNet.Client.CelesteNetClientSettings celestenetSettings = CelesteNet.Client.CelesteNetClientModule.Settings;

        /// <summary>
		/// If 0 we can make an animation;
		/// If 1 we are playing animation by hand;
		/// If 2 the game is playing an animation and we cant emote
		/// </summary>
		public static int anim_by_game;

        public static EmoteModModule Instance { get; private set; }
        public EmoteModModule()
        {
            Instance = this;
        }

        // easier way to yea
        public static void echo(string text)
        {
            try
            {
                Engine.Commands.Log(text);
            }
            catch { }
        }
        // TODO: this looks dumb, find a better way
        public override void LoadSettings()
        {
            AnimationHelper.Init();
            base.LoadSettings();

            if (!Settings.ConvertedToV2)
            {
                Settings.Emotes.Add(new EmoteEntry(Settings.emote0, Settings.button0));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote1, Settings.button1));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote2, Settings.button2));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote3, Settings.button3));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote4, Settings.button4));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote5, Settings.button5));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote6, Settings.button6));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote7, Settings.button7));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote8, Settings.button8));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote9, Settings.button9));

                Settings.ConvertedToV2 = true;
                SaveSettings();
            }
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
        }

        public override void Load()
        {
            anim_by_game = 0; // this tells that base state is no animations

            Gravity.Load();
            Emote.Load();
            EmoteCancel.Load();
            BackpackChanger.Load();
            Speed.Load();
            Stretcher.Load();
            EmoteWheel.Load();
            MadhuntNerf.Load();
            CNetHelper.Load();

            if (Engine.Scene is Level)
                foreach (EmoteEntry e in Settings.Emotes)
                    e.RefreshInfo();
        }

        public override void Unload()
        {
            EmoteCancel.cancelEmote();

            Emote.Unload();
            EmoteCancel.Unload();
            BackpackChanger.Unload();
            Speed.Unload();
            Stretcher.Unload();
            Gravity.Unload();
            EmoteWheel.Unload();
            MadhuntNerf.Unload();
            CNetHelper.Unload();
        }
    }
}
