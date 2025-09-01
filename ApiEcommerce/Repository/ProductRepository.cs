using System;
using ApiEcommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiEcommerce.Repository.IRepository;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _db;

    public ProductRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public bool BuyProduct(string name, int quantity)
    {
        if (string.IsNullOrWhiteSpace(name) || quantity <= 0)
        {
            return false;
        }

        var product = _db.Products.FirstOrDefault(p =>
            p.Name.Trim().ToLower() == name.Trim().ToLower()
        );
        if (product == null || product.Stock < quantity)
        {
            return false;
        }

        product.Stock -= quantity;
        _db.Products.Update(product);
        return Save();
    }

    public bool CreateProduct(Product product)
    {
        if (product == null)
        {
            return false;
        }
        product.CreatedAt = DateTime.UtcNow;
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Add(product);
        return Save();
    }

    public bool DeleteProduct(Product product)
    {
        if (product == null)
        {
            return false;
        }
        _db.Products.Remove(product);
        return Save();
    }

    public Product? GetProduct(int productId)
    {
        if (productId <= 0)
        {
            return null;
        }

        return _db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == productId);
    }

    public ICollection<Product> GetProducts()
    {
        return _db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToList();
    }

    public ICollection<Product> GetProductsForCategory(int categoryId)
    {
        if (categoryId <= 0)
        {
            return new List<Product>();
        }

        return _db
            .Products.Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderBy(p => p.Name)
            .ToList();
    }

    public ICollection<Product> GetProductsInPages(int pageNumber, int pageSize)
    {
        return _db
            .Products.Include(p => p.Category)
            .OrderBy(p => p.ProductId)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }

    public int GetTotalProducts()
    {
        return _db.Products.Count();
    }

    public bool ProductExists(int id)
    {
        if (id <= 0)
        {
            return false;
        }
        return _db.Products.Any(p => p.ProductId == id);
    }

    public bool ProductExists(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }
        return _db.Products.Any(p => p.Name.Trim().ToLower() == name.Trim().ToLower());
    }

    public bool Save()
    {
        return _db.SaveChanges() >= 0;
    }

    public ICollection<Product> SearchProducts(string searchTerm)
    {
        var searchTermToLower = searchTerm.Trim().ToLower();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return new List<Product>();
        }

        IQueryable<Product> query = _db.Products;
        query = query
            .Include(p => p.Category)
            .Where(p =>
                p.Name.Trim().ToLower().Contains(searchTermToLower)
                || p.Description.Trim().ToLower().Contains(searchTermToLower)
            );
        return query.OrderBy(p => p.Name).ToList();
    }

    public bool UpdateProduct(Product product)
    {
        if (product == null)
        {
            return false;
        }
        product.UpdatedAt = DateTime.UtcNow;
        _db.Products.Update(product);
        return Save();
    }
}
