using System;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using EmoteMod.Features;
using EmoteMod.Hooks;
using EmoteMod.Utility;
using Monocle;

namespace EmoteMod.Module
{
    public class EmoteModModule : EverestModule
    {

        // emotemod settigns
        public override Type SettingsType => typeof(EmoteModModuleSettings);
        public static EmoteModModuleSettings Settings => (EmoteModModuleSettings)Instance._Settings;
        public override Type SessionType => typeof(EmoteModModuleSession);
        public static EmoteModModuleSession Session => (EmoteModModuleSession)Instance._Session;

        public static Celeste.Mod.CelesteNet.Client.CelesteNetClientSettings celestenetSettings = Celeste.Mod.CelesteNet.Client.CelesteNetClientModule.Settings;

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
        public static void echo(string text, bool debug = true)
        {
            try
            {
                if (!debug)
                    Engine.Commands.Log(text);
#if DEBUG
                else
                    Engine.Commands.Log(text);
#endif
            }
            catch { }
        }
        // TODO: this looks dumb, find a better way
        public override void LoadSettings()
        {
            base.LoadSettings();
        }

        public override void SaveSettings()
        {
            base.SaveSettings();
        }
        public override void Initialize()
        {
            base.Initialize();

            AnimationHelper.Init();
        }

        private void convert_to_v2()
        {
            if (!Settings.ConvertedToV2)
            {
                Settings.Emotes.Clear();
                Settings.Emotes.Add(new EmoteEntry(Settings.emote0, Settings.button0, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote1, Settings.button1, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote2, Settings.button2, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote3, Settings.button3, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote4, Settings.button4, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote5, Settings.button5, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote6, Settings.button6, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote7, Settings.button7, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote8, Settings.button8, true));
                Settings.Emotes.Add(new EmoteEntry(Settings.emote9, Settings.button9, true));

                Settings.ConvertedToV2 = true;
                SaveSettings();
            }
        }

        public override void Load()
        {
            anim_by_game = 0; // this tells that base state is no animations

            On.Celeste.GFX.LoadData += on_gfx_load_data;

            GravityHooks.Load();
            EmoteHooks.Load();
            BackpackHooks.Load();
            Speed.Load();
            Stretcher.Load();
            EmoteWheel.Load();
            MadhuntNerf.Load();
            CNetHelper.Load();
            PauseButton.Load();

            if (Engine.Scene is Level)
                foreach (EmoteEntry e in Settings.Emotes)
                    e.RefreshInfo();
        }

        private void on_gfx_load_data(On.Celeste.GFX.orig_LoadData orig)
        {
            orig();

            convert_to_v2();
        }

        public override void Unload()
        {

            EmoteHooks.Unload();
            BackpackHooks.Unload();
            Speed.Unload();
            Stretcher.Unload();
            GravityHooks.Unload();
            EmoteWheel.Unload();
            MadhuntNerf.Unload();
            CNetHelper.Unload();
        }
    }
}
