namespace MenuLib;

public class Auth
{
    const string usersFilename = "users.txt";
    public string userPermissions { get; private set; } = null;

    public List<User> Users { get; private set; } = new();

    public Auth(string enterName, string enterPassword)
    {
        try
        {
            using var file = new StreamReader(usersFilename);

            while (!file.EndOfStream)
            {
                string line = file.ReadLine()!;
                User cur_user = User.Parse(line);
                Users.Add(cur_user);
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
        }

        bool match_flag = false;
        foreach (User el in Users) {
            if (el.Name == enterName && el.Password == enterPassword) {
                match_flag = true;
                break;
            }
        }

        if (match_flag)
        {
            try
            {
                string name = enterName;
                using var file = new StreamReader(name + ".txt");
                userPermissions = name + ".txt";
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}:{ex.StackTrace}");
            }
        }
        else
        {
            Console.Error.WriteLine($"No matching login and password");
        }
    }
}
