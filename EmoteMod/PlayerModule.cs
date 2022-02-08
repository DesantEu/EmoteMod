namespace Celeste.Mod.EmoteMod
{
    internal class PlayerModule
	{

		public static Player player;

		public void Load()
		{
			On.Celeste.Player.Update += Player_Update;
		}

		private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		{
			player = self;
		}

		public void Unload()
		{
			On.Celeste.Player.Update -= Player_Update;
		}
	}
}
