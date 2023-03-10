using Monocle;

namespace Celeste.Mod.EmoteMod
{
    internal class PlayerHelper
	{

        internal bool ForceInvincibility = false;

		public void Load()
		{
            // TODO: hook death or something
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
