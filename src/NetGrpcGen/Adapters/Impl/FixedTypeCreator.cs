namespace NetGrpcGen.Adapters.Impl
{
    public class FixedTypeCreator<TObject> : ITypeCreator<TObject>
    {
        private readonly TObject _instance;

        public FixedTypeCreator(TObject instance)
        {
            _instance = instance;
        }
        
        public TObject Create()
        {
            return _instance;
        }
    }
}