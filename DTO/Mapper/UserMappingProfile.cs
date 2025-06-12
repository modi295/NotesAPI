using AutoMapper;
using NotesAPI.Models;

namespace NotesAPI.DTO.Mapper
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // Mapping from User -> UserResponseDTO (get API responses)
            CreateMap<User, UserResponseDTO>()
                .ForMember(dest => dest.ProfilePicture, opt => opt.MapFrom(src =>
                    src.ProfilePicture != null
                        ? new { type = "Buffer", data = src.ProfilePicture.Select(b => (int)b).ToArray() }
                        : null
                ));

            // Mapping from UpdateUserDto -> User (for updating users)
            CreateMap<UpdateUserDto, User>()
                .ForAllMembers(opts => 
                    opts.Condition((src, dest, srcMember) => srcMember != null));
        }
    }
}
