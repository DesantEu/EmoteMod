using System;
using System.Linq;
using MonoMod.RuntimeDetour;
using MonoMod.Cil;



namespace Celeste.Mod.EmoteMod
{
    internal class CNetHelper
    {
        internal static bool InteractionsAllowed
        {
            get { return _InteractionsAllowed; }
            set
            {
                _InteractionsAllowed = value;

                if (isLoaded && isConnected)
                {
                    try
                    {
                        CNMainComponent?.GetType().GetField("StateUpdated")?.SetValue(CNMainComponent, true);
                        EmoteModModule.echo($"WHAT?!?!! ITS WORKIN!!!!!!!!!G!!!!!!!!!!!!!");
                        CNRelease();
                    }
                    catch (Exception e)
                    {
                        EmoteModModule.echo($"we got exception for some reason {e}");
                    }
                }

            }
        }
        private static bool _InteractionsAllowed = true;
        internal static bool isLoaded = false;
        internal static bool isConnected => !CNContext?.Equals(null) ?? false;
        internal static EverestModule CNModule = null;

        // TODO: remove
        private static ILHook cnetGrabHook = null;
        private static ILHook cnetInteractionsGetHook = null;

        private static ILHook OnSendDataHook = null;

        internal static object CNInstance => CNModule.GetType().GetField("Instance").GetValue(null);
        internal static object CNContext => CNInstance.GetType().GetField("Context").GetValue(CNInstance);
        internal static object CNMainComponent => CNContext?.GetType().GetField("Main").GetValue(CNContext);
        internal static bool CNIsGrabbed => (bool)CNMainComponent?.GetType().GetField("IsGrabbed")?.GetValue(CNMainComponent);

        internal static string test = "";

        internal static void Load()
        {
            if (Everest.Loader.DependencyLoaded(new EverestModuleMetadata { Name = "CelesteNet.Client", Version = new Version(2, 2, 2) }))
            {
                isLoaded = true;
                CNModule = Everest.Modules.Where(module => module.GetType().FullName == "Celeste.Mod.CelesteNet.Client.CelesteNetClientModule").First();

                // CelesteNet.Client.CelesteNetClientModule.Instance.Context.Main.Tick




                // TODO: hook interactions
                //cnetGrabHook = new ILHook(CNModule.GetType().GetProperty("Context").GetType().GetProperty("Main").GetType().GetMethod("Handle"), cnetGrab);
                //
                //
                //
                //
                // cnetGrabHook = new ILHook(CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.Components.CelesteNetMainComponent")
                //         .GetMethods().Where(m => m.Name == "Handle") // we get multiple handles
                //         .Where(m => m.GetParameters().Where(p => p.Name == "grab").Count() > 0) // so we work with the one that has a "grab" param
                //         .First(), cnetGrab);

                // object mainComponent = context.GetType().GetProperty("Main").GetValue(context);

                // CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.Components.CelesteNetMainComponent")
                //         .GetMethods().Where(m => m.Name == "SendReleaseMe").First().Invoke(mainComponent, null);
                //

                //
                // cnetInteractionsGetHook = new ILHook(CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.CelesteNetClientSettings+InGameMenu")
                //         .GetProperty("Interactions", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), cnetInteractionsGet);
                // cnetInteractionsGetHook = new ILHook(CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.CelesteNetClientSettings+InGameMenu")
                //         .GetProperty("Interactions", BindingFlags.Instance | BindingFlags.Public).GetGetMethod(), cnetInteractionsGet);
                // CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.CelesteNetClientSettings.InGameMenu").GetProperties();
                //




                // TODO: uncomment this we need it
                // OnSendDataHook = new(CNModule.GetType().Assembly.GetType("Celeste.Mod.CelesteNet.Client.Components.CelesteNetMainComponent").GetMethod("SendState"), OnSendData);



            }
        }

        internal static void CNRelease()
        {
            if (CNIsGrabbed)
            {
                CNMainComponent?.GetType().GetMethod("SendReleaseMe")?.Invoke(CNMainComponent, null);
                CNMainComponent?.GetType().GetField("GrabbedBy")?.SetValue(CNMainComponent, null);
                CNMainComponent?.GetType().GetField("IsGrabbed")?.SetValue(CNMainComponent, false);
            }
            EmoteModModule.echo($"didnt crash");

        }

        private static void OnSendData(ILContext il)
        {
            ILCursor c = new(il);

            // int orig = c.Index;
            //
            // while (true)
            // {
            //     try
            //     {
            //         test += c.ToString().Split('\n')[2] + "\n";
            //         // test += c.Next.ToString() + " ::: " + "\n";
            //         c.Index++;
            //     }
            //     catch
            //     {
            //         test += $"caught exception at index {c.Index}";
            //         break;
            //     }
            // }
            // c.Index = orig;
            //
            // while (c.TryGotoNext(MoveType.After, instr => instr.Match(OpCodes.Stfld)))
            // {
            //     test += $"found stfld at {c.Index}";
            // }
            //
            // c.Index = orig;

            // foreach (EverestModule m in Everest.Modules.Where(m => m.GetType().FullName.Contains("CelesteNet")))
            // {
            //
            //     test += m.GetType().FullName;
            // }
            //


            // foreach (Type t in CNModule.GetType().BaseType.Assembly.GetTypes())
            // {
            //     test += $"{t.FullName}\n";
            // }
            // test += $"app domain: {AppDomain.CurrentDomain.FriendlyName}";
            // test += "ass emblies:\n";
            // // foreach (AssemblyName a in CNModule.GetType().Assembly.GetReferencedAssemblies())
            // foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("CelesteNet")))
            // {
            //     // test += (a.FullName) + ":::" + a.GetType().FullName + '\n';
            //     test += a.FullName;
            // }
            //.Where(t => t.Name.Contains("DataPlayerState")).First().FullName;

            // try
            // {
            //     foreach (TypeInfo t in AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.Contains("CelesteNet.Shared")).First().GetTypes())
            //     {
            //         test += $"{t.FullName}\n";
            //     }
            // }
            // catch (Exception e)
            // {
            //     test += $"got exception '{e}'";
            // }

            Type DataPlayerState = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.Contains("CelesteNet.Shared")).First()
                .GetTypes().Where(t => t.FullName.Contains("DataPlayerState")).First();

            if (c.TryGotoNext(MoveType.Before, instr => instr.MatchStfld(DataPlayerState, "Interactive")))
            {
                // test += $"found the bitch: {c.Next.ToString()}";
                c.EmitDelegate<Func<bool, bool>>((orig) =>
                {
                    if (InteractionsAllowed)
                    {
                        EmoteModModule.echo($"returning orig: {orig}");
                        return orig;
                    }
                    else
                    {
                        EmoteModModule.echo($"returning false");
                        return false;
                    }
                });
            }

        }

        // private static void cnetGrab(ILContext il)
        // {
        //     ILCursor c = new(il);
        //     c.EmitDelegate<Action>(() =>
        //     {
        //         // EmoteModModule.echo("hook is working");
        //         if (EmoteModModule.anim_by_game == 1)
        //         {
        //             CelesteNet.Client.CelesteNetClientModule.Instance.Context.Main.SendReleaseMe();
        //             // EmoteModModule.echo(context.ToString());
        //             // EmoteModModule.echo(mainComponent.ToString());
        //
        //         }
        //     });
        //     c.Emit(OpCodes.Brtrue, 123);
        // }

        // private static void cnetInteractionsGet(bool orig)
        // private static void cnetInteractionsGet(ILContext il)
        // {
        //     // Logger.Log("EmoteMod/Hooks", "trying to patch celestenet interactions...");
        //     // orig();
        //     ILCursor c = new(il);
        //     // load either the orig or false
        //     if (c.TryGotoNext(MoveType.Before, instr => instr.Match(OpCodes.Ret)))
        //     {
        //         // Logger.Log("EmoteMod/Hooks", "trying to patch celestenet interactions... found ret");
        //         c.EmitDelegate<Func<bool, bool>>((orig) =>
        //         {
        //             if (EmoteModModule.anim_by_game == 1)
        //             {
        //
        //                 EmoteModModule.echo("tryna read interactions, returning false");
        //                 return false;
        //             }
        //             EmoteModModule.echo("tryna read interactions, returning orig");
        //             return orig;
        //         });
        //         // Logger.Log("sex", $"the entire john wich movie: {il.Method.Body.ToString()}");
        //     }
        // Logger.Log("sex", $"the entire john wich movie: {il.Method.Body.ToString()}");


        // c.Emit(OpCodes.Ret);
        // ILCursor c = new(il);
        // c.EmitDelegate<Action>(() =>
        // {
        //     EmoteModModule.echo("hook is working");
        //
        //     //     if (EmoteModModule.anim_by_game == 1)
        //     //     {
        //     //         CelesteNet.Client.CelesteNetClientModule.Instance.Context.Main.SendReleaseMe();
        //     //         // EmoteModModule.echo(context.ToString());
        //     //         // EmoteModModule.echo(mainComponent.ToString());
        //     //
        //     //     }
        //     // });
        //
        // });
        // }


        internal static void Unload()
        {

        }
    }
}
