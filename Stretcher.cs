namespace Celeste.Mod.EmoteMod
{
    internal class Stretcher
	{
		public static float x_stretch = 1;
		public static float y_stretch = 1;

		public static bool stretch_lock = false;

		private static Player player;

		internal static void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		{
			 orig(self);

			 player = self;

			if (stretch_lock)
			{
				if (MadHuntNerf.inRound)
				{
					self.Sprite.Scale.X = x_stretch >= 0 ? 1 : -1;
					self.Sprite.Scale.Y = 1;
					self.Hair.Sprite.Scale.X = x_stretch >= 0 ? 1 : -1;
					self.Hair.Sprite.Scale.Y = 1;
				}
				else
				{
					self.Sprite.Scale.X = x_stretch;
					self.Sprite.Scale.Y = y_stretch;
					self.Hair.Sprite.Scale.X = x_stretch;
					self.Hair.Sprite.Scale.Y = y_stretch;
				}
			}
		}

		public static void stretch_x(float x)
		{
			player.Sprite.Scale.X = x;
			player.Hair.Sprite.Scale.X = x;
			x_stretch = x;
		}
		public static void stretch_y(float y)
		{
			player.Sprite.Scale.Y = y;
			player.Hair.Sprite.Scale.Y = y;
			y_stretch = y;
		}

		internal static void lock_stretch()
		{
			stretch_lock = !stretch_lock;
			x_stretch = 1;
			y_stretch = 1;
		}

        internal static void Load()
        {
			On.Celeste.Player.Update += Player_Update;
		}
		internal static void Unload()
		{
			On.Celeste.Player.Update += Player_Update;
		}
	}
}
