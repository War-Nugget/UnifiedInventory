using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.IO;
using System.IO;
using UnifiedInventory.SharedInventory.Utils;
using System.Collections.Generic;

using UnifiedInventory.SharedInventory.Database;


namespace UnifiedInventory.SharedInventory.Systems;
    public static class TeamSyncTracker
    {
        private static Dictionary<int, int> teamHosts = new(); // team => player.whoAmI

        public static void RegisterTeamHost(int team, int playerId)
        {
            if (!teamHosts.ContainsKey(team))
            {
                teamHosts[team] = playerId;
            }
        }

        public static int? GetTeamHost(int team)
        {
            return teamHosts.TryGetValue(team, out int playerId) ? playerId : (int?)null;
        }

        public static bool IsTeamHost(int team, int playerId)
        {
            return GetTeamHost(team) == playerId;
        }

        public static void Reset()
        {
            teamHosts.Clear();
        }
    }
