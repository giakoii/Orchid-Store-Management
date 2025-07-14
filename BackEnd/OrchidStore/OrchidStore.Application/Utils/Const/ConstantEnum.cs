namespace BackEnd.Utils.Const;

public static class ConstantEnum
{
    public enum UserRole
    {
        Customer = 1,
        Admin = 2,	
    }
    
    public enum OrderStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Cancelled = 4,
    }
    
    public enum PaymentStatus
    {
        Success = 0,
    }

    public enum PaymentMethod
    {
        PayOs = 1,
    }
    
}