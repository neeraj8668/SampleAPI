namespace Sample.Shared.Models
{
     
    public class DomainEvent<T>
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string EventName { get; set; }

        public T  Data { get; set; }
         
    }

}
