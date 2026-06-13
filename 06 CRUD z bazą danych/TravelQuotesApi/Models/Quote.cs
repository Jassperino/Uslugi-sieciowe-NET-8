namespace TravelQuotesApi.Models
{
	public class Quote
	{
		public int Id { get; set; }
		public required string Author { get; set; }
		public required string Message { get; set; }
	}
}
