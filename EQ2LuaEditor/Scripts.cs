using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EQ2LuaEditor
{
    class Scripts
    {
        private static string GetHeader()
        {
            // Standard header at the top of every script
            string ret = "--[[\n\tScript Name\t\t:\t<script-name>\n\tScript Purpose\t:\t<purpose>\n\tScript Author\t:\t" + ((EQ2LuaEditor.Settings.AuthorName == "NULL" || EQ2LuaEditor.Settings.AuthorName == string.Empty) ? "<author-name>" : EQ2LuaEditor.Settings.AuthorName) + "\n\tScript Date\t\t:\t" + DateTime.UtcNow.Date.ToString("d") + "\n\tScript Notes\t:\t<special-instructions>\n--]]\n\n";
            return ret;
        }

        private static string GetQuestHeader()
        {
            string ret;
            // Start with the original header
            ret = GetHeader();
            // Strip off the "--[[\n\n" at the end
            ret = ret.Substring(0, ret.Length - 6);
            // Add the extra info that the quest header have
            ret += "\n\tZone\t\t\t:\t<zone-name>\n\tQuest Giver\t\t:\t<quest-giver-name>\n\tPreceded by\t\t:\t<preceded-quest-name(lua file)>\n\tFollowed by\t\t:\t<followed-by-quest-name(lua file)>\n--]]\n\n";
            return ret;
        }

        public static string GetSpawnScript()
        {
            string ret = GetHeader();
            ret += "function spawn(NPC)\nend\n\n"
                + "function respawn(NPC)\n\tspawn(NPC)\nend\n\n"
                + "function hailed(NPC, Spawn)\nend\n\n"
                + "function hailed_busy(NPC, Spawn)\nend\n\n"
                + "function casted_on(NPC, Spawn, Message)\nend\n\n"
                + "function targeted(NPC, Spawn)\nend\n\n"
                + "function attacked(NPC, Spawn)\nend\n\n"
                + "function aggro(NPC, Spawn)\nend\n\n"
                + "function healthchanged(NPC, Spawn)\nend\n\n"
                + "function auto_attack_tick(NPC, Spawn)\nend\n\n"
                + "function death(NPC, Spawn)\nend\n\n"
                + "function killed(NPC, Spawn)\nend\n\n"
                + "function CombatReset(NPC)\nend\n\n"
                + "function randomchat(NPC, Message)\nend";
            return ret;
        }

        public static string GetZoneScript()
        {
            string ret = GetHeader();
            ret += "function init_zone_script(zone)\nend\n\n"
                + "function player_entry(zone, player)\nend\n\n"
                + "function enter_location(zone, spawn, grid)\nend\n\n"
                + "function leave_location(zone, spawn, grid)\nend\n\n"
                + "function dawn(zone)\nend\n\n"
                + "function dusk(zone)\nend";

            return ret;
        }

        public static string GetItemScript()
        {
            string ret = GetHeader();
            ret += "function obtained(Item, Player)\nend\n\n"
                + "function removed(Item, Player)\nend\n\n"
                + "function destroyed(Item, Player)\nend\n\n"
                + "function examined(Item, Player)\nend\n\n"
                + "function used(Item, Player)\nend\n\n"
                + "function cast(Item, Player)\nend\n\n"
                + "function equipped(Item, Player)\nend\n\n"
                + "function unequipped(Item, Player)\nend\n\n"
                + "function proc(Item, Caster, Target, Type)\nend";

            return ret;
        }

        public static string GetSpellScript()
        {
            string ret = GetHeader();
            ret += "function cast(Caster, Target) -- Add more params as needed for the values from the db\nend\n\n"
                + "function tick(Caster, Target) -- Add more params as needed for the values from the db\nend\n\n"
                + "function proc(Caster, Target)\nend\n\n"
                + "function remove(Caster, Target)\nend";

            return ret;
        }

        public static string GetQuestScript()
        {
            string ret = GetQuestHeader();
            ret += "function Init(Quest)\nend\n\n"
                + "function Accepted(Quest, QuestGiver, Player)\nend\n\n"
                + "function Deleted(Quest, QuestGiver, Player)\nend\n\n"
                + "function Declined(Quest, QuestGiver, Player)\nend\n\n"
                + "function Reload(Quest, QuestGiver, Player, Step)\nend";

            return ret;
        }
    }
}
