namespace Core.Database;

public interface IDataSeeder
{
    Task TrySeedAsync();
}