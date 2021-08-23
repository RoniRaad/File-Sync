namespace FileSync.DomainModel.Models
{
    public class AzureAdConfig
    {
        public string AADInstance { get; set; }
        public string Tenant { get; set; }
        public string ClientId { get; set; }
        public string FileSyncScope { get; set; }
        public string FileSyncBaseAddress { get; set; }
    }
}
