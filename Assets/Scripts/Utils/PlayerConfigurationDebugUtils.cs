using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace Utils
{
    public class PlayerConfigurationDebugUtils
    {
        public static void PrintPlayerConfigurations(List<PlayerConfiguration> playerConfigurations)
        {
            string result = String.Empty;
            foreach (var playerConfiguration in playerConfigurations)
            {
                result += FormatPlayerConfiguration(playerConfiguration);
            }
            
            Debug.Log(result);
        }
        
        public static string FormatPlayerConfiguration(PlayerConfiguration playerConfiguration)
        {
            string result = "PlayerConfiguration (player index = " + playerConfiguration.Input.playerIndex + "): {\n";
            result += "  Input: {\n";
            result += "    playerIndex: " + playerConfiguration.Input.playerIndex + "\n";
            result += "  }\n";
            result += "  Player: {\n";
            result += "    playerIndex: " + playerConfiguration.PlayerIndex + "\n";
            result += "    characterIndex: " + playerConfiguration.CharacterIndex + "\n";
            result += "  }\n";
            result += "}\n";

            return result;
        }
    }
}