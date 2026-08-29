namespace ReallyNotInstantService.Data {
    public interface IPersistence<T> {
        public T Read(Guid id);
        public void Save(T entity);
    }
}