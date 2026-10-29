using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Services.Interfaces;

public interface IUsersService
{
    public List<User> RecupererUsers();
    public User? RecupererUserParLogin(string login);

}