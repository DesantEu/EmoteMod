using System;

namespace Celeste.Mod.EmoteMod
{
    public class GravityModule
	{

        public static float playerY;

		public static void Load()
		{
			On.Celeste.Player.Update += Player_Update;
		}

        private static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
        {
            orig(self);
			if (EmoteModMain.Settings.CancelGravity && EmoteModMain.anim_by_game == 1)
				self.Y = playerY;
		}

        internal static void Unload()
        {
            On.Celeste.Player.Update -= Player_Update;
        }
    }

}
