using Microsoft.VisualStudio.TestTools.UnitTesting;
using Narumikazuchi;
using Narumikazuchi.Extensibility;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TestAddInInterface;

// TODO: Test every function of the provided classes (IAddInStore, Discoverer, CreateStore)

namespace UnitTest
{
    [TestClass]
    public class AddInStoreTests
    {
        [TestMethod]
        public void DiscoverAddInsTest()
        {
            IEnumerable<AddInDefinition> addIns = AddInDiscoverer.DiscoverAddInsContainedInAssembly(_addinLibrary);

            foreach (AddInDefinition addIn in addIns)
            {
                _context.WriteLine($"[{addIn.UniqueIdentifier}] {addIn.Name} | Version: {addIn.Version}");
            }
        }

        [TestMethod]
        public void InstantiateStoreTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            Assert.IsNotNull(store);
        }

        [TestMethod]
        public void RegisterAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();

            Boolean registered;
            registered = store.TryRegisterAddIn(typeof(InternalDirectAddInTest));
            Assert.IsTrue(registered);
            registered = store.TryRegisterAddIn(typeof(InternalDirectConfigurableAddInTest));
            Assert.IsTrue(registered);
            registered = store.TryRegisterAddIn(typeof(InternalIndirectAddInTest));
            Assert.IsTrue(registered);
        }

        [TestMethod]
        public void ActivateInternalDirectAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();

            store.TryRegisterAddIn(typeof(InternalDirectAddInTest));

            if (store.IsAddInRegistered(guid: InternalDirectAddInTest.GUID,
                                        version: InternalDirectAddInTest.MyVersion))
            {
                _context.WriteLine("AddIn is registered.");
                var activated = store.TryActivate(guid: InternalDirectAddInTest.GUID,
                                                  version: InternalDirectAddInTest.MyVersion,
                                                  addIn: out InternalDirectAddInTest result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void ActivateInternalDirectConfigurableAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();

            store.TryRegisterAddIn(typeof(InternalDirectConfigurableAddInTest));

            if (store.IsAddInRegistered(guid: InternalDirectConfigurableAddInTest.GUID,
                                        version: InternalDirectConfigurableAddInTest.MyVersion))
            {
                _context.WriteLine("AddIn is registered.");
                Configuration config = new()
                {
                    Name = "Cavvalierie",
                    Description = "Angelo",
                    Count = 384,
                    Rate = 0.3333d
                };
                var activated = store.TryActivate(guid: InternalDirectConfigurableAddInTest.GUID,
                                                  version: InternalDirectConfigurableAddInTest.MyVersion,
                                                  options: config,
                                                  addIn: out InternalDirectConfigurableAddInTest result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void ActivateInternalIndirectAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();

            store.TryRegisterAddIn(typeof(InternalIndirectAddInTest));

            if (store.IsAddInRegistered(guid: InternalIndirectAddInTest.GUID,
                                        version: InternalIndirectAddInTest.MyVersion))
            {
                _context.WriteLine("AddIn is registered.");
                var activated = store.TryActivate(guid: InternalIndirectAddInTest.GUID,
                                                  version: InternalIndirectAddInTest.MyVersion,
                                                  addIn: out InternalIndirectAddInTest result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                if (result is not IMyAddIn myAddIn)
                {
                    _context.WriteLine("AddIn has wrong type.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                myAddIn.MyFunctionality(_context);
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void ActivateExternalDirectAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();
            store.TryRegisterAddIns(_addinLibrary);

            Guid guid = Guid.Parse("8849A7A0-2F62-41BB-B687-F4BAF0340E9B");
            Version version = new(1, 0);
            if (store.IsAddInRegistered(guid: guid,
                                        version: version))
            {
                _context.WriteLine("AddIn is registered.");
                var activated = store.TryActivate(guid: guid,
                                                  version: version,
                                                  addIn: out IAddIn result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void ActivateExternalDirectConfigurableAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();
            store.TryRegisterAddIns(_addinLibrary);

            Guid guid = Guid.Parse("15D432FD-C336-49BD-BC50-DE80BE57CE51");
            Version version = new(1, 2);
            if (store.IsAddInRegistered(guid: guid,
                                        version: version))
            {
                _context.WriteLine("AddIn is registered.");
                Configuration config = new()
                {
                    Name = "Cavvalierie",
                    Description = "Angelo",
                    Count = 384,
                    Rate = 0.3333d
                };
                var activated = store.TryActivate(guid: guid,
                                                  version: version,
                                                  options: config,
                                                  addIn: out IAddIn result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void ActivateExternalIndirectAddInTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePathFromStandardImplementation()
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();
            store.TryRegisterAddIns(_addinLibrary);

            Guid guid = Guid.Parse("289E2974-AADF-4880-AA3B-452EBE953F67");
            Version version = new(5, 4, 3, 2);
            if (store.IsAddInRegistered(guid: guid,
                                        version: version))
            {
                _context.WriteLine("AddIn is registered.");
                var activated = store.TryActivate(guid: guid,
                                                  version: version,
                                                  addIn: out IAddIn result);
                if (!activated)
                {
                    _context.WriteLine("AddIn wasn't activated.");
                    return;
                }

                if (result is not IMyAddIn myAddIn)
                {
                    _context.WriteLine("AddIn has wrong type.");
                    return;
                }

                _context.WriteLine($"AddIn is now running and accessible.");
                myAddIn.MyFunctionality(_context);
                Debugger.Break();
            }
            else
            {
                _context.WriteLine("AddIn wasn't registered.");
            }
        }

        [TestMethod]
        public void CacheReadWriteTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePath(_cache)
                               .TrustAllAddIns()
                               .Construct();

            store.LoadCacheFromDisk();

            store.TryRegisterAddIn(typeof(InternalDirectAddInTest));
            store.TryRegisterAddIn(typeof(InternalDirectConfigurableAddInTest));
            store.TryRegisterAddIn(typeof(InternalIndirectAddInTest));

            store.WriteCacheToDisk();

            store.UnloadCacheFromMemory();

            Assert.IsFalse(store.IsAddInRegistered(guid: InternalDirectAddInTest.GUID,
                                                   version: InternalDirectAddInTest.MyVersion));
            Assert.IsFalse(store.IsAddInRegistered(guid: InternalDirectConfigurableAddInTest.GUID,
                                                   version: InternalDirectConfigurableAddInTest.MyVersion));
            Assert.IsFalse(store.IsAddInRegistered(guid: InternalIndirectAddInTest.GUID,
                                                   version: InternalIndirectAddInTest.MyVersion));

            store.LoadCacheFromDisk();

            Assert.IsTrue(store.IsAddInRegistered(guid: InternalDirectAddInTest.GUID,
                                                  version: InternalDirectAddInTest.MyVersion));
            Assert.IsTrue(store.IsAddInRegistered(guid: InternalDirectConfigurableAddInTest.GUID,
                                                  version: InternalDirectConfigurableAddInTest.MyVersion));
            Assert.IsTrue(store.IsAddInRegistered(guid: InternalIndirectAddInTest.GUID,
                                                  version: InternalIndirectAddInTest.MyVersion));
        }

        [TestMethod]
        public void TrustedCacheReadWriteTest()
        {
            IAddInStore store = CreateAddInStore
                               .WithDefaultCachePath(_cache)
                               .TrustBothProvidedAndUserApprovedAddIns()
                               .IgnoreWhenNotSystemTrusted()
                               .PromptUserWhenNotUserTrusted(a => true)
                               .ProvidingSystemTrustedAddIns(_trustList)
                               .Construct();

            store.LoadCacheFromDisk();

            store.TryRegisterAddIn(typeof(InternalDirectAddInTest));
            store.TryRegisterAddIn(typeof(InternalDirectConfigurableAddInTest));
            store.TryRegisterAddIn(typeof(InternalIndirectAddInTest));

            store.WriteCacheToDisk();

            store.UnloadCacheFromMemory();

            Assert.IsFalse(store.IsAddInRegistered(guid: InternalDirectAddInTest.GUID,
                                                   version: InternalDirectAddInTest.MyVersion));
            Assert.IsFalse(store.IsAddInRegistered(guid: InternalDirectConfigurableAddInTest.GUID,
                                                   version: InternalDirectConfigurableAddInTest.MyVersion));
            Assert.IsFalse(store.IsAddInRegistered(guid: InternalIndirectAddInTest.GUID,
                                                   version: InternalIndirectAddInTest.MyVersion));

            store.LoadCacheFromDisk();

            Assert.IsTrue(store.IsAddInRegistered(guid: InternalDirectAddInTest.GUID,
                                                  version: InternalDirectAddInTest.MyVersion));
            Assert.IsTrue(store.IsAddInRegistered(guid: InternalDirectConfigurableAddInTest.GUID,
                                                  version: InternalDirectConfigurableAddInTest.MyVersion));
            Assert.IsTrue(store.IsAddInRegistered(guid: InternalIndirectAddInTest.GUID,
                                                  version: InternalIndirectAddInTest.MyVersion));
        }

        public TestContext TestContext
        {
            get => _context;
            set => _context = value;
        }

        private static TestContext _context;
        private static readonly String _cache = Environment.CurrentDirectory + "\\mycache.addin.cache";
        private static readonly String _addinLibrary = Environment.CurrentDirectory + "\\TestAddInLib.dll";
        private static readonly Guid[] _trustList = new Guid[]
        {
            //Guid.Parse("8849A7A0-2F62-41BB-B687-F4BAF0340E9B"),
            //Guid.Parse("289E2974-AADF-4880-AA3B-452EBE953F67"),
            //Guid.Parse("DB942E7E-CDF3-441B-B8A9-16D7A0B07A37")
        };
    }
}
