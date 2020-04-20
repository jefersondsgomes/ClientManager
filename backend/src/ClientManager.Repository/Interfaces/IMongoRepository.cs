using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace ClientManager.Repository.Interfaces
{
    public interface IMongoRepository<TDocument> where TDocument : IDocument
    {
        Task<IList<TDocument>> FindAsync();
        Task<TDocument> FindAsync(string id);
        Task<TDocument> FindAsync(FilterDefinition<TDocument> filter);
        Task<TDocument> ReplaceAsync(string id, TDocument t);
        Task<TDocument> CreateAsync(TDocument t);
        Task RemoveAsync(string id);
    }
}