using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfinityScript;

namespace AntiWeaponCheat
{
    public class AntiWeaponCheat : BaseScript
    {
        public static string TeamNameAllies = "^1TEAM A";
        public static string TeamNameAxis = "^2TEAM B";

        public AntiWeaponCheat() : base()
        {

           AntiWeponCheat_F();
           handlePlayers();

            PlayerConnected += new Action<Entity>(entity =>
            {
                               if (CustomTeamNames)
                {
                    setDvar("g_TeamName_Allies", TeamNameAllies);
                    setDvar("g_TeamName_Axis", TeamNameAxis);
                }

            });
        }

        
        private void AntiWeponCheat_F() 
        {
            Log.Write(LogLevel.All, "AntiHardScope Started!");
            PlayerConnecting += new Action<Entity>(entity =>
            {
                int adsTime = 0;
                entity.OnInterval(100, player =>
                {
                    if (!player.IsAlive)
                    {
                        return true;
                    }
                    if (!player.CurrentWeapon.Equals(MainWeapon))
                    {
                        return true;
                    }

                    if (player.Call<float>("playerads") >= 1) {
                        adsTime++;
                    } else {
                        adsTime = 0;
                    }

		            if( adsTime >= maxtime*10)
		            {
			            adsTime = 0;
                        player.Call("allowads", false);
                        OnInterval(50, () => {
                            if (player.Call<int>("adsbuttonpressed") > 0)
                            {
                                return true;
                            }
                            player.Call("allowads", true);
                            return false;
                        });
		            }
                    return true;
                });
            });
        }

        
        private List<Entity> allPlayers;
        private bool _prematchDone;

        private void handlePlayers()
        {
            allPlayers = new List<Entity>();
            OnNotify("prematch_done", () =>
            {
                _prematchDone = true;
                Log.Write(LogLevel.All, "Searching for player entities");
                for (int i = 0; i < 2048; i++)
                {
                    Entity entity = Call<Entity>("getentbynum", i);
                    if (entity != null)
                    {
                        if (entity.IsPlayer)
                        {
                            preSpawnPlayer(entity, false);
                        }
                    }
                }
            });
            PlayerConnected += new Action<Entity>(entity =>
            {
                //if (_prematchDone)
                //{
                    preSpawnPlayer(entity, true);
                //}
            });

            PlayerDisconnected += new Action<Entity>(entity =>
            {
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (allPlayers.ElementAt(i).GetField<string>("name").Equals(entity.GetField<string>("name")))
                    {
                        allPlayers.Remove(allPlayers.ElementAt(i));
                    }
                }
            });
        }

        private void preSpawnPlayer(Entity entity, bool firsttime)
        {
            Entity found = allPlayers.Find(delegate(Entity ent)
            {
                return ent.GetField<string>("name").Equals(entity.GetField<string>("name"));
            });
            if (found == null)
            {
                allPlayers.Add(entity);
                Log.Write(LogLevel.All, "Spawned player " + entity.GetField<string>("name"));
            }
            else
            {
                Log.Write(LogLevel.All, "Attempted to add duplicate player entity to list... ignored");
            }
        }
        

    }
}
