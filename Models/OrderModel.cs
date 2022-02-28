using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OrderService.Models
{
    public enum EnumOrderStatus
    {
        INITIATED = 1,
        SUCCESS = 2,
        FAILED = 3
    }
    public class ProductModel
    {
        [Key]
        public int ProductID { set; get; } = 0;
        public double ProductPrices { set; get; } = 0.0;
    }
    public class OrderModel
    {
        [Key]
        public int OrderID { set; get; } = 0;
        public int OrderStatusID { set; get; } = 0;
        public string OrderStatus
        {
            get
            {
                return GetEnumOrderStatus(OrderStatusID);
            }
        }
        public int CartID { set; get; }

        public static string GetEnumOrderStatus(int input)
        {
            return ((EnumOrderStatus)input).ToString();
        }

        public List<ProductModel> ProductList { set; get; }
    }
}
