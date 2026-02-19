namespace MomsAppApi.Models.StructuresDTO
{
    public class CreateStructureDTO
    {
        public string name { get; set; }
        public string address_line { get; set; }
        public string city {  get; set; } 
        public string zip {  get; set; }
        public string client_name { get; set; } = string.Empty;

        public CreateStructureDTO(
        string name,
        string address_line,
        string city,
        string zip,
        string client_name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.");

            if (string.IsNullOrWhiteSpace(address_line))
                throw new ArgumentException("Address cannot be empty.");

            if (string.IsNullOrWhiteSpace(city))
                throw new ArgumentException("City cannot be empty.");

            if (string.IsNullOrWhiteSpace(zip))
                throw new ArgumentException("Zip cannot be empty.");

            

            this.name = name;
            this.address_line = address_line;
            this.city = city;
            this.zip = zip;
            this.client_name = client_name;
        }
    }
}
