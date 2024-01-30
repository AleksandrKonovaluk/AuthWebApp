using AuthWebApp.Models;

namespace AuthWebApp.Infastructure
{
	public class AllowedUsersList
	{
		public List<Person> allowedPersons = new List<Person>
		{
			new("testUser1", "test@mail.com"),
			new("testUser2", "test2@mail.com"),
			new("tom@gmail.com", "12345"),
			new("bob@gmail.com", "55555")
		};
	}
}
