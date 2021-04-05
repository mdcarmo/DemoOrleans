using Orleans;
using System.Threading.Tasks;

namespace DemoOrleans.Contracts
{
    /// <summary>
    /// IGrainWithIntegerKey é o tipo de identificador do grão
    /// possui algumas outras possibilidades
    /// </summary>
    public interface IProduct : IGrainWithIntegerKey
    {
        //Metodos que deverão ser implementados e que deverão alterar o estado interno do grão
        Task SetStock(int quantity);
        Task<int> GetStock();
    }
}
