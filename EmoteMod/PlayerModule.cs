using Monocle;

namespace Celeste.Mod.EmoteMod
{
    internal class PlayerModule
	{

		//public static Player player;

		public void Load()
		{
			//On.Celeste.Player.Update += Player_Update;
		}

		//private void Player_Update(On.Celeste.Player.orig_Update orig, Player self)
		//{
		//	player = self;
		//}

		public static Player GetPlayer()
        {
			return Engine.Scene?.Tracker?.GetEntity<Player>();
		}

		public void Unload()
		{
			//On.Celeste.Player.Update -= Player_Update;
		}
	}
}
