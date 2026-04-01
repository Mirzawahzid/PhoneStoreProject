using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PhoneStoreApi.Models;

namespace PhoneStoreApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IConfiguration _config;

    public ProductsController(IConfiguration config)
    {
        _config = config;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var products = await connection.QueryAsync<Product>("SELECT * FROM Products");

        return Ok(products);
    }

    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var sql = @"INSERT INTO Products (Name, Brand, Price, Stock, ImageUrl)
                    VALUES (@Name, @Brand, @Price, @Stock, @ImageUrl)";

        await connection.ExecuteAsync(sql, product);

        return Ok(new { message = "Product added successfully" });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var sql = @"UPDATE Products 
                    SET Name = @Name,
                        Brand = @Brand,
                        Price = @Price,
                        Stock = @Stock,
                        ImageUrl = @ImageUrl
                    WHERE Id = @Id";

        product.Id = id;

        var rowsAffected = await connection.ExecuteAsync(sql, product);

        if (rowsAffected == 0)
            return NotFound(new { message = "Product not found" });

        return Ok(new { message = "Product updated successfully" });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var rowsAffected = await connection.ExecuteAsync(
            "DELETE FROM Products WHERE Id = @Id", new { Id = id });

        if (rowsAffected == 0)
            return NotFound(new { message = "Product not found" });

        return Ok(new { message = "Product deleted successfully" });
    }
}