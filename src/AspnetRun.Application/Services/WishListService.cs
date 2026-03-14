using AspnetRun.Application.Interfaces;
using AspnetRun.Application.Models;
using AspnetRun.Core.Entities;
using AspnetRun.Core.Repositories;
using AspnetRun.Core.Specifications;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AspnetRun.Application.Services
{
    public class WishListService(
        IWishlistRepository wishlistRepository,
        IProductRepository productRepository,
        IMapper mapper)
        : IWishlistService
    {
        private readonly IWishlistRepository _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        public async Task<WishlistModel> GetWishlistByUserName(string userName)
        {
            var wishlist = await GetExistingOrCreateNewWishlist(userName);
            var wishlistModel = mapper.Map<WishlistModel>(wishlist);

            foreach (var item in wishlist.ProductWishlists)
            {
                var product = await _productRepository.GetProductByIdWithCategoryAsync(item.ProductId);
                var productModel = mapper.Map<ProductModel>(product);
                wishlistModel.Items.Add(productModel);
            }

            return wishlistModel;
        }

        public async Task AddItem(string userName, int productId)
        {
            var wishlist = await GetExistingOrCreateNewWishlist(userName);
            wishlist.AddItem(productId);
            await _wishlistRepository.UpdateAsync(wishlist);
        }

        public async Task RemoveItem(int wishlistId, int productId)
        {
            var spec = new WishlistWithItemsSpecification(wishlistId);
            var wishlist = (await _wishlistRepository.GetAsync(spec)).FirstOrDefault();

            wishlist?.RemoveItem(productId);
            await _wishlistRepository.UpdateAsync(wishlist);
        }

        private async Task<Wishlist> GetExistingOrCreateNewWishlist(string userName)
        {
            var wishlist = await _wishlistRepository.GetByUserNameAsync(userName);
            if (wishlist != null)
                return wishlist;

            // if it is first attempt create new
            var newWishlist = new Wishlist
            {
                UserName = userName
            };

            await _wishlistRepository.AddAsync(newWishlist);
            return newWishlist;
        }
    }
}
