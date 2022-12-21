using AutoMapper;
using DataLayer;
using HermesChatApp.Models;

namespace HermesChatApp.MapperProfiles
{
    public class AppProfile : Profile
    {
        public AppProfile()
        {
            CreateMap<User, UserModel>()
                .ReverseMap();

            CreateMap<Group, GroupModel>()
                .ReverseMap();
        }
    }
}
