namespace App.Models
{
    public class Planet
    {
        public Planet(int id, string name, string info)
        {
            Id=id;
            Name = name;
            Info = info;
        }
        public int Id { get; set; }
        public string Name {get; set; }
        public string Info {get; set;}
    }
}