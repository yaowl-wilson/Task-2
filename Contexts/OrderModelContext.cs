using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using OrderService.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace OrderService.Contexts
{
    public class OrderModelContext : DbContext
    {
        private IDbContextTransaction transaction = null;
        private readonly ILogger<OrderModelContext> _logger;

        public OrderModelContext(
            DbContextOptions<OrderModelContext> options,
            ILogger<OrderModelContext> logger) : base(options)
        {
            _logger = logger;
        }
        public OrderModelContext(DbContextOptions<OrderModelContext> options) : base(options) { }
        public DbSet<OrderModel> OrderModelItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("dbo");
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<OrderModel>(entity => {
                entity.ToTable("tbl_orders");
            });

            //modelBuilder.Entity<CartModel>().HasNoKey();
            //modelBuilder.Entity<ProductModel>().HasNoKey();
        }

        public async Task<int> InsertOrderItems(OrderModel orderModel)
        {
            int orderID = -1;

            using (transaction = Database.BeginTransaction())
            {
                string sqlExecute =
                    @"EXEC dbo.InsertBookingItem @OrderID OUTPUT, @OrderStatusID, @ProductList";

                try
                {
                    SqlParameter[] parameters = new SqlParameter[]
                    {
                        new SqlParameter("@OrderID", SqlDbType.Int) { Direction = ParameterDirection.Output },
                        new SqlParameter("@OrderStatusID", orderModel.OrderStatusID),
                        new SqlParameter("@ProductList", JsonSerializer.Serialize(orderModel.ProductList))
                    };

                    await Task.Run(() => Database.ExecuteSqlRaw(sqlExecute, parameters));
                    SaveChanges();

                    orderID = (int)parameters[0].Value;

                    Debug.WriteLine("OrderModelContext InsertOrderItems orderID: " + orderID);
                    transaction.Commit();

                    _logger.LogInformation("Order Items Created Successfully with orderID: " + orderID);
                }
                catch (Exception ex)
                {
                    //----- Error Logging -----//
                    _logger.LogError("** Failed to create Order Items");

                    //----- Rollback Transaction -----//
                    transaction.Rollback();

                    //----- Clear Memory -----//
                    transaction.Dispose();

                    //----- Throw Exception -----//
                    throw new Exception(ex.Message);
                }
                finally
                {
                    //----- Clear Memory -----//
                    transaction.Dispose();
                }
            }

            return orderID;
        }
    }
}
