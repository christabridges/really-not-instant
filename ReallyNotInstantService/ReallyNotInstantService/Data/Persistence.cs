using NHibernate;
using ReallyNotInstantService.Models;

namespace ReallyNotInstantService.Data {
    public class Persistence<T> : IPersistence<T> {

        private readonly NHibernate.ISession session;

        public Persistence(NHibernate.ISession session) {
            this.session = session;
        }

        public void Save(T entity) {
            this.session.Save(entity);
            this.session.Flush();
        }
    }
}
