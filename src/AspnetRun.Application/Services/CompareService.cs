using AspnetRun.Application.Interfaces;
using AspnetRun.Application.Models;
using AspnetRun.Core.Entities;
using AspnetRun.Core.Repositories;
using AspnetRun.Core.Specifications;
using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;

namespace AspnetRun.Application.Services
{
    public class CompareService(
        ICompareRepository compareRepository,
        IProductRepository productRepository,
        IMapper mapper)
        : ICompareService
    {
        private readonly ICompareRepository _compareRepository = compareRepository ?? throw new ArgumentNullException(nameof(compareRepository));
        private readonly IProductRepository _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));

        public async Task<CompareModel> GetCompareByUserName(string userName)
        {
            var compare = await GetExistingOrCreateNewCompare(userName);
            var compareModel = mapper.Map<CompareModel>(compare);

            foreach (var item in compare.ProductCompares)
            {
                var product = await _productRepository.GetProductByIdWithCategoryAsync(item.ProductId);
                var productModel = mapper.Map<ProductModel>(product);
                compareModel.Items.Add(productModel);
            }
            return compareModel;
        }

        public async Task AddItem(string userName, int productId)
        {
            var compare = await GetExistingOrCreateNewCompare(userName);
            compare.AddItem(productId);
            await _compareRepository.UpdateAsync(compare);
        }

        public async Task RemoveItem(int compareId, int productId)
        {
            var spec = new CompareWithItemsSpecification(compareId);
            var compare = (await _compareRepository.GetAsync(spec)).FirstOrDefault();
            compare?.RemoveItem(productId);
            await _compareRepository.UpdateAsync(compare);
        }

        private async Task<Compare> GetExistingOrCreateNewCompare(string userName)
        {
            var compare = await _compareRepository.GetByUserNameAsync(userName);
            if (compare != null)
                return compare;

            // if it is first attempt create new
            var newCompare = new Compare
            {
                UserName = userName
            };

            await _compareRepository.AddAsync(newCompare);
            return newCompare;
        }

    }
}
