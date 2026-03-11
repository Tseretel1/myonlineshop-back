using marusa_line.Dtos;
using marusa_line.Dtos.ControlPanelDtos;
using marusa_line.Dtos.ControlPanelDtos.Dashboard;
using marusa_line.Dtos.ControlPanelDtos.ShopDtos;
using marusa_line.Dtos.ControlPanelDtos.User;
using marusa_line.interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace marusa_line.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ControlPanelController : Controller
    {
        private readonly ProductInterface _postService;
        private readonly ControlPanelInterface _controlPanelService;
        public ControlPanelController(ProductInterface postService, ControlPanelInterface controlPanelService)
        {
            _postService = postService;
            _controlPanelService = controlPanelService;
        }

        [HttpGet("login-to-shop")]
        public async Task<IActionResult> GetPostsForAdminPanel(string email, string password)
        {
            try
            {
                var token = await _controlPanelService.AuthorizeShopAsync(email,password);
                if (token == null || !token.Any())
                {
                    var notSucceded = new
                    {
                        succeeded = false,
                        token = "",
                    };
                    return Ok(notSucceded);
                }
                var Succeeded = new
                {
                    succeeded = true,
                    token = token,
                };
                return Ok(Succeeded);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("get-products")]
        public async Task<IActionResult> GetPostsForAdminPanel(GetPostsDto getPosts)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                {
                    return Unauthorized("ShopId missing in token");
                }
                int shopId = int.Parse(shopIdClaim);
                var posts = await _controlPanelService.GetPostsForAdminPanel(getPosts, shopId);
                if (posts == null || !posts.Any())
                {
                    return Ok();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-post-byid-controlpanel")]
        public async Task<IActionResult> GetPostByIdControlPanel(int id, int? userid)
        {
            try
            {
                var posts = await _controlPanelService.GetPostWithIdControlPanel(id, userid);

                if (posts == null)
                {
                    return NotFound();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("add-post")]
        public async Task<IActionResult> InsertPostAsync([FromBody] InsertPostDto dto)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                {
                    return Unauthorized("ShopId missing in token");
                }
                int shopId = int.Parse(shopIdClaim);
                var posts = await _controlPanelService.InsertPostAsync(dto,shopId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("edit-post")]
        public async Task<IActionResult> EditPostAsync([FromBody] InsertPostDto dto)
        {
            try
            {
                var posts = await _controlPanelService.EditPostAsync(dto);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("remove-post")]
        public async Task<IActionResult> RemovePost(int postid)
        {
            try
            {
                var posts = await _controlPanelService.RemoveProductById(postid);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("revert-post")]
        public async Task<IActionResult> RevertPost(int postid)
        {
            try
            {
                var posts = await _controlPanelService.RevertProductById(postid);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("delete-photo")]
        public async Task<IActionResult> deletePhoto(int photoId)
        {
            try
            {
                var posts = await _controlPanelService.deletePhoto(photoId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpGet("get-like-count")]
        public async Task<IActionResult> GetLikeCount()
        {
            try
            {
                var posts = await _controlPanelService.GetLikeCount();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("get-orders")]
        public async Task<IActionResult> GetOrdersControlPanel(GetOrdersControlPanelDto dto)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var orders = await _controlPanelService.GetOrdersControlPanel(dto, shopId);
                var totalCount = await _controlPanelService.GetOrdersTotalCountAsync(dto.IsPaid, shopId);

                if (orders == null || !orders.Any())
                {
                    return Ok(null);
                }
                var returnObj = new
                {
                    orders = orders,
                    totalCount = totalCount,
                };
                return Ok(returnObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("get-statistics")]
        public async Task<IActionResult> GetStatistics(GetDahsboard dto)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var statistics = await _controlPanelService.GetDashboardStatistics(shopId,dto);
                if (statistics == null)
                {
                    return Ok(null);
                }
                var returnObj = new
                {
                    statistics = statistics,
                };
                return Ok(returnObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-dashboard-by-year")]
        public async Task<IActionResult> GetDashboard(int year)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var statistics = await _controlPanelService.GetDashboard(shopId,year);
                if (statistics == null)
                {
                    return Ok(null);
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-sold-producttypes")]
        public async Task<IActionResult> GetSoldProductTypes(int year, int? month)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var statistics = await _controlPanelService.GetSoldProductTypes(shopId,year, month);
                if (statistics == null)
                {
                    return Ok(null);
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-order-details")]
        public async Task<IActionResult> GetOrderDetails(int orderId)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var orders = await _controlPanelService.GetOrderById(shopId,orderId);

                if (orders == null)
                {
                    return Ok(null);
                }
                else
                {
                    var prodcut = await _postService.GetOrderProduct(orders.ProductId, 0);
                    var returnOrder = new
                    {
                        orders,
                        product = prodcut,
                    };


                    return Ok(returnOrder);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost("change-order-ispaid")]
        public async Task<IActionResult> ChangeIsPaid(int orderId,bool ispaid, int quantity)
        {
            try
            {
                var posts = await _controlPanelService.ToggleOrderIsPaidAsync(orderId,ispaid, quantity);
                if (posts == null)
                {
                    return NotFound();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("change-order-status")]
        public async Task<IActionResult> orderStatus(int orderId, int statusId)
        {
            try
            {
                var posts = await _controlPanelService.ChangeOrderStatus(orderId, statusId);
                if (posts == null)
                {
                    return NotFound();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("delete-order")]
        public async Task<IActionResult> deleteOrder(int orderId)
        {
            try
            {
                var posts = await _controlPanelService.DeleteOrder(orderId);
                if (posts == null)
                {
                    return NotFound();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-product-types")]
        public async Task<IActionResult> GetProductTypes()
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var posts = await _postService.GetProductTypes(shopId);

                if (posts == null || !posts.Any())
                {
                    return Ok();
                }

                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("insert-product-type")]
        public async Task<IActionResult> InsertProductType(string productType)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var productTypes = await _controlPanelService.InsertProducType(shopId,productType);
                if (productTypes == null)
                {
                    return Ok(null);
                }
                var returnObj = new
                {
                    productTypes = productTypes,
                };
                return Ok(returnObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPost("edit-product-type")]
        public async Task<IActionResult> EditProductType(int id, string productType)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var productTypes = await _controlPanelService.EditProductType(shopId,id,productType);
                if (productTypes == null)
                {
                    return Ok(null);
                }
                var returnObj = new
                {
                    productTypes = productTypes,
                };
                return Ok(returnObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpDelete("delete-product-type")]
        public async Task<IActionResult> DeleteProductType(int id)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var productTypes = await _controlPanelService.DeleteProductType(shopId,id);
                if (productTypes == null)
                {
                    return Ok(null);
                }
                var returnObj = new
                {
                    productTypes = productTypes,
                };
                return Ok(returnObj);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("get-users")]
        public async Task<IActionResult> GetUsers(GetUserFilteredDto dto)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var users = await _controlPanelService.GetUsersList(dto, shopId);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("get-shop-followers")]
        public async Task<IActionResult> GetFollowersList(GetUserFilteredDto dto)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var users = await _controlPanelService.GetShopFollowersList(shopId, dto);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _controlPanelService.GetUser(id);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-user-by-name")]
        public async Task<IActionResult> GetsuerByName(string search)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var statistics = await _controlPanelService.SearchUserByName(shopId,search);
                if (statistics == null)
                {
                    return Ok(null);
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet("get-user-by-email")]
        public async Task<IActionResult> GetsuerByEmail(string search)
        {
            try
            {
                var statistics = await _controlPanelService.SearchUserByEmail(search);
                if (statistics == null)
                {
                    return Ok(null);
                }

                return Ok(statistics);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("update-user-role")]
        public async Task<IActionResult> UpdateUserRole(int id,string role)
        {
            try
            {
                var user = await _controlPanelService.UpdateUserRole(id, role);

                if (user == null)
                {
                    return NotFound();
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update-product-order-allowed")]
        public async Task<IActionResult> UpdateProductOderAllowed(int productID, bool allowed)
        {
            try
            {
                var user = await _controlPanelService.UpdateProductOderAllowed(productID, allowed);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-shop-stats")]
        public async Task<IActionResult> UpdateProductOderAllowed()
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var user = await _controlPanelService.GetShopStats(shopId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpGet("get-shop-by-id")]
        public async Task<IActionResult> GetShopbyId()
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);
                var user = await _controlPanelService.GetShopById(shopId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPut("update-shop")]
        public async Task<IActionResult> UpdateShop([FromBody] ShopDto shop)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var user = await _controlPanelService.UpdateShopAsync(shop,shopId);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("block-user")]
        public async Task<IActionResult> FollowShop(int userId)
        {
            try
            {
                var shopIdClaim = User.FindFirst("ShopId")?.Value;
                if (string.IsNullOrEmpty(shopIdClaim))
                    return Unauthorized("ShopId missing in token");
                int shopId = int.Parse(shopIdClaim);

                var posts = await _controlPanelService.BlockUser(userId, shopId);
                return Ok(posts);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [Authorize]
        [HttpPut("update-quantity")]
        public async Task<IActionResult> UpdateProductOderAllowed(int productId, int quantity)
        {
            try
            {
                var user = await _controlPanelService.UpdateQuantity(productId, quantity);
                return Ok(user);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
