using System;
using System.Collections.Generic;
using System.Text;

namespace ACMESharp.Enrollment
{
    public interface IStorage
    {
        void Save<T>(string key, T value) where T: class;
        T Load<T>(string key) where T : class;
    }
}
