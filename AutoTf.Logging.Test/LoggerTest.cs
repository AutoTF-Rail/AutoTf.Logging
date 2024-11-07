namespace AutoTf.Logging.Test;

public class Tests
{
    private Logger _logger;
    
    [SetUp]
    public void Setup()
    {
        _logger = new Logger();
        _logger.NewLog += NewLog;
    }

    private void NewLog(string obj)
    {
        Assert.Pass();
    }

    [Test]
    public void Test1()
    {
        _logger.Log("Bluba");
        Thread.Sleep(2500);
    }

    [TearDown]
    public void TearDown()
    {
        _logger.Dispose();
    }
}