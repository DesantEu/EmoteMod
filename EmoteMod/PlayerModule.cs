using Monocle;

namespace Celeste.Mod.EmoteMod
{
    internal class PlayerModule
	{

		public void Load()
		{
		}

		public static Player GetPlayer()
        {
			return Engine.Scene?.Tracker?.GetEntity<Player>();
		}

		public void Unload()
		{
		}
	}
}
