namespace AppInterfaces;

public interface IAuth
{
    public string UserPermissions { get; }
    public void TryAuth(string enterName, string enterPassword);
}