using AutoMapper;
using URLShortening.Data;
using URLShortening.DTOs;

namespace URLShortening.AutoMapper;

public class UrlMappingProfile : Profile
{
    public UrlMappingProfile()
    {
        CreateMap<Url, urlDto>()
            .ForMember(dest => dest.Url,
                opt => opt.MapFrom(src => src.LongUrl))
            .ForMember(dest => dest.ShortCode,
                opt => opt.MapFrom(src => src.ShortId));
    }
}
