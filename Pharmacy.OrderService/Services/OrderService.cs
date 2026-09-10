// public async Task VerifyOrderAsync(int orderId)
// {
//     var order = await _context.Orders.FindAsync(orderId);

//     if (order == null)
//         throw new Exception("Order not found");

//     if (order.Status != "Pending")
//         throw new Exception("Only Pending orders can be verified");

//     order.Status = "Verified";

//     await _context.SaveChangesAsync();
// }

// public async Task PickupOrderAsync(int orderId)
// {
//     var order = await _context.Orders.FindAsync(orderId);

//     if (order == null)
//         throw new Exception("Order not found");

//     if (order.Status != "Verified")
//         throw new Exception("Only Verified orders can be picked up");

//     order.Status = "PickedUp";

//     await _context.SaveChangesAsync();
// }