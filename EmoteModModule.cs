using System;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.EmoteMod {
    public class EmoteModModule : EverestModule {

        // emotemod settigns
        public override Type SettingsType => typeof(EmoteModModuleSettings);
        public static EmoteModModuleSettings Settings => (EmoteModModuleSettings) Instance._Settings;
        public override Type SessionType => typeof(EmoteModModuleSession);
        public static EmoteModModuleSession Session => (EmoteModModuleSession) Instance._Session;

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
			base.LoadSettings();
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
        }
    }
}
