namespace SafeBox.Interfaces
{
    public interface ICryptographer<T>
    {
        string Encrypt(string data, string key = null);
        T Decrypt(string data, string key = null);
    }
}
