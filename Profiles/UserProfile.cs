using AutoMapper;
using MyFirstMvcApp.Models;

namespace MyFirstMvcApp.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<User, UserDto>();
            CreateMap<UserDto, User>();
        }
    }
}