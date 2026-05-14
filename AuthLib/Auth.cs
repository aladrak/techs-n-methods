using AppInterfaces;

namespace AuthLib;

public class Auth : IAuth
{
    public string UserPermissions { get; private set; }

    private List<User> _users = [];

    public void TryAuth(string enterName, string enterPassword)
    {
        try
        {
            using var file = new StreamReader("users.txt");

            while (!file.EndOfStream)
            {
                var line = file.ReadLine()!;
                var curUser = User.Parse(line);
                _users.Add(curUser);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
        }

        var matchFlag = _users.Any(el => el.Name == enterName && el.Password == enterPassword);

        if (matchFlag)
        {
            try
            {
                using var file = new StreamReader(enterName + ".txt");
                UserPermissions = enterName + ".txt";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
            }
        }
        else
        {
            Console.Error.WriteLine("No matching login and password");
        }
    }
}