using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using nguyentuanvuduy_2123110226.Data;
using nguyentuanvuduy_2123110226.Services;
using System.Text;
// Nhớ thêm dòng using này ở tít trên cùng file Program.cs:
using PayOS;// Đăng ký PayOSClient như một Singleton (chỉ tạo 1 lần dùng mãi mãi)
var builder = WebApplication.CreateBuilder(args);
// ====================================================
// ✅ CẤU HÌNH PAYOS (PHẢI ĐẶT Ở ĐÂY, SAU KHI CÓ builder)
// ====================================================
var payOsClientId = builder.Configuration["PayOS:ClientId"] ?? throw new Exception("Thiếu ClientId");
var payOsApiKey = builder.Configuration["PayOS:ApiKey"] ?? throw new Exception("Thiếu ApiKey");
var payOsChecksumKey = builder.Configuration["PayOS:ChecksumKey"] ?? throw new Exception("Thiếu ChecksumKey");
// Đăng ký Dependency Injection cho CategoryService
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IBlogCategoryService, BlogCategoryService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddSingleton(new PayOSClient(payOsClientId, payOsApiKey, payOsChecksumKey));

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();

// ✅ BƯỚC MỚI: Đăng ký CORS để cho phép React (chạy ở cổng 3000 hoặc 5173) gọi API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// BƯỚC 1: Đăng ký dịch vụ kiểm tra Token (JWT)
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// BƯỚC 2: Cấu hình Swagger có thêm nút nhập Token
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Nhập token theo định dạng: Bearer {mã_token_của_bạn}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();

app.UseHttpsRedirection();

// ✅ BƯỚC MỚI CỰC KỲ QUAN TRỌNG: Kích hoạt CORS (Bắt buộc phải nằm TRƯỚC Authentication)
app.UseCors("AllowReactApp");

// BƯỚC 3: Kích hoạt Middleware (Lưu ý: Authentication TRƯỚC Authorization)
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles(); // Cho phép trình duyệt truy cập thẳng vào thư mục wwwroot
app.MapControllers();

app.Run();