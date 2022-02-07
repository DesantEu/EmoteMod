namespace Celeste.Mod.EmoteMod
{
    internal class EmoteStretcher
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
				//EmoteModMain.echo(self.Sprite.Scale.X.ToString() +" "+ self.Sprite.Scale.Y.ToString());

				self.Sprite.Scale.X = x_stretch;
				self.Sprite.Scale.Y = y_stretch;
				self.Hair.Sprite.Scale.X = x_stretch;
				self.Hair.Sprite.Scale.Y = y_stretch;

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
