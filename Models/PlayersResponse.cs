using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LeagueLobby.Models
{
    #nullable disable
    public sealed class PlayersResponse
    {
        [JsonPropertyName("participants")]
        public List<Player> Participants { get; set; }
    }

    #nullable disable
    public sealed class Player
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("game_tag")]
        public string GameTag { get; set; }
    }
}
