namespace net.shonx.stocks;

public class StockException : Exception
{
    public StockException(string message) : base(message) { }
    public StockException(Exception exception) : base(null, exception) { }

    public StockException(string message, Exception exception) : base(message, exception) { }
}