using DemoOrleans.Contracts;
using Orleans;
using Orleans.Providers;
using System.Threading.Tasks;

namespace DemoOrleans.Grains
{
    [StorageProvider(ProviderName = "Products")]
    public class ProductGrain : Grain<Product>, IProduct
    {
        public Task<int> GetStock()
        {
            //Forma de chamar outro grão (como um ator chama o outro)
            //var otherProduct = GrainFactory.GetGrain<IProduct>(2);
            return Task.FromResult(State.QuantityInStock);
        }

        public async Task SetStock(int quantity)
        {
            State.QuantityInStock = quantity;
            await WriteStateAsync();
        }
    }
}
