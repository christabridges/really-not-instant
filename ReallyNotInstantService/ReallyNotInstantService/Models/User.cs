public class User {
    public readonly Guid Id = Guid.NewGuid();
    public string Forename; 
    public string Surname;
    public Location PostboxLocation;

    public User(string forename, string surname, Location postboxLocation) {
        Forename = forename;
        Surname = surname;
        PostboxLocation = postboxLocation;
    }
}
