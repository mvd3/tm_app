public class Task 
{
   public int Id { get; init; }
   public string Name {get; set; }
   public string Description { get; set; }

   public DateTime CreatedDate { get; init; }

   public int StatusId { get; set; }

   public Task(int id, string name, string description, DateTime createdDate, int statusId)
   {
    Id = id;
    Name = name;
    Description = description;
    CreatedDate = createdDate;
    StatusId = statusId;
   }
}