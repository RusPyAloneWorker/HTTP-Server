using System;
using System.Collections.Generic;
using System.Text;

namespace HTTPResponse.DAO
{
    interface DAO <T, TKey>
    {
        public List<T> GetAll();
        public T Update(T entity);
        public T GetEntityById(TKey id);
        public bool Delete(TKey id);
        public bool Create(T entity);
    }
}
