using AutoMapper;
using VendorAPI.DTOs;
using VendorAPI.Models;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Vendor, VendorReadDto>();
        CreateMap<Vendor, VendorReadDto.WithChildren>()
            .ForMember(dest => dest.BankAccounts, opt => opt.MapFrom(src => src.BankAccounts))
            .ForMember(dest => dest.ContactPersons, opt => opt.MapFrom(src => src.ContactPersons));
        CreateMap<VendorCreateDto, Vendor>();
        CreateMap<VendorUpdateDto, Vendor>();

        CreateMap<ContactPerson, ContactPersonReadDto>();
        CreateMap<ContactPerson, ContactPersonCreateDto>();
        CreateMap<ContactPerson, ContactPersonReadDto.CPWithVendor>();
        CreateMap<ContactPersonCreateDto, ContactPerson>();
        CreateMap<ContactPersonUpdateDto, ContactPerson>();

        CreateMap<BankAccount, BankAccountReadDto>();
        CreateMap<BankAccount, BankAccountCreateDto>();
        CreateMap<BankAccount, BankAccountReadDto.BAWithVendor>();
        CreateMap<BankAccountCreateDto, BankAccount>();
        CreateMap<BankAccountUpdateDto, BankAccount>();

        CreateMap<User, UserBasicDto>();
        CreateMap<User, UserLoginDto>();
        CreateMap<User, UserCreateDto>();
        CreateMap<User, UserUpdateDto>();
        CreateMap<UserCreateDto, User>();

        CreateMap<AccessRefreshToken, AccessTokenDto>();
    }
}