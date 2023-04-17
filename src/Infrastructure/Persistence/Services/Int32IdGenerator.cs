using MongoDB.Bson.Serialization;

namespace HotelServices.Infrastructure.Persistence.Services;

public class Int32IdGenerator : IIdGenerator
{
    private int _counter = 1;

    public object GenerateId(object container, object document)
    {
        return _counter++;
    }

    public bool IsEmpty(object id)
    {
        return (int)id == 0;
    }
}