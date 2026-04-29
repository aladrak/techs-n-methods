namespace MenuLib;

public class User(string name, string password)
{
    public string Name { get; } = name;
    public string Password { get; } = password;

    public static User Parse(string value)
    {
        var data = value.Split(' ');
        var name = data[0];
        var password = data[1];

        return new User(name, password);
    }

}
