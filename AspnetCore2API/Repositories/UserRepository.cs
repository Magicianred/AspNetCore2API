using AutoMapper;
using FormPdfEasy.Entities;
using FormPdfEasy.Interfaces;
using FormPdfEasy.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FormPdfEasy.Repositories
{
    public class UserRepository : IUserRepository
    {
        private FormPdfEasyContext _context;

        public UserRepository(FormPdfEasyContext context)
        {
            _context = context;
        }

        public IEnumerable<CustomUserDto> GetUsers()
        {
            var userEntities = _context.CustomUsers.ToList();

            var users = Mapper.Map<IEnumerable<CustomUserDto>>(userEntities);

            return users;
        }
    }
}
