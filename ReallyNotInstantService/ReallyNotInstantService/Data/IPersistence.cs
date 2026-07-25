namespace ReallyNotInstantService.Data {
    public interface IPersistence<T> {
        public void Save(T entity);
    }
}