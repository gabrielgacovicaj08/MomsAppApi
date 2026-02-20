namespace MomsAppApi.Models.StructuresDTO
{
    public class StructureResponseDTO
    {
        public int structure_id { get; set; }
        public string name { get; set; } = string.Empty;
        public string address_line { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string zip { get; set; } = string.Empty;
        public string client_name { get; set; } = string.Empty;
        public bool is_active { get; set; }
    }
}
