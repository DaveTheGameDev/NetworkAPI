using DNet.NetStack;

namespace DNet.Unity
{
    public class ConnectionToken : IConnectionToken
    {
        private readonly string _password;

        public ConnectionToken(string password)
        {
            _password = password;
        }

        public void Write(BitBuffer buffer)
        {
            buffer.AddString(_password);
        }

        public bool Validate(BitBuffer buffer)
        {
            var pass = buffer.ReadString();
            return pass == _password;
        }
    }
}