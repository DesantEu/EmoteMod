using Monocle;
using System;

namespace Celeste.Mod.EmoteMod
{
	public class EmoteModMain : EverestModule
	{
		
		// emotemod settigns
		public override Type SettingsType => typeof(EmoteSettings);
		public static EmoteSettings Settings => (EmoteSettings)Instance._Settings;
		// celestenet settings
		public static CelesteNet.Client.CelesteNetClientSettings celestenetSettings = CelesteNet.Client.CelesteNetClientModule.Settings;
		
		/// <summary>
		/// If 0 we can make an animation;
		/// If 1 we are playing animation by hand;
		/// If 2 the game is playing an animation and we cant emote
		/// </summary>
		public static int anim_by_game;
		// Only one alive module instance can exist at any given time.
		public static EmoteModMain Instance;


		// i have no idea what is this for
		public EmoteModMain()
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

            GravityModule.Load();
            EmoteModule.Load();
            EmoteCancelModule.Load();
            BackpackModule.Load();
            SpeedModule.Load();
            EmoteStretcher.Load();
        }

		public override void Unload()
		{
            EmoteCancelModule.cancelEmote();

            EmoteModule.Unload();
            EmoteCancelModule.Unload();
            BackpackModule.Unload();
            SpeedModule.Unload();
            EmoteStretcher.Unload();
            GravityModule.Unload();
        }

		#region garbage

		// Optional, initialize anything after Celeste has initialized itself properly.
		public override void Initialize()
		{

		}

		// Optional, do anything requiring either the Celeste or mod content here.
		public override void LoadContent(bool firstLoad)
		{

		}


		// Check the next section for more information about mod settings, save data and session.
		// Those are optional: if you don't need one of those, you can remove it from the module.

		// If you need to store settings:
		//public override Type SettingsType => typeof(EmoteModSettings);
		//public static EmoteModSettings Settings => (EmoteModSettings)Instance._Settings;

		// If you need to store save data:
		//public override Type SaveDataType => typeof(EmoteModSaveData);
		//public static EmoteModSaveData SaveData => (EmoteModSaveData)Instance._SaveData;

		// If you need to store session data:
		//public override Type SessionType => typeof(EmoteModSession);
		// public static EmoteModSession Session => (EmoteModSession)Instance._Session;

		// Set up any hooks, event handlers and your mod in general here.
		// Load runs before Celeste itself has initialized properly.
		#endregion

	}
}