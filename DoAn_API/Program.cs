using DoAn_API.Data;
using DoAn_API.Entities;
using DoAn_API.Middlewares;
using DoAn_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "NutriCook API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Nhập JWT token của bạn vào đây.\n\nVí dụ: eyJhbGciOiJIUzI1NiIsInR..."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 1. Kích hoạt Caching trong bộ nhớ (RAM)
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // CẤU HÌNH MẬT KHẨU
    options.Password.RequiredLength = 6;            // Độ dài tối thiểu là 6 ký tự
    options.Password.RequireDigit = false;          // Không bắt buộc phải có chữ số (0-9)
    options.Password.RequireLowercase = false;      // Không bắt buộc phải có chữ thường (a-z)
    options.Password.RequireUppercase = false;      // Không bắt buộc phải có chữ hoa (A-Z)
    options.Password.RequireNonAlphanumeric = false;// Không bắt buộc ký tự đặc biệt (!@#$...)
    options.Password.RequiredUniqueChars = 0;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15); // Khóa 15 phút
    options.Lockout.MaxFailedAccessAttempts = 5;                       // Khóa sau 5 lần nhập sai
    options.Lockout.AllowedForNewUsers = true;                         // Áp dụng cho cả user mới tạo
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Set true nếu chạy Production
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true, // Kiểm tra xem token còn hạn không
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextJsApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // URL Frontend của bạn
              .AllowAnyHeader()  // Cho phép mọi Headers (như Authorization, Content-Type...)
              .AllowAnyMethod()  // Cho phép mọi Methods (GET, POST, PUT, DELETE...)
              .AllowCredentials(); // Rất quan trọng nếu bạn gửi Token kèm theo
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Bỏ qua các thuộc tính tạo ra vòng lặp
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<NutritionService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ITipService, TipService>();
builder.Services.AddScoped<IInteractionService, InteractionService>();
builder.Services.AddScoped<IUserActivityService, UserActivityService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IUploadService, UploadService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IPostDeletionService, PostDeletionService>(); // <-- Thêm dòng này

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 2. Đăng ký Global Exception Middleware (Phải đặt trước các Use khác)
app.UseMiddleware<ExceptionMiddleware>();
app.UseRouting();
app.UseCors("AllowNextJsApp");
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
