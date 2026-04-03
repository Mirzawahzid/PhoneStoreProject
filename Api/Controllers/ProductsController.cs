using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Dapper;
using PhoneStoreApi.Models;

namespace PhoneStoreApi.Controllers;

/// <summary>Manage phone products in the store inventory.</summary>
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IConfiguration _config;

    public ProductsController(IConfiguration config)
    {
        _config = config;
    }

    /// <summary>Get all products</summary>
    /// <remarks>Returns the full list of phones in the inventory.</remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<Product>), 200)]
    public async Task<IActionResult> GetProducts()
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var products = await connection.QueryAsync<Product>("SELECT * FROM Products");

        return Ok(products);
    }

    /// <summary>Add a new product</summary>
    /// <remarks>Creates a new phone entry in the inventory.</remarks>
    [HttpPost]
    [ProducesResponseType(201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> AddProduct(Product product)
    {
        var connectionString = _config.GetConnectionString("DefaultConnection");

        using var connection = new SqlConnection(connectionString);

        var sql = @"INSERT INTO Products (Name, Brand, Price, Stock, ImageUrl)
                    VALUES (@Name, @Brand, @Price, @Stock, @ImageUrl)";

        await connection.ExecuteAsync(sql, product);

        return StatusCode(201, new { message = "Product added successfully" });
    }

    /// <summary>Update a product</summary>
    /// <param name="id">The product ID to update</param>
    [HttpPut("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
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

    /// <summary>Delete a product</summary>
    /// <param name="id">The product ID to delete</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
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