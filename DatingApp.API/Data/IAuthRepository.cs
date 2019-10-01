using System.Threading.Tasks;
using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public interface IAuthRepository
    {
        //Register User
        Task<User> Register(User user, string password);
        //Login User
        Task<User> Login(string username, string password);
        //Check if User exist
        Task<bool> DoesUserExist(string username);
    }
}