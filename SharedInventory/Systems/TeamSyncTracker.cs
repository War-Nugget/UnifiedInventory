using System.Collections.Generic;
using Terraria;
using Terraria.ID;

namespace UnifiedInventory.SharedInventory.Systems
{
    /// <summary>
    /// Keeps track of which player is acting as the host for each team.
    /// </summary>
    public static class TeamSyncTracker
    {
        private static Dictionary<int, int> teamHosts = new(); // teamID → whoAmI


        public static void RegisterTeamHost(int team, int playerId)
        {
            if (Main.netMode == NetmodeID.Server && !teamHosts.ContainsKey(team))
            {
                teamHosts[team] = playerId;
               // Main.NewText($"[Server] Registered Player {playerId} as host for Team {team}");
            }
        }
        public static int? GetTeamHost(int team)
            => teamHosts.TryGetValue(team, out var id) ? id : (int?)null;

        public static bool IsTeamHost(int team, int playerId)
            => GetTeamHost(team) == playerId;

        public static void Reset() 
            => teamHosts.Clear();
    }
}
