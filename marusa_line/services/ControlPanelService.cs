using Dapper;
using System.Data;
using marusa_line.interfaces;
using Microsoft.Data.SqlClient;
using marusa_line.Dtos.ControlPanelDtos;
using marusa_line.Dtos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using marusa_line.Models;
using marusa_line.Dtos.ControlPanelDtos.Dashboard;
using marusa_line.Dtos.ControlPanelDtos.User;
using marusa_line.Dtos.ControlPanelDtos.NewFolder;
using marusa_line.Dtos.ControlPanelDtos.ShopDtos;

namespace marusa_line.services
{
    public class ControlPanelService : ControlPanelInterface
    {
       
        private readonly string _connectionString;
        private readonly IConfiguration _config;

        public ControlPanelService(IConfiguration config)
        {
            _config = config;
            _connectionString = config.GetConnectionString("marusa_line_connection");
        }

        public async Task<string?> AuthorizeShopAsync(string gmail, string password)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var shop = await conn.QuerySingleOrDefaultAsync<ShopDto>(
                "[dbo].[spAuthorizeShop]",
                new
                {
                    Gmail = gmail,
                    Password = password
                },
                commandType: CommandType.StoredProcedure
            );

            if (shop == null)
                return null;

            return CreateJwtForShop(shop);
        }
        private string CreateJwtForShop(ShopDto shop)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Appsettings:Token"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
         
            var claims = new List<Claim>
            {
                new Claim("ShopId", shop.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, shop.Gmail),

                new Claim("Subscription", shop.Subscription),
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(30),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<List<OrderControlPanel>> GetOrdersControlPanel(GetOrdersControlPanelDto order,int shopid)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var lookup = new Dictionary<int, OrderControlPanel>();

            var result = await conn.QueryAsync<OrderControlPanel, Photos, User, OrderControlPanel>(
                "[dbo].[GetAllOrdersControlPanel]",
                (orderControl, photo, user) =>
                {
                    if (!lookup.TryGetValue(orderControl.OrderId, out var existingOrder))
                    {
                        existingOrder = orderControl;
                        existingOrder.Photos = new List<Photos>();
                        existingOrder.User = user;
                        lookup.Add(existingOrder.OrderId, existingOrder);
                    }

                    if (photo != null && photo.PhotoId != 0)
                    {
                        existingOrder.Photos.Add(photo);
                    }
                    return existingOrder;
                },
                param: new
                {
                    ShopId = shopid,
                    UserId = order.UserId,
                    IsPaid = order.IsPaid,
                    OrderId = order.OrderId,
                    PageNumber = order.PageNumber,
                    PageSize = order.PageSize,
                },
                splitOn: "PhotoId,UserId",
                commandType: CommandType.StoredProcedure
            );

            return lookup.Values.ToList();
        }

        public async Task<List<Post>> GetPostsForAdminPanel(GetPostsDto getPosts, int shopid)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<Post>(
                "[dbo].[GetProductsForControlPanel]",
                new
                {
                    ShopId = shopid,
                    ProductTypeId = getPosts.ProductTypeId,
                    IsDeleted = getPosts.IsDeleted,
                    PageNumber = getPosts.PageNumber,
                    PageSize = getPosts.PageSize
                },
                commandType: CommandType.StoredProcedure
            );

            return result.ToList();
        }


        public async Task<int> ToggleOrderIsPaidAsync(int orderId, bool isPaid,int quantity)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId, DbType.Int32);
            parameters.Add("@Paid", isPaid, DbType.Boolean);
            parameters.Add("@OrderCount", quantity, DbType.Int32);

            var rowsAffected = await conn.ExecuteAsync(
                "[dbo].[ChangeOrderIsPaid]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<int> ChangeOrderStatus(int orderId, int isPaid)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId, DbType.Int32);
            parameters.Add("@StatusId", isPaid, DbType.Int32);

            var rowsAffected = await conn.ExecuteAsync(
                "[dbo].[ChangeOrderStatus]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<int> GetOrdersTotalCountAsync(bool? isPaid, int shopId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ShopId", shopId, DbType.Int32);
            parameters.Add("@Paid", isPaid, DbType.Boolean);

            var totalCount = await conn.QuerySingleAsync<int>(
                "[dbo].[getOrdersTotalCount]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return totalCount;
        }

        public async Task<int> DeleteOrder(int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@OrderId", orderId, DbType.Int32);
            var rowsAffected = await conn.ExecuteAsync(
                "[dbo].[DeleteOrder]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<OrderDetailsDto?> GetOrderById(int shopId, int orderId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<OrderDetailsDto, User, OrderDetailsDto>(
                "[dbo].[GetOrderByIdControlPanel]",
                (order, user) =>
                {
                    order.user = user;
                    return order;
                },
                new
                {
                    ShopId = shopId,
                    OrderId = orderId
                },
                splitOn: "UserId",
                commandType: CommandType.StoredProcedure
            );

            return result.FirstOrDefault();
        }

        public async Task<Post?> GetPostWithIdControlPanel(int id, int? userId = null)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var lookup = new Dictionary<int, Post>();

            var result = await conn.QueryAsync<Post, Photos, Post>(
                "[dbo].[GetProductsByIdForControlPanel]",
                (post, photo) =>
                {
                    if (!lookup.TryGetValue(post.Id, out var existingPost))
                    {
                        existingPost = post;
                        existingPost.Photos = new List<Photos>();
                        lookup.Add(existingPost.Id, existingPost);
                    }

                    if (photo != null && photo.PhotoId > 0)
                    {
                        existingPost.Photos.Add(photo);
                    }

                    return existingPost;
                },
                param: new
                {
                    Id = id,
                    UserId = userId
                },
                splitOn: "PhotoId",
                commandType: CommandType.StoredProcedure
            );
            return lookup.Values.FirstOrDefault();
        }
        public async Task<int> InsertPostAsync(InsertPostDto dto, int ShopId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var postId = await conn.ExecuteScalarAsync<int>(
                "[dbo].[InsertProduct]",
                new
                {
                    ShopId,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    dto.DiscountedPrice,
                    dto.Quantity,
                    dto.ProductTypeId,
                    dto.OrderNotAllowed,
                },
                commandType: CommandType.StoredProcedure
            );
            foreach (var photo in dto.Photos)
            {
                await conn.ExecuteAsync(
                    "[dbo].[InsertPhoto]",
                    new { ProductId = postId, photo.PhotoUrl },
                    commandType: CommandType.StoredProcedure
                );
            }
            return postId;
        }

        public async Task<int> EditPostAsync(InsertPostDto dto)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var postId = await conn.ExecuteScalarAsync<int>(
                "[dbo].[EditProduct]",
                new
                {
                    dto.Id,
                    dto.Title,
                    dto.Description,
                    dto.Price,
                    dto.DiscountedPrice,
                    dto.ProductTypeId
                },
                commandType: CommandType.StoredProcedure
            );
            if (dto.Photos != null){
            foreach (var photo in dto.Photos)
            {
                await conn.ExecuteAsync(
                    "[dbo].[InsertPhoto]",
                    new { ProductId = postId, photo.PhotoUrl },
                    commandType: CommandType.StoredProcedure
                );
            }
            }
            return postId;
        }

        public async Task<int> RemoveProductById(int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId, DbType.Int32);
            var rowsAffected = await conn.QuerySingleAsync<int>(
                "[dbo].[RemoveProductById]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }
        public async Task<int> RevertProductById(int productId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@ProductId", productId, DbType.Int32);
            var rowsAffected = await conn.QuerySingleAsync<int>(
                "[dbo].[RevertProductById]",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return rowsAffected;
        }

        public async Task<DateTime> deletePhoto(int photoId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var photoDeleted = await conn.ExecuteScalarAsync<DateTime>(
                "[dbo].[DeletePhoto]",
                new
                {
                    PhotoId = photoId,
                },
                commandType: CommandType.StoredProcedure
            );
            return photoDeleted;
        }

        public async Task<int> GetLikeCount()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var count = await conn.QuerySingleAsync<int>(
                "[dbo].[GetLikesCount]",
                commandType: CommandType.StoredProcedure
            );
            return count;
        }

        public async Task<DashboardStats> GetDashboardStatistics(int shopid,GetDahsboard stats)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QuerySingleAsync<DashboardStats>(
                "[dbo].[GetOrderStatistics]",
                new
                {
                    ShopId = shopid,
                    StartDate = stats.StartDate,
                    EndDate = stats.EndDate
                },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
        public async Task<DashboardStatsByYear> GetDashboard(int shopid,int year)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var statsByMonth = (await conn.QueryAsync<DashboardStatsByMonths>(
                "[dbo].[spGetMonthlyOrderStats]",
                new 
                { 
                    ShopId=shopid,
                    Year = year
                },
                commandType: CommandType.StoredProcedure
            )).ToList();

            var YearStat = await conn.QuerySingleAsync<DashboardYearSum>(
                "[dbo].[spGetYearlyOrderStats]",
                new 
                {
                    ShopId = shopid,
                    Year = year 
                },
                commandType: CommandType.StoredProcedure
            );

            return new DashboardStatsByYear
            {
                statsByMonth = statsByMonth,
                YearStat = YearStat
            };
        }

        public async Task<List<SoldProductTypes>> GetSoldProductTypes(int shopid,int year, int? month)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<SoldProductTypes>(
                "[dbo].[spGetProductTypeSalesStats]",
                new
                {
                    ShopId = shopid,
                    Year = year,
                    Month= month
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
  

        public async Task<List<ProductTypes>> InsertProducType(int shopId, string productType)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<ProductTypes>(
                "[dbo].[AddProductType]",
                new
                {
                    ShopId = shopId,
                    productType = productType,
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<ProductTypes>> EditProductType(int  shopId,int id, string productType)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<ProductTypes>(
                "[dbo].[EditProductType]",
                new
                {
                    ShopId= shopId,
                    Id = id,
                    productType = productType,
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
        public async Task<List<ProductTypes>> DeleteProductType(int shopId,int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<ProductTypes>(
                "[dbo].[DeleteProductType]",
                new
                {
                    ShopId = shopId,
                    Id = id,
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<GetUserDto> GetUser(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QuerySingleAsync<GetUserDto>(
                "[dbo].[GetuserById]",
                new
                {
                    Id = id,
                },
                commandType: CommandType.StoredProcedure
            );
            return result;
        }
        public async Task<List<GetUserDto>> SearchUserByName(int shopId,string search)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<GetUserDto>(
                "[dbo].[spSearchUsersByName]",
                new
                {
                    ShopId= shopId,
                    SearchText = search,
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }
        public async Task<List<GetUserDto>> SearchUserByEmail(string search)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var result = await conn.QueryAsync<GetUserDto>(
                "[dbo].[spSearchUsersByEmail]",
                new
                {
                    SearchText = search,
                },
                commandType: CommandType.StoredProcedure
            );
            return result.ToList();
        }

        public async Task<List<GetUserDto>> GetUsersList(GetUserFilteredDto dto, int shopId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            using var multi = await conn.QueryMultipleAsync(
                "[dbo].[GetUsersList]",
                new
                {

                    ShopId = shopId,
                    UserId = dto.UserId,
                    IsBlocked = dto.IsBlocked,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize
                },
                commandType: CommandType.StoredProcedure
            );

            var users = (await multi.ReadAsync<GetUserDto>()).ToList();
            var totalCount = await multi.ReadFirstAsync<int>();
            users[0].totalCount = totalCount;

            return users;
        }

        public async Task<List<GetUserDto>> GetShopFollowersList(int shopId, GetUserFilteredDto dto)
        { 
            using var conn = new SqlConnection(_connectionString);

            using var multi = await conn.QueryMultipleAsync(
                "[dbo].[GetShopFollowersList]",
                new
                {
                    ShopId = shopId,
                    UserId = dto.UserId,
                    IsBlocked = dto.IsBlocked,
                    PageNumber = dto.PageNumber,
                    PageSize = dto.PageSize
                },
                commandType: CommandType.StoredProcedure
            );

            var users = (await multi.ReadAsync<GetUserDto>()).ToList();
            return users;
        }


        public async Task<int> UpdateProductOderAllowed(int productID, bool allowed)
        {
            using var conn = new SqlConnection(_connectionString);

            var result = await conn.QueryAsync<int>(
                "[dbo].[UpdateProductOrderAllowed]",
                new
                {
                    ProductId = productID,
                    Allowed = allowed
                },
                commandType: CommandType.StoredProcedure
            ) ;
            return 0;
        }
        public async Task<int> UpdateQuantity(int productId, int quantity)
        {
            using var conn = new SqlConnection(_connectionString);

            var result = await conn.QueryAsync<int>(
                "[dbo].[UpdateQuantity]",
                new
                {
                    ProductId = productId,
                    quantity = quantity
                },
                commandType: CommandType.StoredProcedure
            );
            return 0;
        }
        public async Task<int> UpdateUserRole(int userId, string role)
        {
            using var conn = new SqlConnection(_connectionString);

            var parameters = new DynamicParameters();
            parameters.Add("UserId", userId);
            parameters.Add("Role", role);
            parameters.Add("ReturnValue", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);

            await conn.ExecuteAsync(
                "[dbo].[UpdateRole]",
                parameters,
                commandType: CommandType.StoredProcedure
            );
            return parameters.Get<int>("ReturnValue");
        }
        public async Task<ShopStatsDto> GetShopStats(int shopId)
        {
            using var conn = new SqlConnection(_connectionString);

            return await conn.QuerySingleAsync<ShopStatsDto>(
                "[dbo].[spGetShopStats]",
                new { ShopId = shopId },
                commandType: CommandType.StoredProcedure
            );
        }

        public async Task<ShopDto?> GetShopById(int shopId)
        {
            using var conn = new SqlConnection(_connectionString);

            return await conn.QuerySingleOrDefaultAsync<ShopDto>(
                "[dbo].[spGetShopById]",
                new { ShopId = shopId },
                commandType: CommandType.StoredProcedure
            );
        }
        public async Task<bool> UpdateShopAsync(ShopDto shop,int shopid)
        {
            using var conn = new SqlConnection(_connectionString);

            var result = await conn.QuerySingleAsync<bool>(
                "[dbo].[spUpdateShop]",
                new
                {
                    ShopId = shopid,
                    Name = shop.Name,
                    Logo = shop.Logo,
                    Location = shop.Location,
                    Gmail = shop.Gmail,
                    Subscription = shop.Subscription,
                    Instagram = shop.Instagram,
                    Facebook = shop.Facebook,
                    Tiktok = shop.Titkok,
                    BOG = shop.Bog,
                    TBC = shop.Tbc,
                    Receiver = shop.Receiver,
                },
                commandType: CommandType.StoredProcedure
            );

            return result;
        }
        public async Task <int>BlockUser(int userId, int shopId)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.ExecuteAsync(
                "[dbo].[BlockUserFromShop]",
                new { ShopId = shopId, UserId = userId },
                commandType: CommandType.StoredProcedure
            );
            return 1;
        }
    }
}
