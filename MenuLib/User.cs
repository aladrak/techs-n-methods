namespace MenuLib;

public class User
{
    public string Name { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public Dictionary<string, int> Items { get; init; } = new();

    public User(string name, string password)
    {
        Name = name;
        Password = password;
    }

    public static User Parse(string value)
    {
        var data = value.Split(' ');
        string name = data[0];
        string password = data[1];

        return new User(name, password);
    }

}
