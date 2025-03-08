using System.Text.Json.Serialization;

namespace FikaServer.Models.Fika.Headless
{
    public record HeadlessAvailableClients
    {
        /// <summary>
        /// SessionID of the headless client
        /// </summary>
        [JsonPropertyName("headlessSessionID")]
        public string HeadlessSessionID { get; set; } = string.Empty;
        /// <summary>
        /// The alias of the headless client, if it has any assigned. If it doesn't have any assigned uses the nickname 
        /// </summary>
        [JsonPropertyName("alias")]
        public string Alias { get; set; } = string.Empty;
    }
}
