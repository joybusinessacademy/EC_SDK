public abstract class AbstractAPI<DATA, RESPONSE> : AbstractAPI
{
    public DATA data;
    public RESPONSE response;

    public AbstractAPI(string url) : base(url){ }
}

public abstract class AbstractAPI{
    public bool authenticated = false;
    public HttpRequestType requestType = HttpRequestType.GET;

    public string URL => this.url;
    protected readonly string url;

    public AbstractAPI(string @url)
    {
        this.url = @url;
    }

    public enum HttpRequestType
    {
        POST,
        GET
    }

    [System.Serializable]
    public class EmptyData { }

    [System.Serializable]
    public class EmptyResponse : BaseResponse
    { }
}

public class BaseResponse : AbstractResponse
{
    public override void Read(dynamic @rawData)
    {
        this.rawData = @rawData;
    }
}

public abstract class AbstractResponse
{
    public dynamic rawData;

    public abstract void Read(dynamic objs);
}

