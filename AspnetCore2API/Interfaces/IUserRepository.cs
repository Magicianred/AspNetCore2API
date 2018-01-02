using FormPdfEasy.Models;
using System.Collections.Generic;

namespace FormPdfEasy.Interfaces
{
    public interface IUserRepository
    {
        IEnumerable<CustomUserDto> GetUsers();
    }
}
