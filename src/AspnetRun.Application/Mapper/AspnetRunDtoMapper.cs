using AspnetRun.Application.Models;
using AspnetRun.Core.Entities;
using AutoMapper;

namespace AspnetRun.Application.Mapper
{
    public class AspnetRunDtoMapper : Profile
    {
        public AspnetRunDtoMapper()
        {
            CreateMap<Product, ProductModel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)).ReverseMap();

            CreateMap<Category, CategoryModel>().ReverseMap();
            CreateMap<Wishlist, WishlistModel>().ReverseMap();
            CreateMap<Compare, CompareModel>().ReverseMap();
            CreateMap<Order, OrderModel>().ReverseMap();
            CreateMap<Cart, CartModel>().ReverseMap();
            CreateMap<CartItem, CartItemModel>().ReverseMap();
        }
    }
}
