
using HotChocolate;
using Architecture_1.BusinessLogic.Services.DbServices.NhapServices;
using EF = Architecture_1.DataAccess.Entities;
using System.Text.Json;
using Architecture_1.GraphQL.Schema.MutationGroups;

namespace Architecture_1.API.GraphQL.Features.Product.Mutations
{
    public class ProductMutation
    {
        private readonly ProductService _productService;

        public ProductMutation(ProductService productService)
        {
            _productService = productService;
        }

        // Mutation thêm mới sản phẩm
        public EF.Product AddProduct(
            [Service] ProductService productService,
            string productName,
            int? categoryId,
            decimal price,
            int? stockQuantity
            )
        {
            var product = new EF.Product
            {
                ProductName = productName,
                CategoryId = categoryId,
                Price = price,
                StockQuantity = stockQuantity,
                CreatedAt = DateTime.Now
            };

            productService.AddProduct(product); // Giả sử có phương thức AddProduct trong ProductService

            return product;
        }

        // Mutation cập nhật thông tin sản phẩm
        public EF.Product UpdateProduct(
            [Service] ProductService productService,
            int productId,
            string productName,
            int? categoryId,
            decimal price,
            int? stockQuantity
            )
        {
            var existingProduct = productService.GetProductById(productId);
            if (existingProduct == null) throw new Exception("Product not found");

            existingProduct.ProductName = productName;
            existingProduct.CategoryId = categoryId;
            existingProduct.Price = price;
            existingProduct.StockQuantity = stockQuantity;
            existingProduct.CreatedAt = existingProduct.CreatedAt ?? DateTime.Now;

            productService.UpdateProduct(existingProduct); // Giả sử có phương thức UpdateProduct trong ProductService

            return existingProduct;
        }

        public JsonDocument DeleteProduct(
            [Service] ProductService productService,
            int productId
            )
        {
            var existingProduct = productService.GetProductById(productId);
            if (existingProduct == null) throw new Exception("Product not found");

            productService.DeleteProduct(existingProduct.ProductId); // Giả sử có phương thức DeleteProduct trong ProductService

            var data = new
            {
                message = "Product deleted successfully",
                productId
            };

            // Serialize object to JSON string
            string jsonString = JsonSerializer.Serialize(data);

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            // Return JsonDocument
            return jsonDocument;
        }

        public string DeleteProduct_ReturnString(
            [Service] ProductService productService,
            int productId
            )
        {
            var existingProduct = productService.GetProductById(productId);
            if (existingProduct == null) throw new Exception("Product not found");

            productService.DeleteProduct(existingProduct.ProductId); // Giả sử có phương thức DeleteProduct trong ProductService

            var data = new
            {
                message = "Product deleted successfully",
                productId
            };

            // Serialize object to JSON string
            string jsonString = JsonSerializer.Serialize(data);

            // Parse JSON string to JsonDocument
            JsonDocument jsonDocument = JsonDocument.Parse(jsonString);

            // Return JsonDocument
            return "Product deleted successfully";
        }
    }
}
