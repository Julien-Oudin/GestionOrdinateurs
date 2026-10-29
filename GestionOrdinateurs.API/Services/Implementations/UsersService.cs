using GestionOrdinateurs.API.Services.Interfaces;
using GestionOrdinateurs.Shared.classes;

namespace GestionOrdinateurs.API.Services.Implementations;

public class UsersService : IUsersService
{
    private readonly ParcInformatiqueContext _context;

    public UsersService(ParcInformatiqueContext context)
    {
        _context = context;
    }

    public List<User> RecupererUsers()
    {
        return _context.Users.ToList();
    }

    public User? RecupererUserParLogin(string login)
    {
        return _context.Users.FirstOrDefault(user => user.Login == login);
    }
}
