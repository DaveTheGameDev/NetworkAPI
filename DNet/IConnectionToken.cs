using DNet.NetStack;

namespace DNet
{
    public interface IConnectionToken
    {
        void Write(BitBuffer buffer);
        bool Validate(BitBuffer buffer);
    }
}