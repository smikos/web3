
public class ProductService
{
    private List<Product> _products = new List<Product>();

    public Product GetProductById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public List<Product> GetProducts()
    {
        return _products;
    }

    public void AddProduct(Product product)
    {
        _products.Add(product);
    }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    // Другие свойства товара
}

// Далее создадим API и GraphQL для доступа к сервису ProductService

// Реализация API контроллера для ProductService
[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ProductService _productService;

    public ProductController(ProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<Product>> GetProducts()
    {
        return _productService.GetProducts();
    }

    [HttpGet("{id}")]
    public ActionResult<Product> GetProduct(int id)
    {
        var product = _productService.GetProductById(id);
        if (product == null)
        {
            return NotFound();
        }
        return product;
    }
}

// Реализация GraphQL для ProductService
public class ProductSchema : Schema
{
    public ProductSchema(IDependencyResolver resolver) : base(resolver)
    {
        Query = resolver.Resolve<ProductQuery>();
    }
}

public class ProductQuery : ObjectGraphType
{
    public ProductQuery(ProductService productService)
    {
        Field<ListGraphType<ProductType>>(
            "products",
            resolve: context => productService.GetProducts()
        );

        Field<ProductType>(
            "product",
            arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "id" }),
            resolve: context =>
            {
                var id = context.GetArgument<int>("id");
                return productService.GetProductById(id);
            }
        );
    }
}

public class ProductType : ObjectGraphType<Product>
{
    public ProductType()
    {
        Field(x => x.Id);
        Field(x => x.Name);
        Field(x => x.Price);
        // Добавляем остальные поля товара
    }
}

// Наконец, реализуем API Gateway для ProductService и сервиса из второй лекции

// Взаимодействие с другими микросервисами через HttpClient или gRPC
public class ApiGateway
{
    private HttpClient _httpClient;

    public ApiGateway(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GetProductInfoFromWarehouse(int productId)
    {
        HttpResponseMessage response = await _httpClient.GetAsync($"https://warehouse-service/api/products/{productId}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        return null;
    }

    // Добавляем другие методы для взаимодействия с другими микросервисами
}

// Этот код демонстрирует создание отдельного сервиса для хранения информации о товарах на складе/магазине,
// реализацию доступа к этому сервису через API и GraphQL,
// а также использование API Gateway для взаимодействия с другими микросервисами.



// Разработка web-приложения на C# (семинары)
// Урок 3. GraphQL и микросервисная архитектура
// Добавьте отдельный сервис позволяющий хранить информацию о товарах на складе/магазине. 
// Реализуйте к нему доступ посредством API и GraphQL.
// Реализуйте API-Gateway для API сервиса склада и API-сервиса из второй лекции.

// 1. Создайте новый проект веб-приложения на C# для сервиса товаров на складе. Добавьте необходимые модели данных для товаров
// (например, название, цена, количество на складе и т.д.).

// 2. Реализуйте API для этого сервиса, позволяющий выполнять CRUD операции с товарами (создание, чтение, обновление, удаление).

// 3. Добавьте GraphQL поддержку к вашему API сервису. Создайте GraphQL схему и определите необходимые запросы и мутации для работы с товарами.

// 4. Разработайте API-Gateway, который будет объединять вызовы к API сервисам склада и другим сервисам из предыдущих уроков. Используйте GraphQL как промежуточный слой для запросов к сервисам.

// 5. Протестируйте работу вашего web-приложения, убедитесь что API и GraphQL схемы работают корректно.

// Эти шаги помогут вам разработать web-приложение на C# с микросервисной архитектурой, используя GraphQL для удобного доступа к данным.


// 1. Напишите модели данных для товаров:

public class Product
{
public int Id { get; set; }
public string Name { get; set; }
public decimal Price { get; set; }
public int QuantityInStock { get; set; }
}
```

// 2. Реализуйте API для CRUD операций с товарами (создание, чтение, обновление, удаление):

// GET api/products
[HttpGet]
public IActionResult Get()
{
var products = _repository.GetProducts();
return Ok(products);
}

// GET api/products/{id}
[HttpGet("{id}")]
public IActionResult Get(int id)
{
var product = _repository.GetProductById(id);
if (product == null)
{
return NotFound();
}
return Ok(product);
}

// POST api/products
[HttpPost]
public IActionResult Post([FromBody] Product product)
{
_repository.AddProduct(product);
return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
}

// PUT api/products/{id}
[HttpPut("{id}")]
public IActionResult Put(int id, [FromBody] Product product)
{
if (id != product.Id)
{
return BadRequest();
}
_repository.UpdateProduct(product);
return NoContent();
}

// DELETE api/products/{id}
[HttpDelete("{id}")]
public IActionResult Delete(int id)
{
_repository.DeleteProduct(id);
return NoContent();
}
```

// 3. Добавьте GraphQL поддержку к вашему API:

public class ProductSchema : Schema
{
public ProductSchema(IDependencyResolver resolver) : base(resolver)
{
Query = resolver.Resolve();
Mutation = resolver.Resolve();
}
}

public class ProductQuery : ObjectGraphType
{
public ProductQuery(IProductRepository productRepository)
{
Field(
"products",
resolve: context => productRepository.GetProducts()
);

Field(
"product",
arguments: new QueryArguments(new QueryArgument { Name = "id" }),
resolve: context =>
{
var id = context.GetArgument("id");
return productRepository.GetProductById(id);
}
);
}
}

public class ProductMutation : ObjectGraphType
{
public ProductMutation(IProductRepository productRepository)
{
Field(
"createProduct",
arguments: new QueryArguments(new QueryArgument { Name = "product" }),
resolve: context =>
{
var product = context.GetArgument("product");
return productRepository.AddProduct(product);
}
);

Field(
"updateProduct",
arguments: new QueryArguments(new QueryArgument { Name = "id" },
new QueryArgument { Name = "product" }),
resolve: context =>
{
var id = context.GetArgument("id");
var product = context.GetArgument("product");
product.Id = id;
return productRepository.UpdateProduct(product);
}
);

Field(
"deleteProduct",
arguments: new QueryArguments(new QueryArgument { Name = "id" }),
resolve: context =>
{
var id = context.GetArgument("id");
return productRepository.DeleteProduct(id);
}
);
}
}

public class ProductType : ObjectGraphType
{
public ProductType()
{
Field(x => x.Id);
Field(x => x.Name);
Field(x => x.Price);
Field(x => x.QuantityInStock);
}
}

public class ProductInputType : InputObjectGraphType
{
public ProductInputType()
{
Field(x => x.Name);
Field(x => x.Price);
Field(x => x.QuantityInStock);
}
}
```

// 4. Разработайте API-Gateway:

public class APIGateway
{
private readonly HttpClient _client;

public APIGateway(HttpClient client)
{
_client = client;
}

public async Task GetProductsAsync()
{
var response = await _client.GetAsync("api/products");
response.EnsureSuccessStatusCode();
var productsJson = await response.Content.ReadAsStringAsync();
return JsonConvert.DeserializeObject(productsJson);
}

// Implement other CRUD operations similarly
}
```

// 5. Протестируйте ваше web-приложение, удостоверившись, что все компоненты работают корректно.

---