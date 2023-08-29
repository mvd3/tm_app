public class Status 
{
    public int Id { get; init; }
    public string Name { get; init; }

    public Status(int id, string name)
    {
        Id = id;
        Name = name;
    }
}