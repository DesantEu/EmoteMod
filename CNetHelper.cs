using System;
using System.Linq;
using Celeste.Mod.CelesteNet.Client.Components;
using Microsoft.Xna.Framework.Input;
using Monocle;
using MonoMod.RuntimeDetour;
using System;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using Microsoft.Xna.Framework;


namespace Celeste.Mod.EmoteMod
{
    internal class CNetHelper
	{
        internal static bool InteractionsAllowed = true;
        internal static bool isLoaded = false;
        internal static EverestModule CNModule = null;

        private static ILHook cnetGrabHook = null;

        internal static void Load()
        {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 1, 2) }))
			{
				isLoaded = true;
				CNModule = Everest.Modules.Where(module => module.GetType().FullName == "Celeste.Mod.CelesteNet.Client.CelesteNetClientModule").First();
                
                // TODO: hook interactions
                //cnetGrabHook = new ILHook(CNModule.GetType().GetProperty("Context").GetType().GetProperty("Main").GetType().GetMethod("Handle"), cnetGrab);
                cnetGrabHook = new ILHook(CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.Components.CelesteNetMainComponent")
                        .GetMethods().Where(m => m.Name == "Handle") // we get multiple handles
                        .Where(m => m.GetParameters().Where(p => p.Name == "grab").Count() > 0) // so we work with the one that has a "grab" param
                        .First(), cnetGrab);
			}
        }

        private static void cnetGrab(ILContext il){

        }

        internal static void Unload()
        {

        }
    }
}
