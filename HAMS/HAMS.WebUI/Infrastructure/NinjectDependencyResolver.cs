using HAMS.Repository.Abstract;
using HAMS.Repository.Concrete;
using HAMS.WebUI.Services.Abstract;
using HAMS.WebUI.Services.Concrete;
using Ninject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HAMS.WebUI.Infrastructure
{
    public class NinjectDependencyResolver : IDependencyResolver
    {
        private IKernel _kernel;

        public NinjectDependencyResolver(IKernel kernel)
        {
            _kernel = kernel;
            AddBindings();
        }

        public object GetService(Type serviceType)
        {
            return _kernel.TryGet(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _kernel.GetAll(serviceType);
        }

        public void AddBindings()
        {
            _kernel.Bind<IAccountService>().To<AccountService>();
            _kernel.Bind<IUnitOfWork>().To<UnitOfWork>();
            _kernel.Bind<ITaskService>().To<TaskService>();
        }
    }
}