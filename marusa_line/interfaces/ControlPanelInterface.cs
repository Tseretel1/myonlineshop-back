using Azure.Identity;
using marusa_line.Dtos;
using marusa_line.Dtos.AdminPanelDtos;
using marusa_line.Dtos.ControlPanelDtos;
using marusa_line.Dtos.ControlPanelDtos.Dashboard;
using marusa_line.Dtos.ControlPanelDtos.NewFolder;
using marusa_line.Dtos.ControlPanelDtos.ShopDtos;
using marusa_line.Dtos.ControlPanelDtos.User;
using marusa_line.Models;

namespace marusa_line.interfaces
{
    public interface ControlPanelInterface
    {
        Task<string?> AuthorizeShopAsync(string gmail, string password);
        Task<List<OrderControlPanel>> GetOrdersControlPanel(GetOrdersControlPanelDto order, int shopid);
        Task<List<Post>> GetPostsForAdminPanel(GetPostsDto getPosts,int shopid);
        Task<int> ToggleOrderIsPaidAsync(int orderId,bool isPaid, int quantity);
        Task<int> ChangeOrderStatus(int orderId, int isPaid);
        Task<OrderDetailsDto> GetOrderById(int shopId, int userId);
        Task<int> DeleteOrder(int orderId);
        Task<int> GetOrdersTotalCountAsync(bool? isPaid,int shopId);
        Task<Post?> GetPostWithIdControlPanel(int id, int? userId = null);
        Task<int> InsertPostAsync(InsertPostDto dto,int shopid);
        Task<int> EditPostAsync(InsertPostDto dto);
        Task<int> RemoveProductById(int postId);
        Task<int> RevertProductById(int postId);
        Task<DateTime> deletePhoto(int photoId);
        Task<int> GetLikeCount();
        Task<DashboardStats> GetDashboardStatistics(int shopid, GetDahsboard stats);
        Task<DashboardStatsByYear> GetDashboard(int shopid, int year);
        Task<List<SoldProductTypes>> GetSoldProductTypes(int shopid, int year,int? month);
        Task<List<ProductTypes>> InsertProducType(int shopId, string productType);
        Task<List<ProductTypes>> EditProductType(int shopId,int id,string productType);
        Task<List<ProductTypes>> DeleteProductType(int shopId,int id);
        Task<GetUserDto> GetUser(int id);
        Task<List<GetUserDto>>SearchUserByName(int shopId, string search);
        Task<List<GetUserDto>> SearchUserByEmail(string search);

        Task<int> UpdateUserRole(int userId, string role);
        Task<int> UpdateProductOderAllowed(int productID, bool allowed);
        Task<int> UpdateQuantity(int productId, int quantity);
        Task<List<GetUserDto>> GetUsersList(GetUserFilteredDto dto,int shopId);
        Task<List<GetUserDto>> GetShopFollowersList(int shopId, GetUserFilteredDto dto);
        Task<ShopStatsDto> GetShopStats(int shopId);
        Task<ShopDto?> GetShopById(int shopId);
        Task<bool> UpdateShopAsync(ShopDto shop,int shopId);
        Task<int> BlockUser(int userId, int shopId);
    }
}
