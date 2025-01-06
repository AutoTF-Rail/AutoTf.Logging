namespace AutoTf.Logging.Test;

public class Tests
{
    private Logger _logger;
    
    [SetUp]
    public void Setup()
    {
        _logger = new Logger(true);
        _logger.NewLog += NewLog;
    }

    private void NewLog(string obj)
    {
        Thread.Sleep(2500);
        Assert.Pass();
    }

    [Test]
    public void Test1()
    {
        _logger.Log("Bluba");
    }

    [TearDown]
    public void TearDown()
    {
        _logger.Dispose();
    }
}