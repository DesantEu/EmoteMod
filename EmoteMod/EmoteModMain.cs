using Monocle;
using System;

namespace Celeste.Mod.EmoteMod
{
    public class EmoteModMain : EverestModule
    {
        // speed multipliers
        public static float[] speeds = { 0.1f, 0.2f, 0.3f, 0.4f, 0.5f, 0.6f, 0.7f, 0.8f, 0.9f, 1, 2, 4, 5, 6, 10, 20, 30, 40, 50, 60, 70, 100 };
        // for debug
        public static bool debug;
        // emotemod settigns
        public override Type SettingsType => typeof(EmoteSettings);
        public static EmoteSettings Settings => (EmoteSettings)Instance._Settings;
        // celestenet settings
        public static CelesteNet.Client.CelesteNetClientSettings celestenetSettings = CelesteNet.Client.CelesteNetClientModule.Settings;
        //speed formatter
        public static Func<int, string> speedFormatter = arg =>
        {
            return $"{speeds[arg]}x";
        };
        //backpack formatter
        public static Func<int, string> backpackFormatter = arg =>
        {
            return arg == 0 ? "default" : arg == 1 ? "force on" : arg == 2 ? "force off" : arg == 3 ? "white" : "error";
        };
        public static Player player;

        /// <summary>
        /// If 0 we can make an animation;
        /// If 1 we are playing animation by hand;
        /// If 2 the game is playing an animation and we cant emote
        /// </summary>
        public static int anim_by_game;
        // for emote cancelling
        public static bool changedSprite;


        // Only one alive module instance can exist at any given time.
        public static EmoteModMain Instance;
        // for floating
        public static float playerY;
        // default values to return to
        public static bool interactDefault;
        public static bool invincibilityDefault;
        /// <summary>
        /// current animation delay
        /// </summary>
        public static float currentDelay;


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

        public override void Load()
        {
            debug = false; // debug by default
            anim_by_game = 0; // this tells that base state is no animations
            player = null; // anticrash
            EmoteCancelModule.player = null;
            changedSprite = false; // uh
            interactDefault = celestenetSettings.Interactions; // yea need to do that

            // overrides
            On.Celeste.Player.Update += Player_Update;
            On.Celeste.Level.Update += Level_Update;

            On.Celeste.Player.Update += EmoteModule.Player_Update;

            On.Celeste.LevelExit.ctor += EmoteCancelModule.LevelExit;
            On.Celeste.Player.Update += EmoteCancelModule.Player_Update;
            On.Celeste.Level.Update += EmoteCancelModule.Level_Update;
            On.Celeste.LevelExit.Begin += EmoteCancelModule.LevelExit_Begin;
            On.Celeste.Level.LoadLevel += EmoteCancelModule.LoadLevel;

            On.Celeste.Player.Update += BackpackModule.Player_ResetSprite;

            On.Celeste.Player.Update += SpeedModule.Player_Update;
            On.Celeste.Level.Update += SpeedModule.Level_Update;
        }

        // i didnt comment what is this for and now i dont know
        private void Level_Update(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            //if (anim_by_game != 1)
            //{
            //    interactDefault = celestenetSettings.Interactions;
            //    invincibilityDefault = SaveData.Instance.Assists.Invincible;
            //}
        }


        private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            player = self; // this is so that we can do things to player out of this method
            orig(self); //bruh

            #region ifs
            // log if stupid things happen
            if (debug)
                Logger.Log("EmoteModAnimId", $"int {celestenetSettings.Interactions}, anim {anim_by_game}");

            // disable gravity if gravity switch
            if (Settings.CancelGravity && anim_by_game == 1)
                player.Y = playerY;

            #endregion

            // turn left and right during emote
            if (anim_by_game == 1)
            {
                if (Input.MoveX == 1)
                    player.Facing = Facings.Right;
                if (Input.MoveX == -1)
                    player.Facing = Facings.Left;
            }

        }

        public override void LoadSettings()
        {
            base.LoadSettings();
        }



        public override void SaveSettings()
        {
            base.SaveSettings();
        }
        public override void Unload()
        {
            EmoteCancelModule.cancelEmote();

            On.Celeste.Player.Update -= Player_Update;
            On.Celeste.Level.Update -= Level_Update;

            On.Celeste.Player.Update -= EmoteModule.Player_Update;

            On.Celeste.Player.Update -= EmoteCancelModule.Player_Update;
            On.Celeste.Level.Update -= EmoteCancelModule.Level_Update;
            On.Celeste.LevelExit.Begin -= EmoteCancelModule.LevelExit_Begin;
            On.Celeste.Level.LoadLevel -= EmoteCancelModule.LoadLevel;

            On.Celeste.Player.Update -= BackpackModule.Player_ResetSprite;

            On.Celeste.Player.Update -= SpeedModule.Player_Update;
            On.Celeste.Level.Update -= SpeedModule.Level_Update;

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